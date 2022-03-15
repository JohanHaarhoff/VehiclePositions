using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace VehiclePositions
{
    public class VehiclePositionPoint
    {
        public int PositionId; // Int32
        public string VehicleRegistration; // Null terminated ASCII string
        public double Latitude; // 4 byte
        public double Longitude; // 4 byte
        public UInt64 RecordedTimeUTC; 

        public VehiclePositionPoint(int positionId, string vehicleRegistration, double latitude, double longitude, UInt64 recordedTimeUTC)
        {
            PositionId = positionId;
            VehicleRegistration = vehicleRegistration;
            Latitude = latitude;
            Longitude = longitude;
            RecordedTimeUTC = recordedTimeUTC;
        }

    }

    public class VehiclePositionList : List<VehiclePositionPoint> { }

    public class TechTest
    {
        // Latitudes are from +90 to -90
        // Longitudes are from -180 to + 180
        // Min in data: -102.100843, 31.895839
        // Max in data:  -94.792232, 35.195739

        private readonly double MaxLatitude = 90;
        private readonly double MaxLongitude = 180;

        private List<(double Latitude, double Longitude)> PositionList;

        public VehiclePositionList ClosestVehiclePositionList;
        private VehiclePositionList VehiclePositionData;
        private VehiclePositionList[,] VehiclePositionDataBins;

        private int NumberOfIncrements;
        private double LatitudeIncrementSize;
        private double LongitudeIncrementSize;
        private int LatitudeIndexNumber(double latitude) => (int)((latitude + MaxLatitude) / LatitudeIncrementSize);
        private int LongitudeIndexNumber(double longitude) => (int)((longitude + MaxLongitude) / LongitudeIncrementSize);
        private double LatitudeBinStart(int index) =>  (index * LatitudeIncrementSize - MaxLatitude);
        private double LongitudeBinStart(int index) => (index * LongitudeIncrementSize - MaxLongitude);
        private double LatitudeBinEnd(int index) =>  (LatitudeBinStart(index) + LatitudeIncrementSize);
        private double LongitudeBinEnd(int index) => (LongitudeBinStart(index) + LongitudeIncrementSize);

        public TechTest(string fileName)
        {
            VehiclePositionData = Read(fileName);
            ClosestVehiclePositionList = new VehiclePositionList();

            PositionList = new List<(double Latitude, double Longitude)>
         { (34.544909f, -102.100843f),
           (32.345544f, -99.123124f),
           (33.234235f, -100.214124f),
           (35.195739f, -95.348899f),
           (31.895839f, -97.789573f),
           (32.895839f, -101.789573f),
           (34.115839f, -100.225732f),
           (32.335839f, -99.992232f),
           (33.535339f, -94.792232f),
           (32.234235f, -100.222222f) };
        }

        public VehiclePositionList Read(string fileName)
        {
            // The data set does conform to this, with real world data it might be dangerous.
            // It does avoid an iterative loop to find the end of string '\0' as BinaryReader.ReadString 
            // does not support this string termination.
            // The 10th character is '\0'.
            int registrationStringLength = 10;

            VehiclePositionList toReturn = new VehiclePositionList();
            int positionId;
            string vehicleRegistration = new string('0', 256);
            double latitude;
            double longitude;
            UInt64 recordedTimeUTC;
            BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));

            try
            {
                while (true)
                {
                    positionId = binReader.ReadInt32();
                    vehicleRegistration = new string(binReader.ReadChars(registrationStringLength));
                    latitude = (double)binReader.ReadSingle();
                    longitude = (double)binReader.ReadSingle();
                    recordedTimeUTC = binReader.ReadUInt64();
                    
                    toReturn.Add(new VehiclePositionPoint(positionId, vehicleRegistration, latitude, longitude, recordedTimeUTC));
                }
            }
            catch (EndOfStreamException)
            {
                // This simply means the end of the string has been reached.
            }

            binReader.Close();
            return (toReturn);
        }

        // This is not as accurate as the great circle distance, but significantly faster.
        // From the times from the provided methods it is difficult to match the method used there,
        // however the time improvement from the bin method is clear especially with the more computationally
        // intensive great circle distand calculation.
        // With the example data provided the results are the same between the two distance calculation methods
        // are the same.
        private double AngularDistance(double lat1, double long1, double lat2, double long2)
        {
            return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(long1 - long2, 2));
        }
       
        private double GreatCircleDistance(double lat1, double long1, double lat2, double long2)
        {
            return RadiansToDegrees(Math.Acos(Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) +
                                              Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * 
                                              Math.Cos(DegreesToRadians(long2-long1))));

            double DegreesToRadians(double degrees) => (degrees/180.0) * Math.PI;
            double RadiansToDegrees(double radians) => (radians/Math.PI) * 180.0;
            
        }

        /// <summary>
        /// Iterates over whole data set.
        /// </summary>
        public void BenchMark()
        {
            VehiclePositionPoint closestPosition;
            double minimumDistance;

            foreach(var position in PositionList)
            {
                closestPosition = VehiclePositionData[0];
                minimumDistance = double.MaxValue;

                foreach(var dataPoint in VehiclePositionData)
                {
                    //double dist = AngularDistance(dataPoint.Latitude, dataPoint.Longitude, position.Latitude, position.Longitude);
                    double dist = GreatCircleDistance(dataPoint.Latitude, dataPoint.Longitude, position.Latitude, position.Longitude);
                    if(dist < minimumDistance)
                    { 
                        closestPosition = dataPoint;
                        minimumDistance = dist;
                    }
                }
                ClosestVehiclePositionList.Add(closestPosition);
            }
        }

        /// <summary>
        /// Creates bins with the data in, only searches in the relevant bin. Thereafter if the distance to a bin boundary is smaller than the 
        /// distance to the closest 
        /// </summary>
        public void CreateBinsArray()
        { 
            #region Could be in the constructor - leaving here for benchmarking now.
            // Increasing the number of increments used for the bins increase the time taken to create the bins, and reduces the time taken to search
            // for the closest positions. The user will have to tune this to their amount of data V.S. number of positions to search.
            NumberOfIncrements = 1440;//360;
            LatitudeIncrementSize = (2.0 * MaxLatitude)/NumberOfIncrements;
            LongitudeIncrementSize = (2.0 * MaxLongitude)/NumberOfIncrements;

            // [Latitude][Longitude]
            VehiclePositionDataBins = new VehiclePositionList[NumberOfIncrements, NumberOfIncrements];
            for(int latI = 0; latI < NumberOfIncrements; latI ++)
                for(int longI = 0; longI < NumberOfIncrements; longI ++)
                    VehiclePositionDataBins[latI, longI] = new VehiclePositionList() { };
            #endregion

            foreach(var position in VehiclePositionData)
                VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude), LongitudeIndexNumber(position.Longitude)].Add(position);
        }

        /// <summary>
        /// Used to make sure the amount of bins sums up to 2000000
        /// </summary>
        public void VerifyBins() 
        { 
            int numberOfBins = 0;
            int totalInBins = 0;
            for(int latI = 0; latI < NumberOfIncrements; latI ++)
                for(int longI = 0; longI < NumberOfIncrements; longI ++)
                {
                    if(VehiclePositionDataBins[latI, longI].Count != 0) 
                    { 
                        numberOfBins++;
                        totalInBins += VehiclePositionDataBins[latI, longI].Count;
                     }
                }

            Console.WriteLine($"Number of bins: {numberOfBins}, total data points in bins: {totalInBins}");
        }

        public void BenchMarkBins()
        {
            VehiclePositionPoint closestVehiclePosition;
            double minimumDistance;

            foreach(var position in PositionList)
            {
                minimumDistance = double.MaxValue;
                closestVehiclePosition = VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude), LongitudeIndexNumber(position.Longitude)][0];

                // Find the vehicle position closest to the position, searching in the bin of vehicle positions.
                FindClosestPosition(position, VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude), LongitudeIndexNumber(position.Longitude)], 
                                    ref minimumDistance, ref closestVehiclePosition);

                // Check if adjacent bins must be searched, if so seach there as well.
                VehiclePositionList extraList = new VehiclePositionList();
                if(CloserToBoundary(position, minimumDistance, extraList))
                    FindClosestPosition(position, extraList, ref minimumDistance, ref closestVehiclePosition);

                ClosestVehiclePositionList.Add(closestVehiclePosition);
            }

            // Find the vehicle position closest to the position, searching in the bin provided.
            void FindClosestPosition((double Latitude, double Longitude) position, VehiclePositionList binList, ref double mD, ref VehiclePositionPoint cP)
            { 
                foreach(var dataPoint in binList)
                {
                    double dist = GreatCircleDistance(dataPoint.Latitude, dataPoint.Longitude, position.Latitude, position.Longitude);
                    if(dist < mD)
                    { 
                        cP = dataPoint;
                        mD = dist;
                    }
                }
            }

            // If the position is closer to any bin boundary, add the bin on that boundary to a list that will be used to find the closest.
            bool CloserToBoundary((double Latitude, double Longitude) position, double mD, VehiclePositionList listToAddTo)
            {
                bool toReturn = false;
                int latI = LatitudeIndexNumber(position.Latitude);
                int longI = LongitudeIndexNumber(position.Longitude);

                //Distance + latitude
                double distPlusLatitude = AngularDistance(position.Latitude, position.Longitude, LatitudeBinEnd(latI), position.Longitude);
                //Distance - lat
                double distMinLatitude = AngularDistance(position.Latitude, position.Longitude, LatitudeBinStart(latI), position.Longitude);
                //Distance + long
                double distPlusLongitude = AngularDistance(position.Latitude, position.Longitude, position.Latitude, LongitudeBinStart(longI));
                //Distance - long
                double distMinLongitude = AngularDistance(position.Latitude, position.Longitude, position.Latitude, LongitudeBinEnd(longI));


                if(distPlusLatitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) + 1, LongitudeIndexNumber(position.Longitude)]);
                    toReturn = true;
                }
                if(distMinLatitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) - 1, LongitudeIndexNumber(position.Longitude)]);
                    toReturn = true;
                }
                if(distPlusLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude), LongitudeIndexNumber(position.Longitude) + 1]);
                    toReturn = true;
                }
                if(distPlusLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude), LongitudeIndexNumber(position.Longitude) - 1]);
                    toReturn = true;
                }

                if(distPlusLatitude < mD && distPlusLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) + 1, LongitudeIndexNumber(position.Longitude) + 1]);
                    toReturn = true;
                }
                if(distPlusLatitude < mD && distMinLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) + 1, LongitudeIndexNumber(position.Longitude) - 1]);
                    toReturn = true;
                }
                if(distMinLatitude < mD && distPlusLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) - 1, LongitudeIndexNumber(position.Longitude) + 1]);
                    toReturn = true;
                }
                if(distMinLatitude < mD && distMinLongitude < mD) 
                {
                    listToAddTo.AddRange(VehiclePositionDataBins[LatitudeIndexNumber(position.Latitude) - 1, LongitudeIndexNumber(position.Longitude) - 1]);
                    toReturn = true;
                }

                return toReturn;
            }
        }

        public void WriteResultsToScreen()
        {
            Console.WriteLine("\nResults");
            for (int i = 0; i < PositionList.Count; i++)
                Console.WriteLine($"{PositionList[i].Latitude}, {PositionList[i].Longitude}, " +
                                  $"{ClosestVehiclePositionList[i].Latitude}, {ClosestVehiclePositionList[i].Longitude}");
        }
    }
}
