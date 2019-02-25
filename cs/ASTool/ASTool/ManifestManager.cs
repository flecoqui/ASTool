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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTool.SmoothHelper;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;


namespace ASTool
{
    public enum AssetStatus
    {
        Initialized = 0,
        DownloadingManifest,
        ManifestDownloaded,
        DownloadingChunks,
        AssetPlayable,
        ChunksDownloaded,
        ErrorManifestAlreadyInCache,
        ErrorManifestCreationError,
        ErrorDownloadAssetLimit,
        ErrorManifestStorage,
        ErrorManifestNotInCache,
        ErrorManifestDownload,
        ErrorChunksDownload,
        ErrorDownloadSessionLimit,
        ErrorStorageLimit,
        ErrorParameters,
    }

    public  class Track
    {
        public int Index { get; set; }
        public int Bitrate { get; set; }
        public string FourCC { get; set; }
        public string Language { get; set; }

        public string CustomAttributes { get; set; }
    }

    public sealed class TextTrack : Track
    {

    }

    public sealed class AudioTrack : Track
    {

        public int BitsPerSample { get; set; }
        public int Channels { get; set; }
        public int SamplingRate { get; set; }
        public string CodecPrivateData { get; set; }
    }
    public sealed class VideoTrack :Track
    {
        public int MaxHeight { get; set; }
        public int MaxWidth { get; set; }

        public string CodecPrivateData { get; set; }
    }
    [DataContract(Name ="ManifestManager")]
    public  class ManifestManager : IDisposable
    {

        public const ulong TimeUnit = 10000000;


        /// <summary>
        /// ManifestUri 
        /// Uri of the manifest associated with this asset.
        /// </summary>
        [DataMember]
        public Uri ManifestUri { get; private set; }
        /// <summary>
        /// RedirectUri 
        /// Redirect Uri if an http redirection is used on the server side.
        /// </summary>
        [DataMember]
        public Uri RedirectUri { get; private set; }
        /// <summary>
        /// DownloadToGo 
        /// if the value is true, the user could play this asset once the whole asset will be downloaded.When downloading the chunks, if the number of http error reach this value, the download thread is terminated.
        /// if the value is false, the user could play this asset once a percentage of the asset is downloaded.
        /// </summary>
        [DataMember]
        public bool DownloadToGo { get;  set; }

        /// <summary>
        /// MaxError 
        /// When downloading the chunks, if the number of http error reach this value, the download thread is terminated.
        /// </summary>
        [DataMember]
        public uint MaxError { get; set; }
        /// <summary>
        /// MaxMemoryBufferSize 
        /// When the amount of audio and video chunks in memory is over this value, they are stored on disk and removed from memory 
        /// </summary>
        [DataMember]
        public ulong MaxMemoryBufferSize { get; set; }
        /// <summary>
        /// IsLive 
        /// True if Live manifest (Download To go is not supported for Live streams) 
        /// </summary>
        [DataMember]
        public bool IsLive { get; private set; }
        /// <summary>
        /// BaseUrl 
        /// Base Url of the asset 
        /// </summary>
        [DataMember]
        public string BaseUrl { get; set; }
        /// <summary>
        /// RedirectBaseUrl 
        /// Redirect Base Url if a redirection is used on the server side 
        /// </summary>
        [DataMember]
        public string RedirectBaseUrl { get; set; }
        /// <summary>
        /// StoragePath 
        /// Folder name where the asset will be stored on disk  
        /// </summary>
        [DataMember]
        public string StoragePath { get; private set; }
        /// <summary>
        /// Duration 
        /// Duration of the asset (unit: 100 ns)  
        /// </summary>
        [DataMember]
        public ulong Duration { get; set; }
        /// <summary>
        /// TimeScale 
        /// TimeScale defined in the manifest  
        /// </summary>
        [DataMember]
        public ulong TimeScale { get; set; }
        /// <summary>
        /// AudioBitrate 
        /// Audio bitrate of the asset  
        /// </summary>
        [DataMember]
        public ulong AudioBitrate { get; set; }
        /// <summary>
        /// VideoBitrate 
        /// Video bitrate of the asset  
        /// </summary>
        [DataMember]
        public ulong VideoBitrate { get; set; }

        /// <summary>
        /// ExpectedMediaSize 
        /// The estimated asset size in bytes based on the duration, the audio bitrate and video bitrate  
        /// </summary>
        public ulong ExpectedMediaSize { get { return GetExpectedSize();  } }
        /// <summary>
        /// CurrentMediaSize 
        /// The number of audio and video bytes stored on disk 
        /// </summary>
        public ulong CurrentMediaSize { get { return GetMediaSize(); } }



        /// <summary>
        /// AudioTemplateUrl 
        /// The Url template to download  the audio chunks
        /// </summary>
        /// <summary>
        /// AudioChunkList 
        /// List of the audio chunk to download  
        /// </summary>
        [DataMember]
        public List<ChunkList> AudioChunkListList { get; set; }
        /// <summary>
        /// VideoChunkList 
        /// List of the video chunk to download  
        /// </summary>
        [DataMember]
        public List<ChunkList> VideoChunkListList { get; set; }
        /// <summary>
        /// TextChunkList 
        /// List of the text chunk to download  
        /// </summary>
        [DataMember]
        public List<ChunkList> TextChunkListList { get; set; }

        /// <summary>
        /// DownloadThreadStartTime 
        /// Download thread start time  
        /// </summary>
        [DataMember]
        public DateTime DownloadThreadStartTime { get; set; }
        /// <summary>
        /// DownloadThreadAudioCount 
        /// Number of audio chunks downloaded since the download thread is running 
        /// </summary>
        [DataMember]
        public ulong DownloadThreadAudioCount { get; set; }
        /// <summary>
        /// DownloadThreadVideoCount 
        /// Number of video chunks downloaded since the download thread is running 
        /// </summary>
        [DataMember]
        public ulong DownloadThreadVideoCount { get; set; }
        /// <summary>
        /// DownloadThreadTextCount 
        /// Number of text chunks downloaded since the download thread is running 
        /// </summary>
        [DataMember]
        public ulong DownloadThreadTextCount { get; set; }
        /// <summary>
        /// manifestBuffer 
        /// Buffer where the manifest is stored 
        /// </summary>
        [DataMember]
        public byte[] manifestBuffer{ get; set; }
        /// <summary>
        /// Get Protection Guid.
        /// </summary>
        [DataMember]
        public Guid ProtectionGuid { get; protected set; }
        /// <summary>
        /// Get Protection Data.
        /// </summary>
        [DataMember]
        public string ProtectionData { get; protected set; }
        /// <summary>
        /// IsPlayReadyLicenseAcquired 
        /// true if PlayReady License has been acquired
        /// </summary>
        [DataMember]
        public bool IsPlayReadyLicenseAcquired { get; set; }
        /// <summary>
        /// MinBitrate 
        /// Minimum video bitrate of the video track to select 
        /// </summary>
        [DataMember]
        public ulong MinBitrate { get; set; }
        /// <summary>
        /// MaxBitrate 
        /// Maximum video bitrate of the video track to select 
        /// </summary>
        [DataMember]
        public ulong MaxBitrate { get; set; }

        /// <summary>
        /// AudioTrackName
        /// Name of the Audio track to select
        /// </summary>
        [DataMember]
        public string AudioTrackName  { get; set; }

        /// <summary>
        /// TextTrackName
        /// Name of the Text track to select
        /// </summary>
        [DataMember]
        public string TextTrackName { get; set; }

        /// <summary>
        /// MaxDuration 
        /// Max duration of the capture in milliseconds.
        /// </summary>
        [DataMember]
        public ulong MaxDuration { get; set; }

        /// <summary>
        /// LiveOffset 
        /// LiveOffset used when capturing Live services in seconds.
        /// </summary>
        [DataMember]
        public int LiveOffset { get; set; }

        /// <summary>
        /// DownloadMethod 
        /// Downlaod Method for audio and video chunks 
        ///     0 Auto: The cache will create if necessary several threads to download audio and video chunks
        ///     1 Default: The cache will download the audio and video chunks step by step in one single thread
        ///     N The cache will create N parallel threads to download the audio chunks and N parallel threads to downlaod video chunks
        /// </summary>
        [DataMember]
        public int DownloadMethod { get; set; }



