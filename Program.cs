namespace GeoNodes
{
    using System;
    using System.Device.Location;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const double distanceMeters = 35 * 1000;
            const string fileName = "./postneStevilke.csv";
            var numberOfResults = args.Length > 0 ? ParseInt(args[0]) : 5;

            var addresses = File
                .ReadLines(fileName)
                .Skip(1) // skip header
                .Select(s =>
                {
                    var row = s.Split(',');
                    return new Poi
                    {
                        City = row[4],
                        Gis = ParseCoordinates(row[0], row[1]),
                        PostalCode = ParseInt(row[5])
                    };
                }).ToList();

            while (true)
            {
                var agentLocation = ReadCoordinates();

                addresses
                    .Where(w => isWithin(w.Gis, CalculateDistantPoint(agentLocation, -distanceMeters), CalculateDistantPoint(agentLocation, distanceMeters)))
                    .Select(s => { s.Distance = s.Gis.GetDistanceTo(agentLocation); return s; })
                    .OrderBy(o => o.Distance)
                    .Take(numberOfResults)
                    .ToList()
                    .ForEach(l => Console.WriteLine(l));
            }
        }

        private static GeoCoordinate ParseCoordinates(string latitude, string longtitude) => new GeoCoordinate(ParseDouble(latitude), ParseDouble(longtitude));

        private static double ParseDouble(string s) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0.0;

        private static int ParseInt(string s) => int.TryParse(s, out var i) ? i : 0;

        private static GeoCoordinate ReadCoordinates()
        {
            Console.WriteLine("Input coordinates:");
            var input = Console.ReadLine();

            // 45.33333 44.1234
            var xy = input.Split(' ');
            // 45.33333,44.1234
            if (xy.Length != 2)
                xy = input.Split(',');

            if (xy.Length != 2)
                Environment.Exit(1);

            return new GeoCoordinate(ParseDouble(xy[0]), ParseDouble(xy[1]));
        }

        private static GeoCoordinate CalculateDistantPoint(GeoCoordinate coordinate, double distanceMeters)
        {
            const double r_earth = 6371000.0;
            const double latitude = 200.0;

            var new_latitude = coordinate.Latitude + (distanceMeters / r_earth) * (180.0 / Math.PI);
            var new_longitude = coordinate.Longitude + (distanceMeters / r_earth) * (180.0 / Math.PI) / Math.Cos(latitude * Math.PI / 180.0);

            return new GeoCoordinate(new_latitude, new_longitude);
        }

        public static bool isWithin(GeoCoordinate poi, GeoCoordinate southWest, GeoCoordinate northEast)
        {
            return poi.Latitude >= southWest.Latitude
                && poi.Latitude <= northEast.Latitude
                && poi.Longitude >= southWest.Longitude
                && poi.Longitude <= northEast.Longitude;
        }
    }
}