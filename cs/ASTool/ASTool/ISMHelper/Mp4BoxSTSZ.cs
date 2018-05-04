using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxCTSZ : Mp4Box
    {
        static public Mp4BoxCTSZ CreateCTSZBox(Int32 size,Int32 count)
        {
           
            Mp4BoxCTSZ box = new Mp4BoxCTSZ();
            if ((box != null)&&(count == 0) && (size == 0))
            {
                
                box.Length = 8 + 4 + 4 +4 ;
                box.Type = "ctsz";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    byte version = 0;
                    Int32 flag = 0;
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, flag);
                    WriteMp4BoxInt32(Buffer, 4, size);
                    WriteMp4BoxInt32(Buffer, 4, count);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
