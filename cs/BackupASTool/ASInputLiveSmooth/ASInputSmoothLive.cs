using System;

namespace ASTool
{
    public class ASInputSmoothLive : ASInput
    {
        const string Name = "InputSmoothLive";
        const string Description = "InputSmoothLive";
        Int32 version = ASVersion.SetVersion(1, 0, 0, 0);
        string Path;
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
            Path = path;
            return true;
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
            return ASTrackType.AudioVideo;
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
            return null;
        }
        public Int64 GetInputLenghtByByte(int IndexStream, int IndexTrack)
        {
            return 0;
        }
    }
}