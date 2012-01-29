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

            var field = GenerateField(8, 8);
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
            var wordsInField = BoggleUtilities.FindWords(field, dictionary).ToArray();
            Console.WriteLine("Word finding took {0} ms", time.ElapsedMilliseconds);
            Console.WriteLine("Found {0} words", wordsInField.Length);

            foreach (var foundword in wordsInField.Select(w => w.Word).OrderBy(w=>w))
                Console.WriteLine(foundword);

            string word;
            do
            {
                word = Console.ReadLine();
                var wordToDisplay = wordsInField.FirstOrDefault(w => w.Word == word);
                if (wordToDisplay == null) // word is not in field
                {
                    // either its not a word or its not in field.
                    Console.WriteLine("Sorry, '{0}' is not a word in the field.", word);
                }
                else
                {
                    DisplayWord(wordToDisplay, field);
                }
            } while (!String.IsNullOrEmpty(word));
        }

        private static void DisplayWord(BoggleSolution word, char[,] field)
        {
            for (int y = 0; y < field.GetLength(1); y++)
            {
                for (int x = 0; x < field.GetLength(0); x++)
                {
                    var thispoint = new Point { X = x, Y = y };
                    // Mark the letter in the word
                    if (word.Path.Contains(thispoint))
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write(field[x, y].ToString().ToUpper());
                    Console.BackgroundColor = ConsoleColor.Black;
                    // mark the whitespace between letters
                    if (word.PathContains(thispoint, new Point { X = x + 1, Y = y }))
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write("  ");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine();
                for (int x = 0; x < field.GetLength(0); x++)
                {
                    var thispoint = new Point { X = x, Y = y };
                    // mark the whitespace between lines
                    if (word.PathContains(thispoint, new Point { X = x, Y = y + 1 }))
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" ");
                    Console.BackgroundColor = ConsoleColor.Black;
                    // whitespace for pretty-printing
                    Console.Write("  ");
                }
                Console.WriteLine();
            }
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
            for (int y = 0; y < chars.GetLength(1); y++)
            {
                for (int x = 0; x < chars.GetLength(0); x++)
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
