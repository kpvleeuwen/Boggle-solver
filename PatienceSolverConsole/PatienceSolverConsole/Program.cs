using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PatienceSolverConsole
{
    static class Program
    {

        private static string solvableFieldsFile = "solvable.txt";

        static void Main(string[] args)
        {
            if (args.Contains("play"))
                PlayGame(PatienceField.FillWithRandomCards(new Random(4)));
            else
                SolveField(GetLastField());
        }

        private static int SolveField(int currentFieldNumber)
        {
            while (true)
            {
                currentFieldNumber++;

                var field = PatienceField.FillWithRandomCards(new Random(currentFieldNumber));
                field.DumpToConsole();
                TimeSpan timeout = TimeSpan.FromSeconds(30);
                var stopwatch = Stopwatch.StartNew();
                var solution = TrySolve(field, timeout);
                stopwatch.Stop();
                if (solution == null)
                {
                    Console.WriteLine("No solution found for field {0} in {1} seconds :(", currentFieldNumber, stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    Console.WriteLine("Solution found for field {0} in {1} seconds :)({2} steps) ",
                        currentFieldNumber, stopwatch.Elapsed.TotalSeconds, solution.GetSequence().Count());
                    File.AppendAllText(solvableFieldsFile, String.Format("{0}\r\n", currentFieldNumber));
                }
            }
        }

        private static int GetLastField()
        {

            int currentFieldNumber = 0;
            try
            {
                Int32.TryParse(File.ReadAllLines(solvableFieldsFile).Last(), out currentFieldNumber);
            }
            catch (Exception) { }
            Console.WriteLine("Last known solvable: {0}", currentFieldNumber);

            return currentFieldNumber;
        }

        private static SolverEntry TrySolve(PatienceField field, TimeSpan timeout)
        {
            Console.WriteLine(timeout);
            var solver = new Solver(field, silent: true);
            SolverEntry result = null;
            var solverThread = new Thread(() => result = solver.Solve());
            solverThread.Start();
            if (!solverThread.Join(timeout))
            {
                solverThread.Abort();
                return null;
            }
            return result;
        }



        private static void PlayGame(PatienceField field)
        {
            field.Stock.JustMoveTop = true;
            string input;
            do
            {
                field.DumpToConsole();
                input = Console.ReadLine();
                if (input.Length < 2)
                    field = field.NextCard();
                else if (input.Length == 2)
                {
                    var from = field.GetStack(input[0]);
                    var to = field.GetStack(input[1]);
                    field = Move(field, from, to);
                    if (field.IsDone())
                        break;
                }
            } while (input != "exit");
        }

        private static PatienceField Move(PatienceField field, CardStack from, CardStack to)
        {
            if (from == null || to == null)
            {
                Console.WriteLine("Invalid move: unknown stack");
                return field;
            }

            var cardToMove = from.GetMovableCards().FirstOrDefault(c => to.CanAccept(c, from));
            if (cardToMove == null)
            {
                Console.WriteLine("Invalid move: none of the cards on from can enter destination");
                return field;
            }
            return field.Move(cardToMove, from, to);

        }

        private static CardStack GetStack(this PatienceField field, char p)
        {
            switch (p)
            {
                case '1': return field.PlayStacks.Take(1).Last();
                case '2': return field.PlayStacks.Take(2).Last();
                case '3': return field.PlayStacks.Take(3).Last();
                case '4': return field.PlayStacks.Take(4).Last();
                case '5': return field.PlayStacks.Take(5).Last();
                case '6': return field.PlayStacks.Take(6).Last();
                case '7': return field.PlayStacks.Take(7).Last();
                case 'a': return field.FinishStacks.Take(1).Last();
                case 'b': return field.FinishStacks.Take(2).Last();
                case 'c': return field.FinishStacks.Take(3).Last();
                case 'd': return field.FinishStacks.Take(4).Last();
                case '0': return field.Stock;
            }
            Console.WriteLine("Unknown stack {0}", p);
            return null;
        }
    }
}
