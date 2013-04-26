using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ProtonAstroLib.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAngle();
            TestCoordinateNorthPole1();
            TestCoordinateNorthPole2();
            TestEquationOfTime();
            TestHourangle();
            TestCoordinateVegaOnEpoch();
            TestCoordinateVega();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void TestEquationOfTime()
        {
            var undertest = Constants.J2000Epoch;
            var sum = 0.0;
            for (int i = 0; i < 365; i++)
            {
                var eot = undertest.EquationOfTime().TotalMinutes;
                // We know that the two effects that cause the irreguarities can never add up to more than 20 minutes
                Debug.Assert(Math.Abs(eot) < 20, "Equation of time greater than 20min");
                sum += eot;
                undertest = undertest.AddDays(1);
            }
            // We know that over a year, all effects should cancel out
            Debug.Assert(Math.Abs(sum) < 1, "Equation of time ");
        }

        private static void TestHourangle()
        {
            Console.WriteLine(J2000Extensions.GreenwichMeanSiderialTime(DateTimeOffset.Now).Normalized.Time);
            Console.WriteLine(J2000Extensions.GreenwichSiderialTime(DateTimeOffset.Now).Normalized.Time);
        }

        private static void TestCoordinateVegaOnEpoch()
        {
            var vega = new EquatorialCoordinate(Angle.FromTime(new TimeSpan(18, 36, 56)), Angle.FromDegrees(38, 47, 3));
            var moment = Constants.J2000Epoch;
            // 17:05:30
            HorizontalCoordinate result;
            result = vega.GetHorizontalCoordinate(moment, maassluisLon, maassluisLat);
            // expected values from Stellarium
            AssertEqual(result.Azimuth, Angle.FromDegrees(197, 29, 13));
            AssertEqual(result.Altitude, Angle.FromDegrees(76, 22, 16));
        }

        static readonly Angle maassluisLat = Angle.FromDegrees(51, 55, 11.99);
        static readonly Angle maassluisLon = -Angle.FromDegrees(4, 15, 36.00);

        private static void TestCoordinateVega()
        {
            var vega = new EquatorialCoordinate(Angle.FromTime(new TimeSpan(18, 36, 56)), Angle.FromDegrees(38, 47, 3));
            var moment = new DateTimeOffset(2012, 5, 7, 23, 20, 12, TimeSpan.FromHours(2));
            // 23:56:04
            var result = vega.GetHorizontalCoordinate(moment, maassluisLon, maassluisLat);

            // expected values from Stellarium
            AssertEqual(result.Azimuth, Angle.FromDegrees(64, 19, 06));
            AssertEqual(result.Altitude, Angle.FromDegrees(30, 09, 21));
        }


        private static void TestCoordinateNorthPole1()
        {

            var northpole = new EquatorialCoordinate((Angle)13.7, Angle.FromDegrees(90.0));

            var moment = new DateTimeOffset(2012, 5, 7, 23, 20, 12, TimeSpan.FromHours(2));
            // If we're on the Earth's pole, Polaris is straight up
            var result = northpole.GetHorizontalCoordinate(moment, (Angle)34.2, Angle.FromDegrees(90));

            AssertEqual(result.Altitude.Degrees, 90.0);
        }

        private static void TestCoordinateNorthPole2()
        {
            var northpole = new EquatorialCoordinate((Angle)13.7, Angle.FromDegrees(90.0));
            var moment = new DateTimeOffset(2012, 5, 7, 23, 20, 12, TimeSpan.FromHours(2));

            var result = northpole.GetHorizontalCoordinate(moment, maassluisLon, maassluisLat);

            AssertEqual(result.Altitude, maassluisLat);
            AssertEqual(result.Azimuth, 0.0);
        }

        private static void TestAngle()
        {
            AssertEqual(Angle.FromDegrees(0.0), 0.0);
            AssertEqual(Angle.FromDegrees(180.0), Math.PI);
            AssertEqual(Angle.FromDegrees(90), Math.PI / 2);
            AssertEqual(((Angle)0.0).Degrees, 0.0);
            AssertEqual(((Angle)Math.PI).Degrees, 180);
            AssertEqual(((Angle)(Math.PI / 2)).Degrees, 90);

            AssertEqual(Angle.FromTime(TimeSpan.FromHours(3)).Degrees, 45);
            AssertEqual(Angle.FromTime(TimeSpan.FromHours(0)).Degrees, 0);

            AssertEqual(Angle.FromDegrees(12, 30, 0).Degrees, 12.5);

        }

        [DebuggerNonUserCode]
        public static void AssertEqual(double actual, double expected)
        {
            Debug.Assert(Math.Abs(actual - expected) < 1e-12, actual + "  expected " + expected);
        }

        [DebuggerNonUserCode]
        public static void AssertEqual(Angle actual, double expected)
        {
            Debug.Assert(Math.Abs((double)actual - expected) < 1e-12, actual + "  expected " + (Angle)expected);
        }

        [DebuggerNonUserCode]
        public static void AssertEqual(Angle actual, Angle expected)
        {
            Debug.Assert(Math.Abs((actual - expected).Degrees) < 1, actual + "  expected " + expected);
        }
    }


}
