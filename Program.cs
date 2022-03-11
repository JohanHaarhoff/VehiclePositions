using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehiclePositions
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            FileReader.Read(@"C:\Users\johan\source\repos\VehiclePositions\VehiclePositions.dat");
            TimeSpan ts = DateTime.Now - startTime;
            Console.WriteLine(ts.TotalMilliseconds);
            Console.Read();
        }
    }
}
