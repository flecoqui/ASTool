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
    class Mp4BoxSMHD : Mp4Box
    {
        static public Mp4BoxSMHD CreateSMHDBox()
        {
            byte version = 0x00;
            Mp4BoxSMHD box = new Mp4BoxSMHD();
            if (box != null)
            {
                box.Length = 8 + 8 ;
                box.Type = "smhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, 0);

                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, 0);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
