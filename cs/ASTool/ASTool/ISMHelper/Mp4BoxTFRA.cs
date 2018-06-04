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
    class Mp4BoxTFRA : Mp4Box
    {
        public int GetTrackID()
        {
            return ReadMp4BoxInt32(this.Data, 4);
        }
        public int GetNumberOfEntry()
        {
            return ReadMp4BoxInt32(this.Data, 12);
        }
        public bool UpdateEntries(List<long> list)
        {
            bool result = false;
            byte[] version = ReadMp4BoxBytes(this.Data, 0, 1);
            if (GetNumberOfEntry() == list.Count)
            {
                if (version[0] == 1)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        WriteMp4BoxInt64(this.Data, 16 + 8 + i * 19, (long)list[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        WriteMp4BoxInt32(this.Data, 16 + 8 + i * 11, (Int32)list[i]);
                    }
                }
                result = true;
            }

            return result;
        }
        static public Mp4BoxTFRA CreateTFRABox(Int32 TrackID, List<TimeMoofOffset> list)
        {
            int Flag = 0;
            byte version = 0x01;
            if (list != null)
            {
                int NumberOfEntry = list.Count;
                Mp4BoxTFRA box = new Mp4BoxTFRA();
                if (box != null)
                {
                    box.Length = 8 + 4 + 4 + 4 + 4 + (8 + 8 + 3) * NumberOfEntry;
                    box.Type = "tfra";
                    byte[] Buffer = new byte[box.Length - 8];
                    if (Buffer != null)
                    {
                        WriteMp4BoxByte(Buffer, 0, version);
                        WriteMp4BoxInt24(Buffer, 1, Flag);
                        WriteMp4BoxInt32(Buffer, 4, TrackID);
                        int Reserved = 0;
                        WriteMp4BoxInt32(Buffer, 8, Reserved);
                        WriteMp4BoxInt32(Buffer, 12, NumberOfEntry);
                        for (int i = 0; i < NumberOfEntry; i++)
                        {

                            WriteMp4BoxInt64(Buffer, 16 + i*19, (long)list[i].time);
                            WriteMp4BoxInt64(Buffer, 16 + 8 + i * 19, (long)list[i].offset);
                            WriteMp4BoxInt8(Buffer, 16 + 8 + 8 + i * 19, 1);
                            WriteMp4BoxInt8(Buffer, 16 + 8 + 8 + 1 + i * 19, 1);
                            WriteMp4BoxInt8(Buffer, 16 + 8 + 8 + 1 + 1 + i * 19, 1);
                        }
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
