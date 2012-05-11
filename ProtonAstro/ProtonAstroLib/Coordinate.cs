using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtonAstroLib
{
    public struct HorizontalCoordinate
    {
        private Angle _alt;
        private Angle _az;
        public Angle Altitude { get { return _alt; } }
        public Angle Azimuth { get { return _az; } }

        public HorizontalCoordinate(Angle alt, Angle az)
        { _alt = alt; _az = az; }

        public Angle Distance(HorizontalCoordinate other)
        {
            var dist = (Distance)12;
            var altdiff = (double)(Altitude - other.Altitude);
            var azdiff = (double)(Azimuth - other.Azimuth);
            return (Angle)Math.Sqrt(altdiff * altdiff + azdiff * azdiff);
        }
    }

    public struct EquatorialCoordinate
    {
        public Angle RightAscention { get { return _ra; } }
        public Angle Declination { get { return _dec; } }

        public EquatorialCoordinate(Angle ra, Angle dec)
        { _ra = ra; _dec = dec; }

        public Angle Distance(EquatorialCoordinate other)
        {
            var dist = (Distance)12;

            var altdiff = (double)(RightAscention - other.RightAscention);
            var azdiff = (double)(Declination - other.Declination);
            return (Angle)Math.Sqrt(altdiff * altdiff + azdiff * azdiff);
        }


        // syntactic sugar
        static readonly Func<Angle, double> sin = a => Math.Sin((double)a);
        static readonly Func<Angle, double> cos = a => Math.Cos((double)a);
        private Angle _ra;
        private Angle _dec;

        // All wikipedia below is accessed 7 may 2012

        /// <summary>
        /// Calculates the alt/az coordinates on the specified time and view coordinates
        /// </summary>
        /// <param name="moment"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        public HorizontalCoordinate GetHorizontalCoordinate(DateTimeOffset moment, Angle longitude, Angle latitude)
        {
            // Careful reading of http://en.wikipedia.org/wiki/Hour_angle#Relation_with_the_right_ascension
            var hourangle = GreenwichSiderialTime(moment) + longitude - RightAscention;
            // Code adapted from http://en.wikipedia.org/wiki/Horizontal_coordinate_system#equatorial_to_horizontal 20120504
            // This is some sphere trigonometry
            var sinAlt = sin(latitude) * sin(Declination) + cos(latitude) * cos(Declination) * cos(hourangle);
            var cosAzCosAlt = cos(latitude) * sin(Declination) - sin(latitude) * cos(Declination) * cos(hourangle);
            var sinAzCosAlt = -cos(Declination) * sin(hourangle);

            var radius = Pythagoras(cosAzCosAlt, sinAzCosAlt);
            // Using Atan2 makes sure that the angle from the right quadrant is calculated
            var azimuth = (Angle)Math.Atan2(sinAzCosAlt, cosAzCosAlt);
            var altitude = (Angle)Math.Atan2(sinAlt, radius);

            var test = Pythagoras(radius, sinAlt);

            return new HorizontalCoordinate(altitude, azimuth);
        }

        /// <summary>
        /// Calculates the right ascention offset of moment, using the J2000 epoch
        /// </summary>
        /// <param name="moment"></param>
        /// <returns></returns>
        public static Angle GreenwichSiderialTime(DateTimeOffset moment)
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

        private static double Pythagoras(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
