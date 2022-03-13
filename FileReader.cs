using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace VehiclePositions
{
    public class VehiclePositionData
    {
        public int PositionId; // Int32
        public string VehicleRegistration; // Null terminated ASCII string
        public float Latitude; // 4 byte
        public float Longitude; // 4 byte
        public UInt64 RecordedTimeUTC; 

        public VehiclePositionData() { }

        public VehiclePositionData(int positionId, string vehicleRegistration, float latitude, float longitude, UInt64 recordedTimeUTC)
        {
            PositionId = positionId;
            VehicleRegistration = vehicleRegistration;
            Latitude = latitude;
            Longitude = longitude;
            RecordedTimeUTC = recordedTimeUTC;
        }

        public VehiclePositionData SetVehiclePositionData(int positionId, string vehicleRegistration, float latitude, float longitude, UInt64 recordedTimeUTC)
        {
            PositionId = positionId;
            VehicleRegistration = vehicleRegistration;
            Latitude = latitude;
            Longitude = longitude;
            RecordedTimeUTC = recordedTimeUTC;

            return this;
        }
    }

    public class VehiclePositionList : List<VehiclePositionData>
    {
        // Add variables for bins here. 
    }

    public class TechTest
    {
        public VehiclePositionList PositionData;

        //public List<Tuple<float, float>> PositionList = new List<Tuple<float, float>>()
        // { new Tuple<float, float>(34.544909f, -102.100843f),
        //   new Tuple<float, float>(32.345544f, -99.123124f),
        //   new Tuple<float, float>(33.234235f, -100.214124f),
        //   new Tuple<float, float>(35.195739f, -95.348899f),
        //   new Tuple<float, float>(31.895839f, -97.789573f),
        //   new Tuple<float, float>(32.895839f, -101.789573f),
        //   new Tuple<float, float>(34.115839f, -100.225732f),
        //   new Tuple<float, float>(32.335839f, -99.992232f),
        //   new Tuple<float, float>(33.535339f, -94.792232f),
        //   new Tuple<float, float>(32.234235f, -100.222222f) };

        public List<(float Latitude, float Longitude)> PositionList;

        public VehiclePositionList ClosestPositionList;

        public TechTest(string fileName)
        {
            PositionData = Read(fileName);
            ClosestPositionList = new VehiclePositionList();

            PositionList = new List<(float Latitude, float Longitude)>
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
            // The data set does conform to this, with real data it might be dangerous.
            // It does avoid an iterative loop to find the end of string '\0' as BinaryReader.ReadString 
            // does not support this string termination.
            // The 10th character is '\0'.
            int registrationStringLength = 10;

            VehiclePositionList toReturn = new VehiclePositionList();
            int positionId;
            string vehicleRegistration = new string('0', 256);
            float latitude;
            float longitude;
            UInt64 recordedTimeUTC;
            BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));

            VehiclePositionData dataPoint = new VehiclePositionData();
            try
            {
                while (true)
                {
                    positionId = binReader.ReadInt32();
                    vehicleRegistration = new string(binReader.ReadChars(registrationStringLength));
                    latitude = binReader.ReadSingle();
                    longitude = binReader.ReadSingle();
                    recordedTimeUTC = binReader.ReadUInt64();
                    
                    toReturn.Add(new VehiclePositionData(positionId, vehicleRegistration, latitude, longitude, recordedTimeUTC));
                }
            }
            catch (EndOfStreamException)
            {
                // This simply means the end of the string has been reached.
            }

            binReader.Close();
            return (toReturn);
        }

        public VehiclePositionList Read3(string fileName)
        {
            // The data set does conform to this, with real data it might be dangerous.
            // It does avoid an iterative loop to find the end of string '\0' as BinaryReader.ReadString 
            // does not support this string termination.
            // The 10th character is '\0'.
            int registrationStringLength = 10;

            VehiclePositionList toReturn = (VehiclePositionList)new List<VehiclePositionData>(2000000);
            BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));

            VehiclePositionData dataPoint = new VehiclePositionData();
            try
            {
                //while (true)
                for(int i = 0; i < 2000000; i++)
                {
                    dataPoint.PositionId = binReader.ReadInt32();
                    dataPoint.VehicleRegistration = new string(binReader.ReadChars(registrationStringLength));
                    dataPoint.Latitude = binReader.ReadSingle();
                    dataPoint.Longitude = binReader.ReadSingle();
                    dataPoint.RecordedTimeUTC = binReader.ReadUInt64();
                    
                    toReturn[i] = dataPoint;//. (positionId, vehicleRegistration, latitude, longitude, recordedTimeUTC));
                }
            }
            catch (EndOfStreamException)
            {
                // This simply means the end of the string has been reached.
            }

            binReader.Close();
            return (toReturn);
        }

        public VehiclePositionList Read2(string fileName)
        {
            VehiclePositionList toReturn = new VehiclePositionList();

            BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));

            try
            {
                while (true) 
                {
                    toReturn.Add(FromBinaryReaderBlock(binReader));
                }
            }
            catch (EndOfStreamException)
            {
                // This simply means the end of the string has been reached.
            }

            binReader.Close();
            return (toReturn);
        }

        public VehiclePositionData FromBinaryReaderBlock(BinaryReader br)
        {
            byte[] buff = br.ReadBytes(Marshal.SizeOf(typeof(VehiclePositionData)));
            GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            VehiclePositionData vpd = (VehiclePositionData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(VehiclePositionData));
            handle.Free();
            return vpd;
        }

        public float AngularDistance(float lat1, float long1, float lat2, float long2)
        {
            return (float)Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(long1 - long2, 2));
        }

        public void BenchMark()
        {
            VehiclePositionData closestPosition;
            float minimumDistance;

            foreach(var position in PositionList)
            {
                closestPosition = PositionData[0];
                minimumDistance = float.MaxValue;

                foreach(var dataPoint in PositionData)
                {
                    float dist = AngularDistance(dataPoint.Latitude, dataPoint.Longitude, position.Latitude, position.Longitude);
                    if(dist < minimumDistance)
                    { 
                        closestPosition = dataPoint;
                        minimumDistance = dist;
                    }
                }
                ClosestPositionList.Add(closestPosition);
                //Console.WriteLine($"Position: {position.Item1} {position.Item2}, closest: {closestPosition.Latitide} {closestPosition.Longitude}");
            }
        }

        /// Ways to improve performance
        /// 1) Search for all ten vehicles at the same time.
        /// 2) Build subsets 
        /// 2.1) Latitude: -90 to 90
        /// 2.2) Longitude: -180 to 180
    }
}
