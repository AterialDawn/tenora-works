using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psu_generic_parser.Aterial
{
    class PSUTexBinFile
    {
        const int _OFFSET_RIPC_CHUNK_SIZE_2 = 108 - 96;
        const int _OFFSET_IMG_DATA_HEADER = 1152 - 96;
        const int _OFFSET_COLOR_TABLE = 128 - 96;
        const int _OFFSET_IMG_DATA_START = 1168 - 96;
        public Color[] ColorTable { get; private set; } = new Color[256];
        public byte[,] IndexedImageInfo { get; set; } = new byte[0, 0];
        public int MaxUsableColors { get { return ColorTable.Where(c => !IsUnsafePixel(c)).Count(); } }

        byte[] originalFile = new byte[0];
        public PSUTexBinFile(Stream str)
        {
            using (BinaryReader read = new BinaryReader(str, Encoding.ASCII))
            {

                //read image information
                str.Position = _OFFSET_IMG_DATA_HEADER;
                BeginAssert("Image Data Header was incorrect");
                var imgDataVer = read.ReadUInt16();
                Assert(imgDataVer == 2, $"Image Data header version incorrect. Expected 2, read {imgDataVer}");
                var imageWidth = read.ReadUInt16();
                var imageHeight = read.ReadUInt16();
                var bpp = read.ReadUInt16();
                Assert(bpp == 8, $"Image BytesPerPixel incorrect. Expected 8, got {bpp}");

                //read image color table (always 256 colors)
                str.Position = _OFFSET_COLOR_TABLE;
                for (int i = 0; i < 256; i++)
                {
                    byte b = (byte)str.ReadByte();
                    byte g = (byte)str.ReadByte();
                    byte r = (byte)str.ReadByte();
                    byte a = (byte)str.ReadByte();
                    Color curColor = Color.FromArgb(a, r, g, b);
                    ColorTable[i] = curColor;
                }

                IndexedImageInfo = new byte[imageWidth, imageHeight];
                str.Position = _OFFSET_IMG_DATA_START;
                for (int h = 0; h < imageHeight; h++)
                {
                    for (int w = 0; w < imageWidth; w++)
                    {
                        IndexedImageInfo[w, h] = (byte)str.ReadByte();
                    }
                }

                str.Position = 0;
                originalFile = new byte[(int)str.Length];
                str.Read(originalFile, 0, originalFile.Length);

                //Done!
            }
        }

        bool IsUnsafePixel(Color col)
        {
            return col.A == 255 && col.R == 0 && col.G == 0 && col.B == 0;
        }

        public byte[] GenerateBinFile()
        {

            using (MemoryStream str = new MemoryStream(32 * 1024))
            using (BinaryWriter writer = new BinaryWriter(str, Encoding.ASCII, true))
            {
                int expectedFileSize = ((IndexedImageInfo.GetLength(0) * IndexedImageInfo.GetLength(1)) + 16) + (ColorTable.Length * 4) + 32; // 16 + 32 are header sizes

                //ripc header
                byte[] curBuffer = new byte[] { 0x52, 0x49, 0x50, 0x43, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x30, 0x84, 0x00, 0x00, 0x03, 0x00, 0x00, 0x01, 0x01, 0x00, 0x20, 0x00, 0x10, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                str.Write(curBuffer, 0, curBuffer.Length);
                //size
                long lastPos = str.Position;
                str.Position = _OFFSET_RIPC_CHUNK_SIZE_2;
                writer.Write((int)expectedFileSize);
                str.Position = lastPos;
                //dump color table
                curBuffer = new byte[256 * 4];
                for (int i = 0; i < ColorTable.Length; i++)
                {
                    var curBytes = BitConverter.GetBytes(ColorTable[i].ToArgb());
                    curBuffer[i * 4 + 0] = curBytes[0];
                    curBuffer[i * 4 + 1] = curBytes[1];
                    curBuffer[i * 4 + 2] = curBytes[2];
                    curBuffer[i * 4 + 3] = curBytes[3];
                }
                str.Write(curBuffer, 0, curBuffer.Length);

                //Write header before image data
                writer.Write((ushort)2);
                writer.Write((ushort)IndexedImageInfo.GetLength(0));
                writer.Write((ushort)IndexedImageInfo.GetLength(1));
                writer.Write((ushort)8);
                writer.Write((ushort)(IndexedImageInfo.GetLength(0) * IndexedImageInfo.GetLength(1)) + 16); //image width*image height + 16byte header
                writer.Write((ushort)0);
                writer.Write((ushort)0);

                //Loop and write image data
                for (int h = 0; h < IndexedImageInfo.GetLength(1); h++)
                {
                    for (int w = 0; w < IndexedImageInfo.GetLength(0); w++)
                    {
                        writer.Write(IndexedImageInfo[w, h]);
                    }
                }

                //pad bin file up to next multiple of 256
                long nextMultiple = ((long)Math.Ceiling((double)str.Position / 256.0)) * 256;
                curBuffer = Enumerable.Repeat<byte>(0, (int)(nextMultiple - str.Position)).ToArray();
                str.Write(curBuffer, 0, curBuffer.Length);

                return str.ToArray(); //im a god.
            }
        }

        string _lastAssertMsg = "Assertion Failed";
        void BeginAssert(string msg) => _lastAssertMsg = msg;
        void EndAssert() => _lastAssertMsg = "Assertion Failed";
        void Assert(bool condition)
        {
            if (!condition) throw new InvalidOperationException(_lastAssertMsg);
        }
        void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
