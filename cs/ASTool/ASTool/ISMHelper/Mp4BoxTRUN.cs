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
        public int SampleFlags;        public int SampleCompositionTimeoffset;
    }
    class Mp4BoxTRUN : Mp4Box
    {
        public bool SetDataOffset(int offset)
        {
            return WriteMp4BoxInt32(this.Data, 8, offset);
        }
        public Int32 GetDataOffset()
        {
            return ReadMp4BoxInt32(this.Data, 8);
        }
        public List<Int32> GetSampleSizeList()
        {
            List<Int32> list = new List<Int32>();
            if(list!=null)
            {
                //	0x000001. (data - offset - present).Specifies whether the data_offset field is present.MUST be set.
                //	0x000004  (first - sample - flags - present).Overrides the default flags for the first sample only.This makes it possible to record a group of frames where the first is a key and the rest are difference frames, without supplying explicit flags for every sample. If this flag and field are used, sample-flags shall not be present.
                //	0x000100 (sample-duration-present). Indicates that each sample has its own duration, otherwise the default is used.
                //	0x000200 (sample-size-present). Each sample has its own size, otherwise the default is used.
                //	0x000400 (sample-flags-present). Each sample has its own flags, otherwise the default is used.          
                //	0x000800 (sample-composition-time-offsets-present). Each sample has a composition time offset(e.g., as used for I/P/B video in MPEG).


                int Flag = ReadMp4BoxInt24(this.Data, 1);
                int TableOffset = 8;
                if ((Flag & 0x00000001) == 0x00000001)
                    TableOffset += 4;
                if ((Flag & 0x00000004) == 0x00000004)
                    TableOffset += 4;
                int RowSize = 0;
                if ((Flag & 0x00000100) == 0x00000100)
                {
                    RowSize += 4;
                    TableOffset += 4;
                }
                if ((Flag & 0x00000200) == 0x00000200)
                    RowSize += 4;
                if ((Flag & 0x00000400) == 0x00000400)
                    RowSize += 4;
                if ((Flag & 0x00000800) == 0x00000800)
                    RowSize += 4;

                {
                    int offset = 0;
                    int SampleCount = ReadMp4BoxInt32(this.Data, 4);
                    for (int i = 0; i < SampleCount; i++)
                    {
                        list.Add(ReadMp4BoxInt32(this.Data, TableOffset + offset));
                        offset += RowSize;
                    }
                }
            }
            return list;
        }
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
