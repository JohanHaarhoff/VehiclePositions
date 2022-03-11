using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehiclePositions
{
    public class VehiclePositionData
    {
        int PositionId; // Int32
        string VehicleRegistration; // Null terminated ASCII string
        float Latitide; // 4 byte
        float Longitude; // 4 byte
        UInt64 RecordedTimeUTC; //
        public VehiclePositionData(int positionId, string vehicleRegistration, float latitude, float longitude, UInt64 recordedTimeUTC)
        {
            PositionId = positionId;
            VehicleRegistration = vehicleRegistration;
            Latitide = latitude;
            Longitude = longitude;
            RecordedTimeUTC = recordedTimeUTC;
        }
    }

    public class VehiclePositionList : List<VehiclePositionData>
    {
        // Add variables for bins here. 
    }

    public static class FileReader
    {
        public static VehiclePositionList Read(string fileName)
        {
            VehiclePositionList toReturn = new VehiclePositionList();
            try
            {
                // Maybe using or disose of?
                BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));

                while (true) 
                {
                    int positionId = binReader.ReadInt32();

                    StringBuilder sb = new StringBuilder();
                    char charToAdd = binReader.ReadChar();
                    while (charToAdd != '\0')
                    {
                        sb.Append(charToAdd);
                        charToAdd = binReader.ReadChar();
                    }

                    string vehicleRegistration = sb.ToString();

                    float latitude = binReader.ReadSingle();
                    float longitude = binReader.ReadSingle();
                    UInt64 recordedTimeUTC = binReader.ReadUInt64();

                    toReturn.Add(new VehiclePositionData(positionId, vehicleRegistration, latitude, longitude, recordedTimeUTC));
                }
            }
            catch (EndOfStreamException)
            {
                // This simply means the end of the string has been reached.
            }

            return (toReturn);
        }

        /// Ways to improve performance
        /// 1) Search for all ten vehicles at the same time.
        /// 2) Build subsets 
        /// 2.1) Latitude: -90 to 90
        /// 2.2) Longitude: -180 to 180
    }
}
