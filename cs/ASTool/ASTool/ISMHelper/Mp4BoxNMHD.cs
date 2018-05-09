using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxNMHD : Mp4Box
    {
        static public Mp4BoxNMHD CreateNMHDBox()
        {

            Mp4BoxNMHD box = new Mp4BoxNMHD();
            if (box != null)
            {
                box.Length = 8 + 4 ;
                box.Type = "nmhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, 0);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
