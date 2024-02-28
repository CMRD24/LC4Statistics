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
using Accord.Statistics.Testing;
using LC4Statistics.IND;

namespace LC4Statistics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
           

        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] arr = Util.GetRandomKey();
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



        

        

        



        

        public double entropy<T>(T[] data)
        {
            int[] count = data.GroupBy(x => x).Select(x => x.Count()).ToArray();
            var p = count.Select(x => x / (double)data.Length);
            return -p.Select(x => x * Math.Log(x, 2)).Sum();
        }


        private void button4_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(() => { 
               AuthenticationTests.simulation(label1, label2, chart1);
            }));
            t.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AuthenticationTests.booksimulation();
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
            
            byte[] data = Util.Get36Rand(100000);
            LC4 lc4 = new LC4(Util.GetRandomKey(), 0, 0);
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
            byte[] arr = Util.GetRandomKey();
            LC4 lc4 = new LC4(arr, 0, 0);
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

            byte[] data = Util.Get36Rand(1000000);
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









        private void button8_Click(object sender, EventArgs e)
        {
            byte[] data = { (byte)numericUpDown2.Value, (byte)numericUpDown3.Value };
            List<byte[]> encList = new List<byte[]>();
            for (int k = 0; k < 1000000; k++)
            {
                LC4 lc4 = new LC4(Util.GetRandomKey(), 0, 0);
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


        private void button11_Click(object sender, EventArgs e)
        {
            BroadcastAttackTest.sameMessageAttackSim();
        }

        private void button12_Click(object sender, EventArgs e)
        {
           
        }

        private void button13_Click(object sender, EventArgs e)
        {
            byte[] b = Util.GetRandomKey();
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


        private void button16_Click(object sender, EventArgs e)
        {
           LC5Tests.drawPropChart(chart1, label3);
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

                    double p = INDGames.randomIndAttackProb(s, rep);

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





//_____________________________Key recovery:_____________________________________________

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
                    var key0 = Util.GetRandomKey();
                    LC4 lc4a = new LC4(key0, 0, 0);
                    int msgLength = 500000;
                    byte[] data0 = Util.Get36Rand(msgLength);
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
                var key0 = Util.GetRandomKey();
                LC4 lc4a = new LC4(key0, 0, 0);
                int msgLength = 500;
                byte[] data0 = Util.Get36Rand(msgLength);
                                                    
                
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
                    var key0 = Util.GetRandomKey();
                    LC4 lc4a = new LC4(key0, 0, 0);
                    int msgLength = 200;
                    byte[] data0 = Util.Get36Rand(msgLength);


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
                    byte[] plain = Util.Get36Rand(msgLength);
                    byte[][] states = new byte[msgLength][];
                    byte[] cipherCalc = new byte[msgLength];

                    byte[] key = Util.GetRandomKey();
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

        

        

        private void button26_Click(object sender, EventArgs e)
        {

           int successCounter = INDGames.testIndCPA((int)numericUpDown5.Value,100000);
           MessageBox.Show("successrate: " + successCounter + "/" + 100000);
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
            textBox5.Text = LC5.BytesToString(Util.GetRandomKey()) + ";" + r.Next(0, 6) + ";" + r.Next(0, 6);
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

        


        private void button31_Click(object sender, EventArgs e)
        {
            string[] s = textBox5.Text.Split(';');
            byte[] key = LC5.StringToByteState(s[0]);
            byte i2 = (byte)int.Parse(s[1]);
            byte j2 = (byte)int.Parse(s[2]);
            LC5 lc5 = new LC5(key, 0, 0, i2, j2);

            using (var stream = new FileStream("output.bin", FileMode.Append))
            {
                //
                for (int j = 0; j < 4*100; j++)
                {
                    byte[] data = new byte[100000000];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = 0;
                    }
                    byte[] cipher = lc5.Encrypt(data);
                    var converted = Util.Convert36to256RandomBy2(cipher).ToArray();//convert36toWordRandom(cipher).ToArray();//convert36to256Random(cipher).ToArray();
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



        private void button14_Click(object sender, EventArgs e)
        {
            string[] s = textBox5.Text.Split(';');
            byte[] key = LC5.StringToByteState(s[0]);
            byte i2 = (byte)int.Parse(s[1]);
            byte j2 = (byte)int.Parse(s[2]);
            LC5Tests.dab_bytedistribLC5(key, i2, j2);
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            textBox2.Text = LC5.BytesToString(Util.GetRandomKey());
            MessageBox.Show(string.Join(",", Util.GetRandomKey()));
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

        private void button6_Click(object sender, EventArgs e)
        {
            int[] occurences = new int[36];
            for (int i = 0; i < 100; i++)
            {
                Random r = new Random();
                LC5 lc5 = new LC5(Util.GetRandomKey(), (byte)r.Next(0, 6), (byte)r.Next(0, 6), 0, 0);
                
                for (int j = 0; j < 10000000; j++)
                {
                    occurences[lc5.SingleByteEncryption(0)]++;
                }
                
            }
            File.WriteAllLines("bytedistrib.txt", occurences.Select((x, c) => c.ToString() + ":" + x.ToString()).ToArray());

        }


        private void button9_Click(object sender, EventArgs e)
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            using (var stream = new FileStream("sanity-output.bin", FileMode.Append))
            {
                for (int j = 0; j < 100; j++)
                {
                    
                    byte[] data = new byte[100000000];
                    rng.GetBytes(data);
                    data = data.Where(x => x < 252).Select(x => (byte)(x % 36)).ToArray();
                    var converted = Util.Convert36to256Random(data).ToArray();
                    stream.Write(converted, 0, converted.Length);
                }

            }
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
