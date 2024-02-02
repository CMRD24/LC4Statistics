using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LC4Statistics
{
    public class OtherTests
    {


        private static bool setStateField(byte[] state, int index, byte value)
        {
            if (index < 0 || index > 35)
            {
                MessageBox.Show("???" + index);
            }
            if (state[index] != 255 && state[index] != value)
            {
                return false;
            }
            int i = Array.FindIndex(state, x => x == value);
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

        public void testAverageDepth(byte[] key, byte[] plain, int numberOfUnknowns)
        {

            //generate key and plain+ciphertext:
            int msgLength = plain.Length;
            //File.AppendAllLines("tv.txt", new string[] { string.Join(",", key.Select(x => x.ToString())) });
            byte[] coppiedKey = new byte[36];
            Array.Copy(key, coppiedKey, 36);
            byte[][] states = new byte[msgLength][];
            byte[] cipher = new byte[msgLength];
            LC4 lc4a = new LC4(key, 0, 0);
            for (int i = 0; i < msgLength; i++)
            {
                states[i] = lc4a.GetNormalizedState();
                cipher[i] = lc4a.SingleByteEncryption(plain[i]);

            }
            byte[] partKey = removeNofKey(key, numberOfUnknowns);

            Backtracking backtracking = new Backtracking();
            var recovered = backtracking.calculateKey(partKey, plain, cipher, 0);

            if (recovered == null)
            {
                MessageBox.Show("fail!");
                return;
            }
            MessageBox.Show($"{LC4.BytesToString(states[recovered.Item2])}   (key ({recovered.Item2}))" + Environment.NewLine +
                $"{LC4.BytesToString(recovered.Item1)}   (rec)");
        }


        byte[] removeNofKey(byte[] key, int n)
        {
            for (int i = 35; i > 35 - n; i--)
            {
                key[i] = 255;
            }
            return key;
        }

        bool isSameState(byte[] state1, byte[] state2)
        {
            for (int i = 0; i < 36; i++)
            {
                if (state1[i] != state2[i])
                {
                    return false;
                }
            }
            return true;
        }



        private bool containsAny(IEnumerable<byte> array, byte[] values)
        {
            foreach (byte b in values)
            {
                if (array.Contains(b))
                {
                    return true;
                }
            }
            return false;
        }

       /*private void test0()
        {
            for (int i = 0; i < 100; i++)
            {
                testAverageDepth(200, 25);
            }
        }*/

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

        /// <summary>
        /// Find better starting index step by step.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plain"></param>
        /// <param name="numberOfUnknowns"></param>
        public void recoveryTest1(byte[] key, byte[] plain, int numberOfUnknowns, Label outputlabel)
        {
            Thread t = new Thread(new ThreadStart(() => {
                //test state function:
                //testStateFunction();

                

                int msgLength = plain.Length;

                //generate key and plain+ciphertext:
                //File.AppendAllLines("tv.txt", new string[] { string.Join(",", key.Select(x => x.ToString())) });
                byte[] coppiedKey = new byte[36];
                Array.Copy(key, coppiedKey, 36);
                byte[][] states = new byte[msgLength][];
                byte[] cipher = new byte[msgLength];
                LC4 lc4a = new LC4(key, 0, 0);

                for (int i = 0; i < msgLength; i++)
                {
                    states[i] = lc4a.GetNormalizedState();
                    cipher[i] = lc4a.SingleByteEncryption(plain[i]);

                }




                int pli = Array.IndexOf(coppiedKey, plain[0]);
                int cli = Array.IndexOf(coppiedKey, cipher[0]);
                //byte[] isState = newState(coppiedKey, (byte)pli, (byte)cli);


                //MessageBox.Show(keyToSquareString(states[1]) + Environment.NewLine + Environment.NewLine + keyToSquareString(isState));

                //MessageBox.Show($"pl: {LC4.ByteToChar(plain[0])}, ci: {LC4.ByteToChar(cipher[0])}" + Environment.NewLine + Environment.NewLine + keyToSquareString(key) + Environment.NewLine + "_________is:" + Environment.NewLine + keyToSquareString(isState) + Environment.NewLine + "_________supp" + Environment.NewLine + keyToSquareString(states[1]));



                //delete n parts of key:
                //removeNofKey(key, 27);


                //recover key:

                //get promising index:

                Backtracking backtracking = new Backtracking();

                List<Tuple<int, int>> prIndexes = new List<Tuple<int, int>>();
                for (int i = 0; i < msgLength - 100; i++)
                {
                    List<byte> pool = new List<byte>();
                    var pl = plain.Skip(i).Take(10).ToArray();
                    var ci = cipher.Skip(i).Take(10).ToArray();
                    int score = 0;
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
                MessageBox.Show("!" + sortedIndexes.First().Item2);
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
                    int possibilitiesAtIndex = backtracking.calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 2).Count;
                    prIndexes2.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                    outputlabel.Invoke(new Action(() => {
                        outputlabel.Text = $"c2({i}): " + pindex + ":" + possibilitiesAtIndex;
                    }));
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
                    int possibilitiesAtIndex = backtracking.calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 3).Count;
                    prIndexes3.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                    outputlabel.Invoke(new Action(() => {
                        outputlabel.Text = $"c3({i}): " + pindex + ":" + possibilitiesAtIndex;
                    }));
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
                    int possibilitiesAtIndex = backtracking.calculateKeyPossibilities(ck, cpl.Skip(0), cci.Skip(0), 5).Count;
                    prIndexes4.Add(Tuple.Create(pindex, possibilitiesAtIndex));
                    outputlabel.Invoke(new Action(() => {
                        outputlabel.Text = $"c5({i}): " + pindex + ":" + possibilitiesAtIndex;
                    }));
                    //MessageBox.Show(pindex + ":" + possibilitiesAtIndex);

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

                int d = 8;
                var test = backtracking.calculateKeyPossibilities(chosenKey, chosenPlain.Skip(0), chosenCipher.Skip(0), d);
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




                var recovered = backtracking.calculateKey(chosenKey, chosenPlain.Skip(0), chosenCipher.Skip(0), 0);

                if (recovered == null)
                {
                    MessageBox.Show("fail!");
                    return;
                }

                MessageBox.Show($"{LC4.BytesToString(states[recovered.Item2 + index])}   (key ({recovered.Item2}))" + Environment.NewLine +
                    $"{LC4.BytesToString(recovered.Item1)}   (rec)");
            }));
            t.Start();
        }


        public static void knownPlaintextAttackAssumptions(byte[] plaintext, byte[] key)
        {
            LC4 lc4 = new LC4(key, 0, 0);
            byte[][] nstates = new byte[20000][];
            byte[] ciphertext = new byte[20000];

            for (int i = 0; i < plaintext.Length; i++)
            {
                nstates[i] = lc4.GetNormalizedState();
                ciphertext[i] = lc4.SingleByteEncryption(plaintext[i]);
            }

            byte[] recoveredIthKey = new byte[36];

            //recover key from ciphertext and plaintext only:

            for (int i = 0; i < plaintext.Length - 27; i++)
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
                    int linearNullIndex = (12 - right) % 6 + 6 * ((12 - down) % 6);
                    bool assumption2 = nstates[i + 1][linearNullIndex] == 0;
                    if (!assumption2)
                    {
                        MessageBox.Show("no!2");
                        MessageBox.Show(linearNullIndex.ToString() + "::" + Array.FindIndex(nstates[i], x => x == 0).ToString() + Environment.NewLine + Environment.NewLine
                            + Array.FindIndex(nstates[i + 1], x => x == 0).ToString() +
                            Environment.NewLine + Environment.NewLine + "plain: " + plaintext[i] + ",cipher: " + ciphertext[i]);

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
                    if (plaintext[i + 1] == 0)
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
                        for (byte j = 1; j < 36; j++)
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
                            //this is a guess because we are looking at the pre-encryption state of i+1, the post-encryption
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
                            if (post_cipherPosCol == startindex)
                            {
                                startindex += 6;
                            }

                            int post_s0_i2_position = (ciphertext[i + 1] + startindex) % 6 + ((ciphertext[i + 1] / 6 + startindex / 6) % 6) * 6;







                            //-> take previous into account to make det. -1 or -7 or stays:
                            //plainrow = s0_i2_position row
                            int pre_s0_i2_position = post_s0_i2_position;
                            if (post_cipherPosCol == post_s0_i2_position % 6)
                            {
                                pre_s0_i2_position = pre_s0_i2_position % 6 + (((pre_s0_i2_position / 6) + 5) % 6) * 6;
                            }
                            if (indexOfPlaintext / 6 == (int)(pre_s0_i2_position / 6))
                            {
                                pre_s0_i2_position = (pre_s0_i2_position + 5) % 6 + (pre_s0_i2_position / 6) * 6;
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
                                if (!setStateField(rstate_b, pre_s0_i2_position, s0i2val))
                                {
                                    continue;
                                }


                                if (Array.IndexOf(rstate_b, plaintext[i + 2]) != -1)
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
                                        if (ct_pos != Array.IndexOf(nstates[i + 1], ciphertext[i + 2]))
                                            MessageBox.Show(
                                                "adapted1:" + Environment.NewLine +
                                                $"plain index before: {indexOfPlaintext}" + Environment.NewLine +
                                                $"cipher index before: {cipherPosition}" + Environment.NewLine +
                                                $"plain index orig (orig): {fromOrig}" + Environment.NewLine +
                                                $"plain index (from): {from}" + Environment.NewLine +
                                                $"s0i2val: {s0i2val}" + Environment.NewLine +
                                                $"nstates[i+2][0]: {nstates[i + 2][0]}" + Environment.NewLine +
                                                $"correct index: {Array.IndexOf(nstates[i + 1], ciphertext[i + 2])}" + Environment.NewLine +
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
                                    if (!setStateField(rstate_b, ct_pos, ciphertext[i + 2]))
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


                                    int pt_pos = (from - s0i2val + 6) % 6 + (((from / 6 - s0i2val / 6) + 6) % 6) * 6;
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
                                        if (pt_pos != Array.IndexOf(nstates[i + 1], plaintext[i + 2]))
                                            MessageBox.Show(
                                                "adapted2" + Environment.NewLine +
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
                                    for (int pt_pos = 0; pt_pos < 36; pt_pos++)
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
                            for (int k = 0; k < 36; k++)
                            {
                                //MessageBox.Show(((int)plausible[k]).ToString());
                                if (plausible[k] != 255 && plausible[k] != nstates[i + 1][k])
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
                    else if (ciphertext[i + 1] == 0)
                    {
                        //...

                    }
                }
            }




        }

    }
}
