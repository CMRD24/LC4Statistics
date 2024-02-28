using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LC4Statistics.IND
{
    public class INDGames
    {
        private static bool encryptedOnesIND(byte[] ciphertext)
        {

            int first = Array.IndexOf(ciphertext, (byte)0);
            if (first != -1)
            {
                foreach (byte b in ciphertext.Skip(first))
                {
                    if (b != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// returns number of successful outputs
        /// </summary>
        /// <param name="msgLength"></param>
        /// <param name="rounds"></param>
        /// <returns></returns>
        public static int testIndCPA(int msgLength, int rounds)
        {
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            int successCounter = 0;
            for (int i = 0; i < rounds; i++)
            {
                var key = Util.GetRandomKey();
                LC4 lc4 = new LC4(key, 0, 0);
                var one = new byte[msgLength];
                var zero = new byte[msgLength];
                for (int j = 0; j < msgLength; j++)
                {
                    one[j] = 1;
                    zero[j] = 0;
                }
                //choose message: 
                byte[] rbyte = new byte[msgLength];
                r.GetBytes(rbyte);
                bool encOne = rbyte[0] % 2 == 0;
                var ctext = encOne ? lc4.Encrypt(one) : lc4.Decrypt(zero);

                //distinguish:
                if (encryptedOnesIND(ctext) == encOne)
                {
                    successCounter++;
                }

            }

            return successCounter;
        }

        public static double randomIndAttackProb(int messageLength, int repetitions)
        {
            int count = 0;
            List<int> zeros = new List<int>();
            List<int> same = new List<int>();
            for (int i = 0; i < repetitions; i++)
            {
                byte[] key = Util.GetRandomKey();
                byte[] data0 = Util.Get36Rand(messageLength);
                byte[] data1 = Util.Get36Rand(messageLength);


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

        private static Distinguished distinguishNotSame(byte[] plaintext, byte[] ciphertext)
        {
            bool isNotSame = false;
            int cipherZeros = 0;
            int same = 0;
            for (int i = 0; i < plaintext.Length - 1; i++)
            {
                if ((ciphertext[i] == 0 && plaintext[i] == 0 && ciphertext[i + 1] != plaintext[i + 1])
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
                if ((plaintext[i] != 0 && plaintext[i] == ciphertext[i])
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


    }
}
