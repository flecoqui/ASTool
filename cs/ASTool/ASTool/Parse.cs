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
using ASTool.CacheHelper;
using ASTool.ISMHelper;
namespace ASTool
{
    public partial class Program
    {
        static Mp4BoxFTYP CreateFTYPBox()
        {
            List<string> list = new List<string>();
            if (list != null)
            {
                list.Add("piff");
                list.Add("iso2");
                return Mp4BoxFTYP.CreateFTYPBox("isml", 1, list);
            }
            return null;
        }
        static Mp4BoxMOOV CreateVideoMOOVBox(Int16 TrackID, Int16 Width, Int16 Height, Int32 TimeScale, Int64 Duration, string LanguageCode,
            /*Int32 MaxBitrate, Int32 AvgBitrate, Int32 BufferSize*/
            Byte[] SPSNALUContent,
            Byte[] PPSNALUContent
            )

        {
            Byte ConfigurationVersion = 0x01;
            Byte AVCProfileIndication = 0x64;
            Byte ProfileCompatibility = 0x40;
            Byte AVCLevelIndication = 0x1F;
            Int16 RefIndex = 1;
            Int16 HorizontalRes = 72;
            Int16 VerticalRes = 72;
            Int16 FrameCount = 1;
            Int16 Depth = 24;

            string handler_type = "vide";
            string handler_name = "Video Media Handler\0";
            DateTime CreationTime = DateTime.Now;
            DateTime UpdateTime = DateTime.Now;
            Int32 Flags = 7;
            Mp4BoxAVCC avccbox = Mp4BoxAVCC.CreateAVCCBox(ConfigurationVersion, AVCProfileIndication, ProfileCompatibility, AVCLevelIndication, SPSNALUContent, PPSNALUContent);
           // Mp4BoxBTRT btrtbox = Mp4BoxBTRT.CreateBTRTBox(BufferSize, MaxBitrate, AvgBitrate);
            if (avccbox != null)
                //&& (btrtbox != null))
            {
                List<Mp4Box> list = new List<Mp4Box>();
                if (list != null)
                {
                    list.Add(avccbox);
//                    list.Add(btrtbox);
                    Mp4BoxAVC1 boxavc1 = Mp4BoxAVC1.CreateAVC1Box(RefIndex, Width, Height, HorizontalRes, VerticalRes, FrameCount, Depth, list);
                    if (boxavc1 != null)
                    {
                        list.Clear();
                        list.Add(boxavc1);
                        Mp4BoxSTSD boxstsd = Mp4BoxSTSD.CreateSTSDBox(1, list);
                        Mp4BoxSTTS boxstts = Mp4BoxSTTS.CreateSTTSBox(0);
                        Mp4BoxCTTS boxctts = Mp4BoxCTTS.CreateCTTSBox(0);
                        Mp4BoxSTSC boxstsc = Mp4BoxSTSC.CreateSTSCBox(0);
                        Mp4BoxSTCO boxstco = Mp4BoxSTCO.CreateSTCOBox(0);
                        Mp4BoxSTSZ boxstsz = Mp4BoxSTSZ.CreateSTSZBox(0, 0);
                        if ((boxstsd != null) &&
                            (boxstts != null) &&
                            (boxctts != null) &&
                            (boxstsc != null) &&
                            (boxstco != null) &&
                            (boxstsz != null)
                            )
                        {
                            list.Clear();
                            list.Add(boxstts);
                            list.Add(boxctts);
                            list.Add(boxstsc);
                            list.Add(boxstco);
                            list.Add(boxstsz);
                            list.Add(boxstsd);
                            Mp4BoxSTBL boxstbl = Mp4BoxSTBL.CreateSTBLBox(list);
                            if (boxstbl != null)
                            {
                                string url = string.Empty;
                                Mp4BoxURL boxurl = Mp4BoxURL.CreateURLBox(url);
                                if (boxurl != null)
                                {
                                    list.Clear();
                                    list.Add(boxurl);
                                    Mp4BoxDREF boxdref = Mp4BoxDREF.CreateDREFBox((Int32)list.Count, list);
                                    if (boxdref != null)
                                    {
                                        list.Clear();

                                        list.Add(boxdref);
                                        Mp4BoxDINF boxdinf = Mp4BoxDINF.CreateDINFBox(list);
                                        if (boxdinf != null)
                                        {
                                            Mp4BoxVMHD boxvmhd = Mp4BoxVMHD.CreateVMHDBox();
                                            if (boxvmhd != null)
                                            {
                                                list.Clear();
                                                list.Add(boxvmhd);
                                                list.Add(boxdinf);
                                                list.Add(boxstbl);

                                                Mp4BoxMINF boxminf = Mp4BoxMINF.CreateMINFBox(list);
                                                if (boxminf != null)
                                                {
                                                    Mp4BoxHDLR boxhdlr = Mp4BoxHDLR.CreateHDLRBox(handler_type, handler_name);
                                                    if (boxhdlr != null)
                                                    {
                                                        Mp4BoxMDHD boxmdhd = Mp4BoxMDHD.CreateMDHDBox(CreationTime, UpdateTime, TimeScale, Duration, LanguageCode);
                                                        if (boxmdhd != null)
                                                        {

                                                            list.Clear();
                                                            list.Add(boxmdhd);
                                                            list.Add(boxhdlr);
                                                            list.Add(boxminf);
                                                            Mp4BoxMDIA boxmdia = Mp4BoxMDIA.CreateMDIABox(list);
                                                            if (boxmdia != null)
                                                            {
                                                                Mp4BoxTKHD boxtkhd = Mp4BoxTKHD.CreateTKHDBox(Flags, CreationTime, UpdateTime, TrackID, Duration, false, Width, Height);
                                                                if (boxtkhd != null)
                                                                {
                                                                    list.Clear();
                                                                    list.Add(boxtkhd);
                                                                    list.Add(boxmdia);
                                                                    Mp4BoxTRAK boxtrak = Mp4BoxTRAK.CreateTRAKBox(list);
                                                                    if (boxtrak != null)
                                                                    {
                                                                        Mp4BoxMVHD boxmvhd = Mp4BoxMVHD.CreateMVHDBox(CreationTime, UpdateTime, TimeScale, Duration, TrackID + 1);
                                                                        if (boxmvhd != null)
                                                                        {
                                                                            Mp4BoxMEHD boxmehd = Mp4BoxMEHD.CreateMEHDBox(Duration);
                                                                            Mp4BoxTREX boxtrex = Mp4BoxTREX.CreateTREXBox(TrackID);
                                                                            if ((boxmehd != null)&&
                                                                                (boxtrex != null))
                                                                            {

                                                                                list.Clear();
                                                                                list.Add(boxmehd);
                                                                                list.Add(boxtrex);
                                                                                Mp4BoxMVEX boxmvex = Mp4BoxMVEX.CreateMVEXBox(list);
                                                                                if (boxmvex != null)
                                                                                {
                                                                                    list.Clear();
                                                                                    list.Add(boxmvhd);
                                                                                    list.Add(boxtrak);
                                                                                    list.Add(boxmvex);
                                                                                    return Mp4BoxMOOV.CreateMOOVBox(list);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        static Mp4BoxMOOV CreateAudioMOOVBox(Int16 TrackID, int MaxFrameSize, int Bitrate, int SampleSize, int SampleRate, int Channels, Int32 TimeScale, Int64 Duration, string LanguageCode)

        {
            Int16 RefIndex = 1;


            string handler_type = "soun";
            string handler_name = "Audio\0";
            DateTime CreationTime = DateTime.Now;
            DateTime UpdateTime = DateTime.Now;
            Int32 Flags = 7;
            Mp4BoxESDS boxesds = Mp4BoxESDS.CreateESDSBox(MaxFrameSize, Bitrate, SampleRate, Channels, string.Empty);
            if (boxesds != null)
            {
                List<Mp4Box> list = new List<Mp4Box>();
                if (list != null)
                {
                    list.Add(boxesds);
                    Mp4BoxMP4A boxmp4a = Mp4BoxMP4A.CreateMP4ABox(RefIndex,(Int16) Channels, (Int16) SampleSize, SampleRate, list);
                    if (boxmp4a != null)
                    {
                        list.Clear();
                        list.Add(boxmp4a);
                        Mp4BoxSTSD boxstsd = Mp4BoxSTSD.CreateSTSDBox(1, list);
                        Mp4BoxSTTS boxstts = Mp4BoxSTTS.CreateSTTSBox(0);
                        Mp4BoxCTTS boxctts = Mp4BoxCTTS.CreateCTTSBox(0);
                        Mp4BoxSTSC boxstsc = Mp4BoxSTSC.CreateSTSCBox(0);
                        Mp4BoxSTCO boxstco = Mp4BoxSTCO.CreateSTCOBox(0);
                        Mp4BoxSTSZ boxstsz = Mp4BoxSTSZ.CreateSTSZBox(0, 0);
                        if ((boxstsd != null) &&
                            (boxstts != null) &&
                            (boxctts != null) &&
                            (boxstsc != null) &&
                            (boxstco != null) &&
                            (boxstsz != null)
                            )
                        {
                            list.Clear();
                            list.Add(boxstts);
                            list.Add(boxctts);
                            list.Add(boxstsc);
                            list.Add(boxstco);
                            list.Add(boxstsz);
                            list.Add(boxstsd);
                            Mp4BoxSTBL boxstbl = Mp4BoxSTBL.CreateSTBLBox(list);
                            if (boxstbl != null)
                            {
                                string url = string.Empty;
                                Mp4BoxURL boxurl = Mp4BoxURL.CreateURLBox(url);
                                if (boxurl != null)
                                {
                                    list.Clear();
                                    list.Add(boxurl);
                                    Mp4BoxDREF boxdref = Mp4BoxDREF.CreateDREFBox((Int32)list.Count, list);
                                    if (boxdref != null)
                                    {
                                        list.Clear();

                                        list.Add(boxdref);
                                        Mp4BoxDINF boxdinf = Mp4BoxDINF.CreateDINFBox(list);
                                        if (boxdinf != null)
                                        {
                                            Mp4BoxSMHD boxsmhd = Mp4BoxSMHD.CreateSMHDBox();
                                            if (boxsmhd != null)
                                            {
                                                list.Clear();
                                                list.Add(boxsmhd);
                                                list.Add(boxdinf);
                                                list.Add(boxstbl);

                                                Mp4BoxMINF boxminf = Mp4BoxMINF.CreateMINFBox(list);
                                                if (boxminf != null)
                                                {
                                                    Mp4BoxHDLR boxhdlr = Mp4BoxHDLR.CreateHDLRBox(handler_type, handler_name);
                                                    if (boxhdlr != null)
                                                    {
                                                        Mp4BoxMDHD boxmdhd = Mp4BoxMDHD.CreateMDHDBox(CreationTime, UpdateTime, TimeScale, Duration, LanguageCode);
                                                        if (boxmdhd != null)
                                                        {

                                                            list.Clear();
                                                            list.Add(boxmdhd);
                                                            list.Add(boxhdlr);
                                                            list.Add(boxminf);
                                                            Mp4BoxMDIA boxmdia = Mp4BoxMDIA.CreateMDIABox(list);
                                                            if (boxmdia != null)
                                                            {
                                                                Mp4BoxTKHD boxtkhd = Mp4BoxTKHD.CreateTKHDBox(Flags, CreationTime, UpdateTime, TrackID, Duration, true, 0, 0);
                                                                if (boxtkhd != null)
                                                                {
                                                                    list.Clear();
                                                                    list.Add(boxtkhd);
                                                                    list.Add(boxmdia);
                                                                    Mp4BoxTRAK boxtrak = Mp4BoxTRAK.CreateTRAKBox(list);
                                                                    if (boxtrak != null)
                                                                    {
                                                                        Mp4BoxMVHD boxmvhd = Mp4BoxMVHD.CreateMVHDBox(CreationTime, UpdateTime, TimeScale, Duration, TrackID + 1);
                                                                        if (boxmvhd != null)
                                                                        {
                                                                            Mp4BoxMEHD boxmehd = Mp4BoxMEHD.CreateMEHDBox(Duration);
                                                                            Mp4BoxTREX boxtrex = Mp4BoxTREX.CreateTREXBox(TrackID);
                                                                            if ((boxmehd != null) &&
                                                                                (boxtrex != null))
                                                                            {

                                                                                list.Clear();
                                                                                list.Add(boxmehd);
                                                                                list.Add(boxtrex);
                                                                                Mp4BoxMVEX boxmvex = Mp4BoxMVEX.CreateMVEXBox(list);
                                                                                if (boxmvex != null)
                                                                                {
                                                                                    list.Clear();
                                                                                    list.Add(boxmvhd);
                                                                                    list.Add(boxtrak);
                                                                                    list.Add(boxmvex);
                                                                                    return Mp4BoxMOOV.CreateMOOVBox(list);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        static byte[] GetFTYPBoxData()
        {
            List<string> list = new List<string>();
            if (list != null)
            {
                list.Add("piff");
                list.Add("iso2");
                Mp4BoxFTYP box = Mp4BoxFTYP.CreateFTYPBox("isml", 1, list);
                if (box != null)
                {
                    return box.GetBoxBytes();
                }
            }
            return null;
        }
        static byte[] GetDINFBoxData(List<Mp4Box> listChild)
        {
            Mp4BoxDINF box = Mp4BoxDINF.CreateDINFBox(listChild);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetDREFBoxData(Int32 count, List<Mp4Box> listChild)
        {
            Mp4BoxDREF box = Mp4BoxDREF.CreateDREFBox(count, listChild);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetSMHDBoxData()
        {
            Mp4BoxSMHD box = Mp4BoxSMHD.CreateSMHDBox();
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetURLBoxData(string url)
        {
            Mp4BoxURL box = Mp4BoxURL.CreateURLBox(url);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetMDHDBoxData(DateTime CreationTime, DateTime ModificationTime, Int32 TimeScale, Int64 Duration, string LanguageCode)
        {
            Mp4BoxMDHD box = Mp4BoxMDHD.CreateMDHDBox(CreationTime, ModificationTime, TimeScale, Duration, LanguageCode);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetHDLRBoxData(string handler_type, string name)
        {
            Mp4BoxHDLR box = Mp4BoxHDLR.CreateHDLRBox(handler_type, name);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetMVHDBoxData(DateTime CreationTime, DateTime ModificationTime, Int32 TimeScale, Int64 Duration, Int32 NextTrackID)
        {
            Mp4BoxMVHD box = Mp4BoxMVHD.CreateMVHDBox( CreationTime, ModificationTime,  TimeScale, Duration,  NextTrackID);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static byte[] GetTKHDBoxData(Int32 Flag, DateTime CreationTime, DateTime ModificationTime, Int32 TrackID, Int64 duration, bool IsAudioTrack, Int32 width, Int32 height)
        {
            Mp4BoxTKHD box = Mp4BoxTKHD.CreateTKHDBox(Flag, CreationTime, ModificationTime, TrackID, duration, IsAudioTrack, width, height);
            if (box != null)
            {
                return box.GetBoxBytes();
            }
            return null;
        }
        static char GetChar(byte b)
        {
            if ((b >= 32) && (b < 127))
                return (char) b;
            return '.';
        }

        static void DumpHex(byte[] data)
        {
            string resultHex = " ";
            string resultASCII = " ";
            int Len = ((data.Length%16 == 0) ? (data.Length/16): (data.Length / 16)+1)*16;
            for (int i = 0; i < Len; i++)
            {
                if (i < data.Length)
                {
                    resultASCII += string.Format("{0}", GetChar(data[i])); 
                    resultHex += string.Format("{0:X2} ", data[i]);
                }
                else
                {
                    resultASCII += " ";
                    resultHex += "   ";
                }
                if (i%16==15)
                {
                    Console.WriteLine(string.Format("{0:X8} ",i-15) + resultHex + resultASCII);
                    resultHex = " ";
                    resultASCII = " ";
                }
            }
        }
        public static bool InsertFTYPMOOV(string path, Mp4BoxFTYP boxftyp, Mp4BoxMOOV boxmoov)
        {
            bool result = false;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {

                    FileStream fso = new FileStream(path + ".ismv", FileMode.Create, FileAccess.ReadWrite);
                    if (fso != null)
                    {
                        byte[] data = boxftyp.GetBoxBytes();
                        if (data != null)
                        {
                            fso.Write(data, 0, data.Length);
                            data = boxmoov.GetBoxBytes();
                            if (data != null)
                            {
                                fso.Write(data, 0, data.Length);
                                data = new byte[65535];
                                long offset = 0;
                                while (offset < fs.Length)
                                {
                                    int Len = fs.Read(data, 0, data.Length);
                                    if (Len == 0)
                                        break;
                                    fso.Write(data, 0, Len);
                                    offset += Len;
                                }
                                if (offset == fs.Length)
                                    result = true;
                                fso.Close();
                            }
                        }

                    }
                    fs.Close();
                }
            }
            catch(Exception )
            {
                result = false;
            }
            
            return result;
        }



        static bool Parse(Options opt)
        {
            bool result = true;
            opt.Status = Options.TheadStatus.Running;
            opt.ThreadStartTime = DateTime.Now;
            opt.LogInformation("Parsing file: " + opt.InputUri);

            if(((opt.TraceLevel>= Options.LogLevel.Verbose)&&(!string.IsNullOrEmpty(opt.TraceFile)))||(opt.ConsoleLevel >= Options.LogLevel.Verbose))
                opt.LogVerbose(Mp4Box.ParseFileVerbose(opt.InputUri));
            else
                opt.LogInformation(Mp4Box.ParseFile(opt.InputUri));
            opt.LogInformation("Parsing file: " + opt.InputUri + " done");
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
