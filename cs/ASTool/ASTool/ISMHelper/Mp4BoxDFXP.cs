using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxDFXP : Mp4Box
    {
  
        static public Mp4BoxDFXP CreateDFXPBox(Int16  RefIndex)
        {

            Mp4BoxDFXP box = new Mp4BoxDFXP();
            if (box != null)
            {

                box.Length = 8 + 8;
                box.Type = "dfxp";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, 0);
                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, RefIndex);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
