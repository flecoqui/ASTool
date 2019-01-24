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
    class Mp4BoxTREX : Mp4Box
    {
        static public Mp4BoxTREX CreateTREXBox(Int32 TrackID)
        {
            // update version to 0
            byte version = 0x00;
            Mp4BoxTREX box = new Mp4BoxTREX();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 + 16 ;
                box.Type = "trex";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, 0);

                    WriteMp4BoxInt32(Buffer, 4, TrackID);
                    WriteMp4BoxInt32(Buffer, 8, 1);
                    WriteMp4BoxInt32(Buffer, 12, 0);
                    WriteMp4BoxInt32(Buffer, 16, 0);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
