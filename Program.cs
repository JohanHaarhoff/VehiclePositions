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
            /// This is quite slow to the supplied benchmark. 
            /// They could be using a more raw format?
            /// Not BinaryReader?
            /// pre-declaring the list size?
            Console.WriteLine("Read");
            DateTime startTime = DateTime.Now;
            TechTest doTest = new TechTest(@"C:\Users\JohanHaarhoff\source\repos\JohanHaarhoff\VehiclePositions\VehiclePositions.dat");
            Console.WriteLine((DateTime.Now - startTime).TotalMilliseconds);

            Console.WriteLine("\nBenchmark");
            startTime = DateTime.Now;
            doTest.BenchMark();
            Console.WriteLine((DateTime.Now - startTime).TotalMilliseconds);

            Console.WriteLine("\nResults");
            for (int i = 0; i < doTest.PositionList.Count; i++)
                Console.WriteLine($"{doTest.PositionList[i].Latitude}, {doTest.PositionList[i].Longitude}, " +
                                  $"{doTest.ClosestPositionList[i].Latitude}, {doTest.ClosestPositionList[i].Longitude}");
        }
    }
}
