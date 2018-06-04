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
    class Mp4BoxMFHD : Mp4Box
    {
        static public Mp4BoxMFHD CreateMFHDBox(int SequenceNumber)
        {
            int Flag = 0;
            byte version = 0x00;
            Mp4BoxMFHD box = new Mp4BoxMFHD();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 ;
                box.Type = "mfhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, Flag);
                    WriteMp4BoxInt32(Buffer, 4, SequenceNumber);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
