using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace BoggleSolverConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var field = GenerateField(5, 5);
            DumpField(field);

            var filename = "woorden.txt";
            Console.WriteLine("Reading {0}", filename);
            var words = File.ReadAllLines(filename);
            Console.WriteLine("Loading dictionary {0}", filename);
            var time = Stopwatch.StartNew();
            var dictionary = BoggleUtilities.LoadWords(words, UniqueCharsIn(field));
            Console.WriteLine("Loaded {0} words in {1} ms", words.Length, time.ElapsedMilliseconds);

            Console.WriteLine("Press enter to show words");
            Console.ReadLine();
            time = Stopwatch.StartNew();
            foreach (var word in
                    BoggleUtilities.FindWords(field, dictionary))
            {
                Console.WriteLine(word);
            }

            Console.WriteLine("Word finding took {0} ms", time.ElapsedMilliseconds);
            Console.ReadLine();
        }

        private static HashSet<char> UniqueCharsIn(char[,] chars)
        {
            return new HashSet<char>(chars.Cast<char>().Distinct());
        }

        private static char[,] GenerateField(int xmax, int ymax)
        {
            var chars = //Dutch scrabble distribution
                new string('a', 6) +
                new string('b', 2) +
                new string('c', 2) +
                new string('d', 5) +
                new string('e', 18) +
                new string('f', 2) +
                new string('g', 3) +
                new string('h', 2) +
                new string('i', 4) +
                new string('j', 2) +
                new string('k', 3) +
                new string('l', 3) +
                new string('m', 3) +
                new string('n', 10) +
                new string('o', 6) +
                new string('p', 2) +
                new string('q', 1) +
                new string('r', 5) +
                new string('s', 5) +
                new string('t', 5) +
                new string('u', 3) +
                new string('v', 2) +
                new string('w', 2) +
                new string('x', 1) +
                new string('y', 1) +
                new string('z', 2);
            var field = new char[xmax, ymax];
            var random = new Random();
            for (int x = 0; x < xmax; x++)
                for (int y = 0; y < ymax; y++)
                {
                    field[x, y] = chars[random.Next(chars.Length)];
                }
            return field;
        }

        static void DumpField(char[,] chars)
        {
            for (int x = 0; x < chars.GetLength(0); x++)
            {
                for (int y = 0; y < chars.GetLength(1); y++)
                {
                    Console.Write(chars[x, y]);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }



        void Test()
        {
            var dictionary = BoggleUtilities.LoadWords(new[] {
                "aap",   // 2
                "aapje", // 0
                "art",   // 1 
                "gaap",  // 2
                "raapt", // 1
                "rat",   // 1
            });
            /* 
                The dictionary is now:
             * a
             *  a
             *   p*
             *    j
             *     e*
             *  r
             *   t*
             * g
             *  a
             *   a
             *    p*
             * r
             *  a
             *   a
             *    p
             *     t*
             *   t*
             */
            foreach (var word in
            BoggleUtilities.FindWords(
                new[,]
                {
                    {'a','a','p'},
                    {'g','a','t'},
                    {'g','r','t'},
                }
                , dictionary))
            {
                Console.WriteLine(word);
            }

            Console.ReadLine();
        }
    }
}
