using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatienceSolverConsole
{
    public class Move
    {
        static string stacknames = "01234567abcd";
        
        public int From { get; set; }

        public int To { get; set; }

        public override string ToString()
        {
            return Card.ToString() + stacknames[From] + stacknames[To];
        }

        public Card Card { get; set; }
    }
}
