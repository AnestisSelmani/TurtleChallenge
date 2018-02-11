using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Linq;

namespace TurtleChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example
            // Format used for game settings: json in Settings class format
            // Format used for moves: values of MoveEnum separated by comma
            string currentDirectory = Directory.GetCurrentDirectory();
            RunTurtleChallenge(Path.Combine(currentDirectory, "game-settings.txt"), Path.Combine(currentDirectory, "moves.txt"));

            // Uncomment, set the filepaths needed for the settings and the moves and run the turtle challenge
            //string gameSettingsFilePath = string.Empty;
            //string movesFilePath = string.Empty;
            //RunTurtleChallenge(gameSettingsFilePath, movesFilePath);    
        }

        /// <summary>
        /// Turtle Challenge
        /// 1. Read from the file game-settings the settings necessairy for the game
        /// 2. Read from the file moves the moves tha the turtle has to make
        /// 3. Write in the file moves the what happens after every move of the turtle
        /// </summary>
        /// <param name="settingsFilePath"> The file path for the game settings</param>
        /// <param name="movesFilePath"> The file path for the moves</param>
        public static void RunTurtleChallenge(string settingsFilePath, string movesFilePath)
        {
            List<MoveEnum> moves = new List<MoveEnum>();
            Settings gameSettings = new Settings();

            //Read and map settings
            using (StreamReader sr = new StreamReader(settingsFilePath))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadToEnd();
                Console.WriteLine(line);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
                MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
                gameSettings = (Settings)serializer.ReadObject(ms);
            }

            //Read and map Moves
            using (StreamReader sr = new StreamReader(movesFilePath))
            {
                // Read the stream to a string, and write the string to the console.
                String line = sr.ReadToEnd();
                string[] stringMoves = line.Split(',');


                foreach (string m in stringMoves)
                {
                    MoveEnum mov = (MoveEnum)Enum.Parse(typeof(MoveEnum), m);
                    moves.Add(mov);
                }
            }

            // Append in the file moves all the results from the turtle movement
            using (StreamWriter file = new StreamWriter(movesFilePath, append: true))
            {
                // First append a line with the Board dimensions and Mine positions for guidance
                StringBuilder set = new StringBuilder();
                set.Append("Board" + ": " + "(" + gameSettings.Board.MaxX + "," + gameSettings.Board.MaxY + ")/ ");
                foreach (var min in gameSettings.MinePositions)
                {
                    set.Append("Mine" + ": " + "(" + min.X + "," + min.Y + ") ");
                }
                file.WriteLine(Environment.NewLine);
                file.WriteLine(set.ToString(), Environment.NewLine);

                Tile currentPosition = gameSettings.StartingPosition;
                DirectionEnum direction = gameSettings.Direction;

                foreach (MoveEnum m in moves)
                {
                    Tile newPosition = new Tile();
                    StringBuilder str = new StringBuilder();
                    str.Append(m.ToString() + ": ");
                    switch (m)
                    {
                        case MoveEnum.MOVE:
                            newPosition = GetPositionFromMove(currentPosition, direction, gameSettings.Board);
                            break;
                        case MoveEnum.ROTATE:
                            direction = (int)direction != 4 ? direction + 1 : DirectionEnum.NORTH;
                            newPosition = currentPosition;
                            break;
                    }
                    // Evaluate case and get log message (among with a WIN flag)
                    EvaluationAndLog eval = GetLogMessage(gameSettings, newPosition, currentPosition, m, direction);
                    currentPosition = newPosition; // Now we can set as the current position the new position
                    // Write the line in the file
                    file.WriteLine(m.ToString() + ": " + eval.Message + " CurrentPosition: (" + currentPosition.X + "," + currentPosition.Y + ")", Environment.NewLine);
                    // If the evaluation has given the flag WIN true then the game is over.
                    if (eval.Win) break;
                }
            }
        }

        /// <summary>
        /// Get the next position of the turtle after a MOVE order
        /// If the turtle is at the end of the board and can't go outside of it then the turtle stays at the same spot.
        /// </summary>
        /// <param name="position">The current position of the turtle before the MOVE order</param>
        /// <param name="direction">The current direction of the turtle</param>
        /// <param name="board">The Board size</param>
        /// <returns></returns>
        public static Tile GetPositionFromMove(Tile position, DirectionEnum direction, Board board)
        {
            switch (direction)
            {
                case DirectionEnum.NORTH:
                    position.Y = position.Y != board.MaxY ? position.Y + 1 : position.Y;
                    break;
                case DirectionEnum.EAST:
                    position.X = position.X != 0 ? position.X - 1 : position.X;
                    break;
                case DirectionEnum.SOUTH:
                    position.Y = position.Y != 0 ? position.Y - 1 : position.Y;
                    break;
                case DirectionEnum.WEST:
                    position.X = position.X != board.MaxX ? position.X + 1 : position.X;
                    break;
            }
            return position;
        }

        /// <summary>
        /// When the user orders a ROTATE move then we evaluate the mines on his new direction and let him know
        /// that he is in danger
        /// </summary>
        /// <param name="mines">The game settings mines positions</param>
        /// <param name="direction">The new direction of the turtle</param>
        /// <param name="position">The current position of the turtle</param>
        /// <returns></returns>
        public static bool GetDangerFromRotation(List<Tile> mines, DirectionEnum direction, Tile position)
        {
            bool danger = false;
            switch (direction)
            {
                case DirectionEnum.NORTH:
                    danger = mines.Any(m => m.Y == position.Y + 1 && m.X == position.X);
                    break;
                case DirectionEnum.EAST:
                    danger = mines.Any(m => m.X == position.X - 1 && m.Y == position.Y);
                    break;
                case DirectionEnum.SOUTH:
                    danger = mines.Any(m => m.Y == position.Y - 1 && m.X == position.X);
                    break;
                case DirectionEnum.WEST:
                    danger = mines.Any(m => m.X == position.X + 1 && m.Y == position.Y);
                    break;
            }
            return danger;
        }

        /// <summary>
        /// Evaluate the move and return the appropriate message
        /// </summary>
        /// <param name="settings">The game settings</param>
        /// <param name="newPosition">The new positions after the move</param>
        /// <param name="currentPosition">The current position before the move</param>
        /// <param name="move">The type of move</param>
        /// <param name="direction">The direction of the turtle (if the move if type of ROTATE then we are talking about the new direction)</param>
        /// <returns></returns>
        public static EvaluationAndLog GetLogMessage(Settings settings, Tile newPosition, Tile currentPosition, MoveEnum move, DirectionEnum direction)
        {
            EvaluationAndLog model = new EvaluationAndLog();
            string result = string.Empty;
            if (newPosition.X == settings.ExitPosition.X && newPosition.Y == settings.ExitPosition.Y)
            {
                model.Win = true;
                model.Message = "You found the exit point! YOU WIN!";
                return model;
            }
            else if (settings.MinePositions.Any(t => t.X == newPosition.X && t.Y == newPosition.Y))
            {
                model.Message = "You have hit a mine"; // According to the example we don't finish the game when a mine is stepped.
                return model;
            }
            else if (move == MoveEnum.ROTATE)
            {
                if(GetDangerFromRotation(settings.MinePositions, direction, currentPosition))
                {
                    model.Message = "Still in danger";
                    return model;
                }
            }

            model.Message = "Success!";
            return model;
        }
    }



    [DataContract]
    public class Board
    {
        [DataMember(Name = "MaxX")]
        public int MaxX { get; set; }
        [DataMember(Name = "MaxY")]
        public int MaxY { get; set; }
    }

    [DataContract]
    public class Tile
    {
        [DataMember(Name = "X")]
        public int X { get; set; }
        [DataMember(Name = "Y")]
        public int Y { get; set; }
    }

    [DataContract]
    public class Settings
    {
        [DataMember(Name = "StartingPosition")]
        public Tile StartingPosition { get; set; }
        [DataMember(Name = "ExitPosition")]
        public Tile ExitPosition { get; set; }
        [DataMember(Name = "MinePositions")]
        public List<Tile> MinePositions { get; set; }
        [DataMember(Name = "Direction")]
        public DirectionEnum Direction { get; set; }
        [DataMember(Name = "Board")]
        public Board Board { get; set; }
    }

    public class EvaluationAndLog
    {
        public bool Win { get; set; }
        //public bool Lose { get; set; }
        public string Message { get; set; }

        public EvaluationAndLog()
        {
            Win = false;
        }
    }

    public enum DirectionEnum
    {
        NORTH = 1,
        WEST = 2,
        SOUTH = 3,
        EAST = 4
    }

    public enum MoveEnum
    {
        MOVE = 1,
        ROTATE = 2
    }
}