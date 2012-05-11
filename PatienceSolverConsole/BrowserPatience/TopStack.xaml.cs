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

namespace BrowserPatience
{
    /// <summary>
    /// Interaction logic for TopStack.xaml
    /// </summary>
    public partial class TopStack : UserControl
    {
        public TopStack()
        {
            InitializeComponent();
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

        private CardStack _demo;

        private CardStack _stack;

        private CardStack DemoStack
        {
            get
            {
                return _demo ??
                    (_demo = new PatienceSolverConsole.Stock(Card.Random(5)));
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Stack == null) Stack = DemoStack;
            Card top;
            if ((top = Stack.LastOrDefault()) != null)
            {
                var cardRect = new Rect(0, 0, Width, Width * 1.5);
                drawingContext.DrawCard(top, cardRect);
            }
            else
            {
                DrawEmptyStack(drawingContext);
            }
        }

        private void DrawEmptyStack(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(Brushes.BlueViolet, null, this.RenderSize.Center(), this.Width / 2, this.Width / 2);
        }

    }
}
