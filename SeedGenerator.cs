
using System.Numerics;


namespace Project_Carlo
{
    public class SeedGenerator
    {
        UInt32[] basemessage = new uint[16];
        public SeedGenerator(UInt32[] basemessage)
        {
            this.basemessage = basemessage;
        }

        Vector<UInt32> mask1 = new Vector<UInt32>(0xFF00FF00U);
        Vector<UInt32> mask2 = new Vector<UInt32>(0x00FF00FFU);

        Vector<UInt64> LCGMulConst = new Vector<UInt64>(0x5D588B656C078965UL);
        Vector<UInt64> LCGAddConst = new Vector<UInt64>(0x269EC3UL);

        Vector<UInt32> ChangeEndian(Vector<UInt32> v)
        {
            v = (Vector.ShiftLeft(v, 8) & mask1) | (Vector.ShiftRightLogical(v, 8) & mask2);
            return (Vector.ShiftLeft(v, 16) | Vector.ShiftRightLogical(v, 16));
        }

        public UInt32[] calculate(UInt32[] timer0s, UInt32[] vcounts)
        {
            var result_length = timer0s.Length * vcounts.Length;
            var seeds = new UInt32[result_length];

            var simdsize = Vector<UInt32>.Count;
            var W = new Vector<uint>[16];

            var H0 = new Vector<UInt32>(0x67452301);
            var H1 = new Vector<UInt32>(0xEFCDAB89);
            var H2 = new Vector<UInt32>(0x98BADCFE);
            var H3 = new Vector<UInt32>(0x10325476);
            var H4 = new Vector<UInt32>(0xC3D2E1F0);

            var C0 = new Vector<UInt32>(0x5A827999);
            var C1 = new Vector<UInt32>(0x6ED9EBA1);
            var C2 = new Vector<UInt32>(0x8F1BBCDC);
            var C3 = new Vector<UInt32>(0xCA62C1D6);



            //Timer0
            for (int i = 0; i < timer0s.Length; i++)
            {
                for (int j = 0; j < vcounts.Length; j += simdsize)
                {
                    //VCount側でSIMDを適用して並列化を試みる
                    uint t = 0;
                    for (t = 0; t < basemessage.Length; t++) W[t] = new Vector<UInt32>(basemessage[t]);
                    W[0] = ChangeEndian(Vector.ShiftLeft(new Vector<UInt32>(timer0s[i]), 16) | new Vector<UInt32>(vcounts, j));

                    var A = new Vector<UInt32>(0x67452301);
                    var B = new Vector<UInt32>(0xEFCDAB89);
                    var C = new Vector<UInt32>(0x98BADCFE);
                    var D = new Vector<UInt32>(0x10325476);
                    var E = new Vector<UInt32>(0xC3D2E1F0);

                    for (t = 0; t < 16; t++)
                    {
                        var temp = (Vector.ShiftLeft(A, 5) | Vector.ShiftRightLogical(A, 27)) + ((B & C) | ((~B) & D)) + E + W[t] + C0;
                        E = D;
                        D = C;
                        C = Vector.ShiftLeft(B, 30) | Vector.ShiftRightLogical(B, 2);
                        B = A;
                        A = temp;
                    }
                    for (; t < 20; t++)
                    {
                        var w = W[(t - 3) & 0xF] ^ W[(t - 8) & 0xF] ^ W[(t - 14) & 0xF] ^ W[(t - 16) & 0xF];
                        W[t & 0xF] = Vector.ShiftLeft(w, 1) | Vector.ShiftRightLogical(w, 31);
                        var temp = (Vector.ShiftLeft(A, 5) | Vector.ShiftRightLogical(A, 27)) + ((B & C) | ((~B) & D)) + E + W[t & 0xF] + C0;
                        E = D;
                        D = C;
                        C = Vector.ShiftLeft(B, 30) | Vector.ShiftRightLogical(B, 2);
                        B = A;
                        A = temp;
                    }
                    for (; t < 40; t++)
                    {
                        var w = W[(t - 3) & 0xF] ^ W[(t - 8) & 0xF] ^ W[(t - 14) & 0xF] ^ W[(t - 16) & 0xF];
                        W[t & 0xF] = Vector.ShiftLeft(w, 1) | Vector.ShiftRightLogical(w, 31);
                        var temp = (Vector.ShiftLeft(A, 5) | Vector.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + W[t & 0xF] + C1;
                        E = D;
                        D = C;
                        C = Vector.ShiftLeft(B, 30) | Vector.ShiftRightLogical(B, 2);
                        B = A;
                        A = temp;
                    }
                    for (; t < 60; t++)
                    {
                        var w = W[(t - 3) & 0xF] ^ W[(t - 8) & 0xF] ^ W[(t - 14) & 0xF] ^ W[(t - 16) & 0xF];
                        W[t & 0xF] = Vector.ShiftLeft(w, 1) | Vector.ShiftRightLogical(w, 31);
                        var temp = (Vector.ShiftLeft(A, 5) | Vector.ShiftRightLogical(A, 27)) + ((B & C) | (B & D) | (C & D)) + E + W[t & 0xF] + C2;
                        E = D;
                        D = C;
                        C = Vector.ShiftLeft(B, 30) | Vector.ShiftRightLogical(B, 2);
                        B = A;
                        A = temp;
                    }
                    for (; t < 80; t++)
                    {
                        var w = W[(t - 3) & 0xF] ^ W[(t - 8) & 0xF] ^ W[(t - 14) & 0xF] ^ W[(t - 16) & 0xF];
                        W[t & 0xF] = Vector.ShiftLeft(w, 1) | Vector.ShiftRightLogical(w, 31);
                        var temp = (Vector.ShiftLeft(A, 5) | Vector.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + W[t & 0xF] + C3;
                        E = D;
                        D = C;
                        C = Vector.ShiftLeft(B, 30) | Vector.ShiftRightLogical(B, 2);
                        B = A;
                        A = temp;
                    }

                    var SeedsBE = (H0 + A) ^ (H1 + B) ^ (H2 + C) ^ (H3 + D) ^ (H4 + E);
                    var Seeds = ChangeEndian(SeedsBE);

                    var pos = i * vcounts.Length + j;


                    Seeds.CopyTo(seeds, pos); //[pos, pos + simdsize) に格納

                }
            }
            return seeds;
        }
    }
}
