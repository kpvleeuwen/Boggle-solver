using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace PatienceSolverConsole
{

    public enum Suit
    {
        Clubs,
        Hearts,
        Spades,
        Diamonds
    }

    public enum Value
    {
        Ace = 1,
        _2,
        _3,
        _4,
        _5,
        _6,
        _7,
        _8,
        _9,
        _10,
        Jack,
        Queen,
        King,
    }

    public enum CardColor
    {
        Black,
        Red
    }
    [DebuggerDisplay("{Value} of {Suit}")]
    [Serializable]
    public class Card
    {
        public Card(Suit suit, Value value, bool visible = false)
        {
            Suit = suit;
            Value = value;
            Visible = visible;
        }
        
        public Suit Suit { get; private set; }

        public Value Value { get; private set; }

        public bool Visible { get; private set; }

        public CardColor Color
        {
            get
            {
                switch (Suit)
                {
                    case PatienceSolverConsole.Suit.Clubs:
                    case PatienceSolverConsole.Suit.Spades:
                        return CardColor.Black;
                }
                return CardColor.Red;
            }
        }

        public const int Height = 6;
        private Suit Suit1;
        private Value Value1;
        public bool WriteLine(int line)
        {
            using (new BlockConsoleColor(ConsoleColor.Black, ConsoleColor.Gray))
            {

                switch (line)
                {
                    case 0:
                        Console.Write(".----.");
                        return true;
                    case 1:
                        if (Visible)
                        {
                            Console.Write("|");
                            WriteCardName();
                            Console.Write("|");
                        }
                        else
                            WriteInvisibleLine(line);
                        return true;
                    case 2:
                    case 3:
                    case 4:
                        if (Visible)
                            Console.Write("|    |");
                        else
                            WriteInvisibleLine(line);
                        return true;
                    case Height - 1:
                        Console.Write("`----'"); return false;
                }
            }
            Console.Write("      "); return false;
        }

        private void WriteInvisibleLine(int line)
        {
            if (line % 2 == 0)
                Console.Write(@"|\/\/|");
            else
                Console.Write(@"|/\/\|");
        }

        /// <summary>
        /// writes 4 chars to Console.Out in the correct color
        /// </summary>
        private void WriteCardName()
        {
            using (new BlockConsoleColor(Color.ToConsoleColor()))
            {
                Console.Write(ToString());
                Console.Write(" ");
            }
        }

        public override string ToString()
        {
            return "" + Suit.ToSuitChar() + Value.ToValueString();
        }

        public override bool Equals(object obj)
        {
            var card = obj as Card;
            if (card == null) return false;
            return Value == card.Value && Suit == card.Suit;
        }

        public override int GetHashCode()
        {
            return (((int)Value) << 2 | (int)Suit).GetHashCode();
        }

        public static IEnumerable<Card> Random(int max)
        {
            var random = new Random();
            var visible = false;
            for (int i = 0; i < max; i++)
            {
                var suit = GetEnumValue<Suit>(random);
                var value = GetEnumValue<Value>(random);
                yield return new Card(suit, value) { Visible = visible };
                var sw = random.NextDouble();
                if (sw < 0.2) yield break;
                visible |= sw < 0.4;
            }
        }

        public static T GetEnumValue<T>(Random rnd)
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(rnd.Next(values.Length));
        }

        internal Card AsVisible()
        {
            if(Visible) return this;
            return new Card(Suit, Value, visible: true);
        }

        internal Card AsInvisible()
        {
            if (!Visible) return this;
            return new Card(Suit, Value, visible: false);
        }
    }

    public static class CardUtils
    {

        /// <summary>
        /// returns a 2-char string describing the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToValueString(this Value value)
        {
            switch (value)
            {
                case Value.Ace: return "A ";
                case Value._2: return "2 ";
                case Value._3: return "3 ";
                case Value._4: return "4 ";
                case Value._5: return "5 ";
                case Value._6: return "6 ";
                case Value._7: return "7 ";
                case Value._8: return "8 ";
                case Value._9: return "9 ";
                case Value._10: return "10";
                case Value.Jack: return "J ";
                case Value.Queen: return "Q ";
                case Value.King: return "K ";
            }
            throw new NotImplementedException(value.ToString());
        }

        public static ConsoleColor ToConsoleColor(this CardColor value)
        {
            switch (value)
            {
                case CardColor.Red: return ConsoleColor.Red;
                case CardColor.Black: return ConsoleColor.Black;
            }
            throw new NotImplementedException(value.ToString());
        }

        public static char ToSuitChar(this Suit value)
        {
            switch (value)
            {
                case Suit.Diamonds: return '\u2666';
                case Suit.Clubs: return '\u2663';
                case Suit.Hearts: return '\u2665';
                case Suit.Spades: return '\u2660';
            }
            throw new NotImplementedException(value.ToString());
        }
    }

    public class BlockConsoleColor : IDisposable
    {
        ConsoleColor _previousFore;
        ConsoleColor? _previousBack;

        public BlockConsoleColor(ConsoleColor newForeColor, ConsoleColor? newBackColor = null)
        {
            _previousFore = Console.ForegroundColor;
            Console.ForegroundColor = newForeColor;
            if (newBackColor != null)
            {
                _previousBack = Console.BackgroundColor;
                Console.BackgroundColor = (ConsoleColor)newBackColor;
            }
        }

        public void Dispose()
        {
            Console.ForegroundColor = _previousFore;
            if (_previousBack != null)
            {
                Console.BackgroundColor = (ConsoleColor)_previousBack;
            }
        }
    }
}
