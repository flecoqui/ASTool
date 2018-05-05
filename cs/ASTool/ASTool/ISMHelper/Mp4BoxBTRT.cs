using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxBTRT : Mp4Box
    {
        static public Mp4BoxBTRT CreateBTRTBox(Int32 BufferSize, Int32 MaxBitrate, Int32 AvrBitrate)
        {
            Mp4BoxBTRT box = new Mp4BoxBTRT();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 + 4;
                box.Type = "btrt";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, BufferSize);
                    WriteMp4BoxInt32(Buffer, 4, MaxBitrate);
                    WriteMp4BoxInt32(Buffer, 8, AvrBitrate);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