        private const string audioString = "audio";
        private const string videoString = "video";
        private List<AudioTrack> ListAudioTracks;
        private List<VideoTrack> ListVideoTracks;
        private List<TextTrack> ListTextTracks;
        private AssetStatus mStatus;
        private ManifestOutput OutputManager = null;
        private System.Threading.Tasks.Task downloadTask;
        private System.Threading.CancellationTokenSource downloadTaskCancellationtoken;
        private bool downloadTaskRunning = false;
        private System.Threading.Tasks.Task downloadManifestTask;
        private System.Threading.CancellationTokenSource downloadManifestTaskCancellationtoken;
        private bool downloadManifestTaskRunning = false;
        private Options options = null;
        /// <summary>
        /// Initialize 
        /// Initialize the Manifest Cache parameters 
        /// </summary>
        private void Initialize()
        {
            DownloadToGo = false;
            ManifestUri = null;
            RedirectUri = null;
            StoragePath = string.Empty;
            MinBitrate = 0;
            MaxBitrate = 0;
            AudioTrackName = string.Empty;
            TextTrackName = string.Empty;
            MaxError = 20;
            DownloadMethod = 1;
            MaxMemoryBufferSize = 256000;
            VideoChunkListList = new List<ChunkList>();
            AudioChunkListList = new List<ChunkList>();
            TextChunkListList = new List<ChunkList>();

            BaseUrl = string.Empty;
            RedirectBaseUrl = string.Empty;

            ListAudioTracks = new List<AudioTrack>();
            ListVideoTracks = new List<VideoTrack>();
            ListTextTracks = new List<TextTrack>();

            DownloadedPercentage = 0;
            IsPlayReadyLicenseAcquired = false;
            mStatus = AssetStatus.Initialized;
            options = null;

        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestManager() {
            Initialize();
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestManager(Uri manifestUri, ulong minBitrate, ulong maxBitrate, string audioTrackName, string textTrackName, ulong maxDuration, ulong maxMemoryBufferSize, int liveOffset, Options opt = null)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeManifest(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = true;
            MinBitrate = minBitrate;
            MaxBitrate = maxBitrate;
            AudioTrackName = audioTrackName;
            TextTrackName = textTrackName;
            MaxDuration = maxDuration;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = 20;
            DownloadMethod = 1;
            LiveOffset = liveOffset;
            options = opt;
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestManager(Uri manifestUri,  ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1, int liveOffset = 0)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeManifest(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = true;
            MinBitrate = 0;
            MaxBitrate = 0;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = maxError;
            DownloadMethod = downloadMethod;
            LiveOffset = liveOffset;
        }
        /// <summary>
        /// Convert manifest timescale times to HNS for reporting
        /// </summary>
        /// <param name="tsTime">time in timescale units</param>
        /// <returns>time in HNS units</returns>
        public ulong TimescaleToHNS(ulong tsTime)
        {
            ulong hnsTime = tsTime;
            if (TimeScale != TimeUnit)
            {
                double scale = TimeUnit / TimeScale;
                hnsTime = (ulong)(tsTime * scale);
            }
            return hnsTime;
        }

        /// <summary>
        /// IsAssetProtected
        /// Return true if the asset is protected with PlayReady
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if protected</returns>
        public bool IsAssetProtected()
        {
            if (!string.IsNullOrEmpty(this.ProtectionData) &&
                (this.ProtectionGuid != Guid.Empty))
                return true;
            return false;
        }

        public AssetStatus GetAssetStatus()
        {
            return mStatus;
        }
        void LogMessage(Options.LogLevel level, string Log)
        {
            if (options != null)
            {
                switch (level)
                {
                    case Options.LogLevel.Error:
                        options.LogError(Log);
                        break;
                    case Options.LogLevel.Information:
                        options.LogInformation(Log);
                        break;
                    case Options.LogLevel.Verbose:
                        options.LogVerbose(Log);
                        break;
                    case Options.LogLevel.Warning:
                        options.LogWarning(Log);
                        break;
                    default:
                        break;
                }
            }
            System.Diagnostics.Debug.WriteLine(Log);
        }
        /// <summary>
        /// DownloadManifestAsync
        /// Downloads a manifest asynchronously.
        /// </summary>
        /// <param name="forceNewDownload">Specifies whether to force a new download and avoid cached results.</param>
        /// <returns>A byte array</returns>
        public async Task<byte[]> DownloadManifestAsync(bool forceNewDownload)
        {
            Uri manifestUri = this.ManifestUri;
            LogMessage(Options.LogLevel.Verbose,"Download Manifest: " + manifestUri.ToString() + " start at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));

            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    if (forceNewDownload)
                    {
                        string modifier = manifestUri.AbsoluteUri.Contains("?") ? "&" : "?";
                        string newUriString = string.Concat(manifestUri.AbsoluteUri, modifier, "ignore=", Guid.NewGuid());
                        manifestUri = new Uri(newUriString);
                    }

                    System.Net.Http.HttpResponseMessage response = await client.GetAsync(manifestUri, System.Net.Http.HttpCompletionOption.ResponseContentRead);

                    response.EnsureSuccessStatusCode();
                    /*
                    foreach ( var v in response.Content.Headers)
                    {
                        System.Diagnostics.Debug.WriteLine("Content Header key: " + v.Key + " value: " + v.Value.ToString());
                    }
                    foreach (var v in response.Headers)
                    {
                        System.Diagnostics.Debug.WriteLine("Header key: " + v.Key + " value: " + v.Value.ToString());
                    }
                    */
                    var buffer = await response.Content.ReadAsByteArrayAsync();
                    if (buffer != null)
                    {

                        if ((response.Headers.Location != null) && (response.Headers.Location != manifestUri))
                        {
                            this.RedirectUri = response.Headers.Location;
                            this.RedirectBaseUrl = GetBaseUri(RedirectUri.AbsoluteUri);
                        }
                        else
                        {
                            this.RedirectBaseUrl = string.Empty;
                            this.RedirectUri = null;
                        }
                        this.BaseUrl = GetBaseUri(manifestUri.AbsoluteUri);

                        int val = buffer.Length;
                        LogMessage(Options.LogLevel.Verbose, "Download " + val.ToString() + " Bytes Manifest: " + manifestUri.ToString() + " done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));
                        return buffer;
                    }
                }
                catch (Exception e)
                {
                    LogMessage(Options.LogLevel.Error, "Exception: " + e.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// ParseDashManifest
        /// Parse DASH manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseDashManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// ParseHLSManifest
        /// Parse HLS manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseHLSManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// ParseAndUpdateDashManifest
        /// Parse DASH manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseAndUpdateDashManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// ParseAndUpdateHLSManifest
        /// Parse HLS manifest (not implemented).
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ParseAndUpdateHLSManifest()
        {
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
            }
            return false;
        }
        /// <summary>
        /// GetBaseUri
        /// Get Base Uri of the input source url.
        /// </summary>
        /// <param name="source">Source url</param>
        /// <returns>string base Uri</returns>
        public static string GetBaseUri(string source)
        {
            int manitestPosition = source.LastIndexOf(@"/manifest",StringComparison.OrdinalIgnoreCase);
            if (manitestPosition < 0)
                manitestPosition = source.LastIndexOf(@"/qualitylevels",StringComparison.OrdinalIgnoreCase);
            return manitestPosition < 0 ?
                                source :
                                source.Substring(0, manitestPosition);
        }
        /// <summary>
        /// GetType
        /// Get Type from the url template.
        /// </summary>
        /// <param name="source">Source url</param>
        /// <returns>string Type included in the source url</returns>
        private string GetType(string Template)
        {
            string[] url = Template.ToLower().Split('/');

            string type = string.Empty;
            try
            {
                if (Template.ToLower().Contains("/fragments("))
                {
                    //url = "fragments(audio=0)"
                    string[] keys = { "(", "=", ")" };
                    url = url[url.Length - 1].Split(keys, StringSplitOptions.RemoveEmptyEntries);

                    type = url[url.Length - 2];
                }
            }
            catch (Exception)
            {
            }

            return type;
        }
        /// <summary>
        /// GetAudioTracks
        /// Get the audio tracks .
        /// </summary>
        /// <param name=""></param>
        /// <returns>List of audio tracks</returns>
        public IReadOnlyList<AudioTrack> GetAudioTracks()
        {
            return ListAudioTracks;
        }
        /// <summary>
        /// GetVideoTracks
        /// Get the video tracks .
        /// </summary>
        /// <param name=""></param>
        /// <returns>List of video tracks</returns>
        public IReadOnlyList<VideoTrack> GetVideoTracks()
        {
            return ListVideoTracks ;
        }
        /// <summary>
        /// GetTextTracks
        /// Get the text tracks .
        /// </summary>
        /// <param name=""></param>
        /// <returns>List of text tracks</returns>
        public IReadOnlyList<TextTrack> GetTextTracks()
        {
            return ListTextTracks;
        }
        /// <summary>
        /// ClearChunkLists
        /// Clear chunks lists.
        /// </summary>
        /// <returns>true if success</returns>
        bool ClearChunkLists()
        {
            try
            {
                for (int i = 0; i < VideoChunkListList.Count; i++)
                {
                    if (VideoChunkListList[i] != null)
                        VideoChunkListList[i].Dispose();
                }
                VideoChunkListList.Clear();
                for (int i = 0; i < AudioChunkListList.Count; i++)
                {
                    if (AudioChunkListList[i] != null)
                        AudioChunkListList[i].Dispose();
                }
                AudioChunkListList.Clear();
                for (int i = 0; i < TextChunkListList.Count; i++)
                {
                    if (TextChunkListList[i] != null)
                        TextChunkListList[i].Dispose();
                }
                TextChunkListList.Clear();
            }
            catch(Exception)
            {

            }
            return true;
        }
        /// <summary>
        /// UpdateChunkList
        /// Add the chunks list in the list of chunks to download.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="configuration"></param>
        /// <returns>true if success</returns>
        bool UpdateChunkList(StreamInfo stream, ChunkListConfiguration Configuration)
        {
            int Bitrate = 0;
            string UrlTemplate = string.Empty;
            string OriginalUrlTemplate = string.Empty;
            if ((Configuration != null) && (stream != null))
            {
                Bitrate = Configuration.Bitrate;
                if (Bitrate > 0)
                {
                    if (stream.TryGetAttributeValueAsString("Url", out OriginalUrlTemplate))
                    {
                        UrlTemplate = OriginalUrlTemplate.Replace("{bitrate}", Bitrate.ToString());
                        UrlTemplate = UrlTemplate.Replace("{start time}", "{start_time}");
                        
                        UrlTemplate = UrlTemplate.Replace("{CustomAttributes}", Configuration.CustomAttributes);
                        if ((stream.StreamType.ToLower() == "audio") ||
                            (stream.StreamType.ToLower() == "video") ||
                            (stream.StreamType.ToLower() == "text"))
                        {
                            //UInt64 time = 0;
                            ulong duration = 0;
                            ChunkList l = new ChunkList();
                            if (l != null)
                            {
                                l.Configuration = Configuration;
                                foreach (var chunk in stream.Chunks)
                                {
                                    if (chunk.Duration != null)
                                        duration = (ulong)chunk.Duration;
                                    else
                                        duration = 0;
                                    if (chunk.Time != null)
                                        l.LastTimeChunksToRead = (UInt64)chunk.Time;


                                    ChunkBuffer cc = new ChunkBuffer(l.LastTimeChunksToRead, duration);

                                    if (cc != null)
                                    {
                                        l.ChunksToReadQueue.Enqueue(cc);
                                    }
                                    l.LastTimeChunksToRead += (ulong)duration;
                                }
                                l.Bitrate = Bitrate;
                                l.TotalChunks = (ulong)l.ChunksToReadQueue.Count;
                                l.OriginalTemplateUrl = OriginalUrlTemplate;
                                l.TemplateUrl = UrlTemplate;
                                l.TemplateUrlType = ChunkList.GetType(UrlTemplate);
                                if (stream.StreamType.ToLower() == "video")
                                {
                                    foreach(var vl in this.VideoChunkListList)
                                    {
                                        if(vl.Configuration.GetSourceName()==
                                            l.Configuration.GetSourceName())
                                        {
                                            UpdateChunkList(vl, l);
                                            break;
                                        }

                                    }

                                }
                                else if (stream.StreamType.ToLower() == "audio")
                                {
                                    foreach (var vl in this.AudioChunkListList)
                                    {
                                        if (vl.Configuration.GetSourceName() ==
                                            l.Configuration.GetSourceName())
                                        {
                                            UpdateChunkList(vl, l);
                                            break;
                                        }

                                    }
                                }
                                if (stream.StreamType.ToLower() == "text")
                                {
                                    foreach (var vl in this.TextChunkListList)
                                    {
                                        if (vl.Configuration.GetSourceName() ==
                                            l.Configuration.GetSourceName())
                                        {
                                            UpdateChunkList(vl, l);
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                        return true;
                    };
                }

            }
            return false;
        }
        bool UpdateChunkList(ChunkList org, ChunkList upd)
        {
            bool bResult = false;

            int NewChunk = 0;

            try
            {
                foreach (var cl in upd.ChunksToReadQueue)
                {
                    if (cl.Time >= org.LastTimeChunksToRead)
                    {
                        lock (org.ChunksToReadQueue)
                        {
                            org.ChunksToReadQueue.Enqueue(cl);
//                            org.LastTimeChunksToRead = cl.Time;
                            org.LastTimeChunksToRead += cl.Duration;
                            NewChunk++;
                            org.TotalChunks++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LogMessage(Options.LogLevel.Error, "Exception while Updating ChunkList " + org.Configuration.GetSourceName() + ": " + ex.Message);
            }
            if (NewChunk>0)
                LogMessage(Options.LogLevel.Verbose, "Updating ChunkList " + org.Configuration.GetSourceName() + " Add " + NewChunk.ToString() + " from " + org.TotalChunks.ToString() + "  chunks in the chunklist " +  org.Configuration.GetSourceName() );
            return bResult;
        }
        int GetNumberOfChunks(IList<Chunk> list, int TimeScale, int LiveOffset)
        {
            if((list!=null)&& (LiveOffset > 0))
            {
                int i = 0;
                float d = 0;
                while ((list.Count - 1 - i)>=0)
                {
                    if(i!=0)
                        d += (float) list[list.Count - 1 - i].Duration / (float) TimeScale;
                    if(d>LiveOffset)
                    {
                        return i;
                    }
                    i++;
                }

            }
            return 0;
        }
        /// <summary>
        /// AddChunkList
        /// Add the chunks list in the list of chunks to download.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="configuration"></param>
        /// <returns>true if success</returns>
        bool AddChunkList(StreamInfo stream, ChunkListConfiguration Configuration, int LiveOffset = 0)
        {
            int Bitrate = 0;
            int NumberOfLiveChunks = 0;
            string OriginalUrlTemplate = string.Empty;
            string UrlTemplate = string.Empty;
            if ((Configuration != null) && (stream != null))
            {
                Bitrate = Configuration.Bitrate;
                if (Bitrate > 0)
                {
                    if (stream.TryGetAttributeValueAsString("Url", out OriginalUrlTemplate))
                    {
                        UrlTemplate = OriginalUrlTemplate.Replace("{bitrate}", Bitrate.ToString());
                        UrlTemplate = UrlTemplate.Replace("{start time}", "{start_time}");
                        UrlTemplate = UrlTemplate.Replace("{CustomAttributes}", Configuration.CustomAttributes);
                        if ((stream.StreamType.ToLower() == "audio") ||
                            (stream.StreamType.ToLower() == "video") ||
                            (stream.StreamType.ToLower() == "text"))
                        {
                            ulong duration = 0;
                            NumberOfLiveChunks = GetNumberOfChunks(stream.Chunks, Configuration.TimeScale, LiveOffset);
                            ChunkList l = new ChunkList();
                            if (l != null)
                            {
                                l.Configuration = Configuration;
                                int Count = stream.Chunks.Count;
                                int Threshold = 0;
                                if (NumberOfLiveChunks == 0)
                                    Threshold = 0;
                                else
                                {
                                    if ((Count - NumberOfLiveChunks) > 0)
                                        Threshold = Count - NumberOfLiveChunks ;
                                    else
                                        Threshold = 0;
                                }
                                int index = 0;
                                foreach (var chunk in stream.Chunks)
                                {
                                    if (chunk.Duration != null)
                                        duration = (ulong)chunk.Duration;
                                    else
                                        duration = 0;
                                    if (chunk.Time != null)
                                        l.LastTimeChunksToRead  = (UInt64)chunk.Time;

                                    if (index++ >= Threshold)
                                    {

                                        ChunkBuffer cc = new ChunkBuffer(l.LastTimeChunksToRead, duration);

                                        if (cc != null)
                                        {
                                            l.ChunksToReadQueue.Enqueue(cc);
                                        }
                                    }
                                    l.LastTimeChunksToRead += (ulong)duration;
                                }
                                // Set Duration using the duration information in the chuncks
                                Configuration.Duration = (long) l.LastTimeChunksToRead;
                                l.Bitrate = Bitrate;
                                l.TotalChunks = (ulong)l.ChunksToReadQueue.Count;
                                l.OriginalTemplateUrl = OriginalUrlTemplate;
                                l.TemplateUrl = UrlTemplate;
                                l.TemplateUrlType = ChunkList.GetType(UrlTemplate);
                                if (stream.StreamType.ToLower() == "video")
                                {
                                    this.VideoChunkListList.Add(l);
                                }
                                else if (stream.StreamType.ToLower() == "audio")
                                {
                                    this.AudioChunkListList.Add(l);
                                }
                                if (stream.StreamType.ToLower() == "text")
                                {
                                    this.TextChunkListList.Add(l);
                                }
                            }
                        }
                        return true;
                    };
                }

            }
            return false;
        }
        /// <summary>
        /// ParseAndUpdateSmoothManifest
        /// Parse Smooth Streaming manifest 
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        private async Task<bool> ParseAndUpdateSmoothManifest()
        {
            bool bResult = false;
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
                try
                {
                    SmoothStreamingManifest parser = new SmoothStreamingManifest(manifestBuffer);
                    if ((parser != null) && (parser.ManifestInfo != null))
                    {
                        ManifestInfo mi = parser.ManifestInfo;

                        ulong Duration = mi.ManifestDuration;
                        string UrlTemplate = string.Empty;

                        if(this.Duration != Duration)
                            this.Duration = Duration;
                        if(this.TimeScale != mi.Timescale)
                            this.TimeScale = mi.Timescale;
                        this.IsLive = mi.IsLive;



                        this.ProtectionGuid = mi.ProtectionGuid;
                        this.ProtectionData = mi.ProtectionData;

                        // We don't support multiple audio. Therefore, we need to download the first audio track. 


                        QualityLevel textselected = null;
                        QualityLevel audioselected = null;
                        QualityLevel videoselected = null;
                        StreamInfo audiostream = null;
                        StreamInfo videostream = null;
                        StreamInfo textstream = null;

                        foreach (StreamInfo stream in mi.Streams)
                        {
                            if (stream.StreamType.ToUpper() == "VIDEO")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);

                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    ulong currentIndex = 0;
                                    string currentFourCC = string.Empty;
                                    string currentCodecPrivateData = string.Empty;
                                    ulong currentWidth = 0;
                                    ulong currentHeight = 0;
                                    track.TryGetAttributeValueAsUlong("Index", out currentIndex);
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    if (!track.TryGetAttributeValueAsString("CodecPrivateData", out currentCodecPrivateData))
                                        currentCodecPrivateData = string.Empty;
                                    track.TryGetAttributeValueAsUlong("MaxWidth", out currentWidth);
                                    track.TryGetAttributeValueAsUlong("MaxHeight", out currentHeight);

                                    if (((this.MinBitrate == 0) || (currentBitrate >= (ulong)this.MinBitrate)) &&
                                        ((this.MaxBitrate == 0) || (currentBitrate <= (ulong)this.MaxBitrate)))
                                    {
                                        videoselected = track;
                                        videostream = stream;
                                        VideoChunkListConfiguration configuration = new VideoChunkListConfiguration();
                                        configuration.Bitrate = (int)currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString( track.CustomAttributes);
                                        configuration.Duration = (long)Duration;
                                        configuration.TimeScale = (int)TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = currentFourCC;
                                        configuration.Width = (int)currentWidth;
                                        configuration.Height = (int)currentHeight;
                                        configuration.CodecPrivateData = currentCodecPrivateData;
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        UpdateChunkList(videostream, configuration);
                                    }
                                }
                            }
                            if (stream.StreamType.ToUpper() == "AUDIO")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);
                                string AudioTag = string.Empty;
                                stream.TryGetAttributeValueAsString("AudioTag", out AudioTag);
                                ulong PacketSize = 0;
                                stream.TryGetAttributeValueAsUlong("PacketSize", out PacketSize);



                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    ulong currentBitsPerSample = 16;
                                    ulong currentChannels = 2;
                                    ulong currentSamplingRate = 0;
                                    string currentFourCC = string.Empty;
                                    string currentCodecPrivateData = string.Empty;
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    track.TryGetAttributeValueAsUlong("BitsPerSample", out currentBitsPerSample);
                                    track.TryGetAttributeValueAsUlong("Channels", out currentChannels);
                                    track.TryGetAttributeValueAsUlong("SamplingRate", out currentSamplingRate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    if (!track.TryGetAttributeValueAsString("CodecPrivateData", out currentCodecPrivateData))
                                        currentCodecPrivateData = string.Empty;
                                    string currentAudioTag = string.Empty;
                                    track.TryGetAttributeValueAsString("AudioTag", out currentAudioTag);
                                    ulong currentPacketSize = 0;
                                    track.TryGetAttributeValueAsUlong("PacketSize", out currentPacketSize);


                                    if ((string.IsNullOrEmpty(AudioTrackName)) || (AudioTrackName == TrackName))
                                    {
                                        audioselected = track;
                                        audiostream = stream;

                                        AudioChunkListConfiguration configuration = new AudioChunkListConfiguration();
                                        configuration.Bitrate = (int)currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString(track.CustomAttributes);

                                        configuration.Duration = (long)Duration;
                                        configuration.TimeScale = (int)TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = currentFourCC;
                                        configuration.BitsPerSample = (int)currentBitsPerSample;
                                        configuration.Channels = (int)currentChannels;
                                        configuration.SamplingRate = (int)currentSamplingRate;
                                        configuration.CodecPrivateData = currentCodecPrivateData;
                                        configuration.AudioTag = currentAudioTag;
                                        configuration.PacketSize = (int)currentPacketSize;
                                        // Evaluation of the buffersize for audio decoding
                                        configuration.MaxFramesize = (int)((currentBitrate * 16) / (10 * currentBitsPerSample * 2));
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        UpdateChunkList(audiostream, configuration);
                                    }
                                }
                            }
                            if (stream.StreamType.ToUpper() == "TEXT")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);
                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    string currentFourCC = string.Empty;
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;

                                    if ((string.IsNullOrEmpty(TextTrackName)) || (TextTrackName == TrackName))
                                    {
                                        textselected = track;
                                        textstream = stream;
                                        TextChunkListConfiguration configuration = new TextChunkListConfiguration();
                                        configuration.Bitrate = (int)currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString(track.CustomAttributes);

                                        configuration.Duration = (long)Duration;
                                        configuration.TimeScale = (int)TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = FourCC;
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        UpdateChunkList(textstream, configuration);
                                    }
                                }
                            }
                        }
                        if (this.manifestBuffer == null)
                            this.manifestBuffer = manifestBuffer;
                        bResult = true;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while updating and parsing Smooth Streaming manifest : " + ex.Message);
                    bResult = false;
                }
            }
            return bResult;
        }
        string GetCustomAttributesString(IDictionary<string, string> Dict)
        {
            string result = string.Empty;
            if(Dict!=null)
            {
                foreach(var val in Dict)
                {
                    if (string.IsNullOrEmpty(result))
                        result = val.Key + "=" + val.Value;
                    else
                        result = ","  + val.Key + "=" + val.Value;
                }
            }
            return result;
        }
        /// <summary>
        /// ParseSmoothManifest
        /// Parse Smooth Streaming manifest 
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>
        private async Task<bool> ParseSmoothManifest()
        {
            bool bResult = false;
            var manifestBuffer = await this.DownloadManifestAsync(true);
            if (manifestBuffer != null)
            {
                try
                { 
                SmoothStreamingManifest parser = new SmoothStreamingManifest(manifestBuffer);
                    if ((parser != null) && (parser.ManifestInfo != null))
                    {
                        ManifestInfo mi = parser.ManifestInfo;

                        ulong Duration = mi.ManifestDuration;
                        string UrlTemplate = string.Empty;

                        this.Duration = Duration;
                        this.TimeScale = mi.Timescale;
                        this.IsLive = mi.IsLive;



                        this.ProtectionGuid = mi.ProtectionGuid;
                        this.ProtectionData = mi.ProtectionData;

                        // We don't support multiple audio. Therefore, we need to download the first audio track. 

                        ListVideoTracks.Clear();
                        ListAudioTracks.Clear();
                        ListTextTracks.Clear();
                        ClearChunkLists();
                        int audioIndex = 0;
                        int textIndex = 0;

                        QualityLevel textselected = null;
                        QualityLevel audioselected = null;
                        QualityLevel videoselected = null;
                        StreamInfo audiostream = null;
                        StreamInfo videostream = null;
                        StreamInfo textstream = null;

                        foreach (StreamInfo stream in mi.Streams)
                        {
                            if (stream.StreamType.ToUpper() == "VIDEO")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language",out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);

                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    ulong currentIndex = 0;
                                    string currentFourCC = string.Empty;
                                    string currentCodecPrivateData = string.Empty;
                                    ulong currentWidth = 0;
                                    ulong currentHeight = 0;
                                    track.TryGetAttributeValueAsUlong("Index", out currentIndex);
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    if (!track.TryGetAttributeValueAsString("CodecPrivateData", out currentCodecPrivateData))
                                        currentCodecPrivateData = string.Empty;
                                    track.TryGetAttributeValueAsUlong("MaxWidth", out currentWidth);
                                    track.TryGetAttributeValueAsUlong("MaxHeight", out currentHeight);
                                    ListVideoTracks.Add(new VideoTrack
                                    {
                                        Index = (int)currentIndex,
                                        Bitrate = (int)currentBitrate,
                                        CustomAttributes = GetCustomAttributesString(track.CustomAttributes),
                                        FourCC = currentFourCC,
                                        MaxHeight = (int)currentHeight,
                                        MaxWidth = (int)currentWidth,
                                        CodecPrivateData = currentCodecPrivateData,
                                        Language = Lang,
                                    });
                                    if (((this.MinBitrate == 0) || (currentBitrate >= (ulong)this.MinBitrate)) &&
                                        ((this.MaxBitrate == 0) || (currentBitrate <= (ulong)this.MaxBitrate)))
                                    {
                                        videoselected = track;
                                        videostream = stream;
                                        VideoChunkListConfiguration configuration = new VideoChunkListConfiguration();
                                        configuration.Bitrate = (int) currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString(track.CustomAttributes);

                                        configuration.Duration = (long) Duration;
                                        configuration.TimeScale = (int) TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = currentFourCC;
                                        configuration.Width = (int)currentWidth;
                                        configuration.Height = (int)currentHeight;
                                        configuration.CodecPrivateData = currentCodecPrivateData;
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        AddChunkList(videostream, configuration,LiveOffset);
                                    }
                                }
                            }
                            if (stream.StreamType.ToUpper() == "AUDIO")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);
                                string AudioTag = string.Empty;
                                stream.TryGetAttributeValueAsString("AudioTag", out AudioTag);
                                ulong PacketSize = 0;
                                stream.TryGetAttributeValueAsUlong("PacketSize", out PacketSize);



                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;
                                    ulong currentBitsPerSample = 16;
                                    ulong currentChannels = 2;
                                    ulong currentSamplingRate = 0;
                                    string currentFourCC = string.Empty;
                                    string currentCodecPrivateData = string.Empty;
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    track.TryGetAttributeValueAsUlong("BitsPerSample", out currentBitsPerSample);
                                    track.TryGetAttributeValueAsUlong("Channels", out currentChannels);
                                    track.TryGetAttributeValueAsUlong("SamplingRate", out currentSamplingRate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    if (!track.TryGetAttributeValueAsString("CodecPrivateData", out currentCodecPrivateData))
                                        currentCodecPrivateData = string.Empty;
                                    string currentAudioTag = string.Empty;
                                    track.TryGetAttributeValueAsString("AudioTag", out currentAudioTag);
                                    ulong currentPacketSize = 0;
                                    track.TryGetAttributeValueAsUlong("PacketSize", out currentPacketSize);

                                    ListAudioTracks.Add(new AudioTrack
                                    {
                                        Index = (int)audioIndex,
                                        Bitrate = (int)currentBitrate,
                                        CustomAttributes = GetCustomAttributesString(track.CustomAttributes),
                                        FourCC = currentFourCC,
                                        BitsPerSample = (int) currentBitsPerSample,
                                        Channels = (int) currentChannels,
                                        SamplingRate = (int) currentSamplingRate,
                                        CodecPrivateData = currentCodecPrivateData,
                                        Language = Lang,
                                    });

                                    if ((string.IsNullOrEmpty(AudioTrackName)) || (AudioTrackName == TrackName))
                                    {
                                        audioselected = track;
                                        audiostream = stream;

                                        AudioChunkListConfiguration configuration = new AudioChunkListConfiguration();
                                        configuration.Bitrate = (int)currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString(track.CustomAttributes);

                                        configuration.Duration = (long)Duration;
                                        configuration.TimeScale = (int)TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = currentFourCC;
                                        configuration.BitsPerSample = (int)currentBitsPerSample;
                                        configuration.Channels = (int)currentChannels;
                                        configuration.SamplingRate = (int) currentSamplingRate;
                                        configuration.CodecPrivateData = currentCodecPrivateData;
                                        configuration.AudioTag = currentAudioTag;
                                        configuration.PacketSize = (int) currentPacketSize;
                                        // Evaluation of the buffersize for audio decoding
                                        configuration.MaxFramesize = (int)((currentBitrate * 16) / (10 * currentBitsPerSample * 2));
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        AddChunkList(audiostream, configuration, LiveOffset);
                                    }
                                }
                            }
                            if (stream.StreamType.ToUpper() == "TEXT")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);
                                string TrackName = string.Empty;
                                stream.TryGetAttributeValueAsString("Name", out TrackName);
                                string FourCC = string.Empty;
                                stream.TryGetAttributeValueAsString("FourCC", out FourCC);
                                foreach (QualityLevel track in stream.QualityLevels)
                                {
                                    ulong currentBitrate = 0;                                    
                                    string currentFourCC = string.Empty;
                                    track.TryGetAttributeValueAsUlong("Bitrate", out currentBitrate);
                                    if (!track.TryGetAttributeValueAsString("FourCC", out currentFourCC))
                                        currentFourCC = string.Empty;
                                    ListTextTracks.Add(new TextTrack
                                    {
                                        Index = (int)textIndex,
                                        Bitrate = (int)currentBitrate,
                                        CustomAttributes = GetCustomAttributesString(track.CustomAttributes),
                                        FourCC = currentFourCC,
                                        Language = Lang,
                                    });
                                    if ((string.IsNullOrEmpty(TextTrackName)) || (TextTrackName == TrackName))
                                    {
                                        textselected = track;
                                        textstream = stream;
                                        TextChunkListConfiguration configuration = new TextChunkListConfiguration();
                                        configuration.Bitrate = (int)currentBitrate;
                                        configuration.CustomAttributes = GetCustomAttributesString(track.CustomAttributes);

                                        configuration.Duration = (long)Duration;
                                        configuration.TimeScale = (int)TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackName = TrackName;
                                        configuration.TrackID = -1;
                                        configuration.FourCC = currentFourCC;
                                        configuration.Source = this.StoragePath;
                                        configuration.ProtectionGuid = this.ProtectionGuid;
                                        configuration.ProtectionData = this.ProtectionData;
                                        AddChunkList(textstream, configuration, LiveOffset);
                                    }
                                }
                            }
                        }
                        if (this.manifestBuffer == null)
                            this.manifestBuffer = manifestBuffer;

                        if(this.IsLive)
                        {
                            this.StartChunkListsUpdateThread(6000);
                        }
                        bResult = true;
                    }                
                }
                catch(Exception ex)
                {
                    LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while parsing Smooth Streaming manifest : " + ex.Message);
                    bResult = false;
                }
            }
            return bResult;
        }
        bool StartChunkListsUpdateThread(int Period)
        {
            if (downloadManifestTaskCancellationtoken == null)
                downloadManifestTaskCancellationtoken = new System.Threading.CancellationTokenSource();
            var t = Task.Run(async  () =>
            {
                this.downloadManifestTaskRunning = true;
                while (this.downloadManifestTaskRunning)
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if ((downloadManifestTaskCancellationtoken != null) && (downloadManifestTaskCancellationtoken.Token.IsCancellationRequested))
                    {
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Update Manifest Thread downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                        // Clean up here, then...
                        downloadManifestTaskCancellationtoken.Token.ThrowIfCancellationRequested();
                    }
                    System.Threading.Tasks.Task.Delay(Period).Wait();
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Update Manifest for Uri: " + ManifestUri.ToString());
                    await UpdateManifest();
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Update Manifest done for Uri: " + ManifestUri.ToString());
                }

            }, downloadManifestTaskCancellationtoken.Token);

