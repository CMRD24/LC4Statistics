using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LC4Statistics
{
    public  class LC4
    {
        public byte I { get; private set; }
        public byte J { get; private set; }
        public byte[] State { get; private set; }

        private byte i;
        private byte j;
        private byte[] firstState;
        public LC4(byte[] state, byte i, byte j)
        {
            I = i;
            J = j;
            State = state;
            firstState = state;
            this.i = i;
            this.j = j;
        }

        public void Reset()
        {
            I = i;
            J = j;
            State = firstState;
        }

        /// <summary>
        /// Returns state independent of indexes i,j
        /// </summary>
        /// <returns></returns>
        public byte[] GetNormalizedState()
        {
            byte[] result = new byte[36];
            for(int k = 0; k < 36; k++)
            {
                int index = ((I + k / 6) * 6) % 36 + ((J + k) % 6);
                result[k] = State[index]; 
            }
            
            return result;
        }


        /// <summary>
        /// Conversion to 36 possible values
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static byte CharToByte(char ch)
        {
            if (ch >= 97 && ch <= 122)
            {
                return (byte)(ch - 87);//87+35=122
            }
            else if (ch >= 50 && ch <= 57)
            {
                return (byte)(ch - 48);
            }
            else if (ch == 35)
            {
                return (byte)0;
            }
            else
            {
                return (byte)1;
            }
        }

        /// <summary>
        /// Conversion to 36 possible characters (#, _, 2-9, a-z)
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static char ByteToChar(byte b)
        {
            if (b == 0)
            {
                return (char)35;
            }
            else if (b >= 2 && b < 10)
            {
                return (char)(b + 48);
            }
            else if (b >= 10 && b<36)
            {
                return (char)(b + 87);
            }
            else if (b == 1)
            {
                return (char)95;
            }
            //invalid: '?'
            return (char)63;
            
        }

        public static byte[] StringToByteState(string str)
        {
            return str.Select(x => CharToByte(x)).ToArray();
        }

        public static string BytesToString(byte[] data)
        {
            return new string(data.Select(x => ByteToChar(x)).ToArray());
        }

        //123456 789abc (3. Zeile) ...

        /// <summary>
        /// i. zeile, j. spalte
        /// </summary>
        /// <param name="state"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private byte getByIndex(byte[] state, byte i, byte j)
        {
                return state[6 * i + j];
        }

        private byte getRow(byte[] state, byte value)
        {
            int index = Array.IndexOf(state, value);
            if (index == -1)
            {
                throw new Exception(String.Join(",", state)+" does not contain "+value);
            }
            return (byte)(index / 6);
        }

        private byte getColumn(byte[] state, byte value)
        {
            int index = Array.IndexOf(state, value);
            if (index == -1)
            {
                throw new Exception(String.Join(",", state) + " does not contain " + value);
            }
            return (byte)((index) % 6);
        }

        //123456 abcdef
        //123456 fab
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

        /// <summary>
        /// rotate column 2:
        /// 123456      1?3456
        /// abcdef ->   a2cdef
        /// ghijkl      gbijkl
        /// </summary>
        /// <param name="state"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private byte[] rotateColumnDown(byte[] state, byte column)
        {
            byte[] result = new byte[36];
            Array.Copy(state, result, 36);
            for (byte i = 0; i < 6; i++)
            {
                //MessageBox.Show((6 * i + column).ToString());

                result[6 * i + column] = state[mod(6 * i + column - 6,36)];

            }
            return result;
        }

        public byte SingleByteEncryption(byte clear)
        {
            
            byte c_k = clear;
            byte r = getRow(State, c_k);
            byte c = getColumn(State, c_k);
            byte x = (byte)((r + (getByIndex(State, I, J) / 6)) % 6);
            byte y = (byte)((c + (getByIndex(State, I, J) % 6)) % 6);
            //MessageBox.Show($"r: {r}, c: {c}, x: {x}, y: {y}");
            byte enc = getByIndex(State, x, y);

            //File.AppendAllLines("test.txt", new string[] {$"state: {bytesToString(state)}, i:{i},j:{j}, c_k:{byteToChar(c_k)}, r:{r}, c:{c}, enc:{enc}"});

            //update state:
            State = rotateRowRight(State, r);
            c = (byte)((c + 1) % 6);
            if (x == r)
            {
                //MessageBox.Show("why here?");
                y = (byte)((y + 1) % 6);
            }
            if (I == r)
            {
                J = (byte)((J + 1) % 6);
            }
            //MessageBox.Show("y:" + y);
            State = rotateColumnDown(State, y);
            if (J == y)
            {
                I = (byte)((I + 1) % 6);
            }
            I = (byte)((I + (enc / 6)) % 6);
            J = (byte)((J + (enc % 6)) % 6);

            return enc;
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }


        public byte SingleByteDecryption(byte enc)
        {

            byte c_k = enc;
            byte x = getRow(State, c_k);
            byte y = getColumn(State, c_k);
            //mod function statt % da wert negativ werden kann
            byte r = (byte)(mod((x - (getByIndex(State, I, J) / 6)), 6));
            byte c = (byte)(mod((y - (getByIndex(State, I, J) % 6)), 6));

            byte clear = getByIndex(State, r, c);

            //File.AppendAllLines("test.txt", new string[] {$"state: {bytesToString(state)}, i:{i},j:{j}, c_k:{byteToChar(c_k)}, r:{r}, c:{c}, enc:{enc}"});

            //update state:
            State = rotateRowRight(State, r);
            c = (byte)((c + 1) % 6);
            if (x == r)
            {
                y = (byte)((y + 1) % 6);
            }
            if (I == r)
            {
                J = (byte)((J + 1) % 6);
            }

            State = rotateColumnDown(State, y);
            if (J == y)
            {
                I = (byte)((I + 1) % 6);
            }
            I = (byte)((I + (enc / 6)) % 6);
            J = (byte)((J + (enc % 6)) % 6);

            return clear;
        }



        /// <summary>
        /// DO NOT USE BEFORE RESETTING AFTER ENCYPTION!
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] data)
        {
            byte[] clear = new byte[data.Length];
            for (int k = 0; k < data.Length; k++)
            {
                clear[k] = SingleByteDecryption(data[k]);
            }
            return clear;
        }



        public byte[] Encrypt(byte[] data)
        {
            byte[] encrypted = new byte[data.Length];
            for (int k = 0; k < data.Length; k++)
            {
                encrypted[k] = SingleByteEncryption(data[k]);
            }
            return encrypted;
        }

    }
}
