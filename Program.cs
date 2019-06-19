namespace GeoNodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        private static void Main(string[] args)
        {
            alglib.read_csv("./postneStevilke.csv", ',', 0x1, out var matrix);
            Console.WriteLine("Sup");
        }
    }
}