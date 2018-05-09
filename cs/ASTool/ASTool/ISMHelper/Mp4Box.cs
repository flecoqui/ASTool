using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    public class Mp4Box
    {
        protected Int32 Length;
        protected string Type;
        protected byte[] Data;
        protected List<Mp4Box> Children;
        public Int32 GetBoxLength()
        {
            return Length;
        }
        public string GetBoxType()
        {
            return Type;
        }
        public byte[] GetBoxData()
        {
            return Data;
        }
        public byte[] GetBoxBytes()
        {
            byte[] result = new byte[Length];
            if(WriteMp4BoxInt32(result, 0, Length))
            {
                if(WriteMp4BoxString(result,4,this.Type))
                {
                    if (WriteMp4BoxData(result, 8, this.Data))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
        public bool AddMp4Box(Mp4Box box, bool bAddInData = false)
        {
            if (Children == null)
                Children = new List<Mp4Box>();
            if (Children != null)
            {
                Children.Add(box);
                if(bAddInData == true)
                {
                    Append(box.Data);
                }
                return true;
            }
            return false;
        }
        public bool Append(byte[] data)
        {
            if ((data != null)&&(Data != null))
            {
                byte[] newData = new byte[data.Length + Data.Length];
                Buffer.BlockCopy(Data, 0, newData, 0, Data.Length);
                Buffer.BlockCopy(data, 0, newData, Data.Length,data.Length);
                Data = newData;
                return true;
            }
            return false;
        }
        public static Mp4Box CreateMp4Box(byte[] buffer, int offset)
        {
            if((buffer != null)&&
                (offset+8 < buffer.Length))
            {
                Mp4Box box = new Mp4Box();
                if(box!=null)
                {
                    box.Length = ReadMp4BoxLength(buffer, offset);
                    if ((offset + box.Length <= buffer.Length)&&(box.Length>8))
                    {
                        box.Type = ReadMp4BoxType(buffer, offset);
                        box.Data = ReadMp4BoxData(buffer, offset, box.Length);
                        List<Mp4Box> list = box.GetChildren();
                        if((list!=null)&&(list.Count>0))
                        {
                            foreach (var b in list)
                                box.AddMp4Box(box);
                        }
                        return box;
                    }
                }
            }
            return null;
        }
        public static Mp4Box CreateEmptyMp4Box(string Type)
        {
            Mp4Box box = new Mp4Box();
            if (box != null)
            {
                box.Length = 8;
                if (!string.IsNullOrEmpty(Type) &&
                    (Type.Length <= 4))
                {
                    box.Type = Type;
                    return box;
                }
            }
            
            return null;
        }
        static public int ReadMp4BoxLength(byte[] buffer, int offset)
        {
            int mp4BoxLen = 0;
            mp4BoxLen |= (int)(buffer[offset + 0] << 24);
            mp4BoxLen |= (int)(buffer[offset + 1] << 16);
            mp4BoxLen |= (int)(buffer[offset + 2] << 8);
            mp4BoxLen |= (int)(buffer[offset + 3] << 0);
            return mp4BoxLen;
        }
        static public int ReadMp4BoxInt32(byte[] buffer, int offset)
        {
            int Len = 0;
            Len |= (int)(buffer[offset + 0] << 24);
            Len |= (int)(buffer[offset + 1] << 16);
            Len |= (int)(buffer[offset + 2] << 8);
            Len |= (int)(buffer[offset + 3] << 0);
            return Len;
        }

        static public string ReadMp4BoxType(byte[] buffer, int offset)
        {
            string s = string.Empty;
            if ((offset + 8) > buffer.Length)
            {
                return s;
            }
            char[] array = new char[4];
            for (int i = 0; i < 4; i++)
                array[0 + i] = (char)buffer[offset + 4 + i];
            
            s = new string(array);
            return s;
        }
        static public byte[] ReadMp4BoxData(byte[] buffer, int offset, int Length)
        {
            if ((offset + Length) > buffer.Length)
            {
                return null;
            }
            byte[] array = new byte[Length-8];
            for (int i = 0; i < Length - 8; i++)
                array[i] = buffer[offset + 8 + i];
            return array;
        }
        static public byte[] ReadMp4BoxBytes(byte[] buffer, int offset, int Length)
        {
            if ((offset + Length) > buffer.Length)
            {
                return null;
            }
            byte[] array = new byte[Length];
            for (int i = 0; i < Length; i++)
                array[i] = buffer[offset  + i];
            return array;
        }
        static public bool WriteMp4BoxInt64(byte[] buffer, int offset, Int64 value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value >> 56);
                buffer[offset++] = (byte)(value >> 48);
                buffer[offset++] = (byte)(value >> 40);
                buffer[offset++] = (byte)(value >> 32);
                buffer[offset++] = (byte)(value >> 24);
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxInt32(byte[] buffer, int offset, Int32 value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value >> 24);
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxInt24(byte[] buffer, int offset, Int32 value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxInt16(byte[] buffer, int offset, Int16 value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxByte(byte[] buffer, int offset, byte value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value);
                return true;
            }
            return false;
        }
        static public Int64 GetMp4BoxTime(DateTime time)
        {
            DateTime Begin = new DateTime(1904, 1, 1, 0, 0, 0);
            TimeSpan t = time - Begin;
            Int64 Total = (Int64) t.TotalSeconds;
            return Total;
        }

        static public bool WriteMp4BoxString(byte[] buffer, int offset, string Message)
        {
            if ((buffer != null)&&(!string.IsNullOrEmpty(Message)))
            {
                char[] array = Message.ToCharArray();
                if(offset + array.Length <= buffer.Length)
                {
                    for(int i = 0; i < array.Length; i++)
                    {
                        buffer[offset+i] = (byte)array[i];
                    }
                    return true;
                }
            }
            return false;
        }
        static public bool WriteMp4BoxString(byte[] buffer, int offset, string Message, int MessageLength)
        {
            if ((buffer != null) && (!string.IsNullOrEmpty(Message)))
            {
                char[] array = Message.ToCharArray();
                if ((array.Length >= MessageLength) && (offset + MessageLength <= buffer.Length))
                {
                    for (int i = 0; i < MessageLength; i++)
                    {
                        buffer[offset + i] = (byte)array[i];
                    }
                    return true;
                }
            }
            return false;
        }
        static public bool WriteMp4BoxData(byte[] buffer, int offset, byte[] data)
        {
            if ((buffer != null) && (data != null))
            {
                if (offset + data.Length <= buffer.Length)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        buffer[offset + i] = (byte)data[i];
                    }
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            return "Box: " + Type + "\tLength: " + Length.ToString();
        }
        private static Mp4Box ReadMp4Box(FileStream fs)
        {
            Mp4Box box = null;
            if (fs != null)
            {
                byte[] buffer = new byte[4];
                if (buffer!=null)
                {
                    if (fs.Read(buffer, 0, 4) == 4)
                    {
                        int mp4BoxLen = 0;
                        mp4BoxLen |= (int)(buffer[0] << 24);
                        mp4BoxLen |= (int)(buffer[1] << 16);
                        mp4BoxLen |= (int)(buffer[2] << 8);
                        mp4BoxLen |= (int)(buffer[3] << 0);
                        if(mp4BoxLen >= 8)
                        {
                            buffer = new byte[mp4BoxLen];
                            if(buffer!=null)
                            {
                                WriteMp4BoxInt32(buffer, 0, mp4BoxLen);
                                if (fs.Read(buffer, 4, mp4BoxLen-4) == (mp4BoxLen - 4))
                                {

                                    return CreateMp4Box(buffer, 0);
                                }
                            }

                        }
                    }
                }
            }
            return box;
        }
        public List<Mp4Box> GetChildren()
        {
            List<Mp4Box> list = new List<Mp4Box>();
            if ((list!=null)&&(Data!=null))
            {
                int offset = 0;
                while (offset < Data.Length)
                {
                    Mp4Box box = CreateMp4Box(Data, offset);
                    if (box != null)
                    {
                        list.Add(box);
                        offset += box.Length;
                    }
                    else
                        break;
                }
            }
            return list;
        }
        public static string GetBoxChildrenString(int level, Mp4Box box)
        {
            string result = string.Empty;
            int locallevel = level + 1;
            if(box!=null)
            {
                List<Mp4Box> list = box.GetChildren();
                if((list!=null)&&(list.Count>0))
                {
                    foreach(var m in list)
                    {
                        string prefix = string.Empty;
                        for (int i = 0; i < locallevel; i++) prefix += "\t\t";
                        result += prefix + m.ToString() + "\r\n";
                        result += GetBoxChildrenString(locallevel, m);
                    }
                }
            }
            return result;
        }

        public static string ParseFile(string Path)
        {
            string result = "\r\n";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    long offset = 0;
                    fs.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fs.Length)
                    {
                        Mp4Box box = ReadMp4Box(fs);
                        if (box != null)
                        {
                            result += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";
                            if(box.Type!="mdat\0")
                                result += GetBoxChildrenString(0,box);

                            
                            offset += box.Length;
                        }
                        else
                            break;
                    }
                    fs.Close();

                }

            }
            catch (Exception ex)
            {
                result += "ERROR: Exception while parsing the file: " + ex.Message;
            }
            return result;
        }

        public static string ParseFileVerbose(string Path)
        {
            string result = "\r\n";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    long offset = 0;
                    fs.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fs.Length)
                    {
                        Mp4Box box = ReadMp4Box(fs);
                        if (box != null)
                        {
                            result += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";
                            if (box.Type != "mdat\0")
                                result += GetBoxChildrenString(0, box);
                            result += Options.DumpHex(box.GetBoxBytes());
                            offset += box.Length;
                        }
                        else
                            break;
                    }
                    fs.Close();

                }

            }
            catch (Exception ex)
            {
                result += "ERROR: Exception while parsing the file: " + ex.Message;
            }
            return result;
        }
        public static byte[] HexStringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 255; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
                hexres.Add(hexindex[str.Substring(i, 2)]);

            return hexres.ToArray();
        }
        public static byte[] GetSPSNALUContent(string HexString)
        {
            // Hack to be confirmed
            if(!string.IsNullOrEmpty(HexString))
            {
                string[] value = HexString.Split("00000001");
                if((value!=null)&&(value.Length == 3))
                {
                    return HexStringToByteArray(value[1]);
                }
            }
            return null;
        }
        public static byte[] GetPPSNALUContent(string HexString)
        {
            // Hack to be confirmed
            if (!string.IsNullOrEmpty(HexString))
            {
                string[] value = HexString.Split("00000001");
                if ((value != null) && (value.Length == 3))
                {
                    return HexStringToByteArray(value[2]);
                }
            }
            return null;
        }
        public static Mp4BoxFTYP CreateFTYPBox()
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
        public static Mp4BoxMOOV CreateVideoMOOVBox(Int16 TrackID, 
            Int16 Width, Int16 Height, Int32 TimeScale, Int64 Duration, string LanguageCode,
            string CodecPrivateData)
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
            Byte[] SPSNALUContent = GetSPSNALUContent(CodecPrivateData);
            Byte[] PPSNALUContent = GetPPSNALUContent(CodecPrivateData);
            if (string.IsNullOrEmpty(LanguageCode))
                LanguageCode = "und";

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
        public static Mp4BoxMOOV CreateAudioMOOVBox(Int16 TrackID, int MaxFrameSize, int Bitrate, int SampleSize, int SampleRate, int Channels, Int32 TimeScale, Int64 Duration, string LanguageCode, string CodecPrivateData)

        {
            Int16 RefIndex = 1;


            string handler_type = "soun";
            string handler_name = "Audio\0";
            DateTime CreationTime = DateTime.Now;
            DateTime UpdateTime = DateTime.Now;
            Int32 Flags = 7;
            if (string.IsNullOrEmpty(LanguageCode))
                LanguageCode = "und";
            Mp4BoxESDS boxesds = Mp4BoxESDS.CreateESDSBox(MaxFrameSize, Bitrate, SampleRate, Channels, CodecPrivateData);
            if (boxesds != null)
            {
                List<Mp4Box> list = new List<Mp4Box>();
                if (list != null)
                {
                    list.Add(boxesds);
                    Mp4BoxMP4A boxmp4a = Mp4BoxMP4A.CreateMP4ABox(RefIndex, (Int16)Channels, (Int16)SampleSize, SampleRate, list);
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
        public static Mp4BoxMOOV CreateTextMOOVBox(Int16 TrackID, Int32 TimeScale, Int64 Duration, string LanguageCode)
        {
            Int16 RefIndex = 1;


            string handler_type = "dfxp";
            string handler_name = "DFXP Handler\0";
            DateTime CreationTime = DateTime.Now;
            DateTime UpdateTime = DateTime.Now;
            int Width = 0;
            int Height = 0;
            Int32 Flags = 7;

            if (string.IsNullOrEmpty(LanguageCode))
                LanguageCode = "und";


            List<Mp4Box> list = new List<Mp4Box>();
            if (list != null)
            {
                Mp4BoxDFXP boxdfxp = Mp4BoxDFXP.CreateDFXPBox(RefIndex);
                if (boxdfxp != null)
                {
                    list.Clear();
                    list.Add(boxdfxp);
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
                                        Mp4BoxNMHD boxnmhd = Mp4BoxNMHD.CreateNMHDBox();
                                        if (boxnmhd != null)
                                        {
                                            list.Clear();
                                            list.Add(boxnmhd);
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
            return null;
        }

    }
}
