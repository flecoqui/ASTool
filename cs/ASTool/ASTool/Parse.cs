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
        public static bool InsertFTYPMOOV(string path)
        {
            bool result = false;
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {

                    FileStream fso = new FileStream(path + ".isma", FileMode.Create, FileAccess.ReadWrite);
                    if (fso != null)
                    {
                        byte[] data = GetFTYPBoxData();
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
                    fs.Close();
                }
            }
            catch(Exception ex)
            {
                result = false;
            }
            
            return result;
        }



        static bool Parse(Options opt)
        {
            bool result = false;
            Console.WriteLine("Parsing file: " + opt.InputUri);

            Console.Write(Mp4Box.ParseFile(opt.InputUri));
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
            Int16 RefIndex = 1;
            Int16 ChannelCount = 2;
            Int16 SampleSize = 16;
            Int32 SampleRate = 44100;
            Int32 Bitrate = 64000;
            Int32 FrameLength = 1024;

           Mp4BoxESDS box = Mp4BoxESDS.CreateESDSBox(FrameLength, Bitrate, SampleRate, ChannelCount);
            if (box != null)
            {
                List<Mp4Box> list = new List<Mp4Box>();
                if(list!=null)
                {
                    list.Add(box);
                    Mp4BoxMP4A boxend = Mp4BoxMP4A.CreateMP4ABox(RefIndex, ChannelCount, SampleSize, SampleRate, list);
                    byte[] data = boxend.GetBoxBytes();
                    if (data != null)
                        DumpHex(data);
                }

            }
            return result;
        }
    }
}
