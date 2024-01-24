using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LC4Statistics
{
    public  class LC5
    {
        public byte I1 { get; private set; }
        public byte J1 { get; private set; }
        public byte I2 { get; private set; }
        public byte J2 { get; private set; }
        public byte[] State { get; private set; }

        private byte i1;
        private byte j1;
        private byte[] firstState;
        public LC5(byte[] state, byte i1, byte j1, byte i2, byte j2)
        {
            I1 = i1;
            J1 = j1;
            I2 = i2;
            J2 = j2;
            State = state;
            firstState = state;
            this.i1 = i1;
            this.j1 = j1;
        }

        public void Reset()
        {
            I1 = i1;
            J1 = j1;
            I2 = i1;
            J2 = j1;
            State = firstState;
        }

        /// <summary>
        /// Returns state independent of indexes i,j
        /// using i1, i2 at 0,0
        /// </summary>
        /// <returns></returns>
        public byte[] GetNormalizedState()
        {
            byte[] result = new byte[36];
            for(int k = 0; k < 36; k++)
            {
                int index = ((I1 + k / 6) * 6) % 36 + ((J1 + k) % 6);
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
        private byte[] rotateRowLeft(byte[] state, byte row, int amount)
        {
            byte[] result = new byte[36];
            Array.Copy(state, result, 36);
            for (byte i = 0; i < 6; i++)
            {
                result[6 * row + i] = state[6 * row + ((i + amount) % 6)];

            }
            return result;
        }

        private byte getXIndexRotatedLeftByN(byte initialindex, byte n)
        {
            return (byte)((initialindex / 6) * 6 + ((initialindex + 6 - n) % 6));
        }

        private void rotateJIndexesRight(byte row, byte amount)
        {
            
            if (I1 / 6 == row)
            {
                J1 = (byte)((J1 + amount) % 6);
            }
            if (I2 / 6 == row)
            {
                J2 = (byte)((J2 + amount) % 6);
            }
        }

        private byte getIndexRotatedDownByN(byte initialindex, byte n)
        {
            return (byte)((initialindex+n*6)%36);
        }

        private void rotateIIndexesDown(byte column, byte amount)
        {
            if (J1 % 6 == column)
            {
                I1 = (byte)((I1+amount)%6); 
            }
            if (J2 % 6 == column)
            {
                I2 = (byte)((I2 + amount) % 6);
            }
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
        private byte[] rotateColumnDown(byte[] state, byte column, int amount)
        {
            byte[] result = new byte[36];
            Array.Copy(state, result, 36);
            for (byte i = 0; i < 6; i++)
            {
                //MessageBox.Show((6 * i + column).ToString());

                result[6 * i + column] = state[mod(6 * i + column - 6*amount,36)];

            }
            return result;
        }

        public byte SingleByteEncryption(byte plaintextByte)
        {
            
            byte plaintextRow = getRow(State, plaintextByte);
            byte plainTextColumn = getColumn(State, plaintextByte);
            byte ciphertextRow = (byte)((plaintextRow + (getByIndex(State, I1, J1) / 6) + (getByIndex(State, I2, J2) / 6)) % 6);
            byte ciphertextColumn = (byte)((plainTextColumn + (getByIndex(State, I1, J1) % 6) + (getByIndex(State, I2, J2) % 6)) % 6);
            //MessageBox.Show($"r: {r}, c: {c}, x: {x}, y: {y}");
            byte enc = getByIndex(State, ciphertextRow, ciphertextColumn);

            //File.AppendAllLines("test.txt", new string[] {$"state: {BytesToString(State)}, i1:{I1},j1:{J1},i2:{I2},j2:{J2}, c_k:{ByteToChar(plaintextByte)}, pr:{plaintextRow}, pc:{plainTextColumn}, enc:{enc}"});

            //update state:
            State = rotateRowLeft(State, plaintextRow, 5);
            rotateJIndexesRight(plaintextRow, 1);

            State = rotateColumnDown(State, ciphertextColumn, 1);
            rotateIIndexesDown(ciphertextColumn, 1);

            //change stone of indizes:

            //move i1,j1 by ciphertext byte:
            I1 = (byte)((I1 + (enc / 6)) % 6);
            J1 = (byte)((J1 + (enc % 6)) % 6);

            //move i2,j2 by value of stone one position over current + plaintext value (down/right)
            //      *,12,*...   => 12 and plaintext value
            //      *,x,*...  
            byte valueOnTopOfI2J2 = getByIndex(State, (byte)((I2+5)%6), J2);
            int nrOfRight = plaintextByte / 6 + valueOnTopOfI2J2 / 6;
            I2 = (byte)((I2 + nrOfRight) % 6);
            int nrOfDown = plaintextByte % 6 + valueOnTopOfI2J2 % 6;
            J2 = (byte)((J2 + nrOfDown) % 6);

            //File.AppendAllLines("test.txt", new string[] { $"update: {BytesToString(State)}, i1:{I1},j1:{J1},i2:{I2},j2:{J2}" });

            return enc;
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }


        public byte SingleByteDecryption(byte ciphertextByte)
        {
            byte ciphertextRow = getRow(State, ciphertextByte);
            byte ciphertextColumn = getColumn(State, ciphertextByte);
            byte plaintextRow = (byte)mod(ciphertextRow - (getByIndex(State, I1, J1) / 6) - (getByIndex(State, I2, J2) / 6), 6);
            byte plaintextColumn = (byte)mod(ciphertextColumn - (getByIndex(State, I1, J1) % 6) - (getByIndex(State, I2, J2) % 6), 6);
            //MessageBox.Show($"r: {r}, c: {c}, x: {x}, y: {y}");
            byte clear = getByIndex(State, plaintextRow, plaintextColumn);

            //File.AppendAllLines("test.txt", new string[] { $"state: {BytesToString(State)}, i1:{I1},j1:{J1},i2:{I2},j2:{J2}, c(d):{ByteToChar(clear)}, pr:{plaintextRow}, pc:{plaintextColumn}, enc:{ciphertextByte}" });

            //update state:
            State = rotateRowLeft(State, plaintextRow, 5);
            rotateJIndexesRight(plaintextRow, 1);

            State = rotateColumnDown(State, ciphertextColumn, 1);
            rotateIIndexesDown(ciphertextColumn, 1);


            //change stone of indizes:

            //move i1,j1 by ciphertext byte:
            I1 = (byte)((I1 + (ciphertextByte / 6)) % 6);
            J1 = (byte)((J1 + (ciphertextByte % 6)) % 6);

            //move i2,j2 by value of stone one position over current + plaintext value (down/right)
            //      *,12,*...   => 12 and plaintext value
            //      *,x,*...  
            byte valueOnTopOfI2J2 = getByIndex(State, (byte)((I2 + 5) % 6), J2);
            int nrOfRight = clear / 6 + valueOnTopOfI2J2 / 6;
            I2 = (byte)((I2 + nrOfRight) % 6);
            int nrOfDown = clear % 6 + valueOnTopOfI2J2 % 6;
            J2 = (byte)((J2 + nrOfDown) % 6);

            //File.AppendAllLines("test.txt", new string[] { $"update: {BytesToString(State)}, i1:{I1},j1:{J1},i2:{I2},j2:{J2}" });


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
