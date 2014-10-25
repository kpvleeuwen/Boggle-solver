using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace PatienceSolverConsole
{
    /// <summary>
    /// Immutable stack of cards
    /// </summary>
    public abstract class CardStack : IEnumerable<Card>
    {
        public CardStack(IEnumerable<Card> cards)
        {
            Cards = new List<Card>(cards);
            if (Top != null)
                Cards[Count - 1] = Top.AsVisible();
            _hash = DoGetHashCode();
        }

        protected virtual IList<Card> Cards { get; private set; }
        public Card Top { get { return Cards.LastOrDefault(); } }
        public int Count { get { return Cards.Count; } }

        private int _hash;

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

        public abstract bool CanAccept(Card c, CardStack from);

        protected abstract CardStack DoAccept(Card c, CardStack from);

        internal CardStack Accept(Card c, CardStack from)
        {
            if (!CanAccept(c, from))
                throw new InvalidOperationException();
            return DoAccept(c, from);
        }

        internal abstract CardStack Remove(Card c);

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
        public Stock(IEnumerable<Card> cards, bool justMoveTop)
            : base(cards.Select(c => c.AsVisible()))
        {
            JustMoveTop = justMoveTop;
        }

        protected override CardStack DoAccept(Card c, CardStack from)
        {
            // Cards cannot return to stock
            throw new InvalidOperationException();
        }

        public override IEnumerable<Card> GetMovableCards()
        {
            if (JustMoveTop)
                return new[] { Top };
            return base.GetMovableCards();
        }

        public override bool CanAccept(Card c, CardStack from)
        {
            return false;
        }

        internal override CardStack Remove(Card c)
        {
            return new Stock(Cards.Where(cd => cd != c), JustMoveTop);
        }

        public Stock NextCard()
        {
            if (Cards.Count < 2)
                return this;
            // Move the old top to the bottom

            return new Stock(new[] { Top }.Concat(Cards.Except(new[] { Top })), JustMoveTop);
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
                    Cards[Cards.Count - 2].AsInvisible().WriteLine(line);
                }
                else
                {
                    WriteEmpty(line);
                }
                return morelines;
            }
        }

        public bool JustMoveTop { get; set; }
    }

    public class PlayStack : CardStack
    {
        public PlayStack(IEnumerable<Card> cards) : base(cards) { }

        protected override CardStack DoAccept(Card c, CardStack from)
        {
            if (from is PlayStack)
            {
                // all cards on top of this c are moved too
                var playablecards = from.SkipWhile(mc => mc != c);
                return new PlayStack(Cards.Concat(playablecards));
            }
            return new PlayStack(Cards.Concat(new[] { c }));
        }

        public override IEnumerable<Card> GetMovableCards()
        {
            return base.GetMovableCards();
        }

        public override bool CanAccept(Card c, CardStack from)
        {
            if (Top == null)
                return c.Value == Value.King && !(from is PlayStack && c == from.First()); // it makes no sense to move kings around
            if (c.Value == Value.Ace)
                return false;
            return (int)c.Value == (int)Top.Value - 1 && c.Color != Top.Color;
        }

        internal override CardStack Remove(Card c)
        {
            // leftovers are all cards until the card to move
            return new PlayStack(Cards.TakeWhile(cs => cs != c));
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

    public class FinishStack : CardStack
    {
        public FinishStack(IEnumerable<Card> cards) : base(cards) { }
        public FinishStack() : this(new Card[] { }) { }

        protected override CardStack DoAccept(Card c, CardStack from)
        {
            return new FinishStack(Cards.Concat(new[] { c }));
        }

        public override bool CanAccept(Card c, CardStack from)
        {
            if (from is PlayStack && c != from.Top)
                return false;
            if (Top == null)
                return c.Value == Value.Ace;
            return (int)c.Value == (int)Top.Value + 1 && c.Suit == Top.Suit;
        }

        internal override CardStack Remove(Card c)
        {
            return new FinishStack(Cards.Where(cd => cd != c));
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
