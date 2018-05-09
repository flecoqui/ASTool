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
using System.Net.Sockets;
namespace ASTool
{
    public class SmoothPushManager
    {
        public string TrackName { get; set; }
        public int Bitrate { get; set; }
        public TcpClient NetworkClient { get; set; }
        public Stream NetworkStream { get; set; }
        public int StreamID { get; set; }
        public static string GetKeyName(string trackName, int bitrate )
        {
            return trackName + "_" + bitrate.ToString();
        }
    }
    public class SmoothLiveOutput:  ManifestOutput 
    {
        static Guid kTrackFragExtHeaderBoxGuid = new Guid("{6D1D9B05-42D5-44E6-80E2-141DAFF757B2}");
        static Guid LiveServerManBoxGuid = new Guid("{A5D40B30-E814-11DD-BA2F-0800200C9A66}");
        static string VideoLiveManifestTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">\r\n  <head>\r\n    <meta name=\"creator\" content=\"pushEncoder\" />\r\n  </head>\r\n  <body>\r\n    <switch>\r\n      <video src=\"<ismvfile>\" systemBitrate=\"<bitrate>\">\r\n        <param name=\"trackID\" value=\"<trackid>\" valuetype=\"data\" />\r\n        <param name=\"trackName\" value=\"<trackname>\" valuetype=\"data\" />\r\n        <param name=\"timescale\" value=\"<timescale>\" valuetype=\"data\" />\r\n        <param name=\"FourCC\" value=\"<fourcc>\" valuetype=\"data\" />\r\n        <param name=\"CodecPrivateData\" value=\"<codecprivatedata>\" valuetype=\"data\" />\r\n        <param name=\"MaxWidth\" value=\"<width>\" valuetype=\"data\" />\r\n        <param name=\"MaxHeight\" value=\"<height>\" valuetype=\"data\" />\r\n        <param name=\"DisplayWidth\" value=\"<width>\" valuetype=\"data\" />\r\n        <param name=\"DisplayHeight\" value=\"<height>\" valuetype=\"data\" />\r\n        <param name=\"Subtype\" value=\"\" valuetype=\"data\" />\r\n      </video>\r\n    </switch>\r\n  </body>\r\n</smil>\r\n";
        static string AudioLiveManifestTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">\r\n  <head>\r\n    <meta name=\"creator\" content=\"pushEncoder\" />\r\n  </head>\r\n  <body>\r\n    <switch>\r\n      <audio src=\"<ismafile>\" systemBitrate=\"<bitrate>\" systemLanguage=\"<lang>\">\r\n        <param name=\"trackID\" value=\"<trackid>\" valuetype=\"data\" />\r\n        <param name=\"trackName\" value=\"<trackname>\" valuetype=\"data\" />\r\n        <param name=\"timescale\" value=\"<timescale>\" valuetype=\"data\" />\r\n        <param name=\"FourCC\" value=\"<fourcc>\" valuetype=\"data\" />\r\n        <param name=\"CodecPrivateData\" value=\"<codecprivatedata>\" valuetype=\"data\" />\r\n        <param name=\"AudioTag\" value=\"<audiotag>\" valuetype=\"data\" />\r\n        <param name=\"Channels\" value=\"<channels>\" valuetype=\"data\" />\r\n        <param name=\"SamplingRate\" value=\"<samplingrate>\" valuetype=\"data\" />\r\n        <param name=\"BitsPerSample\" value=\"<bitpersample>\" valuetype=\"data\" />\r\n        <param name=\"PacketSize\" value=\"<packetsize>\" valuetype=\"data\" />\r\n        <param name=\"Subtype\" value=\"\" valuetype=\"data\" />\r\n      </audio>\r\n    </switch>\r\n  </body>\r\n</smil>\r\n";
        static string TextLiveManifestTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">\r\n  <head>\r\n    <meta name=\"creator\" content=\"pushEncoder\" />\r\n  </head>\r\n  <body>\r\n    <switch>\r\n      <textstream src=\"<ismtfile>\" systemBitrate=\"<bitrate>\" systemLanguage=\"<lang>\">\r\n        <param name=\"trackID\" value=\"<trackid>\" valuetype=\"data\" />\r\n        <param name=\"trackName\" value=\"<trackname>\" valuetype=\"data\" />\r\n        <param name=\"timescale\" value=\"<timescale>\" valuetype=\"data\" />\r\n         </textstream>\r\n    </switch>\r\n  </body>\r\n</smil>\r\n";

