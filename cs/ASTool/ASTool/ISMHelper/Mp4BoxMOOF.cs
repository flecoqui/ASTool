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
    public class Mp4BoxMOOF : Mp4Box
    {


        public int GetTrackID()
        {
            int result = 0;
            Mp4Box box = FindChildBox("tfhd");
            if(box!=null)
            {
                Mp4BoxTFHD tfhdbox = box as Mp4BoxTFHD;
                if (tfhdbox != null)
                    return tfhdbox.GetTrackID();
            }
            return result;
        }
        static public Mp4BoxMOOF CreateMOOFBox(List<Mp4Box> listChild)
        {
           
            Mp4BoxMOOF box = new Mp4BoxMOOF();
            if (box != null)
            {
                int ChildLen = 0;
                if(listChild!=null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }
                box.Length = 8 + ChildLen ;
                box.Type = "moof";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    int offset = 0;
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
