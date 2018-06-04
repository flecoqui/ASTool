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
    class Mp4BoxTFHD : Mp4Box
    {
        public int GetTrackID()
        {
            return ReadMp4BoxInt32(this.Data, 4);
        }
        static public Mp4BoxTFHD CreateTFHDBox(int TrackID, int SampleFlags)
        {
            int Flag = 32;
            byte version = 0x00;
            Mp4BoxTFHD box = new Mp4BoxTFHD();
            if (box != null)
            {
                box.Length = 8 + 4 + 4 + 4;
                box.Type = "tfhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1, Flag);
                    WriteMp4BoxInt32(Buffer, 4, TrackID);
                    WriteMp4BoxInt32(Buffer, 8, SampleFlags);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
