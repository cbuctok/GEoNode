namespace GeoNodes
{
    using System;
    using System.Device.Location;

    internal class Poi
    {
        internal string City { get; set; }
        internal int? PostalCode { get; set; }
        internal GeoCoordinate Gis { get; set; }
        internal double Distance { get; set; }

        public override string ToString()
        {
            return $"{City} {Math.Round(Distance, 2)}m";
        }
    }
}