using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Wav
{
    public class WavOptions
    {
        public byte[] ChunkID = WavHandler.ToBytes("RIFF");
        public int ChunkSize;
        public byte[] Format = WavHandler.ToBytes("WAVE");
        public byte[] SubChunk1Id = WavHandler.ToBytes("fmt ");
        public int SubShunk1Size = 16;
        public short AudioFormat = 1;
        public short NumChannels = 2;
        public int SampleRate = 44100;
        public short BitsPerSample = 16;
        public int ByteRate=>SampleRate * NumChannels * BitsPerSample / 8;
        public short BlockAlign=>(short)(NumChannels*BitsPerSample/ 8);
        public byte[] SubChunk2ID = WavHandler.ToBytes("data");
        public int SubChunk2Size;
        public byte[] ExtraData = new byte[] { };
        public byte[] Data = null;
        public double Duration
        {
            get
            {
                return this.Samples/this.SampleRate;
            }
        }
        public int Samples
        {
            get
            {
                return 8*this.SubChunk2Size/(this.NumChannels*this.BitsPerSample);
            }
        }
    }
    public class WavHandler
    {
        public WavOptions options;
        public WavHandler()
        {
            this.options = new WavOptions();
        }
        public WavHandler(WavOptions opts)
        {
            this.options = opts;
        }
        public static byte[] ToBytes(string inp)
        {
            return Encoding.ASCII.GetBytes(inp);
        }
        public static WavOptions Read(string path)
        {
            WavOptions options = new WavOptions();
            byte[] bytes = File.ReadAllBytes(path).ToArray();
            List<byte> b = new List<byte>(bytes);
            options.ChunkSize = ChunkReader(b.GetRange(4, 4).ToArray());
            options.SubShunk1Size = ChunkReader(b.GetRange(16, 4).ToArray());
            options.AudioFormat = (short)ChunkReader(b.GetRange(20, 2).ToArray());
            options.NumChannels = (short)ChunkReader(b.GetRange(22, 2).ToArray());
            options.SampleRate = ChunkReader(b.GetRange(24, 4).ToArray());
            options.BitsPerSample = (short)ChunkReader(b.GetRange(34, 2).ToArray());
            int offset = 0;
            if(options.AudioFormat != 1 || options.SubShunk1Size != 16)
            {
                offset = ChunkReader(b.GetRange(36, 2).ToArray())+2;
                options.ExtraData = b.GetRange(36, offset).ToArray();
            }
            options.SubChunk2Size = ChunkReader(b.GetRange(offset+40, 4).ToArray());
            options.Data = b.GetRange(44 + offset, b.Count - offset - 44).ToArray();
            return options;
        }
        private static byte[] b2(Int16 i) => BitConverter.GetBytes(i);
        private static byte[] b4(int i) => BitConverter.GetBytes(i);
        public static void Write(WavOptions options, byte[] data, string path)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(options.ChunkID);
            bytes.AddRange(b4(options.ChunkSize));
            bytes.AddRange(options.Format);
            bytes.AddRange(options.SubChunk1Id);
            bytes.AddRange(b4(options.SubShunk1Size));
            bytes.AddRange(b2(options.AudioFormat));
            bytes.AddRange(b2(options.NumChannels));
            bytes.AddRange(b4(options.SampleRate));
            bytes.AddRange(b4(options.ByteRate));
            bytes.AddRange(b2(options.BlockAlign));
            bytes.AddRange(b2(options.BitsPerSample));
            bytes.AddRange(options.ExtraData);
            bytes.AddRange(options.SubChunk2ID);
            bytes.AddRange(b4(options.SubChunk2Size));
            bytes.AddRange(data);
            File.WriteAllBytes(path, bytes.ToArray());
        }
        public static int ChunkReader(byte[] arr)
        {
            if (arr.Length < 4) return BitConverter.ToInt16(arr);
            return BitConverter.ToInt32(arr);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string source = @"C:\____",
                   dest = @"C:\____";
            WavOptions d = WavHandler.Read(source);
            WavHandler.Write(d, d.Data, dest);
            Console.WriteLine(File.ReadAllText(source) == File.ReadAllText(dest));
        }
    }
}
