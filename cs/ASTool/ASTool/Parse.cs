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
            bool result = false;
            Console.WriteLine("Parsing file: " + opt.InputUri);

            //Console.Write(Mp4Box.ParseFile(opt.InputUri));
            //Console.WriteLine("Dump FTYP: ");
            //byte[] data = GetFTYPBoxData();
            //if(data!=null)
            //    DumpHex(data);
            Console.WriteLine("Parsing file: " + opt.InputUri + " done");
            //Int32 TimeScale = 10000000;
            //Int64 Duration = 120*10000000;
            //byte[] data = GetMVHDBoxData(DateTime.Now, DateTime.Now, TimeScale, Duration, 3);
            //if(data!=null)
            //    DumpHex(data);

            //Int64 Duration = 120*10000000;
            //Int32 TrackID = 1;
            //bool IsAudioTrack = true;
            //Int32 width = 0;
            //Int32 height = 0;
            //Int32 Flag = 0x07;
            //byte[] data = GetTKHDBoxData(Flag, DateTime.Now, DateTime.Now, TrackID, Duration, IsAudioTrack, width, height);
            //if(data!=null)
            //    DumpHex(data);

            //Duration = 120 * 10000000;
            //TrackID = 2;
            //IsAudioTrack = false;
            //width = 640;
            //height = 396;
            //Flag = 0x07;
            //data = GetTKHDBoxData(Flag, DateTime.Now, DateTime.Now, TrackID, Duration, IsAudioTrack, width, height);
            //if (data != null)
            //    DumpHex(data);

            //Int32 TimeScale = 10000000;
            //Int64 Duration = 120 * 10000000;
            //string LanguageCode = "eng";
            //byte[] data = GetMDHDBoxData(DateTime.Now, DateTime.Now, TimeScale, Duration, LanguageCode);
            //if (data != null)
            //    DumpHex(data);



            //string handler = "soun";
            //string name = "Sound Media Handler\0";
            //byte[] data = GetHDLRBoxData(handler, name);
            //if (data != null)
            //    DumpHex(data);


            //byte[] data = GetSMHDBoxData();
            //if (data != null)
            //    DumpHex(data);

            //byte[] data = GetURLBoxData(string.Empty);
            //if (data != null)
            //    DumpHex(data);

            //string url = string.Empty;
            //Mp4BoxURL box = Mp4BoxURL.CreateURLBox(url);
            //if (box != null)
            //{
            //    List<Mp4Box> l = new List<Mp4Box>();
            //    if (l != null)
            //    {
            //        l.Add(box);
            //        byte[] data = GetDREFBoxData((Int32)l.Count, l);
            //        if (data != null)
            //            DumpHex(data);
            //    }
            //}

            //string url = string.Empty;
            //Mp4BoxURL box = Mp4BoxURL.CreateURLBox(url);
            //if (box != null)
            //{
            //    List<Mp4Box> l = new List<Mp4Box>();
            //    if (l != null)
            //    {
            //        l.Add(box);
            //        Mp4BoxDREF boxDREF = Mp4BoxDREF.CreateDREFBox((Int32)l.Count, l);
            //        if (boxDREF != null)
            //        {
            //            List<Mp4Box> ldref = new List<Mp4Box>();
            //            if (ldref != null)
            //            {
            //                ldref.Add(boxDREF);
            //                byte[] data = GetDINFBoxData(ldref);
            //                if (data != null)
            //                    DumpHex(data);
            //            }
            //        }
            //    }
            //}

            // InsertFTYPMOOV(opt.InputUri);
            //Mp4BoxESDS box = Mp4BoxESDS.CreateESDSBox(1024, 64000, 44100, 2);
            //if(box!=null)
            //{
            //    byte[] data = box.GetBoxBytes();
            //    if (data != null)
            //                  DumpHex(data);
            //}


            // Int16 RefIndex = 1;
            // Int16 ChannelCount = 2;
            // Int16 SampleSize = 16;
            // Int32 SampleRate = 44100;
            // Int32 Bitrate = 64000;
            // Int32 FrameLength = 1024;

            //Mp4BoxESDS box = Mp4BoxESDS.CreateESDSBox(FrameLength, Bitrate, SampleRate, ChannelCount);
            // if (box != null)
            // {
            //     List<Mp4Box> list = new List<Mp4Box>();
            //     if(list!=null)
            //     {
            //         list.Add(box);
            //         Mp4BoxMP4A boxend = Mp4BoxMP4A.CreateMP4ABox(RefIndex, ChannelCount, SampleSize, SampleRate, list);
            //         byte[] data = boxend.GetBoxBytes();
            //         if (data != null)
            //             DumpHex(data);
            //     }

            // }

            //Byte ConfigurationVersion = 0x01;
            //Byte AVCProfileIndication = 0x64;
            //Byte ProfileCompatibility = 0x00;
            //Byte AVCLevelIndication = 0x1E;
            //Byte[] SPSNALUContent = { 0x27, 0x64, 0x00, 0x1E, 0xAC, 0x2C, 0xA5, 0x02, 0x80, 0xBF, 0xE5, 0xC0, 0x44, 0x00, 0x00, 0x0F,
            //                          0xA4, 0x00, 0x03, 0xA9, 0x82, 0x50};

            //Byte[] PPSNALUContent = { 0x28, 0xE9, 0x30, 0x60, 0xCC, 0xB0};
            //Int32 BufferSize = 118750;
            //Int32 MaxBitrate = 950000;
            //Int32 AvrBitrate = 550000;
            //Int16 TrackID = 1;
            //Int16 RefIndex = TrackID;
            //Int16 Width = 640;
            //Int16 Height = 360;
            //Int16 HorizontalRes = 72;
            //Int16 VerticalRes = 72;
            //Int16 FrameCount = 1;
            //Int16 Depth = 24;

            //string handler_type = "vide";
            //string handler_name = "Video Media Handler\0";
            //DateTime CreationTime = DateTime.Now;
            //DateTime UpdateTime = DateTime.Now;
            //Int32 TimeScale = 10000000;
            //Int64 Duration = 120 * 10000000;
            //string LanguageCode = "eng";
            //Int32 Flags = 7;


            //Mp4BoxAVCC avccbox = Mp4BoxAVCC.CreateAVCCBox(ConfigurationVersion, AVCProfileIndication, ProfileCompatibility, AVCLevelIndication, SPSNALUContent, PPSNALUContent);
            //Mp4BoxBTRT btrtbox = Mp4BoxBTRT.CreateBTRTBox(BufferSize, MaxBitrate, AvrBitrate);
            //if ((avccbox != null)&& (btrtbox != null))
            //{
            //    List<Mp4Box> list = new List<Mp4Box>();
            //    if (list != null)
            //    {
            //        list.Add(avccbox);
            //        list.Add(btrtbox);
            //        Mp4BoxAVC1 boxavc1 = Mp4BoxAVC1.CreateAVC1Box(RefIndex, Width, Height, HorizontalRes, VerticalRes, FrameCount, Depth,list);
            //        if (boxavc1 != null)
            //        {
            //            list.Clear();
            //            list.Add(boxavc1);
            //            Mp4BoxSTSD boxstsd = Mp4BoxSTSD.CreateSTSDBox(1, list);
            //            Mp4BoxSTTS boxstts = Mp4BoxSTTS.CreateSTTSBox(0);
            //            Mp4BoxCTTS boxctts = Mp4BoxCTTS.CreateCTTSBox(0);
            //            Mp4BoxSTSC boxstsc = Mp4BoxSTSC.CreateSTSCBox(0);
            //            Mp4BoxSTCO boxstco = Mp4BoxSTCO.CreateSTCOBox(0);
            //            Mp4BoxSTSZ boxstsz = Mp4BoxSTSZ.CreateSTSZBox(0,0);
            //            if ((boxstsd!=null)&&
            //                (boxstts != null) &&
            //                (boxctts != null) &&
            //                (boxstsc != null) &&
            //                (boxstco != null) &&
            //                (boxstsz != null) 
            //                )
            //            {
            //                list.Clear();
            //                list.Add(boxstts);
            //                list.Add(boxctts);
            //                list.Add(boxstsc);
            //                list.Add(boxstco);
            //                list.Add(boxstsz);
            //                list.Add(boxstsd);
            //                Mp4BoxSTBL boxstbl = Mp4BoxSTBL.CreateSTBLBox(list);
            //                if (boxstbl != null)
            //                {
            //                    string url = string.Empty;
            //                    Mp4BoxURL boxurl = Mp4BoxURL.CreateURLBox(url);
            //                    if (boxurl != null)
            //                    {
            //                        list.Clear();
            //                        list.Add(boxurl);
            //                        Mp4BoxDREF boxdref = Mp4BoxDREF.CreateDREFBox((Int32)list.Count, list);
            //                        if (boxdref != null)
            //                        {
            //                            list.Clear();

            //                            list.Add(boxdref);
            //                            Mp4BoxDINF boxdinf = Mp4BoxDINF.CreateDINFBox(list);
            //                            if (boxdinf != null)
            //                            {
            //                                Mp4BoxVMHD boxvmhd = Mp4BoxVMHD.CreateVMHDBox();
            //                                if(boxvmhd != null)
            //                                {
            //                                    list.Clear();
            //                                    list.Add(boxvmhd);
            //                                    list.Add(boxdinf);
            //                                    list.Add(boxstbl);

            //                                    Mp4BoxMINF boxminf = Mp4BoxMINF.CreateMINFBox(list);
            //                                    if(boxminf != null)
            //                                    {
            //                                        Mp4BoxHDLR boxhdlr = Mp4BoxHDLR.CreateHDLRBox(handler_type, handler_name);
            //                                        if (boxhdlr != null)
            //                                        {
            //                                            Mp4BoxMDHD boxmdhd = Mp4BoxMDHD.CreateMDHDBox(CreationTime, UpdateTime, TimeScale, Duration, LanguageCode);
            //                                            if (boxmdhd != null)
            //                                            {

            //                                                list.Clear();
            //                                                list.Add(boxmdhd);
            //                                                list.Add(boxhdlr);
            //                                                list.Add(boxminf);
            //                                                Mp4BoxMDIA boxmdia = Mp4BoxMDIA.CreateMDIABox(list);
            //                                                if (boxmdia != null)
            //                                                {
            //                                                    Mp4BoxTKHD boxtkhd = Mp4BoxTKHD.CreateTKHDBox(Flags, CreationTime, UpdateTime, TrackID, Duration, false,Width,Height);
            //                                                    if (boxtkhd != null)
            //                                                    {
            //                                                        list.Clear();
            //                                                        list.Add(boxtkhd);
            //                                                        list.Add(boxmdia);
            //                                                        Mp4BoxTRAK boxtrak = Mp4BoxTRAK.CreateTRAKBox(list);
            //                                                        if (boxtrak != null)
            //                                                        {
            //                                                            Mp4BoxMVHD boxmvhd = Mp4BoxMVHD.CreateMVHDBox(CreationTime, UpdateTime, TimeScale, Duration, TrackID+1);
            //                                                            if (boxmvhd != null)
            //                                                            {
            //                                                                list.Clear();
            //                                                                list.Add(boxmvhd);
            //                                                                list.Add(boxtrak);
            //                                                                Mp4BoxMOOV boxmoov = Mp4BoxMOOV.CreateMOOVBox(list);
            //                                                                if (boxmoov != null)
            //                                                                {
            //                                                                    List<string> l = new List<string>();
            //                                                                    l.Add("piff");
            //                                                                    l.Add("iso2");
            //                                                                    Mp4BoxFTYP boxftyp = Mp4BoxFTYP.CreateFTYPBox("isml", 1, l);
            //                                                                    if (boxftyp != null)
            //                                                                    {
            //                                                                        byte[] data = boxmoov.GetBoxBytes();
            //                                                                        if (data != null)
            //                                                                            DumpHex(data);
            //                                                                        data = boxftyp.GetBoxBytes();
            //                                                                        if (data != null)
            //                                                                            DumpHex(data);
            //                                                                    }
            //                                                                }
            //                                                            }
            //                                                        }
            //                                                    }
            //                                                }
            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }

            //                    }


            //                }
            //            }
            //        }
            //    }

            //}


            //Int32 BufferSize = 118750;
            //Int32 MaxBitrate = 950000;
            //Int32 AvgBitrate = 550000;
            //Int16 TrackID = 2;
            //Int16 Width = 640;
            //Int16 Height = 396;
            //            Byte[] SPSNALUContent = { 0x27, 0x64, 0x00, 0x1E, 0xAC, 0x2C, 0xA5, 0x02, 0x80, 0xBF, 0xE5, 0xC0, 0x44, 0x00, 0x00, 0x0F,
            //                                      0xA4, 0x00, 0x03, 0xA9, 0x82, 0x50};
            //Byte[] SPSNALUContent = { 0x27, 0x64, 0x00, 0x1F, 0xAC, 0x2E, 0xC0, 0xA0, 0x33, 0xFB, 0xFF, 0xC0, 0xFF, 0xC1, 0x00, 0x04,
            //                          0x00, 0x16, 0xE3, 0x60, 0x0C, 0xDF, 0xE6, 0x03, 0xA1, 0x00, 0x7A, 0x00, 0x00, 0xF4, 0x25, 0xBD,
            //                          0xEF, 0x83, 0xB4, 0x38, 0x62, 0x70};



            //            Byte[] PPSNALUContent = { 0x28, 0xE9, 0x30, 0x60, 0xCC, 0xB0 };
            //Byte[] PPSNALUContent = { 0x28, 0xEE, 0x3C, 0xB0 };


            //DateTime CreationTime = DateTime.Now;
            //DateTime UpdateTime = DateTime.Now;
            //Int32 TimeScale = 10000000;
            //Int64 Duration = 1979501134;
            //string LanguageCode = "eng";

            //Mp4BoxMOOV boxmoov = CreateVideoMOOVBox(TrackID, Width, Height, TimeScale, Duration, LanguageCode, SPSNALUContent, PPSNALUContent);
            //if (boxmoov != null)
            //{
            //    List<string> l = new List<string>();
            //    l.Add("piff");
            //    l.Add("iso2");
            //    Mp4BoxFTYP boxftyp = CreateFTYPBox();
            //    if (boxftyp != null)
            //    {
            //        byte[] data = boxmoov.GetBoxBytes();
            //        if (data != null)
            //            DumpHex(data);
            //        data = boxftyp.GetBoxBytes();
            //        if (data != null)
            //            DumpHex(data);
            //        if (InsertFTYPMOOV(opt.InputUri, boxftyp, boxmoov) == true)
            //            result = true;
            //    }
            //}

            Int16 TrackID = 1;
            int MaxFrameSize = 1024;
            int Bitrate = 64000;
            int SampleSize = 16;
            int SampleRate = 44100;
            int Channels = 2;
            Int32 TimeScale = 10000000;
            Int64 Duration = 1979501134;
            string LanguageCode = "eng";

            DateTime CreationTime = DateTime.Now;
            DateTime UpdateTime = DateTime.Now;


            Mp4BoxMOOV boxmoov = CreateAudioMOOVBox(TrackID, MaxFrameSize, Bitrate, SampleSize,SampleRate,Channels, TimeScale, Duration, LanguageCode);
            if (boxmoov != null)
            {
                List<string> l = new List<string>();
                l.Add("piff");
                l.Add("iso2");
                Mp4BoxFTYP boxftyp = CreateFTYPBox();
                if (boxftyp != null)
                {
                    byte[] data = boxmoov.GetBoxBytes();
                    if (data != null)
                        DumpHex(data);
                    data = boxftyp.GetBoxBytes();
                    if (data != null)
                        DumpHex(data);
                    if (InsertFTYPMOOV(opt.InputUri, boxftyp, boxmoov) == true)
                        result = true;
                }
            }


            return result;
        }
    }
}
