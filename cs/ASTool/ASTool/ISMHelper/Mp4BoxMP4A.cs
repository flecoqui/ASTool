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
    class Mp4BoxMP4A : Mp4Box
    {
  
        static public Mp4BoxMP4A CreateMP4ABox(Int16  RefIndex, Int16 ChannelCount, Int16 SampleSize, Int32 SampleRate, List<Mp4Box> listChild)
        {

            Mp4BoxMP4A box = new Mp4BoxMP4A();
            if (box != null)
            {
                int ChildLen = 0;
                if (listChild != null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }

                box.Length = 8 + 6 + 2 + 2 + 2 + 4 + 2 + 2 + 2 +2 + 2 + 2 + ChildLen;
                box.Type = "mp4a";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, 0);
                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, RefIndex);
                    WriteMp4BoxInt16(Buffer, 8, 0);
                    WriteMp4BoxInt16(Buffer, 10, 0);
                    WriteMp4BoxInt32(Buffer, 12, 0);
                    WriteMp4BoxInt16(Buffer, 16, ChannelCount);
                    WriteMp4BoxInt16(Buffer, 18, SampleSize);
                    WriteMp4BoxInt16(Buffer, 20, 0);
                    WriteMp4BoxInt16(Buffer, 22, 0);
                    // HACK
                    //WriteMp4BoxInt32(Buffer, 24, SampleRate);
                    WriteMp4BoxInt16(Buffer, 24, (Int16)SampleRate);
                    WriteMp4BoxInt16(Buffer, 26, 0);
                    int offset = 28;
                    if (listChild != null)
                    {
                        foreach (var c in listChild)
                        {
                            WriteMp4BoxData(Buffer, offset, c.GetBoxBytes());
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
