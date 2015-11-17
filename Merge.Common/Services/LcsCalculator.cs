using System;

namespace Merge.Common.Services
{
    /// <summary>
    /// Calculate longest common subsequence.
    /// </summary>
    public class LcsCalculator
    {
        private static int[,] _matrix;

        /// <summary>
        /// Sets without changes items.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="changed"></param>
        public void SetLongestCommonSubsequence(Line[] original, Line[] changed)
        {
            _matrix = new int[original.Length + 1, changed.Length + 1];

            Lcs(original, changed);

            Backtrack(original, changed, original.Length, changed.Length);
        }

        private void Lcs(Line[] s1, Line[] s2)
        {
            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    if (Equals(s1[i - 1], s2[j - 1]))
                        _matrix[i, j] = _matrix[i - 1, j - 1] + 1;
                    else
                    {
                        _matrix[i, j] = Math.Max(_matrix[i - 1, j], _matrix[i, j - 1]);
                    }
                }
            }

            //_matrix[original.Length, changed.Length] - max length longest common subsequence.
        }

        private void Backtrack(Line[] original, Line[] changed, int i, int j)
        {
            while (i != 0 && j != 0)
            {
                if (i == 0 || j == 0)
                    return;

                var changedItem = changed[j - 1];

                if (Equals(original[i - 1], changedItem))
                {
                    changedItem.OriginalLineNumber = original[i - 1].OriginalLineNumber;
                    changedItem.ChangeState = ChangeState.WithoutChanges;

                    i = i - 1;
                    j = j - 1;

                    continue;
                }

                if (_matrix[i - 1, j] >= _matrix[i, j - 1])
                {
                    i = i - 1;
                }
                else if (_matrix[i, j - 1] >= _matrix[i - 1, j])
                {
                    j = j - 1;
                }
            }
        }

        //private void Backtrack(Line[] original, Line[] changed, int i, int j)
        //{
        //    if (i == 0 || j == 0)
        //        return;

        //    var changedItem = changed[j - 1];

        //    if (Equals(original[i - 1], changedItem))
        //    {
        //        Backtrack(original, changed, i - 1, j - 1);

        //        changedItem.OriginalLineNumber = original[i - 1].OriginalLineNumber;
        //        changedItem.ChangeState = ChangeState.WithoutChanges;

        //        return;
        //    }

        //    if (_matrix[i - 1, j] >= _matrix[i, j - 1])
        //    {
        //        Backtrack(original, changed, i - 1, j);
        //    }
        //    else if (_matrix[i, j - 1] >= _matrix[i - 1, j])
        //    {
        //        Backtrack(original, changed, i, j - 1);
        //    }
        //}
    }
}