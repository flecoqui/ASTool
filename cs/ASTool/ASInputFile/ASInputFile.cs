using System;

namespace ASTool
{
    public class ASInputFile : ASInput
    {
        const string Name = "InputFile";
        const string Description = "InputFile used to read file on disk";
        Int32 version = ASVersion.SetVersion(1, 0, 0, 0);
        string Path;
        long Length;
        public string GetPluginName()
        {
            return Name;
        }
        public string GetPluginDescription()
        {
            return Description;
        }
        public Int32 GetPluginVersion()
        {
            return version;
        }

        public bool LoadInput(string path)
        {
            try
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(path);
                if (fs != null)
                {
                    Length = fs.Length;
                    if (Length > 0)
                    {
                        Path = path;
                        fs.Close();
                        return true;
                    }
                }
            }

            catch(Exception)
            {

            }
            return false;
        }
        public string GetInputDescription()
        {
            return string.Empty;
        }
        public ASIndexType GetInputIndexType()
        {
            return ASIndexType.Byte;
        }

        public int GetInputStreamCount()
        {
            return 1;
        }
        public int GetInputStreamTrackCount(int IndexStream)
        {
            return 1;
        }
        public ASTrackType GetInputStreamTrackType(int IndexStream, int IndexTrack)
        {
            return ASTrackType.Unknown;
        }

        public Byte[] GetInputDataByTime(int IndexStream, int IndexTrack, TimeSpan time, TimeSpan duration)
        {
            return null;
        }
        public TimeSpan GetInputDurationByTime(int IndexStream, int IndexTrack)
        {
            return new TimeSpan(0);
        }
        public Byte[] GetInputDataByByte(int IndexStream, int IndexTrack, Int64 index, Int64 length)
        {
            if ((IndexStream == 0) && (IndexTrack == 0) && (Length > 0))
            {
                try
                {
                    System.IO.FileStream fs = System.IO.File.OpenRead(Path);
                    if (fs != null)
                    {
                        fs.Position = index;
                        long buffersize = 0;
                        if (index + length > Length)
                            buffersize = Length - index;
                        else
                            buffersize = length;
                        byte[] buffer = new byte[buffersize];
                        if (buffer != null)
                        {
                            int count = fs.Read(buffer, (int) index, (int)buffersize);                       
                            Length = fs.Length;
                            fs.Close();
                            if (count == buffersize)
                            {
                                return buffer;
                            }
                        }
                        else
                            fs.Close();
                    }
                }

                catch (Exception)
                {

                }
            }
            return null;
        }
        public Int64 GetInputLenghtByByte(int IndexStream, int IndexTrack)
        {
            if ((IndexStream == 0) && (IndexTrack == 0) && (Length > 0))
            {
                return Length;
            }
            return 0;
        }
    }
}
