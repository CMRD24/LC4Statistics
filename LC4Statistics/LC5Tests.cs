using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using Accord.Statistics.Testing;
using System.Windows.Forms;

namespace LC4Statistics
{
    public class LC5Tests
    {



        public static void drawPropChart(Chart chart, Label infolabel)
        {
            chart.Series.Clear();
            Thread t = new Thread(new ThreadStart(() => {
                int rep = 5000000;
                for (int s = 2; s <= 4; s++)
                {
                    string sname = "|mac|=" + s.ToString();
                    Series ser = new Series("|mac|=" + s.ToString());
                    ser.ChartArea = "ChartArea1";
                    ser.Name = sname;

                    ser.ChartType = SeriesChartType.Line;
                    chart.Invoke(new Action(() => {
                        chart.Series.Add(ser);
                    }));
                    for (int st = 0; st < 30; st++)
                    {
                        int count = testPropLC5(st, s, rep);
                        string l = $"{s}:{st}:{rep}:{count}";
                        infolabel.Invoke(new Action(() => {
                            infolabel.Text = l;
                        }));
                        File.AppendAllLines("authentication-failures-lc5.txt", new string[] { l });
                        chart.Invoke(new Action(() => {
                            chart.Series[sname].Points.AddXY(st, (double)count / (double)rep);
                        }));

                    }

                }
            }));
            t.Start();
        }





        private static int testPropLC5(int st, int sameAfter, int repetitions)
        {

            int counter = 0;

            for (int i = 0; i < repetitions; i++)
            {
                Random rnd = new Random();
                byte[] key = Util.GetRandomKey();
                byte i2 = (byte)rnd.Next(0, 6);
                byte j2 = (byte)rnd.Next(0, 6);
                LC5 lc5 = new LC5(key, 0, 0, i2, j2);
                byte[] plain = Util.Get36Rand(st + 1 + sameAfter);

                byte[] enc1 = lc5.Encrypt(plain);

                byte changeValue = Util.Get36Rand(20).First(x => x != 0);
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

        public static void dab_bytedistribLC5(byte[] key, byte i2, byte j2)
        {
            LC5 lc5 = new LC5(key, 0, 0, i2, j2);



            int consecutiveWords = 3;
            int wordlength = 8;
            int[] samplePositions = { 0, 1, 2, 3, 4, 5, 6, 7 };
            int[] counter = new int[36 * samplePositions.Length * consecutiveWords];
            int[] singleOutputCounter = new int[36];

            int samples = 150000000;



            for (int i = 0; i < samples; i++)
            {
                //get words:
                byte[] clearWords = new byte[wordlength * consecutiveWords];
                for (int c = 0; c < clearWords.Length; c++)
                {
                    clearWords[c] = 0;
                }
                byte[] words = lc5.Encrypt(clearWords);
                foreach (byte b in words)
                {
                    singleOutputCounter[b]++;
                }
                for (int k = 0; k < consecutiveWords; k++)
                {
                    byte[] word = words.Skip(k * wordlength).Take(wordlength).ToArray();
                    for (int j = 0; j < samplePositions.Length; j++)
                    {
                        counter[36 * (k * samplePositions.Length + j) + word[samplePositions[j]]]++;
                    }

                }
            }

            File.WriteAllLines("dab-bytedistrib.txt", singleOutputCounter.Select((x, c) => c.ToString() + ":" + x.ToString()).ToArray());

            MessageBox.Show("completed 1");



            double[] should = new double[36 * 9];
            for (int i = 0; i < should.Length; i++)
            {
                should[i] = samples / 36d;
            }

            ChiSquareTest chiSquareTest = new ChiSquareTest(counter.Select(x => (double)x).ToArray(), should, 9 * 35);
            MessageBox.Show(chiSquareTest.PValue.ToString());


            File.WriteAllLines("dab.txt", counter.Select((x, c) => c.ToString() + ":" + x.ToString()).ToArray());
        }

    }
}
