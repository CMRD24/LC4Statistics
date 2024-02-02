using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LC4Statistics
{
    public class IndexSearch
    {
        public IndexSearch() { }

        private int guessCount = 0;

        public Tuple<int, int> check(byte[] plain, byte[] cipher)
        {
            int lookInto = Math.Min(10, plain.Length-1);
            bool[,] determined = new bool[lookInto+1, 38];
            
            int guessed = 0;
            int derived = 0;
            for(int i = 0; i < determined.GetLength(0)-1; i++)
            {
                bool hasNewDetermined = false;
                do
                {
                    hasNewDetermined = applyFirstK(i);
                    //MessageBox.Show(hasNewDetermined.ToString());
                    if (!hasNewDetermined)
                    {
                        //guess:
                        
                        if(setPos(i, plain[i]))
                        {
                            guessed++;
                            hasNewDetermined = applyFirstK(i);
                        }
                        
                        if (!hasNewDetermined)
                        {
                            //guess s0:
                            
                            
                            if (setS0Val(i))
                            {
                                guessed++;
                                hasNewDetermined = applyFirstK(i);
                            }
                            

                        }
                    }
                    
                }
                while (hasNewDetermined);
                //MessageBox.Show(guessed.ToString());
            }
            int vars = plain.Take(lookInto).Concat(cipher.Take(lookInto)).Distinct().Count()+lookInto;

            string s = "";
            for(int i = 0; i < 38; i++)
            {
                bool set = false;
                for(int j = 0; j < Math.Min(10, plain.Length); j++)
                {
                    if (determined[j, i])
                    {
                        set = true;
                    }
                }
                s = s + (set?"x":"_");
            }
            //MessageBox.Show(differentChars.ToString());
            return Tuple.Create(guessed, vars);

            bool applyFirstK(int k)
            {
                bool hasNewDetermined = false;
                for (int i = 0; i <= k; i++)
                {
                    if(applyFirstRule(i) || applySecondRule(i) || applyThirdRule(i) || applyForthRule(i))
                    {
                        hasNewDetermined = true;
                    }
                }
                return hasNewDetermined;
            }


            bool posOfCharIsSet(int round, byte b)
            {
                return determined[round, b];
            }

            bool valS0Set(int round)
            {
                return determined[round, 36];
            }

            bool posS0Set(int round)
            {
                return determined[round, 37];
            }

            bool setPos(int round, byte b)
            {
                bool newSet = !determined[round, b];
                determined[round, b] = true;
                return newSet;
            }

            bool setS0Val(int round)
            {
                bool newSet = !determined[round, 36];
                determined[round, 36] = true;
                return newSet;
            }

            void setS0Pos(int round)
            {
                determined[round, 37] = true;
            }

            bool applyFirstRule(int round)
            {
                int count = boolToInt(posOfCharIsSet(round, plain[round])) + boolToInt(posOfCharIsSet(round, cipher[round])) + boolToInt(valS0Set(round));
                if(count == 2)
                {
                    setPos(round, plain[round]);
                    setPos(round, cipher[round]);
                    setS0Val(round);
                    derived++;
                    return true;
                }
                return false;
            }

            bool applySecondRule(int round)
            {
                int count = boolToInt(posOfCharIsSet(round, plain[round])) + boolToInt(posOfCharIsSet(round, cipher[round])) + boolToInt(posS0Set(round))+ boolToInt(posS0Set(round+1));
                if (count == 3)
                {
                    setPos(round, plain[round]);
                    setPos(round, cipher[round]);
                    setS0Pos(round);
                    setS0Pos(round+1);
                    derived++;
                    return true;
                }
                return false;
            }

            bool applyThirdRule(int round)
            {
                int count = boolToInt(posOfCharIsSet(round, cipher[round+1])) + boolToInt(posOfCharIsSet(round, cipher[round])) + boolToInt(posOfCharIsSet(round, plain[round])) + boolToInt(posOfCharIsSet(round+1, cipher[round+1]));
                if (count == 3)
                {
                    setPos(round, cipher[round+1]);
                    setPos(round, cipher[round]);
                    setPos(round, plain[round]);
                    setPos(round+1, cipher[round+1]);
                    derived++;
                    return true;
                }
                return false;
            }
            bool applyForthRule(int round)
            {
                int count = boolToInt(posOfCharIsSet(round, plain[round + 1])) + boolToInt(posOfCharIsSet(round, cipher[round])) + boolToInt(posOfCharIsSet(round, plain[round])) + boolToInt(posOfCharIsSet(round + 1, plain[round + 1]));
                if (count == 3)
                {
                    setPos(round, plain[round + 1]);
                    setPos(round, cipher[round]);
                    setPos(round, plain[round]);
                    setPos(round + 1, plain[round + 1]);
                    derived++;
                    return true;
                }
                return false;
            }
        }

        private int boolToInt(bool b)
        {
            if (b)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
