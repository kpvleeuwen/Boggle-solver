using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtonAstroLib
{
    /// <summary>
    /// DateTimeOffset extension methods using the J2000 epoch
    /// </summary>
    public static class J2000Extensions
    {

        /// <summary>
        /// Calculates the mean siderial time of moment, using the J2000 epoch
        /// </summary>
        /// <param name="moment"></param>
        /// <returns></returns>
        public static Angle GreenwichMeanSiderialTime(this DateTimeOffset moment)
        {
            // From http://en.wikipedia.org/wiki/Sidereal_time#Definition
            // Greenwich Mean Sidereal Time (GMST) and UT1 differ from each other in rate, with the second of sidereal time a little shorter than that of UT1, so that (as at 2000 January 1 noon) 1.002737909350795 second of mean sidereal time was equal to 1 second of Universal Time (UT1). The ratio varies slightly with time, reaching 1.002737909409795 after a century.[3]
            // To an accuracy within 0.1 second per century, Greenwich (Mean) Sidereal Time (in hours and decimal parts of an hour) can be calculated as
            //    GMST = 18.697374558 + 24.06570982441908 * D ,
            // where D is the interval, in UT1 days including any fraction of a day, since 2000 January 1, at 12h UT (interval counted positive if forwards to a later time than the 2000 reference instant), and the result is freed from any integer multiples of 24 hours to reduce it to a value in the range 0-24.[4]

            // UT1 matches UTC within a second http://en.wikipedia.org/wiki/DUT1
            // I guess that's good enough for me.

            var D = moment.Subtract(Constants.J2000Epoch).TotalDays;
            return Angle.FromTime(TimeSpan.FromHours(18.697374558 + 24.06570982441908 * D));
        }

        public static Angle GreenwichSiderialTime(this DateTimeOffset moment)
        {
            return moment.GreenwichMeanSiderialTime() + Angle.FromTime(moment.EquationOfTime());
        }


        public static TimeSpan EquationOfTime(this DateTimeOffset moment)
        {
            var daynumber = moment.Subtract(new DateTimeOffset(moment.Year, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalDays;
            var orbitalVelocity = (Angle)(2 * Math.PI / 365.2422); // angle per day
            var meanOrbitAngle = orbitalVelocity * (daynumber + 10);
            var eccentricOrbitAngle = meanOrbitAngle + (Angle)(2 * 0.0167 * Angle.Sin(orbitalVelocity * (daynumber - 2)));
            var obliquity = Angle.FromDegrees(23.44);
            var speedDiff = meanOrbitAngle - Angle.ArcTan(Angle.Cos(obliquity), Angle.Tan(eccentricOrbitAngle));


            var threshold = Angle.FromTime(TimeSpan.FromMinutes(720)); // fix tangent
            while (speedDiff > threshold / 2)
                speedDiff -= threshold;

            return speedDiff.Time;
        }
    }
}
