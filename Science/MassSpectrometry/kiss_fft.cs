//-----------------------------------------------------------------------
// Copyright 2018 Eli Lilly and Company
//
// Licensed under the Apache License, Version 2.0 (the "License");
//
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

#region copyright
/**
 * Project Title: Isotopic distribution Calc
 * Description: Isotopic Distribution Calculator
 * Copyright: Copyright (C) 2006/2007
 * Company: Eli Lilly and Company
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Kiss_fft_cfg = MassSpectrometry.Kiss_fft_state;

namespace MassSpectrometry
{

    internal class kiss_fft_c
    {
        private static kiss_fft_cpx[] _scratchbuf = null;
        private static int _nscratchbuf = 0;
        private static kiss_fft_cpx[] _tmpbuf = null;
        private static int _ntmpbuf = 0;
        private const double _pi = 3.14159265358979323846264338327;

        private static void Kf_bfly2(ref kiss_fft_cpx[] Fout, int fstride, Kiss_fft_state st, int m, int foutPointer)
        {
            int fout1Idx = foutPointer;
            int fout2Idx = foutPointer + m;
            int tw1Idx = 0;
            kiss_fft_cpx t;
            t.i = 0;
            t.r = 0;
            do
            {
                C_FIXDIV(ref Fout[fout1Idx], 2); C_FIXDIV(ref Fout[fout2Idx], 2);
                C_MUL(ref t, Fout[fout2Idx], st.twiddles[tw1Idx]);

                tw1Idx += fstride;

                C_SUB(ref Fout[fout2Idx], Fout[fout1Idx], t);
                C_ADDTO(ref Fout[fout1Idx], t);
                ++fout2Idx;
                ++fout1Idx;
            } while (--m > 0);
        }

        private static void Kf_bfly3(ref kiss_fft_cpx[] Fout, int fstride, Kiss_fft_cfg st, int m, int foutPointer)
        {
            int k = m;
            int m2 = 2 * m;
            int tw1Idx, tw2Idx;
            kiss_fft_cpx[] scratch = new kiss_fft_cpx[5];
            int epi3 = fstride * m;
            tw1Idx = tw2Idx = 0;
            do
            {
                C_FIXDIV(ref Fout[foutPointer], 3); C_FIXDIV(ref Fout[m], 3); C_FIXDIV(ref Fout[m2], 3);

                C_MUL(ref scratch[1], Fout[m], st.twiddles[tw1Idx]);
                C_MUL(ref scratch[2], Fout[m2], st.twiddles[tw2Idx]);

                C_ADD(ref scratch[3], scratch[1], scratch[2]);
                C_SUB(ref scratch[0], scratch[1], scratch[2]);
                tw1Idx += fstride;
                tw2Idx += fstride * 2;

                Fout[m].r = Fout[foutPointer].r - HALF_OF(scratch[3].r);
                Fout[m].i = Fout[foutPointer].i - HALF_OF(scratch[3].i);

                C_MULBYSCALAR(ref scratch[0], st.twiddles[epi3].i);

                C_ADDTO(ref Fout[foutPointer], scratch[3]);

                Fout[m2].r = Fout[m].r + scratch[0].i;
                Fout[m2].i = Fout[m].i - scratch[0].r;

                Fout[m].r -= scratch[0].i;
                Fout[m].i += scratch[0].r;

                ++foutPointer;
            } while (--k > 0);
        }

        private static void Kf_bfly4(ref kiss_fft_cpx[] Fout, int fstride, Kiss_fft_state st, int m, int foutPointer)
        {
            kiss_fft_cpx[] scratch = new kiss_fft_cpx[6];

            int k = m;

            int m2 = 2 * m + foutPointer;
            int m3 = 3 * m + foutPointer;
            m = m + foutPointer;

            int tw3Idx, tw2Idx, tw1Idx;
            tw3Idx = tw2Idx = tw1Idx = 0;
            int FoutIdx = foutPointer;
            do
            {
                C_FIXDIV(ref Fout[FoutIdx], 4); C_FIXDIV(ref Fout[m], 4); C_FIXDIV(ref Fout[m2], 4); C_FIXDIV(ref Fout[m3], 4);

                C_MUL(ref scratch[0], Fout[m], st.twiddles[tw1Idx]);
                C_MUL(ref scratch[1], Fout[m2], st.twiddles[tw2Idx]);
                C_MUL(ref scratch[2], Fout[m3], st.twiddles[tw3Idx]);

                C_SUB(ref scratch[5], Fout[FoutIdx], scratch[1]);
                C_ADDTO(ref Fout[FoutIdx], scratch[1]);
                C_ADD(ref scratch[3], scratch[0], scratch[2]);
                C_SUB(ref scratch[4], scratch[0], scratch[2]);
                C_SUB(ref Fout[m2], Fout[FoutIdx], scratch[3]);
                tw1Idx += fstride;
                tw2Idx += fstride * 2;
                tw3Idx += fstride * 3;
                C_ADDTO(ref Fout[FoutIdx], scratch[3]);

                if (st.inverse > 0)
                {
                    Fout[m].r = scratch[5].r - scratch[4].i;
                    Fout[m].i = scratch[5].i + scratch[4].r;
                    Fout[m3].r = scratch[5].r + scratch[4].i;
                    Fout[m3].i = scratch[5].i - scratch[4].r;
                }
                else
                {
                    Fout[m].r = scratch[5].r + scratch[4].i;
                    Fout[m].i = scratch[5].i - scratch[4].r;
                    Fout[m3].r = scratch[5].r - scratch[4].i;
                    Fout[m3].i = scratch[5].i + scratch[4].r;
                }
                ++FoutIdx;
                ++m;
                ++m2;
                ++m3;
            } while (--k > 0);
        }

        private static void Kf_bfly5(ref kiss_fft_cpx[] Fout, int fstride, Kiss_fft_cfg st, int m, int foutPointer)
        {
            int Fout0Idx, Fout1Idx, Fout2Idx, Fout3Idx, Fout4Idx;
            int u;
            kiss_fft_cpx[] scratch = new kiss_fft_cpx[13];

            int ya = fstride * m;
            int yb = fstride * 2 * m;

            Fout0Idx = foutPointer;
            Fout1Idx = Fout0Idx + m;
            Fout2Idx = Fout0Idx + 2 * m;
            Fout3Idx = Fout0Idx + 3 * m;
            Fout4Idx = Fout0Idx + 4 * m;

            for (u = 0; u < m; ++u)
            {
                C_FIXDIV(ref Fout[Fout0Idx], 5); C_FIXDIV(ref Fout[Fout1Idx], 5); C_FIXDIV(ref Fout[Fout2Idx], 5); C_FIXDIV(ref Fout[Fout3Idx], 5); C_FIXDIV(ref Fout[Fout4Idx], 5);
                scratch[0] = Fout[Fout0Idx];

                C_MUL(ref scratch[1], Fout[Fout1Idx], st.twiddles[u * fstride]);
                C_MUL(ref scratch[2], Fout[Fout2Idx], st.twiddles[2 * u * fstride]);
                C_MUL(ref scratch[3], Fout[Fout3Idx], st.twiddles[3 * u * fstride]);
                C_MUL(ref scratch[4], Fout[Fout4Idx], st.twiddles[4 * u * fstride]);

                C_ADD(ref scratch[7], scratch[1], scratch[4]);
                C_SUB(ref scratch[10], scratch[1], scratch[4]);
                C_ADD(ref scratch[8], scratch[2], scratch[3]);
                C_SUB(ref scratch[9], scratch[2], scratch[3]);

                Fout[Fout0Idx].r += scratch[7].r + scratch[8].r;
                Fout[Fout0Idx].i += scratch[7].i + scratch[8].i;

                scratch[5].r = scratch[0].r + S_MUL(scratch[7].r, st.twiddles[ya].r) + S_MUL(scratch[8].r, st.twiddles[yb].r);
                scratch[5].i = scratch[0].i + S_MUL(scratch[7].i, st.twiddles[ya].r) + S_MUL(scratch[8].i, st.twiddles[yb].r);

                scratch[6].r = S_MUL(scratch[10].i, st.twiddles[ya].i) + S_MUL(scratch[9].i, st.twiddles[yb].i);
                scratch[6].i = -S_MUL(scratch[10].r, st.twiddles[ya].i) - S_MUL(scratch[9].r, st.twiddles[yb].i);

                C_SUB(ref Fout[Fout1Idx], scratch[5], scratch[6]);
                C_ADD(ref Fout[Fout4Idx], scratch[5], scratch[6]);

                scratch[11].r = scratch[0].r + S_MUL(scratch[7].r, st.twiddles[yb].r) + S_MUL(scratch[8].r, st.twiddles[ya].r);
                scratch[11].i = scratch[0].i + S_MUL(scratch[7].i, st.twiddles[yb].r) + S_MUL(scratch[8].i, st.twiddles[ya].r);
                scratch[12].r = -S_MUL(scratch[10].i, st.twiddles[yb].i) + S_MUL(scratch[9].i, st.twiddles[ya].i);
                scratch[12].i = S_MUL(scratch[10].r, st.twiddles[yb].i) - S_MUL(scratch[9].r, st.twiddles[ya].i);

                C_ADD(ref Fout[Fout2Idx], scratch[11], scratch[12]);
                C_SUB(ref Fout[Fout3Idx], scratch[11], scratch[12]);

                ++Fout0Idx; ++Fout1Idx; ++Fout2Idx; ++Fout3Idx; ++Fout4Idx;
            }
        }

        private static void Kf_factor(int n, ref int[] facbuf)
        {
            int p = 4;
            double floor_sqrt;
            floor_sqrt = Math.Floor(Math.Sqrt((double)n));
            int i = 0;
            /*factor out powers of 4, powers of 2, then any remaining primes */
            do
            {
                while (n % p > 0)
                {
                    switch (p)
                    {
                        case 4: p = 2; break;
                        case 2: p = 3; break;
                        default: p += 2; break;
                    }
                    if (p > floor_sqrt)
                        p = n;          /* no more factors, skip to end */
                }
                n /= p;
                facbuf[i++] = p;
                facbuf[i++] = n;
            } while (n > 1);

        }

        /*
         *
         * User-callable function to allocate all necessary storage space for the fft.
         *
         * The return value is a contiguous block of memory, allocated with malloc.  As such,
         * It can be freed with free(), rather than a kiss_fft-specific function.
         * */
        internal static Kiss_fft_cfg Kiss_fft_alloc(int nfft, int inverse_fft)//Kiss_fft_cfg Kiss_fft_alloc(int nfft,int inverse_fft,void * mem,size_t * lenmem )
        {
            Kiss_fft_cfg st = new Kiss_fft_cfg();
            st.factors = new int[2 * ConstVar.MAXFACTORS];
            st.twiddles = new List<kiss_fft_cpx>();

            int i;
            st.nfft = nfft;
            st.inverse = inverse_fft;

            for (i = 0; i < nfft; ++i)
            {
                double phase = (-2 * _pi / nfft) * i;
                if (st.inverse > 0)
                    phase *= -1;
                kiss_fft_cpx cpx = new kiss_fft_cpx();
                kf_cexp(ref cpx, phase);
                st.twiddles.Add(cpx);
            }

            Kf_factor(nfft, ref st.factors);
            return st;
        }


        /* perform the butterfly for one stage of a mixed radix FFT */
        private static void Kf_bfly_generic(
                ref kiss_fft_cpx[] Fout,
                int fstride,
        Kiss_fft_cfg st,
        int m,
        int p,
        int foutPointer
                )
        {
            int u, k, q1, q;
            kiss_fft_cpx t = new kiss_fft_cpx();
            int Norig = st.nfft;

            CHECKBUF(ref _scratchbuf, ref _nscratchbuf, p);

            for (u = 0; u < m; ++u)
            {
                k = u;
                for (q1 = 0; q1 < p; ++q1)
                {
                    _scratchbuf[q1] = Fout[k];
                    C_FIXDIV(ref _scratchbuf[q1], p);
                    k += m;
                }

                k = u;
                for (q1 = 0; q1 < p; ++q1)
                {
                    int twidx = 0;
                    Fout[k] = _scratchbuf[0];
                    for (q = 1; q < p; ++q)
                    {
                        twidx += fstride * k;
                        if (twidx >= Norig) twidx -= Norig;
                        C_MUL(ref t, _scratchbuf[q], st.twiddles[twidx]);
                        C_ADDTO(ref Fout[k], t);
                    }
                    k += m;
                }
            }
        }

        private static void Kf_work(ref kiss_fft_cpx[] Fout, kiss_fft_cpx[] f, int fstride, int in_stride,
            int[] factors, Kiss_fft_cfg st, int factorsPointer, int foutPointer, int fPointer)
        {
            //Debug.Indent(); Debug.WriteLine(string.Format("factorsPointer = {0}", factorsPointer));
            int Fout_beg = foutPointer;
            int p = factors[factorsPointer++]; /* the radix  */
            int m = factors[factorsPointer++]; /* stage's fft length/p */
            int Fout_end = foutPointer + p * m;
            //Debug.WriteLine(string.Format("++ factorsPointer = {0}", factorsPointer));
            if (m == 1)
            {
                do
                {
                    Fout[foutPointer] = f[fPointer];
                    fPointer += fstride * in_stride;
                } while (++foutPointer != Fout_end);
            }
            else
            {
                do
                {
                    Kf_work(ref Fout, f, fstride * p, in_stride, factors, st, factorsPointer, foutPointer, fPointer);
                    fPointer += fstride * in_stride;
                } while ((foutPointer += m) != Fout_end);
            }
            foutPointer = Fout_beg;
            //Qmass qm = new Qmass();
            //qm.DebugPrint(Fout);
            //Debug.WriteLine("==============================================");
            switch (p)
            {
                case 2:
                    Kf_bfly2(ref Fout, fstride, st, m, foutPointer);
                    break;
                case 3:
                    Kf_bfly3(ref Fout, fstride, st, m, foutPointer);
                    break;
                case 4:
                    Kf_bfly4(ref Fout, fstride, st, m, foutPointer);
                    break;
                case 5:
                    Kf_bfly5(ref Fout, fstride, st, m, foutPointer);
                    break;
                default:
                    Kf_bfly_generic(ref Fout, fstride, st, m, p, foutPointer);
                    break;
            }
        }

        private static void Kiss_fft_stride(Kiss_fft_cfg st, kiss_fft_cpx[] fin, ref kiss_fft_cpx[] fout, int in_stride)
        {
            if (fin.Equals(fout))
            {
                CHECKBUF(ref _tmpbuf, ref _ntmpbuf, st.nfft);
                Kf_work(ref _tmpbuf, fin, 1, in_stride, st.factors, st, 0, 0, 0);
                _tmpbuf.CopyTo(fout, 0);
            }
            else
            {
                Kf_work(ref fout, fin, 1, in_stride, st.factors, st, 0, 0, 0);
            }
        }

        internal static void Kiss_fft(Kiss_fft_cfg cfg, kiss_fft_cpx[] fin, ref kiss_fft_cpx[] fout)
        {
            Kiss_fft_stride(cfg, fin, ref fout, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="div"></param>
        ///
        private static double HALF_OF(double x)
        {
            return (x) * 0.5;
        }

        private static double S_MUL(double a, double b)
        {
            return a * b;
        }

        private static void C_MULBYSCALAR(ref kiss_fft_cpx c, double s)
        {
            c.r *= s;
            c.i *= s;
        }

        private static void CHECKBUF(ref kiss_fft_cpx[] buf, ref int nbuf, int n)
        {
            if (nbuf < n)
            {
                buf = new kiss_fft_cpx[n];
                nbuf = n;
            }
        }

        private static void kf_cexp(ref kiss_fft_cpx x, double phase)
        {
            x.r = Math.Cos(phase);
            x.i = Math.Sin(phase);
        }


        private static void C_FIXDIV(ref kiss_fft_cpx c, int div)
        {
            //NOOP
        }

        private static void C_MUL(ref kiss_fft_cpx m, kiss_fft_cpx a, kiss_fft_cpx b)
        {
            (m).r = (a).r * (b).r - (a).i * (b).i;
            (m).i = (a).r * (b).i + (a).i * (b).r;
        }

        private static void C_SUB(ref kiss_fft_cpx res, kiss_fft_cpx a, kiss_fft_cpx b)
        {
            CHECK_OVERFLOW_OP_Subtration((a).r, (b).r);
            CHECK_OVERFLOW_OP_Subtration((a).i, (b).i);
            (res).r = (a).r - (b).r;
            (res).i = (a).i - (b).i;
        }

        private static void C_ADDTO(ref kiss_fft_cpx res, kiss_fft_cpx a)
        {
            CHECK_OVERFLOW_OP_Addition((res).r, (a).r);
            CHECK_OVERFLOW_OP_Addition((res).i, (a).i);
            (res).r += (a).r;
            (res).i += (a).i;
        }
        private static void C_ADD(ref kiss_fft_cpx res, kiss_fft_cpx a, kiss_fft_cpx b)
        {
            CHECK_OVERFLOW_OP_Addition((a).r, (b).r);
            CHECK_OVERFLOW_OP_Addition((a).i, (b).i);
            (res).r = (a).r + (b).r; (res).i = (a).i + (b).i;
        }
        //Replace the method CHECK_OVERFLOW_OP(a,+,b) at line 98 of kiss_fft_guts.h
        private static void CHECK_OVERFLOW_OP_Addition(double a, double b)
        {
            //NOOP
        }

        //Replace the method CHECK_OVERFLOW_OP(a,-,b) at line 98 of kiss_fft_guts.h
        private static void CHECK_OVERFLOW_OP_Subtration(double a, double b)
        {
            //NOOP
        }
    }
}
