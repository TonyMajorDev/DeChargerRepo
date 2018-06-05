using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class NAligner
    {

        

        public static List<Alignment> AlignAllSequences(List<Sequence> sequences, List<Sequence> Databasesequences)
        {
            var matrix = App.PAM30MS;
            List<Alignment> allAlignments = new List<Alignment>();

            foreach(var seq in sequences)
            {
                foreach (var dtseq in Databasesequences)
                {
                    allAlignments.Add(Align(seq, dtseq, matrix, 13, 5));
                }
            }

            return allAlignments;
        }

        /// <summary>
        /// Alignement using NAligner method
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="matrix"></param>
        /// If you don't want to favor gaps, you should make your gap penalty a large value.
        /// <param name="o">Open gap</param>
        /// <param name="e"></param>
        /// <returns></returns>       
        ///Gap opening - cost to create a gap
        ///Gap extension - cost to make a gap bigger (must already have created a gap)
        public static Alignment Align(Sequence s1, Sequence s2, Matrix matrix, float o, float e)
        {
            float[,] scores = matrix.Scores;
            NAligner smithWatermanGotoh = new NAligner();
            int num1 = s1.Length + 1;
            int num2 = s2.Length + 1;
            byte[] pointers = new byte[num1 * num2];
            int num3 = 0;
            int index1 = 0;
            while (num3 < num1)
            {
                pointers[index1] = (byte)0;
                ++num3;
                index1 += num2;
            }
            for (int index2 = 1; index2 < num2; ++index2)
                pointers[index2] = (byte)0;
            ushort[] sizesOfVerticalGaps = new ushort[num1 * num2];
            ushort[] sizesOfHorizontalGaps = new ushort[num1 * num2];
            int num4 = 0;
            int num5 = 0;
            while (num4 < num1)
            {
                for (int index2 = 0; index2 < num2; ++index2)
                    sizesOfVerticalGaps[num5 + index2] = sizesOfHorizontalGaps[num5 + index2] = (ushort)1;
                ++num4;
                num5 += num2;
            }
            Cell cell = smithWatermanGotoh.Construct(s1, s2, scores, o, e, pointers, sizesOfVerticalGaps, sizesOfHorizontalGaps);
            Alignment alignment = smithWatermanGotoh.Traceback(s1, s2, matrix, pointers, cell, sizesOfVerticalGaps, sizesOfHorizontalGaps);
            alignment.Name1 = s1.Id;
            alignment.Name2 = s2.Id;
            alignment.Matrix = matrix;
            alignment.Open = o;
            alignment.Extend = e;
            alignment.OriginalSequence1 = s1;
            alignment.OriginalSequence2 = s2;
            return alignment;
        }

        public static CompactAlignment CompactAligner(Sequence s1, Sequence s2, Matrix matrix, float o, float e)
        {
            return new CompactAlignment(Align(s1, s2, matrix, o, e));
        }

        private Cell Construct(Sequence s1, Sequence s2, float[,] matrix, float o, float e, byte[] pointers, ushort[] sizesOfVerticalGaps, ushort[] sizesOfHorizontalGaps)
        {
            char[] chArray1 = s1.ToArray();
            char[] chArray2 = s2.ToArray();
            int num1 = s1.Length + 1;
            int length = s2.Length + 1;
            float[] numArray1 = new float[length];
            float[] numArray2 = new float[length];
            numArray1[0] = float.NegativeInfinity;
            numArray2[0] = 0.0f;
            for (int index = 1; index < length; ++index)
            {
                numArray1[index] = float.NegativeInfinity;
                numArray2[index] = 0.0f;
            }
            Cell cell = new Cell();
            int row = 1;
            int num2 = length;
            while (row < num1)
            {
                float c = float.NegativeInfinity;
                float num3 = numArray2[0];
                int column = 1;
                int index = num2 + 1;
                while (column < length)
                {
                    float num4 = matrix[(int)chArray1[row - 1], (int)chArray2[column - 1]];
                    float a = num3 + num4;
                    float num5 = numArray1[column] - e;
                    float num6 = numArray2[column] - o;
                    if ((double)num5 > (double)num6)
                    {
                        numArray1[column] = num5;
                        sizesOfVerticalGaps[index] = (ushort)((uint)sizesOfVerticalGaps[index - length] + 1U);
                    }
                    else
                        numArray1[column] = num6;
                    float num7 = c - e;
                    float num8 = numArray2[column - 1] - o;
                    if ((double)num7 > (double)num8)
                    {
                        c = num7;
                        sizesOfHorizontalGaps[index] = (ushort)((uint)sizesOfHorizontalGaps[index - 1] + 1U);
                    }
                    else
                        c = num8;
                    num3 = numArray2[column];
                    numArray2[column] = Max(a, numArray1[column], c, 0.0f);
                    pointers[index] = (double)numArray2[column] != 0.0 ? ((double)numArray2[column] != (double)a ? ((double)numArray2[column] != (double)numArray1[column] ? (byte)1 : (byte)3) : (byte)2) : (byte)0;
                    if ((double)numArray2[column] > (double)cell.Score)
                        cell.Set(row, column, numArray2[column]);
                    ++column;
                    ++index;
                }
                ++row;
                num2 += length;
            }
            return cell;
        }

        private Alignment Traceback(Sequence s1, Sequence s2, Matrix m, byte[] pointers, Cell cell, ushort[] sizesOfVerticalGaps, ushort[] sizesOfHorizontalGaps)
        {
            char[] chArray1 = s1.ToArray();
            char[] chArray2 = s2.ToArray();
            float[,] scores = m.Scores;
            int num1 = s2.Length + 1;
            Alignment alignment = new Alignment();
            alignment.Score = cell.Score;
            int length = s1.Length + s2.Length;
            char[] a1 = new char[length];
            char[] a2 = new char[length];
            char[] a3 = new char[length];
            int len1 = 0;
            int len2 = 0;
            int len3 = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int row = cell.Row;
            int column = cell.Column;
            int num5 = row * num1;
            bool flag = true;
            while (flag)
            {
                switch (pointers[num5 + column])
                {
                    case (byte)0:
                        flag = false;
                        break;
                    case (byte)1:
                        int num6 = 0;
                        for (int index = (int)sizesOfHorizontalGaps[num5 + column]; num6 < index; ++num6)
                        {
                            a1[len1++] = '-';
                            a2[len2++] = chArray2[--column];
                            a3[len3++] = ' ';
                            ++num4;
                        }
                        break;
                    case (byte)2:
                        char ch1 = chArray1[--row];
                        char ch2 = chArray2[--column];
                        num5 -= num1;
                        a1[len1++] = ch1;
                        a2[len2++] = ch2;
                        if ((int)ch1 == (int)ch2)
                        {
                            a3[len3++] = '|';
                            ++num2;
                            ++num3;
                            break;
                        }
                        else if ((double)scores[(int)ch1, (int)ch2] > 0.0)
                        {
                            a3[len3++] = ':';
                            ++num3;
                            break;
                        }
                        else
                        {
                            a3[len3++] = '.';
                            break;
                        }
                    case (byte)3:
                        int num7 = 0;
                        for (int index = (int)sizesOfVerticalGaps[num5 + column]; num7 < index; ++num7)
                        {
                            a1[len1++] = chArray1[--row];
                            a2[len2++] = '-';
                            a3[len3++] = ' ';
                            num5 -= num1;
                            ++num4;
                        }
                        break;
                }
            }
            alignment.Sequence1 = Reverse(a1, len1);
            alignment.Start1 = row;
            alignment.Sequence2 = Reverse(a2, len2);
            alignment.Start2 = column;
            alignment.MarkupLine = Reverse(a3, len3);
            alignment.Identity = num2;
            alignment.Gaps = num4;
            alignment.Similarity = num3;
            return alignment;
        }

        private static float Max(float a, float b, float c, float d)
        {
            return Math.Max(Math.Max(a, b), Math.Max(c, d));
        }

        private static char[] Reverse(char[] a, int len)
        {
            char[] chArray = new char[len];
            int index1 = len - 1;
            int index2 = 0;
            while (index1 >= 0)
            {
                chArray[index2] = a[index1];
                --index1;
                ++index2;
            }
            return chArray;
        }

        [Serializable]
        public class Alignment
        {
            public const char GAP = '-';
            private const string DEFAULT_SEQUENCE1_NAME = "pepAlign_1";
            private const string DEFAULT_SEQUENCE2_NAME = "pepAlign_2";
            private Matrix matrix;
            private float open;
            private float extend;
            private float score;
            private char[] sequence1;
            private string name1;
            private int start1;
            private char[] sequence2;
            private string name2;
            private int start2;
            private char[] markupLine;
            private int identity;
            private int similarity;
            private int gaps;

            public Sequence OriginalSequence1 { get; set; }

            public Sequence OriginalSequence2 { get; set; }

            public virtual float Extend
            {
                get
                {
                    return this.extend;
                }
                set
                {
                    this.extend = value;
                }
            }

            public virtual Matrix Matrix
            {
                get
                {
                    return this.matrix;
                }
                set
                {
                    this.matrix = value;
                }
            }

            public virtual string Name1
            {
                get
                {
                    return this.name1 == null || this.name1.Length == 0 ? "pepAlign_1" : this.name1;
                }
                set
                {
                    this.name1 = value;
                }
            }

            public virtual string Name2
            {
                get
                {
                    return this.name2 == null || this.name2.Length == 0 ? "pepAlign_2" : this.name2;
                }
                set
                {
                    this.name2 = value;
                }
            }

            public virtual float Open
            {
                get
                {
                    return this.open;
                }
                set
                {
                    this.open = value;
                }
            }

            public virtual float Score
            {
                get
                {
                    return this.score;
                }
                set
                {
                    this.score = value;
                }
            }

            public virtual char[] Sequence1
            {
                get
                {
                    return this.sequence1;
                }
                set
                {
                    this.sequence1 = value;
                }
            }

            public virtual char[] Sequence2
            {
                get
                {
                    return this.sequence2;
                }
                set
                {
                    this.sequence2 = value;
                }
            }

            public virtual int Start1
            {
                get
                {
                    return this.start1;
                }
                set
                {
                    this.start1 = value;
                }
            }

            public virtual int Start2
            {
                get
                {
                    return this.start2;
                }
                set
                {
                    this.start2 = value;
                }
            }

            public virtual int Gaps
            {
                get
                {
                    return this.gaps;
                }
                set
                {
                    this.gaps = value;
                }
            }

            public virtual int Identity
            {
                get
                {
                    return this.identity;
                }
                set
                {
                    this.identity = value;
                }
            }

            public virtual char[] MarkupLine
            {
                get
                {
                    return this.markupLine;
                }
                set
                {
                    this.markupLine = value;
                }
            }

            public virtual int Similarity
            {
                get
                {
                    return this.similarity;
                }
                set
                {
                    this.similarity = value;
                }
            }

            public virtual string Summary
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    int length = this.Sequence1.Length;
                    stringBuilder.Append("Sequence #1: " + this.Name1);
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Sequence #2: " + this.Name2);
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Matrix: " + (this.matrix.Id == null ? "" : this.matrix.Id));
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Gap open: " + (object)this.open);
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Gap extend: " + (object)this.extend);
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Length: " + (object)length);
                    stringBuilder.Append(Commons.LineSeparator);
                    double num1 = Math.Round((double)this.identity / (double)length, 3);
                    stringBuilder.Append("Identity: " + (object)this.identity + "/" + (string)(object)length + " (" + (string)(object)num1 + ")");
                    stringBuilder.Append(Commons.LineSeparator);
                    double num2 = Math.Round((double)this.similarity / (double)length, 3);
                    stringBuilder.Append("Similarity: " + (object)this.similarity + "/" + (string)(object)length + " (" + (string)(object)num2 + ")");
                    stringBuilder.Append(Commons.LineSeparator);
                    double num3 = Math.Round((double)this.gaps / (double)length, 3);
                    stringBuilder.Append("Gaps: " + (object)this.gaps + "/" + (string)(object)length + " (" + (string)(object)num3 + ")");
                    stringBuilder.Append(Commons.LineSeparator);
                    stringBuilder.Append("Score: " + (object)Math.Round((double)this.score, 2));
                    stringBuilder.Append(Commons.LineSeparator);
                    return ((object)stringBuilder).ToString();
                }
            }
        }

        [Serializable]
        public class Sequence
        {
            private string id = (string)null;
            private string description = (string)null;
            private SequenceType type = SequenceType.Protein;
            private string aalist;

            public virtual string Id
            {
                get
                {
                    return this.id;
                }
                set
                {
                    this.id = value;
                }
            }

            public virtual string Description
            {
                get
                {
                    return this.description;
                }
                set
                {
                    this.description = value;
                }
            }

            public virtual SequenceType Type
            {
                get
                {
                    return this.type;
                }
                set
                {
                    this.type = value;
                }
            }

            public virtual string AAList
            {
                get
                {
                    return this.aalist;
                }
                set
                {
                    this.aalist = value;
                }
            }

            public virtual int Length
            {
                get
                {
                    return this.aalist.Length;
                }
            }

            public Sequence()
            {
            }

            public Sequence(string sequence, string id, string description, SequenceType type)
            {
                this.aalist = sequence;
                this.id = id;
                this.description = description;
                this.type = type;
            }

            public virtual string Subsequence(int index, int length)
            {
                return this.aalist.Substring(index, index + length - index);
            }

            public virtual char AcidAt(int index)
            {
                return this.aalist[index];
            }

            public virtual char[] ToArray()
            {
                return this.aalist.ToCharArray();
            }

            public static Sequence Parse(string stringToParse)
            {
                stringToParse = stringToParse.Replace("\r\n", "\n");
                string id = (string)null;
                string description = (string)null;
                if (stringToParse.StartsWith(">"))
                {
                    int startIndex = stringToParse.IndexOf("\n");
                    if (startIndex == -1)
                        throw new Exception("Invalid sequence");
                    string str = stringToParse.Substring(1, startIndex - 1);
                    stringToParse = stringToParse.Substring(startIndex);
                    int length = 0;
                    int index = 0;
                    while (index < str.Length && (int)str[index] != 32 && (int)str[index] != 9)
                    {
                        ++index;
                        ++length;
                    }
                    id = str.Substring(0, length);
                    description = length + 1 > str.Length ? "" : str.Substring(length + 1);
                }
                return new Sequence(Sequence.PrepareAndValidate(stringToParse), id, description, SequenceType.Protein);
            }

            public static Sequence Parse(FileInfo file)
            {
                string id = (string)null;
                string description = (string)null;
                StreamReader streamReader = new StreamReader(file.FullName);
                StringBuilder stringBuilder = new StringBuilder();
                string stringToParse1 = streamReader.ReadLine();
                if (stringToParse1.StartsWith(">"))
                {
                    string str = stringToParse1.Substring(1).Trim();
                    int length = 0;
                    int index = 0;
                    while (index < str.Length && (int)str[index] != 32 && (int)str[index] != 9)
                    {
                        ++index;
                        ++length;
                    }
                    id = str.Substring(0, length);
                    description = length + 1 > str.Length ? "" : str.Substring(length + 1);
                }
                else
                    stringBuilder.Append(Sequence.PrepareAndValidate(stringToParse1));
                string stringToParse2;
                while ((stringToParse2 = streamReader.ReadLine()) != null)
                    stringBuilder.Append(Sequence.PrepareAndValidate(stringToParse2));
                streamReader.Close();
                return new Sequence(((object)stringBuilder).ToString(), id, description, SequenceType.Protein);
            }

            private static string PrepareAndValidate(string stringToParse)
            {
                StringBuilder stringBuilder = new StringBuilder();
                string str = stringToParse.Trim().ToUpper();
                int index = 0;
                for (int length = str.Length; index < length; ++index)
                {
                    switch (str[index])
                    {
                        case '\t':
                        case '\n':
                        case '\r':
                        case ' ':
                            goto case '\t';
                        case '*':
                        case '-':
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                        case 'G':
                        case 'H':
                        case 'I':
                        case 'K':
                        case 'L':
                        case 'M':
                        case 'N':
                        case 'P':
                        case 'Q':
                        case 'R':
                        case 'S':
                        case 'T':
                        case 'U':
                        case 'V':
                        case 'W':
                        case 'X':
                        case 'Y':
                        case 'Z':
                            stringBuilder.Append(str[index]);
                            goto case '\t';
                        //default:
                        //throw new NAlignerException("Invalid sequence character: " + (object)str[index]);
                    }
                }
                return ((object)stringBuilder).ToString();
            }
        }

        public enum SequenceType
        {
            Protein,
            Nucleic,
        }

        [Serializable]
        public class Matrix
        {
            private string id = (string)null;
            private float[,] scores = (float[,])null;
            private const char COMMENT_STARTER = '#';
            private const int SIZE = 127;

            public virtual string Id
            {
                get
                {
                    return this.id;
                }
            }

            public virtual float[,] Scores
            {
                get
                {
                    return this.scores;
                }
            }

            public Matrix(string id, float[,] scores)
            {
                this.id = id;
                this.scores = scores;
            }

            public static Matrix Load(FileInfo matrixPath)
            {
                StreamReader streamReader = new StreamReader(matrixPath.FullName);
                string matrixString = streamReader.ReadToEnd();
                streamReader.Close();
                return Matrix.Load(matrixString, matrixPath.FullName);
            }

            public static Matrix Load(string matrixString, string matrixName)
            {
                char[] chArray = new char[(int)sbyte.MaxValue];
                for (int index = 0; index < (int)sbyte.MaxValue; ++index)
                    chArray[index] = char.MinValue;
                float[,] scores = new float[(int)sbyte.MaxValue, (int)sbyte.MaxValue];
                StringReader stringReader = new StringReader(matrixString);
                string str1;
                do
                    ;
                while ((str1 = stringReader.ReadLine()) != null && (int)str1.Trim()[0] == 35);
                Tokenizer tokenizer1 = new Tokenizer(str1.Trim());
                int index1 = 0;
                while (tokenizer1.HasMoreTokens())
                {
                    chArray[index1] = tokenizer1.NextToken()[0];
                    ++index1;
                }
                string str2;
                while ((str2 = stringReader.ReadLine()) != null)
                {
                    Tokenizer tokenizer2 = new Tokenizer(str2.Trim());
                    char ch = tokenizer2.NextToken()[0];
                    for (int index2 = 0; index2 < (int)sbyte.MaxValue; ++index2)
                    {
                        if ((int)chArray[index2] != 0)
                            scores[(int)ch, (int)chArray[index2]] = float.Parse(tokenizer2.NextToken());
                    }
                }
                stringReader.Close();
                return new Matrix(matrixName, scores);
            }

            public virtual float GetScore(char a, char b)
            {
                return this.scores[(int)a, (int)b];
            }
        }

        public class Cell
        {
            private int row;
            private int column;
            private float score;

            public virtual int Column
            {
                get
                {
                    return this.column;
                }
                set
                {
                    this.column = value;
                }
            }

            public virtual int Row
            {
                get
                {
                    return this.row;
                }
                set
                {
                    this.row = value;
                }
            }

            public virtual float Score
            {
                get
                {
                    return this.score;
                }
                set
                {
                    this.score = value;
                }
            }

            public Cell()
            {
                this.row = 0;
                this.column = 0;
                this.score = float.NegativeInfinity;
            }

            public virtual void Set(int row, int column, float score)
            {
                this.row = row;
                this.column = column;
                this.score = score;
            }
        }

        public abstract class Commons
        {
            private static string userDirectory = ".";
            private static string fileSeparator = "/";
            private static string lineSeparator = "\r\n";
            private const string CURRENT_RELEASE = "0.9";
            private const string DEFAULT_USER_DIRECTORY = ".";
            private const string DEFAULT_FILE_SEPARATOR = "/";
            private const string DEFAULT_LINE_SEPARATOR = "\r\n";

            public static string FileSeparator
            {
                get
                {
                    return Commons.fileSeparator;
                }
            }

            public static string LineSeparator
            {
                get
                {
                    return Commons.lineSeparator;
                }
            }

            public static string UserDirectory
            {
                get
                {
                    return Commons.userDirectory;
                }
            }

            public static string CurrentRelease
            {
                get
                {
                    return "0.9";
                }
            }

            static Commons()
            {
                try
                {
                    Commons.userDirectory = Environment.CurrentDirectory;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed getting user current directory: " + ((object)ex).ToString());
                }
                try
                {
                    Commons.fileSeparator = Path.DirectorySeparatorChar.ToString();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed getting system file separator: " + ((object)ex).ToString());
                }
                try
                {
                    Commons.lineSeparator = Environment.NewLine;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed getting system line separator: " + ((object)ex).ToString());
                }
            }
        }

        //public class Tokenizer : IEnumerator
        //{
        //    private long currentPos = 0L;
        //    private bool includeDelims = false;
        //    private char[] chars = (char[])null;
        //    private string delimiters = " \t\n\r\f";

        //    public int Count
        //    {
        //        get
        //        {
        //            long num1 = this.currentPos;
        //            int num2 = 0;
        //            try
        //            {
        //                while (true)
        //                {
        //                    this.NextToken();
        //                    ++num2;
        //                }
        //            }
        //            catch (ArgumentOutOfRangeException ex)
        //            {
        //                this.currentPos = num1;
        //                return num2;
        //            }
        //        }
        //    }

        //    public object Current
        //    {
        //        get
        //        {
        //            return (object)this.NextToken();
        //        }
        //    }

        //    public Tokenizer(string source)
        //    {
        //        this.chars = source.ToCharArray();
        //    }

        //    public Tokenizer(string source, string delimiters)
        //        : this(source)
        //    {
        //        this.delimiters = delimiters;
        //    }

        //    public Tokenizer(string source, string delimiters, bool includeDelims)
        //        : this(source, delimiters)
        //    {
        //        this.includeDelims = includeDelims;
        //    }

        //    public string NextToken()
        //    {
        //        return this.NextToken(this.delimiters);
        //    }

        //    public string NextToken(string delimiters)
        //    {
        //        this.delimiters = delimiters;
        //        if (this.currentPos == (long)this.chars.Length)
        //            throw new ArgumentOutOfRangeException();
        //        if (Array.IndexOf<char>(delimiters.ToCharArray(), this.chars[this.currentPos]) != -1 && this.includeDelims)
        //            return string.Concat((object)this.chars[this.currentPos++]);
        //        else
        //            return this.nextToken(delimiters.ToCharArray());
        //    }

        //    private string nextToken(char[] delimiters)
        //    {
        //        string str = "";
        //        long num = this.currentPos;
        //        while (Array.IndexOf<char>(delimiters, this.chars[this.currentPos]) != -1)
        //        {
        //            if (++this.currentPos == (long)this.chars.Length)
        //            {
        //                this.currentPos = num;
        //                throw new ArgumentOutOfRangeException();
        //            }
        //        }
        //        while (Array.IndexOf<char>(delimiters, this.chars[this.currentPos]) == -1)
        //        {
        //            str = str + (object)this.chars[this.currentPos];
        //            if (++this.currentPos == (long)this.chars.Length)
        //                break;
        //        }
        //        return str;
        //    }

        //    public bool HasMoreTokens()
        //    {
        //        long num = this.currentPos;
        //        try
        //        {
        //            this.NextToken();
        //        }
        //        catch (ArgumentOutOfRangeException ex)
        //        {
        //            return false;
        //        }
        //        finally
        //        {
        //            this.currentPos = num;
        //        }
        //        return true;
        //    }

        //    public bool MoveNext()
        //    {
        //        return this.HasMoreTokens();
        //    }

        //    public void Reset()
        //    {
        //    }
        //}

        [Serializable]
        public struct CompactAlignment
        {
            public double Score;
            public string Identifier;
            public short Start;
            public short Length;

            public CompactAlignment(Alignment alignment)
            {
                this.Score = (double)alignment.Score;
                this.Identifier = alignment.OriginalSequence1.Id;
                this.Start = (short)alignment.Start1;
                this.Length = (short)alignment.OriginalSequence2.Length;
            }
        }

    }
}
