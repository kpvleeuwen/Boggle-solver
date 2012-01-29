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
            Word = word;
            Path = path.ToArray();
        }

        public IEnumerable<Point> Path { get; private set; }

        public string Word { get; private set; }

        public bool PathContains(Point point1, Point point2)
        {
            var prev = Path.First();
            foreach (var point in Path.Skip(1))
            {
                if (point.Equals(point1) && prev.Equals(point2))
                    return true;
                if (point.Equals(point2) && prev.Equals(point1))
                    return true;
                prev = point;
            }
            return false;
        }
    }

    struct Point
    {
        public int X, Y;
    }
}
