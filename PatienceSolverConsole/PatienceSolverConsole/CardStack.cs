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
        protected CardStack() { }

        public abstract Card Top { get; }
        public abstract int Count { get; }

        public abstract IEnumerable<Card> GetMovableCards();

        public abstract bool CanAccept(Card c, CardStack from);

        protected abstract CardStack DoAccept(Card c, CardStack from);

        internal CardStack Accept(Card c, CardStack from)
        {
            if (!CanAccept(c, from))
                throw new InvalidOperationException();
            return DoAccept(c, from);
        }

        public abstract IEnumerator<Card> GetEnumerator();

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


        public override abstract int GetHashCode();


        public override bool Equals(object obj)
        {
            var stack = obj as CardStack;
            if (stack == null)
                return false;
            if (stack.GetType() != GetType())
                return false;
            if (GetHashCode() != stack.GetHashCode())
                return false;
            if (Count == 0 && stack.Count == 0)
                return true;
            return this.SequenceEqual(stack);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)this.GetEnumerator();
        }
    }

    public abstract class ListBasedCardStack : CardStack
    {
        public ListBasedCardStack(IEnumerable<Card> cards)
        {
            Cards = new List<Card>(cards);
            if (Top != null)
                Cards[Count - 1] = Top.AsVisible();
            _hash = DoGetHashCode();
        }

        protected virtual IList<Card> Cards { get; private set; }
        public override Card Top { get { return Cards.LastOrDefault(); } }
        public override int Count { get { return Cards.Count; } }

        private int _hash;

        private int DoGetHashCode()
        {
            if (Cards.Count == 0) return 0;
            var hashcode = 0;
            foreach (var card in Cards)
                hashcode = hashcode * 81 + card.GetHashCode();
            return hashcode;
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        /// <summary>
        /// Enumerates the cards of this stack from bottom to top
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Card> GetEnumerator()
        {
            return Cards.GetEnumerator();
        }

        public override IEnumerable<Card> GetMovableCards()
        {
            return Cards.Reverse().TakeWhile(c => c.Visible);
        }
    }


    public class Stock : ListBasedCardStack
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
        private class EmptyStack : PlayStack
        {
            public override IEnumerator<Card> GetEnumerator()
            {
                return GetMovableCards().GetEnumerator();
            }

            public override IEnumerable<Card> GetMovableCards()
            {
                return Enumerable.Empty<Card>();
            }

            public override int Count { get { return 0; } }

            public override int GetHashCode()
            {
                return 381;
            }
        }

        private static PlayStack Empty = new EmptyStack();
        public static PlayStack Create(IEnumerable<Card> cards)
        {
            return Create(PlayStack.Empty, cards);
        }

        private static PlayStack Create(PlayStack parent, IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                parent = new PlayStack(parent, card);
            }
            return parent.WithTopVisible();
        }

        private Card _top;
        private int _count;
        private int _hash;

        private PlayStack() { }

        private PlayStack(PlayStack parent, Card topCard)
        {
            Parent = parent;
            _top = topCard;
            _count = parent.Count + 1;
            _hash = 81 * parent.GetHashCode() + _top.GetHashCode();
        }

        private PlayStack Parent { get; set; }

        public override Card Top { get { return _top; } }

        public override int Count { get { return _count; } }

        public override int GetHashCode()
        {
            return _hash;
        }

        protected override CardStack DoAccept(Card c, CardStack from)
        {
            if (from is PlayStack)
            {
                // all cards on top of this c are moved too
                var playablecards = from.SkipWhile(mc => mc != c);
                return Create(this, playablecards);
            }
            else
            {
                return new PlayStack(this, c);
            }
        }

        public override IEnumerable<Card> GetMovableCards()
        {
            if (Top.Visible)
            {
                yield return Top;
                foreach (var movablecard in Parent.GetMovableCards())
                    yield return movablecard;
            }
            else
                yield break;
        }

        public override bool CanAccept(Card c, CardStack from)
        {
            if (Top == null)
                return c.Value == Value.King && !(from is PlayStack && c == from.First()); // it makes no sense to move kings around
            if (c.Value == Value.Ace)
                return false;
            return (int)c.Value == (int)Top.Value - 1 && c.Color != Top.Color;
        }

        private IEnumerable<Card> GetCards()
        {
            foreach( var bottomcard in Parent)
                yield return bottomcard;
            yield return Top;
        }

        public override IEnumerator<Card> GetEnumerator()
        {
            return GetCards().GetEnumerator();
        }

       internal override CardStack Remove(Card c)
        {
            // leftovers are all cards until the card to move
            // Make sure the remaining stack has a visible top
            if (Top == c)
            {
                if (Parent is EmptyStack)
                    return Parent;
                return Parent.WithTopVisible();
            }
            else return Parent.Remove(c);
        }

       private PlayStack WithTopVisible()
       {
           if (Top.Visible)
               return this;
           return new PlayStack(Parent, Top.AsVisible());
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
                var cards = this.ToArray();
                bool morelines;
                if (Count > 0)
                {
                    if (line < 2 * (Count - 1))
                        morelines = cards[line / 2].WriteLine(line % 2);
                    else
                        morelines = Top.WriteLine(line - (2 * (Count - 1)));
                }
                else
                    morelines = WriteEmpty(line);
                Console.Write(' ');
                return morelines;
            }
        }
    }

    public class FinishStack : ListBasedCardStack
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
