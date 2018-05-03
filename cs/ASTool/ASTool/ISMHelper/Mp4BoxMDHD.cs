using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxMDHD : Mp4Box
    {
        static public Mp4BoxMDHD CreateMDHDBox(DateTime CreationTime, DateTime ModificationTime, Int32 TimeScale, Int64 Duration, string LanguageCode)
        {
            byte version = 0x01;
            char[] array = LanguageCode.ToCharArray();
            if ((array != null) && (array.Length == 3))
            {
                Int16 Lang = 0;
                Lang |= (Int16)(((array[0] - 0x60) & 0x01F) << 10);
                Lang |= (Int16)(((array[1] - 0x60) & 0x01F) << 5);
                Lang |= (Int16)(((array[2] - 0x60) & 0x01F) << 0);
                Mp4BoxMDHD box = new Mp4BoxMDHD();
                if (box != null)
                {
                    box.Length = 8 + 4 + 8 + 8 + 4 + 8 + 2 + 2;
                    box.Type = "mdhd";
                    byte[] Buffer = new byte[box.Length - 8];
                    if (Buffer != null)
                    {
                        WriteMp4BoxByte(Buffer, 0, version);
                        WriteMp4BoxInt24(Buffer, 1, 0);

                        WriteMp4BoxInt64(Buffer, 4, GetMp4BoxTime(CreationTime));
                        WriteMp4BoxInt64(Buffer, 12, GetMp4BoxTime(ModificationTime));
                        WriteMp4BoxInt32(Buffer, 20, TimeScale);
                        WriteMp4BoxInt64(Buffer, 24, Duration);


                        WriteMp4BoxInt16(Buffer, 32, Lang);
                        WriteMp4BoxInt16(Buffer, 34, 0);
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
