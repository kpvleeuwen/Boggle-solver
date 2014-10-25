using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace PatienceSolverConsole
{

    /// <summary>
    /// Immutable representation of a patience field state
    /// </summary>
    public class PatienceField
    {
        /// <summary>
        /// Gets valid destination stacks, from least to most likely to lead to a solution
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CardStack> GetDestinationStacks()
        {
            return PlayStacks.Cast<CardStack>()
                .Concat(FinishStacks.ToArray());
        }

        /// <summary>
        /// Gets valid origin stacks, from least to most likely to lead to a solution
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CardStack> GetOriginStacks()
        {
            yield return Stock;
            foreach (var stack in PlayStacks)
                yield return stack;
        }


        public IEnumerable<PlayStack> PlayStacks { get; private set; }
        public IEnumerable<FinishStack> FinishStacks { get; private set; }
        public Stock Stock { get; private set; }

        private int _hash;

        public PatienceField(Stock stock, IEnumerable<PlayStack> playStacks, IEnumerable<FinishStack> finishStacks)
        {
            Stock = stock;
            PlayStacks = playStacks;
            FinishStacks = finishStacks;
            _hash = DoGetHashCode();
        }

        public static PatienceField FillWithRandomCards(Random random)
        {
            var cards = GetStock().ToList();
            Util.Shuffle(cards, random);
            IEnumerable<Card> stackless = cards;
            var playstacks = new List<PlayStack>();
            var finishstacks = new List<FinishStack>();
            for (int playstack = 1; playstack <= 7; playstack++)
            {
                var stack = new PlayStack(stackless.Take(playstack));
                stackless = stackless.Skip(playstack);
                playstacks.Add(stack);
            }
            for (int finishstack = 1; finishstack <= 4; finishstack++)
            {
                var stack = new FinishStack();
                finishstacks.Add(stack);
            }
            var stock = new Stock(stackless, false);

            return new PatienceField(stock, playstacks, finishstacks);
        }

        /// <summary>
        /// returns all cards of every suit, ordered by suit.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Card> GetStock()
        {
            foreach (var suit in Util.GetValues<Suit>())
                foreach (var value in Util.GetValues<Value>())
                    yield return new Card(suit, value);
        }

        /// <summary>
        /// Prints the field to Console.Out
        /// </summary>
        public void DumpToConsole()
        {
            var toprow = FinishStacks.Cast<CardStack>()
                .Concat(new[] { Stock });
            DumpRows(toprow);
            DumpRows(PlayStacks.ToArray());
        }


        private static void DumpRows(IEnumerable<CardStack> toprow)
        {
            int i = 0;
            bool morerows;
            do
            {
                morerows = false;
                foreach (var row in toprow)
                {
                    morerows |= row.WriteLine(i);
                }
                Console.WriteLine();
                i++;
            } while (morerows);
        }

        /// <summary>
        /// Two fields equal if all the stacks are the same.
        /// But, if the location of the stacks are rearranged, the fields are equal too.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as PatienceField;
            if (other == null) return false;
            if (other.GetHashCode() != this.GetHashCode())
                return false; // this should be performant
            if (!Stock.Equals(other.Stock))
                return false;
            var mystacksOrdered = PlayStacks.OrderByDescending(s => s.GetHashCode());
            var hisstacksOrdered = other.PlayStacks.OrderByDescending(s => s.GetHashCode());

            return mystacksOrdered.SequenceEqual(hisstacksOrdered);
            // The finish stacks are not checked, because only the cards not in stock or on the play stacks are there.
            // There is only one significant order possible, so if the cards in stock and on the play stacks are equal, so must be the finish stacks.
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        private int DoGetHashCode()
        {
            var mystacksOrdered = PlayStacks.OrderByDescending(s => s.GetHashCode());
            var hashcode = 0;
            foreach (var stack in mystacksOrdered)
                hashcode = hashcode * 81 + stack.GetHashCode();
            return hashcode;
        }

        public bool IsDone()
        {
            return
                PlayStacks
                .SelectMany(s => s)
                .All(card => card.Visible);
        }

        private int GetValue(Card c)
        {
            if (c == null) return 0;
            return (int)c.Value;
        }

        public PatienceField Move(Card toMove, CardStack from, CardStack to)
        {
            var newto = to.Accept(toMove, from);
            var newfrom = from.Remove(toMove);
            return new PatienceField(
                Replace(Stock, from, newfrom),
                PlayStacks.Select(ps => Replace(ps, from, newfrom)).Select(ps => Replace(ps, to, newto)),
                FinishStacks.Select(ps => Replace(ps, from, newfrom)).Select(ps => Replace(ps, to, newto))
                );
        }

        private T Replace<T>(T instance, object tosearch, object replacement)
        {
            if (object.ReferenceEquals(tosearch, instance))
                return (T)replacement;
            return instance;
        }

        public PatienceField DoTrivialMoves()
        {
            var stacks = GetOriginStacks().ToList();

            foreach (var stack in stacks)
                foreach (var card in stack.GetMovableCards())
                    foreach (var dest in FinishStacks.Where(s => s.CanAccept(card, stack)))
                    {
                        if (FinishStacks.Select(f => GetValue(f.Top)).All(
                            value => value >= GetValue(card) - 2))
                        {
                            // this move is always valid since it 
                            // cannot block another card: 
                            // all cards that can go on top of this card, 
                            // can also enter their finish stack.
                            return Move(card, stack, dest).DoTrivialMoves();
                        }
                    }
            return this;
        }

        internal PatienceField NextCard()
        {
            return new PatienceField(Stock.NextCard(), PlayStacks, FinishStacks);
        }
    }

    static class Util
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        internal static void Shuffle<T>(IList<T> cards, Random random)
        {
            var number = cards.Count;
            for (int i = 0; i < number; i++)
            {
                var toshuffle = cards[i];
                var newplace = random.Next(number);
                cards[i] = cards[newplace];
                cards[newplace] = toshuffle;
            }
        }
    }
}
