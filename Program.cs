namespace GeoNodes
{
    using System;
    using System.Device.Location;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
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
                        Gis = ParseCoordinates(row),
                        PostalCode = ParseInt(row[5])
                    };
                }).ToList();

            while (true)
            {
                var agentLocation = ReadCoordinates();

                addresses
                    .Select(s => { s.Distance = s.Gis.GetDistanceTo(agentLocation); return s; })
                    .OrderBy(o => o.Distance)
                    .Take(numberOfResults)
                    .ToList()
                    .ForEach(l => Console.WriteLine(l));
            }
        }

        private static GeoCoordinate ParseCoordinates(string[] row) => new GeoCoordinate(ParseDouble(row[0]), ParseDouble(row[1]));

        private static double ParseDouble(string s) => double.TryParse(s, out var d) ? d : 0.0;

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
    }
}