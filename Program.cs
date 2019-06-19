namespace GeoNodes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal enum CsvHeaders
    {
        CSV_DEFAULT = 0x0,
        CSV_SKIP_HEADERS = 0x1,
    }

    internal enum Norms
    {
        Infinity_norm = 0,
        OneNorm = 1,
        TwoNorm = 2,
    }

    internal class Program
    {
        private const int _neighborsToReturn = 1;
        private static readonly Dictionary<string, int> matrixDictionary = new Dictionary<string, int>();
        private static double[,] dataSet;
        private static List<string> lines;

        private static void FindNearest(alglib.kdtree kNearestTree)
        {
            var agentLocation = GetCoordinates();

            alglib.kdtreequeryknn(kNearestTree, agentLocation, _neighborsToReturn);

            var buffer = new double[0, 0];
            alglib.kdtreequeryresultsx(kNearestTree, ref buffer);

            Console.WriteLine("{0} => {1}", alglib.ap.format(buffer, 6), GetCityFromGis(buffer));
        }

        private static string GetCityFromGis(double[,] gis)
        {
            double postalCode;
            var correctGis = $"{gis[0, 0]} {gis[0, 1]}";

            if (matrixDictionary.ContainsKey(correctGis))
                postalCode = matrixDictionary[correctGis];
            else
                return "NOT FOUND";

            var row = lines.Find(f => f.Contains($"{postalCode}")).Split(',');

            return row.Length >= 4 ? row[4] : "NOT IN LIST";
        }

        private static double[] GetCoordinates()
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

            return new double[] { double.Parse(xy[0]), double.Parse(xy[1]) };
        }

        private static void Main(string[] args)
        {
            const string fileName = "./postneStevilke.csv";

            lines = File.ReadLines(fileName).ToList();

            alglib.read_csv(fileName, ',', (int)CsvHeaders.CSV_SKIP_HEADERS, out var matrix);

            dataSet = new double[matrix.Length, 2];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                dataSet[i, 0] = matrix[i, 0];
                dataSet[i, 1] = matrix[i, 1];
                matrixDictionary.Add($"{matrix[i, 0]} {matrix[i, 1]}", (int)matrix[i, 5]);
            }

            const int spaceDimension = 2;
            const int optionalValues = 0;
            const int normtype = (int)Norms.TwoNorm;

            // Build & serialize the tree
            alglib.kdtreebuild(dataSet, spaceDimension, optionalValues, normtype, out var kNearestTree);
            while (true)
                FindNearest(kNearestTree);
        }
    }
}