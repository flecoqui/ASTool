﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxTREX : Mp4Box
    {
        static public Mp4BoxTREX CreateTREXBox(Int32 TrackID)
        {
            byte version = 0x01;
            Mp4BoxTREX box = new Mp4BoxTREX();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 + 16 ;
                box.Type = "trex";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, 0);

                    WriteMp4BoxInt32(Buffer, 4, TrackID);
                    WriteMp4BoxInt32(Buffer, 8, 1);
                    WriteMp4BoxInt32(Buffer, 12, 0);
                    WriteMp4BoxInt32(Buffer, 16, 0);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
