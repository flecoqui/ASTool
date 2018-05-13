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
    class Mp4BoxTKHD : Mp4Box
    {
        static public Mp4BoxTKHD CreateTKHDBox(Int32 Flag, DateTime CreationTime, DateTime ModificationTime, Int32 TrackID, Int64 duration, bool IsAudioTrack, Int32 width, Int32 height)
        {
            byte version = 0x01;
            Mp4BoxTKHD box = new Mp4BoxTKHD();
            if (box != null)
            {
                box.Length = 8 + 4 + 8 + 8 + 4 + 4 + 8 + 4 *2 + 2 + 2 + 2 + 2 + 9*4 + 4 + 4 ;
                box.Type = "tkhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, Flag);

                    WriteMp4BoxInt64(Buffer, 4, GetMp4BoxTime(CreationTime));
                    WriteMp4BoxInt64(Buffer, 12, GetMp4BoxTime(ModificationTime));

                    WriteMp4BoxInt32(Buffer, 20, TrackID);
                    
                    WriteMp4BoxInt32(Buffer, 24, 0);
                    WriteMp4BoxInt64(Buffer, 28, duration);

                    WriteMp4BoxInt32(Buffer, 36, 0);
                    WriteMp4BoxInt32(Buffer, 40, 0);
                    Int16 layer = 0;
                    WriteMp4BoxInt16(Buffer, 44, layer);
                    Int16 alternate_group = 0;
                    WriteMp4BoxInt16(Buffer, 46, alternate_group);
                    Int16 volume = (Int16) (IsAudioTrack == true ? 0x0100 : 0);
                    WriteMp4BoxInt16(Buffer, 48, volume);
                    WriteMp4BoxInt16(Buffer, 50, 0);

                    WriteMp4BoxInt32(Buffer, 52, 0x00010000);
                    WriteMp4BoxInt32(Buffer, 56, 0);
                    WriteMp4BoxInt32(Buffer, 60, 0);
                    WriteMp4BoxInt32(Buffer, 64, 0);
                    WriteMp4BoxInt32(Buffer, 68, 0x00010000);
                    WriteMp4BoxInt32(Buffer, 72, 0);
                    WriteMp4BoxInt32(Buffer, 76, 0);
                    WriteMp4BoxInt32(Buffer, 80, 0);
                    WriteMp4BoxInt32(Buffer, 84, 0x40000000);

                    WriteMp4BoxInt32(Buffer, 86, width);
                    WriteMp4BoxInt32(Buffer, 90, height);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
