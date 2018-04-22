using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool
{

    public interface ASInput
    {

        string GetInputPluginName();
        string GetInputPluginDescription();
        Int32 GetInputPluginVersion();

        bool LoadInput(string path);
        string GetInputDescription();
        ASIndexType GetInputIndexType();

        int GetInputStreamCount();
        int GetInputStreamTrackCount(int IndexStream);
        ASTrackType GetInputStreamTrackType(int IndexStream, int IndexTrack);

        Byte[] GetInputDataByTime(int Index, TimeSpan time, TimeSpan duration);
        TimeSpan GetInputDurationByTime(int Index);
        Byte[] GetInputDataByRange(int Index, Int64 index, Int64 length);
        Int64 GetInputLenghtByRange(int Index);

    }
}
