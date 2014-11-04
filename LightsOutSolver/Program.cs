using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightsOutSolver
{
    class Program
    {
        const int _rows = 5;
        const int _columns = 5;
        const int _size = _rows * _columns;

        static void Main(string[] args)
        {
            /// Main idea: represent a field by the bits in an Int32
            /// so that means a max size of 5x5, 4x8, 3x10, 2x16 or 1x32.
            /// That way, we can calculate the effect of switching a light 
            /// by xor'ing two ints, which is fast.
            
            // Calculate the xor patterns for each light
            var lights = new int[_size];
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _columns; c++)
                {
                    lights[GetIndex(r, c)] = GetLightPattern(r, c);
                }
            
            // Setup field to solve
            var field = SetBit(2, 2);
            Console.WriteLine("Start solving field:");
            PrintLights(field);
            Console.WriteLine();

            // Start the brute-force solve process; time it for extra nerdiness.
            var stopwatch = Stopwatch.StartNew();
            // Just try all possible patterns, which are enumerated by incrementing an integer.
            for (int i = 0; i < 1 << _size; i++)
            {
                //copy field
                var result = field;
                // switch the lights of the pattern
                for (int light = 0; light < _size; light++)
                {
                    if ((i & 1 << light) != 0)
                        result = result ^ lights[light]; // <= xor magic happens here
                }
                // Test result
                if (result == 0)
                {
                    // Done
                    stopwatch.Stop();
                    PrintLights(i);
                    Console.WriteLine();
                }
            }
            Console.WriteLine(stopwatch.Elapsed);
            Console.ReadLine();
        }

        private static void PrintLights(int p)
        {
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _columns; c++)
                {
                    if ((p & SetBit(r, c)) != 0)
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                }
                Console.WriteLine();
            }
        }

        private static int GetLightPattern(int h, int w)
        {
            return
                SetBit(h, w) |
                SetBit(h - 1, w) |
                SetBit(h + 1, w) |
                SetBit(h, w - 1) |
                SetBit(h, w + 1);
        }

        private static int SetBit(int r, int c)
        {
            if (r < 0 || c < 0 || r >= _rows || c >= _columns)
                return 0;
            return 1 << GetIndex(r, c);
        }

        private static int GetIndex(int r, int c)
        {
            return _columns * r + c;
        }
    }
}
