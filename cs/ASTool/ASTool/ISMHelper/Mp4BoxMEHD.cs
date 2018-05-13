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
    class Mp4BoxMEHD : Mp4Box
    {
        static public Mp4BoxMEHD CreateMEHDBox(Int64 Duration)
        {
            byte version = 0x01;
            Mp4BoxMEHD box = new Mp4BoxMEHD();
            if (box != null)
            {
                box.Length = 8 + 4 + 8 ;
                box.Type = "mehd";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, 0);

                    WriteMp4BoxInt64(Buffer, 4, Duration);


                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
