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
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Patience : Page
    {
        public Patience()
        {
            InitializeComponent();

            Field = new PatienceField();
            Field.FillWithRandomCards(new Random(24));
            Field.Stock.JustMoveTop = true;
            playStack1.Stack = Field.PlayStacks[0];
            playStack2.Stack = Field.PlayStacks[1];
            playStack3.Stack = Field.PlayStacks[2];
            playStack4.Stack = Field.PlayStacks[3];
            playStack5.Stack = Field.PlayStacks[4];
            playStack6.Stack = Field.PlayStacks[5];
            playStack7.Stack = Field.PlayStacks[6];

            AddPlayHandler(playStack1);
            AddPlayHandler(playStack2);
            AddPlayHandler(playStack3);
            AddPlayHandler(playStack4);
            AddPlayHandler(playStack5);
            AddPlayHandler(playStack6);
            AddPlayHandler(playStack7);

            finishStack1.Stack = Field.FinishStacks[0];
            finishStack2.Stack = Field.FinishStacks[1];
            finishStack3.Stack = Field.FinishStacks[2];
            finishStack4.Stack = Field.FinishStacks[3];

            AddFinishHandler(finishStack1);
            AddFinishHandler(finishStack2);
            AddFinishHandler(finishStack3);
            AddFinishHandler(finishStack4);

            openCardStack.Stack = Field.Stock;
            openCardStack.MouseDown += (sender, e) => SetOrigin(openCardStack.Stack);

            closedCardStack.Stack = new StockComplementStack() { Stock = Field.Stock };
        }

        private void AddPlayHandler(PlayStack playStack)
        {
            playStack.MouseDown += (sender, e) =>
            {
                SetupMove(playStack.Stack);
            };
            playStack.MouseUp += (sender, e) =>
            {
                SetupMove(playStack.Stack);
            };
            playStack.MouseDoubleClick += (sender, e) =>
                {
                    var top = playStack.Stack.Top;
                    if (top != null)
                    {
                        var dest = Field.FinishStacks.FirstOrDefault(s => s.CanAccept(playStack.Stack.Top));
                        if (dest != null)
                            top.Move(dest);
                    }
                };
        }

        private void AddFinishHandler(TopStack finishStack)
        {
            finishStack.MouseUp += (sender, e) =>
                {
                    SetDestination(finishStack.Stack);
                };
            finishStack.MouseDoubleClick += (sender, e) =>
                {
                    Field.DoTrivialMoves();
                };

        }

        private void SetDestination(CardStack destination)
        {
            if (destination == _origin) return;
            var origin = _origin;
            _origin = null;
            if (origin == null || !origin.Any()) return;
            var cardToMove = origin.GetMovableCards().FirstOrDefault(destination.CanAccept);
            if (cardToMove == null)
            {
                return;
            }
            origin.Move(cardToMove, destination);
        }

        private void SetOrigin(CardStack cardStack)
        {
            _origin = cardStack;
        }

        private void SetupMove(CardStack cardStack)
        {
            if (_origin == null)
                SetOrigin(cardStack);
            else
                SetDestination(cardStack);
        }

        private CardStack _origin;

        private void NewCard(object sender, MouseButtonEventArgs e)
        {
            Field.Stock.NextCard();
        }

        public PatienceField Field { get; set; }
    }
}
