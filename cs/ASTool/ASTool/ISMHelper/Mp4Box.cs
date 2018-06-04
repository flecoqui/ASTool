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

    public class Mp4Box
    {
        public static Guid kExtPiffTrackEncryptionBoxGuid = new Guid("{8974DBCE-7BE7-4C51-84F9-7148F9882554}");
        public static Guid kExtProtectHeaderBoxGuid = new Guid("{d08a4f18-10f3-4a82-b6c8-32d8aba183d3}");
        public static Guid kExtProtectHeaderMOOFBoxGuid = new Guid("{a2394f52-5a9b-4f14-a244-6c427c648df4}");
        protected Int32 Length;
        protected string Type;
        protected byte[] Data;
        protected List<Mp4Box> Children;
        protected string Path;
        protected Mp4Box Parent;
        protected int ChildrenOffset = 0;
        public Int32 GetBoxLength()
        {
            return Length;
        }
        public void SetBoxLength(int Len)
        {
            Length = Len;
        }
        public string GetBoxType()
        {
            return Type;
        }
        public void SetBoxType(string t)
        {
            Type = t;
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
        public Mp4Box GetParent()
        {
            return Parent;
        }
        public void SetParent(Mp4Box box)
        {
            this.Path = box.GetPath() + "/" + this.GetBoxType();
            Parent = box;
        }
        public void  SetPath(string path)
        {
            Path = path;            
        }
        public string GetPath()
        {
            if (string.IsNullOrEmpty(Path))
                Path = "/" + Type;
            return Path;
        }
        public bool RemoveChildBox(string BoxType)
        {
            if (Children != null)
            {
                foreach (var box in Children)
                {
                    if (box.GetBoxType() == BoxType)
                    {
                        Children.Remove(box);
                        return true;
                    }
                }
            }
            return false;
        }
        public Mp4Box FindChildBox(string BoxType)
        {
            if (Children != null)
            {
                foreach (var box in Children)
                {
                    if (box.GetBoxType() == BoxType)
                    {
                        return box;
                    }
                    else
                    {
                        Mp4Box ChildBox = box.FindChildBox(BoxType);
                        if (ChildBox != null)
                            return ChildBox;
                    }
                }
            }
            return null;
        }
        public bool RemoveUUIDBox(Guid id)
        {
            bool result = false;
            if (Children != null)
            {
                foreach (var box in Children)
                {
                    if (box.GetBoxType() == "uuid")
                    {
                        Mp4BoxUUID uuidbox = box as Mp4BoxUUID;
                        if (uuidbox != null)
                        {
                            if (uuidbox.GetUUID() == id)
                            {
                                int Len = uuidbox.GetBoxLength();
                                UpdateParentLength(uuidbox, -Len);
                                Children.Remove(box);
                                result = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        bool res = box.RemoveUUIDBox(id);
                        if (res == true)
                        {
                            result = true;
                            break;
                        }

                    }
                }
            }
            return result;
        }
        public byte[] UpdateBoxBuffer()
        {
            byte[] buffer = new byte[this.Length];

            if  (buffer != null)
            {
                int offset = 0;
                WriteMp4BoxInt32(buffer, offset, this.Length);
                offset += 4;
                WriteMp4BoxString(buffer, offset, this.Type);
                offset += 4;
                if (this.Children != null)
                {
                    if (this.ChildrenOffset > 0)
                    {
                        WriteMp4BoxData(buffer, offset, this.Data, this.ChildrenOffset);
                        offset += this.ChildrenOffset;
                    }

                    foreach (var box in Children)
                    {
                        byte[] childBuffer = box.UpdateBoxBuffer();
                        if (childBuffer != null)
                        {
                            WriteMp4BoxData(buffer, offset, childBuffer);
                            offset += childBuffer.Length;
                        }
                    }
                }
                else
                {
                    WriteMp4BoxData(buffer, offset, this.Data);
                }
            }
            return buffer;
        }
        public void UpdateParentLength(Mp4Box box, int Len)
        {
            Mp4Box pbox = box.GetParent();
            while (pbox != null)
            {
                pbox.SetBoxLength(pbox.GetBoxLength() + Len);
                pbox = pbox.GetParent();
            }
        }
        public bool AddMp4Box(Mp4Box box, bool bAddInData = false)
        {
            if (Children == null)
                Children = new List<Mp4Box>();
            if (Children != null)
            {
                box.SetParent(this);
                box.SetPath(this.GetPath() + "/" + box.GetType());
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
        public static Mp4Box CreateMp4BoxFromType(string BoxType)
        {
            switch(BoxType)
            {
                case "ftyp":
                    return new Mp4BoxFTYP();
                case "moov":
                    return new Mp4BoxMOOV();
                case "mvhd":
                    return new Mp4BoxMVHD();
                case "uuid":
                    return new Mp4BoxUUID();
                case "trak":
                    return new Mp4BoxTRAK();
                case "tkhd":
                    return new Mp4BoxTKHD();
                case "mdia":
                    return new Mp4BoxMDIA();
                case "mdhd":
                    return new Mp4BoxMDHD();
                case "hdlr":
                    return new Mp4BoxHDLR();
                case "enca":
                    return new Mp4BoxENCA();
                case "encv":
                    return new Mp4BoxENCV();
                case "enct":
                    return new Mp4BoxENCT();
                case "moof":
                    return new Mp4BoxMOOF();
                case "mfhd":
                    return new Mp4BoxMFHD();
                case "traf":
                    return new Mp4BoxTRAF();
                case "tfhd":
                    return new Mp4BoxTFHD();
                case "trun":
                    return new Mp4BoxTRUN();
                case "sdtp":
                    return new Mp4BoxSDTP();
                default:
                    return new Mp4Box();
            }
        }
        public static Mp4Box CreateMp4Box(byte[] buffer, int offset)
        {
            if((buffer != null)&&
                (offset+8 < buffer.Length))
            {
                Mp4Box box = CreateMp4BoxFromType(ReadMp4BoxType(buffer, offset));
                if (box!=null)
                {
                    box.Length = ReadMp4BoxLength(buffer, offset);
                    if ((offset + box.Length <= buffer.Length)&&(box.Length>8))
                    {
                        box.Type = ReadMp4BoxType(buffer, offset);
                        box.SetPath("/" + box.GetBoxType());
                        box.Data = ReadMp4BoxData(buffer, offset, box.Length);
                        List<Mp4Box> list = box.GetChildren();
                        if((list!=null)&&(list.Count>0))
                        {
                            foreach (var b in list)
                                box.AddMp4Box(b);
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
        static public int ReadMp4BoxInt16(byte[] buffer, int offset)
        {
            int Len = 0;
            Len |= (int)(buffer[offset + 1] << 8);
            Len |= (int)(buffer[offset + 0] << 0);
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
        static public bool WriteMp4BoxInt8(byte[] buffer, int offset, Int16 value)
        {
            if (buffer != null)
            {
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
        static public bool WriteMp4BoxData(byte[] buffer, int offset, byte[] data, int Length)
        {
            if ((buffer != null) && (data != null))
            {
                if (offset + Length <= buffer.Length)
                {
                    for (int i = 0; i < Length; i++)
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
        public static Mp4Box ReadMp4Box(FileStream fs)
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
        public static bool WriteMp4Box(Mp4Box box, FileStream fs)
        {
            bool result = false;
            if ((box != null)&&(fs != null))
            {
                try
                {
                    byte[] header = new byte[8];
                    Mp4Box.WriteMp4BoxInt32(header, 0, box.Length);
                    Mp4Box.WriteMp4BoxString(header, 4, box.GetBoxType());
                    fs.Write(header, 0, 8);
                    fs.Write(box.Data, 0, box.Data.Length);
                    result = true;
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.Write("Exception while writing box in file: " + ex.Message);
                }
            }
            return result;
        }
        public static bool WriteMp4BoxBuffer(byte[] buffer, FileStream fs)
        {
            bool result = false;
            if ((buffer != null) && (fs != null))
            {
                try
                {
                    fs.Write(buffer, 0, buffer.Length);
                    result = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write("Exception while writing buffer box in file: " + ex.Message);
                }
            }
            return result;
        }
        public List<Mp4Box> GetChildren()
        {
            List<Mp4Box> list = new List<Mp4Box>();
            if ((list!=null)&&(Data!=null))
            {
                ChildrenOffset = 0;
                if (this.GetBoxType() == "stsd")
                    ChildrenOffset = 8;
                else if (this.GetBoxType() == "dref")
                    ChildrenOffset = 8;
                else if (this.GetBoxType() == "encv")
                    ChildrenOffset = 78;
                else if (this.GetBoxType() == "enca")
                    ChildrenOffset = 28;
                int Offset = ChildrenOffset;
                while (Offset < Data.Length)
                {
                    Mp4Box box = CreateMp4Box(Data, Offset);
                    if (box != null)
                    {
                        list.Add(box);
                        Offset += box.Length;
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
        static Guid GetKIDFromProtectionData(string data)
        {
            Guid result = Guid.Empty;
            var base64EncodedBytes = System.Convert.FromBase64String(data);
            if (base64EncodedBytes != null)
            {
                int Len = ReadMp4BoxInt16(base64EncodedBytes, 0);
                if (Len == base64EncodedBytes.Length)
                {
                    string s = System.Text.Encoding.Unicode.GetString(base64EncodedBytes,10,Len-10);
                    if (!string.IsNullOrEmpty(s))
                    {
                        int start = s.IndexOf("<KID>");
                        int end = s.IndexOf("</KID>");
                        if ((start > 0) && (end > 0) && (start < end))
                        {
                            byte[] KIDBytes = System.Convert.FromBase64String(s.Substring(start + 5, end - start - 5));
                            if (KIDBytes != null)
                            {
                                result = new Guid(KIDBytes);
                            }
                        }
                    }
                }
            }
            return result;
        }
        public static Mp4BoxMOOV CreateVideoMOOVBox(Int16 TrackID, 
            Int16 Width, Int16 Height, Int32 TimeScale, Int64 Duration, string LanguageCode,
            string CodecPrivateData, Guid ProtectionGuid, string ProtectionData)
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
                    Mp4BoxSTSD boxstsd = null;
                    if ((!string.IsNullOrEmpty(ProtectionData)) &&
                            (ProtectionGuid != Guid.Empty))
                    {
                        int AlgorithmID = 1;
                        int SampleSize = 8;
                        Guid KID = GetKIDFromProtectionData(ProtectionData);
                        Mp4BoxUUID boxuuid = Mp4BoxUUID.CreateUUIDBox(kExtPiffTrackEncryptionBoxGuid, AlgorithmID,SampleSize,KID );
                        if (boxuuid != null)
                        {
                            list.Add(boxuuid);
                            Mp4BoxSCHI boxschi = Mp4BoxSCHI.CreateSCHIBox(list);
                            if (boxschi != null)
                            {
                                Mp4BoxFRMA boxfrma = Mp4BoxFRMA.CreateFRMABox("avc1");
                                if (boxfrma != null)
                                {
                                    Mp4BoxSCHM boxschm = Mp4BoxSCHM.CreateSCHMBox("piff",65537);
                                    if (boxfrma != null)
                                    {
                                        list.Clear();
                                        list.Add(boxfrma);
                                        list.Add(boxschm);
                                        list.Add(boxschi);
                                        Mp4BoxSINF boxsinf = Mp4BoxSINF.CreateSINFBox(list);
                                        if (boxsinf != null)
                                        {
                                            list.Clear();
                                            list.Add(avccbox);
                                            list.Add(boxsinf);
                                            Mp4BoxENCV boxencv = Mp4BoxENCV.CreateENCVBox(RefIndex, Width, Height, HorizontalRes, VerticalRes, FrameCount, Depth, list);
                                            if (boxencv != null)
                                            {
                                                list.Clear();
                                                list.Add(boxencv);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        list.Add(avccbox);
                        Mp4BoxAVC1 boxavc1 = Mp4BoxAVC1.CreateAVC1Box(RefIndex, Width, Height, HorizontalRes, VerticalRes, FrameCount, Depth, list);
                        if (boxavc1 != null)
                        {
                            list.Clear();
                            list.Add(boxavc1);
                        }
                    }
                    boxstsd = Mp4BoxSTSD.CreateSTSDBox(1, list);
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
                                                                                if ((!string.IsNullOrEmpty(ProtectionData))&&
                                                                                    (ProtectionGuid != Guid.Empty))
                                                                                {
                                                                                    Mp4BoxUUID boxuuid = Mp4BoxUUID.CreateUUIDBox(kExtProtectHeaderBoxGuid,ProtectionGuid,ProtectionData);
                                                                                    if (boxuuid != null)
                                                                                    {
                                                                                        list.Add(boxuuid);
                                                                                    }

                                                                                }

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
        public static Mp4BoxMOOV CreateAudioMOOVBox(Int16 TrackID, int MaxFrameSize, int Bitrate, int SampleSize, int SampleRate, int Channels, Int32 TimeScale, Int64 Duration, string LanguageCode, string CodecPrivateData, Guid ProtectionGuid, string ProtectionData)

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

                    Mp4BoxSTSD boxstsd = null;
                    if ((!string.IsNullOrEmpty(ProtectionData)) &&
                            (ProtectionGuid != Guid.Empty))
                    {
                        int AlgorithmID = 1;
                        int SampleIDSize = 8;
                        Guid KID = GetKIDFromProtectionData(ProtectionData);
                        Mp4BoxUUID boxuuid = Mp4BoxUUID.CreateUUIDBox(kExtPiffTrackEncryptionBoxGuid, AlgorithmID, SampleIDSize, KID);
                        if (boxuuid != null)
                        {
                            list.Add(boxuuid);
                            Mp4BoxSCHI boxschi = Mp4BoxSCHI.CreateSCHIBox(list);
                            if (boxschi != null)
                            {
                                Mp4BoxFRMA boxfrma = Mp4BoxFRMA.CreateFRMABox("mp4a");
                                if (boxfrma != null)
                                {
                                    Mp4BoxSCHM boxschm = Mp4BoxSCHM.CreateSCHMBox("piff", 65537);
                                    if (boxfrma != null)
                                    {
                                        list.Clear();
                                        list.Add(boxfrma);
                                        list.Add(boxschm);
                                        list.Add(boxschi);
                                        Mp4BoxSINF boxsinf = Mp4BoxSINF.CreateSINFBox(list);
                                        if (boxsinf != null)
                                        {
                                            list.Clear();
                                            list.Add(boxesds);
                                            list.Add(boxsinf);
                                            Mp4BoxENCA boxenca = Mp4BoxENCA.CreateENCABox(RefIndex, (Int16)Channels, (Int16)SampleSize, SampleRate, list);
                                            if (boxenca != null)
                                            {
                                                list.Clear();
                                                list.Add(boxenca);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        list.Add(boxesds);
                        Mp4BoxMP4A boxmp4a = Mp4BoxMP4A.CreateMP4ABox(RefIndex, (Int16)Channels, (Int16)SampleSize, SampleRate, list);
                        if (boxmp4a != null)
                        {
                            list.Clear();
                            list.Add(boxmp4a);
                        }
                    }

                    boxstsd = Mp4BoxSTSD.CreateSTSDBox(1, list);
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
                                                                                if ((!string.IsNullOrEmpty(ProtectionData)) &&
                                                                                        (ProtectionGuid != Guid.Empty))
                                                                                {
                                                                                    Mp4BoxUUID boxuuid = Mp4BoxUUID.CreateUUIDBox(kExtProtectHeaderBoxGuid, ProtectionGuid, ProtectionData);
                                                                                    if (boxuuid != null)
                                                                                    {
                                                                                        list.Add(boxuuid);
                                                                                    }

                                                                                }
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
        public static Mp4BoxMOOV CreateTextMOOVBox(Int16 TrackID, Int32 TimeScale, Int64 Duration, string LanguageCode,Guid ProtectionGuid, string ProtectionData)
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
                                                                                if ((!string.IsNullOrEmpty(ProtectionData)) &&
                                                                                        (ProtectionGuid != Guid.Empty))
                                                                                {
                                                                                    Mp4BoxUUID boxuuid = Mp4BoxUUID.CreateUUIDBox(kExtProtectHeaderBoxGuid, ProtectionGuid, ProtectionData);
                                                                                    if (boxuuid != null)
                                                                                    {
                                                                                        list.Add(boxuuid);
                                                                                    }

                                                                                }
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
        public static Mp4BoxMFRA CreateMFRABox(Int16 TrackID, List<TimeMoofOffset> listTimeOffset)
        {
            if (listTimeOffset != null)
            {
                List<Mp4Box> list = new List<Mp4Box>();
                if (list != null)
                {
                    Mp4BoxTFRA boxtfra = Mp4BoxTFRA.CreateTFRABox(TrackID, listTimeOffset);
                    if (boxtfra != null)
                    {

                        Mp4BoxMFRO boxmfro = Mp4BoxMFRO.CreateMFROBox(boxtfra.Length + 8 + 16);
                        if (boxmfro != null)
                        {
                            list.Add(boxtfra);
                            list.Add(boxmfro);
                            return Mp4BoxMFRA.CreateMFRABox(list);
                        }
                    }
                }
            }
            return null;
        }

    }
}
