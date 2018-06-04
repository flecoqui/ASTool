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
    public class Mp4BoxMOOV : Mp4Box
    {


        public bool UpdateEncBoxes()
        {
            bool result = false;
            if (Children != null)
            {
                foreach (var box in Children)
                {
                    if (box.GetBoxType() == "trak")
                    {
                        Mp4Box encbox = box.FindChildBox("encv");
                        if (encbox != null)
                        {
                            encbox.SetBoxType("avc1");
                            Mp4Box sinfbox = box.FindChildBox("sinf");
                            if (sinfbox != null)
                            {
                                int Len = sinfbox.GetBoxLength();
                                UpdateParentLength(sinfbox,-Len);
                                encbox.RemoveChildBox("sinf");
                            }
                        }
                        else
                        {
                            encbox = box.FindChildBox("enca");
                            if (encbox != null)
                            {
                                encbox.SetBoxType("mp4a");
                                Mp4Box sinfbox = box.FindChildBox("sinf");
                                if (sinfbox != null)
                                {
                                    int Len = sinfbox.GetBoxLength();
                                    UpdateParentLength(sinfbox, -Len);
                                    encbox.RemoveChildBox("sinf");
                                }
                            }
                            else
                            {
                                encbox = box.FindChildBox("enct");
                                if (encbox != null)
                                {
                                    encbox.SetBoxType("dfxp");
                                    Mp4Box sinfbox = box.FindChildBox("sinf");
                                    if (sinfbox != null)
                                    {
                                        int Len = sinfbox.GetBoxLength();
                                        UpdateParentLength(sinfbox, -Len);
                                        encbox.RemoveChildBox("sinf");
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return result;
        }

        public List<int> GetListTrackToDecrypt()
        {
            List<int> result = new List<int>();
            if (Children != null)
            {
                foreach (var box in Children)
                {
                    if(box.GetBoxType()=="trak")
                    {
                        Mp4Box encbox = box.FindChildBox("encv");
                        if (encbox == null)
                        {
                            encbox = box.FindChildBox("enca");
                            if (encbox == null)
                                encbox = box.FindChildBox("enct");
                        }
                        if (encbox != null)
                        {
                            Mp4BoxTRAK trakbox = box as Mp4BoxTRAK;
                            if (trakbox != null)
                                result.Add(trakbox.GetTrackID());
                        }
                    }
                }
            }
            return result;
        }
        
        public List<int> GetListTrackToEncrypt(bool bProtectCaption)
        {
            List<int> result = new List<int>();
            return result;
        }
        static public Mp4BoxMOOV CreateMOOVBox(List<Mp4Box> listChild)
        {
           
            Mp4BoxMOOV box = new Mp4BoxMOOV();
            if (box != null)
            {
                int ChildLen = 0;
                if(listChild!=null)
                {
                    foreach (var c in listChild)
                        ChildLen += c.GetBoxLength();
                }
                box.Length = 8 + ChildLen ;
                box.Type = "moov";
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
