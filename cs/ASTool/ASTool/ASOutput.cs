using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool
{

    public interface ASOutput
    {

        string GetOutputPluginName();
        string GetOutputPluginDescription();
        Int32 GetOutputPluginVersion();

        bool LoadOutput(string path);
        string GetOutputDescription();
        ASIndexType GetOutputIndexType();

        int SetOutputStreamCount();
        int SetOutputStreamTrackCount(int IndexStream);
        ASTrackType SetOutputStreamTrackType(int IndexStream, int IndexTrack);

        bool SetOutputDataByTime(int Index, TimeSpan time, TimeSpan duration, Byte[] data);
        TimeSpan SetOutputDurationByTime(int Index);
        bool SetOutputDataByRange(int Index, Int64 index, Int64 length, Byte[] data);
        TimeSpan SetOutputLenghtByRange(int Index);

    }
}
