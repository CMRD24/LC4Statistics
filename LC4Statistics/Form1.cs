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
using System.Threading;
using Newtonsoft.Json;
using System.Data.OleDb;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.ComTypes;
using MathNet.Numerics.Statistics;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using static System.Resources.ResXFileRef;

namespace LC4Statistics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
            byte[] key = LC5.StringToByteState(textBox2.Text);
            LC4 lc4 = new LC4(key, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            List<Tuple<byte, byte>> result = new List<Tuple<byte, byte>>();
            List<string> states = new List<string>();
            byte[] clear = LC4.StringToByteState(textBox7.Text);
            for (int i = 0; i < clear.Length; i++)
            {
                string state = LC4.BytesToString(lc4.GetNormalizedState());
                states.Add(state);
                byte enc = lc4.SingleByteEncryption(clear[i]);
                result.Add(Tuple.Create(clear[i], enc));
                
            }
            File.WriteAllLines("states.txt", states);
            /*chart1.Series[0].Points.Clear();
            int[] histogram = result.Where(x => x.Item1 == 0).GroupBy(x => x.Item2).Select(x => x.Count()).ToArray();
            for(int i=0;i< histogram.Length; i++)
            {
                chart1.Series[0].Points.AddXY(i, histogram[i]);
            }*/
           

        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] arr = GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] data = new byte[200];
            randomNumberGenerator.GetBytes(data);
            data = data.Select(x => (byte)(x % 36)).ToArray();
            byte[] chiffrat = new byte[200];
            File.AppendAllLines("correctnes-test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J });
            for (int k = 0; k < data.Length; k++)
            {
                
                chiffrat[k] = lc4.SingleByteEncryption(data[k]);
                File.AppendAllLines("correctnes-test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + data[k] + ", enc:" + LC4.ByteToChar(chiffrat[k]) });
            }
            File.AppendAllLines("correctnes-test.txt", new string[] {"----------" });
            lc4.Reset();
            File.AppendAllLines("correctnes-test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J });
            byte[] decrypted = new byte[200];
            for (int k = 0; k < data.Length; k++)
            {
                
                decrypted[k] =lc4.SingleByteDecryption(chiffrat[k]);
                File.AppendAllLines("correctnes-test.txt", new string[] { LC4.BytesToString(lc4.GetNormalizedState()) + " i:" + lc4.I + " j:" + lc4.J + ", clear:" + decrypted[k]+", enc:" + LC4.ByteToChar(chiffrat[k]) });
            }
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
                for(int i=0;i<S1.Length; i++)
                {
                    if (S1[i] == S2[i]) { same++; }
                }
                return same;
            }
        }


        public double randomIndAttackProb(int messageLength, int repetitions)
        {
            int count = 0;
            List<int> zeros = new List<int>();
            List<int> same = new List<int>();
            for (int i = 0; i < repetitions; i++)
            {
                byte[] key = GetRandomKey();
                byte[] data0 = get36Rand(messageLength);
                byte[] data1 = get36Rand(messageLength);


                LC4 lc4 = new LC4(key, 0, 0);
                byte[] ciphertext0 = lc4.Encrypt(data0);
                byte[] ciphertext1 = lc4.Encrypt(data1);
                var c1 = distinguishNotSame(data1, ciphertext0);
                var c2 = distinguishNotSame(data0, ciphertext1);
                var check1 = distinguishNotSame(data0, ciphertext0);
                var check2 = distinguishNotSame(data1, ciphertext1);
                same.Add(c1.SameCount);
                zeros.Add(c1.CipherZeros);
                if (c1.IsNotSame || c2.IsNotSame)
                {
                    count++;
                }
            }

            double safeprobability = (double)count / (double)repetitions;
            double prob = safeprobability + 0.5 * (1 - safeprobability);
            return prob;

        }

        public struct Distinguished
        {
            public bool IsNotSame { get; set; }
            public int CipherZeros { get; set; }
            public int SameCount { get; set; }
        }

        public Distinguished distinguishNotSame(byte[] plaintext, byte[] ciphertext)
        {
            bool isNotSame = false;
            int cipherZeros = 0;
            int same = 0;
            for(int i = 0; i < plaintext.Length-1; i++)
            {
                if ((ciphertext[i]==0 && plaintext[i]==0 && ciphertext[i+1] != plaintext[i + 1])
                    || (ciphertext[i] == 0 && plaintext[i] != 0 && ciphertext[i + 1] == plaintext[i + 1])
                    || (plaintext[i] != 0 && plaintext[i] == ciphertext[i] && ciphertext[i + 1] == plaintext[i + 1])
                    )
                {
                    isNotSame = true;
                }

                if (ciphertext[i] == 0)
                {
                    cipherZeros++;
                }
                if ( (plaintext[i] != 0 && plaintext[i] == ciphertext[i])
                   )
                {
                    same++;
                }


            }
            return new Distinguished
            {
                IsNotSame = isNotSame,
                CipherZeros = cipherZeros,
                SameCount = same
            };
        }

        public void simulation()
        {
            int counter = 0;
            Random rgen = new Random();
            int macLength = 4;
            int dataLength = 100;
            int[] successfulPositionChange = new int[104];
            List<SDiff> lastStates = new List<SDiff>();
            for (int r = 0; r < 100000000; r++)//repetitions
            {
                label1.Invoke(new Action(() =>
                {
                    label1.Text = r.ToString();
                }));
                label2.Invoke(new Action(() =>
                {
                    label2.Text = counter.ToString();
                }));
                byte[] arr = GetRandomKey();
                LC4 lc4 = new LC4(arr, 0, 0);
                List<string> information = new List<string>();
                information.Add($"::::::key: {arr}");
                RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] nonce = get36Rand(10);

                byte[] data = get36Rand(dataLength);

                byte[] mac = get36Rand(macLength);

                //usually nonce would be discarded, but to know what happens still take a look:
                byte[] allDataClear = nonce.Concat(data).Concat(mac).ToArray();
                byte[] chiffrat = new byte[allDataClear.Length];
                

                for (int k = 0; k < allDataClear.Length; k++)
                {
                    chiffrat[k] = lc4.SingleByteEncryption(allDataClear[k]);
                    information.Add($"state: {LC4.BytesToString(lc4.State)}, i: {lc4.I}, j:{lc4.J}, nstate: {LC4.BytesToString(lc4.GetNormalizedState())}, plain:{allDataClear[k]}, enc: {chiffrat[k]}");
                }

                //Random change at position 10 to 100+10+macLength-1:
                int pos = rgen.Next(10, dataLength + 10+macLength-1);
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
                    successfulPositionChange[pos - 10]++;
                    lastStates.Add(new SDiff(pos-10, last, lc4.GetNormalizedState()));
                    information.Add("-----------------");
                    information.Add("-----------------");
                    File.AppendAllLines("false-auth.txt", information);
                    counter++;
                }
            }
            chart1.Invoke(new Action(() => { 
                for (int i = 0; i < 104; i++)
                {
                    if (successfulPositionChange[i] != 0)
                    {
                        chart1.Series[0].Points.AddXY(i, successfulPositionChange[i]);
                    }
                
                }
            }));
            /*
            chart2.Invoke(new Action(() =>
            {
                for (int i = 0; i < 104; i++)
                {
                    var collect = lastStates.Where(x => x.Pos == i);
                    if (collect.Count() != 0)
                    {
                        var sim = collect.Select(x => x.Similarity()).Average();
                        chart2.Series[0].Points.AddXY(i, sim);
                    }

                }
            }));*/

        }



        private void drawPropChart()
        {
            chart1.Series.Clear();
            Thread t = new Thread(new ThreadStart(() => {
            int rep = 5000000;
            for (int s = 2; s <= 4; s++)
            {
                    string sname = "|mac|=" + s.ToString();
                Series ser = new Series("|mac|=" + s.ToString());
                ser.ChartArea = "ChartArea1";
                ser.Name = sname;

                ser.ChartType = SeriesChartType.Line;
                    chart1.Invoke(new Action(() => {
                        chart1.Series.Add(ser);
                    }));
                    for (int st = 0; st < 30; st++)
                    {
                    int count = testPropLC5(st, s, rep);
                        string l = $"{s}:{st}:{rep}:{count}";
                        label3.Invoke(new Action(() => {
                            label3.Text = l;
                        }));
                    File.AppendAllLines("authentication-failures-lc5.txt", new string[] { l });
                        chart1.Invoke(new Action(() => {
                            chart1.Series[sname].Points.AddXY(st, (double)count / (double)rep);
                        }));

                }
                
            }
            }));
            t.Start();
        }


        private int testProp(int st, int sameAfter, int repetitions)
        {
            //int st = 5;
            int counter = 0;
            //int sameAfter = 3;


            

            for (int i = 0; i < repetitions; i++)
            {
                byte[] key = GetRandomKey();
                LC4 lc4 = new LC4(key, 0, 0);
                byte[] plain = get36Rand(st+1+sameAfter);

                byte[] enc1 = lc4.Encrypt(plain);

                byte changeValue = get36Rand(20).First(x => x != 0);
                byte[] changedFirst = new byte[st+1+sameAfter];
                changedFirst[0] = (byte)((plain[0] + changeValue) % 36);
                for(int j = 1; j <= st+sameAfter;j++)
                {
                    changedFirst[j] = plain[j];
                }
                

                LC4 lc4var = new LC4(key, 0, 0);
                byte[] enc2 = lc4var.Encrypt(changedFirst);
                bool tr = true;
                for(int j = st+1; j < st + sameAfter+1; j++)
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


        private int testPropLC5(int st, int sameAfter, int repetitions)
        {

            int counter = 0;

            for (int i = 0; i < repetitions; i++)
            {
                Random rnd = new Random();
                byte[] key = GetRandomKey();
                byte i2 = (byte)rnd.Next(0, 6);
                byte j2 = (byte)rnd.Next(0, 6);
                LC5 lc5 = new LC5(key, 0, 0, i2, j2);
                byte[] plain = get36Rand(st + 1 + sameAfter);

                byte[] enc1 = lc5.Encrypt(plain);

                byte changeValue = get36Rand(20).First(x => x != 0);
                byte[] changedFirst = new byte[st + 1 + sameAfter];
                changedFirst[0] = (byte)((plain[0] + changeValue) % 36);
                for (int j = 1; j <= st + sameAfter; j++)
                {
                    changedFirst[j] = plain[j];
                }


                LC5 lc5var = new LC5(key, 0, 0, i2, j2);
                byte[] enc2 = lc5var.Encrypt(changedFirst);
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
                    counter++;
                }
            }

            return counter;

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

        public double entropy<T>(T[] data)
        {
            int[] count = data.GroupBy(x => x).Select(x => x.Count()).ToArray();
            var p = count.Select(x => x / (double)data.Length);
            return -p.Select(x => x * Math.Log(x, 2)).Sum();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(() => { 
                simulation();
            }));
            t.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            booksimulation();
        }



        private void button7_Click(object sender, EventArgs e)
        {
            List<byte> bookData = new List<byte>();
            if (Directory.Exists("text"))
            {
                foreach (FileInfo fi in new DirectoryInfo("text").EnumerateFiles())
                {
                    bookData.AddRange(LC4.StringToByteState(File.ReadAllText(fi.FullName)));
                }
            }
            
            byte[] data = get36Rand(100000);
            LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
            byte[] encrpted = lc4.Encrypt(data.ToArray());
            lc4.Reset();
            byte[] encrpted2 = lc4.Encrypt(bookData.ToArray());
            MessageBox.Show("Random data: "+entropy(data).ToString()+Environment.NewLine+
                "data in /text: " + entropy(bookData.ToArray()).ToString() + Environment.NewLine +
                "ciphertext (rand): " + entropy(encrpted).ToString() + Environment.NewLine+
                "ciphertext (/text): " + entropy(encrpted2).ToString());
        }

        List<byte[]> l = new List<byte[]>();


        private void testHypothesis()
        {
            byte[] arr = GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] data = get36Rand(1000000);
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
                        return;
                    }
                }
                else if (data[i] == enc[i])
                {
                    counter2++;
                    bool isTrue = (data[i + 1] != enc[i + 1]);
                    if (!isTrue)
                    {
                        MessageBox.Show("!2");
                        return;
                    }
                }
            }
            MessageBox.Show("success");
            MessageBox.Show(counter.ToString());
            MessageBox.Show(counter2.ToString());
            
        }

        public struct Term
        {
            public Term(byte value1, byte value2, int index)
            {
                Value1 = value1;
                Value2 = value2;
                Index = index;
            }
            public byte Value1 { get; set; }
            public byte Value2 { get; set; }

            public int Index { get; set; }
        }

        /// <summary>
        /// nur vom start bis endindex im jeweiligen chiffrat.
        /// /// </summary>
        /// <param name="chiffrate"></param>
        /// <returns></returns>
        private List<Term> build2KNF(List<byte[]> chiffrate, int start, int end)
        {
            List<Term> knf = new List<Term>();
            foreach (byte[] b in chiffrate.Skip(start).Take(end - start))
            {
                for(int i = 0; i < b.Length-1; i++)
                {
                    if (b[i] != 0)
                    {
                        knf.Add(new Term(b[i], b[i+1], i));
                    }
                }
            }
            return knf;

        }


        //start not from 0!!!
        private byte[][] extractFromFixedPart(List<byte[]> chiffrate, int start, int end)
        {
            byte[][] possiblePlaintexts = new byte[end-start][];

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

            //check knf could happen here

            byte[] firstGuess = possiblePlaintexts.Select(x => x.First()).ToArray();
            string guessedMessage = LC4.BytesToString(firstGuess);
            return possiblePlaintexts;
        }

        

        public void sameMessafeAttackSim()
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
                    LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
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
            for(int i = 0; i < 19; i++)
            {
                double p = (double)l.Select(x => x[i]).Sum() / (double)l.Count;
                occProb.Add(p);
            }
            File.AppendAllLines("broadcast-attack.txt", new string[] { $"occProb: {JsonConvert.SerializeObject(occProb)}" });

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
            byte[] data = { (byte)numericUpDown2.Value, (byte)numericUpDown3.Value };
            List<byte[]> encList = new List<byte[]>();
            for (int k = 0; k < 1000000; k++)
            {
                LC4 lc4 = new LC4(GetRandomKey(), 0, 0);
                byte[] enc = lc4.Encrypt(data);
                encList.Add(enc);

            }
            l = encList;
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            
                int i = (int)numericUpDown1.Value;
                var v = l.Where(x => x[0] == i);
                for (int j = 0; j < 36; j++)
                {
                    var d = v.Where(x => x[1] == j);

                    chart1.Series[0].Points.AddXY(LC4.BytesToString(new byte[] { (byte)j }), d.Count());
                }
            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            sameMessafeAttackSim();
        }

        private void button12_Click(object sender, EventArgs e)
        {
           
        }

        private void button13_Click(object sender, EventArgs e)
        {
            byte[] b = GetRandomKey();
            LC4 lc4 = new LC4(b, 0, 0);
            byte[] data = new byte[200];
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(data);
            data = data.Select(x => (byte)(x % 36)).ToArray();
            byte[] enc = lc4.Encrypt(data);
            File.AppendAllText("example-data.txt", Environment.NewLine + Environment.NewLine + "key: [" + string.Join(",", b.Select(x => ((int)x).ToString())) + "]");
            File.AppendAllText("example-data.txt", Environment.NewLine + "data: [" + string.Join(",", data.Select(x => ((int)x).ToString())) + "]");
            File.AppendAllText("example-data.txt", Environment.NewLine + "enc: [" + string.Join(",", enc.Select(x => ((int)x).ToString())) + "]");
        }

        List<List<byte[]>> bll = new List<List<byte[]>>();
        List<byte[]> encs = new List<byte[]>();

        public byte[] get36Rand(int nr)
        {
            byte[] b = new byte[nr*2+20];
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(b);
            return b.Where(x => x < 36 * 7).Take(nr).Select(x => (byte)(x % 36)).ToArray();
        }



        private void button16_Click(object sender, EventArgs e)
        {
            drawPropChart();
        }


        private void button17_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            string sname = "p";
            Series ser = new Series("p");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;
            ser.BorderWidth = 2;
            ser.ChartType = SeriesChartType.Line;
            chart1.Series.Add(ser);
            Series ser2 = new Series("p*");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "p*";
            ser2.BorderWidth = 1;
            ser2.IsValueShownAsLabel = false;
            ser2.ChartType = SeriesChartType.Line;
            chart1.Series.Add(ser2);
            chart1.ChartAreas[0].AxisY.Minimum = 0.5;
            chart1.ChartAreas[0].AxisY.Maximum = 1;

            Thread t = new Thread(new ThreadStart(() => {
                int rep = 1000;

                for (int s = 5; s <= 2000; s+=5)
                {

                    double p = randomIndAttackProb(s, rep);

                    double m_0 = (((double)s-1) / 36);
                    double m_e = (((double)s-1-m_0) / 36);
                    double p_corr = 1 - Math.Pow(((double)(1 + 35 * 35) / (double)(36d * 36)), m_0) * Math.Pow((double)35 / (double)36, m_0);
                    double p_ges = 2 * (p_corr) * (1 - p_corr) + p_corr * p_corr;
                    double p_game = p_ges + 0.5 * (1 - p_ges);
                        chart1.Invoke(new Action(() => {
                            chart1.Series[sname].Points.AddXY(s, p);
                            chart1.Series["p*"].Points.AddXY(s, p_game);
                        }));
                    File.AppendAllLines("random-ind-1000.txt", new string[] { $"{s};{p}" });
                }
            }));
            t.Start();
        }



        

        

        byte[] removeNofKey(byte[] key, int n)
        {
            for (int i = 35; i > 35-n; i--)
            {
                key[i] = 255;
            }
            return key;
        }




        private bool containsAny(IEnumerable<byte> array, byte[] values)
        {
            foreach(byte b in values)
            {
                if (array.Contains(b))
                {
                    return true;
                }
            }
            return false;
        }


        int getPromisingIndex(IEnumerable<byte> plain, IEnumerable<byte> cipher)
        {
            List<Tuple<int, int>> prIndexes = new List<Tuple<int, int>>();
            for (int i = 0; i < plain.Count() - 100; i++)
            {
                List<byte> pool = new List<byte>();
                var pl = plain.Skip(i).Take(10).ToArray();
                var ci = cipher.Skip(i).Take(10).ToArray();
                int score = 0;
                if (ci[0]==0 && ci[1]==0 && ci[2] == 0)
                {
                    score += 5;
                }
                if (pl[0] == ci[0])
                {
                    score++;
                    if (pl[1] == 0 || ci[1] == 0)
                    {
                        score++;
                        var z = pl.Take(2).Concat(ci.Take(2));
                        if (containsAny(z, new byte[] { pl[2], ci[2] }))
                        {
                            score++;
                            var z3 = pl.Take(3).Concat(ci.Take(3));
                            if (containsAny(z3, new byte[] { pl[3], ci[3] }))
                            {
                                score++;
                                var z4 = pl.Take(4).Concat(ci.Take(4));
                                if (containsAny(z4, new byte[] { pl[4], ci[4] }))
                                {
                                    score++;
                                    var z5 = pl.Take(5).Concat(ci.Take(5));
                                    if (containsAny(z5, new byte[] { pl[5], ci[5] }))
                                    {
                                        score++;
                                    }
                                }
                            }
                        }
                    }
                }
                prIndexes.Add(Tuple.Create(i, score));
            }
            return prIndexes.OrderByDescending(x => x.Item2).First().Item1;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            int upTo = (int)numericUpDown4.Value;
            int repetitions = 20;
            chart1.Series.Clear();
            string sname = "Kollisionen";
            Series ser = new Series("Kollisionen");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;

            ser.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser);

            Series ser2 = new Series("avg");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "avg";
            ser2.BorderWidth = 2;

            ser2.ChartType = SeriesChartType.Line;
            chart1.Series.Add(ser2);

            chart1.ChartAreas[0].AxisY.Minimum = 0;

            Thread t = new Thread(new ThreadStart(()=>{
                List<int[]> collisions = new List<int[]>();
                for (int k = 0; k < repetitions; k++)
                {
                    var key0 = GetRandomKey();
                    LC4 lc4a = new LC4(key0, 0, 0);
                    int msgLength = 500000;
                    byte[] data0 = get36Rand(msgLength);
                    byte[][] states = new byte[msgLength][];
                    byte[] cipher0 = new byte[msgLength];

                    for (int i = 0; i < msgLength; i++)
                    {
                        states[i] = lc4a.GetNormalizedState();
                        cipher0[i] = lc4a.SingleByteEncryption(data0[i]);

                    }
                    //get promising index:
                    int index = 0;//getPromisingIndex(data0, cipher0);
                    var data = data0.Skip(index).ToArray();
                    var cipher = cipher0.Skip(index).ToArray();
                    var key = states[index];
                    Backtracking backtracking = new Backtracking();

                    int[] coll = new int[upTo+1];

                    for (int i = 5; i <= upTo; i++)
                    {
                        byte[] partialKey = new byte[36];
                        Array.Copy(key, partialKey, 36);

                        partialKey = removeNofKey(partialKey, i);
                        backtracking.ResetCollisions();
                        backtracking.calculateKey(partialKey, data, cipher, 0);
                        if (backtracking.Collisions > 0)
                        {
                            double log2 = Math.Log(backtracking.Collisions, 2);
                           
                            chart1.Invoke(new Action(() => {
                                chart1.Series[0].Points.AddXY(i, log2);
                            }));
                        }
                        coll[i] = backtracking.Collisions;

                        label3.Invoke(new Action(() =>
                        {
                            label3.Text = i.ToString();
                        }));
                    }
                    collisions.Add(coll);

                }
                double[] avg = new double[upTo+1];
                for(int i = 5; i <=upTo; i++)
                {
                    avg[i] = collisions.Select(x => x[i]).Average();
                    if (avg[i] > 1)
                    {
                        double log2 = Math.Log(avg[i], 2);

                        chart1.Invoke(new Action(() => {
                            chart1.Series[1].Points.AddXY(i, log2);
                        }));
                    }
                }
                
            
                

            }));
            t.Start();

        }

        

        


        private void button21_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            string sname = "Kollisionen";
            Series ser = new Series("Kollisionen");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;

            ser.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser);

            Series ser2 = new Series("avg");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "avg";
            ser2.BorderWidth = 2;

            ser2.ChartType = SeriesChartType.Line;
            chart1.Series.Add(ser2);
            int round = 1;

            Thread t = new Thread(new ThreadStart(() => {
                List<double> complexities = new List<double>();
            for(int k = 0; k < 200; k++)
            {
                var key0 = GetRandomKey();
                LC4 lc4a = new LC4(key0, 0, 0);
                int msgLength = 500;
                byte[] data0 = get36Rand(msgLength);
                                                    
                
                byte[][] states = new byte[msgLength][];
                byte[] cipher0 = new byte[msgLength];

                

                for (int i = 0; i < msgLength; i++)
                {
                    states[i] = lc4a.GetNormalizedState();
                    cipher0[i] = lc4a.SingleByteEncryption(data0[i]);
                }


                    var estimatedCollisionsTotal1 = LC4CollisionEstimation. getEstimatedCollisionsForPair(cipher0, data0, states.ToList());

                double log2 = Math.Log(estimatedCollisionsTotal1.EstimatedTotal, 2);
                    File.AppendAllLines("collEst-woSelection.txt", new string[] { $"{LC4.BytesToString(data0)}; {LC4.BytesToString(cipher0)}; {estimatedCollisionsTotal1.EstimatedTotal}; {estimatedCollisionsTotal1.NrOf0to5Possibilities}; {estimatedCollisionsTotal1.Avg5to10Possibilities}; {estimatedCollisionsTotal1.Avg10to15Possibilities}; {estimatedCollisionsTotal1.AvgCollisionsWith15Known}" });
                    
                    chart1.Invoke(new Action(() => {
                        if (log2 < 0)
                        {
                            log2 = 0;
                        }
                        chart1.Series[0].Points.AddXY(round, log2);
                    }));
                    round++;
                    complexities.Add(estimatedCollisionsTotal1.EstimatedTotal);
                    label3.Invoke(new Action(() => {
                        label3.Text = Math.Log(complexities.Average(), 2).ToString();
                    }));
                    label4.Invoke(new Action(() => {
                        label4.Text = Math.Log(complexities.Median(), 2).ToString();
                    }));
                    label5.Invoke(new Action(() => {
                        label5.Text = Math.Log(complexities.StandardDeviation(), 2).ToString();
                    }));
                }
                MessageBox.Show("fertig!");
            }));
            t.Start();


        }


        private void button22_Click(object sender, EventArgs e)
        {
            
            chart1.Series.Clear();
            string sname = "Kollisionen";
            Series ser = new Series("Kollisionen");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;

            ser.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser);

            Series ser2 = new Series("avg");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "avg";
            ser2.BorderWidth = 2;

            ser2.ChartType = SeriesChartType.Line;
            chart1.Series.Add(ser2);
            int round = 1;

            Thread t = new Thread(new ThreadStart(() => {
                List<double> bestcomplexities = new List<double>();
                for (int k = 0; k < 100; k++)
                {
                    var key0 = GetRandomKey();
                    LC4 lc4a = new LC4(key0, 0, 0);
                    int msgLength = 200;
                    byte[] data0 = get36Rand(msgLength);


                    byte[][] states = new byte[msgLength][];
                    byte[] cipher0 = new byte[msgLength];

                    File.AppendAllLines("collEstimation3.txt", new string[] { "-----------------------", LC4.BytesToString(key0) });

                    for (int i = 0; i < msgLength; i++)
                    {
                        states[i] = lc4a.GetNormalizedState();
                        cipher0[i] = lc4a.SingleByteEncryption(data0[i]);
                    }

                    List<double> complexities = new List<double>();
                    for (int i = 0; i < 100; i++)
                    {
                        var cipher = cipher0.Skip(i).ToArray();
                        var data = data0.Skip(i).ToArray();
                        var states_i = states.Skip(i).ToArray();
                        var estimatedCollisionsTotal1 = LC4CollisionEstimation.getEstimatedCollisionsForPair(cipher, data, states_i.ToList());

                        double log2 = Math.Log(estimatedCollisionsTotal1.EstimatedTotal, 2);
                        File.AppendAllLines("collEstimation3.txt", new string[] { $"{i}: plain:{LC4.BytesToString(data)}, cipher:{LC4.BytesToString(cipher)}, est. collisions: {estimatedCollisionsTotal1.EstimatedTotal}, (a: {estimatedCollisionsTotal1.NrOf0to5Possibilities}, b: {estimatedCollisionsTotal1.Avg5to10Possibilities}, c: {estimatedCollisionsTotal1.Avg10to15Possibilities}, d: {estimatedCollisionsTotal1.AvgCollisionsWith15Known})" });

                        chart1.Invoke(new Action(() => {
                            if (log2 < 0)
                            {
                                log2 = 0;
                            }
                            chart1.Series[0].Points.AddXY(round, log2);
                        }));
                        complexities.Add(estimatedCollisionsTotal1.EstimatedTotal);
                    }
                    double bestValue = complexities.Min();
                    int index = complexities.IndexOf(bestValue);
                    File.AppendAllLines("collEstimation3.txt", new string[] {$"best Index: {index}", "---------------------------------------------"});
 
                     bestcomplexities.Add(bestValue);
                    
                    round++;
                    
                    label3.Invoke(new Action(() => {
                        label3.Text = Math.Log(bestcomplexities.Average(), 2).ToString();
                    }));
                }
                MessageBox.Show("fertig!");
            }));
            t.Start();
        }

       


        private void button25_Click(object sender, EventArgs e)
        {
            testKPA();
        }

        void testKPA()
        {
            chart1.Series.Clear();
            string sname = "Kollisionen";
            Series ser = new Series("Kollisionen");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;

            ser.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser);

            Series ser2 = new Series("avg");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "new";

            ser2.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser2);

            Thread t = new Thread(new ThreadStart(() => {
                List<double> bestcomplexities = new List<double>();
                for (int i = 0; i < 200; i++)
                {
                    int msgLength = 200;
                    byte[] plain = get36Rand(msgLength);
                    byte[][] states = new byte[msgLength][];
                    byte[] cipherCalc = new byte[msgLength];

                    byte[] key = GetRandomKey();
                    LC4 lc4 = new LC4(key, 0, 0);
                    for (int j = 0; j < msgLength; j++)
                    {
                        states[j] = lc4.GetNormalizedState();
                        cipherCalc[j] = lc4.SingleByteEncryption(plain[j]);
                    }


                    Tuple<int, int> bestIndexValue = Tuple.Create(int.MaxValue, int.MaxValue);
                    int bestIndex = 0;
                    for(int j = 0; j < 100; j++)
                    {
                        IndexSearch isearch = new IndexSearch();
                        var r = isearch.check(plain.Skip(j).ToArray(), cipherCalc.Skip(j).ToArray());
                        if(r.Item1 < bestIndexValue.Item1 || (r.Item1==bestIndexValue.Item1 && r.Item2 < bestIndexValue.Item2))
                        {
                            bestIndexValue = r;
                            bestIndex = j;
                        }
                    }
                    var estimatedCollisionsTotal1 = LC4CollisionEstimation.getEstimatedCollisionsForPair(cipherCalc.Skip(bestIndex).ToArray(), plain.Skip(bestIndex).ToArray(), states.Skip(bestIndex).ToList());
                    double log2 = Math.Log(estimatedCollisionsTotal1.EstimatedTotal, 2);
                    File.AppendAllLines("collEst-w.index.Selection.txt", new string[] { $"{bestIndex}; {LC4.BytesToString(plain)}; {LC4.BytesToString(cipherCalc)}; {estimatedCollisionsTotal1.EstimatedTotal}; {estimatedCollisionsTotal1.NrOf0to5Possibilities}; {estimatedCollisionsTotal1.Avg5to10Possibilities}; {estimatedCollisionsTotal1.Avg10to15Possibilities}; {estimatedCollisionsTotal1.AvgCollisionsWith15Known}" });


                    chart1.Invoke(new Action(() => {
                        if (log2 < 0)
                        {
                            log2 = 0;
                        }
                        chart1.Series[0].Points.AddXY(i, log2);
                    }));

                    bestcomplexities.Add(estimatedCollisionsTotal1.EstimatedTotal);


                    label3.Invoke(new Action(() => {
                        label3.Text = Math.Log(bestcomplexities.Average(), 2).ToString();
                    }));
                    label4.Invoke(new Action(() => {
                        label4.Text = Math.Log(bestcomplexities.Median(), 2).ToString();
                    }));
                    label5.Invoke(new Action(() => {
                        label5.Text = Math.Log(bestcomplexities.StandardDeviation(), 2).ToString();
                    }));
                }
            }));
            t.Start();
        }

        bool encryptedOnesIND(byte[] ciphertext)
        {
            
            int first = Array.IndexOf(ciphertext, (byte)0);
            if (first != -1)
            {
                foreach(byte b in ciphertext.Skip(first))
                {
                    if(b != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void testIndCPA(int msgLength, int rounds)
        {
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            int successCounter = 0;
            for (int i = 0; i < rounds; i++)
            {
                var key = GetRandomKey();
                LC4 lc4 = new LC4(key, 0, 0);
                var one = new byte[msgLength];
                var zero = new byte[msgLength];
                for(int j=0; j < msgLength; j++)
                {
                    one[j] = 1; 
                    zero[j] = 0;
                }
                //choose message: 
                byte[] rbyte = new byte[msgLength];
                r.GetBytes(rbyte);
                bool encOne = rbyte[0] % 2 == 0;
                var ctext = encOne? lc4.Encrypt(one) : lc4.Decrypt(zero);

                //distinguish:
                if (encryptedOnesIND(ctext) == encOne)
                {
                    successCounter++;
                }

            }

            MessageBox.Show("successrate: " + successCounter + "/" + rounds);
        }

        private void button26_Click(object sender, EventArgs e)
        {
            testIndCPA((int)numericUpDown5.Value,100000);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            string sname = "Kollisionen";
            Series ser = new Series("Kollisionen");
            ser.ChartArea = "ChartArea1";
            ser.Name = sname;

            ser.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser);

            Series ser2 = new Series("avg");
            ser2.ChartArea = "ChartArea1";
            ser2.Name = "new";

            ser2.ChartType = SeriesChartType.Point;
            chart1.Series.Add(ser2);

            Thread t = new Thread(new ThreadStart(() => {
                List<double> bestcomplexities = new List<double>();
                string[] file = File.ReadAllLines("collEst-w.index.Selection.txt");
                int i = 0;
                foreach(string line in file)
                {
                    double estimatedCollisionsTotal1 = double.Parse(line.Split(';')[3].Trim());
                    double log2 = Math.Log(estimatedCollisionsTotal1, 2);
                    chart1.Invoke(new Action(() => {
                        if (log2 < 0)
                        {
                            log2 = 0;
                        }
                        chart1.Series[0].Points.AddXY(i, log2);

                    }));
                    i++;

                    bestcomplexities.Add(estimatedCollisionsTotal1);


                    label3.Invoke(new Action(() => {
                        label3.Text = Math.Log(bestcomplexities.Average(), 2).ToString();
                    }));
                    label4.Invoke(new Action(() => {
                        label4.Text = Math.Log(bestcomplexities.Median(), 2).ToString();
                    }));
                    label5.Invoke(new Action(() => {
                        label5.Text = Math.Log(bestcomplexities.StandardDeviation(), 2).ToString();
                    }));
                }
            }));
            t.Start();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            textBox5.Text = LC5.BytesToString(GetRandomKey()) + ";" + r.Next(0, 6) + ";" + r.Next(0, 6);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            string[] s = textBox5.Text.Split(';');
            byte[] key = LC5.StringToByteState(s[0]);
            byte i2 = (byte)int.Parse(s[1]);
            byte j2 = (byte)int.Parse(s[2]);

            LC5 lc5 = new LC5(key, 0, 0, i2, j2);
            byte[] clear = LC5.StringToByteState(textBox3.Text);
            byte[] cipher = lc5.Encrypt(clear);
            textBox4.Text = LC5.BytesToString(cipher);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            string[] s = textBox5.Text.Split(';');
            byte[] key = LC5.StringToByteState(s[0]);
            byte i2 = (byte)int.Parse(s[1]);
            byte j2 = (byte)int.Parse(s[2]);

            LC5 lc5 = new LC5(key, 0, 0, i2, j2);
            byte[] cipher = LC5.StringToByteState(textBox4.Text);
            byte[] plain = lc5.Decrypt(cipher);
            textBox3.Text = LC5.BytesToString(plain);
        }

        private List<byte> convert36to256Random(byte[] data)
        {
            List<byte> list = new List<byte>();
            for(int i=0;i<data.Length; i=i+2)
            {
                int d = data[i] * 36 + data[i + 1];
                if (d >= 1280)
                {
                    continue;
                }
                list.Add((byte)(d % 256));
            }
            return list;

        }


        private void button31_Click(object sender, EventArgs e)
        {
            string[] s = textBox5.Text.Split(';');
            byte[] key = LC5.StringToByteState(s[0]);
            byte i2 = (byte)int.Parse(s[1]);
            byte j2 = (byte)int.Parse(s[2]);
            LC5 lc5 = new LC5(key, 0, 0, i2, j2);

            using (var stream = new FileStream("output.bin", FileMode.Append))
            {
                for (int j = 0; j < 1; j++)
                {
                    byte[] data = new byte[1000000];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = 0;
                    }
                    byte[] cipher = lc5.Encrypt(data);
                    var converted = convert36to256Random(cipher).ToArray();
                    stream.Write(converted, 0, converted.Length);
                }
                    
            }

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button32_Click(object sender, EventArgs e)
        {
            testHypothesis();
        }

       

        private void button13_Click_1(object sender, EventArgs e)
        {

            
            
        }

        int loopAfterForRandom()
        {
            Random r = new Random();
            LC5 lc5 = new LC5(GetRandomKey(), 0, 0, (byte)r.Next(0, 6), (byte)r.Next(0, 6));
            List<byte> enc = new List<byte>();
            for (int j = 0; j < 100000000; j++)
            {
                byte c = lc5.SingleByteEncryption(0);
                enc.Add(c);
                if (j > 100)
                {
                    byte b = enc[enc.Count - 2];
                    if (enc[enc.Count - 3] == c &&
                        enc[enc.Count - 5] == c &&
                        enc[enc.Count - 7] == c &&
                        enc[enc.Count - 9] == c &&
                        enc[enc.Count - 11] == c &&
                        enc[enc.Count - 13] == c &&
                        enc[enc.Count - 15] == c &&
                        enc[enc.Count - 17] == c &&
                        enc[enc.Count - 19] == c &&
                        enc[enc.Count - 4] == b &&
                        enc[enc.Count - 6] == b &&
                        enc[enc.Count - 8] == b &&
                        enc[enc.Count - 10] == b &&
                        enc[enc.Count - 12] == b &&
                        enc[enc.Count - 14] == b &&
                        enc[enc.Count - 16] == b &&
                        enc[enc.Count - 18] == b &&
                        enc[enc.Count - 20] == b

                        )
                    {
                        return j;
                    }
                }
            }
            return -1;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            List<int> loopafter = new List<int>();
            for(int k = 0; k < 50; k++)
            {
                int i = loopAfterForRandom();
                if(i >= 0)
                {
                    loopafter.Add(i);
                }
               
            }
            MessageBox.Show(loopafter.Average().ToString());
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            textBox2.Text = LC5.BytesToString(GetRandomKey());
        }

        private void button19_Click(object sender, EventArgs e)
        {
            byte[] key = LC4.StringToByteState(textBox2.Text);
            LC4 lc4 = new LC4(key, 0, 0);
            byte[] clear = LC4.StringToByteState(textBox7.Text);
            byte[] cipher = lc4.Encrypt(clear);
            textBox6.Text = LC4.BytesToString(cipher);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            byte[] key = LC4.StringToByteState(textBox2.Text);
            LC4 lc4 = new LC4(key, 0, 0);
            byte[] cipher = LC4.StringToByteState(textBox6.Text);
            byte[] clear = lc4.Decrypt(cipher);
            textBox7.Text = LC4.BytesToString(clear);
        }
    }



    public static class EM
    {
        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }
    }
}
