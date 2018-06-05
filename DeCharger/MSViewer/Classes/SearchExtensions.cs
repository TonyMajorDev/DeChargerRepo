using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SearchExtensions
{
    static byte alignmentScore = new byte();
    static int bestScore = 0;
    static int bestScoreIndex = 0;
    static int matchPosition = 0;

    //static byte one = (byte)1;
    //static byte zero = (byte)0;

    public static bool Match(this byte[] seq1, byte[] seq2)
    {

        //int l1 = seq1.Length;
        //int l2 = seq2.Length;



        // fill dpMatrix and pointer matrix as Needleman Wunsch algorithmn
        // two different point are marked below



        for (int i = 0; i < seq2.Length - 3; i++)
        {
            alignmentScore = 0;

            for (int j = 0; j < seq1.Length; j++)
            {
                if (seq1[j] == seq2[i]) alignmentScore++;

                //alignmentScore += (seq1[j] == seq2[i]) ? one : zero;
                //alignmentScores[i] += (seq1[j] == seq2[i]) ? 1 : 0;
            }

            if (alignmentScore > bestScore)
            {
                bestScore = alignmentScore;
                bestScoreIndex = i;
            }
        }



        //for (int i = 0; i <= seq2.Length - 3; i++)  alignmentScores.Max();
        //matchPosition = alignmentScores[bestScore];

        return false; // bestScore != 0;
    }


    public unsafe static bool MatchNew(this byte[] seq1, byte[] seq2)
    {

        //int l1 = seq1.Length;
        //int l2 = seq2.Length;



        // fill dpMatrix and pointer matrix as Needleman Wunsch algorithmn
        // two different point are marked below

        fixed (byte* p1 = seq1, p2 = seq2)
        {
            byte* x1 = p1, x2 = p2;

            for (int i = 0; i < seq2.Length - 4; i++)
            {
                alignmentScore = 0;

                for (int j = 0; j < seq1.Length; j++)
                {
                    if (*(x1++) == *(x2)) alignmentScore++;
                    //if (seq1[j] == seq2[i]) alignmentScore++;

                    //alignmentScore += (seq1[j] == seq2[i]) ? one : zero;
                    //alignmentScores[i] += (seq1[j] == seq2[i]) ? 1 : 0;

                    //x1++;
                }

                x2++;
                x1 = p1;

                if (alignmentScore > bestScore)
                {
                    bestScore = alignmentScore;
                    bestScoreIndex = i;
                }
            }



            //for (int i = 0; i <= seq2.Length - 3; i++)  alignmentScores.Max();
            //matchPosition = alignmentScores[bestScore];

            return false; // bestScore != 0;
        }
    }

}

public class ProteinMatch
{
    public string Sequence { get; set; }
    public byte[] SequenceBytes { get; set; }

}

