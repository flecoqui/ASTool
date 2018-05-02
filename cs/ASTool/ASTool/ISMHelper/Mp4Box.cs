using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4Box
    {
        Int32 Length;
        string Type;
        byte[] Data;

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
                        return box;
                    }
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
        static public string ReadMp4BoxType(byte[] buffer, int offset)
        {
            string s = string.Empty;
            if ((offset + 8) > buffer.Length)
            {
                return s;
            }
            char[] array = new char[5];
            for (int i = 0; i < 4; i++)
                array[0 + i] = (char)buffer[offset + 4 + i];
            array[4] = (char)0;
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
            string result = string.Empty;
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
    }
}
