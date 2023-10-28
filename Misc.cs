namespace Project_Carlo
{
    public static class Misc
    {
        private static readonly UInt32[] BCD = new UInt32[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153 };

        public static UInt32 DecToBCD(UInt32 val)
        {
            return val + val / 10 * 6;
        }

        public static UInt32 BCDToDec(UInt32 val)
        {
            return (val >> 4) * 10 + (val & 0xF);
        }
        public static UInt32 YYMMDDToDatecode(UInt32[] val)
        {
            //algorithm:https://en.wikipedia.org/wiki/Zeller%27s_congruence
            var year = 2000U + val[0];
            var m = val[1];
            var d = val[2];
            if (m == 1 || m == 2)
            {
                m += 12;
                year--;
            }
            var y = year % 100;
            var j = year / 100;

            var nn = (d + 13 * (m + 1) / 5 + y + y / 4 + j / 4 - 2 * j + 6) % 7;

            //nnddmmyy
            var result = nn;
            result <<= 8;
            result |= DecToBCD(val[2]);
            result <<= 8;
            result |= DecToBCD(val[1]);
            result <<= 8;
            result |= DecToBCD(val[0]);

            return result;
        }

        public static UInt32 HHMMSSToTimecode(UInt32[] val)
        {
            //zzssmmhh
            var result = DecToBCD(val[2]);
            result <<= 8;
            result |= DecToBCD(val[1]);
            result <<= 8;
            result |= DecToBCD(val[0]);
            if (val[0] >= 12) result += 0x40;

            return result;
        }
        public static UInt32 ChangeEndian(UInt32 val)
        {
            val = ((val << 8) & 0xFF00FF00U) | ((val >> 8) & 0xFF00FFU);
            return (val << 16) | (val >> 16);
        }

        public static UInt32[] CreateBaseMessage(uint gxstat, uint mode, uint vframe, uint[] mac, uint datecode, uint timecode)
        {

            //UInt32 v = 0x5F;
            //UInt32 t0 = 0xC68;
            //UInt32 frame = 8;

            var b = new uint[16];

            b[0] = ChangeEndian(0);//(vcount <<16) | timer0
            b[1] = ChangeEndian((mac[4] << 16) | (mac[5] << 24) ^ mode);
            b[2] = ChangeEndian(gxstat ^ vframe ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0]));
            b[3] = ChangeEndian(datecode);//datecode
            b[4] = ChangeEndian(timecode);//timecode
            b[5] = 0x00000000;
            b[6] = 0x00000000;
            b[7] = ChangeEndian(0x00002FFF);
            b[8] = 0x80000000;
            b[15] = 0x00000100;
            return b;
        }
    }
}