        public string GetTextManifest(int TrackID, string TrackName, int Bitrate, string source, string Lang, int TimeScale, string FourCC)
        {
            string manifest = string.Empty;

            manifest = TextLiveManifestTemplate;

            try
            {
                manifest = manifest.Replace("<ismtfile>", source);
                manifest = manifest.Replace("<bitrate>", Bitrate.ToString());
                manifest = manifest.Replace("<lang>", Lang);
                manifest = manifest.Replace("<trackid>", TrackID.ToString());
                manifest = manifest.Replace("<trackname>", TrackName.ToString());
                manifest = manifest.Replace("<timescale>", TimeScale.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating Live Manifest: " + ex.Message);
                manifest = string.Empty;
            }
            return manifest;
        }
        public string GetAudioManifest(int TrackID, string TrackName, int Bitrate, string source, string Lang, int TimeScale, string FourCC, string CodecPrivateData, string AudioTag, int Channels,int SamplingRate, int BitsPerSample, int PacketSize )
        {
            string manifest = string.Empty;

            try
            {
                manifest = AudioLiveManifestTemplate;


                manifest = manifest.Replace("<ismafile>", source);
                manifest = manifest.Replace("<bitrate>", Bitrate.ToString());
                manifest = manifest.Replace("<lang>", Lang);
                manifest = manifest.Replace("<trackid>", TrackID.ToString());
                manifest = manifest.Replace("<trackname>", TrackName.ToString());
                manifest = manifest.Replace("<timescale>", TimeScale.ToString());
                manifest = manifest.Replace("<fourcc>", FourCC.ToString());
                manifest = manifest.Replace("<codecprivatedata>", CodecPrivateData.ToString());
                manifest = manifest.Replace("<audiotag>", AudioTag.ToString());
                manifest = manifest.Replace("<channels>", Channels.ToString());
                manifest = manifest.Replace("<samplingrate>", SamplingRate.ToString());
                manifest = manifest.Replace("<bitpersample>", BitsPerSample.ToString());
                manifest = manifest.Replace("<packetsize>", PacketSize.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating Live Manifest: " + ex.Message);
                manifest = string.Empty;
            }
            return manifest;
        }
        public string GetVideoManifest(int TrackID, string TrackName, int Bitrate, string source, int TimeScale, string FourCC, string CodecPrivateData, int MaxWidth, int MaxHeight)
        {
            string manifest = string.Empty;
            try
            {

                manifest = VideoLiveManifestTemplate;
                manifest = manifest.Replace("<ismvfile>", source);
                manifest = manifest.Replace("<bitrate>", Bitrate.ToString());
                manifest = manifest.Replace("<trackid>", TrackID.ToString());
                manifest = manifest.Replace("<trackname>", TrackName.ToString());
                manifest = manifest.Replace("<timescale>", TimeScale.ToString());
                manifest = manifest.Replace("<fourcc>", FourCC.ToString());
                manifest = manifest.Replace("<codecprivatedata>",CodecPrivateData.ToString());
                manifest = manifest.Replace("<width>", MaxWidth.ToString());
                manifest = manifest.Replace("<height>", MaxHeight.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while creating Live Manifest: " + ex.Message);
                manifest = string.Empty;
            }
            return manifest;
        }
        private string pushurl;
        private int AssetID;
        private int StreamID;
        public SmoothLiveOutput()
        {
            ListPushManager = new Dictionary<string, SmoothPushManager>();
            AssetID = 22;
            StreamID = 0;
            pushurl = string.Empty;
        }
        /// <summary>
        /// Initialize
        /// Initialize the output uri
        /// </summary>
        public  async  Task<bool> Initialize(string uri)
        {
            bool result = true;
            ListPushManager = new Dictionary<string, SmoothPushManager>();
            AssetID = 22;
            StreamID = 0;
            pushurl = uri;
            System.Threading.Tasks.Task.Delay(1).Wait();
            return result;
        }
        /// <summary>
        /// ProcessManifest
        /// Initialize the output to receive the audio/video/text chunks based on the manifest content
        /// </summary>
        public async Task<bool> ProcessManifest(ManifestManager manifest)
        {
            bool result = false;
            await System.Threading.Tasks.Task.Delay(1);
            return result;
        }
        /// <summary>
        /// ProcessChunks
        /// Process the received the audio/video/text chunks
        /// </summary>
        public async Task<bool> ProcessChunks(ManifestManager cache)

        {

            await System.Threading.Tasks.Task.Delay(1);
            bool bResult = true;
            if (!(SendAudioChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while sending audio chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(SendVideoChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while sending video chunks for url: " + cache.ManifestUri.ToString());
            }
            if (!(SendTextChunks(cache)))
            {
                bResult = false;
                System.Diagnostics.Debug.WriteLine(string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " Error while sending text chunks for url: " + cache.ManifestUri.ToString());
            }
            return bResult;

        }
        bool SendBuffer(Stream stm, byte[] buffer, string source)
        {
            bool result = false;

            int offset = 0;
            while (offset < buffer.Length)
            {
                int BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(buffer, offset);
                string BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(buffer, offset);
                if (SendBox(stm, ISMHelper.Mp4Box.ReadMp4BoxBytes(buffer, offset, BoxLen)) == true)
                {
                   // Console.WriteLine("Source " + source + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                    offset += BoxLen;
                }
                else
                {
                    Console.WriteLine("Error while sending to Source " + source + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                    return false;
                }

            }
            if (offset == buffer.Length)
                result = true;
            return result;
        }
        bool SendTextLoop(Stream stm, ChunkList cl, ManifestManager cache)
        {
            bool result = true;
            for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
            {
                var cc = cl.ChunksList.Values.ElementAt(Index);
                if ((cc != null) && (cc.GetLength() > 0))
                {
                    ulong res = cc.GetLength();

                    if (res > 0)
                    {
                        if (SendBuffer(stm, cc.chunkBuffer, cl.Configuration.GetSourceName()) == true)
                        {
                            cache.TextSavedChunks++;
                            cache.TextSavedBytes += res;
                            // Free buffer
                            cc.chunkBuffer = null;
                            cl.OutputBytes += res;
                            cl.OutputChunks++;
                        }
                        else
                        {
                            result = false;
                            System.Diagnostics.Debug.WriteLine("Error while senfing audio chuncks");
                        }
                    }
                    else
                    {
                        result = false;
                        System.Diagnostics.Debug.WriteLine("Error while sending audio chuncks");
                    }
                }
                else
                    break;
            }
            return result;
        }
        bool SendAudioLoop(Stream stm, ChunkList cl, ManifestManager cache)
        {
            bool result = true;
            for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
            {
                var cc = cl.ChunksList.Values.ElementAt(Index);
                if ((cc != null) && (cc.GetLength() > 0))
                {
                    ulong res = cc.GetLength();

                    if (res > 0)
                    {
                        if (SendBuffer(stm, cc.chunkBuffer, cl.Configuration.GetSourceName()) == true)
                        {
                            cache.AudioSavedChunks++;
                            cache.AudioSavedBytes += res;
                            // Free buffer
                            cc.chunkBuffer = null;
                            cl.OutputBytes += res;
                            cl.OutputChunks++;
                        }
                        else
                        {
                            result = false;
                            System.Diagnostics.Debug.WriteLine("Error while senfing audio chuncks");
                        }
                    }
                    else
                    {
                        result = false;
                        System.Diagnostics.Debug.WriteLine("Error while sending audio chuncks");
                    }
                }
                else
                    break;
            }
            return result;
        }
        bool SendVideoLoop(Stream stm, ChunkList cl, ManifestManager cache)
        {
            bool result = true;
            for (int Index = (int)cl.OutputChunks; Index < (int)cl.InputChunks; Index++)
            {
                var cc = cl.ChunksList.Values.ElementAt(Index);
                if ((cc != null) && (cc.GetLength() > 0))
                {
                    ulong res = cc.GetLength();

                    if (res > 0)
                    {
                        if (SendBuffer(stm, cc.chunkBuffer, cl.Configuration.GetSourceName()) == true)
                        {
                            cache.VideoSavedChunks++;
                            cache.VideoSavedBytes += res;
                            // Free buffer
                            cc.chunkBuffer = null;
                            cl.OutputBytes += res;
                            cl.OutputChunks++;
                        }
                        else
                        {
                            result = false;
                            System.Diagnostics.Debug.WriteLine("Error while sending video chunks");
                        }
                    }
                    else
                    {
                        result = false;
                        System.Diagnostics.Debug.WriteLine("Error while sending video chunks");
                    }
                }
                else
                    break;
            }
            return result;
        }
        bool SendBox(Stream stm, byte[] buffer)
        {
            if ((buffer != null) && (buffer.Length > 0))
            {
                try
                {
                    string StringLength = String.Format("{0:X}", buffer.Length) + "\r\n";
                    byte[] sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                    stm.Write(sb, 0, sb.Length);
                    stm.Flush();
                    System.Threading.Thread.Sleep(0);

                    stm.Write(buffer, 0, buffer.Length);
                    stm.Flush();
                    System.Threading.Thread.Sleep(0);

                    StringLength = "\r\n";
                    sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                    stm.Write(sb, 0, sb.Length);
                    stm.Flush();
                    System.Threading.Thread.Sleep(0);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while sending box: " + ex.Message);
                    return false;
                }
                return true;
            }
            return false;
        }
        bool IsResponseOk(Stream stm)
        {
            bool rok = false;
            System.Threading.Thread.Sleep(1);
            byte[] rb = new byte[1024];
            int ri = stm.Read(rb, 0, rb.Length);
            if (ri > 0)
            {
                string PostResponse = "HTTP/1.1 200 OK";
                byte[] sb = UTF8Encoding.UTF8.GetBytes(PostResponse);
                rok = true;
                for (int j = 0; j < sb.Length; j++)
                {
                    if (sb[j] != rb[j])
                    {
                        rok = false;
                        break;
                    }
                }
            }
            return rok;
        }

        /// <summary>
        /// SendTextChunks
        /// Send text chunks on disk 
        /// </summary>
        public bool SendTextChunks(ManifestManager cache)
        {
            bool bResult = false;


            int TextTrack = 0;
            foreach (var cl in cache.TextChunkListList)
            {

                if (cl.OutputChunks == 0)
                {
                    if (cl.ftypData != null)
                    {
                        // Ready to talk to the server, moov box has been received
                        SmoothPushManager spm = new SmoothPushManager();
                        if (spm != null)
                        {
                            spm.NetworkClient = new TcpClient();
                            Uri u = new Uri(pushurl);
                            spm.NetworkClient.NoDelay = true;
                            spm.NetworkClient.Connect(u.Host, 80);
                            
                            if (spm.NetworkClient.Connected == true)
                            {
                                string FirstPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection: Keep-Alive\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nContent-Length: 0\r\nHost: " + u.Host + "\r\n\r\n";
                                spm.NetworkStream = spm.NetworkClient.GetStream();
                                if (spm.NetworkStream != null)
                                {
                                    byte[] sb = UTF8Encoding.UTF8.GetBytes(FirstPostData);
                                    if (sb != null)
                                    {
                                        spm.NetworkStream.Write(sb, 0, sb.Length);
                                        if (IsResponseOk(spm.NetworkStream) == true)
                                        {
                                            string url = string.Format("{0}/Streams({1}-stream{2})", pushurl, AssetID, StreamID);
                                            u = new Uri(url);
                                            string NextPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection : Keep-Alive\r\nTransfer-Encoding: Chunked\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nHost: " + u.Host + "\r\n\r\n";
                                            sb = UTF8Encoding.UTF8.GetBytes(NextPostData);
                                            spm.NetworkStream.Write(sb, 0, sb.Length);
                                            spm.NetworkStream.Flush();
                                            System.Threading.Thread.Sleep(1);

                                            // Initialization Done
                                            spm.TrackName = cl.Configuration.TrackName;
                                            spm.Bitrate = cl.Configuration.Bitrate;
                                            spm.StreamID = StreamID++;
                                            ListPushManager.Add(SmoothPushManager.GetKeyName(spm.TrackName, spm.Bitrate), spm);

                                            // Sending ftyp
                                            int BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.ftypData, 0);
                                            string BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.ftypData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.ftypData) == false)
                                                return false;
                                            // Sending Live Manifest
                                            int uuidHeaderSize = 4 + 4 + 16 + 4;
                                            uint flags = 0;
                                            TextChunkListConfiguration ac = cl.Configuration as TextChunkListConfiguration;
                                            if (ac != null)
                                            {
                                                string outerXml = GetTextManifest(
                                                    ac.TrackID,
                                                    ac.TrackName,
                                                    ac.Bitrate,
                                                    ac.GetSourceName(),
                                                    ac.Language,
                                                    ac.TimeScale,
                                                    ac.FourCC
                                                    );

                                                byte[] extendedData = UTF8Encoding.UTF8.GetBytes(outerXml);
                                                byte[] buffer = new byte[(uuidHeaderSize + extendedData.Length)]; // double it
                                                if (ISMHelper.Mp4BoxHelper.WriteExtendedBox(
                                                       buffer, LiveServerManBoxGuid,
                                                       1, flags,
                                                       extendedData,
                                                       0) > 0)
                                                {
                                                    BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(buffer, 0);
                                                    BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(buffer, 0);
                                                    Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");

                                                    if (SendBox(spm.NetworkStream, buffer) == false)
                                                        return false;
                                                }
                                            }

                                            // Sending moov
                                            BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.moovData, 0);
                                            BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.moovData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.moovData) == false)
                                                return false;
                                            // Sending the other boxes
                                            if (SendTextLoop(spm.NetworkStream, cl, cache) == false)
                                                return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ListPushManager.ContainsKey(SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)))
                    {
                        SmoothPushManager spm = ListPushManager[SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)];
                        if (spm != null)
                        {
                            if (SendTextLoop(spm.NetworkStream, cl, cache) == false)
                                return false;
                        }
                    }
                }
                if (cl.InputChunks == cl.OutputChunks)
                    bResult = true;
                TextTrack++;
            }


            return bResult;
        }

        /// <summary>
        /// SendAudioChunks
        /// Send audio chunks on disk 
        /// </summary>
        public bool SendAudioChunks(ManifestManager cache)
        {
            bool bResult = false;


            int AudioTrack = 0;
            foreach (var cl in cache.AudioChunkListList)
            {

                if (cl.OutputChunks == 0)
                {
                    if (cl.ftypData != null)
                    {
                        // Ready to talk to the server, moov box has been received
                        SmoothPushManager spm = new SmoothPushManager();
                        if (spm != null)
                        {
                            spm.NetworkClient = new TcpClient();
                            Uri u = new Uri(pushurl);
                            spm.NetworkClient.NoDelay = true;
                            spm.NetworkClient.Connect(u.Host, 80);
                            if (spm.NetworkClient.Connected == true)
                            {
                                string FirstPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection: Keep-Alive\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nContent-Length: 0\r\nHost: " + u.Host + "\r\n\r\n";
                                spm.NetworkStream = spm.NetworkClient.GetStream();
                                if(spm.NetworkStream!=null)
                                {
                                    byte[] sb = UTF8Encoding.UTF8.GetBytes(FirstPostData);
                                    if (sb != null)
                                    {
                                        spm.NetworkStream.Write(sb, 0, sb.Length);
                                        if (IsResponseOk(spm.NetworkStream) == true)
                                        {
                                            string url = string.Format("{0}/Streams({1}-stream{2})", pushurl, AssetID, StreamID);
                                            u = new Uri(url);
                                            string NextPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection : Keep-Alive\r\nTransfer-Encoding: Chunked\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nHost: " + u.Host + "\r\n\r\n";
                                            sb = UTF8Encoding.UTF8.GetBytes(NextPostData);
                                            spm.NetworkStream.Write(sb, 0, sb.Length);
                                            spm.NetworkStream.Flush();
                                            System.Threading.Thread.Sleep(1);

                                            // Initialization Done
                                            spm.TrackName = cl.Configuration.TrackName;
                                            spm.Bitrate = cl.Configuration.Bitrate;
                                            spm.StreamID = StreamID++;
                                            ListPushManager.Add(SmoothPushManager.GetKeyName(spm.TrackName, spm.Bitrate), spm);

                                            // Sending ftyp
                                            int BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.ftypData, 0);
                                            string BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.ftypData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.ftypData) == false)
                                                return false;

                                            // Sending Live Manifest
                                            int uuidHeaderSize = 4 + 4 + 16 + 4;
                                            uint flags = 0;
                                            AudioChunkListConfiguration ac = cl.Configuration as AudioChunkListConfiguration;
                                            if (ac != null)
                                            {
                                                string outerXml = GetAudioManifest(
                                                    ac.TrackID,
                                                    ac.TrackName,
                                                    ac.Bitrate,
                                                    ac.GetSourceName(),
                                                    ac.Language,
                                                    ac.TimeScale,
                                                    ac.FourCC,
                                                    ac.CodecPrivateData,
                                                    ac.AudioTag,
                                                    ac.Channels,
                                                    ac.SamplingRate,
                                                    ac.BitsPerSample,
                                                    ac.PacketSize
                                                    );

                                                byte[] extendedData = UTF8Encoding.UTF8.GetBytes(outerXml);
                                                byte[] buffer = new byte[(uuidHeaderSize + extendedData.Length)]; // double it
                                                if (ISMHelper.Mp4BoxHelper.WriteExtendedBox(
                                                       buffer, LiveServerManBoxGuid,
                                                       1, flags,
                                                       extendedData,
                                                       0) > 0)
                                                {
                                                    BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(buffer, 0);
                                                    BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(buffer, 0);
                                                    Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");

                                                    if (SendBox(spm.NetworkStream, buffer) == false)
                                                        return false;
                                                }
                                            }

                                            // Sending moov
                                            BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.moovData, 0);
                                            BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.moovData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.moovData) == false)
                                                return false;
                                            // Sending the other boxes
                                            if (SendAudioLoop(spm.NetworkStream, cl, cache) == false)
                                                return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if(ListPushManager.ContainsKey(SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)))
                    {
                        SmoothPushManager spm = ListPushManager[SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)];
                        if(spm!=null)
                        {
                            if (SendAudioLoop(spm.NetworkStream, cl, cache) == false)
                                return false;
                        }
                    }
                }
                if (cl.InputChunks == cl.OutputChunks)
                    bResult = true;
                AudioTrack++;
            }
 

            return bResult;
        }


        /// <summary>
        /// SendVideoChunks
        /// Send video chunks  
        /// </summary>
        public bool SendVideoChunks(ManifestManager cache)
        {
            bool bResult = false;


            int VideoTrack = 0;
            foreach (var cl in cache.VideoChunkListList)
            {

                if (cl.OutputChunks == 0)
                {
                    if (cl.ftypData != null)
                    {
                        // Ready to talk to the server, moov box has been received
                        SmoothPushManager spm = new SmoothPushManager();
                        if (spm != null)
                        {
                            spm.NetworkClient = new TcpClient();
                            Uri u = new Uri(pushurl);
                            spm.NetworkClient.NoDelay = true;
                            spm.NetworkClient.Connect(u.Host, 80);
                            if (spm.NetworkClient.Connected == true)
                            {
                                string FirstPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection: Keep-Alive\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nContent-Length: 0\r\nHost: " + u.Host + "\r\n\r\n";
                                spm.NetworkStream = spm.NetworkClient.GetStream();
                                if (spm.NetworkStream != null)
                                {
                                    byte[] sb = UTF8Encoding.UTF8.GetBytes(FirstPostData);
                                    if (sb != null)
                                    {
                                        spm.NetworkStream.Write(sb, 0, sb.Length);
                                        if (IsResponseOk(spm.NetworkStream) == true)
                                        {
                                            string url = string.Format("{0}/Streams({1}-stream{2})", pushurl, AssetID, StreamID);
                                            u = new Uri(url);
                                            string NextPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection : Keep-Alive\r\nTransfer-Encoding: Chunked\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nHost: " + u.Host + "\r\n\r\n";
                                            sb = UTF8Encoding.UTF8.GetBytes(NextPostData);
                                            spm.NetworkStream.Write(sb, 0, sb.Length);
                                            spm.NetworkStream.Flush();
                                            System.Threading.Thread.Sleep(1);

                                            // Initialization Done
                                            spm.TrackName = cl.Configuration.TrackName;
                                            spm.Bitrate = cl.Configuration.Bitrate;
                                            spm.StreamID = StreamID++;
                                            ListPushManager.Add(SmoothPushManager.GetKeyName(spm.TrackName, spm.Bitrate), spm);

                                            // Sending ftyp
                                            int BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.ftypData, 0);
                                            string BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.ftypData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.ftypData) == false)
                                                return false;

                                            // Sending Live Manifest
                                            int uuidHeaderSize = 4 + 4 + 16 + 4;
                                            uint flags = 0;
                                            VideoChunkListConfiguration ac = cl.Configuration as VideoChunkListConfiguration;
                                            if (ac != null)
                                            {
                                                string outerXml = GetVideoManifest(
                                                    ac.TrackID,
                                                    ac.TrackName,
                                                    ac.Bitrate,
                                                    ac.GetSourceName(),
                                                    ac.TimeScale,
                                                    ac.FourCC,
                                                    ac.CodecPrivateData,
                                                    ac.Width,
                                                    ac.Height
                                                    );

                                                byte[] extendedData = UTF8Encoding.UTF8.GetBytes(outerXml);
                                                byte[] buffer = new byte[(uuidHeaderSize + extendedData.Length)]; // double it
                                                if (ISMHelper.Mp4BoxHelper.WriteExtendedBox(
                                                       buffer, LiveServerManBoxGuid,
                                                       1, flags,
                                                       extendedData,
                                                       0) > 0)
                                                {
                                                    BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(buffer, 0);
                                                    BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(buffer, 0);
                                                    Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");

                                                    if (SendBox(spm.NetworkStream, buffer) == false)
                                                        return false;
                                                }
                                            }

                                            // Sending moov
                                            BoxLen = ISMHelper.Mp4Box.ReadMp4BoxInt32(cl.moovData, 0);
                                            BoxType = ISMHelper.Mp4Box.ReadMp4BoxType(cl.moovData, 0);
                                            Console.WriteLine("Source " + cl.Configuration.GetSourceName() + " Streaming MP4 Box " + BoxType + " " + BoxLen.ToString() + " Bytes");
                                            if (SendBox(spm.NetworkStream, cl.moovData) == false)
                                                return false;

                                            // Sending the other boxes
                                            if (SendVideoLoop(spm.NetworkStream, cl, cache) == false)
                                                return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ListPushManager.ContainsKey(SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)))
                    {
                        SmoothPushManager spm = ListPushManager[SmoothPushManager.GetKeyName(cl.Configuration.TrackName, cl.Configuration.Bitrate)];
                        if (spm != null)
                        {
                            if (SendVideoLoop(spm.NetworkStream, cl, cache) == false)
                                return false;
                        }
                    }
                }
                if (cl.InputChunks == cl.OutputChunks)
                    bResult = true;
                VideoTrack++;
            }


            return bResult;
        }



        private Dictionary<string, SmoothPushManager> ListPushManager;
    }
}
