using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoggleSolverConsole
{
    class BoggleUtilities
    {
        /// <summary>
        /// takes about 18 seconds on this puny laptop for 500.000 words
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static CharDictionaryEntry LoadWords(IEnumerable<string> words)
        {
            var root = new CharDictionaryEntry("", false);
            foreach (var word in words)
            {
                if (word.Length > 3)
                    root.AddWordTail(word.ToLower());
            }
            return root;
        }

        /// <summary>
        /// takes ~1 second, depending on chars
        /// </summary>
        /// <param name="words">All the words</param>
        /// <param name="chars">The distinct chars in the boggle field</param>
        /// <returns>a dictionary with words which consist of chars</returns>
        public static CharDictionaryEntry LoadWords(IEnumerable<string> words, HashSet<char> chars)
        {
            var root = new CharDictionaryEntry("", false);
            foreach (var word in words)
            {
                var theword = word.ToLower();
                if (word.Length > 3 && theword.All(chars.Contains))
                    root.AddWordTail(theword);
            }
            return root;
        }

        /// <summary>
        /// Finds all words in dictionary that are in the char field
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEnumerable<BoggleSolution> FindWords(char[,] chars, CharDictionaryEntry dictionary)
        {
            for (int x = 0; x < chars.GetLength(0); x++)
                for (int y = 0; y < chars.GetLength(1); y++)
                {
                    foreach (var word in FindWords(
                            chars,
                            new bool[chars.GetLength(0), chars.GetLength(1)],
                            dictionary,
                            new Stack<Point>(),
                            x,
                            y
                            ))
                        yield return word;
                }
        }

        private static IEnumerable<BoggleSolution> FindWords(char[,] chars, bool[,] visited, CharDictionaryEntry lastStep, Stack<Point> path, int x, int y)
        {
            if (x < 0 || y < 0 || x >= chars.GetLength(0) || y >= chars.GetLength(0) || visited[x, y])
                yield break;
            var nextstep = lastStep[chars[x, y]];
            if (nextstep == null) // no word in this direction
                yield break;
            path.Push(new Point { X = x, Y = y });
            if (nextstep.IsWord)  // Victory! Found a word, return it... 
                yield return new BoggleSolution(nextstep.Word, path);
            // Return possible longer words.
            var newVisited = new bool[chars.GetLength(0), chars.GetLength(1)];
            Array.Copy(visited, newVisited, visited.Length);
            newVisited[x, y] = true;
            foreach (var word in
                FindWords(chars, newVisited, nextstep, path, x + 1, y).Concat(
                FindWords(chars, newVisited, nextstep, path, x, y + 1)).Concat(
                FindWords(chars, newVisited, nextstep, path, x - 1, y)).Concat(
                FindWords(chars, newVisited, nextstep, path, x, y - 1)))
                yield return word;
            path.Pop();
        }
    }
}
