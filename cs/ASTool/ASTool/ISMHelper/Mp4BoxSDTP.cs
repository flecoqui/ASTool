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
    class Mp4BoxSDTP : Mp4Box
    {
        static public Mp4BoxSDTP CreateSDTPBox(int SampleCount,  List<byte> list)
        {
            int Flag = 0;
            byte version = 0x00;
            Mp4BoxSDTP box = new Mp4BoxSDTP();
            if (box != null)
            {
                box.Length = 8 + 4 + 1*SampleCount;
                box.Type = "sdtp";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, Flag);
                    int offset = 0;
                    foreach(var info in list)
                    {
                        WriteMp4BoxByte(Buffer, 4 +  offset, info);
                        offset += 1;
                    }
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
