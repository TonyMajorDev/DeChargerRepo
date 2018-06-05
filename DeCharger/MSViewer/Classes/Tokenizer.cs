using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSViewer.Classes
{
    public class Tokenizer : IEnumerator
    {
        private long currentPos = 0L;
        private bool includeDelims = false;
        private char[] chars = (char[])null;
        private string delimiters = " \t\n\r\f";

        public int Count
        {
            get
            {
                long num1 = this.currentPos;
                int num2 = 0;
                try
                {
                    while (true)
                    {
                        this.NextToken();
                        ++num2;
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    this.currentPos = num1;
                    return num2;
                }
            }
        }

        public object Current
        {
            get
            {
                return (object)this.NextToken();
            }
        }

        public Tokenizer(string source)
        {
            this.chars = source.ToCharArray();
        }

        public Tokenizer(string source, string delimiters)
            : this(source)
        {
            this.delimiters = delimiters;
        }

        public Tokenizer(string source, string delimiters, bool includeDelims)
            : this(source, delimiters)
        {
            this.includeDelims = includeDelims;
        }

        public string NextToken()
        {
            return this.NextToken(this.delimiters);
        }

        public string NextToken(string delimiters)
        {
            this.delimiters = delimiters;
            if (this.currentPos == (long)this.chars.Length)
                throw new ArgumentOutOfRangeException();
            if (Array.IndexOf<char>(delimiters.ToCharArray(), this.chars[this.currentPos]) != -1 && this.includeDelims)
                return string.Concat((object)this.chars[this.currentPos++]);
            else
                return this.nextToken(delimiters.ToCharArray());
        }

        private string nextToken(char[] delimiters)
        {
            string str = "";
            long num = this.currentPos;
            while (Array.IndexOf<char>(delimiters, this.chars[this.currentPos]) != -1)
            {
                if (++this.currentPos == (long)this.chars.Length)
                {
                    this.currentPos = num;
                    throw new ArgumentOutOfRangeException();
                }
            }
            while (Array.IndexOf<char>(delimiters, this.chars[this.currentPos]) == -1)
            {
                str = str + (object)this.chars[this.currentPos];
                if (++this.currentPos == (long)this.chars.Length)
                    break;
            }
            return str;
        }

        public bool HasMoreTokens()
        {
            long num = this.currentPos;
            try
            {
                this.NextToken();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return false;
            }
            finally
            {
                this.currentPos = num;
            }
            return true;
        }

        public bool MoveNext()
        {
            return this.HasMoreTokens();
        }

        public void Reset()
        {
        }
    }
}
