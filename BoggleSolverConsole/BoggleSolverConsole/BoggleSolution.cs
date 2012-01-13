using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoggleSolverConsole
{
    class BoggleSolution
    {

        public BoggleSolution(string word, IEnumerable<Point> path)
        {
            // TODO: Complete member initialization
            Word = word;
            Path = path.ToArray();
        }

        public IEnumerable<Point> Path { get; private set; }

        public string Word { get; private set; }
    }

    struct Point
    {
        public int X, Y;
    }
}
