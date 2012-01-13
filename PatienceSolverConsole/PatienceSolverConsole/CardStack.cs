using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace PatienceSolverConsole
{

    [Serializable]
    public abstract class CardStack : IEnumerable<Card>
    {
        public CardStack(IEnumerable<Card> cards)
        {
            Cards = new List<Card>(cards);
            foreach (var card in Cards)
                card.Stack = this;
            if (Top != null)
                Top.Visible = true;
            _hashDirty = true;
        }

        protected IList<Card> Cards { get; private set; }
        public Card Top { get { return Cards.LastOrDefault(); } }
        public int Count { get { return Cards.Count; } }

        private int _hash;
        protected bool _hashDirty;

        public IEnumerator<Card> GetEnumerator()
        {
            return Cards.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerable<Card> GetMovableCards()
        {
            return Cards.Reverse().TakeWhile(c => c.Visible);
        }

        public abstract bool CanAccept(Card c);

        protected abstract void DoAccept(Card c);

        internal void Accept(Card c)
        {
            if (!CanAccept(c))
                throw new InvalidOperationException();
            DoAccept(c);
            c.Stack = this;
            _hashDirty = true;
            Debug.Assert(Top.Visible, "invisible top");
        }

        public abstract void Move(Card c, CardStack s);

        /// <summary>
        /// Writes a ascii art line of cards to s, returns true if more lines are to be written.
        /// Does not append a newline!
        /// </summary>
        /// <param name="s"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public abstract bool WriteLine(int line);

        protected bool WriteEmpty(int line)
        {
            if (line < Card.Height)
            {
                Console.Write(". . . ");
                return true;
            }
            Console.Write("      ");
            return false;
        }


        public override int GetHashCode()
        {
            if (_hashDirty)
            {
                _hash = DoGetHashCode();
                _hashDirty = false;
            }
            return _hash;
        }

        protected virtual int DoGetHashCode()
        {
            if (Cards.Count == 0) return 0;
            var hashcode = 0;
            foreach (var card in Cards)
                hashcode = hashcode * 81 + card.GetHashCode();
            return hashcode;
        }

        public override bool Equals(object obj)
        {
            var stack = obj as CardStack;
            if (stack == null)
                return false;
            if (stack.GetType() != GetType())
                return false;
            if (GetHashCode() != stack.GetHashCode())
                return false;
            if (Cards.Count == 0)
                return true;
            return Cards.SequenceEqual(stack.Cards);
        }
    }

    [Serializable]
    public class Stock : CardStack
    {
        public Stock(IEnumerable<Card> cards)
            : base(cards)
        {
            foreach (var card in Cards)
                card.Visible = true; // only valid on 'infite one card draw' games
        }

        protected override void DoAccept(Card c)
        {
            throw new InvalidOperationException();
        }

        public override bool CanAccept(Card c)
        {
            return false;
        }

        public override void Move(Card c, CardStack s)
        {
            if (!c.Visible)
                throw new InvalidOperationException("card is invisible");
            if (c.Stack != this)
                throw new InvalidOperationException("not owned by me");
            //if (Top != c)
            //    throw new InvalidOperationException("not top");
            s.Accept(c);
            Cards.Remove(c);
            _hashDirty = true;
            // New top is visible
            // if (Top != null)
            //     Top.Visible = true;
        }

        public void NextCard()
        {
            if (Cards.Count < 2)
                return;
            var oldTop = Top;
            oldTop.Visible = false;
            Cards.Add(Cards[0]);
            Cards.RemoveAt(0);
            _hashDirty = true;
            Top.Visible = true;
        }

        /// <summary>
        /// Writes a ascii art line of cards to s, returns true if more lines are to be written.
        /// Does not append a newline!
        /// </summary>
        /// <param name="s"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public override bool WriteLine(int line)
        {
            using (new BlockConsoleColor(ConsoleColor.Gray, ConsoleColor.DarkGreen))
            {
                bool morelines = false;
                Console.Write("    ");
                if (Top != null)
                    morelines = Top.WriteLine(line);
                else
                    morelines = WriteEmpty(line);
                Console.Write(" ");
                if (Cards.Count > 1)
                {
                    Cards[Cards.Count - 2].WriteLine(line);
                }
                else
                {
                    WriteEmpty(line);
                }
                return morelines;
            }
        }
    }

    [Serializable]
    public class PlayStack : CardStack
    {
        public PlayStack(IEnumerable<Card> cards) : base(cards) { }

        protected override void DoAccept(Card c)
        {
            Cards.Add(c);
        }

        public override bool CanAccept(Card c)
        {
            if (!c.Visible)
                return false;
            if (Top == null)
                return c.Value == Value.King && c.Stack.First() != c; //  no sense in moving kings around
            if (c.Value == Value.Ace)
                return false;
            return (int)c.Value == (int)Top.Value - 1 && c.Color != Top.Color;
        }

        public override void Move(Card c, CardStack s)
        {
            if (!c.Visible)
                throw new InvalidOperationException("card is invisible");
            if (c.Stack != this)
                throw new InvalidOperationException("not owned by me");
            var tomove = Cards.Skip(Cards.IndexOf(c)).ToArray();
            foreach (var card in tomove)
            {
                s.Accept(card);
                Cards.Remove(card);
                _hashDirty = true;
            }
            // New top is visible
            if (Top != null)
                Top.Visible = true;
        }

        /// <summary>
        /// Writes a ascii art line of cards to s, returns true if more lines are to be written.
        /// Does not append a newline!
        /// </summary>
        /// <param name="s"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public override bool WriteLine(int line)
        {
            using (new BlockConsoleColor(ConsoleColor.Gray, ConsoleColor.DarkGreen))
            {
                bool morelines;
                if (Cards.Any())
                {
                    if (line < 2 * (Cards.Count - 1))
                        morelines = Cards[line / 2].WriteLine(line % 2);
                    else
                        morelines = Top.WriteLine(line - (2 * (Cards.Count - 1)));
                }
                else
                    morelines = WriteEmpty(line);
                Console.Write(' ');
                return morelines;
            }
        }
    }


    [Serializable]
    public class FinishStack : CardStack
    {
        public FinishStack(IEnumerable<Card> cards) : base(cards) { }
        public FinishStack() : this(new Card[] { }) { }

        protected override void DoAccept(Card c)
        {
            Debug.Assert(c.Visible);
            if (Top != null)
                Top.Visible = false; // the old top is not visible any more
            Cards.Add(c); // this one's visible
        }

        public override bool CanAccept(Card c)
        {
            if (c != c.Stack.Top && c.Stack is PlayStack)
                return false; // a finishstack can only accept top cards from other stacks
            if (c.Stack is FinishStack)
                return false; // no sense in moving aces around
            if (!c.Visible)
                return false;
            if (Top == null)
                return c.Value == Value.Ace;
            return (int)c.Value == (int)Top.Value + 1 && c.Suit == Top.Suit;

        }

        public override void Move(Card c, CardStack s)
        {
            if (s == this)
                throw new InvalidOperationException("same stack");
            if (!c.Visible)
                throw new InvalidOperationException("card is invisible");
            if (c.Stack != this)
                throw new InvalidOperationException("not owned by me");
            if (Top != c)
                throw new InvalidOperationException("not top");
            s.Accept(c);
            Cards.Remove(c);
            _hashDirty = true;
            // New top is visible
            if (Top != null)
                Top.Visible = true;
        }

        /// <summary>
        /// Writes a ascii art line of cards to s, returns true if more lines are to be written.
        /// Does not append a newline!
        /// </summary>
        /// <param name="s"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public override bool WriteLine(int line)
        {

            using (new BlockConsoleColor(ConsoleColor.Gray, ConsoleColor.DarkGreen))
            {
                bool morelines = false;
                if (Top != null)
                    morelines = Top.WriteLine(line);
                else
                    morelines = WriteEmpty(line);
                Console.Write("  ");
                return morelines;
            }
        }
    }
}
