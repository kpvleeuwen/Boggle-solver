using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace PatienceSolverConsole
{
    public class Solver
    {
        private Stack<SolverEntry> _toTry;
        private HashSet<PatienceField> _knownFields;
        private int _move;
        private PatienceField _startfield;
        private readonly bool _silent;

        public Solver(PatienceField field, bool silent = false)
        {
            _knownFields = new HashSet<PatienceField>();
            _toTry = new Stack<SolverEntry>();
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

            while (_toTry.Any() && stopwatch.Elapsed < timeout)
            {
                var currentEntry = _toTry.Pop();

                var current = currentEntry.Field;
                Interlocked.Increment(ref _move);

                if (current.IsDone())
                {
                    Log("######### Won in {0} moves (time: {1}, evaluated {2} cases) ########", currentEntry.GetSequence().Count(), stopwatch.Elapsed, _move);
                    return currentEntry;
                }
                else
                {
                    DoMoves(currentEntry);
                }
            }
            Log("######### No solution, time: {0}, evaluated {1} cases ########", stopwatch.Elapsed, _move);
            return null;
        }

        private void DoMoves(SolverEntry currentEntry)
        {
            var current = currentEntry.Field;
            // Play => finish, just top
            foreach (var stack in current.PlayStacks.Where(s => s.Count > 0))
                foreach (var dest in current.FinishStacks.Where(s => s.CanAccept(stack.Top, stack)))
                {
                    TryMove(currentEntry, stack.Top, stack, dest);
                    if (stack.Top.Value == Value.Ace) break;
                }
            // Stock => everywhere
            foreach (var card in current.Stock.GetMovableCards())
                foreach (var dest in current.GetDestinationStacks().Where(s => s.CanAccept(card, current.Stock)))
                {
                    TryMove(currentEntry, card, current.Stock, dest);
                    if (card.Value == Value.Ace || card.Value == Value.King) break;
                }
            
            // Play => Play, partial stacks
            foreach (var stack in current.PlayStacks)
                foreach (var card in stack.GetMovableCards())
                    foreach (var dest in current.PlayStacks.Where(s => s.CanAccept(card, stack)))
                    {
                        TryMove(currentEntry, card, stack, dest);
                    }
            // Possibly, move cards back from finish=>play
            var min = current.FinishStacks.Min(s => s.GetTopValue()) + 1;
            foreach (var stack in current.FinishStacks.Where(s => s.GetTopValue() > min))
                foreach (var dest in current.PlayStacks.Where(s => s.CanAccept(stack.Top, stack)))
                {
                    TryMove(currentEntry, stack.Top, stack, dest);
                    if (stack.Top.Value == Value.Ace) break;
                }

        }

        private void TryMove(SolverEntry currentEntry, Card card, CardStack from, CardStack dest)
        {
            var field = currentEntry.Field;
            var newField = field.Move(card, from, dest);
            TryAddWork(currentEntry, newField);
        }

        private void TryAddWork(SolverEntry currentEntry, PatienceField newField)
        {
            var newFieldm = newField.DoTrivialMoves();

            if (_knownFields.Add(newFieldm))
            {
                _toTry.Push(new SolverEntry { Field = newFieldm, Previous = currentEntry });
            }
        }
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

        public string Move { get; set; }
    }
}
