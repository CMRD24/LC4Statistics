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

        public void ind0Attack()
        {
            int count = 0;
            List<int> zeros = new List<int>();
            List<int> same = new List<int>();
            for (int i = 0; i < 10000; i++)
            {
                byte[] key = GetRandomKey();
                byte[] data0 = get36Rand(2000);
                byte[] data1 = get36Rand(2000);
                /*RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] choose = new byte[1];
                randomNumberGenerator.GetBytes(choose);
                bool chosen0 = choose[0] % 2 == 0;*/

                


                LC4 lc4 = new LC4(key, 0, 0);
                byte[] ciphertext = lc4.Encrypt(data0);
                var c1 = distinguishNotSame(data1, ciphertext);
                var c2 = distinguishNotSame(data0, ciphertext);
                same.Add(c1.SameCount);
                zeros.Add(c1.CipherZeros);
                if (c2.IsNotSame)
                {
                    MessageBox.Show("something is wrong!");
                }
                if (c1.IsNotSame)
                {
                    count++;
                }
            }
            MessageBox.Show($"success: {count}"+Environment.NewLine+$"zeros (avg): {zeros.Average()}" + Environment.NewLine + $"same (avg): {same.Average()}");


        }

        public double indAttackProb(int messageLength, int repetitions)
        {
            int count = 0;
            List<int> zeros = new List<int>();
            List<int> same = new List<int>();
            for (int i = 0; i < repetitions; i++)
            {
                byte[] key = GetRandomKey();
                byte[] data0 = get36Rand(messageLength);
                byte[] data1 = get36Rand(messageLength);
                /*RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

                byte[] choose = new byte[1];
                randomNumberGenerator.GetBytes(choose);
                bool chosen0 = choose[0] % 2 == 0;*/




                LC4 lc4 = new LC4(key, 0, 0);
                byte[] ciphertext0 = lc4.Encrypt(data0);
                byte[] ciphertext1 = lc4.Encrypt(data1);
                var c1 = distinguishNotSame(data1, ciphertext0);
                var c2 = distinguishNotSame(data0, ciphertext1);
                var check1 = distinguishNotSame(data0, ciphertext0);
                var check2 = distinguishNotSame(data1, ciphertext1);
                same.Add(c1.SameCount);
                zeros.Add(c1.CipherZeros);
                if (check1.IsNotSame || check2.IsNotSame)
                {
                    MessageBox.Show("something is wrong!");
                }
                if (c1.IsNotSame || c2.IsNotSame)
                {
                    count++;
                }
            }

            double safeprobability = (double)count / (double)repetitions;
            double prob = safeprobability + 0.5 * (1 - safeprobability);
            return prob;
            //MessageBox.Show($"success: {count}" + Environment.NewLine + $"zeros (avg): {zeros.Average()}" + Environment.NewLine + $"same (avg): {same.Average()}");

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
            //int nonceLength = 10;
            int[] successfulPositionChange = new int[104];
            List<SDiff> lastStates = new List<SDiff>();
            //List<int> successfulPositionChange = new List<int>();
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

                byte[] nonce = get36Rand(10);// new byte[10];
                //randomNumberGenerator.GetBytes(nonce);
                //nonce = nonce.Select(x => (byte)(x % 36)).ToArray();

                byte[] data = get36Rand(dataLength);//new byte[dataLength];
                //randomNumberGenerator.GetBytes(data);
                //data = data.Select(x => (byte)(x % 36)).ToArray();

                byte[] mac = get36Rand(macLength);// new byte[macLength];
                //randomNumberGenerator.GetBytes(mac);
                //mac = mac.Select(x => (byte)(x % 36)).ToArray();

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
                byte[] last = lc4.GetNormalizedState();
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
                    successfulPositionChange[pos - 10]++;
                    lastStates.Add(new SDiff(pos-10, last, lc4.GetNormalizedState()));
                    information.Add("-----------------");
                    information.Add("-----------------");
                    File.AppendAllLines("false-auth.txt", information);
                    counter++;
                }
            }
            //MessageBox.Show(counter.ToString());
            //successfulPositionChange[20] = 1; 
            chart1.Invoke(new Action(() => { 
                for (int i = 0; i < 104; i++)
                {
                    if (successfulPositionChange[i] != 0)
                    {
                        chart1.Series[0].Points.AddXY(i, successfulPositionChange[i]);
                    }
                
                }
            }));

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
            }));

        }



        private void drawPropChart()
        {
            chart1.Series.Clear();
            Thread t = new Thread(new ThreadStart(() => {
            int rep = 100000;
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
                    int count = testProp(st, s, rep);
                        string l = $"{s}:{st}:{rep}:{count}";
                        label3.Invoke(new Action(() => {
                            label3.Text = l;
                        }));
                    File.AppendAllLines("pr1.txt", new string[] { l });
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
            Thread t = new Thread(new ThreadStart(() => { 
                simulation();
            }));
            t.Start();
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

            byte[] data = get36Rand(1000000);// new byte[1000000];
            //randomNumberGenerator.GetBytes(data);
            //data = data.Select(x => (byte)(x % 36)).ToArray();
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
        /// nur vom start bis endinex im jeweiligen chiffrat.
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

        private List<Term> removeTrueTerms(List<Term> terms, byte[][] possiblePlaintexts, int start)
        {

            for (int i = 0; i < possiblePlaintexts.Length - 1; i++)
            {
                int termIndex = i + start - 1;
                if (possiblePlaintexts[i].Length == 1)
                {
                    byte sure = possiblePlaintexts[i][0];
                    terms.RemoveAll(x => x.Index == termIndex && (x.Value1 != sure));
                    terms.RemoveAll(x => x.Index == termIndex-1 && (x.Value2 != sure));
                }
            }
            return terms;
        }

        private byte[][] removeAmbig(List<Term> terms, byte[][] possiblePlaintexts, int start)
        {
            for (int i = 0; i < possiblePlaintexts.Length; i++)
            {
                int termIndex = i + start - 1;
                if (possiblePlaintexts[i].Length > 1)
                {
                    byte[] next = new byte[0];
                    if (i < possiblePlaintexts.Length - 1)
                    {
                        next = possiblePlaintexts[i + 1]; //
                    }
                    byte[] prev = new byte[0];
                    if (i > 0) { prev = possiblePlaintexts[i - 1]; }
                    List<byte> newPoss = new List<byte>();
                    foreach (byte possible in possiblePlaintexts[i])
                    {
                        bool notFullfiled1 = terms.Any(x => x.Index == termIndex && x.Value1 == possible && next.Contains(x.Value2));
                        bool notFullfiled2 = terms.Any(x => x.Index == termIndex-1 && x.Value2 == possible && prev.Contains(x.Value2));
                        if (!notFullfiled1 && !notFullfiled2)
                        {
                            newPoss.Add(possible);
                        }
                    }
                    possiblePlaintexts[i] = newPoss.ToArray();
                }
            }
            return possiblePlaintexts;
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

            //check knf:
            List<Term> terms = build2KNF(chiffrate, start, end);
            //terms = removeTrueTerms(terms, possiblePlaintexts, start);
            //MessageBox.Show(terms.Count.ToString());
            possiblePlaintexts = removeAmbig(terms, possiblePlaintexts, start);


            byte[] firstGuess = possiblePlaintexts.Select(x => x.First()).ToArray();
            string guessedMessage = LC4.BytesToString(firstGuess);
            //MessageBox.Show(guessedMessage);
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
            byte[][] extracted = extractFromFixedPart(chiffrate, 100, 100 + fixmessage.Length);

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
                File.AppendAllLines("attack-msg.txt", new string[] { $"{k}: {guessedMessage}" });
                List<int> ambig = new List<int>();
                for (int i = 1; i < 20; i++)
                {
                    ambig.Add(extracted.Where(x => x.Length == i).Count());

                }
                l.Add(ambig.ToArray());

                File.AppendAllLines("attack-stat.txt", new string[] { $"{k}: {JsonConvert.SerializeObject(ambig)}" });
            }
            List<double> occProb = new List<double>();
            for(int i = 0; i < 19; i++)
            {
                double p = (double)l.Select(x => x[i]).Sum() / (double)l.Count;
                occProb.Add(p);
            }
            File.AppendAllLines("attack-stat2.txt", new string[] { $"occProb: {JsonConvert.SerializeObject(occProb)}" });

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
            //testHypothesis();
            byte[] key = new byte[] {4, 20, 23, 1, 5, 25, 6, 14, 7, 33, 9, 3, 11, 18, 12, 8, 22, 24, 35, 31, 13, 15, 19, 16, 0, 21, 10, 28, 26, 32, 17, 30, 34, 27, 29,2 };
            MessageBox.Show(key.Length.ToString());
            LC4 lc4 = new LC4(key, 0, 0);
            textBox2.Text += string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;
            lc4.SingleByteEncryption((byte)11);
            textBox2.Text += string.Join(",", lc4.State.Select(x => ((int)x).ToString()))+Environment.NewLine+lc4.I+","+lc4.J;
            
            lc4.SingleByteEncryption((byte)19);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;
           
            lc4.SingleByteEncryption((byte)35);
            textBox2.Text += Environment.NewLine+string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)7);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)8);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)5);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;
            
            lc4.SingleByteEncryption((byte)7);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)6);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)6);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)32);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)26);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)2);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)21);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)13);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)31);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;
            
            lc4.SingleByteEncryption((byte)10);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)4);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)27);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)26);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)22);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)12);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)35);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)11);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)25);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)31);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)23);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)4);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)24);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)19);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)25);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            lc4.SingleByteEncryption((byte)3);
            textBox2.Text += Environment.NewLine + string.Join(",", lc4.State.Select(x => ((int)x).ToString())) + Environment.NewLine + lc4.I + "," + lc4.J;

            //lc4.Reset();
            //byte[] c = lc4.Encrypt(new byte[] {11,19,35 });
            //MessageBox.Show(string.Join(",", c.Select(x => ((int)x).ToString())));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            sameMessafeAttack();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            sameMessafeAttackSim();
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

        private void button14_Click(object sender, EventArgs e)
        {
            byte[] key = new byte[36];// GetRandomKey();
            for(int i = 0; i < 36; i++)
            {
                key[i] = (byte)i;
            }

            key = GetRandomKey();
            
            
            textBox1.Text = string.Join(",", key.Select(x => (int)x));
            for (int i = 0; i < 1000000; i++)
            {
                LC4 lc4 = new LC4(key, 0, 0);
                byte[] data = get36Rand(20);
                byte[] enc = new byte[20];
                List<byte[]> bl = new List<byte[]>();
                for(int j = 0; j < 20; j++)
                {
                    enc[j] = lc4.SingleByteEncryption(data[j]);
                    bl.Add(lc4.GetNormalizedState());
                }
                bll.Add(bl);
                encs.Add(enc);
            }
            
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            int index = (int)numericUpDown4.Value;
            //int[] atIndexInState = bll[index].Select(x => x[0]).GroupBy(x => x).Select(x => x.Count()).ToArray();

            chart1.Series[0].Points.Clear();
            if (index < 1)
            {
                return;
            }
            for (int i = 0; i < 36; i++)
            {
                //int count = bll.Select(x => x[index]).Select(x => x[0]).Where(x => x == i).Count();
                int count = encs.Where(x => x[0]==0).Select(x => ((x[index] - x[0])+36)%36).Where(x => x==i).Count();
                chart1.Series[0].Points.AddXY(i, count);
            }

            /*int[] sums = new int[36];
            for(int i = 0; i < 36; i++)
            {
                sums[i] = 
            }*/
        }

        private void button15_Click(object sender, EventArgs e)
        {
            
        }

        private void button16_Click(object sender, EventArgs e)
        {
            drawPropChart();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[0].Enabled = !checkBox1.Checked;
            //chart1.Series[0].IsVisibleInLegend = false;
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

                    double p = indAttackProb(s, rep);

                    double m_0 = (((double)s-1) / 36);
                    double m_e = (((double)s-1-m_0) / 36);
                    double p_corr = 1 - Math.Pow(((double)(1 + 35 * 35) / (double)(36d * 36)), m_0) * Math.Pow((double)35 / (double)36, m_0);
                    double p_ges = 2 * (p_corr) * (1 - p_corr) + p_corr * p_corr;
                    double p_game = p_ges + 0.5 * (1 - p_ges);
                    /*MessageBox.Show(m_0.ToString());
                    MessageBox.Show(m_e.ToString());
                    MessageBox.Show(p_corr.ToString());
                    MessageBox.Show(p_ges.ToString());
                    MessageBox.Show(p_game.ToString());*/
                        chart1.Invoke(new Action(() => {
                            chart1.Series[sname].Points.AddXY(s, p);
                            chart1.Series["p*"].Points.AddXY(s, p_game);
                        }));

                }
            }));
            t.Start();
        }


        bool setStateField(byte[] state, int index, byte value)
        {
            if(index<0 || index > 35)
            {
                MessageBox.Show("???"+index);
            }
            if (state[index]!=255 && state[index] != value)
            {
                return false;
            }
            int i = Array.FindIndex(state, x => x==value);
            if (i != -1)
            {
                if (i != index)
                {
                    return false;
                }
                return true;
            }
            state[index] = value;
            return true;
        }

        public void knownPlaintextAttack()
        {
            byte[] plaintext = get36Rand(20000);
            byte[] key = GetRandomKey();
            LC4 lc4 = new LC4(key, 0, 0);
            byte[][] nstates = new byte[20000][];
            byte[] ciphertext = new byte[20000];

            for(int i = 0; i < plaintext.Length; i++)
            {
                nstates[i] = lc4.GetNormalizedState();
                ciphertext[i] = lc4.SingleByteEncryption(plaintext[i]);
            }

            byte[] recoveredIthKey = new byte[36];

            //recover key from ciphertext and plaintext only:
            
            for(int i = 0; i < plaintext.Length - 27; i++)
            {
                //rstate only thought for for state i+1
                if (plaintext[i] == ciphertext[i])
                {
                    //first assumption: S_0(0)=0
                    if (nstates[i][0] != 0)
                    {
                        MessageBox.Show("no!");
                    }
                    //second assumption: 0 at matrix position defined by ciphertext_i
                    int right = ciphertext[i] % 6;
                    int down = ciphertext[i] / 6;
                    int linearNullIndex = (12 - right)%6 + 6*((12 - down)%6);
                    bool assumption2 = nstates[i + 1][linearNullIndex] == 0;
                    if (!assumption2)
                    {
                        MessageBox.Show("no!2");
                        MessageBox.Show(linearNullIndex.ToString()+"::"+Array.FindIndex(nstates[i], x => x==0).ToString() + Environment.NewLine + Environment.NewLine 
                            + Array.FindIndex(nstates[i+1], x => x == 0).ToString() +
                            Environment.NewLine+Environment.NewLine+"plain: " + plaintext[i]+",cipher: " + ciphertext[i]);
                    
                    }

                    

                    //assumption 2 a: null trackable also in next states (only with probability):
                    /*int right2 = ciphertext[i+1] % 6;
                    int down2 = ciphertext[i+1] / 6;
                    int linearNullIndex2 = (6 - right) % 6 + 6 * ((6 - down) % 6);
                    bool assumption2a = nstates[i + 2][linearNullIndex2] == 0;
                    if (!assumption2)
                    {
                        MessageBox.Show("no!2");
                        MessageBox.Show(linearNullIndex.ToString() + "::" + Array.FindIndex(nstates[i], x => x == 0).ToString() + Environment.NewLine + Environment.NewLine
                            + Array.FindIndex(nstates[i + 1], x => x == 0).ToString() +
                            Environment.NewLine + Environment.NewLine + "plain: " + plaintext[i] + ",cipher: " + ciphertext[i]);

                    }*/


                    //assumption 3
                    if (plaintext[i+1]==0)
                    {
                        byte[] rstate = new byte[36];
                        for (int o = 0; o < 36; o++)
                        {
                            rstate[o] = 255;
                        }
                        rstate[linearNullIndex] = 0; //known for sure
                        int indexOfPlaintext = linearNullIndex;
                        //possible combinations:
                        //bool combinationSeen = false;
                        List<byte[]> plausibleStates = new List<byte[]>();
                        for(byte j = 1; j < 36;j++)
                        {
                            //use local copy of state:indexOfPlaintext
                            byte[] rstate_a = new byte[36];
                            Array.Copy(rstate, rstate_a, 36);

                            int S_0 = j;
                            int cipherPosition = (indexOfPlaintext + S_0) % 6 + (((indexOfPlaintext / 6 + S_0 / 6)) % 6) * 6;
                            /*if (rstate_a[0]!=255 || rstate_a[cipherPosition] != 255)
                            {
                                continue;
                            }
                            rstate_a[0] = j;
                            rstate_a[cipherPosition] = ciphertext[i + 1];*/
                            if (!setStateField(rstate_a, 0, j))
                            {
                                continue;
                            }
                            if (cipherPosition < 0)
                            {
                                MessageBox.Show("ciphpos");
                            }
                            if (!setStateField(rstate_a, cipherPosition, ciphertext[i + 1]))
                            {
                                continue;
                            }



                            //one of these 35 combinations

                            //S0 of i+2 has to be in field defined by ciphertext[i+1] (r/d)
                            //set this field to any value not yet given:
                            //this is a guess because we are looking ate the pre-encryption state of i+1, the post-encryption
                            //state is dependant of plain and cipher positions in i+1 (plain[i+1]=0 at indexOfPlaintext, 

                            int post_cipherPosCol = cipherPosition % 6;
                            if (indexOfPlaintext / 6 == cipherPosition / 6)
                            {
                                post_cipherPosCol = (post_cipherPosCol + 1) % 6;
                            }


                            int startindex = 0;
                            if (indexOfPlaintext / 6 == 0)
                            {
                                startindex = 1;
                            }
                            if(post_cipherPosCol == startindex)
                            {
                                startindex += 6;
                            }

                            int post_s0_i2_position = (ciphertext[i+1]+startindex) % 6 + ((ciphertext[i+1] / 6 + startindex/6)%6) * 6;


                            

                            


                            //-> take previous into account to make det. -1 or -7 or stays:
                            //plainrow = s0_i2_position row
                            int pre_s0_i2_position = post_s0_i2_position;
                            if (post_cipherPosCol == post_s0_i2_position % 6)
                            {
                                pre_s0_i2_position = pre_s0_i2_position % 6 + (((pre_s0_i2_position / 6) + 5) % 6) * 6;
                            }
                            if (indexOfPlaintext / 6 == (int)(pre_s0_i2_position / 6))
                            {
                                pre_s0_i2_position = (pre_s0_i2_position + 5)%6 + (pre_s0_i2_position / 6)*6;
                            }

                            if (pre_s0_i2_position < 0)
                            {
                                MessageBox.Show("no please no");
                            }




                            //soundness check:
                            if (rstate_a[0] == nstates[i + 1][0])
                            {
                                /*MessageBox.Show($"ciphertext[i+1]: {(int)ciphertext[i + 1]}" + Environment.NewLine +
                                $"calc.pos: {pre_s0_i2_position}" + Environment.NewLine +
                                $"s_i+1 at pos: {nstates[i + 1][pre_s0_i2_position]}" + Environment.NewLine +
                                $"s_i+2 at 0: {nstates[i + 2][0]}" + Environment.NewLine+
                                $"changed row/column?: {pre_s0_i2_position!=post_s0_i2_position}" + Environment.NewLine+
                                $"correct pos: {Array.FindIndex(nstates[i+1], x => x==nstates[i + 2][0])}" + Environment.NewLine+
                                $"plaintext pos: {Array.FindIndex(nstates[i + 1], x => x == plaintext[i+1])}" + Environment.NewLine+
                                $"encr pos: {Array.FindIndex(nstates[i + 1], x => x == ciphertext[i + 1])}" + Environment.NewLine
                                );*/

                                //soundness check:
                                bool sound = nstates[i + 1][pre_s0_i2_position] == nstates[i + 2][0];

                                if (!sound)
                                {
                                    //MessageBox.Show("unsound");
                                }
                                else
                                {
                                    //MessageBox.Show("sound");
                                }
                            }

                            



                            for (byte s0i2val = 0; s0i2val < 36; s0i2val++)
                            {
                                //copy to local state:
                                byte[] rstate_b = new byte[36];
                                Array.Copy(rstate_a, rstate_b, 36);

                                
                                //take look at plaintext[i+2] and ciphertext[i+2]
                                if(!setStateField(rstate_b, pre_s0_i2_position, s0i2val))
                                {
                                    continue;
                                }


                                if (Array.IndexOf(rstate_b, plaintext[i + 2])!=-1)
                                {
                                    //calculate position of plaintext:

                                    int from = Array.IndexOf(rstate_b, plaintext[i + 2]);
                                    int fromOrig = from;
                                    //correct by distortion from prev round:
                                    if (from / 6 == indexOfPlaintext / 6)
                                    {
                                        from = (from + 1) % 6 + (from / 6) * 6;
                                    }
                                    if (from % 6 == cipherPosition % 6)
                                    {
                                        from = (from + 6) % 36;
                                    }


                                    int ct_pos = (from + s0i2val) % 6 + (((from / 6 + s0i2val / 6)) % 6) * 6;

                                    //correct by distortion from prev round:
                                    if (ct_pos % 6 == cipherPosition % 6)
                                    {
                                        ct_pos = (ct_pos + 30) % 36;
                                    }
                                    if (ct_pos / 6 == indexOfPlaintext / 6)
                                    {
                                        ct_pos = (ct_pos + 5) % 6 + (ct_pos / 6) * 6;
                                    }




                                    if (ct_pos < 0)
                                    {
                                        MessageBox.Show("ct");
                                    }



                                    //sound?
                                    if (rstate_a[0] == nstates[i + 1][0] && rstate_b[pre_s0_i2_position] == nstates[i + 1][pre_s0_i2_position])
                                    {
                                        if(ct_pos!= Array.IndexOf(nstates[i + 1], ciphertext[i + 2]))
                                        MessageBox.Show(
                                            "adapted1:"+Environment.NewLine+
                                            $"plain index before: {indexOfPlaintext}" + Environment.NewLine +
                                            $"cipher index before: {cipherPosition}" + Environment.NewLine +
                                            $"plain index orig (orig): {fromOrig}" + Environment.NewLine +
                                            $"plain index (from): {from}" + Environment.NewLine +
                                            $"s0i2val: {s0i2val}" + Environment.NewLine +
                                            $"nstates[i+2][0]: {nstates[i + 2][0]}" + Environment.NewLine +
                                            $"correct index: {Array.IndexOf(nstates[i+1], ciphertext[i+2])}"+Environment.NewLine+
                                            $"calc. index: {ct_pos}" + Environment.NewLine
                                            );


                                        if (nstates[i + 1][ct_pos] != ciphertext[i + 2])
                                        {
                                            //MessageBox.Show("here!");
                                        }
                                        else
                                        {
                                            //MessageBox.Show("clear!");
                                        }
                                    }

                                    /*if (rstate_b[ct_pos] != 255 && rstate_b[ct_pos] != ciphertext[i + 2])
                                    {
                                        continue;
                                    }
                                    rstate_b[ct_pos] = ciphertext[i + 2];*/
                                    if(!setStateField(rstate_b, ct_pos, ciphertext[i + 2]))
                                    {
                                        continue;
                                    }
                                    //now 5 != 0 and 3 fixed
                                    plausibleStates.Add(rstate_b);
                                }
                                else if (Array.IndexOf(rstate_b, ciphertext[i + 2]) != -1)
                                {
                                    //calculate position of ciphertext:
                                    int from = Array.IndexOf(rstate_b, ciphertext[i + 2]);
                                    int fromOrig = from;
                                    //correct by distortion from prev round:
                                    if (from / 6 == indexOfPlaintext / 6)
                                    {
                                        from = (from + 1) % 6 + (from / 6) * 6;
                                    }
                                    //move cipherPosition too! (if on same row as plaintext pos)
                                    int movedCipherPos = cipherPosition;
                                    if (cipherPosition / 6 == indexOfPlaintext / 6)
                                    {
                                        movedCipherPos = (movedCipherPos + 1) % 6 + (movedCipherPos / 6) * 6;
                                    }
                                    if (from % 6 == movedCipherPos % 6)
                                    {
                                        from = (from + 6) % 36;
                                    }


                                    int pt_pos = (from - s0i2val+6) % 6 + (((from / 6 - s0i2val / 6)+6) % 6) * 6;
                                    //if ratio was distorted: (check!)

                                    //correct by distortion from prev round:
                                    if (pt_pos % 6 == cipherPosition % 6)
                                    {
                                        pt_pos = (pt_pos + 30) % 36;
                                    }
                                    if (pt_pos / 6 == indexOfPlaintext / 6)
                                    {
                                        pt_pos = (pt_pos + 5) % 6 + (pt_pos / 6) * 6;
                                    }
                                    
                                    



                                    if (rstate_a[0] == nstates[i + 1][0] && rstate_b[pre_s0_i2_position] == nstates[i + 1][pre_s0_i2_position])
                                    {
                                        if(pt_pos!= Array.IndexOf(nstates[i + 1], plaintext[i + 2]))
                                        MessageBox.Show(
                                            "adapted2"+Environment.NewLine+
                                            $"plain index before: {indexOfPlaintext}" + Environment.NewLine +
                                            $"cipher index before: {cipherPosition}" + Environment.NewLine +
                                            $"cipher orig index: {fromOrig}" + Environment.NewLine +
                                            $"cipher index: {from}" + Environment.NewLine +
                                            $"s0i2val: {s0i2val}" + Environment.NewLine +
                                            $"nstates[i+2][0]: {nstates[i + 2][0]}" + Environment.NewLine +
                                            $"correct index: {Array.IndexOf(nstates[i + 1], plaintext[i + 2])}" + Environment.NewLine +
                                            $"calc. index: {pt_pos}" + Environment.NewLine
                                            );


                                        if (nstates[i + 1][pt_pos] != plaintext[i + 2])
                                        {
                                            //MessageBox.Show("here!");
                                        }
                                        else
                                        {
                                            //MessageBox.Show("clear!");
                                        }
                                    }


                                    //MessageBox.Show("!!!" + ct_pos);
                                    /*if (rstate_b[ct_pos]!=255 && rstate_b[ct_pos] != ciphertext[i + 2])
                                    {
                                        continue;
                                    }
                                    rstate_b[ct_pos] = ciphertext[i + 2];*/
                                    if (!setStateField(rstate_b, pt_pos, plaintext[i + 2]))
                                    {
                                        continue;
                                    }
                                    //now 5 != 0 and 3 fixed
                                    plausibleStates.Add(rstate_b);
                                }
                                else
                                {

                                    //go throught possible positions in rstate_b to set to plaintext[i+2]
                                    for(int pt_pos=0; pt_pos<36;pt_pos++)
                                    {
                                        byte[] rstate_c = new byte[36];
                                        Array.Copy(rstate_b, rstate_c, 36);
                                        int from = pt_pos;
                                        if (from / 6 == indexOfPlaintext / 6)
                                        {
                                            from = (from + 1) % 6 + (from / 6) * 6;
                                        }
                                        if (from % 6 == cipherPosition % 6)
                                        {
                                            from = (from + 6) % 36;
                                        }

                                        //rstate_c[r] = plaintext[i+2];
                                        if (!setStateField(rstate_b, from, plaintext[i + 2]))
                                        {
                                            continue;
                                        }
                                        int ct_pos = (from + s0i2val) % 6 + (((from / 6 + s0i2val / 6)) % 6) * 6;

                                        //correct by offsets of prev round
                                        if (ct_pos % 6 == cipherPosition % 6)
                                        {
                                            ct_pos = (ct_pos + 30) % 36;
                                        }
                                        if (ct_pos / 6 == indexOfPlaintext / 6)
                                        {
                                            ct_pos = (ct_pos + 5) % 6 + (ct_pos / 6) * 6;
                                        }

                                        if (!setStateField(rstate_b, ct_pos, ciphertext[i + 2]))
                                        {
                                            continue;
                                        }

                                        //now 6 !=0 and 3 fixed

                                        //todo check if correct one is always among seen states
                                        plausibleStates.Add(rstate_c);
                                    }

                                }


                            }
                            
                            
                            //soundness check


                            /*
                            if (nstates[i + 1][cipherPosition] == ciphertext[i+1] && nstates[i + 1][0] == S_0)
                            {
                                combinationSeen = true;
                                //check further assumptions:
                                //S0 of i+2 has to be in field defined by ciphertext[i+1] (r/d)
                                //set this field to any value not yet given:
                                for(byte k = 1; k < 36; k++)
                                {
                                    if(S_0==k || k == ciphertext[i + 1])
                                    {
                                        continue;
                                    }
                                    int s0position = ciphertext[i+1] % 6 + (ciphertext[i+1] / 6) * 6;
                                    //assert nstates[i + 1][s0position] == k;
                                    //calculate the possible relative positions:
                                    if (ciphertext[i + 2] == k)
                                    {

                                    }
                                    else if (plaintext[i + 2] == k)
                                    {

                                    }
                                    else if (ciphertext[i + 2] == 0)
                                    {

                                    }
                                    else if (plaintext[i + 2] == 0)
                                    {

                                    }
                                    else if (ciphertext[i + 2] == ciphertext[i+1])
                                    {

                                    }
                                    else if (plaintext[i + 2] == ciphertext[i+1])
                                    {

                                    }
                                    else
                                    {
                                        //assert position of plaintext[i+2]
                                        //for...
                                    }
                                }


                            }
                            */
                            //continue with three known 
                        }

                        

                        //soundness:
                        bool containsCorrect = false;
                        //MessageBox.Show(plausibleStates.Count.ToString());
                        foreach (byte[] plausible in plausibleStates)
                        {
                            bool isCorrect = true;
                            for(int k = 0; k < 36; k++)
                            {
                                //MessageBox.Show(((int)plausible[k]).ToString());
                                if (plausible[k]!=255 && plausible[k] != nstates[i + 1][k])
                                {
                                    isCorrect = false;
                                }
                            }
                            if (isCorrect)
                            {
                                containsCorrect = true;
                                
                            }
                            
                        }
                        if (containsCorrect)
                        {
                            //MessageBox.Show("yes");
                        }
                        else
                        {
                            //File.AppendAllLines("kpa.txt", new string[] {"____________", "____________", LC4.BytesToString(nstates[i + 1]), "-----------------" });
                            //File.AppendAllLines("kpa.txt", plausibleStates.Select(x => LC4.BytesToString(x)));
                            //MessageBox.Show("nah");
                        }
                        /*
                        if (!combinationSeen)
                        {
                            MessageBox.Show("false");
                        }
                        else
                        {
                            MessageBox.Show("true");
                        }*/

                    }
                    else if(ciphertext[i + 1] == 0)
                    {
                        //...

                    }
                }
            }

            


        }


        private byte getIndex(byte from, byte s0)
        {
            return (byte)((from + s0) % 6 + ((from / 6 + s0 / 6) % 6) * 6);
        }

        private byte getIndexReversed(byte from, byte s0)
        {
            return (byte)((from - s0 + 36) % 6 + ((from / 6 - s0 / 6 + 6) % 6) * 6);
        }

        private byte getS0FromIndexes(byte from, byte to)
        {
            //todo
            int y_offset = ((to / 6) - (from / 6) + 6) % 6;
            int x_offset = ((to % 6) - (from % 6) + 6) % 6;
            return (byte)(y_offset * 6 + x_offset);
        }

        private byte getIndex(byte from, byte right, byte down)
        {
            return (byte)((from + right) % 6 + ((from / 6 + down) % 6) * 6);
        }

        private byte[] rotateRowRight(byte[] state, byte row)
        {
            byte[] result = new byte[36];
            Array.Copy(state, result, 36);
            for (byte i = 0; i < 6; i++)
            {
                result[6 * row + i] = state[6 * row + ((i + 5) % 6)];

            }
            return result;
        }

        private byte[] rotateColumnDown(byte[] state, byte column)
        {
            byte[] result = new byte[36];
            Array.Copy(state, result, 36);
            for (byte i = 0; i < 6; i++)
            {
                result[6 * i + column] = state[mod(6 * i + column - 6, 36)];

            }
            return result;
        }

        private byte[] s0offset(byte[] oldState, byte down, byte right)
        {
            byte[] state = new byte[36];
            for(byte i = 0; i < 36; i++)
            {
                state[i] = oldState[getIndex(i, right, down)];
            }
            return state;
        }

        /// <summary>
        /// value at cipherindex has to be set!!!
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="plaintextIndex"></param>
        /// <param name="cipherIndex"></param>
        /// <returns></returns>
        private byte[] newState(byte[] oldState, byte plaintextIndex, byte cipherIndex)
        {
            byte[] s1 = rotateRowRight(oldState, (byte)(plaintextIndex / 6));

            

            int columnToRotate = cipherIndex % 6;
            if (cipherIndex / 6 == plaintextIndex / 6)
            {
                //moved
                columnToRotate = (columnToRotate + 1) % 6;
            }


            byte[] s2 = rotateColumnDown(s1, (byte)columnToRotate);



            //if s0 moved by rotations:
            int additionalXOffset = 0;
            int additionalYOffset = 0;
            

            if (plaintextIndex / 6 == 0 && ((cipherIndex/6!=0 && cipherIndex % 6 == 1) || (cipherIndex/6==0&&cipherIndex%6==0)))
            {
                additionalXOffset = 1;
                additionalYOffset = 1;
            }
            else if (plaintextIndex / 6 == 0)
            {
                additionalXOffset = 1;
            }
            else if (columnToRotate % 6 == 0)
            {
                additionalYOffset = 1;
            }

            return s0offset(s2, (byte)(((oldState[cipherIndex] / 6)+additionalYOffset)%6), (byte)(((oldState[cipherIndex] % 6)+additionalXOffset)%6));
        }


        private List<byte[]> calculateKeyPossibilities(byte[] knownState, IEnumerable<byte> plaintext, IEnumerable<byte> ciphertext, int depth, byte plaintextIndex = 255, byte ciphertextIndex = 255)
        {
            List<byte[]> possibilities = new List<byte[]>();
            if (!knownState.Contains((byte)255))
            {
                possibilities.Add(knownState);
                return possibilities;

            }
            /*var unknownFields = EM.FindAllIndexof(knownState, (byte)255);
            if (unknownFields.Length == 1)
            {
                //which value is missing?
                for(byte i = 0; i < 36; i++)
                {
                    if (!knownState.Contains(i))
                    {
                        knownState[unknownFields[0]] = i;
                        return Tuple.Create(knownState, advancement);
                    }
                }
            }*/

            //temp value
            if (depth == 0)
            {
                possibilities.Add(knownState);
                return possibilities;
            }

            if (plaintextIndex == (byte)255)
            {
                plaintextIndex = (byte)Array.IndexOf(knownState, plaintext.First());
            }

            if (ciphertextIndex == (byte)255)
            {
                ciphertextIndex = (byte)Array.IndexOf(knownState, ciphertext.First());
            }

            //MessageBox.Show(plaintextIndex + "->" + ciphertextIndex);


            if (plaintextIndex != 255)
            {
                if (knownState[0] != 255)
                {
                    byte cipherIndex = getIndex((byte)plaintextIndex, knownState[0]);
                    //MessageBox.Show("calc.c.i.: " + cipherIndex);
                    if (ciphertextIndex != 255 && ciphertextIndex != cipherIndex)
                    {
                        //MessageBox.Show("f1");
                        return null;
                    }
                    if (setStateField(knownState, cipherIndex, ciphertext.First()))
                    {
                        //calculate new state:
                        //MessageBox.Show("cont1");
                        byte[] nextState = newState(knownState, plaintextIndex, cipherIndex);
                        return calculateKeyPossibilities(nextState, plaintext.Skip(1), ciphertext.Skip(1), depth - 1);
                    }
                    else
                    {
                        //MessageBox.Show("f2");
                        return null;
                    }
                }
                else if (ciphertextIndex != 255)
                {
                    byte calculatedS0 = getS0FromIndexes(plaintextIndex, ciphertextIndex);
                    if (setStateField(knownState, 0, calculatedS0))
                    {
                        //calculate new state:
                        byte[] nextState = newState(knownState, plaintextIndex, ciphertextIndex);
                        return calculateKeyPossibilities(nextState, plaintext.Skip(1), ciphertext.Skip(1), depth - 1);
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    //MessageBox.Show("guess s0");
                    //guess s0
                    for (byte i = 0; i < 36; i++)
                    {
                        if (knownState.Contains(i))
                        {
                            continue;
                        }
                        byte[] guessedState = new byte[36];
                        Array.Copy(knownState, guessedState, 36);
                        guessedState[0] = i;
                        var calcState = calculateKeyPossibilities(guessedState, plaintext, ciphertext, depth, plaintextIndex);
                        if (calcState != null)
                        {
                            possibilities.AddRange(calcState);
                        }
                    }
                    return possibilities;
                }
            }
            else if (ciphertextIndex != 255)
            {
                if (knownState[0] != 255)
                {
                    byte plainIndex = getIndexReversed((byte)ciphertextIndex, knownState[0]);

                    if (setStateField(knownState, plainIndex, plaintext.First()))
                    {
                        //calculate new state:
                        byte[] nextState = newState(knownState, plainIndex, ciphertextIndex);
                        return calculateKeyPossibilities(nextState, plaintext.Skip(1), ciphertext.Skip(1), depth - 1);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    //todo (only ciphertextIndex known)
                    //guess s0:
                    //MessageBox.Show("guess s0");
                    for (byte i = 0; i < 36; i++)
                    {
                        if (knownState.Contains(i))
                        {
                            continue;
                        }
                        byte[] guessedState = new byte[36];
                        Array.Copy(knownState, guessedState, 36);
                        guessedState[0] = i;
                        var calcState = calculateKeyPossibilities(guessedState, plaintext, ciphertext, depth, 255, ciphertextIndex);
                        if (calcState != null)
                        {
                            possibilities.AddRange(calcState);
                        }

                    }
                    return possibilities;
                }
            }
            else
            {
                //MessageBox.Show("guess index of plaintext");
                //nothing known
                //guess index of plaintext:
                foreach (byte p_index in EM.FindAllIndexof(knownState, (byte)255))
                {
                    byte[] guessedState = new byte[36];
                    Array.Copy(knownState, guessedState, 36);
                    guessedState[p_index] = plaintext.First();
                    var calcState = calculateKeyPossibilities(guessedState, plaintext, ciphertext, depth, p_index, 255);
                    if (calcState != null)
                    {
                        possibilities.AddRange(calcState);
                    }
                }
                //MessageBox.Show("r3:"+possibilities.Count);
                return possibilities;
            }


        }


        private Tuple<byte[], int> calculateKey(byte[] knownState, IEnumerable<byte> plaintext, IEnumerable<byte> ciphertext, int advancement, byte plaintextIndex=255, byte ciphertextIndex=255)
        {
            if(!knownState.Contains((byte)255))
            {

                return Tuple.Create(knownState, advancement);

            }
            

            if (plaintextIndex == (byte)255)
            {
                plaintextIndex = (byte)Array.IndexOf(knownState, plaintext.First());
            }

            if (ciphertextIndex == (byte)255)
            {
                ciphertextIndex = (byte)Array.IndexOf(knownState, ciphertext.First());
            }

            //MessageBox.Show(plaintextIndex + "->" + ciphertextIndex);


            if (plaintextIndex != 255)
            {
                if (knownState[0] != 255)
                {
                    byte cipherIndex = getIndex((byte)plaintextIndex, knownState[0]);
                    //MessageBox.Show("calc.c.i.: " + cipherIndex);
                    if (ciphertextIndex !=255 && ciphertextIndex != cipherIndex)
                    {
                        //MessageBox.Show("f1");
                        return null;
                    }
                    if(setStateField(knownState, cipherIndex, ciphertext.First()))
                    {
                        //calculate new state:
                        //MessageBox.Show("cont1");
                        byte[] nextState = newState(knownState, plaintextIndex, cipherIndex);
                        return calculateKey(nextState, plaintext.Skip(1), ciphertext.Skip(1), advancement+1);
                    }
                    else
                    {
                        //MessageBox.Show("f2");
                        return null;
                    }
                }
                else if (ciphertextIndex != 255)
                {
                    byte calculatedS0 = getS0FromIndexes(plaintextIndex, ciphertextIndex);
                    if (setStateField(knownState, 0, calculatedS0))
                    {
                        //calculate new state:
                        byte[] nextState = newState(knownState, plaintextIndex, ciphertextIndex);
                        return calculateKey(nextState, plaintext.Skip(1), ciphertext.Skip(1), advancement+1);
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    //MessageBox.Show("guess s0");
                    //guess s0
                    for(byte i = 0; i < 36; i++)
                    {
                        if (knownState.Contains(i))
                        {
                            continue;
                        }
                        byte[] guessedState = new byte[36];
                        Array.Copy(knownState, guessedState, 36);
                        guessedState[0] = i;
                        var calcState = calculateKey(guessedState, plaintext, ciphertext, advancement, plaintextIndex);
                        if (calcState != null)
                        {
                            return calcState;
                        }
                    }
                    return null;
                }
            }
            else if (ciphertextIndex != 255)
            {
                if (knownState[0] != 255)
                {
                    byte plainIndex = getIndexReversed((byte)ciphertextIndex, knownState[0]);

                    if (setStateField(knownState, plainIndex, plaintext.First()))
                    {
                        //calculate new state:
                        byte[] nextState = newState(knownState, plainIndex, ciphertextIndex);
                        return calculateKey(nextState, plaintext.Skip(1), ciphertext.Skip(1), advancement + 1);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    //todo (only ciphertextIndex known)
                    //guess s0:
                    //MessageBox.Show("guess s0");
                    for (byte i = 0; i < 36; i++)
                    {
                        if (knownState.Contains(i))
                        {
                            continue;
                        }
                        byte[] guessedState = new byte[36];
                        Array.Copy(knownState, guessedState, 36);
                        guessedState[0] = i;
                        var calcState = calculateKey(guessedState, plaintext, ciphertext, advancement, 255, ciphertextIndex);
                        if (calcState != null)
                        {
                            return calcState;
                        }
                        
                    }
                    return null;
                }
            }
            else
            {
                //MessageBox.Show("guess index of plaintext");
                //nothing known
                //guess index of plaintext:
                foreach(byte p_index in EM.FindAllIndexof(knownState, (byte)255))
                {
                    byte[] guessedState = new byte[36];
                    Array.Copy(knownState, guessedState, 36);
                    guessedState[p_index] = plaintext.First();
                    var calcState = calculateKey(guessedState, plaintext, ciphertext, advancement, p_index, 255);
                    if (calcState != null)
                    {
                        return calcState;
                    }
                }
                return null;
            }

            
        }


        private void button18_Click(object sender, EventArgs e)
        {
            knownPlaintextAttack();
        }

        private string keyToSquareString(byte[] key)
        {
            string str = LC4.BytesToString(key);
            return str.Substring(0, 6) + Environment.NewLine +
                str.Substring(6, 6) + Environment.NewLine +
                str.Substring(12, 6) + Environment.NewLine +
                str.Substring(18, 6) + Environment.NewLine +
                str.Substring(24, 6) + Environment.NewLine +
                str.Substring(30, 6) + Environment.NewLine;
        }

        bool containsKey(byte[] pattern, byte[] key)
        {
            for(int i = 0; i < 36; i++)
            {
                if(pattern[i]!=255 && pattern[i] != key[i])
                {
                    return false;
                }
            }
            return true;
        }

        private int getBestIndexForBacktracking(byte[] knownState, byte[] plain, byte[] cipher)
        {
            int firstBestIndex = int.MaxValue;
            //int first
            for(int i = 0; i < plain.Length-100; i++)
            {
                if (plain[i] == cipher[i])
                {
                    return i;
                }
            }
            return 0;

        }

        byte[] removeNofKey(byte[] key, int n)
        {
            for (int i = 35; i > 35-n; i--)
            {
                key[i] = 255;
            }
            return key;
        }

        bool isSameState(byte[] state1, byte[] state2)
        {
            for(int i = 0; i < 36; i++)
            {
                if (state1[i] != state2[i])
                {
                    return false;
                }
            }
            return true;
        }


        private void testStateFunction()
        {
            for(int i = 0; i < 10000; i++)
            {
                byte[] key = GetRandomKey();
                LC4 lc4 = new LC4(key, 0, 0);
                byte[] data = get36Rand(5);
                byte c = lc4.SingleByteEncryption(data[0]);
                byte[] supposed = lc4.GetNormalizedState();
                byte pIndex = (byte)Array.IndexOf(key, data[0]);
                byte cIndex = (byte)Array.IndexOf(key, c);
                byte[] isState = newState(key, pIndex, cIndex);
                if (!isSameState(isState, supposed))
                {
                    MessageBox.Show($"##pl: {LC4.ByteToChar(data[0])}, ci: {LC4.ByteToChar(c)}" + Environment.NewLine + Environment.NewLine + keyToSquareString(key) + Environment.NewLine + "_________is:" + Environment.NewLine + keyToSquareString(isState) + Environment.NewLine + "_________supp" + Environment.NewLine + keyToSquareString(supposed));

                }
                
            }
            MessageBox.Show("passed");
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

        private void button19_Click(object sender, EventArgs e)
        {
            //test state function:
            //testStateFunction();
            int msgLength = 20000;
            int numberOfUnknowns = 33;

            //generate key and plain+ciphertext:
            byte[] key = GetRandomKey();
            //File.AppendAllLines("tv.txt", new string[] { string.Join(",", key.Select(x => x.ToString())) });
            byte[] coppiedKey = new byte[36];
            Array.Copy(key, coppiedKey, 36);
            byte[] plain = get36Rand(msgLength);
            byte[][] states = new byte[msgLength][];
            byte[] cipher = new byte[msgLength];
            LC4 lc4a = new LC4(key, 0, 0);

            for(int i = 0; i < msgLength; i++)
            {
                states[i] = lc4a.GetNormalizedState();
                cipher[i] = lc4a.SingleByteEncryption(plain[i]);
                
            }




            int pli = Array.IndexOf(coppiedKey, plain[0]);
            int cli = Array.IndexOf(coppiedKey, cipher[0]);
            byte[] isState = newState(coppiedKey, (byte)pli, (byte)cli);


            //MessageBox.Show(keyToSquareString(states[1]) + Environment.NewLine + Environment.NewLine + keyToSquareString(isState));

            //MessageBox.Show($"pl: {LC4.ByteToChar(plain[0])}, ci: {LC4.ByteToChar(cipher[0])}" + Environment.NewLine + Environment.NewLine + keyToSquareString(key) + Environment.NewLine + "_________is:" + Environment.NewLine + keyToSquareString(isState) + Environment.NewLine + "_________supp" + Environment.NewLine + keyToSquareString(states[1]));



            //delete n parts of key:
            //removeNofKey(key, 27);
            

            //recover key:

            //get promising index:
            
            List<Tuple<int, int>> prIndexes = new List<Tuple<int, int>>();
            for(int i = 0; i < msgLength - 100; i++)
            {
                List<byte> pool = new List<byte>();
                var pl = plain.Skip(i).Take(10).ToArray();
                var ci = cipher.Skip(i).Take(10).ToArray();
                int score = 0;
                if (pl[0] == ci[0])
                {
                    score++;
                    if (pl[1]==0 || ci[1] == 0)
                    {
                        score++;
                        var z = pl.Take(2).Concat(ci.Take(2));
                        if(containsAny(z, new byte[] { pl[2], ci[2] }))
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
                                }
                            }
                        }
                    }
                }
                pool.AddRange(pl);
                pool.AddRange(ci);
                int msgD = pool.Distinct().Count();
                score += 20 - msgD;
                prIndexes.Add(Tuple.Create(i, score));
            }

            var sortedIndexes = prIndexes.OrderByDescending(x => x.Item2).Take(1000);
            MessageBox.Show("!"+sortedIndexes.First().Item2);
            List<Tuple<int, int>> prIndexes2 = new List<Tuple<int, int>>();
            for (int i = 0; i < 1000; i++)
            {
                int pindex = sortedIndexes.ElementAt(i).Item1;
                byte[] ck = new byte[36]; 
                Array.Copy(states[pindex], ck, 36);
                //dont forget to change later
                ck = removeNofKey(ck, numberOfUnknowns);
                byte[] cpl = plain.Skip(pindex).ToArray();
                byte[] cci = cipher.Skip(pindex).ToArray();
                int possibilitiesAtIndex = calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 2).Count;
                prIndexes2.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                //MessageBox.Show(pindex + ":" + possibilitiesAtIndex);
                
            }
            var sortedIndexes2 = prIndexes2.OrderBy(x => x.Item2).Take(100);
            MessageBox.Show("!" + sortedIndexes2.First().Item2);
            List<Tuple<int, int>> prIndexes3 = new List<Tuple<int, int>>();
            for (int i = 0; i < 100; i++)
            {
                int pindex = sortedIndexes2.ElementAt(i).Item1;
                byte[] ck = new byte[36];
                Array.Copy(states[pindex], ck, 36);
                //dont forget to change later
                ck = removeNofKey(ck, numberOfUnknowns);
                byte[] cpl = plain.Skip(pindex).ToArray();
                byte[] cci = cipher.Skip(pindex).ToArray();
                int possibilitiesAtIndex = calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 3).Count;
                prIndexes3.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                //MessageBox.Show(pindex + ":" + possibilitiesAtIndex);

            }

            var sortedIndexes3 = prIndexes3.OrderBy(x => x.Item2).Take(10);
            MessageBox.Show("!" + sortedIndexes3.First().Item2);
            List<Tuple<int, int>> prIndexes4 = new List<Tuple<int, int>>();
            for (int i = 0; i < 10; i++)
            {
                int pindex = sortedIndexes3.ElementAt(i).Item1;
                byte[] ck = new byte[36];
                Array.Copy(states[pindex], ck, 36);
                //dont forget to change later
                ck = removeNofKey(ck, numberOfUnknowns);
                byte[] cpl = plain.Skip(pindex).ToArray();
                byte[] cci = cipher.Skip(pindex).ToArray();
                int possibilitiesAtIndex = calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 5).Count;
                prIndexes4.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                MessageBox.Show(pindex + ":" + possibilitiesAtIndex);

            }
            var sortedIndexes4 = prIndexes4.OrderBy(x => x.Item2).Take(10);
            int bestIndex = sortedIndexes4.First().Item1;
            int bestValue = sortedIndexes4.First().Item2;

            MessageBox.Show($"bestI: {bestIndex}; bestValue: {bestValue}");

            int index = bestIndex;// getBestIndexForBacktracking(key, plain, cipher);

            byte[] chosenKey = states[index];
            chosenKey = removeNofKey(chosenKey, numberOfUnknowns);
            byte[] chosenPlain = plain.Skip(index).ToArray();
            byte[] chosenCipher = cipher.Skip(index).ToArray();

            MessageBox.Show(LC4.BytesToString(chosenPlain.Take(10).ToArray()) + Environment.NewLine + LC4.BytesToString(chosenCipher.Take(10).ToArray()));

            int d = 5;
            var test = calculateKeyPossibilities(chosenKey, chosenPlain.Skip(0), chosenCipher.Skip(0), d);
            MessageBox.Show(test.Count.ToString());
            /*bool contains = false;
            foreach (byte[] pattern in test)
            {
                if (containsKey(pattern, states[d]))
                {
                    contains = true;
                }
            }
            MessageBox.Show(test.Count.ToString() + $": {contains}");*/


           

            var recovered = calculateKey(chosenKey, chosenPlain.Skip(0), chosenCipher.Skip(0), 0);

            if (recovered == null)
            {
                MessageBox.Show("fail!");
                return;
            }

            MessageBox.Show($"{LC4.BytesToString(states[recovered.Item2+index])}   (key ({recovered.Item2}))" + Environment.NewLine +
                $"{LC4.BytesToString(recovered.Item1)}   (rec)");

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
