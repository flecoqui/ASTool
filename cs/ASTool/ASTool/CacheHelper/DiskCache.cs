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
using System.IO;
using System.IO.IsolatedStorage;
using ASTool.ISMHelper;

namespace ASTool.CacheHelper
{
     class DiskCache: ManifestOutput
    {
        public DiskCache()
        {
            root = string.Empty;

        }
        /// <summary>
        /// Used for synchronization
        /// </summary>
        /// 
        private readonly AsyncReaderWriterLock internalAudioDiskLock = new AsyncReaderWriterLock();
        private readonly AsyncReaderWriterLock internalVideoDiskLock = new AsyncReaderWriterLock();
        private readonly AsyncReaderWriterLock internalTextDiskLock = new AsyncReaderWriterLock();
        private readonly AsyncReaderWriterLock internalManifestDiskLock = new AsyncReaderWriterLock();

        public bool Initialize(string Root)
        {
            root = Root;

            return CreateDirectory(Root);
        }

        private string root = "";
        private const string ISMFileName = "asset.ism";
        private const string ISMCFileName = "asset.ismc";
        private const string manifestFileName = "manifest.xml";
        private const string audioIndexFileName = "AudioIndex";
        private const string videoIndexFileName = "VideoIndex";
        private const string textIndexFileName = "TextIndex";
        private const string audioContentFileName = "Audio";
        private const string videoContentFileName = "Video";
        private const string textContentFileName = "Text";
        public const ulong indexSize = 20;

