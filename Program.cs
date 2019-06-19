﻿namespace GeoNodes
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private static GeoCoordinate GetCoordinates()
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

        private static void Main(string[] args)
        {
            const int getTopX = 5;
            const string fileName = "./postneStevilke.csv";

            var agentLocation = GetCoordinates();

            var dataframe = new SortedDictionary<double, (string, GeoCoordinate, int)>(File.ReadLines(fileName)
                .Select(s =>
                {
                    var row = s.Split(',');
                    var coordinate = ParseCoordinates(row);
                    return (coordinate, city: row[4], postalCode: ParseInt(row[5]), distance: coordinate.GetDistanceTo(agentLocation));
                }).ToDictionary(k => k.distance, v => (v.city, v.coordinate, v.postalCode)));

            Console.WriteLine("distance, (city, coordinate, postalCode)");
            foreach (var i in Enumerable.Range(0, getTopX))
                Console.WriteLine(dataframe.ElementAtOrDefault(i));
            Console.ReadKey();
        }

        private static GeoCoordinate ParseCoordinates(string[] row) => new GeoCoordinate(ParseDouble(row[0]), ParseDouble(row[1]));

        private static double ParseDouble(string s) => double.TryParse(s, out var d) ? d : 0;

        private static int ParseInt(string s) => int.TryParse(s, out var i) ? i : 0;
    }
}