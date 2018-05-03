using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxURL : Mp4Box
    {
        static public Mp4BoxURL CreateURLBox(string url)
        {
           
            Mp4BoxURL box = new Mp4BoxURL();
            if (box != null)
            {
                int slen = 0;
                if (!string.IsNullOrEmpty(url) && (url.Length > 0))
                    slen = url.Length;
                box.Length = 8 + 4 + slen ;
                box.Type = "url ";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    byte version = 0;
                    Int32 flag = 1;
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, flag);
                    if(!string.IsNullOrEmpty(url) && (url.Length > 0))
                        WriteMp4BoxString(Buffer, 4, url);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
