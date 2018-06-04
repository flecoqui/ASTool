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
    public struct SampleInformation
    {
        public int SampleDuration;
        public int SampleSize;
        public int SampleFlags;
        public int SampleCompositionTimeoffset;
    }
    class Mp4BoxTRUN : Mp4Box
    {
        static public Mp4BoxTRUN CreateTRUNBox(int SampleCount, int DataOffset, List<SampleInformation> list)
        {
            int Flag = 769;
            byte version = 0x00;
            Mp4BoxTRUN box = new Mp4BoxTRUN();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 + 4 + 16*SampleCount;
                box.Type = "trun";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, Flag);
                    WriteMp4BoxInt32(Buffer, 4, SampleCount);
                    WriteMp4BoxInt32(Buffer, 8, DataOffset);
                    int offset = 0;
                    foreach(var info in list)
                    {
                        WriteMp4BoxInt32(Buffer, 12 +  offset, info.SampleDuration);
                        WriteMp4BoxInt32(Buffer, 16 + offset, info.SampleSize);
                        WriteMp4BoxInt32(Buffer, 20 + offset, info.SampleFlags);
                        WriteMp4BoxInt32(Buffer, 24 + offset, info.SampleDuration);
                        offset += 16;
                    }
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