        /// <summary> Get object from isolated storage file.</summary>
        /// <param name="fullpath"> file name to retreive</param>
        /// <param name="type"> type of object to read</param>
        /// <returns> a <c>object</c> instance, or null if the operation failed.</returns>
        private async Task<object> GetObjectByType(string filepath, Type type)
        {
            object retVal = null;
            try
            {
                using (var releaser = await internalManifestDiskLock.ReaderLockAsync())
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Enter for Uri: " + filepath.ToString());

                    byte[] bytes =  Restore(filepath);
                    if (bytes != null)
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                if (ms != null)
                                {
                                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                                    retVal = ser.ReadObject(ms);
                                }
                            }
                        }
                        catch(Exception e)
                        {

                            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Exception for Uri: " + filepath.ToString() + " Exception: " + e.Message);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Reader Exit for Uri: " + filepath.ToString());

                }
            }
            catch(Exception)
            {

            }
            return retVal;
        }


        /// <summary>
        /// Return a ManifestCache based on its Uri
        /// </summary>
        public async Task<ManifestManager> RestoreAsset(Uri uri)
        {
            List<string> dirs =  GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {

                            ManifestManager de = await GetObjectByType(file, typeof(ManifestManager)) as ManifestManager;
                            if (de != null)
                            {
                                if (de.ManifestUri == uri)
                                {
                                    return de;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// SaveManifest
        /// Save manifest on disk 
        /// </summary>
        public async Task<bool> ProcessManifest(ManifestManager cache)
        {
            await SaveISM(cache);
            await SaveISMC(cache);
            return await SaveManifest(cache);
        }
        /// <summary>
        /// SaveManifest
        /// Save manifest on disk 
        /// </summary>
        public async Task<bool> SaveManifest(ManifestManager cache)
        {
            bool bResult = false;
            if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                CreateDirectory(Path.Combine(root, cache.StoragePath));

            using (var releaser = await internalManifestDiskLock.WriterLockAsync())
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Enter for Uri: " + cache.ManifestUri.ToString());
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (ms != null)
                        {
                            System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(typeof(ManifestManager));
                            ser.WriteObject(ms, cache);
                            bResult = Save(Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName), ms.ToArray());
                        }
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer exception for Uri: " + cache.ManifestUri.ToString() + " Exception: " + e.Message);

                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Exit for Uri: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// Save ISM File
        /// Save manifest on disk 
        /// </summary>
        public async Task<bool> SaveISM(ManifestManager cache)
        {
            bool bResult = false;
            if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                CreateDirectory(Path.Combine(root, cache.StoragePath));

            using (var releaser = await internalManifestDiskLock.WriterLockAsync())
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Enter for Uri: " + cache.ManifestUri.ToString());
                try
                {                   
                    string header = "<?xml version=\"1.0\" encoding=\"utf-16\"?><smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\"><head><meta name=\"clientManifestRelativePath\" content=\"asset.ismc\"/></head><body><switch>";
                    string footer = "</switch></body></smil>";
                    string videoMask = "<video src=\"{0}\" systemBitrate=\"{1}\"><param name=\"trackID\" value=\"{2}\" valuetype=\"data\" /><param name=\"trackName\" value=\"{3}\" valuetype=\"data\"/><param name=\"timescale\" value=\"{4}\" valuetype=\"data\"/></video>";
                    string audioMask = "<audio src=\"{0}\" systemBitrate=\"{1}\" systemLanguage=\"{2}\"><param name=\"trackID\" value=\"{3}\" valuetype=\"data\" /><param name=\"trackName\" value=\"{4}\" valuetype=\"data\"/><param name=\"timescale\" value=\"{5}\" valuetype=\"data\"/></audio>";
                    string textMask = "<textstream src=\"{0}\" systemBitrate=\"{1}\" systemLanguage=\"{2}\"><param name=\"trackID\" value=\"{3}\" valuetype=\"data\" /><param name=\"trackName\" value=\"{4}\" valuetype=\"data\"/><param name=\"timescale\" value=\"{5}\" valuetype=\"data\"/></textstream>";

                    string content = header;
                    int Track = 0;
                    foreach (var cl in cache.VideoChunkListList)
                    {
                        string FileName = videoContentFileName + "_" + Track.ToString() + ".ismv";
                        VideoChunkListConfiguration ac = cl.Configuration as VideoChunkListConfiguration;
                        if (ac != null)
                        {
                            
                            content += string.Format(videoMask, FileName,
                                                ac.Bitrate.ToString(),
                                                ac.TrackID.ToString(),
                                                //cl.Configuration.Language.ToString(),
                                                ( string.IsNullOrEmpty(ac.TrackName) ?"video" :ac.TrackName.ToString()),
                                                ac.TimeScale.ToString());
                            Track++;
                        }
                    }
                    Track = 0;
                    foreach (var cl in cache.AudioChunkListList)
                    {
                        string FileName = audioContentFileName + "_" + Track.ToString() + ".isma";
                        AudioChunkListConfiguration ac = cl.Configuration as AudioChunkListConfiguration;
                        if (ac != null)
                        {
                            content += string.Format(audioMask, FileName,
                                                ac.Bitrate.ToString(),
                                                (!string.IsNullOrEmpty(ac.Language)? ac.Language.ToString(): "unk"),
                                                ac.TrackID.ToString(),
                                                (string.IsNullOrEmpty(ac.TrackName) ? "audio"  : ac.TrackName.ToString()),
                                                ac.TimeScale.ToString());
                            Track++;

                        }
                    }
                    Track = 0;
                    foreach (var cl in cache.TextChunkListList)
                    {
                        string FileName = textContentFileName + "_" + Track.ToString() + ".ismt";
                        AudioChunkListConfiguration ac = cl.Configuration as AudioChunkListConfiguration;
                        if (ac != null)
                        {
                            content += string.Format(textMask, FileName,
                                                ac.Bitrate.ToString(),
                                                (!string.IsNullOrEmpty(ac.Language) ? ac.Language.ToString() : "unk"),
                                                ac.TrackID.ToString(),
                                                (string.IsNullOrEmpty(ac.TrackName) ? "text"  : ac.TrackName.ToString()),
                                                ac.TimeScale.ToString());
                            Track++;

                        }
                    }


                    content += footer;

                    byte[] dump = { 0xFF, 0xFE };
                    bResult = Save(Path.Combine(Path.Combine(root, cache.StoragePath), ISMFileName), dump);
                    Append(Path.Combine(Path.Combine(root, cache.StoragePath), ISMFileName), System.Text.Encoding.Unicode.GetBytes(content));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer exception for Uri: " + cache.ManifestUri.ToString() + " Exception: " + e.Message);

                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Exit for Uri: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// Save ISMC File
        /// Save manifest on disk 
        /// </summary>
        public async Task<bool> SaveISMC(ManifestManager cache)
        {
            bool bResult = false;
            if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                CreateDirectory(Path.Combine(root, cache.StoragePath));

            using (var releaser = await internalManifestDiskLock.WriterLockAsync())
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Enter for Uri: " + cache.ManifestUri.ToString());
                try
                {
                    string headerMask = "<?xml version=\"1.0\" encoding=\"utf-16\"?><SmoothStreamingMedia MajorVersion=\"2\" MinorVersion=\"0\" TimeScale=\"{0}\" Duration=\"{1}\">";
                    string footer = "</SmoothStreamingMedia>";
                    string videoStreamIndexMask = "<StreamIndex Type=\"video\" TimeScale=\"{0}\" Name=\"{1}\" Chunks=\"{2}\" QualityLevels=\"{3}\" Url=\"{4}\" MaxWidth=\"{5}\" MaxHeight=\"{6}\" DisplayWidth=\"{7}\" DisplayHeight=\"{8}\" >";
                    string videoQualityLevelMask = "<QualityLevel Index=\"{0}\" Bitrate=\"{1}\" NominalBitrate=\"{2}\" BufferTime=\"1000\" FourCC=\"{3}\" MaxWidth=\"{4}\" MaxHeight=\"{5}\" CodecPrivateData=\"{6}\" NALUnitLengthField=\"4\"/>";
                    string audioStreamIndexMask = "<StreamIndex Type=\"audio\" TimeScale=\"{0}\" Language=\"{1}\" Name=\"{2}\" Chunks=\"{3}\" QualityLevels=\"1\" Url=\"{4}\">";
                    string audioQualityLevelMask = "<QualityLevel Index=\"0\" Bitrate=\"{0}\" SamplingRate=\"{1}\" Channels=\"{2}\" BitsPerSample=\"{3}\" PacketSize=\"{4}\"  AudioTag=\"{5}\" FourCC=\"{6}\" CodecPrivateData=\"{7}\"/>";
                    string textStreamIndexMask = "<StreamIndex Type=\"text\" TimeScale=\"{0}\" Subtype=\"SUBT\" Language=\"{1}\" Name=\"{2}\" Url=\"{3}\" Chunks=\"{4}\">";
                    string textQualityLevelMask = "<QualityLevel Index=\"0\" Bitrate=\"{0}\" FourCC=\"{1}\"/>";
                    //string chunkBeginMask = "<c t=\"{0}\" d=\"{1}\"/>";
//                    string chunkBeginMask = "<c n=\"{0}\" d=\"{1}\"/>";
                    string chunkBeginMask = "<c t=\"0\" d=\"{0}\"/>";
                    string chunkMask = "<c d=\"{0}\"/>";
                    string streamIndexEnd = "</StreamIndex>";

                    string content = string.Format(headerMask, cache.TimeScale.ToString(),(cache.GetMinChunkDurationMs()*10000).ToString());
                    int Track = 0;
                    ulong TotalChunks = 0;
                    string url = string.Empty;
                    int MaxWidth = 0;
                    int MaxHeight = 0;
                    string Name = string.Empty;
                    // video section
                    
                    foreach (var cl in cache.VideoChunkListList)
                    {
                        if ((TotalChunks == 0) || (cl.TotalChunks < TotalChunks))
                            TotalChunks = cl.TotalChunks;
                        url = cl.OriginalTemplateUrl;
                        VideoChunkListConfiguration ac = cl.Configuration as VideoChunkListConfiguration;
                        if(ac != null)
                        {
                            Name = (string.IsNullOrEmpty(ac.TrackName)?"video": ac.TrackName.ToString());
                            
                            if (ac.Width > MaxWidth)
                                MaxWidth = ac.Width;
                            if (ac.Height > MaxHeight)
                                MaxHeight = ac.Height;

                        }
                    }
                    string chunksContent = string.Empty;
                    ulong offset = 0;
                    ulong size = 20;
                    string IndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName) + "_0";
                    ulong fileSize = GetFileSize(IndexFile);
                    int Index = 0;
                    ulong lastTime = 0;
                    while (offset < fileSize)
                    {
                        byte[] b = Restore(IndexFile, offset, size);
                        IndexCache ic = new IndexCache(b);
                        if (ic != null)
                        {
                            if (Index == 0)
                                lastTime = ic.Time;
                            else
                            {
                                ulong duration = ic.Time - lastTime;
                                if (Index == 1)
                                    chunksContent += string.Format(chunkBeginMask,
                                //                    lastTime.ToString(),
                                duration.ToString());
                                else
                                    chunksContent += string.Format(chunkMask,
                                                   // (Index - 1).ToString(),
                                                    duration.ToString());
                                lastTime = ic.Time;

                            }
                            Index++;
                        }
                        offset += size;
                    }

                    content += string.Format(videoStreamIndexMask,
                        cache.TimeScale.ToString(),
                        Name,
                        (Index-1).ToString(), 
                        cache.VideoChunkListList.Count.ToString(), 
                        url, 
                        MaxWidth, 
                        MaxHeight, 
                        MaxWidth, 
                        MaxHeight);
                    foreach (var cl in cache.VideoChunkListList)
                    {
                        VideoChunkListConfiguration ac = cl.Configuration as VideoChunkListConfiguration;
                        if (ac != null)
                        {
                            content += string.Format(videoQualityLevelMask,
                                        Track.ToString(),
                                        ac.Bitrate.ToString(),
                                        ac.Bitrate.ToString(),
                                        ac.FourCC.ToString(),
                                        ac.Width.ToString(),
                                        ac.Height.ToString(),
                                        ac.CodecPrivateData.ToString());
                            Track++;
                        }
                    }
                    content += chunksContent;
                    content += streamIndexEnd;

                    // Audio section
                    int AudioTrack = 0;
                    string Language = string.Empty;
                    foreach (var cl in cache.AudioChunkListList)
                    {
                        if ((TotalChunks == 0) || (cl.TotalChunks < TotalChunks))
                            TotalChunks = cl.TotalChunks;
                        url = cl.OriginalTemplateUrl;
                        AudioChunkListConfiguration ac = cl.Configuration as AudioChunkListConfiguration;
                        if (ac != null)
                        {
                            offset = 0;
                            size = 20;
                            IndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName) + "_" + AudioTrack.ToString();
                            fileSize = GetFileSize(IndexFile);
                            Index = 0;
                            lastTime = 0;
                            chunksContent = string.Empty;
                            while (offset < fileSize)
                            {
                                byte[] b = Restore(IndexFile, offset, size);
                                IndexCache ic = new IndexCache(b);
                                if (ic != null)
                                {
                                    if (Index == 0)
                                        lastTime = ic.Time;
                                    else
                                    {
                                        ulong duration = ic.Time - lastTime;
                                        if (Index == 1)
                                            chunksContent += string.Format(chunkBeginMask,
                                        //                    lastTime.ToString(),
                                        duration.ToString());
                                        else
                                            chunksContent += string.Format(chunkMask,
                                                            // (Index - 1).ToString(),
                                                            duration.ToString());
                                        lastTime = ic.Time;
                                    }
                                    Index++;
                                }
                                offset += size;
                            }

                            content += string.Format(audioStreamIndexMask,
                                cache.TimeScale.ToString(),
                                ac.Language,
                                (string.IsNullOrEmpty(ac.TrackName) ? "audio" : ac.TrackName.ToString()),
                                (Index - 1).ToString(),
                                url);
                            content += string.Format(audioQualityLevelMask,
                                ac.Bitrate.ToString(),
                                ac.SamplingRate.ToString(),
                                ac.Channels.ToString(),
                                ac.BitsPerSample.ToString(),
                                ac.PacketSize.ToString(),
                                ac.AudioTag.ToString(),
                                ac.FourCC.ToString(),
                                ac.CodecPrivateData.ToString());

                            content += chunksContent;
                            content += streamIndexEnd;
                            AudioTrack++;
                        }
                    }

                    // Text section
                    int TextTrack = 0;
                    Language = string.Empty;
                    foreach (var cl in cache.TextChunkListList)
                    {
                        if ((TotalChunks == 0) || (cl.TotalChunks < TotalChunks))
                            TotalChunks = cl.TotalChunks;
                        url = cl.OriginalTemplateUrl;
                        TextChunkListConfiguration ac = cl.Configuration as TextChunkListConfiguration;
                        if (ac != null)
                        {
                            offset = 0;
                            size = 20;
                            IndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), textIndexFileName) + "_" + TextTrack.ToString();
                            fileSize = GetFileSize(IndexFile);
                            Index = 0;
                            lastTime = 0;
                            chunksContent = string.Empty;
                            while (offset < fileSize)
                            {
                                byte[] b = Restore(IndexFile, offset, size);
                                IndexCache ic = new IndexCache(b);
                                if (ic != null)
                                {
                                    if (Index == 0)
                                        lastTime = ic.Time;
                                    else
                                    {
                                        ulong duration = ic.Time - lastTime;
                                        if (Index == 1)
                                            chunksContent += string.Format(chunkBeginMask,
                                        //                    lastTime.ToString(),
                                        duration.ToString());
                                        else
                                            chunksContent += string.Format(chunkMask,
                                                            // (Index - 1).ToString(),
                                                            duration.ToString());
                                        lastTime = ic.Time;
                                    }
                                    Index++;
                                }
                                offset += size;
                            }

                            content += string.Format(textStreamIndexMask,
                                cache.TimeScale.ToString(),
                                ac.Language,
                                (string.IsNullOrEmpty(ac.TrackName) ? "text" : ac.TrackName.ToString()),
                                url,
                                (Index - 1).ToString());
                            content += string.Format(textQualityLevelMask,
                                ac.Bitrate.ToString(),
                                ac.FourCC.ToString());

                            content += chunksContent;
                            content += streamIndexEnd;
                            TextTrack++;
                        }
                    }



                    content += footer;
                    byte[] dump = { 0xFF, 0xFE };
                    bResult = Save(Path.Combine(Path.Combine(root, cache.StoragePath), ISMCFileName), dump);
                    Append(Path.Combine(Path.Combine(root, cache.StoragePath), ISMCFileName), System.Text.Encoding.Unicode.GetBytes(content));
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer exception for Uri: " + cache.ManifestUri.ToString() + " Exception: " + e.Message);

                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " internalManifestDiskLock Writer Exit for Uri: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// SaveAudioChunks
        /// Save audio chunks on disk 
        /// </summary>
        public async Task<bool> SaveAudioChunks(ManifestManager cache)
        {
            bool bResult = true;
            // Saving Audio and Video chunks 
            string AudioIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
            string AudioContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
            if ((!string.IsNullOrEmpty(AudioIndexFile)) &&
                    (!string.IsNullOrEmpty(AudioContentFile)))
            {
                if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                    CreateDirectory(Path.Combine(root, cache.StoragePath));

                using (var releaser = await internalAudioDiskLock.WriterLockAsync())
                {
                    // delete the initial files
                    /*
                    await DeleteFile(AudioIndexFile);
                    await DeleteFile(AudioContentFile);
                    cache.AudioSavedChunks = 0;
                    */
                    int AudioTrack = 0;
                    foreach (var cl in cache.AudioChunkListList)
                    {
                        string FilePath = AudioContentFile + "_" + AudioTrack.ToString() + ".isma";
                        ulong InitialAudioOffset = 0;
                        ulong AudioOffset = GetFileSize(FilePath);
                        if (AudioOffset == 0)
                        {
                            if (cl.ftypData != null)
                            {
                                Append(FilePath, cl.ftypData);
                                Append(FilePath, cl.moovData);
                                AudioOffset = GetFileSize(FilePath);
                            }
                        }
                        else
                        {
                            InitialAudioOffset = AudioOffset;
                            ChunkBuffer cc;
                            while (cl.ChunksQueue.TryDequeue(out cc) == true)
                            //                            for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
                            //    foreach ( var cc in cl.ChunksQueue)
                            {
                                //var cc = cl.ChunksQueue.ElementAt(Index);

                                if ((cc != null) && (cc.GetLength() > 0))
                                {
                                    cc.chunkBuffer = RemoveLiveTimestampAndUpdateSequenceNumber(cc.chunkBuffer, cl.OutputChunks);
                                    IndexCache ic = new IndexCache(cc.Time, AudioOffset, cc.GetLength());
                                    if (ic != null)
                                    {
                                        ulong res = Append(FilePath, cc.chunkBuffer);
                                        if (res == cc.GetLength())
                                        {
                                            AudioOffset += res;
                                            ulong result = Append(AudioIndexFile + "_" + AudioTrack.ToString(), ic.GetByteData());
                                            if (result == indexSize)
                                            {

                                                // Free buffer
                                                cc.chunkBuffer = null;
                                                cl.OutputBytes += res;
                                                cl.OutputChunks++;
                                            }
                                            else
                                            {
                                                bResult = false;
                                                System.Diagnostics.Debug.WriteLine("Error while archiving audio");
                                            }
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("Error while archiving audio");
                                    }
                                }
                                else
                                    break;
                            }
                        }

                        if (cl.mfraData != null)
                        {
                            Append(FilePath, cl.mfraData);
                            AudioOffset = GetFileSize(FilePath);
                            cl.mfraData = null;
                        }
                        if (InitialAudioOffset < AudioOffset)
                            bResult = true;
                        AudioTrack++;
                    }
                    if (cache.GetOutputAudioSize() == cache.GetInputAudioSize())
                        bResult = true;
                }
            }

            return bResult;
        }
        /// <summary>
        /// SaveTextChunks
        /// Save text chunks on disk 
        /// </summary>
        public async Task<bool> SaveTextChunks(ManifestManager cache)
        {
            bool bResult = true;
            // Saving Audio and Video chunks 
            string TextIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), textIndexFileName);
            string TextContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), textContentFileName);
            if ((!string.IsNullOrEmpty(TextIndexFile)) &&
                    (!string.IsNullOrEmpty(TextContentFile)))
            {
                if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                    CreateDirectory(Path.Combine(root, cache.StoragePath));

                using (var releaser = await internalTextDiskLock.WriterLockAsync())
                {
                    // delete the initial files
                    /*
                    await DeleteFile(AudioIndexFile);
                    await DeleteFile(AudioContentFile);
                    cache.AudioSavedChunks = 0;
                    */
                    int TextTrack = 0;
                    foreach (var cl in cache.TextChunkListList)
                    {
                        string FilePath = TextContentFile + "_" + TextTrack.ToString() + ".ismt";
                        ulong InitialTextOffset = 0;
                        ulong TextOffset = GetFileSize(FilePath);
                        if (TextOffset == 0)
                        {
                            if (cl.ftypData != null)
                            { 
                                Append(FilePath, cl.ftypData);
                                Append(FilePath, cl.moovData);
                                TextOffset = GetFileSize(FilePath);
                            }
                        }
                        else
                        {
                            InitialTextOffset = TextOffset;
                            ChunkBuffer cc;
                            while (cl.ChunksQueue.TryDequeue(out cc) == true)

                            //foreach ( var cc in cl.ChunksQueue)
                            //for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
                            {
                              //  var cc = cl.ChunksList.Values.ElementAt(Index);
                                if ((cc != null) && (cc.GetLength() > 0))
                                {
                                    cc.chunkBuffer = RemoveLiveTimestampAndUpdateSequenceNumber(cc.chunkBuffer, cl.OutputChunks);
                                    IndexCache ic = new IndexCache(cc.Time, TextOffset, cc.GetLength());
                                    if (ic != null)
                                    {
                                        ulong res = Append(FilePath, cc.chunkBuffer);
                                        if (res == cc.GetLength())
                                        {
                                            TextOffset += res;
                                            ulong result = Append(TextIndexFile + "_" + TextTrack.ToString(), ic.GetByteData());
                                            if (result == indexSize)
                                            {

                                                // Free buffer
                                                cc.chunkBuffer = null;
                                                cl.OutputBytes += res;
                                                cl.OutputChunks++;
                                            }
                                            else
                                            {
                                                bResult = false;
                                                System.Diagnostics.Debug.WriteLine("Error while archiving video");
                                            }
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("Error while archiving video");
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        if (InitialTextOffset < TextOffset)
                            bResult = true;
                        if (cl.mfraData != null)
                        {
                            Append(FilePath, cl.mfraData);
                            TextOffset = GetFileSize(FilePath);
                            cl.mfraData = null;
                        }

                        TextTrack++;
                    }
                    if (cache.GetOutputTextSize() == cache.GetInputTextSize())
                        bResult = true;
                }
            }

            return bResult;
        }
        byte[] RemoveLiveTimestampAndUpdateSequenceNumber(byte[] buffer,ulong sequenceNumber)
        {
            byte[] result = null;
            if ((buffer != null) && (buffer.Length > 0))
            {
                int offset = 0;
                while (offset < buffer.Length)
                {
                    Mp4Box box = Mp4Box.ReadMp4BoxFromBuffer(buffer, offset);
                    if (box != null)
                    {
                        if (box.GetBoxType() == "moof")
                        {
                            // Remove uuid box A239...8DF4 which contains the IV (initialisation vectors for the encryption)
                            // Keep the list of IV (initialisation vectors for the encryption) included in this box
                            // Keep the list of sample size
                            // Calculate the new lenght and keep the difference with the previous lenght
                            // Open the next box (mdat) and decrypt sample by sample 
                            Mp4BoxMOOF moof = box as Mp4BoxMOOF;
                            if (moof != null)
                            {
                                int currentLength = moof.GetBoxLength();
                                int currentTrackID = moof.GetTrackID();
                                bool bRemoved = false;
                                
                                Mp4BoxUUID uuidextbox = moof.GetUUIDBox(Mp4Box.kTrackFragExtHeaderBoxGuid) as Mp4BoxUUID;
                                if (uuidextbox != null)
                                {
                                    moof.RemoveUUIDBox(Mp4Box.kTrackFragExtHeaderBoxGuid);
                                    bRemoved = true;
                                }
                                Mp4BoxUUID uuidbox = moof.GetUUIDBox(Mp4Box.kTrackFragHeaderBoxGuid) as Mp4BoxUUID;
                                if (uuidbox != null)
                                {
                                    moof.RemoveUUIDBox(Mp4Box.kTrackFragHeaderBoxGuid);
                                    bRemoved = true;
                                }
                                Int32 decreasedSize = 0;
                                if (bRemoved == true)
                                {
                                    moof.UpdateMFHDSequence((int)sequenceNumber);
                                    Mp4BoxTRUN trunbox = moof.GetChild("trun") as Mp4BoxTRUN;
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
        /// SaveVideoChunks
        /// Save video chunks on disk 
        /// </summary>
        public async Task<bool> SaveVideoChunks(ManifestManager cache)
        {
            bool bResult = false;
            string VideoIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
            string VideoContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
            if ((!string.IsNullOrEmpty(VideoIndexFile)) &&
                    (!string.IsNullOrEmpty(VideoContentFile)))
            {
                if (!DirectoryExists(Path.Combine(root, cache.StoragePath)))
                    CreateDirectory(Path.Combine(root, cache.StoragePath));

                using (var releaser = await internalVideoDiskLock.WriterLockAsync())
                {

                    // delete the initial files
                    /*
                    await DeleteFile(VideoIndexFile);
                    await DeleteFile(VideoContentFile);
                    cache.VideoSavedChunks = 0;
                    */
                    int VideoTrack = 0;
                    foreach (var cl in cache.VideoChunkListList)
                    {
                        string FilePath = VideoContentFile + "_" + VideoTrack.ToString() + ".ismv";
                        ulong InitialVideoOffset = 0;
                        ulong VideoOffset = GetFileSize(FilePath);

                        if (VideoOffset == 0)
                        {
                            if (cl.ftypData != null)
                            { 
                                Append(FilePath, cl.ftypData);
                                Append(FilePath, cl.moovData);
                                VideoOffset = GetFileSize(FilePath);
                            }
                        }
                        else
                        {
                            InitialVideoOffset = VideoOffset;
                            ChunkBuffer cc;
                            while (cl.ChunksQueue.TryDequeue(out cc) == true)
                            //    foreach (var cc in cl.ChunksQueue)
                            //for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
                            {
                              //  var cc = cl.ChunksList.Values.ElementAt(Index);
                                if ((cc != null) && (cc.GetLength() > 0))
                                {
                                   cc.chunkBuffer = RemoveLiveTimestampAndUpdateSequenceNumber(cc.chunkBuffer, cl.OutputChunks);
                                    IndexCache ic = new IndexCache(cc.Time, VideoOffset, cc.GetLength());
                                    if (ic != null)
                                    {
                                        ulong res = Append(FilePath, cc.chunkBuffer);
                                        if (res == cc.GetLength())
                                        {
                                            VideoOffset += res;
                                            ulong result = Append(VideoIndexFile + "_" + VideoTrack.ToString(), ic.GetByteData());
                                            if (result == indexSize)
                                            {                                       
                                                // free buffer
                                                cc.chunkBuffer = null;
                                                cl.OutputBytes += res;
                                                cl.OutputChunks++;
                                            }
                                            else
                                            {
                                                bResult = false;
                                                System.Diagnostics.Debug.WriteLine("Error while archiving video");
                                            }
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("Error while archiving video");
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        VideoTrack++;
                        if (cl.mfraData != null)
                        {
                            Append(FilePath, cl.mfraData);
                            VideoOffset = GetFileSize(FilePath);
                            cl.mfraData = null;
                        }
                        if (InitialVideoOffset < VideoOffset)
                            bResult = true;
                    }
                    if (cache.GetOutputVideoSize() == cache.GetInputVideoSize())
                        bResult = true;
                }
            }

            return bResult;
        }
        /// <summary>
        /// RemoveAudioChunks
        /// Remove audio chunks from disk 
        /// </summary>
        public async Task<bool> RemoveAudioChunks(ManifestManager cache)
        {
            bool bResult = false;
            string pathContent = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
            string pathIndex = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
            using (var releaser = await internalAudioDiskLock.WriterLockAsync())
            {
                for (int i = 0; i < 10; i++)
                {
                    if (pathContent != null)
                    {
                        bool res = FileExists(pathContent + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathContent + "_" + i.ToString());
                    }
                    if (pathIndex != null)
                    {
                        bool res = FileExists(pathIndex + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathIndex + "_" + i.ToString());
                    }
                }
            }
            return bResult;
        }
        /// <summary>
        /// RemoveTextChunks
        /// Remove text chunks from disk 
        /// </summary>
        public async Task<bool> RemoveTextChunks(ManifestManager cache)
        {
            bool bResult = false;
            string pathContent = Path.Combine(Path.Combine(root, cache.StoragePath), textContentFileName);
            string pathIndex = Path.Combine(Path.Combine(root, cache.StoragePath), textIndexFileName);
            using (var releaser = await internalTextDiskLock.WriterLockAsync())
            {
                for (int i = 0; i < 10; i++)
                {
                    if (pathContent != null)
                    {
                        bool res = FileExists(pathContent + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathContent + "_" + i.ToString());
                    }
                    if (pathIndex != null)
                    {
                        bool res = FileExists(pathIndex + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathIndex + "_" + i.ToString());
                    }
                }
            }
            return bResult;
        }
        /// <summary>
        /// RemoveVideoChunks
        /// Remove video chunks from disk 
        /// </summary>
        public async Task<bool> RemoveVideoChunks(ManifestManager cache)
        {
            bool bResult = false;
            string pathContent = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
            string pathIndex = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
            using (var releaser = await internalVideoDiskLock.WriterLockAsync())
            {
                for (int i = 0; i < 10; i++)
                {
                    if (pathContent != null)
                    {
                        bool res = FileExists(pathContent + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathContent + "_" + i.ToString());
                    }
                    if (pathIndex != null)
                    {
                        bool res = FileExists(pathIndex + "_" + i.ToString());
                        if (res)
                            bResult = DeleteFile(pathIndex + "_" + i.ToString());
                    }
                }
            }
            return bResult;
        }
        /// <summary>
        /// RemoveManifest
        /// Remove manifest from disk 
        /// </summary>
        public bool RemoveManifest(ManifestManager cache)
        {
            bool bResult = false;
            string path = Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName);
            if (path != null)
            {
                bool res = FileExists(path);
                if (res)
                    bResult = DeleteFile(path);
            }
            return bResult;
        }
        /// <summary>
        /// GetCacheSize
        /// Return the current size of the cache on disk: adding the size of each asset 
        /// </summary>
        public async Task<ulong> GetCacheSize()
        {
            ulong Val = 0;
            List<string> dirs =  GetDirectoryNames(root);
            if (dirs != null)
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    string path = Path.Combine(root, dirs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string file = Path.Combine(path, manifestFileName);
                        if (!string.IsNullOrEmpty(file))
                        {
                            ManifestManager de = await GetObjectByType(file, typeof(ManifestManager)) as ManifestManager;
                            if (de != null)
                            {
                                Val += await GetAssetSize(de);
                            }
                        }
                    }
                }
            }
            return Val;

        }
        /// <summary>
        /// GetAssetSize
        /// Return the current asset size on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<ulong> GetAssetSize(ManifestManager cache)
        {
            ulong val = 0;
            string path = string.Empty;
            path = Path.Combine(Path.Combine(root, cache.StoragePath), manifestFileName);
            if (!string.IsNullOrEmpty(path))
                val +=  GetFileSize(path);

            using (var releaser = await internalVideoDiskLock.ReaderLockAsync())
            {
                path = Path.Combine(Path.Combine(root, cache.StoragePath), videoIndexFileName);
                if (!string.IsNullOrEmpty(path))
                    val += GetFileSize(path);
                path = Path.Combine(Path.Combine(root, cache.StoragePath), videoContentFileName);
                if (!string.IsNullOrEmpty(path))
                    val +=  GetFileSize(path);
            }

            using (var releaser = await internalAudioDiskLock.ReaderLockAsync())
            {
                path = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
                if (!string.IsNullOrEmpty(path))
                    val +=  GetFileSize(path);
                path = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
                if (!string.IsNullOrEmpty(path))
                    val += GetFileSize(path);
            }
            return val;
        }
        /// <summary>
        /// RemoveAsset
        /// Remove the asset on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<bool> RemoveAsset(ManifestManager cache)
        {

            bool bResult = false;
            try
            {
                await RemoveAudioChunks(cache);
                await RemoveVideoChunks(cache);
                await RemoveTextChunks(cache);
                RemoveManifest(cache);
                bResult = RemoveDirectory(Path.Combine(root, cache.StoragePath));
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while removing Asset: " + ex.Message);
            }
            return bResult;
        }
        /// <summary>
        /// SaveAsset
        /// Save the asset on disk: audio chunks, video chunks and manifest 
        /// </summary>
        public async Task<bool> ProcessChunks(ManifestManager cache)
        {
            bool bResult = true;
            if (!(await SaveAudioChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving audio chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveVideoChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving video chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveTextChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving text chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveManifest(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving manifest chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveISM(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving ism file for url: " + cache.ManifestUri.ToString());
            }
            if (!(await SaveISMC(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while saving ismc file for url: " + cache.ManifestUri.ToString());
            }
            return bResult;
        }
        /// <summary>
        /// GetChunkBuffer
        /// Return the chunk buffer from the disk  
        /// </summary>
        private async Task<byte[]> GetChunkBuffer(bool isVideo, string path, ulong time)
        {
            byte[] buffer = null;
            string dir = Path.Combine(root, path);
            if (!string.IsNullOrEmpty(dir))
            {
                string indexFile = Path.Combine(dir, (isVideo == true ? videoIndexFileName : audioIndexFileName));
                string contentFile = Path.Combine(dir, (isVideo == true ? videoContentFileName : audioContentFileName));
                if ((!string.IsNullOrEmpty(contentFile))&&
                    (!string.IsNullOrEmpty(indexFile)))
                {

                    using (var releaser = (isVideo == true ? await internalVideoDiskLock.ReaderLockAsync(): await internalAudioDiskLock.ReaderLockAsync()))
                    {
                        ulong offset = 0;
                        ulong size = 20;
                        ulong fileSize = GetFileSize(indexFile);
                        while (offset < fileSize)
                        {
                            byte[] b = Restore(indexFile, offset, size);
                            IndexCache ic = new IndexCache(b);
                            if (ic != null)
                            {
                                if (ic.Time == time)
                                {
                                    buffer = Restore(contentFile, ic.Offset, ic.Size);
                                    break;
                                }
                            }
                            offset += size;
                        }
                    }
                }
            }
            return buffer;
        }
        /// <summary>
        /// Return Audio chunk from disk with Uri and Time
        /// </summary>
        public  byte[] GetAudioChunkBuffer(string path, ulong time)
        {
            var t =  GetChunkBuffer(false, path, time);
            t.Wait();
            if(t.IsCompleted)
                return t.Result;
            return null;
        }
        /// <summary>
        /// Return Audio chunk from disk with Uri and Time
        /// </summary>
        public  byte[] GetVideoChunkBuffer(string path, ulong time)
        {
            var t = GetChunkBuffer(true, path, time);
            t.Wait();
            if(t.IsCompleted)
                return t.Result;
            return null;
        }

        //private Windows.Storage.ApplicationData storage;
        /// <summary>
        /// returns available size in bytes
        /// </summary>
        /// <returns></returns>
        public ulong GetAvailableSize()
        {

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                string r = d.RootDirectory.Name;
                if(root.StartsWith(r))
                {
                    return (ulong) d.AvailableFreeSpace;
                }

            }
            return 0;
        }

        /// <summary>
        /// returns a list of directory names for the 
        /// specified directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public List<string> GetDirectoryNames(string directoryPath)
        {
            List<string> listDirectory = new List<string>(); 
            try
            {
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(directoryPath));

                foreach (var dir in dirs)
                {
                    listDirectory.Add( dir.Substring(dir.LastIndexOf("\\") + 1));
                }

            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("GetDirectoryNames: " + e.Message);
            }
            return listDirectory;
        }


        /// <summary>
        /// returns a list of filenames for the 
        /// specified directory.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public List<string> GetFileNames(string directoryPath, string pattern)
        {
            List<string> listFile = new List<string>();


            List<string> dirs = new List<string>(Directory.EnumerateFiles(directoryPath,pattern));

            foreach (var dir in dirs)
            {
                listFile.Add(dir.Substring(dir.LastIndexOf("\\") + 1));
            }

            return listFile;
        }

        /// <summary>
        /// deletes the specified file
        /// </summary>
        /// <param name="fullPath"></param>        
        public bool DeleteFile(string fullPath)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile FileExists for path: " + fullPath);
            bool bRes = System.IO.File.Exists(fullPath);
            if (bRes != true)
                return true;
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile Delete for path: " + fullPath);

                    try
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile for path: " + fullPath + " exception 1: " + ex.Message);

                    }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile Delete done for path: " + fullPath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DeleteFile for path: " +fullPath+" exception 2: " + e.Message);
            }
            return true;
        }
        /// <summary>
        /// IsDirectoryExist
        /// </summary>
        public  bool IsDirectoryExist(string directoryPath)
        {
            bool bResult = false;
            try
            {
                bResult  = System.IO.Directory.Exists(directoryPath);
            }
            catch (Exception)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// creates the specified directory
        /// </summary>
        /// <param name="fullPath"></param>  
        public bool CreateDirectory(string directoryPath)
        {
            bool bResult = false;
            try
            {
                bool result = IsDirectoryExist(directoryPath);
                if (true != result)
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                    var folder = System.IO.Directory.CreateDirectory(directoryPath);
                    if (folder != null)
                        bResult = true;
                }
                else
                    bResult = true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("CreateDirectory - Exception: " + e.Message);
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Gets the size of the file specified in bytes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ulong GetFileSize(string filePath)
        {
            ulong len = 0;
            bool bRes = FileExists(filePath);
            if (bRes != true)
                return len;

            {
                FileInfo  i = new System.IO.FileInfo(filePath);
                if(i!=null)
                    len = (ulong)i.Length;
                
            }
            return len;
        }
        /// <summary>
        /// reads the data from the specified file and 
        /// writes them to a byte[] buffer
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public byte[] Restore(string fullPath)
        {
            return Restore(fullPath, 0, 0);
        }

        /// <summary>
        /// reads the data from the specified file with the offset as the starting point the size
        /// and writes them to a byte[] buffer
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public byte[] Restore(string fullPath, ulong offset, ulong size)
        {
            byte[] dataArray = null;

            try
            {
                FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                if(fs != null)
                {
                    long sizetoread;
                    if ((long)offset < fs.Length)
                    {
                        if ( (long) (offset + size) > fs.Length)
                            sizetoread = (long) (fs.Length - (long)offset);
                        else
                            sizetoread = (long)size;

                        dataArray = new byte[sizetoread];
                        fs.Seek((long)offset, SeekOrigin.Begin);
                        int bytesread = fs.Read(dataArray, 0, (int)sizetoread);
                        if (bytesread != sizetoread)
                        {

                            System.Diagnostics.Debug.WriteLine("Error while reading file: Restore method");
                        }
                    }
                    fs.Close();

                }

            }
            catch (Exception )
            {
            }
            return dataArray;
        }

        /// <summary>
        /// Append data to isolated storage file and return the size of the current file
        /// </summary>
        /// <param name="fullPath">String representation of the path to isolated storage file</param>
        /// <param name="data">MemoryStream containing data to append isolated storage file with</param>
        public ulong Append(string fullPath, byte[] data)
        {
            ulong retVal = 0;

            try
            {
                FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (fs != null)
                {
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    retVal = (ulong)data.Length;
                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while append in file:" + fullPath + " Exception: " + ex.Message);
            }
            return retVal;
        }

        /// <summary>
        /// Saves the provided data to the file
        /// specified
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Save(string fullPath, byte[] data)
        {

            bool retVal = false;
           // System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save DeleteFile for Path: " + fullPath);
           // await DeleteFile(fullPath);
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving for Path: " + fullPath);

                FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
                if (fs != null)
                {
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                    retVal = true;
                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving done for Path: " + fullPath);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache Save Saving exception for Path: " + fullPath + " Exception: " + e.Message);
            }

            return retVal;
        }

        /// <summary>
        /// Truncate file from isolated storage file to specific size
        /// </summary>
        /// <param name="fullPath">String representation of the path to isolated storage file</param>
        /// <param name="size">bytes of file from starting postion to be kept</param>
        public bool Truncate(string fullPath, long size)
        {
            bool bResult = false;
            if (size <= 0) return bResult;

            bool bRes = FileExists(fullPath);
            if (bRes != true)
                return bResult;

            try
            {
                FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite);
                if (fs != null)
                {
                    fs.SetLength(size);
                    fs.Close();
                }
            }
            catch(Exception )
            {

            }
            return bResult;
        }

        /// <summary>
        /// returns true, if the specified file exists otherwise false
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>

        public bool FileExists(string fullPath)
        {
            bool bResult = false;
            try
            {
                bResult = System.IO.File.Exists(fullPath);

            }
            catch (Exception)
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// removes the specified directory
        /// </summary>
        /// <param name="path"></param>
        public bool RemoveDirectory(string fullPath)
        {
            bool bRes =  DirectoryExists(fullPath);
            if (bRes == true)
            {
                System.IO.Directory.Delete(fullPath, true);
            }
            return false;
        }

        /// <summary>
        /// returns true if the specified directory exists otherwise false
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public bool DirectoryExists(string fullPath)
        {
            bool bResult = false;
            try
            {

                if (System.IO.Directory.Exists(fullPath))
                {
                    bResult = true;
                }
            }
            catch (Exception )
            {
                bResult = false;
            }
            return bResult;

        }
    }
}
