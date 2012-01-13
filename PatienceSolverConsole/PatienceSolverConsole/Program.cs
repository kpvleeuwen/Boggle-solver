using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PatienceSolverConsole
{
    class Program
    {
        private static PatienceField field;
        static void Main(string[] args)
        {

            field = new PatienceField();
            field.FillWithRandomCards(new Random(4));

            field.DumpToConsole();
            //1,2,3,4,5,6,7,8,10,11, 13,16,17,19 is doable

            //PlayGame();

            var solver = new Solver(field);
            var solution = solver.Solve();
            if (solution == null)
            {
                Console.WriteLine("No solution :(");
                Console.ReadKey(true);
            }
            else
            {
                var sequence = solution.GetSequence().Reverse().ToList();
                foreach (var solutionfield in sequence)
                {
                    solutionfield.DumpToConsole();
                    Console.ReadKey(true);
                }
            }
        }



        private static void PlayGame()
        {
            string input;
            do
            {
                field.DumpToConsole();
                input = Console.ReadLine();
                if (input.Length < 2)
                    field.Stock.NextCard();
                else if (input.Length == 2)
                {
                    var from = GetStack(input[0]);
                    var to = GetStack(input[1]);
                    Move(from, to);
                    if (field.IsDone())
                        break;
                }
            } while (input != "exit");
        }

        private static void Move(CardStack from, CardStack to)
        {
            if (from == null || to == null)
            {
                Console.WriteLine("Invalid move: unknown stack");
                return;
            }

            var cardToMove = from.GetMovableCards().FirstOrDefault(to.CanAccept);
            if (cardToMove == null)
            {
                Console.WriteLine("Invalid move: none of the cards on from can enter destination");
                return;
            }
            from.Move(cardToMove, to);
            return;
        }

        private static CardStack GetStack(char p)
        {
            switch (p)
            {
                case '1': return field.PlayStacks[0];
                case '2': return field.PlayStacks[1];
                case '3': return field.PlayStacks[2];
                case '4': return field.PlayStacks[3];
                case '5': return field.PlayStacks[4];
                case '6': return field.PlayStacks[5];
                case '7': return field.PlayStacks[6];
                case 'a': return field.FinishStacks[0];
                case 'b': return field.FinishStacks[1];
                case 'c': return field.FinishStacks[2];
                case 'd': return field.FinishStacks[3];
                case '0': return field.Stock;
            }
            Console.WriteLine("Unknown stack {0}", p);
            return null;
        }
    }
}
