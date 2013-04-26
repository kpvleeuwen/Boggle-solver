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
        { _alt = alt.Normalized; _az = az.Normalized; }

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
            var hourangle = moment.GreenwichMeanSiderialTime() - longitude - RightAscention;
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


        private static double Pythagoras(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
