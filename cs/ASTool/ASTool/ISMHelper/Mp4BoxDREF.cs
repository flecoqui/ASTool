using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxDREF : Mp4Box
    {
        static public Mp4BoxDREF CreateDREFBox(Int32 count, List<Mp4Box> listChild)
        {
           
            Mp4BoxDREF box = new Mp4BoxDREF();
            if (box != null)
            {
                int ChildLen = 0;
                if(listChild!=null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }
                box.Length = 8 + 4 + 4 + ChildLen ;
                box.Type = "dref";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    byte version = 0;
                    Int32 flag = 0;
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, flag);
                    WriteMp4BoxInt32(Buffer, 4, count);
                    int offset = 0;
                    if (listChild != null)
                    {
                        foreach (var c in listChild)
                        {
                            WriteMp4BoxData(Buffer, 8 + offset, c.GetBoxBytes());
                            offset += c.GetBoxLength();
                        }
                    }

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
