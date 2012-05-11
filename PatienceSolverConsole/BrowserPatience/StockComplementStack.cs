using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PatienceSolverConsole;

namespace BrowserPatience
{
    class StockComplementStack : CardStack
    {
        public CardStack Stock
        {
            get { return _stack; }
            set
            {
                if (_stack != null) throw new InvalidOperationException();
                _stack = value;
                _stack.StackChanged += (sender, e) => OnStackChanged();
            }
        }

        private Card _dummycard = new Card(Suit.Clubs, Value._10) { Visible = false };
        private CardStack _stack;

        public StockComplementStack()
            : base(new Card[0]) { }

        protected override IList<Card> Cards
        {
            get
            {
                if (Stock != null && Stock.Count() > 1)
                    return new[] { _dummycard };
                return new Card[0];
            }
        }

        public override bool CanAccept(Card c)
        {
            return false;
        }

        protected override void DoAccept(Card c)
        {
            throw new NotImplementedException();
        }

        public override void Move(Card c, CardStack s)
        {
            throw new NotImplementedException();
        }

        public override bool WriteLine(int line)
        {
            throw new NotImplementedException();
        }
    }
}
