using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LC4Statistics
{
    internal class Backtracking
    {

        bool setStateField(byte[] state, int index, byte value)
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

        int mod(int x, int m)
        {
            return (x % m + m) % m;
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
            for (byte i = 0; i < 36; i++)
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


            if (plaintextIndex / 6 == 0 && ((cipherIndex / 6 != 0 && cipherIndex % 6 == 1) || (cipherIndex / 6 == 0 && cipherIndex % 6 == 0)))
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

            return s0offset(s2, (byte)(((oldState[cipherIndex] / 6) + additionalYOffset) % 6), (byte)(((oldState[cipherIndex] % 6) + additionalXOffset) % 6));
        }

        int collisions = 0;

        public int Collisions { get { return collisions; } }
        public void ResetCollisions()
        {
            collisions = 0;
        }

        private byte missingCharacter(byte[] state)
        {
            for(byte i = 0; i < 36; i++)
            {
                if (!state.Contains(i))
                {
                    return i;
                }
            }
            return 255;
        }

        public Tuple<byte[], int> calculateKey(byte[] knownState, IEnumerable<byte> plaintext, IEnumerable<byte> ciphertext, int advancement, byte plaintextIndex = 255, byte ciphertextIndex = 255)
        {

            if (!knownState.Contains((byte)255))
            {

                return Tuple.Create(knownState, advancement);

            }
            /*
             * This is not possible, because it does not check the validity of the last character!
            if(knownState.Where(x => x == (byte)255).Count() == 1)
            {
                byte[] state = new byte[36];
                Array.Copy(knownState, state, 36);
                int index = Array.IndexOf(knownState, (byte)255);
                state[index] = missingCharacter(knownState);
                return Tuple.Create(state, advancement);
            }*/

            //!
            if (plaintext.First() == ciphertext.First())
            {
                if (!setStateField(knownState, 0, 0))
                {
                    collisions++;
                    return null;
                }
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
                        collisions++;
                        return null;
                    }
                    if (setStateField(knownState, cipherIndex, ciphertext.First()))
                    {
                        //calculate new state:
                        //MessageBox.Show("cont1");
                        byte[] nextState = newState(knownState, plaintextIndex, cipherIndex);
                        return calculateKey(nextState, plaintext.Skip(1), ciphertext.Skip(1), advancement + 1);
                    }
                    else
                    {
                        //MessageBox.Show("f2");
                        collisions++;
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
                        return calculateKey(nextState, plaintext.Skip(1), ciphertext.Skip(1), advancement + 1);
                    }
                    else
                    {
                        collisions++;
                        return null;
                    }

                }
                else
                {
                    //MessageBox.Show("guess s0");
                    //guess s0
                    //todo: how to enhance guessing of ratio s0?
                    //start at 1 because it can not be zero (would end up in first case)
                    for (byte i = 1; i < 36; i++)
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
                        collisions++;
                        return null;
                    }
                }
                else
                {
                    // (only ciphertextIndex known)
                    //guess s0:
                    //start at 1 because it can not be zero (would end up in first case)
                    //MessageBox.Show("guess s0");

                    for (byte i = 1; i < 36; i++)
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
                //how to enhance? --> !!!
                foreach (byte p_index in EM.FindAllIndexof(knownState, (byte)255))
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



        public List<byte[]> calculateKeyPossibilities(byte[] knownState, IEnumerable<byte> plaintext, IEnumerable<byte> ciphertext, int depth, byte plaintextIndex = 255, byte ciphertextIndex = 255)
        {
            List<byte[]> possibilities = new List<byte[]>();
            if (!knownState.Contains((byte)255))
            {
                possibilities.Add(knownState);
                return possibilities;

            }
            /*
             * This is not possible, because it does not check the validity of the last character!
            if (knownState.Where(x => x == (byte)255).Count() == 1)
            {
                byte[] state = new byte[36];
                Array.Copy(knownState, state, 36);
                int index = Array.IndexOf(knownState, (byte)255);
                state[index] = missingCharacter(knownState);
                possibilities.Add(state);
                return possibilities;
            }*/

            //!
            if (plaintext.First() == ciphertext.First())
            {
                if (!setStateField(knownState, 0, 0))
                {
                    return null;
                }
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



        public List<Tuple<byte[], int>> calculateKeyPossibilitiesUntilNKnown(byte[] knownState, IEnumerable<byte> plaintext, IEnumerable<byte> ciphertext, int n, int advanced=0, byte plaintextIndex = 255, byte ciphertextIndex = 255)
        {
            List<Tuple<byte[], int>> possibilities = new List<Tuple<byte[], int>>();
            if (!knownState.Contains((byte)255))
            {
                possibilities.Add(Tuple.Create(knownState, advanced));
                return possibilities;

            }

            /*
             * This is not possible, because it does not check the validity of the last character!
             * if (knownState.Where(x => x == (byte)255).Count() == 1)
            {
                byte[] state = new byte[36];
                Array.Copy(knownState, state, 36);
                int index = Array.IndexOf(knownState, (byte)255);
                state[index] = missingCharacter(knownState);
                possibilities.Add(Tuple.Create(state, advanced));
                return possibilities;
            }*/

            //!
            if(plaintext.Count()==0)
            {
                //exception!!!
            }


            if (plaintext.First() == ciphertext.First())
            {
                if (!setStateField(knownState, 0, 0))
                {
                    return null;
                }
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
            if (knownState.Where(x => x != (byte)255).Count() >= n)
            {
                possibilities.Add(Tuple.Create(knownState, advanced));
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
                        return calculateKeyPossibilitiesUntilNKnown(nextState, plaintext.Skip(1), ciphertext.Skip(1), n, advanced+1);
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
                        return calculateKeyPossibilitiesUntilNKnown(nextState, plaintext.Skip(1), ciphertext.Skip(1), n, advanced+1);
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
                        var calcState = calculateKeyPossibilitiesUntilNKnown(guessedState, plaintext, ciphertext, n, advanced, plaintextIndex);
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
                        return calculateKeyPossibilitiesUntilNKnown(nextState, plaintext.Skip(1), ciphertext.Skip(1), n, advanced+1);
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
                        var calcState = calculateKeyPossibilitiesUntilNKnown(guessedState, plaintext, ciphertext, n, advanced, 255, ciphertextIndex);
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
                    var calcState = calculateKeyPossibilitiesUntilNKnown(guessedState, plaintext, ciphertext, n, advanced, p_index, 255);
                    if (calcState != null)
                    {
                        possibilities.AddRange(calcState);
                    }
                }
                //MessageBox.Show("r3:"+possibilities.Count);
                return possibilities;
            }


        }



    }
}
