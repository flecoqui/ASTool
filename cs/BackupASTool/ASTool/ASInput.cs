using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool
{

    public interface ASInput
    {

        string GetPluginName();
        string GetPluginDescription();
        Int32 GetPluginVersion();

        bool LoadInput(string path);
        string GetInputDescription();
        ASIndexType GetInputIndexType();

        int GetInputStreamCount();
        int GetInputStreamTrackCount(int IndexStream);
        ASTrackType GetInputStreamTrackType(int IndexStream, int IndexTrack);

        Byte[] GetInputDataByTime(int IndexStream, int IndexTrack, TimeSpan time, TimeSpan duration);
        TimeSpan GetInputDurationByTime(int IndexStream, int IndexTrack);
        Byte[] GetInputDataByByte(int IndexStream, int IndexTrack, Int64 index, Int64 length);
        Int64 GetInputLenghtByByte(int IndexStream, int IndexTrack);

    }
}
