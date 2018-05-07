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

namespace ASTool.CacheHelper
{
     class DiskCache
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
        /// Return a list of all urls in local storage
        /// </summary>
        public async Task<List<ManifestCache>> RestoreAllAssets(string pattern)
        {
            List<ManifestCache> downloads = new List<ManifestCache>();
            List<string> dirs = GetDirectoryNames(root);
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
                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
                            if (de != null)
                            {
                                // Sanity check are the manifest file and chunk files consistent
                                if ((de.AudioSavedChunks == (ulong)( GetFileSize(Path.Combine(path, audioIndexFileName)) / indexSize)) &&
                                    (de.VideoSavedChunks == (ulong)( GetFileSize(Path.Combine(path, videoIndexFileName)) / indexSize)))
                                {
                                    downloads.Add(de);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " DiskCache - RestoreAllAssets - Manifest and Chunk file not consistent for path: " + path.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return downloads;
        }
        /// <summary>
        /// Return a ManifestCache based on its Uri
        /// </summary>
        public async Task<ManifestCache> RestoreAsset(Uri uri)
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

                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
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
        public async Task<bool> SaveManifest(ManifestCache cache)
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
                            System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(typeof(ManifestCache));
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
        /// SaveAudioChunks
        /// Save audio chunks on disk 
        /// </summary>
        public async Task<bool> SaveAudioChunks(ManifestCache cache)
        {
            bool bResult = false;
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
                            for (int Index = (int)cl.ArchivedChunks; Index < (int)cl.DownloadedChunks; Index++)
                            {
                                var cc = cl.ChunksList[Index];
                                if ((cc != null) && (cc.GetLength() > 0))
                                {
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
                                                cache.AudioSavedChunks++;
                                                cache.AudioSavedBytes += res;
                                                // Free buffer
                                                cc.chunkBuffer = null;
                                                cl.ArchivedBytes += res;
                                                cl.ArchivedChunks++;
                                            }
                                            else
                                                System.Diagnostics.Debug.WriteLine("Error while archiving audio");
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("Error while archiving audio");
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        if (InitialAudioOffset < AudioOffset)
                            bResult = true;
                        if (cache.AudioSavedChunks == cache.AudioDownloadedChunks)
                            bResult = true;
                        AudioTrack++;
                    }
                }
            }

            return bResult;
        }
        /// <summary>
        /// SaveTextChunks
        /// Save text chunks on disk 
        /// </summary>
        public async Task<bool> SaveTextChunks(ManifestCache cache)
        {
            bool bResult = false;
            // Saving Audio and Video chunks 
            string TextIndexFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioIndexFileName);
            string TextContentFile = Path.Combine(Path.Combine(root, cache.StoragePath), audioContentFileName);
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
                            for (int Index = (int)cl.ArchivedChunks; Index < (int)cl.DownloadedChunks; Index++)
                            {
                                var cc = cl.ChunksList[Index];
                                if ((cc != null) && (cc.GetLength() > 0))
                                {
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
                                                cache.TextSavedChunks++;
                                                cache.TextSavedBytes += res;
                                                // Free buffer
                                                cc.chunkBuffer = null;
                                                cl.ArchivedBytes += res;
                                                cl.ArchivedChunks++;
                                            }
                                            else
                                                System.Diagnostics.Debug.WriteLine("Error while archiving video");
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
                        if (cache.TextSavedChunks == cache.TextDownloadedChunks)
                            bResult = true;
                        TextTrack++;
                    }
                }
            }

            return bResult;
        }

        /// <summary>
        /// SaveVideoChunks
        /// Save video chunks on disk 
        /// </summary>
        public async Task<bool> SaveVideoChunks(ManifestCache cache)
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
                            for (int Index = (int)cl.ArchivedChunks; Index < (int)cl.DownloadedChunks; Index++)
                            {
                                var cc = cl.ChunksList[Index];
                                if ((cc != null) && (cc.GetLength() > 0))
                                {
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
                                                cache.VideoSavedChunks++;
                                                cache.VideoSavedBytes += res;
                                                // free buffer
                                                cc.chunkBuffer = null;
                                                cl.ArchivedBytes += res;
                                                cl.ArchivedChunks++;

                                            }
                                            else
                                                System.Diagnostics.Debug.WriteLine("Error while archiving text");
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("Error while archiving text");
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        VideoTrack++;
                        if (InitialVideoOffset < VideoOffset)
                            bResult = true;
                        if (cache.VideoSavedChunks == cache.VideoDownloadedChunks)
                            bResult = true;
                    }
                }
            }

            return bResult;
        }
        /// <summary>
        /// RemoveAudioChunks
        /// Remove audio chunks from disk 
        /// </summary>
        public async Task<bool> RemoveAudioChunks(ManifestCache cache)
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
        public async Task<bool> RemoveTextChunks(ManifestCache cache)
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
        public async Task<bool> RemoveVideoChunks(ManifestCache cache)
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
        public bool RemoveManifest(ManifestCache cache)
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
                            ManifestCache de = await GetObjectByType(file, typeof(ManifestCache)) as ManifestCache;
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
        public async Task<ulong> GetAssetSize(ManifestCache cache)
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
        public async Task<bool> RemoveAsset(ManifestCache cache)
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
        public async Task<bool> SaveAsset(ManifestCache cache)
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
