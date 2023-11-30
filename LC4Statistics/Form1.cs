using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.AxHost;
using System.Collections;

namespace LC4Statistics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static void Shuffle<T>(T[] arr)
        {
            
            Random rand = new Random();
            for (int i = arr.Length - 1; i >= 1; i--)
            {
                int j = rand.Next(0, i + 1);
                T temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }

        private byte[] GetRandomKey()
        {
            
            byte[] arr = new byte[36];
            RandomNumberGenerator numberGenerator = RandomNumberGenerator.Create();
            numberGenerator.GetBytes(arr);
            arr = arr.Select(x => (byte)(x % 36)).ToArray();
            for(int i = 1; i < 36; i++)
            {
                while (arr.Take(i).Contains(arr[i]))
                {
                    arr[i] = (byte)((arr[i] + 1) % 36);
                }
            }
            return arr;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //generate random permutation as key:
            byte[] arr = GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            textBox1.Text = LC4.BytesToString(arr);
            RandomNumberGenerator randomNumberGenerator= RandomNumberGenerator.Create();
            List<Tuple<byte, byte>> result = new List<Tuple<byte, byte>>();
            List<string> states = new List<string>();
            for(int i = 0; i < 1000000; i++)
            {
                byte[] clearArray = new byte[1];
                randomNumberGenerator.GetBytes(clearArray);
                byte clear = (byte)(clearArray[0] % 36);

                string state = LC4.BytesToString(lc4.GetNormalizedState());
                states.Add(state);
                byte enc = lc4.SingleByteEncryption(clear);
                result.Add(Tuple.Create(clear, enc));
                
            }
            File.WriteAllLines("states.txt", states);
            chart1.Series[0].Points.Clear();
            int[] histogram = result.Where(x => x.Item1 == 0).GroupBy(x => x.Item2).Select(x => x.Count()).ToArray();
            for(int i=0;i< histogram.Length; i++)
            {
                chart1.Series[0].Points.AddXY(i, histogram[i]);
            }
           
            //MessageBox.Show(histogram.Length.ToString());

        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] arr = GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] data = new byte[200];
            randomNumberGenerator.GetBytes(data);
            data = data.Select(x => (byte)(x % 36)).ToArray();
            //byte[] chiffrat = lc4.Encrypt(data);
            byte[] chiffrat = new byte[200];
            File.AppendAllLines("test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J });
            for (int k = 0; k < data.Length; k++)
            {
                
                chiffrat[k] = lc4.SingleByteEncryption(data[k]);
                File.AppendAllLines("test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + data[k] + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
            }
            //MessageBox.Show(LC4.BytesToString(chiffrat));
            File.AppendAllLines("test.txt", new string[] {"----------" });
            lc4.Reset();
            File.AppendAllLines("test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J });
            //File.AppendAllLines("test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J });
            byte[] decrypted = new byte[200];
            for (int k = 0; k < data.Length; k++)
            {
                
                //MessageBox.Show(LC4.BytesToString(lc4.GetNormalizedState())+" i:"+lc4.I+" j:"+lc4.J);
                decrypted[k] =lc4.SingleByteDecryption(chiffrat[k]);
                File.AppendAllLines("test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + decrypted[k]+", enc:" + LC4.ByteToChar(chiffrat[k]) });
            }
            //byte[] decrypted = lc4.Decrypt(chiffrat);
            bool same = LC4.BytesToString(decrypted) == LC4.BytesToString(data);
            MessageBox.Show(same.ToString());

        }

        private void button3_Click(object sender, EventArgs e)
        {
            LC4 lc4 = new LC4(LC4.StringToByteState("xv7ydq#opaj_39rzut8b45wcsgehmiknf26l"), 0, 0);
            byte[] data = LC4.StringToByteState("solwbfim_about_to_put_the_hammer_down#rubberduck");
            byte[] chiffrat = new byte[data.Length];
            File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J  });
            for (int k = 0; k < data.Length; k++)
            {

                chiffrat[k] = lc4.SingleByteEncryption(data[k]);
                File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + LC4.ByteToChar(data[k]) + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
            }

        }
        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

       

        public void simulation()
        {
            int counter = 0;
            Random rgen = new Random();
            int macLength = 4;
            int dataLength = 100;
            for (int r = 0; r < 10000000; r++)//repetitions
            {
                byte[] arr = GetRandomKey();
                LC4 lc4 = new LC4(arr, 0, 0);
                List<string> information = new List<string>();
                information.Add($"::::::key: {arr}");
                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] nonce = new byte[10];
                randomNumberGenerator.GetBytes(nonce);
                nonce = nonce.Select(x => (byte)(x % 36)).ToArray();

                byte[] data = new byte[dataLength];
                randomNumberGenerator.GetBytes(data);
                data = data.Select(x => (byte)(x % 36)).ToArray();

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

                    //File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + LC4.ByteToChar(data[k]) + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
                }

                //Random change at position 10 to 100+10+macLength-1:
                int pos = rgen.Next(10, dataLength + 10+macLength-1);
                int changeDegree = rgen.Next(1, 35);
                chiffrat[pos] = (byte)((chiffrat[pos] + changeDegree) % 36);
                information.Add($"-----------------Changed at pos:{pos} by {changeDegree}");
                lc4.Reset();


                byte[] decrypted = new byte[chiffrat.Length];
                for (int k = 0; k < chiffrat.Length; k++)
                {
                    decrypted[k] = lc4.SingleByteDecryption(chiffrat[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}, dec:{decrypted[k]}");

                    //File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + LC4.ByteToChar(data[k]) + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
                }

                //check if signature is the same:
                bool same = true;
                for(int i = dataLength+10; i < dataLength+10+macLength; i++)
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
                    File.AppendAllLines("false-auth.txt", information);
                    counter++;
                }
            }
            MessageBox.Show(counter.ToString());
        }

        public void booksimulation()
        {
            int counter = 0;
            Random rgen = new Random();
            int macLength = 4;
            int dataLength = 100;
            List<byte> bookData = new List<byte>();
            foreach(FileInfo fi in new DirectoryInfo("text").EnumerateFiles())
            {
                bookData.AddRange(LC4.StringToByteState(File.ReadAllText(fi.FullName)));
            }
            for (int r = 0; r < 1000000; r++)//repetitions
            {
                byte[] arr = GetRandomKey();
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

                    //File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + LC4.ByteToChar(data[k]) + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
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

                    //File.AppendAllLines("test2.txt", new string[] { LC4.BytesToString(lc4.State) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + LC4.ByteToChar(data[k]) + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
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

        public double entropy<T>(T[] data)
        {
            int[] count = data.GroupBy(x => x).Select(x => x.Count()).ToArray();
            var p = count.Select(x => x / (double)data.Length);
            return -p.Select(x => x * Math.Log(x, 2)).Sum();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            simulation();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            booksimulation();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            List<byte> bookData = new List<byte>();
            foreach (FileInfo fi in new DirectoryInfo("text").EnumerateFiles())
            {
                bookData.AddRange(LC4.StringToByteState(File.ReadAllText(fi.FullName)));
            }
            MessageBox.Show(entropy(bookData.ToArray()).ToString());
            LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
            byte[] encrpted = lc4.Encrypt(bookData.ToArray());
            MessageBox.Show(entropy(encrpted).ToString());
        }

        public static T[][] Split<T>(IList<T> source, int size)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / size)
                .Select(x => x.Select(v => v.Value).ToArray())
                .ToArray();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            byte[] data = new byte[1000000];
            randomNumberGenerator.GetBytes(data);
            data = data.Select(x => (byte)(x % 36)).ToArray();
            MessageBox.Show(entropy(data).ToString());
            //MessageBox.Show(entropy(Split(data,2)).ToString());
            LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
            byte[] encrpted = lc4.Encrypt(data.ToArray());
            MessageBox.Show(entropy(encrpted).ToString());
            //MessageBox.Show(entropy(Split(encrpted, 2)).ToString());
        }

        List<byte[]> l = new List<byte[]>();

        private void ind()
        {
            byte[] data = { (byte)numericUpDown2.Value, (byte)numericUpDown3.Value };
            //List<string> encList = new List<string>();
            List<byte[]> encList = new List<byte[]>();
            byte i = (byte)num_i.Value;
            byte j = (byte)num_j.Value;
            for(int k = 0; k < 1000000; k++)
            {
                LC4 lc4 = new LC4(GetRandomKey(), i, j);
                byte[] enc = lc4.Encrypt(data);
                encList.Add(enc);

            }
            l = encList;
            


        }

        private void testHypothesis()
        {
            byte[] arr = GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] data = new byte[1000000];
            randomNumberGenerator.GetBytes(data);
            data = data.Select(x => (byte)(x % 36)).ToArray();
            byte[] enc = lc4.Encrypt(data);
            int counter = 0;
            int counter2 = 0;

            for (int i = 0; i < 1000000-1; i++)
            {
                if (enc[i] == 0)
                {
                    counter++;
                    bool isTrue = (data[i] == 0 && enc[i + 1] == data[i + 1])
                        || (data[i]!=0 && enc[i + 1] != data[i+1]);
                    
                    if (!isTrue)
                    {
                        MessageBox.Show("!");
                    }
                }
                else if (data[i] == enc[i])
                {
                    counter2++;
                    bool isTrue = (data[i + 1] != enc[i + 1]);
                    if (!isTrue)
                    {
                        MessageBox.Show("!2");
                    }
                }
            }
            MessageBox.Show(counter.ToString());
            MessageBox.Show(counter2.ToString());
            //yes, it is true!!!
        }

        //start not from 0!!!
        private byte[][] extractFromFixedPart(List<byte[]> chiffrate, int start, int end)
        {
            byte[][] possiblePlaintexts = new byte[end-start][];
             //is there a way?
            //reconstruct message
            for (int i = start-1; i < end - 1; i++)
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

            byte[] firstGuess = possiblePlaintexts.Select(x => x.First()).ToArray();
            string guessedMessage = LC4.BytesToString(firstGuess);
            MessageBox.Show(guessedMessage);
            return possiblePlaintexts;
        }


        public void sameMessafeAttack()
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
                LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
                byte[] c = lc4.Encrypt(message);
                chiffrate.Add(c);
            }
            extractFromFixedPart(chiffrate, 100, 100 + fixmessage.Length);
            
        }

        private byte[] removeAll(IEnumerable<byte> values)
        {
            List<byte> possible = new List<byte>();
            for(int i = 0; i < 36; i++)
            {
                if (!values.Contains((byte)i))
                {
                    possible.Add((byte)i);
                }
            }
            return possible.ToArray();
        }

        private bool allSame(IEnumerable<byte> values)
        {
            if (values.Count() == 0)
            {
                return true;
            }
            byte comp = values.First();
            foreach(byte t in values)
            {
                if (t != comp)
                {
                    return false;
                }
            }
            return true;
        }





        private void button8_Click(object sender, EventArgs e)
        {
            ind();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            //chart1.ChartAreas[0].AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto
            //given first character
            //int[] histogram = encList.Where(x => x[0] == 0).GroupBy(x => x[1]).Select(x => x.Count()).ToArray();
            //for(int i=0;i<36;i++)
            {
                int i = (int)numericUpDown1.Value;
                var v = l.Where(x => x[0] == i);
                for (int j = 0; j < 36; j++)
                {
                    var d = v.Where(x => x[1] == j);

                    chart1.Series[0].Points.AddXY(LC4.BytesToString(new byte[] { (byte)j }), d.Count());
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            testHypothesis();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            sameMessafeAttack();
        }
    }
}
