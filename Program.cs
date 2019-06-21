namespace GeoNodes
{
    using System;
    using System.Device.Location;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        private const double earthRadius = 6371000.0;

        private static void Main(string[] args)
        {
            const int order = 35;
            const double distanceMeters = order * 1000;
            const string fileName = "./postneStevilke.csv";

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
                        PostalCode = row[5]
                    };
                }).ToList();

            while (true)
            {
                var agentLocation = ReadCoordinates();

                var sector = (sw: CalculateDistantPoint(agentLocation, -distanceMeters), ne: CalculateDistantPoint(agentLocation, distanceMeters));
                var geoJson = ToGeoJson(agentLocation, sector);
                addresses
                    .Where(w => IsCoordinateWithinSector(w.Gis, sector))
                    .Select(s => { s.Distance = s.Gis.GetDistanceTo(agentLocation); return s; })
                    .Where(w => w.Distance < distanceMeters)
                    .OrderBy(o => o.Distance)
                    .ToList()
                    .ForEach(Console.WriteLine);
            }
        }

        private static GeoCoordinate ParseCoordinates(string latitude, string longtitude) => new GeoCoordinate(ParseDouble(latitude), ParseDouble(longtitude));

        private static double ParseDouble(string s) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0.0;

        private static GeoCoordinate ReadCoordinates()
        {
            Console.WriteLine("************* Input coordinates *************");
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
            var degreesOffset = (distanceMeters / earthRadius) * (180.0 / Math.PI);
            var latitude = coordinate.Latitude + degreesOffset;
            var longitude = coordinate.Longitude + (degreesOffset / Math.Cos(coordinate.Latitude * Math.PI / 180.0));

            return new GeoCoordinate(latitude, longitude);
        }

        public static bool IsCoordinateWithinSector(GeoCoordinate poi, (GeoCoordinate southWest, GeoCoordinate northEast) sector)
        {
            return poi.Latitude >= sector.southWest.Latitude
                && poi.Latitude <= sector.northEast.Latitude
                && poi.Longitude >= sector.southWest.Longitude
                && poi.Longitude <= sector.northEast.Longitude;
        }

        private static string ToGeoJson(GeoCoordinate agentLocation, (GeoCoordinate sw, GeoCoordinate ne) square)
        {
            return @"{
  'type': 'FeatureCollection',
  'features': [
    {
      'type': 'Feature',
      'geometry': {
        'type': 'Point',
        'coordinates': [CENTERLON, CENTERLAT]
      },
      'properties': {
        'prop0': 'CENTER'
      }
    },
    {
      'type': 'Feature',
      'geometry': {
        'type': 'Point',
        'coordinates': [SWLON, SWLAT]
      },
      'properties': {
        'prop0': 'SW'
      }
    },
    {
      'type': 'Feature',
      'geometry': {
        'type': 'Point',
        'coordinates': [NWLON, NWLAT]
      },
      'properties': {
        'prop0': 'NE'
      }
    }
  ]
}".Replace("CENTERLON", agentLocation.Longitude.ToString("G", CultureInfo.InvariantCulture))
.Replace("CENTERLAT", agentLocation.Latitude.ToString("G", CultureInfo.InvariantCulture))

.Replace("SWLON", square.sw.Longitude.ToString("G", CultureInfo.InvariantCulture))
.Replace("SWLAT", square.sw.Latitude.ToString("G", CultureInfo.InvariantCulture))

.Replace("NWLON", square.ne.Longitude.ToString("G", CultureInfo.InvariantCulture))
.Replace("NWLAT", square.ne.Latitude.ToString("G", CultureInfo.InvariantCulture))

.Replace('\'', '"')
;
        }
    }
}