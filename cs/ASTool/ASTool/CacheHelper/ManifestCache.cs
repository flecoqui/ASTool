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
namespace ASTool.CacheHelper
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
    //public sealed class TextTrack
    //{
    //    public int Index { get; set; }
    //    public int Bitrate { get; set; }
    //    public string FourCC { get; set; }
    //    public string Language { get; set; }
    //}

    //public sealed class AudioTrack
    //{
    //    public int Index { get; set; }
    //    public int Bitrate { get; set; }
    //    public string FourCC { get; set; }
    //    public int BitsPerSample { get; set; }
    //    public int Channels { get; set; }
    //    public int SamplingRate { get; set; }
    //    public string CodecPrivateData { get; set; }
    //    public string Language { get; set; }
    //}
    //public sealed class VideoTrack
    //{
    //    public int Index { get; set; }
    //    public int Bitrate { get; set; }
    //    public string FourCC { get; set; }
    //    public int MaxHeight { get; set; }
    //    public int MaxWidth { get; set; }

    //    public string CodecPrivateData { get; set; }
    //    public string Language { get; set; }
    //}
    public  class Track
    {
        public int Index { get; set; }
        public int Bitrate { get; set; }
        public string FourCC { get; set; }
        public string Language { get; set; }
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
    [DataContract(Name ="ManifestCache")]
     class ManifestCache : IDisposable
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
        public ulong CurrentMediaSize { get { return AudioSavedBytes + VideoSavedBytes; } }

        /// <summary>
        /// AudioChunks 
        /// The number of audio chunks for this asset
        /// </summary>
       // [DataMember]
       // public ulong AudioChunks { get; set; }
        /// <summary>
        /// VideoChunks 
        /// The number of video chunks for this asset
        /// </summary>
       // [DataMember]
       // public ulong VideoChunks { get ; set; }
        /// <summary>
        /// AudioDownloadedChunks 
        /// The number of audio chunks downloaded
        /// </summary>
        [DataMember]
        public ulong AudioDownloadedChunks { get; set; }
        /// <summary>
        /// VideoDownloadedChunks 
        /// The number of video chunks downloaded
        /// </summary>
        [DataMember]
        public ulong VideoDownloadedChunks { get; set; }
        /// <summary>
        /// TextDownloadedChunks 
        /// The number of text chunks downloaded
        /// </summary>
        [DataMember]
        public ulong TextDownloadedChunks { get; set; }


        /// <summary>
        /// AudioSavedChunks 
        /// The number of audio chunks saved on disk
        /// </summary>
        [DataMember]
        public ulong AudioSavedChunks { get; set; }
        /// <summary>
        /// VideoSavedChunks 
        /// The number of video chunks saved on disk
        /// </summary>
        [DataMember]
        public ulong VideoSavedChunks { get; set; }
        /// <summary>
        /// TextSavedChunks 
        /// The number of text chunks saved on disk
        /// </summary>
        [DataMember]
        public ulong TextSavedChunks { get; set; }

        /// <summary>
        /// AudioDownloadedBytes 
        /// The number of audio bytes downloaded
        /// </summary>
        [DataMember]
        public ulong AudioDownloadedBytes { get; set; }
        /// <summary>
        /// VideoDownloadedBytes 
        /// The number of video bytes downloaded
        /// </summary>
        [DataMember]
        public ulong VideoDownloadedBytes { get; set; }
        /// <summary>
        /// TextDownloadedBytes 
        /// The number of text bytes downloaded
        /// </summary>
        [DataMember]
        public ulong TextDownloadedBytes { get; set; }

        /// <summary>
        /// AudioSavedBytes 
        /// The number of audio bytes stored on disk
        /// </summary>
         [DataMember]
         public ulong AudioSavedBytes { get; set; }
        /// <summary>
        /// VideoSavedBytes 
        /// The number of video bytes stored on disk
        /// </summary>
          [DataMember]
          public ulong VideoSavedBytes { get; set; }
        /// <summary>
        /// TextSavedBytes 
        /// The number of text bytes stored on disk
        /// </summary>
        [DataMember]
        public ulong TextSavedBytes { get; set; }

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
        private DiskCache DiskCache = null;
        private System.Threading.Tasks.Task downloadTask;
        private System.Threading.CancellationTokenSource downloadTaskCancellationtoken;
        private bool downloadTaskRunning = false;


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

        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache() {
            Initialize();
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache(Uri manifestUri, bool downloaddToGo, ulong minBitrate, ulong maxBitrate, int AudioIndex, ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeHash(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = downloaddToGo;
            MinBitrate = minBitrate;
            MaxBitrate = maxBitrate;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = maxError;
            DownloadMethod = downloadMethod;
        }
        /// <summary>
        /// ManifestCache 
        /// ManifestCache contructor 
        /// </summary>
        public ManifestCache(Uri manifestUri, bool downloaddToGo, int VideoIndex, int AudioIndex, ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1)
        {
            Initialize();
            ManifestUri = manifestUri;
            StoragePath = ComputeHash(ManifestUri.AbsoluteUri.ToLower());
            DownloadToGo = downloaddToGo;
            MinBitrate = 0;
            MaxBitrate = 0;
            MaxMemoryBufferSize = maxMemoryBufferSize;
            MaxError = maxError;
            DownloadMethod = downloadMethod;
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
        /// RestoreStatus
        /// Restore the manifest status based on the chunks downloaded or saved on disk
        /// </summary>
        /// <param name=""></param>
        /// <returns>true if success</returns>
        public bool RestoreStatus()
        {
            mStatus = AssetStatus.Initialized;
            if((this.VideoBitrate>0)&&
                (this.AudioBitrate > 0))
                mStatus = AssetStatus.ManifestDownloaded;
            if ((this.VideoSavedChunks > 0) ||
                (this.AudioSavedChunks > 0))
                mStatus = AssetStatus.DownloadingChunks;
            if ((IsArchiveCompleted(VideoChunkListList)) &&
                (IsArchiveCompleted(AudioChunkListList)) &&
                (IsArchiveCompleted(TextChunkListList)))
                mStatus = AssetStatus.ChunksDownloaded;            
            return true;
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
        /// <summary>
        /// DownloadManifestAsync
        /// Downloads a manifest asynchronously.
        /// </summary>
        /// <param name="forceNewDownload">Specifies whether to force a new download and avoid cached results.</param>
        /// <returns>A byte array</returns>
        public async Task<byte[]> DownloadManifestAsync(bool forceNewDownload)
        {
            Uri manifestUri = this.ManifestUri;
            System.Diagnostics.Debug.WriteLine("Download Manifest: " + manifestUri.ToString() + " start at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));

            var client = new System.Net.Http.HttpClient();
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
                    System.Diagnostics.Debug.WriteLine("Download " + val.ToString() + " Bytes Manifest: " + manifestUri.ToString() + " done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now));
                    return buffer;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
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
        /// AddChunkList
        /// Add the chunks list in the list of chunks to download.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="configuration"></param>
        /// <returns>true if success</returns>
        bool AddChunkList(StreamInfo stream, ChunkListConfiguration Configuration)
        {
            int Bitrate = 0;
            string UrlTemplate = string.Empty;
            if ((Configuration != null) && (stream != null))
            {
                Bitrate = Configuration.Bitrate;
                if (Bitrate > 0)
                {
                    if (stream.TryGetAttributeValueAsString("Url", out UrlTemplate))
                    {
                        UrlTemplate = UrlTemplate.Replace("{bitrate}", Bitrate.ToString());
                        UrlTemplate = UrlTemplate.Replace("{start time}", "{start_time}");
                        UrlTemplate = UrlTemplate.Replace("{CustomAttributes}", "timeScale=10000000");
                        if ((stream.StreamType.ToLower() == "audio") ||
                            (stream.StreamType.ToLower() == "video") ||
                            (stream.StreamType.ToLower() == "text"))
                        {
                            UInt64 time = 0;
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
                                        time = (UInt64)chunk.Time;

                                    ChunkCache cc = new ChunkCache(time, duration);
                                    time += (ulong)duration;
                                    if (cc != null)
                                    {
                                        l.ChunksList.Add(cc);
                                    }
                                }
                                l.Bitrate = Bitrate;
                                l.Chunks = (ulong)l.ChunksList.Count;
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
                                        configuration.Duration = (long) Duration;
                                        configuration.TimeScale = (int) TimeScale;
                                        configuration.Language = Lang;
                                        configuration.TrackID = -1;
                                        configuration.Width = (Int16)currentWidth;
                                        configuration.Height = (Int16)currentHeight;
                                        configuration.CodecPrivateData = currentCodecPrivateData;
                                        AddChunkList(videostream, configuration);
                                    }
                                }
                            }
                            if (stream.StreamType.ToUpper() == "AUDIO")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);

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

                                    ListAudioTracks.Add(new AudioTrack
                                    {
                                        Index = (int)audioIndex,
                                        Bitrate = (int)currentBitrate,
                                        FourCC = currentFourCC,
                                        BitsPerSample = (int) currentBitsPerSample,
                                        Channels = (int) currentChannels,
                                        SamplingRate = (int) currentSamplingRate,
                                        CodecPrivateData = currentCodecPrivateData,
                                        Language = Lang,
                                    });
                                    audioselected = track;
                                    audiostream = stream;

                                    AudioChunkListConfiguration configuration = new AudioChunkListConfiguration();
                                    configuration.Bitrate = (int)currentBitrate;
                                    configuration.Duration = (long)Duration;
                                    configuration.TimeScale = (int)TimeScale;
                                    configuration.Language = Lang;
                                    configuration.TrackID = -1;
                                    configuration.BitsPerSample = (Int16)currentBitsPerSample;
                                    configuration.Channels = (Int16)currentChannels;
                                    configuration.SamplingRate = (Int16) currentSamplingRate;
                                    configuration.CodecPrivateData = currentCodecPrivateData;
                                    configuration.MaxFramesize = 1024;                                    
                                    AddChunkList(audiostream, configuration);
                                }
                            }
                            if (stream.StreamType.ToUpper() == "TEXT")
                            {
                                string Lang = "und";
                                stream.TryGetAttributeValueAsString("Language", out Lang);

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
                                        FourCC = currentFourCC,
                                        Language = Lang,
                                    });
                                    textselected = track;
                                    textstream = stream;
                                    TextChunkListConfiguration configuration = new TextChunkListConfiguration();
                                    configuration.Bitrate = (int)currentBitrate;
                                    configuration.Duration = (long)Duration;
                                    configuration.TimeScale = (int)TimeScale;
                                    configuration.Language = Lang;
                                    configuration.TrackID = -1;
                                    AddChunkList(textstream, configuration);
                                }
                            }
                        }
                        if (this.manifestBuffer == null)
                            this.manifestBuffer = manifestBuffer;
                        bResult = true;
                    }                
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Exception while parsing Smooth Streaming manifest : " + ex.Message);
                    bResult = false;
                }
            }
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
        public bool SetDiskCache(DiskCache cache)
        {
            if (cache != null)
            {
                DiskCache = cache;
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
        public static ManifestCache CreateManifestCache(Uri manifestUri, bool downloadToGo, ulong minBitrate, ulong maxBitrate, int audioIndex, ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestCache mc = new ManifestCache(manifestUri, downloadToGo, minBitrate, maxBitrate, audioIndex, maxMemoryBufferSize, maxError, downloadMethod);
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
        public static ManifestCache CreateManifestCache(Uri manifestUri, bool downloadToGo, int videoIndex, int audioIndex, ulong maxMemoryBufferSize, uint maxError, int downloadMethod = 1)
        {
            // load the stream associated with the HLS, SMOOTH or DASH manifest
            ManifestCache mc = new ManifestCache(manifestUri, downloadToGo, videoIndex, audioIndex, maxMemoryBufferSize, maxError, downloadMethod);
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
            ulong duration = this.TimescaleToHNS(this.Duration) / (ManifestCache.TimeUnit);
            ulong expectedSize = ((this.AudioBitrate + this.VideoBitrate) * duration) / 8 ;
            return expectedSize;
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
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk start for chunk: " + chunkUri.ToString() );
            
            var client = new System.Net.Http.HttpClient();
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
                if(buffer!=null)
                {
                    int val = buffer.Length;
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk done for chunk: " + chunkUri.ToString());
                    return buffer.ToArray();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DownloadChunk exception: " + e.Message + " for chunk: " + chunkUri.ToString());
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
                ulong i = cl.DownloadedChunks;
                if (i < cl.Chunks)
                    return false;
            }
            return true ;
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
                ulong i = cl.ArchivedChunks;
                if (i < cl.Chunks)
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
                ulong i = cl.DownloadedChunks;
                for(ulong j = 0; j < i; j++)
                {
                    if( (cl.ChunksList[(int)j] != null) &&
                        (cl.ChunksList[(int)j].chunkBuffer !=null))
                    listSize += (ulong)cl.ChunksList[(int)j].chunkBuffer.Length;
                }
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

        /// <summary>
        /// DownloadCurrentChunks
        /// Download the current audio chunk .
        /// </summary>
        /// <param name=""></param>
        /// <returns>return the lenght of the downloaded chunk</returns>
        async Task<ulong> DownloadCurrentChunks(List<ChunkList> list)
        {
            ulong len = 0;
            foreach (var cl in list)
            {
                ulong i = cl.DownloadedChunks;
                if (i < cl.Chunks)
                {
                    string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList[(int)i].Time.ToString());
                    cl.ChunksList[(int)i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
                    if((cl.Configuration!=null) && (cl.Configuration.TrackID == -1))
                    {
                        cl.Configuration.TrackID = GetTrackID(cl.ChunksList[(int)i].chunkBuffer);

                        if (cl.Configuration.TrackID != -1)
                        {
                            // Now the TrackID is available, we can build ftyp and moov boxes
                            cl.ftypData = cl.Configuration.GetFTYPData();
                            cl.moovData = cl.Configuration.GetMOOVData();
                        }

                    }
                    if (cl.ChunksList[(int)i].IsChunkDownloaded())
                    {
                        ulong l = cl.ChunksList[(int)i].GetLength();
                        cl.DownloadedBytes += l;
                        len += l;
                        cl.DownloadedChunks++;
                    }
                }
            }
            return len;
        }
        Task StartDowloadParallelThread(List<ChunkList> list, ulong i)
        {
            return Task.Run(async () =>
            {
                foreach (var cl in list)
                {
                    string url = (string.IsNullOrEmpty(RedirectBaseUrl) ? BaseUrl : RedirectBaseUrl) + "/" + cl.TemplateUrl.Replace("{start_time}", cl.ChunksList[(int)i].Time.ToString());
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks for Uri: " + url.ToString());
                    cl.ChunksList[(int)i].chunkBuffer = await DownloadChunkAsync(new Uri(url));
                    if (cl.ChunksList[(int)i].IsChunkDownloaded())
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks done for Uri: " + url.ToString());
                    else
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading chunks error for Uri: " + url.ToString());
                }

            }, downloadTaskCancellationtoken.Token);

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

            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread started for Uri: " + ManifestUri.ToString());
            downloadTaskRunning = true;
            // Were we already canceled?
            if (downloadTaskCancellationtoken != null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
                downloadTaskCancellationtoken.Token.ThrowIfCancellationRequested();
            }


            DownloadThreadStartTime = DateTime.Now;
            DownloadThreadAudioCount = 0;
            DownloadThreadVideoCount = 0;

            VideoDownloadedChunks = VideoSavedChunks;
            AudioDownloadedChunks = AudioSavedChunks;
            VideoDownloadedBytes = VideoSavedBytes;
            AudioDownloadedBytes = AudioSavedBytes;

            ManifestStatus = AssetStatus.DownloadingChunks;
            int error = 0;
            while (downloadTaskRunning)
            {
                // Poll on this property if you have to do
                // other cleanup before throwing.
                if ((downloadTaskCancellationtoken != null) && (downloadTaskCancellationtoken.Token.IsCancellationRequested))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Cancellation Token throw for Uri: " + ManifestUri.ToString());
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
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks for Uri: " + ManifestUri.ToString());
                        ulong len = await DownloadCurrentChunks(VideoChunkListList);
                        if (len > 0)
                        {
                            DownloadThreadVideoCount += len;
                            result = true;
                        }
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading video chunks done for Uri: " + ManifestUri.ToString());

                    }
                }

                if (downloadTaskRunning == false)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Download Audio Chunks
                if ((AudioChunkListList != null) && (AudioChunkListList.Count > 0))
                {
                    // Something to download
                    if (!IsDownloadCompleted(AudioChunkListList))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks for Uri: " + ManifestUri.ToString());
                        ulong len = await DownloadCurrentChunks(AudioChunkListList);
                        if (len > 0)
                        {
                            DownloadThreadAudioCount += len;
                            result = true;
                        }
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading audio chunks done for Uri: " + ManifestUri.ToString());

                    }
                }
                if (downloadTaskRunning == false)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                // Download Text Chunks
                if ((TextChunkListList != null) && (TextChunkListList.Count > 0))
                {
                    // Something to download
                    if (!IsDownloadCompleted(TextChunkListList))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading text chunks for Uri: " + ManifestUri.ToString());
                        ulong len = await DownloadCurrentChunks(TextChunkListList);
                        if (len > 0)
                        {
                            DownloadThreadTextCount += len;
                            result = true;
                        }
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloading text chunks done for Uri: " + ManifestUri.ToString());

                    }
                }
                if (downloadTaskRunning == false)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread downloadTaskRunning == false for Uri: " + ManifestUri.ToString());
                    break;
                }

                if (result == false)
                {
                    error++;
                    if (error > MaxError)
                    {
                        DateTime time = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine("Download stopped too many error at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                        System.Diagnostics.Debug.WriteLine("Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                        double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                        System.Diagnostics.Debug.WriteLine("Download bitrate for the current session: " + bitrate.ToString() + " bps");
                        ManifestStatus = AssetStatus.ErrorChunksDownload;
                        downloadTaskRunning = false;
                    }
                }
                if (IsDownloadCompleted(VideoChunkListList)&&
                    IsDownloadCompleted(AudioChunkListList)&&
                    IsDownloadCompleted(TextChunkListList))
                {
                    DateTime time = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine("Download done at " + string.Format("{0:d/M/yyyy HH:mm:ss.fff}", time));
                    System.Diagnostics.Debug.WriteLine("Current Media Size: " + this.CurrentMediaSize.ToString() + " Bytes");
                    double bitrate = (this.DownloadThreadVideoCount + this.DownloadThreadAudioCount) * 8 / (time - DownloadThreadStartTime).TotalSeconds;
                    System.Diagnostics.Debug.WriteLine("Download bitrate for the current session: " + bitrate.ToString() + " bps");
                    System.Diagnostics.Debug.WriteLine("Download Thread Saving Asset");
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (DiskCache != null)
                        await DiskCache.SaveAsset(this);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());
                    ManifestStatus = AssetStatus.ChunksDownloaded;
                    downloadTaskRunning = false;
                    break;
                }
                ulong s = GetBufferSize();
                if (s > MaxMemoryBufferSize)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset for Uri: " + ManifestUri.ToString());
                    if (DiskCache != null)
                      await DiskCache.SaveAsset(this);
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread Saving asset done for Uri: " + ManifestUri.ToString());

                }

            }

            downloadTaskRunning = false;
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " downloadThread ended for Uri: " + ManifestUri.ToString());
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
            AudioDownloadedBytes = 0;
            AudioDownloadedChunks = 0;
            AudioSavedBytes = 0;
            VideoDownloadedBytes = 0;
            VideoDownloadedChunks = 0;
            VideoSavedBytes = 0;
            TextDownloadedBytes = 0;
            TextDownloadedChunks = 0;
            TextSavedBytes = 0;

            if (DiskCache!=null)
                await DiskCache.SaveManifest(this);
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
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread for Uri: " + ManifestUri.ToString());
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask downloadTaskRunning = false for Uri: " + ManifestUri.ToString());
                downloadTaskRunning = false;
                if (!downloadTask.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask Cancel token for Uri: " + ManifestUri.ToString());
                    downloadTaskCancellationtoken.Cancel();
                }
                try
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Waiting end of thread for Uri: " + ManifestUri.ToString());
                    downloadTask.Wait(500);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread Exception for Uri: " + ManifestUri.ToString() + " exception: " + e.Message);
                }

                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Stopping downloadTask thread completed for Uri: " + ManifestUri.ToString());
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
        public ChunkCache GetTextChunkCache(int Index, ulong time)
        {
            if (Index < TextChunkListList.Count)
            {
                for (int i = 0; i < TextChunkListList[Index].ChunksList.Count; i++)
                {
                    if (TextChunkListList[Index].ChunksList[i].Time == time)
                        return TextChunkListList[Index].ChunksList[i];
                }
            }
            return null;
        }
        /// <summary>
        /// GetAudioChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        public ChunkCache GetAudioChunkCache(int Index, ulong time)
        {
            if (Index < AudioChunkListList.Count)
            {
                for (int i = 0; i < AudioChunkListList[Index].ChunksList.Count; i++)
                {
                    if (AudioChunkListList[Index].ChunksList[i].Time == time)
                        return AudioChunkListList[Index].ChunksList[i];
                }
            }
            return null;
        }
        /// <summary>
        /// GetVideoChunkCache
        /// Return a ChunkCache based on time
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        /// <returns>Return a ChunkCache </returns>
        public ChunkCache GetVideoChunkCache(int Index, ulong time)
        {
            if (Index < VideoChunkListList.Count)
            {
                for (int i = 0; i < VideoChunkListList[Index].ChunksList.Count; i++)
                {
                    if (VideoChunkListList[Index].ChunksList[i].Time == time)
                        return VideoChunkListList[Index].ChunksList[i];
                }
            }
            return null;
        }

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
                    TotalVideoDownloadedChunks += l.DownloadedChunks;
                    TotalVideoChunks += l.Chunks;
                }

                ulong TotalAudioChunks = 0;
                ulong TotalAudioDownloadedChunks = 0;
                foreach (var l in AudioChunkListList)
                {
                    TotalAudioDownloadedChunks += l.DownloadedChunks;
                    TotalAudioChunks += l.Chunks;
                }
                ulong TotalTextChunks = 0;
                ulong TotalTextDownloadedChunks = 0;
                foreach (var l in TextChunkListList)
                {
                    TotalTextDownloadedChunks += l.DownloadedChunks;
                    TotalTextChunks += l.Chunks;
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
