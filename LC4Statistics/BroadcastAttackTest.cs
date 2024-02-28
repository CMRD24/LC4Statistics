using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LC4Statistics
{
    public class BroadcastAttackTest
    {


        //start not from 0!!!
        private static byte[][] extractFromFixedPart(List<byte[]> chiffrate, int start, int end)
        {
            byte[][] possiblePlaintexts = new byte[end - start][];

            //reconstruct message
            for (int i = start - 1; i < end - 1; i++)
            {
                int arrayIndex = i - start + 1;
                var z = chiffrate.Where(x => x[i] == 0).Select(x => x[i + 1]);
                if (allSame(z))
                {
                    possiblePlaintexts[arrayIndex] = new byte[] { z.First() };
                }
                else
                {
                    possiblePlaintexts[arrayIndex] = removeAll(z);
                }
            }

            //check knf could happen here

            byte[] firstGuess = possiblePlaintexts.Select(x => x.First()).ToArray();
            string guessedMessage = LC4.BytesToString(firstGuess);
            return possiblePlaintexts;
        }



        public static void sameMessageAttackSim()
        {
            List<int[]> l = new List<int[]>();
            for (int k = 0; k < 100; k++)
            {

                List<byte[]> chiffrate = new List<byte[]>();
                byte[] fixmessage = LC4.StringToByteState("diese_nachricht_ist_geheim");
                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] randomPart = new byte[100];
                randomNumberGenerator.GetBytes(randomPart);
                randomPart = randomPart.Select(x => (byte)(x % 36)).ToArray();
                byte[] message = randomPart.Concat(fixmessage).ToArray();


                for (int i = 0; i < 10000; i++)
                {
                    LC4 lc4 = new LC4(Util.GetRandomKey(), 0, 0);
                    byte[] c = lc4.Encrypt(message);
                    chiffrate.Add(c);
                }
                byte[][] extracted = extractFromFixedPart(chiffrate, 100, 100 + fixmessage.Length);
                byte[] firstGuess = extracted.Select(x => x.First()).ToArray();
                string guessedMessage = LC4.BytesToString(firstGuess);
                File.AppendAllLines("broadcast-attack.txt", new string[] { $"{k}: {guessedMessage}" });
                List<int> ambig = new List<int>();
                for (int i = 1; i < 20; i++)
                {
                    ambig.Add(extracted.Where(x => x.Length == i).Count());

                }
                l.Add(ambig.ToArray());

                File.AppendAllLines("broadcast-attack.txt", new string[] { $"{k}: {JsonConvert.SerializeObject(ambig)}" });
            }
            List<double> occProb = new List<double>();
            for (int i = 0; i < 19; i++)
            {
                double p = (double)l.Select(x => x[i]).Sum() / (double)l.Count;
                occProb.Add(p);
            }
            File.AppendAllLines("broadcast-attack.txt", new string[] { $"occProb: {JsonConvert.SerializeObject(occProb)}" });

        }

        private static byte[] removeAll(IEnumerable<byte> values)
        {
            List<byte> possible = new List<byte>();
            for (int i = 0; i < 36; i++)
            {
                if (!values.Contains((byte)i))
                {
                    possible.Add((byte)i);
                }
            }
            return possible.ToArray();
        }

        private static bool allSame(IEnumerable<byte> values)
        {
            if (values.Count() == 0)
            {
                return true;
            }
            byte comp = values.First();
            foreach (byte t in values)
            {
                if (t != comp)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