            if (t != null)
            {
                downloadManifestTask = t;
                return true;
            }
            return false;
        }
        /// <summary>
        /// UpdateManifest
        /// Download, parse and Update the manifest  
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>        
        public async Task<bool> UpdateManifest()
        {
            bool bResult = false;
            // load the stream associated with the HLS, SMOOTH or DASH manifest

            LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Updating Manifest for Uri: " + ManifestUri.ToString());
            bResult = await this.ParseAndUpdateSmoothManifest();
            if (bResult != true)
            {
                bResult = await this.ParseAndUpdateDashManifest();
                if (bResult != true)
                {
                    bResult = await this.ParseAndUpdateHLSManifest();
                }
            }
            if(bResult==true)
                LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Updating Manifest done for Uri: " + ManifestUri.ToString());
            else
                LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Update Manifest failed for Uri: " + ManifestUri.ToString());
            return bResult;
        }
        /// <summary>
        /// DownloadManifest
        /// Download and parse the manifest  
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if successful</returns>        
        public async Task<bool> DownloadManifest()
        {
            bool bResult = false;
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            this.ManifestStatus = AssetStatus.DownloadingManifest;
            bResult = await this.ParseSmoothManifest();
            if (bResult != true)
            {
                bResult = await this.ParseDashManifest();
                if (bResult != true)
                {
                    bResult = await this.ParseHLSManifest();
                }
            }
            if (bResult == true)
                this.ManifestStatus = AssetStatus.ManifestDownloaded;
            else
                this.ManifestStatus = AssetStatus.ErrorManifestDownload;
            return bResult;
        }
        /// <summary>
        /// SetDiskCache
        /// Associate a Disk cache with the manifest.
        /// The DiskCache will be used to store the manifest on disk
        /// </summary>
        /// <param name="cache">DiskCache</param>
        /// <returns>true if successful</returns>
        public bool SetManifestOutput(ManifestOutput cache)
        {
            if (cache != null)
            {
                OutputManager = cache;
                return true;
            }
            return false;
        }

