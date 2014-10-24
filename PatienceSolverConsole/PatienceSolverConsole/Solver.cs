using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace PatienceSolverConsole
{
    public class Solver
    {
        private PatienceField _current;
        private Stack<SolverEntry> _list;
        private MemoryStream _stream;
        private BinaryFormatter _formatter;
        private int _move;
        private PatienceField _startfield;
        private readonly bool _silent;

        public Solver(PatienceField field, bool silent = false)
        {
            Previous = new HashSet<PatienceField>();
            _list = new Stack<SolverEntry>();
            _stream = new MemoryStream(256);
            _formatter = new BinaryFormatter();
            _startfield = field;
            _silent = silent;
        }

        private void Log(string format, params object[] args)
        {
            if (_silent) return;
            Console.WriteLine(format, args);
        }

        private void SetClone(PatienceField toClone)
        {
            _stream.Position = 0;
            _formatter.Serialize(_stream, toClone);
        }

        private PatienceField GetClone()
        {
            _stream.Position = 0;
            return (PatienceField)_formatter.Deserialize(_stream);
        }

        public SolverEntry Solve()
        {
            var stopwatch = Stopwatch.StartNew();
            TryAddWork(null, _startfield);
            while (_list.Any())
            {
                var currentEntry = _list.Pop();
                _current = currentEntry.Field;
                if (!_silent && _move % 100 == 0)
                {
                    Console.WriteLine("======== {0} ({1} left, running {2})========", _move, _list.Count, stopwatch.Elapsed);
                    _current.DumpToConsole();
                }

                _move++;

                if (_current.IsDone())
                {
                    Log("######### Won in {0} moves (time: {1}, evaluated {2} cases) ########", currentEntry.GetSequence().Count(), stopwatch.Elapsed, _move);
                    return currentEntry;
                }
                else
                {
                    SetClone(_current);
                    //var nextCardField = GetClone();
                    //nextCardField.Stock.NextCard();
                    //TryAddWork(currentEntry, nextCardField);

                    var stacks = _current.GetOriginStacks().ToList();
                    foreach (var card in stacks.SelectMany(s => s.GetMovableCards()))
                        foreach (var dest in _current.GetDestinationStacks().Where(s => s.CanAccept(card)))
                        {
                            TryMove(currentEntry, card, dest);
                            if (card.Value == Value.Ace || card.Value == Value.King) break;
                        }
                }
            }
            Log("######### No solution, time: {1}, evaluated {2} cases ########", stopwatch.Elapsed, _move);
            return null;
        }

        private void TryMove(SolverEntry currentEntry, Card card, CardStack dest)
        {
            var newField = GetClone();
            var newStacks = newField.GetOriginStacks();
            var newDest = newField.GetDestinationStacks().First(s => s.Equals(dest));
            var newCard = newStacks.SelectMany(s => s.GetMovableCards()).First(c => c.Equals(card));
            newCard.Stack.Move(newCard, newDest);
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
