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
    public class Mp4BoxMFRA : Mp4Box
    {
        public bool UpdateMOOFOffsetForTrack(int trackID, List<long> MOOFOffsetList)
        {
            bool result = false;
            if(Children!=null)
            {
                foreach(var box in Children)
                {
                    if(box.GetBoxType() == "tfra")
                    {
                        Mp4BoxTFRA tfra = box as Mp4BoxTFRA;
                        if(tfra!=null)
                        {
                            if(tfra.GetTrackID()==trackID)
                            {
                                if (tfra.GetNumberOfEntry() == MOOFOffsetList.Count)
                                    return tfra.UpdateEntries(MOOFOffsetList);
                            }
                        }
                    }
                }
            }
            return result;
        }
        static public Mp4BoxMFRA CreateMFRABox(List<Mp4Box> listChild)
        {

            Mp4BoxMFRA box = new Mp4BoxMFRA();
            if (box != null)
            {
                int ChildLen = 0;
                if(listChild!=null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }
                box.Length = 8 + ChildLen ;
                box.Type = "mfra";
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