        /// <summary>
        /// CreateManifestCache
        /// Create a manifest cache.
        /// 
        /// </summary>
        /// <param name="manifestUri">manifest Uri</param>
        /// <param name="downloadToGo">true if Download To Go experience, Progressive Downlaoad experience  otherwise</param>
        /// <param name="minBitrate">mininmum bitrate for the video track</param>
        /// <param name="maxBitrate">maximum bitrate for the video track</param>
        /// <param name="audioIndex">Index of the audio track to select (usually 0)</param>
        /// <param name="maxMemoryBufferSize">maximum memory buffer size</param>
        /// <param name="maxError">maximum http error while downloading the chunks</param>
        /// <param name="downloadMethod"> 0 Auto: The cache will create if necessary several threads to download audio and video chunks  
        ///                             1 Default: The cache will download the audio and video chunks step by step in one single thread
        ///                             N The cache will create N parallel threads to download the audio chunks and N parallel threads to downlaod video chunks </param>
        /// <returns>return a ManifestCache (null if not successfull)</returns>
        public static ManifestManager CreateManifestCache(Uri manifestUri,  ulong minBitrate, ulong maxBitrate, string AudioTrackName, string TextTrackName, ulong MaxDuration, ulong maxMemoryBufferSize, int liveOffset, Options opt = null)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestManager mc = new ManifestManager(manifestUri, minBitrate, maxBitrate, AudioTrackName, TextTrackName, MaxDuration, maxMemoryBufferSize,  liveOffset, opt);
            if (mc != null)
            {
                mc.ManifestStatus = AssetStatus.Initialized;
            }
            return mc;
        }
        /// <summary>
        /// CreateManifestCache
        /// Create a manifest cache.
        /// 
        /// </summary>
        /// <param name="manifestUri">manifest Uri</param>
        /// <param name="downloadToGo">true if Download To Go experience, Progressive Downlaoad experience  otherwise</param>
        /// <param name="audioIndex">Index of the video track to select </param>
        /// <param name="audioIndex">Index of the audio track to select (usually 0)</param>
        /// <param name="maxMemoryBufferSize">maximum memory buffer size</param>
        /// <param name="maxError">maximum http error while downloading the chunks</param>
        /// <param name="downloadMethod"> 0 Auto: The cache will create if necessary several threads to download audio and video chunks  
        ///                             1 Default: The cache will download the audio and video chunks step by step in one single thread
        ///                             N The cache will create N parallel threads to download the audio chunks and N parallel threads to downlaod video chunks </param>
        /// <returns>return a ManifestCache (null if not successfull)</returns>
        public static ManifestManager CreateManifestCache(Uri manifestUri, bool downloadToGo, ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestManager mc = new ManifestManager(manifestUri,  maxMemoryBufferSize, maxError, downloadMethod);
            if (mc != null)
            {
                mc.ManifestStatus = AssetStatus.Initialized;
            }
            return mc;
        }
        /// <summary>
        /// ComputeHash
        /// Convert the manifest Uri into a unique string which will be the folder name where the asset will be stored.
        /// </summary>
        /// <param name="message">string to hash</param>
        /// <returns>string</returns>
        private string ComputeHash(string message)
        {
            // Convert the message string to binary data.
            //Windows.Storage.Streams.IBuffer buffUtf8Msg = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(message, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
            //Windows.Security.Cryptography.Core.HashAlgorithmProvider sha1 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm("SHA1");
            //Windows.Storage.Streams.IBuffer hash = sha1.HashData(buffUtf8Msg);
            //byte[] array = new byte[hash.Length];
            //Windows.Storage.Streams.DataReader dr = Windows.Storage.Streams.DataReader.FromBuffer(hash);
            //dr.ReadBytes(array);
            //ConvertStringToBinary(message, System.Security.Cryptography.Core. //BinaryStringEncoding.Utf8);
            var sha1 = System.Security.Cryptography.SHA1.Create();


            byte[] myByteArray = System.Text.Encoding.UTF8.GetBytes(message);

            var hash = sha1.ComputeHash(myByteArray);
            string[] hex = new string[hash.Length];
            for (int i = 0; i < hash.Length; i++)
                hex[i] = hash[i].ToString("X2");
            return string.Concat(hex);
        }
        /// <summary>
        /// ComputeManifest
        /// Convert the manifest Uri into a unique string which will be the folder name where the asset will be stored.
        /// </summary>
        /// <param name="message">string to hash</param>
        /// <returns>string</returns>
        private string ComputeManifest(string message)
        {
            string result = "source";
            string[] list = message.Split("/");
            if((list!=null)&&(list.Length>1))
            {
                if(list[list.Length-1].ToLower()!="manifest")
                {
                    result = list[list.Length - 1];
                }
                else
                {
                    result = list[list.Length - 2];
                    int pos = result.IndexOf(".");
                    if(pos>0)
                        result = result.Substring(0, pos);
                }
                if (string.IsNullOrEmpty(result))
                    result = "source";
            }
            return result;
        }
        /// <summary>
        /// IsDownlaodTaskRunning
        /// return true if the download thread is still running.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if the download thread is still running</returns>
        public bool IsDownlaodTaskRunning()
        {
            return downloadTaskRunning;
        }
        /// <summary>
        /// Method: GetExpectedSize
        /// Return the expected size in byte of the asset to be downloaded 
        /// </summary>
        public ulong GetExpectedSize()
        {
            //ulong expectedSize = ((this.AudioBitrate + this.VideoBitrate) * this.TimescaleToHNS(this.Duration)) / (8 * ManifestCache.TimeUnit);
            ulong duration = this.TimescaleToHNS(this.Duration) / (ManifestManager.TimeUnit);
            ulong expectedSize = ((this.AudioBitrate + this.VideoBitrate) * duration) / 8 ;
            return expectedSize;
        }
        /// <summary>
        /// Method: GetMediaSize
        /// Return the media size in byte of the asset  
        /// </summary>
        public ulong GetMediaSize()
        {
            UInt64 OutputBytes = 0;


            foreach (ChunkList cl in this.AudioChunkListList)
            {
                OutputBytes += cl.OutputBytes;
            }
            foreach (ChunkList cl in this.VideoChunkListList)
            {
                OutputBytes += cl.OutputBytes;
            }
            foreach (ChunkList cl in this.TextChunkListList)
            {
                OutputBytes += cl.OutputBytes;
            }

            return OutputBytes;
        }
        public ulong GetInputVideoSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.VideoChunkListList)
            {
                result += cl.InputBytes;
            }
            return result;
        }
        public ulong GetOutputVideoSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.VideoChunkListList)
            {
                result += cl.OutputBytes;
            }
            return result;
        }

        public ulong GetInputAudioSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.AudioChunkListList)
            {
                result += cl.InputBytes;
            }
            return result;
        }
        public ulong GetOutputAudioSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.AudioChunkListList)
            {
                result += cl.OutputBytes;
            }
            return result;
        }
        public ulong GetInputTextSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.TextChunkListList)
            {
                result += cl.InputBytes;
            }
            return result;
        }
        public ulong GetOutputTextSize()
        {
            UInt64 result = 0;
            foreach (ChunkList cl in this.TextChunkListList)
            {
                result += cl.OutputBytes;
            }
            return result;
        }

        /// <summary>
        /// GetCurrentBitrate
        /// return the estimated download bitrate.
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the estimated download bitrate in seconds</returns>
        public double GetCurrentBitrate()
        {
            if ((IsDownloadCompleted(VideoChunkListList)) &&
                (IsDownloadCompleted(AudioChunkListList)) &&
                (IsDownloadCompleted(TextChunkListList) ))
                return 0;
            DateTime time = DateTime.Now;
            double currentBitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
            return currentBitrate;
        }
        
        /// <summary>
        /// DownloadChunkAsync
        /// Download asynchronously a chunk.
        /// </summary>
        /// <param name="chunkUri">Uri of the chunk to download </param>
        /// <param name="forceNewDownload">if true force the downlaod (adding a guid at the end of the uri) </param>
        /// <returns>return a byte array containing the chunk</returns>
        public virtual async Task<byte[]> DownloadChunkAsync(Uri chunkUri, bool forceNewDownload = true)
        {
            LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk start for chunk: " + chunkUri.ToString() );

            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    if (forceNewDownload)
                    {
                        string modifier = chunkUri.AbsoluteUri.Contains("?") ? "&" : "?";
                        string newUriString = string.Concat(chunkUri.AbsoluteUri, modifier, "ignore=", Guid.NewGuid());
                        chunkUri = new Uri(newUriString);
                    }
                    //                System.Net.Http.HttpResponseMessage response = await client.GetAsync(chunkUri, System.Net.Http.HttpCompletionOption.ResponseContentRead).AsTask(downloadTaskCancellationtoken.Token);
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync(chunkUri, System.Net.Http.HttpCompletionOption.ResponseContentRead);

                    response.EnsureSuccessStatusCode();
                    byte[] buffer = await response.Content.ReadAsByteArrayAsync();
                    if (buffer != null)
                    {
                        int val = buffer.Length;
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk done for chunk: " + chunkUri.ToString());
                        return buffer.ToArray();
                    }
                }
                catch (Exception e)
                {
                    LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk exception: " + e.Message + " for chunk: " + chunkUri.ToString());
                }
            }
            return null;
            
        }

        /// <summary>
        /// IsDownloadCompleted
        /// .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if nothing to download</returns>
        public bool IsDownloadCompleted(List<ChunkList> list)
        {
            foreach (var cl in list)
            {
                ulong i = cl.InputChunks;
                if (i < cl.TotalChunks)
                    return false;
            }
            return true ;
        }
        /// <summary>
        /// CreateMFRABuffer
        /// .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if nothing to download</returns>
        public bool CreateMFRABuffer(List<ChunkList> list, ulong TimeFirstVideoChunk)
        {
            if (list == null)
                return false;
            foreach (var cl in list)
            {
                ulong offsetWithVideo = 0;
                if (cl.TimeFirstChunk > TimeFirstVideoChunk)
                    offsetWithVideo = cl.TimeFirstChunk - TimeFirstVideoChunk;
                cl.mfraData = cl.Configuration.GetMFRAData(offsetWithVideo);
            }
            return true;
        }

        /// <summary>
        /// IsArchiveCompleted
        /// .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return true if archive is done</returns>
        public bool IsArchiveCompleted(List<ChunkList> list)
        {
            foreach (var cl in list)
            {
                ulong i = cl.OutputChunks;
                if (i < cl.TotalChunks)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// GetChunkListSize
        /// .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return number of bytes in the list</returns>
        ulong GetChunkListSize(List<ChunkList> list)
        {
            ulong listSize = 0;
            foreach (var cl in list)
            {
                listSize += cl.MemorySizeForChunks;

                //lock (cl.ChunksQueue)
                //{
                //    var enu = cl.ChunksQueue.GetEnumerator();
                //    if(enu!=null)
                //    {
                //        while((enu.MoveNext()) && (enu.Current !=null ))
                //        {
                //            listSize += (ulong)(enu.Current.chunkBuffer == null ? 0:enu.Current.chunkBuffer.Length);
                //        }
                //    }
                //}
            }
            return listSize;
        }
        /// <summary>
        /// GetTrackID
        /// Get the trackID from moof in data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <returns>return the TrackID</returns>
        int GetTrackID(byte[] data)
        {
            int result = -1;
            try
            {

                ISMHelper.Mp4Box box  = ISMHelper.Mp4Box.CreateMp4Box(data, 0);
                if(box.GetBoxType()== "moof")
                {
                    List<ISMHelper.Mp4Box> listmoof =  box.GetChildren();
                    if(listmoof!=null)
                    {
                        foreach(var boxinmoof in listmoof)
                        {
                            if(boxinmoof.GetBoxType()=="traf")
                            {
                                List<ISMHelper.Mp4Box> listtraf = boxinmoof.GetChildren();
                                if (listtraf != null)
                                {
                                    foreach (var boxintraf in listtraf)
                                    {
                                        if (boxintraf.GetBoxType() == "tfhd")
                                        {
                                            byte[] buffer =  boxintraf.GetBoxData();
                                            if(buffer!=null)
                                            {
                                                return ISMHelper.Mp4Box.ReadMp4BoxInt32(buffer, 4);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch(Exception)
            {

            }
            return result;
        }
        byte[] RemoveLiveTimestampAndUpdateSequenceNumber(byte[] buffer, ulong sequenceNumber)
        {
            byte[] result = null;
            if ((buffer != null) && (buffer.Length > 0))
            {
                int offset = 0;
                while (offset < buffer.Length)
                {
                    ISMHelper.Mp4Box box = ISMHelper.Mp4Box.ReadMp4BoxFromBuffer(buffer, offset);
                    if (box != null)
                    {
                        if (box.GetBoxType() == "moof")
                        {
                            // Remove uuid box A239...8DF4 which contains the IV (initialisation vectors for the encryption)
                            // Keep the list of IV (initialisation vectors for the encryption) included in this box
                            // Keep the list of sample size
                            // Calculate the new lenght and keep the difference with the previous lenght
                            // Open the next box (mdat) and decrypt sample by sample 
                            ISMHelper.Mp4BoxMOOF moof = box as ISMHelper.Mp4BoxMOOF;
                            if (moof != null)
                            {
                                int currentLength = moof.GetBoxLength();
                                int currentTrackID = moof.GetTrackID();
                                bool bRemoved = false;

                                ISMHelper.Mp4BoxUUID uuidextbox = moof.GetUUIDBox(ISMHelper.Mp4Box.kTrackFragExtHeaderBoxGuid) as ISMHelper.Mp4BoxUUID;
                                if (uuidextbox != null)
                                {
                                    moof.RemoveUUIDBox(ISMHelper.Mp4Box.kTrackFragExtHeaderBoxGuid);
                                    bRemoved = true;
                                }
                                ISMHelper.Mp4BoxUUID uuidbox = moof.GetUUIDBox(ISMHelper.Mp4Box.kTrackFragHeaderBoxGuid) as ISMHelper.Mp4BoxUUID;
                                if (uuidbox != null)
                                {
                                    moof.RemoveUUIDBox(ISMHelper.Mp4Box.kTrackFragHeaderBoxGuid);
                                    bRemoved = true;
                                }
                                Int32 decreasedSize = 0;
                                if (bRemoved == true)
                                {
                                    moof.UpdateMFHDSequence((int)sequenceNumber);
                                    ISMHelper.Mp4BoxTRUN trunbox = moof.GetChild("trun") as ISMHelper.Mp4BoxTRUN;
                                    if (trunbox != null)
                                    {
                                        Int32 doff = trunbox.GetDataOffset();
                                        if (uuidextbox != null)
                                            decreasedSize += uuidextbox.GetBoxLength();
                                        if (uuidbox != null)
                                            decreasedSize += uuidbox.GetBoxLength();
                                        doff -= decreasedSize;
                                        if (doff > 0)
                                            trunbox.SetDataOffset(doff);
                                    }
                                    byte[] newBuffer = moof.UpdateBoxBuffer();
                                    result = new byte[buffer.Length - decreasedSize];
                                    if (result != null)
                                    {
                                        Buffer.BlockCopy(buffer, 0, result, 0, offset);
                                        Buffer.BlockCopy(newBuffer, 0, result, offset, moof.GetBoxLength());
                                        Buffer.BlockCopy(buffer, offset + moof.GetBoxLength() + decreasedSize, result, offset + moof.GetBoxLength(), buffer.Length - offset - moof.GetBoxLength() - decreasedSize);
                                        return result;
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {

                        }
                        offset += box.GetBoxLength();
                    }
                    else
                        break;
                }
            }
            return buffer;
        }

        /// <summary>
        /// DownloadCurrentChunks
        /// Download the current audio chunk .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the lenght of the downloaded chunk</returns>
        async Task<long> DownloadCurrentChunks(List<ChunkList> list,bool bCreateMFRA)
        {
            long len = 0;
            foreach (var cl in list)
            {
                ChunkBuffer cb;
                if(cl.ChunksToReadQueue.TryDequeue(out cb))
                {
                    string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cb.Time.ToString());
                    cb.chunkBuffer = await DownloadChunkAsync(new Uri(url));
                    if ((cl.Configuration != null) && (cl.Configuration.TrackID == -1) && (cb.IsChunkDownloaded()))
                    {
                        cl.Configuration.TrackID = GetTrackID(cb.chunkBuffer);

                        if (cl.Configuration.TrackID != -1)
                        {
                            // Now the TrackID is available, we can build ftyp and moov boxes
                            cl.ftypData = cl.Configuration.GetFTYPData();
                            cl.moovData = cl.Configuration.GetMOOVData();
                            if(bCreateMFRA)
                                cl.LastDataOffset = (ulong) (cl.ftypData.Length + cl.moovData.Length);
                        }

                    }
                    if (cb.IsChunkDownloaded())
                    {
                        if (bCreateMFRA)
                            cb.chunkBuffer = RemoveLiveTimestampAndUpdateSequenceNumber(cb.chunkBuffer, cl.InputChunks + 1);

                        ulong l = cb.GetLength();
                        cl.InputBytes += l;
                        len += (long)l;
                        cl.InputChunks++;
                        if (bCreateMFRA)
                        {
                            cl.Configuration.AddTimeOffset(cb.Time, cl.LastDataOffset);
                            cl.LastDataOffset += l;
                        }
                        // Set Time for chunks download
                        if((cl.TimeFirstChunk == 0)&& (cl.TimeLastChunk == 0))
                            cl.TimeFirstChunk = cb.Time;
                        else
                            cl.TimeLastChunk = cb.Time + cb.Duration;

                        cl.ChunksQueue.Enqueue(cb);
                        cl.MemorySizeForChunks += l;

                    }
                    else
                        len = -1;

                }
                //ulong i = cl.InputChunks;
                //if (i < cl.TotalChunks)
                //{
                //    string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList.Values.ElementAt((int)i).Time.ToString());
                //    cl.ChunksList.Values.ElementAt((int)i).chunkBuffer = await DownloadChunkAsync(new Uri(url));
                //    if ((cl.Configuration != null) && (cl.Configuration.TrackID == -1))
                //    {
                //        cl.Configuration.TrackID = GetTrackID(cl.ChunksList.Values.ElementAt((int)i).chunkBuffer);

                //        if (cl.Configuration.TrackID != -1)
                //        {
                //            // Now the TrackID is available, we can build ftyp and moov boxes
                //            cl.ftypData = cl.Configuration.GetFTYPData();
                //            cl.moovData = cl.Configuration.GetMOOVData();
                //        }

                //    }
                //    if (cl.ChunksList.Values.ElementAt((int)i).IsChunkDownloaded())
                //    {
                //        ulong l = cl.ChunksList.Values.ElementAt((int)i).GetLength();
                //        cl.InputBytes += l;
                //        len += (long)l;
                //        cl.InputChunks++;
                //    }
                //    else
                //        len = -1;




                    //string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList[(int)i].Time.ToString());
                    //cl.ChunksList[(int)i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
                    //if((cl.Configuration!=null) && (cl.Configuration.TrackID == -1))
                    //{
                    //    cl.Configuration.TrackID = GetTrackID(cl.ChunksList[(int)i].chunkBuffer);

                    //    if (cl.Configuration.TrackID != -1)
                    //    {
                    //        // Now the TrackID is available, we can build ftyp and moov boxes
                    //        cl.ftypData = cl.Configuration.GetFTYPData();
                    //        cl.moovData = cl.Configuration.GetMOOVData();
                    //    }

                    //}
                    //if (cl.ChunksList[(int)i].IsChunkDownloaded())
                    //{
                    //    ulong l = cl.ChunksList[(int)i].GetLength();
                    //    cl.InputBytes += l;
                    //    len += l;
                    //    cl.InputChunks++;
                    //}
                //}
            }
            return len;
        }
        //Task StartDowloadParallelThread(List<ChunkList> list, ulong i)
        //{

        //    return Task.Run(async () =>
        //    {
        //        foreach (var cl in list)
        //        {
        //            string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList.Values.ElementAt((int)i).Time.ToString());
        //            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks for Uri: " + url.ToString());
        //            cl.ChunksList.Values.ElementAt((int)i).chunkBuffer = await DownloadChunkAsync(new Uri(url));
        //            if (cl.ChunksList.Values.ElementAt((int)i).IsChunkDownloaded())
        //                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks done for Uri: " + url.ToString());
        //            else
        //                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks error for Uri: " + url.ToString());
        //        }

        //    }, downloadTaskCancellationtoken.Token);
            //return Task.Run(async () =>
            //{
            //    foreach (var cl in list)
            //    {
            //        string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList[(int)i].Time.ToString());
            //        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks for Uri: " + url.ToString());
            //        cl.ChunksList[(int)i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
            //        if (cl.ChunksList[(int)i].IsChunkDownloaded())
            //            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks done for Uri: " + url.ToString());
            //        else
            //            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks error for Uri: " + url.ToString());
            //    }

            //}, downloadTaskCancellationtoken.Token);

        //}
        public ulong GetMinChunkDurationMs()
        {
            ulong TimeMin = 0;
            foreach (var l in VideoChunkListList)
            {
                if ((l.TimeLastChunk != 0) && ((TimeMin == 0) ||  ((l.TimeLastChunk - l.TimeFirstChunk) < TimeMin)))
                    TimeMin = l.TimeLastChunk - l.TimeFirstChunk;
            }
            foreach (var l in AudioChunkListList)
            {
                if ((l.TimeLastChunk != 0) && ((TimeMin == 0) || ((l.TimeLastChunk - l.TimeFirstChunk) < TimeMin)))
                    TimeMin = l.TimeLastChunk - l.TimeFirstChunk;
            }
            foreach (var l in TextChunkListList)
            {
                if ((l.TimeLastChunk != 0) && ((TimeMin == 0) || ((l.TimeLastChunk - l.TimeFirstChunk) < TimeMin)))
                    TimeMin = l.TimeLastChunk - l.TimeFirstChunk;
            }
            ulong val = TimescaleToHNS(TimeMin) / 10000;
            return val;
        }
        /// <summary>
        /// downloadThread
        /// Download thread 
        /// This thread download one by one the audio and video chunks
        /// When the amount of chunks in memory is over a defined limit, the chunks are stored on disk and disposed
        /// When the download is completed, the thread exits 
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the length of the downloaded audio chunk</returns>
        public async  Task<bool> downloadThread()
        {

            LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread started for Uri: " + ManifestUri.ToString());
            downloadTaskRunning = true;
            // Were we already canceled?
            if (downloadTaskCancellationtoken != null)
            {
                LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                downloadTaskCancellationtoken.Token.ThrowIfCancellationRequested();
            }


            DownloadThreadStartTime = DateTime.Now;
            DownloadThreadAudioCount = 0;
            DownloadThreadVideoCount = 0;


            bool bCreateMFRA = !(this.IsLive && (this.MaxDuration == 0));
            ManifestStatus = AssetStatus.DownloadingChunks;
            int error = 0;
            while (downloadTaskRunning)
            {
                // Poll on this property if you have to do
                // other cleanup before throwing.
                if ((downloadTaskCancellationtoken != null) && (downloadTaskCancellationtoken.Token.IsCancellationRequested))
                {
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                    // Clean up here, then...
                    downloadTaskCancellationtoken.Token.ThrowIfCancellationRequested();
                }

                bool result = false;
                // Download Video Chunks
                if((VideoChunkListList != null)&&(VideoChunkListList.Count>0))
                {
                    // Something to download
                    if(!IsDownloadCompleted(VideoChunkListList))
                    {
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks for Uri: " + ManifestUri.ToString());
                        long len = await DownloadCurrentChunks(VideoChunkListList, bCreateMFRA);
                        if (len >= 0)
                        {
                            DownloadThreadVideoCount += (ulong)len;
                            result = true;
                        }
                        else
                            LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks error for Uri: " + ManifestUri.ToString());

                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks done for Uri: " + ManifestUri.ToString());

                    }
                    else
                    {
                        if (this.IsLive)
                            result = true;
                    }
                }

                if (downloadTaskRunning == false)
                {
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Download Audio Chunks
                if ((AudioChunkListList != null) && (AudioChunkListList.Count > 0))
                {
                    // Something to download
                    if (!IsDownloadCompleted(AudioChunkListList))
                    {
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks for Uri: " + ManifestUri.ToString());
                        long len = await DownloadCurrentChunks(AudioChunkListList, bCreateMFRA);
                        if (len >= 0)
                        {
                            DownloadThreadAudioCount += (ulong)len;
                            result = true;
                        }
                        else
                            LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks error for Uri: " + ManifestUri.ToString());
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks done for Uri: " + ManifestUri.ToString());

                    }
                    else
                    {
                        if (this.IsLive)
                            result = true;
                    }
                }
                if (downloadTaskRunning == false)
                {
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Download Text Chunks
                if ((TextChunkListList != null) && (TextChunkListList.Count > 0))
                {
                    // Something to download
                    if (!IsDownloadCompleted(TextChunkListList))
                    {
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading text chunks for Uri: " + ManifestUri.ToString());
                        long len = await DownloadCurrentChunks(TextChunkListList, bCreateMFRA);
                        if (len >= 0)
                        {
                            DownloadThreadTextCount += (ulong)len;
                            result = true;
                        }
                        else
                            LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading text chunks error for Uri: " + ManifestUri.ToString());
                        LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading text chunks done for Uri: " + ManifestUri.ToString());

                    }
                    else
                    {
                        if (this.IsLive)
                            result = true;
                    }
                }
                if (downloadTaskRunning == false)
                {
                    LogMessage(Options.LogLevel.Verbose, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Get StartTime from the chunks

                if (result == false)
                {
                    error++;
                    if (error > MaxError)
                    {
                        DateTime time = DateTime.Now;
                        LogMessage(Options.LogLevel.Error, "Download stopped too many error at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                        LogMessage(Options.LogLevel.Error, "Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                        double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                        LogMessage(Options.LogLevel.Error, "Download bitrate for the current session: " + bitrate.ToString() + " bps");
                        ManifestStatus = AssetStatus.ErrorChunksDownload;
                        downloadTaskRunning = false;
                        downloadManifestTaskRunning = false; 
                    }
                }
                if (IsDownloadCompleted(VideoChunkListList)&&
                    IsDownloadCompleted(AudioChunkListList)&&
                    IsDownloadCompleted(TextChunkListList)&&
                        (!this.IsLive))
                {
                    DateTime time = DateTime.Now;
                    if (bCreateMFRA)
                    {
                        ulong TimeFirstVideoChunk = 0;
                        ChunkList cl = VideoChunkListList.First();
                        if (cl != null)
                            TimeFirstVideoChunk = cl.TimeFirstChunk;

                        CreateMFRABuffer(VideoChunkListList, TimeFirstVideoChunk);
                        CreateMFRABuffer(AudioChunkListList, TimeFirstVideoChunk);
                        CreateMFRABuffer(TextChunkListList, TimeFirstVideoChunk);
                    }
                    LogMessage(Options.LogLevel.Information, "Download done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                    LogMessage(Options.LogLevel.Information, "Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                    double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                    LogMessage(Options.LogLevel.Information, "Download bitrate for the current session: " + bitrate.ToString() + " bps");
                    LogMessage(Options.LogLevel.Information, "Download Thread Saving Asset");
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (OutputManager != null)
                        await OutputManager.ProcessChunks(this);
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());
                    ManifestStatus = AssetStatus.ChunksDownloaded;
                    downloadTaskRunning = false;
                    downloadManifestTaskRunning = false;
                    break;
                }
                DateTime currentTime = DateTime.Now;
                if ((MaxDuration > 0) &&
                 //((currentTime - DownloadThreadStartTime).TotalMilliseconds > MaxDuration))
                    (GetMinChunkDurationMs() > MaxDuration))
                {
                    DateTime time = DateTime.Now;
                    if (bCreateMFRA)
                    {
                        ulong TimeFirstVideoChunk = 0;
                        ChunkList cl = VideoChunkListList.First();
                        if (cl != null)
                            TimeFirstVideoChunk = cl.TimeFirstChunk;
                        CreateMFRABuffer(VideoChunkListList, TimeFirstVideoChunk);
                        CreateMFRABuffer(AudioChunkListList, TimeFirstVideoChunk);
                        CreateMFRABuffer(TextChunkListList, TimeFirstVideoChunk);
                    }
                    LogMessage(Options.LogLevel.Information, "Download stopped at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time) + " at MaxDuration: " + MaxDuration.ToString());
                    LogMessage(Options.LogLevel.Information,"Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                    double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                    LogMessage(Options.LogLevel.Information, "Download bitrate for the current session: " + bitrate.ToString() + " bps");
                    LogMessage(Options.LogLevel.Information, "Download Thread Saving Asset");
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (OutputManager != null)
                        await OutputManager.ProcessChunks(this);
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());
                    ManifestStatus = AssetStatus.ChunksDownloaded;
                    downloadTaskRunning = false;
                    downloadManifestTaskRunning = false;
                    break;
                }
                ulong s = GetBufferSize();
                if (s > MaxMemoryBufferSize)
                {
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (OutputManager != null)
                        await OutputManager.ProcessChunks(this);
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());

                }

            }

            downloadTaskRunning = false;
            downloadManifestTaskRunning = false;
            LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread ended for Uri: " + ManifestUri.ToString());
            return true; 

        }
        /// <summary>
        /// GetBufferSize
        /// Return the amount of audio and video chunk in memory
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return the amount of audio and video chunk in memory</returns>
        private ulong GetBufferSize()
        {
            ulong audioLength = GetChunkListSize(AudioChunkListList);
            ulong videoLength = GetChunkListSize(VideoChunkListList);
            ulong textLength = GetChunkListSize(TextChunkListList);
            return audioLength + videoLength + textLength;
        }
        /// <summary>
        /// InitializeDownloadParameters
        /// Initialize the download parameters
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private async Task<bool> InitializeDownloadParameters()
        {


            if (OutputManager!=null)
                await OutputManager.ProcessManifest(this);
            return true;
        }
        /// <summary>
        /// StopDownloadThread
        /// Stop the download parameters
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private bool StopDownloadThread()
        {
            if ((downloadTask != null) && (downloadTaskRunning == true))
            {
                LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread for Uri: " + ManifestUri.ToString());
                LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask downloadTaskRunning = false for Uri: " + ManifestUri.ToString());
                downloadTaskRunning = false;
                if (!downloadTask.IsCompleted)
                {
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask Cancel token for Uri: " + ManifestUri.ToString());
                    downloadTaskCancellationtoken.Cancel();
                }
                try
                {
                    LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Waiting end of thread for Uri: " + ManifestUri.ToString());
                    downloadTask.Wait(500);
                }
                catch (Exception e)
                {
                    LogMessage(Options.LogLevel.Error, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Exception for Uri: " + ManifestUri.ToString() + " exception: " + e.Message);
                }

                LogMessage(Options.LogLevel.Information, string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread completed for Uri: " + ManifestUri.ToString());
                downloadTask = null;
            }
            return true;
        }

        /// <summary>
        /// PauseDownloadChunks
        /// Pause the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool PauseDownloadChunks()
        {
            return StopDownloadThread();
        }
        /// <summary>
        /// StopDownloadChunks
        /// Stop the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool StopDownloadChunks()
        {
            return StopDownloadThread();
        }
        /// <summary>
        /// StartDownloadThread
        /// Start the downlaod thread
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        private bool StartDownloadThread()
        {
            if (downloadTask == null)
            {
                downloadTaskRunning = false;
                if(downloadTaskCancellationtoken == null)
                    downloadTaskCancellationtoken = new System.Threading.CancellationTokenSource();
                if(DownloadMethod == 1)
                    downloadTask = Task.Run(async () => { await downloadThread(); }, downloadTaskCancellationtoken.Token);
                //else
                //    downloadTask = Task.Run(async () => { await downloadParallelThread(); }, downloadTaskCancellationtoken.Token);
                if (downloadTask != null)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// StartDownloadChunks
        /// Start the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public async Task<bool> StartDownloadChunks()
        {

            // stop the download thread if running
            StopDownloadThread();
            // Initialize Download paramters
            await InitializeDownloadParameters();
            return StartDownloadThread();
        }

        /// <summary>
        /// ResumeDownloadChunks
        /// Resume the download of chunks
        /// </summary>
        /// <param name=""></param>
        /// <returns>Return true if successful</returns>
        public bool ResumeDownloadChunks()
        {
            // stop the download thread if running
            StopDownloadThread();
            return StartDownloadThread();
        }

        /// <summary>
        /// GetTextChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        //public ChunkBuffer GetTextChunkCache(int Index, ulong time)
        //{
        //    if (Index < TextChunkListList.Count)
        //    {
        //        for (int i = 0; i < TextChunkListList[Index].ChunksList.Count; i++)
        //        {
        //            if (TextChunkListList[Index].ChunksList.Values.ElementAt(i).Time == time)
        //                return TextChunkListList[Index].ChunksList.Values.ElementAt(i);
        //        }
        //    }

            //if (Index < TextChunkListList.Count)
            //{
            //    for (int i = 0; i < TextChunkListList[Index].ChunksList.Count; i++)
            //    {
            //        if (TextChunkListList[Index].ChunksList[i].Time == time)
            //            return TextChunkListList[Index].ChunksList[i];
            //    }
            //}
        //    return null;
        //}
        /// <summary>
        /// GetAudioChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        //public ChunkBuffer GetAudioChunkCache(int Index, ulong time)
        //{
        //    if (Index < AudioChunkListList.Count)
        //    {
        //        for (int i = 0; i < AudioChunkListList[Index].ChunksList.Count; i++)
        //        {
        //            if (AudioChunkListList[Index].ChunksList.Values.ElementAt(i).Time == time)
        //                return AudioChunkListList[Index].ChunksList.Values.ElementAt(i);
        //        }
        //    }
            //if (Index < AudioChunkListList.Count)
            //{
            //    for (int i = 0; i < AudioChunkListList[Index].ChunksList.Count; i++)
            //    {
            //        if (AudioChunkListList[Index].ChunksList[i].Time == time)
            //            return AudioChunkListList[Index].ChunksList[i];
            //    }
            //}
        //    return null;
        //}
        /// <summary>
        /// GetVideoChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        //public ChunkBuffer GetVideoChunkCache(int Index, ulong time)
        //{
            //if (Index < VideoChunkListList.Count)
            //{
            //    for (int i = 0; i < VideoChunkListList[Index].ChunksList.Count; i++)
            //    {
            //        if (VideoChunkListList[Index].ChunksList[i].Time == time)
            //            return VideoChunkListList[Index].ChunksList[i];
            //    }
            //}
        //    if (Index < VideoChunkListList.Count)
        //    {
        //        for (int i = 0; i < VideoChunkListList[Index].ChunksList.Count; i++)
        //        {
        //            if (VideoChunkListList[Index].ChunksList.Values.ElementAt(i).Time == time)
        //                return VideoChunkListList[Index].ChunksList.Values.ElementAt(i);
        //        }
        //    }
        //    return null;
        //}

        #region Events
        /// <summary>
        /// ManifestStatus
        /// Return a ManifestStatus 
        /// </summary>
        public AssetStatus ManifestStatus {
            get
            {
                return mStatus;
            }
            private set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    if (StatusProgress!=null)
                        StatusProgress(this, mStatus);
                }
            }
        }
        private uint downloadedPercentage;
        /// <summary>
        /// DownloadedPercentage
        /// Return a downloaded Percentage based on the number of audio and video chunks downloaded
        /// </summary>
        public uint DownloadedPercentage {
            get
            {
                ulong TotalVideoChunks = 0;
                ulong TotalVideoDownloadedChunks = 0;
                foreach (var l in VideoChunkListList)
                {
                    TotalVideoDownloadedChunks += l.InputChunks;
                    TotalVideoChunks += l.TotalChunks;
                }

                ulong TotalAudioChunks = 0;
                ulong TotalAudioDownloadedChunks = 0;
                foreach (var l in AudioChunkListList)
                {
                    TotalAudioDownloadedChunks += l.InputChunks;
                    TotalAudioChunks += l.TotalChunks;
                }
                ulong TotalTextChunks = 0;
                ulong TotalTextDownloadedChunks = 0;
                foreach (var l in TextChunkListList)
                {
                    TotalTextDownloadedChunks += l.InputChunks;
                    TotalTextChunks += l.TotalChunks;
                }

                downloadedPercentage = (uint)(((TotalTextDownloadedChunks + TotalAudioDownloadedChunks + TotalVideoDownloadedChunks) * 100) / (TotalTextChunks + TotalAudioChunks + TotalVideoChunks));
                return downloadedPercentage;
            }
            set
            {
                if(downloadedPercentage != value)
                {
                    downloadedPercentage = value;
                    if (DownloadProgress != null)
                        DownloadProgress(this, downloadedPercentage);
                } 
            }
        }


        /// <summary>
        /// This event is used to track the download progress
        /// The parameter is an int between 0 and 100
        /// </summary>
        public event System.EventHandler<uint> DownloadProgress;
        /// <summary>
        /// This event is used to track the status progress
        /// The parameter is an int between 0 and 100
        /// </summary>
        public event System.EventHandler<AssetStatus> StatusProgress;

        #endregion
        public void Dispose()
        {
            manifestBuffer = null;
            if(VideoChunkListList!= null)
            {
                foreach( var c in VideoChunkListList)
                {
                    c.Dispose();                    
                }
            }
            VideoChunkListList.Clear();

            if (AudioChunkListList != null)
            {
                foreach (var c in AudioChunkListList)
                {
                    c.Dispose();
                }
            }
            AudioChunkListList.Clear();

            if (TextChunkListList != null)
            {
                foreach (var c in TextChunkListList)
                {
                    c.Dispose();
                }
            }
            TextChunkListList.Clear();
        }

    }
}
