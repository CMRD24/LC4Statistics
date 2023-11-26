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
            Random rand = new Random();
            for (int i = 0; i < 36; i++)
            {
                arr[i] = (byte)i; // initialize array with 0 to 35
            }
            Shuffle(arr);
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
    }
}
