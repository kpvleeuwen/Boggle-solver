using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PatienceSolverConsole;

using BrowserPatience;

namespace BrowserPatience
{
    /// <summary>
    /// Interaction logic for PlayStack.xaml
    /// </summary>
    public partial class PlayStack : UserControl
    {
        public PlayStack()
        {
            InitializeComponent();
        }

        private PatienceSolverConsole.PlayStack _demo;
        private CardStack _stack;
        private CardStack DemoStack
        {
            get
            {
                return _demo ??
                    (_demo = new PatienceSolverConsole.PlayStack(Card.Random(5)));
            }
        }

        public CardStack Stack
        {
            get { return _stack; }
            set
            {
                if (_stack != null) throw new InvalidOperationException();
                _stack = value;
                _stack.StackChanged += (sender, e) => InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Stack == null) Stack = DemoStack;
            var y = 0;
            if (Stack.Any())
            {
                foreach (var card in Stack)
                {
                    var cardRect = new Rect(0, y, Width, Width * 1.5);
                    drawingContext.DrawCard(card, cardRect);
                    y += card.Visible ? 20 : 10;
                }
            }
            else
            {
                DrawEmptyStack(drawingContext);
            }

        }
        private void DrawEmptyStack(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(Brushes.DarkGreen, null, this.RenderSize.Center(), this.Width / 2, this.Width / 2);
        }
    }
}
