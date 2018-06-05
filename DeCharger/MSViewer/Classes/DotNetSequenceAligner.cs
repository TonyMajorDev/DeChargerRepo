using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class DotNetSequenceAligner
    {
        public class SmithWaterman
        {

            public List<AlignResult[]> AlignAllSequences(List<string> sequences, List<string> databasesequences)
            {
                List<AlignResult[]> allalignresults = new List<AlignResult[]>();
                int i = 0;
                foreach (string seq in sequences)
                {
                    foreach (string dtseq in databasesequences)
                    {
                        allalignresults.Add(Align(seq, dtseq));
                        i++;
                    }
                }
                return allalignresults;
            }

            public class AlignResult
            {
                
                public string Name = string.Empty;
                public override string ToString()
                {
                    return Name;
                }
                public AlignResult(string seq1, string seq2)
                {
                    seq_1_aligned = seq1;
                    seq_2_aligned = seq2;
                }
                private string seq_1_aligned = string.Empty;
                private string seq_2_aligned = string.Empty;
                public string MatchString
                {
                    get
                    {
                        StringBuilder sb = new StringBuilder();
                        char[] array1 = seq_1_aligned.ToCharArray();
                        char[] array2 = seq_2_aligned.ToCharArray();
                        MatrixMatch m = new MatrixMatch();
                        for (int i = 0; i < array1.Length; i++)
                        {
                            sb.Append(m.NtMatchString(array1[i], array2[i]));
                        }
                        StringBuilder rsb = new StringBuilder();
                        rsb.Append(seq_1_aligned + "\n");
                        rsb.Append(sb.ToString() + "\n");
                        rsb.Append(seq_2_aligned + "\n");
                        return rsb.ToString();
                    }
                }
                public int MatchLength
                {
                    get
                    {
                        StringBuilder sb = new StringBuilder();
                        char[] array1 = seq_1_aligned.ToCharArray();
                        char[] array2 = seq_2_aligned.ToCharArray();
                        MatrixMatch m = new MatrixMatch();
                        int k = 0;
                        for (int i = 0; i < array1.Length; i++)
                        {
                            if (m.NtMatchString(array1[i], array2[i]) == '|') k++;
                        }
                        return k;
                    }
                }
                public int seq1_end = 0;
                public int seq2_end = 0;
                public int seq1_start
                {
                    get { return seq1_end - seq_1_aligned.Replace("-", "").Length; }
                }
                public int seq2_start
                {
                    get { return seq2_end - seq_2_aligned.Replace("-", "").Length; }
                }
            }
            //internal class to store entry points fo local align fragments
            class seed
            {
                private int x = 0;
                private int y = 0;
                private int score = 0;
                public seed(int sx, int sy)
                {
                    x = sx;
                    y = sy;
                }
                public int X
                { get { return x; } }
                public int Y
                { get { return y; } }
                public int Score
                {
                    get { return score; }
                    set { score = value; }
                }
            }

            int gapPenalty = 2;
            int matchScore = 2;
            int misMatchScore = -1;
            public string matrixType = "Advance DNA";
            public SmithWaterman() { }
            /// <summary>
            /// constructor of Smith Waterman aligner
            /// </summary>
            /// <param name="gap"> minus score when a gap happend</param>
            /// <param name="match">add score when bases are perfect-match</param>
            /// <param name="mismatch">add score when bases are mis-match</param>
            public SmithWaterman(int gap, int match, int mismatch, string mType)
            {
                gapPenalty = gap;
                matchScore = match;
                misMatchScore = mismatch;
                matrixType = mType;
            }

            /// <summary>
            /// Align two sequence and return align result
            /// </summary>
            /// <param name="seq1">sequence 1 as string (case sensitive)</param>
            /// <param name="seq2">sequence 2 as string (case sensitive)</param>
            /// <returns>align result array of all highest score entry point</returns>
            public AlignResult[] Align(byte[] seq1, byte[] seq2)
            {
                return null;
            }

            /// <summary>
            /// Align two sequence and return align result
            /// </summary>
            /// <param name="seq1">sequence 1 as string (case sensitive)</param>
            /// <param name="seq2">sequence 2 as string (case sensitive)</param>
            /// <returns>align result array of all highest score entry point</returns>
            public AlignResult[] Align(string seq1, string seq2, string mType = "Advance DNA")
            {
                #region component
                int l1 = seq1.Length;
                int l2 = seq2.Length;
                int[,] dpMatrix = new int[l2 + 1, l1 + 1];   //dynamic program matrix
                char[,] pointerMatrix = new char[l2 + 1, l1 + 1];  //pointer matrix
                #endregion

                #region fill dynamic program
                char[] seq_1 = seq1.ToCharArray();
                char[] seq_2 = seq2.ToCharArray();
                // inital dynamic matrix with zero
                this.dpm_init(dpMatrix, pointerMatrix, l1, l2);

                // fill dpMatrix and pointer matrix as Needleman Wunsch algorithmn
                // two different point are marked below
                int fU, fD, fL;
                //Create match object
                MatrixMatch m = new MatrixMatch(mType);

                for (int i = 1; i <= l2; i++)
                {
                    for (int j = 1; j <= l1; j++)
                    {
                        int match = (seq_1[j - 1] == seq_2[i - 1]) ? matchScore : misMatchScore;
                        fU = dpMatrix[i - 1, j] - gapPenalty;
                        fD = dpMatrix[i - 1, j - 1] + ((m.Match(seq_1[j - 1], seq_2[i - 1]) >= 1) ? matchScore : misMatchScore);
                        fL = dpMatrix[i, j - 1] - gapPenalty;
                        int max = 0;
                        char ptr;
                        if (fD >= fU && fD >= fL)
                        {
                            max = fD;
                            ptr = '\\';
                        }
                        else if (fU > fL)
                        {
                            max = fU;
                            ptr = '|';
                        }
                        else
                        {
                            max = fL;
                            ptr = '-';
                        }
                        dpMatrix[i, j] = (max < 0) ? 0 : max;  //S.W. method score will be not smaller than Zero;
                        pointerMatrix[i, j] = (dpMatrix[i, j] == 0) ? '.' : ptr; //S.W. method trace back will stop at cell with zero score (marked with ".");
                    }
                }
                #endregion

                /* Debug code for present dynamic program ad pointer matrix 
                    
                     Console.WriteLine("DynamicMatrix");
                     print_matrix(dpMatrix, seq1, seq2);                    
                     Console.WriteLine("PointerMatrix");
                     print_traceback(pointerMatrix,seq1,seq2);                     
                     */


                // S.W method walk from the high score entry point (seed)
                // The high score cell might be not unique.
                #region find highest score seed
                List<seed> highList = new List<seed>();
                int listNumber = 10;
                for (int i = 0; i <= l2; i++)
                {
                    for (int j = 0; j <= l1; j++)
                    {
                        if (highList.Count < listNumber)
                        {
                            seed newEntry = new seed(i, j);
                            newEntry.Score = dpMatrix[i, j];
                            highList.Add(newEntry);
                        }
                        else
                        {
                            int insertPoint = listNumber;
                            for (int k = 4; k >= 0; k--)
                            {
                                if (dpMatrix[i, j] >= highList[k].Score) insertPoint = k;
                            }
                            if (insertPoint < listNumber)
                            {
                                seed newEntry = new seed(i, j);
                                newEntry.Score = dpMatrix[i, j];
                                highList.Insert(insertPoint, newEntry);
                                highList.RemoveAt(listNumber);
                            }
                        }
                    }
                }
                #endregion

                //One seed will have one align result
                // All aligned result will return as a AlignResult array
                #region track back to zero
                List<AlignResult> align = new List<AlignResult>();
                foreach (seed s in highList)
                {
                    align.Add(traceBack(pointerMatrix, seq1, seq2, s.X, s.Y));
                }
                return align.ToArray();
                #endregion
            }

            /// <summary>
            /// The trace back procedure of Smith Wunsch algorithm is similar to needlemen
            /// but:
            /// 1. start from the high score cells
            /// 2. end at zero-score cell or 0,0 point
            /// </summary>
            public AlignResult traceBack(char[,] traceBackMatrix, string seq1, string seq2, int x, int y)
            {
                // start from the highest cells
                //Seed site
                int ti = x;
                int tj = y;
                char[] seq_1 = seq1.ToCharArray();
                char[] seq_2 = seq2.ToCharArray();
                StringBuilder seq1Builder = new StringBuilder();
                StringBuilder seq2Builder = new StringBuilder();
                while (ti > 0 || tj > 0) // stop at (0,0) point
                {
                    if (traceBackMatrix[ti, tj] == '.') break; //stop at zero cell
                    switch (traceBackMatrix[ti, tj])
                    {
                        case '|':
                            seq1Builder.Insert(0, '-');
                            seq2Builder.Insert(0, seq_2[ti - 1]);
                            ti--;
                            break;
                        case '\\':
                            seq1Builder.Insert(0, seq_1[tj - 1]);
                            seq2Builder.Insert(0, seq_2[ti - 1]);
                            ti--;
                            tj--;
                            break;
                        case '-':
                            seq1Builder.Insert(0, seq_1[tj - 1]);
                            seq2Builder.Insert(0, '-');
                            tj--;
                            break;
                        default:
                            break;
                    }
                }
                AlignResult AR = new AlignResult(seq1Builder.ToString(), seq2Builder.ToString());
                AR.seq2_end = x; //ti;
                AR.seq1_end = y; // tj;
                return AR;
            }

            public void dpm_init(int[,] dpMatrix, char[,] traceBackMatrix, int seq1Length, int seq2Length)
            {
                dpMatrix[0, 0] = 0;
                traceBackMatrix[0, 0] = '.';
                for (int i = 1; i <= seq1Length; i++)
                {
                    dpMatrix[0, i] = 0;
                    traceBackMatrix[0, i] = '.';
                }
                for (int i = 1; i <= seq2Length; i++)
                {
                    dpMatrix[i, 0] = 0;
                    traceBackMatrix[i, 0] = '.';
                }
            }

            // methods for debug
            public void print_traceback(char[,] charArray, string seq1, string seq2)
            {
                StringBuilder sb = new StringBuilder();
                char[] seq1Array = seq1.ToCharArray();
                char[] seq2Array = seq2.ToCharArray();
                //Create Title
                sb.Append('#');
                sb.Append('\t');
                for (int j = 0; j < seq1Array.Length; j++)
                {
                    sb.Append('\t');
                    sb.Append(seq1Array[j]);
                }
                sb.Append('\n');
                // dump array
                for (int i = 0; i < seq2Array.Length + 1; i++)
                {
                    sb.Append((i > 0) ? seq2Array[i - 1] : '-');
                    for (int j = 0; j < seq1Array.Length + 1; j++)
                    {
                        sb.Append('\t');
                        sb.Append(charArray[i, j]);
                    }
                    sb.Append('\n');
                }
                System.Console.Write(sb.ToString());


            }
            public void print_matrix(int[,] intArray, string seq1, string seq2)
            {
                StringBuilder sb = new StringBuilder();
                char[] seq1Array = seq1.ToCharArray();
                char[] seq2Array = seq2.ToCharArray();
                //Create Title
                sb.Append('#');
                sb.Append('\t');
                for (int j = 0; j < seq1Array.Length; j++)
                {
                    sb.Append('\t');
                    sb.Append(seq1Array[j]);
                }
                sb.Append('\n');
                // dump array
                for (int i = 0; i < seq2Array.Length + 1; i++)
                {
                    sb.Append((i > 0) ? seq2Array[i - 1] : '-');
                    for (int j = 0; j < seq1Array.Length + 1; j++)
                    {
                        sb.Append('\t' + intArray[i, j].ToString());
                    }
                    sb.Append('\n');
                }
                System.Console.Write(sb.ToString());

            }
        }
        public class MatrixMatch
        {
            #region Build in Matrix
            //basic DNA matrix
            //    char[] ntSymbol = new char[5] { 'A', 'T', 'C', 'G', 'N' };
            //    int[,] ntMatrix = new int[5, 5] { { 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 0 } };
            //    //Advance DNA matrix
            //    char[] antSymbol = new char[15] { 'A', 'C', 'G', 'T', 'R', 'Y', 'K', 'M', 'S', 'W', 'B', 'D', 'H', 'V', 'N' };
            //    int[,] antMatrix = new int[15, 15] { 
            //{ 1,0,0,0,1,0,0,1,0,1,0,1,1,1,1},
            //{ 0,1,0,0,0,1,0,1,1,0,1,0,1,1,1},
            //{ 0,0,1,0,1,0,1,0,1,0,1,1,0,1,1},
            //{ 0,0,0,1,0,1,1,0,0,1,1,1,1,0,1},
            //{ 1,0,1,0,1,0,0,1,1,1,1,1,1,1,1},
            //{ 0,1,0,1,0,1,1,1,1,1,1,1,1,1,1},
            //{ 0,0,1,1,0,1,1,0,1,1,1,1,1,1,1},
            //{ 1,1,0,0,1,1,0,1,1,1,1,1,1,1,1},
            //{ 0,1,1,0,1,1,1,1,1,0,1,1,1,1,1},
            //{ 1,0,0,1,1,1,1,1,0,1,1,1,1,1,1},
            //{ 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            //{ 1,0,1,1,1,1,1,1,1,1,1,1,1,1,1},
            //{ 1,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
            //{ 1,1,1,0,1,1,1,1,1,1,1,1,1,1,1},
            //{ 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}     
            //};

            //    // Blosum62 matrix
            //    char[] aaSymbol = new char[20] { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'Y' };
            //    int[,] blosum62 = new int[20, 20] {
            //{4,0,-2,-1,-2,0,-2,-1,-1,-1,-1,-2,-1,-1,-1,1,-1,0,-3,-2},
            //{0,9,-3,-4,-2,-3,-3,-1,-3,-1,-1,-3,-3,-3,-3,-1,-1,-1,-2,-2},
            //{-2,-3,6,2,-3,-1,1,-3,-1,-4,-3,1,-1,0,-2,0,1,-3,-4,-3},
            //{-1,-4,2,5,-3,-2,0,-3,1,-3,-2,0,-1,2,0,0,0,-2,-3,-2},
            //{-2,-2,-3,-3,6,-3,-1,0,-3,0,0,-3,-4,-3,-3,-2,-2,-1,1,3},
            //{0,-3,-1,-2,-3,6,-2,-4,-2,-4,-3,0,-2,-2,-2,0,1,-3,-2,-3},
            //{-2,-3,1,0,-1,-2,8,-3,-1,-3,-2,-1,-2,0,0,-1,0,-3,-2,2},
            //{-1,-1,-3,-3,0,-4,-3,4,-3,2,1,-3,-3,-3,-3,-2,-2,3,-3,-1},
            //{-1,-3,-1,1,-3,-2,-1,-3,5,-2,-1,0,-1,1,2,0,0,-2,-3,-2},
            //{-1,-1,-4,-3,0,-4,-3,2,-2,4,2,-3,-3,-2,-2,-2,-2,1,-2,-1},
            //{-1,-1,-3,-2,0,-3,-2,1,-1,2,5,-2,-2,0,-1,-1,-1,1,-1,-1},
            //{-2,-3,1,0,-3,0,-1,-3,0,-3,-2,6,-1,0,0,1,0,-3,-4,-2},
            //{-1,-3,-1,-1,-4,-2,-2,-3,-1,-3,-2,-1,7,-1,-2,-1,1,-2,-4,-3},
            //{-1,-3,0,2,-3,-2,0,-3,1,-2,0,0,-1,5,1,0,0,-2,-2,-1},
            //{-1,-3,-2,0,-3,-2,0,-3,2,-2,-1,0,-2,1,5,-1,-1,-3,-3,-2},
            //{1,-1,0,0,-2,0,-1,-2,0,-2,-1,1,-1,0,-1,4,1,-2,-3,-2},
            //{-1,-1,1,0,-2,1,0,-2,0,-2,-1,0,1,0,-1,1,4,-2,-3,-2},
            //{0,-1,-3,-2,-1,-3,-3,3,-2,1,1,-3,-2,-2,-3,-2,-2,4,-3,-1},
            //{-3,-2,-4,-3,1,-2,-2,-3,-3,-2,-1,-4,-4,-2,-3,-3,-3,-3,11,2},
            //{-2,-2,-3,-2,3,-3,2,-1,-2,-1,-1,-2,-3,-1,-2,-2,-2,-1,2,7}};

            char[] pamaaSymbol = new char[20] { 'A', 'R', 'N', 'D', 'C', 'Q', 'E', 'G', 'H', 'I', 'L', 'K', 'M', 'F', 'P', 'S', 'T', 'W', 'Y', 'V' };///, 'B', 'Z', 'X' };

            int[,] pam30ms = new int[20, 20] {
                            {6, -7, -4, -3, -6, -4, -2, -2, -7, -5, -6, -7, -5, -8, -2, 0, -1, -13, -8, -2},
                            {-7, 8, -6, -10, -8, -2, -9, -9, -2, -5, -7, 0, -4, -9, -4, -3, -6, -2, -10, -8},
                            {-4, -6, 8, 2, -11, -3, -2, -3, 0, -5, -6, -1, -9, -9, -6, 0, -2, -8, -4, -8},
                            {-3, -10, 2, 8, -14, -2, 2, -3, -4, -7, -10, -4, -11, -15, -8, -4, -5, -15, -11, -8},
                            {-6, -8, -11, -14, 10, -14, -14, -9, -7, -6, -11, -14, -13, -13, -8, -3, -8, -15, -4, -6},
                            {-4, -2, -3, -2, -14, 8, 1, -7, 1, -8, -7, -3, -4, -13, -3, -5, -5, -13, -12, -7},
                            {-2, -9, -2, 2, -14, 1, 8, -4, -5, -5, -7, -4, -7, -14, -5, -4, -6, -17, -8, -6},
                            {-2, -9, -3, -3, -9, -7, -4, 6, -9, -11, -11, -7, -8, -9, -6, -2, -6, -15, -14, -5},
                            {-7, -2, 0, -4, -7, 1, -5, -9, 9, -9, -8, -6, -10, -6, -4, -6, -7, -7, -3, -6},
                            {-5, -5, -5, -7, -6, -8, -5, -11, -9, 8, 8, -6, -1, -2, -8, -7, -2, -14, -6, 2},
                            {-6, -7, -6, -10, -11, -7, -7, -11, -8, 8, 8, -7, 0, -3, -8, -8, -5, -10, -7, 0},
                            {-7, 0, -1, -4, -14, -3, -4, -7, -6, -6, -7, 7, -2, -14, -6, -4, -3, -12, -9, -9},
                            {-5, -4, -9, -11, -13, -4, -7, -8, -10, -1, 0, -2, 11, -4, -8, -5, -4, -13, -11, -1},
                            {-8, -9, -9, -15, -13, -13, -14, -9, -6, -2, -3, -14, -4, 9, -10, -6, -9, -4, 2, -8},
                            {-2, -4, -6, -8, -8, -3, -5, -6, -4, -8, -8, -6, -8, -10, 8, -2, -4, -14, -13, -6},
                            {0, -3, 0, -4, -3, -5, -4, -2, -6, -7, -8, -4, -5, -6, -2, 6, 0, -5, -7, -6, },
                            {-1, -6, -2, -5, -8, -5, -6, -6, -7, -2, -5, -3, -4, -9, -4, 0, 7, -13, -6, -3, },
                            {-13, -2, -8, -15, -15, -13, -17, -15, -7, -14, -10, -12, -13, -4, -14, -5, -13, 13, -5, -15},
                            {-8, -10, -4, -11, -4, -12, -8, -14, -3, -6, -7, -9, -11, 2, -13, -7, -6, -5, 10, -7},
                            {-2, -8, -8, -8, -6, -7, -6, -5, -6, 2, 0, -9, -1, -8, -6, -6, -3, -15, -7, 7}
                            };

            //    int[,] identity = new int[20, 20] {
            //{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0},
            //{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1}};

            //    // PAM250 matrix 
            //    int[,] PAM250 = new int[20, 20] {    
            //{2,-2,0,0,-4,1,-1,-1,-1,-2,-1,0,1,0,-2,1,1,0,-6,-3},
            //{-2,12,-5,-5,-4,-3,-3,-2,-5,-6,-5,-4,-3,-5,-4,0,-2,-2,-8,0},
            //{0,-5,4,3,-6,1,1,-2,0,-4,-3,2,-1,2,-1,0,0,-2,-7,-4},
            //{0,-5,3,4,-5,0,1,-2,0,-3,-2,1,-1,2,-1,0,0,-2,-7,-4},
            //{-4,-4,-6,-5,9,-5,-2,1,-5,2,0,-4,-5,-5,-4,-3,-3,-1,0,7},
            //{1,-3,1,0,-5,5,-2,-3,-2,-4,-3,0,-1,-1,-3,1,0,-1,-7,-5},
            //{-1,-3,1,1,-2,-2,6,-2,0,-2,-2,2,0,3,2,-1,-1,-2,-3,0},
            //{-1,-2,-2,-2,1,-3,-2,5,-2,2,2,-2,-2,-2,-2,-1,0,4,-5,-1},
            //{-1,-5,0,0,-5,-2,0,-2,5,-3,0,1,-1,1,3,0,0,-2,-3,-4},
            //{-2,-6,-4,-3,2,-4,-2,2,-3,6,4,-3,-3,-2,-3,-3,-2,2,-2,-1},
            //{-1,-5,-3,-2,0,-3,-2,2,0,4,6,-2,-2,-1,0,-2,-1,2,-4,-2},
            //{0,-4,2,1,-4,0,2,-2,1,-3,-2,2,-1,1,0,1,0,-2,-4,-2},
            //{1,-3,-1,-1,-5,-1,0,-2,-1,-3,-2,-1,6,0,0,1,0,-1,-6,-5},
            //{0,-5,2,2,-5,-1,3,-2,1,-2,-1,1,0,4,1,-1,-1,-2,-5,-4},
            //{-2,-4,-1,-1,-4,-3,2,-2,3,-3,0,0,0,1,6,0,-1,-2,2,-4},
            //{1,0,0,0,-3,1,-1,-1,0,-3,-2,1,1,-1,0,2,1,-1,-2,-3},
            //{1,-2,0,0,-3,0,-1,0,0,-2,-1,0,0,-1,-1,1,3,0,-5,-3},
            //{0,-2,-2,-2,-1,-1,-2,4,-2,2,2,-2,-1,-2,-2,-1,0,4,-6,-2},
            //{-6,-8,-7,-7,0,-7,-3,-5,-3,-2,-4,-4,-6,-5,2,-2,-5,-6,17,0},
            //{-3,0,-4,-4,7,-5,0,-1,-4,-1,-2,-2,-5,-4,-4,-3,-3,-2,0,10}};
            //    //Gonnet160  matrix 
            //    int[,] Gonnet160 = new int[20, 20] { 
            //{46,3,-11,-4,-38,2,-18,-18,-12,-22,-12,-12,-1,-7,-16,16,5,1,-55,-37},
            //{3,135,-53,-52,-18,-34,-23,-25,-48,-29,-19,-31,-52,-42,-35,-2,-14,-6,-21,-13},
            //{-11,-53,70,34,-70,-7,-1,-62,-1,-65,-50,26,-19,6,-16,0,-6,-49,-78,-42},
            //{-4,-52,34,59,-62,-21,-1,-43,13,-45,-31,5,-14,23,-3,-3,-8,-30,-64,-44},
            //{-38,-18,-70,-62,91,-76,-7,3,-53,19,14,-47,-58,-41,-53,-45,-36,-8,32,56},
            //{2,-34,-7,-21,-76,82,-27,-70,-24,-67,-52,-2,-30,-21,-21,-1,-24,-52,-55,-60},
            //{-18,-23,-1,-1,-7,-27,93,-37,2,-32,-21,15,-22,17,3,-8,-8,-35,-19,27},
            //{-18,-25,-62,-43,3,-70,-37,59,-35,30,29,-44,-43,-32,-41,-33,-12,40,-34,-20},
            //{-12,-48,-1,13,-53,-24,2,-35,55,-34,-21,8,-16,20,35,-4,-2,-30,-54,-35},
            //{-22,-29,-65,-45,19,-67,-32,30,-34,57,34,-48,-35,-24,-35,-36,-24,17,-20,-11},
            //{-12,-19,-50,-31,14,-52,-21,29,-21,34,76,-36,-42,-12,-29,-23,-11,14,-22,-13},
            //{-12,-31,26,5,-47,-2,15,-44,8,-48,-36,65,-22,5,-4,11,3,-38,-55,-22},
            //{-1,-52,-19,-14,-58,-30,-22,-43,-16,-35,-42,-22,96,-8,-21,0,-4,-32,-74,-48},
            //{-7,-42,6,23,-41,-21,17,-32,20,-24,-12,5,-8,56,17,-2,-4,-27,-40,-29},
            //{-16,-35,-16,-3,-53,-21,3,-41,35,-35,-29,-4,-21,17,71,-9,-9,-34,-24,-29},
            //{16,-2,0,-3,-45,-1,-8,-33,-4,-36,-23,11,0,-2,-9,44,23,-20,-47,-28},
            //{5,-14,-6,-8,-36,-24,-8,-12,-2,-24,-11,3,-4,-4,-9,23,50,0,-54,-32},
            //{1,-6,-49,-30,-8,-52,-35,40,-30,17,14,-38,-32,-27,-34,-20,0,53,-45,-24},
            //{-55,-21,-78,-64,32,-55,-19,-34,-54,-20,-22,-55,-74,-40,-24,-47,-54,-45,158,38},
            //{-37,-13,-42,-44,56,-60,27,-20,-35,-11,-13,-22,-48,-29,-29,-28,-32,-24,38,100}};
            #endregion
            private int[,] matrix;
            private char[] symbol;
            public MatrixMatch()
            {
                //SelectType("DNA");
            }
            public MatrixMatch(string type)
            {
                SelectType(type);
            }

            public MatrixMatch(int[,] user_matrix, char[] user_symbol)
            {
                matrix = user_matrix;
                symbol = user_symbol;
            }

            public void SelectType(string type)
            {
                matrix = pam30ms;
                symbol = pamaaSymbol;
                //switch (type)
                //{
                //    case "Pam30ms":
                //        matrix = pam30ms;
                //        symbol = pamaaSymbol;
                //        break;
                //    //case "DNA":
                //    //    matrix = ntMatrix;
                //    //    symbol = ntSymbol;
                //    //    break;
                //    //case "Advance DNA":
                //    //    matrix = antMatrix;
                //    //    symbol = antSymbol;
                //    //    break;
                //    //case "Blosum62":
                //    //    matrix = blosum62;
                //    //    symbol = aaSymbol;
                //    //    break;
                //    //case "Identity":
                //    //    matrix = identity;
                //    //    symbol = aaSymbol;
                //    //    break;

                //    //case "PAM250":
                //    //    matrix = PAM250;
                //    //    symbol = aaSymbol;
                //    //    break;
                //    //case "Gonnet160":
                //    //    matrix = Gonnet160;
                //    //    symbol = aaSymbol;
                //    //    break;
                //    //default:
                //    //    matrix = ntMatrix;
                //    //    symbol = ntSymbol; break;
                //}
            }

            public int Match(char a, char b)
            {
                symbol = pamaaSymbol;
                //matrix = blosum62;
                //symbol = antSymbol;
                matrix = pam30ms;
                if (matrix == null) throw new Exception("matrix is not defined!");
                if (symbol == null) throw new Exception("symbol list is not defined!");
                int x = -1;
                int y = -1;
                for (int i = 0; i < symbol.Length; i++)
                {
                    if (symbol[i] == a) x = i;
                    if (symbol[i] == b) y = i;
                }
                if (x == -1 || y == -1)
                {
                    return 0;
                    //throw new Exception("symbol " + a + "  " + b + " is not found!");
                }
                return matrix[x, y];
            }


            public char NtMatchString(char a, char b)
            {
                this.SelectType("Advance DNA");

                if (a == '-' || b == '-')
                {
                    return '.';
                }
                else
                {
                    return ((this.Match(a, b) >= 1) ? '|' : ':');
                }
            }

        }
    }
}
