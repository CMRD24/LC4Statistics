using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static LC4Statistics.Form1;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Windows.Forms;

namespace LC4Statistics
{
    public class AuthenticationTests
    {

        public static void simulation(Label lbl_round, Label lbl_counter, Chart chart)
        {
            int counter = 0;
            Random rgen = new Random();
            int macLength = 4;
            int dataLength = 100;
            int[] successfulPositionChange = new int[104];
            List<SDiff> lastStates = new List<SDiff>();
            for (int r = 0; r < 100000000; r++)//repetitions
            {
                lbl_round.Invoke(new Action(() =>
                {
                    lbl_round.Text = r.ToString();
                }));
                lbl_counter.Invoke(new Action(() =>
                {
                    lbl_counter.Text = counter.ToString();
                }));
                byte[] arr = Util.GetRandomKey();
                LC4 lc4 = new LC4(arr, 0, 0);
                List<string> information = new List<string>();
                information.Add($"::::::key: {arr}");
                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] nonce = Util.Get36Rand(10);

                byte[] data = Util.Get36Rand(dataLength);

                byte[] mac = Util.Get36Rand(macLength);

                //usually nonce would be discarded, but to know what happens still take a look:
                byte[] allDataClear = nonce.Concat(data).Concat(mac).ToArray();
                byte[] chiffrat = new byte[allDataClear.Length];


                for (int k = 0; k < allDataClear.Length; k++)
                {
                    chiffrat[k] = lc4.SingleByteEncryption(allDataClear[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}");
                }

                //Random change at position 10 to 100+10+macLength-1:
                int pos = rgen.Next(10, dataLength + 10 + macLength - 1);
                int changeDegree = rgen.Next(1, 35);
                chiffrat[pos] = (byte)((chiffrat[pos] + changeDegree) % 36);
                information.Add($"-----------------Changed at pos:{pos} by {changeDegree}");
                byte[] last = lc4.GetNormalizedState();
                lc4.Reset();


                byte[] decrypted = new byte[chiffrat.Length];
                for (int k = 0; k < chiffrat.Length; k++)
                {
                    decrypted[k] = lc4.SingleByteDecryption(chiffrat[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}, dec:{decrypted[k]}");
                }

                //check if signature is the same:
                bool same = true;
                for (int i = dataLength + 10; i < dataLength + 10 + macLength; i++)
                {
                    if (decrypted[i] != allDataClear[i])
                    {
                        same = false;
                        break;
                    }
                }



                if (same)
                {
                    successfulPositionChange[pos - 10]++;
                    lastStates.Add(new SDiff(pos - 10, last, lc4.GetNormalizedState()));
                    information.Add("-----------------");
                    information.Add("-----------------");
                    File.AppendAllLines("false-auth.txt", information);
                    counter++;
                }
            }
            chart.Invoke(new Action(() => {
                for (int i = 0; i < 104; i++)
                {
                    if (successfulPositionChange[i] != 0)
                    {
                        chart.Series[0].Points.AddXY(i, successfulPositionChange[i]);
                    }

                }
            }));

        }


        public struct SDiff
        {
            public SDiff(int pos, byte[] s1, byte[] s2)
            {
                Pos = pos;
                S1 = s1;
                S2 = s2;
            }
            public int Pos { get; set; }
            public byte[] S1 { get; set; }
            public byte[] S2 { get; set; }

            public int Similarity()
            {
                int same = 0;
                for (int i = 0; i < S1.Length; i++)
                {
                    if (S1[i] == S2[i]) { same++; }
                }
                return same;
            }
        }

        private int testProp(int st, int sameAfter, int repetitions)
        {
            //int st = 5;
            int counter = 0;
            //int sameAfter = 3;




            for (int i = 0; i < repetitions; i++)
            {
                byte[] key = Util.GetRandomKey();
                LC4 lc4 = new LC4(key, 0, 0);
                byte[] plain = Util.Get36Rand(st + 1 + sameAfter);

                byte[] enc1 = lc4.Encrypt(plain);

                byte changeValue = Util.Get36Rand(20).First(x => x != 0);
                byte[] changedFirst = new byte[st + 1 + sameAfter];
                changedFirst[0] = (byte)((plain[0] + changeValue) % 36);
                for (int j = 1; j <= st + sameAfter; j++)
                {
                    changedFirst[j] = plain[j];
                }


                LC4 lc4var = new LC4(key, 0, 0);
                byte[] enc2 = lc4var.Encrypt(changedFirst);
                bool tr = true;
                for (int j = st + 1; j < st + sameAfter + 1; j++)
                {
                    if (enc2[j] != enc1[j])
                    {
                        tr = false;
                    }
                }
                if (tr)
                {
                    // MessageBox.Show(((int)changeValue).ToString());
                    // MessageBox.Show(LC4.BytesToString(plain) + Environment.NewLine + Environment.NewLine + LC4.BytesToString(changedFirst));
                    // MessageBox.Show(LC4.BytesToString(enc1)+Environment.NewLine + Environment.NewLine + LC4.BytesToString(enc2));
                    counter++;
                }
            }

            return counter;
            //MessageBox.Show(counter.ToString());

        }

        public static void booksimulation()
        {
            int counter = 0;
            Random rgen = new Random();
            int macLength = 4;
            int dataLength = 100;
            List<byte> bookData = new List<byte>();
            foreach (FileInfo fi in new DirectoryInfo("text").EnumerateFiles())
            {
                bookData.AddRange(LC4.StringToByteState(File.ReadAllText(fi.FullName)));
            }
            for (int r = 0; r < 1000000; r++)//repetitions
            {
                byte[] arr = Util.GetRandomKey();
                LC4 lc4 = new LC4(arr, 0, 0);
                List<string> information = new List<string>();
                information.Add($"::::::key: {arr}");
                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] nonce = new byte[10];
                randomNumberGenerator.GetBytes(nonce);
                nonce = nonce.Select(x => (byte)(x % 36)).ToArray();

                byte[] data = bookData.Skip(r).Take(100).ToArray();

                byte[] mac = new byte[macLength];
                randomNumberGenerator.GetBytes(mac);
                mac = mac.Select(x => (byte)(x % 36)).ToArray();

                //usually nonce would be discarded, but to know what happens still take a look:
                byte[] allDataClear = nonce.Concat(data).Concat(mac).ToArray();
                byte[] chiffrat = new byte[allDataClear.Length];


                for (int k = 0; k < allDataClear.Length; k++)
                {
                    chiffrat[k] = lc4.SingleByteEncryption(allDataClear[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}");
                }

                //Random change at position 10 to 100+10+macLength-1:
                int pos = rgen.Next(10, dataLength + 10 + macLength - 1);
                int changeDegree = rgen.Next(1, 35);
                chiffrat[pos] = (byte)((chiffrat[pos] + changeDegree) % 36);
                information.Add($"-----------------Changed at pos:{pos} by {changeDegree}");
                lc4.Reset();


                byte[] decrypted = new byte[chiffrat.Length];
                for (int k = 0; k < chiffrat.Length; k++)
                {
                    decrypted[k] = lc4.SingleByteDecryption(chiffrat[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}, dec:{decrypted[k]}");
                }

                //check if signature is the same:
                bool same = true;
                for (int i = dataLength + 10; i < dataLength + 10 + macLength; i++)
                {
                    if (decrypted[i] != allDataClear[i])
                    {
                        same = false;
                        break;
                    }
                }



                if (same)
                {
                    information.Add("-----------------");
                    information.Add("-----------------");
                    File.AppendAllLines($"false-auth (b)n={macLength}.txt", information);
                    counter++;
                }
            }
            MessageBox.Show(counter.ToString());
        }


    }
}
