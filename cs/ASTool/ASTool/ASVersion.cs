using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool
{
    public class ASVersion
    {

        public static Int32 SetVersion(byte b1, byte b2, byte b3, byte b4)
        {
            Int32 v = (Int32) ((b1 << 24) & 0xFF000000);
            v |= (b2 << 16) & 0x00FF0000;
            v |= (b3 << 8) & 0x0000FF00;
            v |= (b4) & 0x000000FF;
            return v;
        }
        public static string GetVersionString(Int32 v)
        {
            string version = string.Empty;

            byte v1 = (byte)((v & 0xFF000000) >> 24);
            byte v2 = (byte)((v & 0x00FF0000) >> 16);
            byte v3 = (byte)((v & 0x0000FF00) >> 8);
            byte v4 = (byte) (v & 0x000000FF);
            version = string.Format("{0}.{1}.{2}.{3}",v1,v2,v3,v4);
            return version;
        }

    }
}
