using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LC4Statistics
{
    public class Util
    {

        public static byte[] GetRandomKey()
        {

            byte[] arr = new byte[36];
            RandomNumberGenerator numberGenerator = RandomNumberGenerator.Create();
            numberGenerator.GetBytes(arr);
            arr = arr.Select(x => (byte)(x % 36)).ToArray();
            for (int i = 1; i < 36; i++)
            {
                while (arr.Take(i).Contains(arr[i]))
                {
                    arr[i] = (byte)((arr[i] + 1) % 36);
                }
            }
            return arr;
        }

        public static byte[] Get36Rand(int nr)
        {
            byte[] b = new byte[nr * 2 + 20];
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(b);
            return b.Where(x => x < 36 * 7).Take(nr).Select(x => (byte)(x % 36)).ToArray();
        }

        public static List<byte> Convert36to256Random(byte[] data)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < data.Length; i = i + 2)
            {
                if (i + 1 >= data.Length)
                {
                    continue;
                }
                int d = data[i] * 36 + data[i + 1];
                if (d >= 1280)
                {
                    continue;
                }
                list.Add((byte)(d % 256));
            }
            return list;

        }

        public static List<byte> Convert36to256RandomBy2(byte[] data)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < data.Length - 8; i = i + 8)
            {
                byte b = (byte)(((data[i] % 2) * 128) + ((data[i + 1] % 2) * 64) + ((data[i + 2] % 2) * 32) + ((data[i + 3] % 2) * 16) + ((data[i + 4] % 2) * 8)
                    + ((data[i + 5] % 2) * 4) + ((data[i + 6] % 2) * 2) + ((data[i + 7] % 2)));
                list.Add(b);
            }
            return list;

        }

        public static List<byte> Convert36toWordRandom(byte[] data)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < data.Length - 7; i = i + 7)
            {
                if (i + 1 >= data.Length)
                {
                    continue;
                }
                ulong d = (ulong)(data[i] * 36l * 36l * 36l * 36l * 36l * 36l
                    + data[i + 1] * 36l * 36l * 36l * 36l * 36l
                    + data[i + 2] * 36l * 36l * 36l * 36l
                    + data[i + 3] * 36l * 36l * 36l
                    + data[i + 4] * 36l * 36l
                    + data[i + 5] * 36l
                    + data[i + 6])
                    ;
                ulong b32 = 1024l * 1024l * 1024 * 4;
                if (d >= b32 * 18)
                {
                    continue;
                }
                uint val32bit = (uint)(d % b32);
                byte[] intBytes = BitConverter.GetBytes(val32bit);
                list.AddRange(intBytes);
            }
            return list;

        }

    }
}
