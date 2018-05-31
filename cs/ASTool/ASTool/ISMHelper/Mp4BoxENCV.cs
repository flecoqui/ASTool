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
    class Mp4BoxENCV : Mp4Box
    {
  
        static public Mp4BoxENCV CreateENCVBox(Int16  RefIndex, Int16 Width, Int16 Height, Int16 HorizontalRes, Int16 VerticalRes, Int16 FrameCount, Int16 Depth, List<Mp4Box> listChild)
        {

            Mp4BoxENCV box = new Mp4BoxENCV();
            if (box != null)
            {
                int ChildLen = 0;
                if (listChild != null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }

                box.Length = 8 + 78 +  ChildLen;
                box.Type = "encv";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, 0);
                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, RefIndex);
                    WriteMp4BoxInt16(Buffer, 8, 0);
                    WriteMp4BoxInt16(Buffer, 10, 0);
                    WriteMp4BoxInt32(Buffer, 12, 0);
                    WriteMp4BoxInt32(Buffer, 16, 0);
                    WriteMp4BoxInt32(Buffer, 20, 0);
                    WriteMp4BoxInt16(Buffer, 24, Width);
                    WriteMp4BoxInt16(Buffer, 26, Height);
                    WriteMp4BoxInt16(Buffer, 28, HorizontalRes);
                    WriteMp4BoxInt16(Buffer, 30, 0);
                    WriteMp4BoxInt16(Buffer, 32, VerticalRes);
                    WriteMp4BoxInt16(Buffer, 34, 0);
                    WriteMp4BoxInt32(Buffer, 36, 0);
                    WriteMp4BoxInt16(Buffer, 40, FrameCount);
                    WriteMp4BoxInt16(Buffer, 42, 0);
                    WriteMp4BoxInt32(Buffer, 44, 0);
                    WriteMp4BoxInt32(Buffer, 48, 0);
                    WriteMp4BoxInt32(Buffer, 52, 0);

                    WriteMp4BoxInt32(Buffer, 56, 0);
                    WriteMp4BoxInt32(Buffer, 60, 0);
                    WriteMp4BoxInt32(Buffer, 64, 0);
                    WriteMp4BoxInt32(Buffer, 68, 0);

                    WriteMp4BoxInt16(Buffer, 72, 0);
                    WriteMp4BoxInt16(Buffer, 74, Depth);
                    WriteMp4BoxInt16(Buffer, 76, -1);

                    int offset = 78;
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
