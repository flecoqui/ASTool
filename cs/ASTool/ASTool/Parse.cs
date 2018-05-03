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
        static byte[] GetMVHDBoxData(DateTime CreationTime, DateTime ModificationTime, Int32 TimeScale, Int64 Duration, Int32 NextTrackID)
        {
            Mp4BoxMVHD box = Mp4BoxMVHD.CreateMVHDBox( CreationTime, ModificationTime,  TimeScale, Duration,  NextTrackID);
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
            Int32 TimeScale = 10000000;
            Int64 Duration = 120*10000000;
            byte[] data = GetMVHDBoxData(DateTime.Now, DateTime.Now, TimeScale, Duration, 3);
            if(data!=null)
                DumpHex(data);
            return result;
        }
    }
}
