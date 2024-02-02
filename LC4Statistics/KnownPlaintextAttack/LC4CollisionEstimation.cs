using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LC4Statistics.Form1;

namespace LC4Statistics
{
    public class LC4CollisionEstimation
    {

        private static HashSet<int> getRandomIndexes(int maxCount, int arrayLength, int excludeValue)
        {
            HashSet<int> indexes = new HashSet<int>();
            if (arrayLength - 1 <= maxCount)
            {
                int[] allValues = new int[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    if (i != excludeValue)
                    {
                        indexes.Add(i);
                    }
                }
                return indexes;
            }

            Random r = new Random();
            while (indexes.Count < maxCount)
            {
                var value = r.Next(arrayLength);
                if (value != excludeValue)
                    indexes.Add(value);
            }

            return indexes;
        }


        public struct CollisionEstimationResult
        {
            public int NrOf0to5Possibilities { get; set; }
            public double EstimatedTotal { get; set; }
            public double Avg5to10Possibilities { get; set; }
            public double Avg10to15Possibilities { get; set; }
            public double AvgCollisionsWith15Known { get; set; }
        }

        private static int getIndexOfCorrectKeyPart(List<Tuple<byte[], int>> possibilities, List<byte[]> stateList)
        {
            for (int i = 0; i < possibilities.Count; i++)
            {
                var t = possibilities[i];
                if (containsKey(t.Item1, stateList[t.Item2]))
                {
                    return i;
                }
            }
            //should never happen!
            return -1;
        }

        private static bool containsKey(byte[] pattern, byte[] key)
        {
            for (int i = 0; i < 36; i++)
            {
                if (pattern[i] != 255 && pattern[i] != key[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static CollisionEstimationResult getEstimatedCollisionsForPair(byte[] cipher0, byte[] plain0, List<byte[]> correctStateList)
        {


            byte[] known = new byte[36];
            for (int i = 0; i < 36; i++)
            {
                known[i] = 255;
            }

            Backtracking b = new Backtracking();
            var poss5 = b.calculateKeyPossibilitiesUntilNKnown(known, plain0, cipher0, 5);

            int to5amount = poss5.Count;



            var poss10 = new List<Tuple<byte[], int>>();
            int keyIndex = getIndexOfCorrectKeyPart(poss5, correctStateList);
            var correctState = poss5[keyIndex].Item1;
            var cStateOffset = poss5[keyIndex].Item2;
            var d10Key = b.calculateKeyPossibilitiesUntilNKnown(correctState, plain0.Skip(cStateOffset).ToArray(), cipher0.Skip(cStateOffset).ToArray(), 10, cStateOffset);

            HashSet<int> rindex = new HashSet<int>();
            if (poss5.Count > 0)
            {
                rindex = getRandomIndexes(100, poss5.Count, keyIndex);
                foreach (int index in rindex)
                {
                    var state = poss5[index].Item1;
                    var offset = poss5[index].Item2;
                    var d10 = b.calculateKeyPossibilitiesUntilNKnown(state, plain0.Skip(offset).ToArray(), cipher0.Skip(offset).ToArray(), 10, cStateOffset);
                    if (d10 != null)
                    {
                        poss10.AddRange(d10);
                    }


                }
            }





            //calculate by looking at keyPossibility seperately:

            double avg_amount5to10 = ((poss10.Count / (double)rindex.Count) * (poss5.Count - 1) + d10Key.Count) / (double)poss5.Count;
            int currentPos10Length = poss10.Count;
            poss10.AddRange(d10Key);

            var poss15 = new List<Tuple<byte[], int>>();
            int keyIndex10 = getIndexOfCorrectKeyPart(d10Key, correctStateList);
            var correctState10 = d10Key[keyIndex10].Item1;
            var cStateOffset10 = d10Key[keyIndex10].Item2;
            var d15Key = b.calculateKeyPossibilitiesUntilNKnown(correctState10, plain0.Skip(cStateOffset10).ToArray(), cipher0.Skip(cStateOffset10).ToArray(), 15, cStateOffset10);
            rindex = new HashSet<int>();

            if (poss10.Count > 0)
            {
                rindex = getRandomIndexes(100, poss10.Count, currentPos10Length + keyIndex10);
                foreach (int index in rindex)
                {
                    var state = poss10[index].Item1;
                    var offset = poss10[index].Item2;
                    var d15 = b.calculateKeyPossibilitiesUntilNKnown(state, plain0.Skip(offset).ToArray(), cipher0.Skip(offset).ToArray(), 15, cStateOffset10);
                    if (d15 != null)
                    {
                        poss15.AddRange(d15);
                    }

                }
            }

            //rest
            double avg_amount10to15 = ((poss15.Count / (double)rindex.Count) * (poss10.Count - 1) + d15Key.Count) / (double)poss10.Count;
            int currentPos15Length = poss15.Count;
            poss15.AddRange(d15Key);

            var collisions15known = new List<int>();
            int keyIndex15 = getIndexOfCorrectKeyPart(d15Key, correctStateList);
            b.ResetCollisions();
            var correctState15 = d15Key[keyIndex15].Item1;
            var cStateOffset15 = d15Key[keyIndex15].Item2;
            b.calculateKey(correctState15, plain0.Skip(cStateOffset15).ToArray(), cipher0.Skip(cStateOffset15).ToArray(), 15);
            var dFinalKeyCollisions = b.Collisions;
            
            rindex = new HashSet<int>();

            if (poss15.Count > 0)
            {
                rindex = getRandomIndexes(100, poss15.Count, currentPos15Length + keyIndex15);
                foreach (int index in rindex)
                {
                    b.ResetCollisions();
                    var state = poss15[index].Item1;
                    var offset = poss15[index].Item2;
                    var d15collisions = b.calculateKey(state, plain0.Skip(offset).ToArray(), cipher0.Skip(offset).ToArray(), 15);
                    collisions15known.Add(b.Collisions);
                }

            }
            double lastFactor = dFinalKeyCollisions / (double)poss15.Count;
            if (rindex.Count > 0)
            {
                lastFactor = ((collisions15known.Sum() / (double)rindex.Count) * (poss15.Count - 1) + dFinalKeyCollisions) / poss15.Count;
            }


            double estimatedCollisionsTotal = to5amount * avg_amount5to10 * avg_amount10to15 * lastFactor;
            return new CollisionEstimationResult()
            {
                EstimatedTotal = estimatedCollisionsTotal,
                NrOf0to5Possibilities = to5amount,
                Avg5to10Possibilities = avg_amount5to10,
                Avg10to15Possibilities = avg_amount10to15,
                AvgCollisionsWith15Known = lastFactor
            };
        }

    }
}
