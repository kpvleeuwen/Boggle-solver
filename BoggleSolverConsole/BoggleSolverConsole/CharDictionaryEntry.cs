using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BoggleSolverConsole
{
    /// <summary>
    /// Tree dictionary lookup entry for a single char in a string
    /// </summary>
    public class CharDictionaryEntry
    {
        private Dictionary<char, CharDictionaryEntry> _next;
        public bool IsWord { get; private set; }
        public string Word { get; private set; }

        public CharDictionaryEntry(string sofar, bool word)
        {
            Word = sofar;
            IsWord = word;
        }

        /// <summary>
        /// lazy-loaded wrapper.
        /// </summary>
        private Dictionary<char, CharDictionaryEntry> Next
        {
            get { return _next ?? (_next = new Dictionary<char, CharDictionaryEntry>(1)); }
        }

        public CharDictionaryEntry this[char next]
        {
            get
            {
                CharDictionaryEntry nextChar;
                if (!Next.TryGetValue(next, out nextChar))
                    return null;
                return nextChar;
            }
            set
            {
                Debug.Assert(this[next] == null);
                Next[next] = value;
            }
        }

        /// <summary>
        /// Creates entries for all characters in tail and adds them to this entry.
        /// </summary>
        /// <param name="tail"></param>
        public void AddWordTail(string tail)
        {
            CharDictionaryEntry nextChar = this[tail[0]];
            var nextIsWord = tail.Length == 1;
            if (nextChar == null)
            {
                nextChar = new CharDictionaryEntry(Word + tail[0], nextIsWord);
                this[tail[0]] = nextChar;
            }
            if (!nextIsWord) //more chars left
            {
                nextChar.AddWordTail(tail.Substring(1)); // consume 1 char and recurse
            }
        }
    }
}
