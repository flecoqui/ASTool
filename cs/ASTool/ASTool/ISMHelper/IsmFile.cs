using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ASTool.ISMHelper
{
    public enum IsmTrackType
    {
        audio,
        video
    }
    public class QualityLevel
    {
        public QualityLevel(XmlNode n)
        {
            if (n.Attributes["Index"] != null)
                Index = int.Parse(n.Attributes["Index"].Value);
            else
                Index = 0;
            Bitrate = int.Parse(n.Attributes["Bitrate"].Value);
            FourCC = n.Attributes["FourCC"].Value;
            CodecPrivateData = n.Attributes["CodecPrivateData"].Value;

        }
        public int Index;
        public int Bitrate;
        public string FourCC;
        public string CodecPrivateData;
    }
    public class VideoQualityLevel : QualityLevel
    {
        public VideoQualityLevel(XmlNode n): base(n)
        {
            MaxWidth = int.Parse(n.Attributes["MaxWidth"].Value);
            MaxHeight = int.Parse(n.Attributes["MaxHeight"].Value);

        }
        // video
        public int MaxWidth;
        public int MaxHeight;
    }
    public class AudioQualityLevel : QualityLevel
    {

        public AudioQualityLevel(XmlNode n): base (n)
        {
            SamplingRate = int.Parse(n.Attributes["SamplingRate"].Value);
            Channels = int.Parse(n.Attributes["Channels"].Value);
            BitsPerSample = int.Parse(n.Attributes["BitsPerSample"].Value);
            PacketSize = int.Parse(n.Attributes["PacketSize"].Value);
            AudioTag = n.Attributes["AudioTag"].Value;
        }
        //audio
        public int SamplingRate;
        public int Channels;
        public int BitsPerSample;
        public int PacketSize;
        public string AudioTag;
    }
    public class Chunk
    {
        public Chunk(XmlNode n, int Ind = 0)
        {
            if (n.Attributes["n"] != null)
                Index = long.Parse(n.Attributes["n"].Value);
            else
                Index = Ind;
            duration = long.Parse(n.Attributes["d"].Value);
        }
        public long Index;
        public long duration;        
    }
    public class StreamIndex
    {
        internal StreamIndex(XmlNode n)
        {


            MediaType = (IsmTrackType)Enum.Parse(typeof(IsmTrackType), n.Attributes["Type"].Value);

            if(n.Attributes["TimeScale"]!=null)
                TimeScale = long.Parse(n.Attributes["TimeScale"].Value);
            else
                TimeScale = 10000000;
            Chunks = long.Parse(n.Attributes["Chunks"].Value);
            QualityLevelCount = int.Parse(n.Attributes["QualityLevels"].Value);
            Name = n.Attributes["Name"].Value;
            if(n.Attributes["Language"]!=null)
                Lang = n.Attributes["Language"].Value;
            else
                Lang = "eng";
            XmlNodeList QualityLevels = n.SelectNodes("QualityLevel");
            if (MediaType == IsmTrackType.audio)
            {
                AudioLevel = new AudioQualityLevel[QualityLevels.Count];
                for (int i = 0; i < QualityLevels.Count; i++)
                {
                    AudioLevel[i] = new AudioQualityLevel(QualityLevels[i]);
                }
            }
            if (MediaType == IsmTrackType.video)
            {
                VideoLevel = new VideoQualityLevel[QualityLevels.Count];
                for (int i = 0; i < QualityLevels.Count; i++)
                {
                    VideoLevel[i] = new VideoQualityLevel(QualityLevels[i]);
                }
            }
        }

        public IsmTrackType MediaType;
        public long TimeScale;
        public long Chunks;
        public AudioQualityLevel[] AudioLevel;
        public VideoQualityLevel[] VideoLevel;
        public string Name;
        public int QualityLevelCount;
        public string Lang;

        static XmlNode GetParam(XmlNode n, string name)
        {
            for (int i = 0; i < n.ChildNodes.Count; i++)
            {
                if (n.ChildNodes[i].Name == "param" &&
                    n.ChildNodes[i].Attributes["name"].Value == name)
                {
                    return n.ChildNodes[i];
                }
            }
            return null;
        }

    }



    public class IsmcFile 
    {
        private XmlDocument _doc = new XmlDocument();
        public IsmcFile(Stream s)
        {
            _doc.Load(s);
            XmlNodeList SmoothStreamingMediaNodeList = SelectNodes("SmoothStreamingMedia");
            if (SmoothStreamingMediaNodeList != null)
            {
                if(SmoothStreamingMediaNodeList[0].Attributes["TimeScale"]!=null)
                TimeScale = long.Parse(SmoothStreamingMediaNodeList[0].Attributes["TimeScale"].Value);
                else
                TimeScale = 10000000;
                Duration = long.Parse(SmoothStreamingMediaNodeList[0].Attributes["Duration"].Value);
                Version = SmoothStreamingMediaNodeList[0].Attributes["MajorVersion"].Value + "." + SmoothStreamingMediaNodeList[0].Attributes["MinorVersion"].Value;
            }
            XmlNodeList StreamIndexNodeList = SelectNodes("SmoothStreamingMedia/StreamIndex");
            StreamIndexs = new StreamIndex[StreamIndexNodeList.Count];
            ChunkIndexs = new Chunk[StreamIndexNodeList.Count][];
            for (int i = 0; i < StreamIndexNodeList.Count; i++)
            {
                StreamIndexs[i] = new StreamIndex(StreamIndexNodeList[i]);
                XmlNodeList ChunkNodeList = StreamIndexNodeList[i].SelectNodes("c");
                if (ChunkNodeList != null)
                {
                    if ((ChunkNodeList.Count > 0) && (ChunkNodeList.Count ==  StreamIndexs[i].Chunks))
                    {
                        Chunk[] ChunkArray = new Chunk[ChunkNodeList.Count];
                        for (int j = 0; j < ChunkNodeList.Count; j++)
                        {
                            ChunkArray[j] = new Chunk(ChunkNodeList[j], j);
                        }
                        ChunkIndexs[i] = ChunkArray;
                    }
                }
            }
        }
        protected XmlNodeList SelectNodes(string xpath)
        {
            return _doc.SelectNodes(xpath);
        }

        public override string ToString()
        {
            return _doc.OuterXml;
        }

        public StreamIndex[] StreamIndexs;
        public Chunk[][] ChunkIndexs;
        string Version;
        long TimeScale;
        long Duration;
    }
    public class IsmFile : SMIL20Stream
    {
        public IsmFile(Stream s)
            : base(s)
        {
            XmlNodeList videoTracks = SelectNodes("smil:smil/smil:body/smil:switch/smil:video");
            XmlNodeList audioTracks = SelectNodes("smil:smil/smil:body/smil:switch/smil:audio");
            Tracks = new IsmFileTrack[videoTracks.Count + audioTracks.Count];
            for (int i = 0; i < videoTracks.Count; i++)
            {
                Tracks[i] = new IsmFileTrack(videoTracks[i]);
            }
            for (int i = 0; i < audioTracks.Count; i++)
            {
                Tracks[i + videoTracks.Count] = new IsmFileTrack(audioTracks[i]);
            }
            IsmcFilePath = string.Empty; 
            XmlNodeList metas = SelectNodes("smil:smil/smil:head/smil:meta");
            if (metas != null)
            {
                for(int i = 0 ; i < metas.Count; i++)
                {
                    if (metas[i].Attributes["name"].Value == "clientManifestRelativePath")
                    {
                        IsmcFilePath = metas[i].Attributes["content"].Value;
                        break;
                    }
                }
            }

        }

        static string VideoLiveManifestTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">\r\n  <head>\r\n    <meta name=\"creator\" content=\"pushEncoder\" />\r\n  </head>\r\n  <body>\r\n    <switch>\r\n      <video src=\"<ismvfile>\" systemBitrate=\"<bitrate>\">\r\n        <param name=\"trackID\" value=\"<trackid>\" valuetype=\"data\" />\r\n        <param name=\"trackName\" value=\"<trackname>\" valuetype=\"data\" />\r\n        <param name=\"timescale\" value=\"<timescale>\" valuetype=\"data\" />\r\n        <param name=\"FourCC\" value=\"<fourcc>\" valuetype=\"data\" />\r\n        <param name=\"CodecPrivateData\" value=\"<codecprivatedata>\" valuetype=\"data\" />\r\n        <param name=\"MaxWidth\" value=\"<width>\" valuetype=\"data\" />\r\n        <param name=\"MaxHeight\" value=\"<height>\" valuetype=\"data\" />\r\n        <param name=\"DisplayWidth\" value=\"<width>\" valuetype=\"data\" />\r\n        <param name=\"DisplayHeight\" value=\"<height>\" valuetype=\"data\" />\r\n        <param name=\"Subtype\" value=\"\" valuetype=\"data\" />\r\n      </video>\r\n    </switch>\r\n  </body>\r\n</smil>\r\n";
        public string GetVideoManifest(int TrackID, string TrackName, int Bitrate, string source, IsmcFile ismc)
        {
            string manifest = string.Empty;
            if(ismc.StreamIndexs != null)
            {
                for(int i= 0 ; i < ismc.StreamIndexs.Length; i++)
                {
                    if ((ismc.StreamIndexs[i].Name == TrackName) && (ismc.StreamIndexs[i].MediaType == IsmTrackType.video))
                    {
                        if(ismc.StreamIndexs[i].VideoLevel != null)
                        {
                            if(ismc.StreamIndexs[i].VideoLevel.Length>0)
                            {
                                for(int j = 0; j<ismc.StreamIndexs[i].VideoLevel.Length;j++)
                                {
                                    if(ismc.StreamIndexs[i].VideoLevel[j].Bitrate == Bitrate)
                                    {
                                        manifest = VideoLiveManifestTemplate;
                                        manifest = manifest.Replace("<ismvfile>",source);
                                        manifest = manifest.Replace("<bitrate>",Bitrate.ToString());
                                        manifest = manifest.Replace("<trackid>",TrackID.ToString());
                                        manifest = manifest.Replace("<trackname>",TrackName.ToString());
                                        manifest = manifest.Replace("<timescale>",ismc.StreamIndexs[i].TimeScale.ToString());
                                        manifest = manifest.Replace("<fourcc>",ismc.StreamIndexs[i].VideoLevel[j].FourCC.ToString());
                                        manifest = manifest.Replace("<codecprivatedata>",ismc.StreamIndexs[i].VideoLevel[j].CodecPrivateData.ToString());
                                        manifest = manifest.Replace("<width>",ismc.StreamIndexs[i].VideoLevel[j].MaxWidth.ToString());
                                        manifest = manifest.Replace("<height>",ismc.StreamIndexs[i].VideoLevel[j].MaxHeight.ToString());
                                        return manifest;
                                    }
                                }
                            }
                        }
                        
                    }
                }
            }
            return manifest;

        }
        public Chunk[] GetChunkList(string TrackName, IsmcFile ismc)
        {
            string manifest = string.Empty;
            if (ismc.StreamIndexs != null)
            {
                for (int i = 0; i < ismc.StreamIndexs.Length; i++)
                {
                    if (ismc.StreamIndexs[i].Name == TrackName) 
                    {
                        return ismc.ChunkIndexs[i];
                    }
                }
            }
            return null;

        }

/*
<?xml version="1.0" encoding="utf-8"?>
<smil xmlns="http://www.w3.org/2001/SMIL20/Language">
  <head>
    <meta name="creator" content="pushEncoder" />
  </head>
  <body>
    <switch>
      <video src="165258_3500000_1.ismv" systemBitrate="3500000">
        <param name="trackID" value="1" valuetype="data" />
        <param name="trackName" value="video" valuetype="data" />
        <param name="timescale" value="10000000" valuetype="data" />
        <param name="FourCC" value="H264" valuetype="data" />
        <param name="CodecPrivateData" value="000000012764001FAC2CA5014016EC04400000FA40003" valuetype="data" />
        <param name="MaxWidth" value="1280" valuetype="data" />
        <param name="MaxHeight" value="720" valuetype="data" />
        <param name="DisplayWidth" value="1280" valuetype="data" />
        <param name="DisplayHeight" value="720" valuetype="data" />
        <param name="Subtype" value="" valuetype="data" />
      </video>
    </switch>
  </body>
</smil>
 */
        static string AudioLiveManifestTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">\r\n  <head>\r\n    <meta name=\"creator\" content=\"pushEncoder\" />\r\n  </head>\r\n  <body>\r\n    <switch>\r\n      <audio src=\"<ismafile>\" systemBitrate=\"<bitrate>\" systemLanguage=\"<lang>\">\r\n        <param name=\"trackID\" value=\"<trackid>\" valuetype=\"data\" />\r\n        <param name=\"trackName\" value=\"<trackname>\" valuetype=\"data\" />\r\n        <param name=\"timescale\" value=\"<timescale>\" valuetype=\"data\" />\r\n        <param name=\"FourCC\" value=\"<fourcc>\" valuetype=\"data\" />\r\n        <param name=\"CodecPrivateData\" value=\"<codecprivatedata>\" valuetype=\"data\" />\r\n        <param name=\"AudioTag\" value=\"<audiotag>\" valuetype=\"data\" />\r\n        <param name=\"Channels\" value=\"<channels>\" valuetype=\"data\" />\r\n        <param name=\"SamplingRate\" value=\"<samplingrate>\" valuetype=\"data\" />\r\n        <param name=\"BitsPerSample\" value=\"<bitpersample>\" valuetype=\"data\" />\r\n        <param name=\"PacketSize\" value=\"<packetsize>\" valuetype=\"data\" />\r\n        <param name=\"Subtype\" value=\"\" valuetype=\"data\" />\r\n      </audio>\r\n    </switch>\r\n  </body>\r\n</smil>\r\n";
        public string GetAudioManifest(int TrackID, string TrackName, int Bitrate, string source, IsmcFile ismc)
        {
            string manifest = string.Empty;
            if (ismc.StreamIndexs != null)
            {
                for (int i = 0; i < ismc.StreamIndexs.Length; i++)
                {
                    if ((ismc.StreamIndexs[i].Name == TrackName) && (ismc.StreamIndexs[i].MediaType == IsmTrackType.audio))
                    {
                        if (ismc.StreamIndexs[i].AudioLevel != null)
                        {
                            if (ismc.StreamIndexs[i].AudioLevel.Length > 0)
                            {
                                for (int j = 0; j < ismc.StreamIndexs[i].AudioLevel.Length; j++)
                                {
                                    if (ismc.StreamIndexs[i].AudioLevel[j].Bitrate == Bitrate)
                                    {
                                        manifest = AudioLiveManifestTemplate;


                                        manifest = manifest.Replace("<ismafile>",source);
                                        manifest = manifest.Replace("<bitrate>",Bitrate.ToString());
                                        manifest = manifest.Replace("<lang>",ismc.StreamIndexs[i].Lang);
                                        manifest = manifest.Replace("<trackid>",TrackID.ToString());
                                        manifest = manifest.Replace("<trackname>",TrackName.ToString());
                                        manifest = manifest.Replace("<timescale>", ismc.StreamIndexs[i].TimeScale.ToString());
                                        manifest = manifest.Replace("<fourcc>", ismc.StreamIndexs[i].AudioLevel[j].FourCC.ToString());
                                        manifest = manifest.Replace("<codecprivatedata>", ismc.StreamIndexs[i].AudioLevel[j].CodecPrivateData.ToString());
                                        manifest = manifest.Replace("<audiotag>",ismc.StreamIndexs[i].AudioLevel[j].AudioTag.ToString());            
                                        manifest = manifest.Replace("<channels>",ismc.StreamIndexs[i].AudioLevel[j].Channels.ToString());
                                        manifest = manifest.Replace("<samplingrate>",ismc.StreamIndexs[i].AudioLevel[j].SamplingRate.ToString());
                                        manifest = manifest.Replace("<bitpersample>",ismc.StreamIndexs[i].AudioLevel[j].BitsPerSample.ToString());
                                        manifest = manifest.Replace("<packetsize>",ismc.StreamIndexs[i].AudioLevel[j].PacketSize.ToString());

                                        return manifest;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return manifest;
        }
        /*
<smil xmlns="http://www.w3.org/2001/SMIL20/Language">
  <head>
    <meta name="creator" content="pushEncoder" />
  </head>
  <body>
    <switch>
      <audio src="165258_64000_1.isma" systemBitrate="64000" systemLanguage="eng">
        <param name="trackID" value="1" valuetype="data" />
        <param name="trackName" value="audio_eng" valuetype="data" />
        <param name="timescale" value="10000000" valuetype="data" />
        <param name="FourCC" value="AACH" valuetype="data" />
        <param name="CodecPrivateData" value="131056E598" valuetype="data" />
        <param name="AudioTag" value="255" valuetype="data" />
        <param name="Channels" value="2" valuetype="data" />
        <param name="SamplingRate" value="24000" valuetype="data" />
        <param name="BitsPerSample" value="16" valuetype="data" />
        <param name="PacketSize" value="4" valuetype="data" />
        <param name="Subtype" value="" valuetype="data" />
      </audio>
    </switch>
  </body>
</smil>*/

        public IsmFileTrack[] Tracks;
        public string IsmcFilePath;
    }

    public class IsmFileTrack
    {
        internal IsmFileTrack(XmlNode n)
        {
            MediaType = (IsmTrackType)Enum.Parse(typeof(IsmTrackType), n.Name);
            Bitrate = int.Parse(n.Attributes["systemBitrate"].Value);
            Source = n.Attributes["src"].Value;
            TrackName = GetParam(n, "trackName").Attributes["value"].Value;
            TrackId = int.Parse(GetParam(n, "trackID").Attributes["value"].Value);
        }

        public IsmTrackType MediaType;
        public string Source;
        public int Bitrate;
        public int TrackId;
        public string TrackName;

        static XmlNode GetParam(XmlNode n, string name)
        {
            for (int i = 0; i < n.ChildNodes.Count; i++)
            {
                if (n.ChildNodes[i].Name == "param" && 
                    n.ChildNodes[i].Attributes["name"].Value == name)
                {
                    return n.ChildNodes[i];
                }
            }
            return null;
        }
    }
}
