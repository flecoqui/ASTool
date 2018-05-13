//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxMVHD : Mp4Box
    {
        static public Mp4BoxMVHD CreateMVHDBox(DateTime CreationTime, DateTime ModificationTime, Int32 TimeScale, Int64 duration, Int32 NextTrackID)
        {
            byte version = 0x01;
            Mp4BoxMVHD box = new Mp4BoxMVHD();
            if (box != null)
            {
                box.Length = 8 + 4 + 8 + 8 + 4 + 8 + 4 + 2 + 2 + 2*4 + 9*4 + 6*4 + 4 ;
                box.Type = "mvhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt64(Buffer, 4, GetMp4BoxTime(CreationTime));
                    WriteMp4BoxInt64(Buffer, 12, GetMp4BoxTime(ModificationTime));

                    WriteMp4BoxInt32(Buffer, 20, TimeScale);
                    
                    WriteMp4BoxInt64(Buffer, 24, duration);
                    Int32 rate = 0x00010000;
                    WriteMp4BoxInt32(Buffer, 32, rate);
                    Int16 volume = 0x0100;
                    WriteMp4BoxInt16(Buffer, 36, volume);
                    WriteMp4BoxInt16(Buffer, 38, 0);

                    WriteMp4BoxInt32(Buffer, 40, 0);
                    WriteMp4BoxInt32(Buffer, 44, 0);

                    WriteMp4BoxInt32(Buffer, 48, 0x00010000);
                    WriteMp4BoxInt32(Buffer, 52, 0);
                    WriteMp4BoxInt32(Buffer, 56, 0);
                    WriteMp4BoxInt32(Buffer, 60, 0);
                    WriteMp4BoxInt32(Buffer, 64, 0x00010000);
                    WriteMp4BoxInt32(Buffer, 68, 0);
                    WriteMp4BoxInt32(Buffer, 72, 0);
                    WriteMp4BoxInt32(Buffer, 76, 0);
                    WriteMp4BoxInt32(Buffer, 80, 0x40000000);

                    WriteMp4BoxInt32(Buffer, 84, 0);
                    WriteMp4BoxInt32(Buffer, 88, 0);
                    WriteMp4BoxInt32(Buffer, 92, 0);
                    WriteMp4BoxInt32(Buffer, 96, 0);
                    WriteMp4BoxInt32(Buffer, 100, 0);
                    WriteMp4BoxInt32(Buffer, 104, 0);

                    WriteMp4BoxInt32(Buffer, 108, NextTrackID);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
