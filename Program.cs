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
            DateTime startTime;
            DateTime readTime;
            DateTime binTime;
            DateTime finishTime;

            // This is quite slow compared to the supplied benchmark. 
            // My focus is the finding of closest vehicle positions, but this should be improved.
            // It is possible the supplied benchmark just reads it in without creating a structure, this methods readtime seems faster...
            startTime = DateTime.Now;
            TechTest doTest = new TechTest(@"..\..\VehiclePositions.dat");
            readTime = DateTime.Now;
            doTest.BenchMark();
            finishTime = DateTime.Now;

            Console.WriteLine($"Data file read execution time : {(readTime - startTime).TotalMilliseconds, 6:F0}");
            Console.WriteLine($"Closest position calculation execution time : {( finishTime - readTime).TotalMilliseconds, 6:F0}");
            Console.WriteLine($"Total execution time : {( finishTime - startTime).TotalMilliseconds, 6:F0}");

            doTest.WriteResultsToScreen();

            Console.WriteLine("\n*****************************************\n");
            startTime = DateTime.Now;
            TechTest doTestBins = new TechTest(@"C:\Users\JohanHaarhoff\source\repos\JohanHaarhoff\VehiclePositions\VehiclePositions.dat");
            readTime = DateTime.Now;
            doTestBins.CreateBinsArray();
            binTime = DateTime.Now;
            doTestBins.BenchMarkBins();
            finishTime = DateTime.Now;

            Console.WriteLine($"Data file read execution time : {(readTime - startTime).TotalMilliseconds, 6:F0}");
            Console.WriteLine($"Bin creation time : {( binTime - readTime).TotalMilliseconds, 6:F0}");
            Console.WriteLine($"Closest position calculation execution time : {( finishTime - binTime).TotalMilliseconds, 6:F0}");
            Console.WriteLine($"Total execution time : {( finishTime - startTime).TotalMilliseconds, 6:F0}");

            doTestBins.VerifyBins();
            doTestBins.WriteResultsToScreen();
        }
    }
}
