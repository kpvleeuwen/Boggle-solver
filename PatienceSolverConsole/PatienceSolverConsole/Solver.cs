using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;

namespace PatienceSolverConsole
{
    public class Solver
    {
        private Stack<SolverEntry> _list;
        
        private int _move;
        private PatienceField _startfield;
        private readonly bool _silent;

        public Solver(PatienceField field, bool silent = false)
        {
            Previous = new HashSet<PatienceField>();
            _list = new Stack<SolverEntry>();
            _startfield = field;
            _silent = silent;
        }

        private void Log(string format, params object[] args)
        {
            if (_silent) return;
            Console.WriteLine(format, args);
        }

        public SolverEntry Solve(TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            TryAddWork(null, _startfield);

            while (_list.Any() && stopwatch.Elapsed < timeout)
            {
                var currentEntry = _list.Pop();

                var current = currentEntry.Field;
                Interlocked.Increment(ref _move);
                if (!_silent && _move % 100 == 0)
                {
                    Console.WriteLine("======== {0} ({1} left, running {2})========", _move, _list.Count, stopwatch.Elapsed);
                    current.DumpToConsole();
                }


                if (current.IsDone())
                {
                    Log("######### Won in {0} moves (time: {1}, evaluated {2} cases) ########", currentEntry.GetSequence().Count(), stopwatch.Elapsed, _move);
                    return currentEntry;
                }
                else
                {
                    var stacks = current.GetOriginStacks().ToList();
                    foreach (var stack in stacks)
                        foreach (var card in stack.GetMovableCards())
                            foreach (var dest in current.GetDestinationStacks().Where(s => s.CanAccept(card, stack)))
                            {
                                TryMove(currentEntry, card, stack, dest);
                                if (card.Value == Value.Ace || card.Value == Value.King) break;
                            }
                }
                return null;
            }
            Log("######### No solution, time: {1}, evaluated {2} cases ########", stopwatch.Elapsed, _move);
            return null;
        }

        private void TryMove(SolverEntry currentEntry, Card card, CardStack from, CardStack dest)
        {
            var field = currentEntry.Field;

            var newField = field.Move(card, from, dest);
            TryAddWork(currentEntry, newField);
        }


        private void TryAddWork(SolverEntry currentEntry, PatienceField newField)
        {
            newField = newField.DoTrivialMoves();
            if (!Previous.Contains(newField))
            {
                Previous.Add(newField);
                _list.Push(new SolverEntry { Field = newField, Previous = currentEntry });
            }
        }

        public HashSet<PatienceField> Previous { get; set; }
    }

    public class SolverEntry
    {
        public SolverEntry Previous { get; internal set; }
        public PatienceField Field { get; internal set; }

        /// <summary>
        /// returns the sequence, starting with this field, with all ancestors.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PatienceField> GetSequence()
        {
            var current = this;
            while (current != null)
            {
                yield return current.Field;
                current = current.Previous;
            }
        }

    }
}
