using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ScrabbleHelper
{
    static class Program
    {
        static readonly string AllChars = "abcdefghijklmnopqrstuvwxyz";
        static void Main(string[] args)
        {
            //Test(); return;
            Console.WriteLine("Jouw letters:");
            var own = Console.ReadLine();
            var blanks = own.Where(c => "? .".Contains(c)).Count();
            string constraints;
            do
            {
                Console.WriteLine("Beperkingen: ");
                constraints = Console.ReadLine();
                var neededChars = constraints.Where(AllChars.Contains);
                var chars = own.AsEnumerable().Concat(neededChars)
                    .GroupBy(c => c)
                    .ToDictionary(g => g.Key, g => g.Count());
                var allwords = File.ReadAllLines("woorden.txt").Select(w => w.ToLower());
                var foundwords = new List<string>();
                var constraintwords = ConstraintWords(allwords, constraints);

                var charfiltered = FilterWords(constraintwords, chars, blanks);
                foreach (var word in charfiltered)
                {
                    Console.WriteLine(word);
                    foundwords.Add(word);
                }
               
                Console.WriteLine("Done.");
            } while (!string.IsNullOrEmpty(constraints));
        }

        private static IEnumerable<string> FilterWords(IEnumerable<string> words, Dictionary<char, int> chars, int blanks)
        {
            return from w in words
                   let shortage = GetShort(w, chars)
                   where shortage <= blanks
                   select w;
        }

        private static int GetShort(string word, Dictionary<char, int> chars)
        {
            var groups = from c in word
                         group c by c into g
                         select g;
            var result = (from g in groups
                          select Math.Max(0, g.Count() - GetCount(chars, g.Key))
                   )
                   .Sum();
            return result;
        }

        private static int GetCount(Dictionary<char, int> chars, char c)
        {
            int result;
            chars.TryGetValue(c, out result);
            return result;
        }



        private static void Test()
        {
            test1();
            test2();
        }

        private static void test2()
        {
            var result2 = ConstraintWords(new[] { "opa", "pakje", "hansop", "opaatje", "kapo", "aap", "rijpaard" }
                , "^.*a..e.*$").ToArray();
            Debug.Assert(result2.SequenceEqual(new[] { "pakje", "opaatje" }));
        }

        private static void test1()
        {

            var result1 = ConstraintWords(new[] { "opa", "pakje", "hansop", "opaatje", "kapo", "aap", "rijpaard" }
                , "^.?.?p.?.?$").ToArray();
            Debug.Assert(result1.SequenceEqual(new[] { "opa", "kapo", "aap" }));
        }

        private static IEnumerable<string> ConstraintWords(IEnumerable<string> words, string constraint)
        {
            Regex regex = new Regex("^"+constraint+"$");

            return words.Where(w => regex.IsMatch(w));
        }

        public static IEnumerable<int> EnumTo(this int start, int max)
        {
            for (int i = start; i < max; i++)
                yield return i;
        }
    }
}
