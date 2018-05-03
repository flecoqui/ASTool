using System;
using System.Collections.Generic;
using System.Text;
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

            string url = string.Empty;
            Mp4BoxURL box = Mp4BoxURL.CreateURLBox(url);
            if (box != null)
            {
                List<Mp4Box> l = new List<Mp4Box>();
                if (l != null)
                {
                    l.Add(box);
                    Mp4BoxDREF boxDREF = Mp4BoxDREF.CreateDREFBox((Int32)l.Count, l);
                    if (boxDREF != null)
                    {
                        List<Mp4Box> ldref = new List<Mp4Box>();
                        if (ldref != null)
                        {
                            ldref.Add(boxDREF);
                            byte[] data = GetDINFBoxData(ldref);
                            if (data != null)
                                DumpHex(data);
                        }
                    }
                }
            }
            return result;
        }
    }
}
