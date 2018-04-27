using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool
{

    public interface ASOutput
    {

        string GetPluginName();
        string GetPluginDescription();
        Int32 GetPluginVersion();

        bool LoadOutput(string path);
        string GetOutputDescription();
        ASIndexType GetOutputIndexType();

        bool SetOutputStreamCount(int Count);
        bool SetOutputStreamTrackCount(int IndexStream, int TrackCount);
        ASTrackType SetOutputStreamTrackType(int IndexStream, int IndexTrack);

        bool SetOutputDataByTime(int IndexStream, int IndexTrack, TimeSpan time, TimeSpan duration, Byte[] data);
        bool SetOutputDurationByTime(int IndexStream, int IndexTrack, TimeSpan duration);
        bool SetOutputDataByByte(int IndexStream, int IndexTrack, Int64 index, Int64 length, Byte[] data);
        bool SetOutputLenghtByByte(int IndexStream, int IndexTrack, Int64 Length);

    }
}
