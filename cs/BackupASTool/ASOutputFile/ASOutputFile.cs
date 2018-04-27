using System;



namespace ASTool
{
    public class ASOutputFile : ASOutput
    {
        const string Name = "OutputFile";
        const string Description = "OutputFile used to write file on disk";
        Int32 version = ASVersion.SetVersion(1,0,0,0);
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

        public bool LoadOutput(string path)
        {
            try
            {
                System.IO.FileStream fs = System.IO.File.OpenWrite(path);
                if (fs != null)
                {
                    Length = fs.Length;
                    Path = path;
                    fs.Close();
                    return true;                   
                }
            }

            catch (Exception)
            {

            }
            return false;
        }
        public string GetOutputDescription()
        {
            return string.Empty;
        }
        public ASIndexType GetOutputIndexType()
        {
            return ASIndexType.Byte;
        }

        public bool SetOutputStreamCount(int Count)
        {
            if(Count == 1)
                return true;
            return false;
        }
        public bool SetOutputStreamTrackCount(int IndexStream, int TrackCount)
        {
            if ((IndexStream == 0) && (TrackCount == 1))
                return true;
            return false;
        }
        public ASTrackType SetOutputStreamTrackType(int IndexStream, int IndexTrack)
        {
            return ASTrackType.Unknown;
        }

        public bool SetOutputDataByTime(int IndexStream, int IndexTrack, TimeSpan time, TimeSpan duration, Byte[] data)
        {
            return false;
        }
        public bool SetOutputDurationByTime(int IndexStream, int IndexTrack, TimeSpan duration)
        {
            return false;
        }
        public  bool SetOutputDataByByte(int IndexStream, int IndexTrack, Int64 index, Int64 length, Byte[] data)
        {
            if ((IndexStream == 0) && (IndexTrack == 0) && (length > 0) && (data!=null))
            {
                try
                {
                    System.IO.FileStream fs = System.IO.File.OpenWrite(Path);
                    if (fs != null)
                    {
                        fs.Position = index;

                        
                        long buffersize = 0;
                        if (length > data.Length)
                            buffersize = data.Length;
                        else
                            buffersize = length;
                        fs.Write(data, (int) index, (int)buffersize);
                        fs.Close();
                        return true;
                    }
                }

                catch (Exception)
                {

                }
            }
            return false;
        }
        public bool SetOutputLenghtByByte(int IndexStream, int IndexTrack, Int64 length)
        {
            return true;
        }
    }
}

