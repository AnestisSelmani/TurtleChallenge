using System;

namespace PowerRanger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(PowerRanger(2, 49, 65));     // Output: 2
            Console.WriteLine(PowerRanger(3, 1, 27));      // Output: 3
            Console.WriteLine(PowerRanger(10, 1, 5));      // Output: 1
            Console.WriteLine(PowerRanger(5, 31, 33));     // Output: 1
            Console.WriteLine(PowerRanger(4, 250, 1300));  // Output: 3
        }


        static int PowerRanger(int n, int a, int b)
        {
            int count = 0;
            int start = (int)Math.Ceiling(Math.Pow(a, 1.0 / n));
            int end = (int)Math.Floor(Math.Pow(b, 1.0 / n));

            for (int i = start; i <= end; i++)
            {
                int value = (int)Math.Pow(i, n);
                if (value >= a && value <= b)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
