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
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ASTool.ISMHelper
{
    public class IsmPushEncoder
    {
        static Guid kTrackFragExtHeaderBoxGuid  = new Guid("{6D1D9B05-42D5-44E6-80E2-141DAFF757B2}");
        static Guid LiveServerManBoxGuid        = new Guid("{A5D40B30-E814-11DD-BA2F-0800200C9A66}");
        private Thread _thread;
        private string _pushurl;
        private string _url;
        private string _liveManifest;
        private FakeLOTTServer _server;
        private Stream _ismvStream;
        private byte[] _mp4BoxBuffer = new byte[1024];
        private int _mp4BoxBufferLength;
        private int _mp4BoxBufferOffset;
        private long _bitrate;
        private string _ismvFile;
        private int _streamID;
        private Chunk[] _ChunkArray;
        public bool InsertNtp;
        public bool InsertBoxes = true;
        public Stream PipedSource;
        public bool Loop = false;
        private bool _seenMoov;
        private IsmFile _ismFile;

        private int _trackID;
        public UInt64 OutputChunks { get; set; }
        public UInt64 OutputBytes { get; set; }
        public string GetSourceName() { return _ismvFile; }
        private Options options;
        public IsmPushEncoder(
            FakeLOTTServer server,
            string ismvFile,
            string pushUrl,
            int AssetId,
            int StreamId,
            int bitrate,
            string manifest,
            Chunk[] ChunkArray,
            int trackID,
            Options opt)
            : this(server, null, ismvFile, pushUrl, AssetId, StreamId, bitrate,manifest,ChunkArray,trackID,opt)
        {
            OutputBytes = 0;
            OutputChunks = 0;
        }
        public IsmPushEncoder(
            FakeLOTTServer server,
            IsmFile ismFile,
            string ismvFile, 
            string pushUrl, 
            int AssetId, 
            int StreamId,
            int bitrate,
            string LiveManifest,
            Chunk[] ChunkArray,
            int trackID,
            Options opt)
        {
            OutputBytes = 0;
            OutputChunks = 0;
            _ismFile = ismFile;
            _server = server;
            _ismvStream = File.OpenRead(ismvFile);
            _ismvFile = ismvFile;
            _bitrate = bitrate;
            _liveManifest = LiveManifest;
            //PushEncoder.exe uses this format - note '?' and lower case 's' in streams
            //_url = string.Format("{0}/Streams({1}-stream{2})", pushUrl, AssetId, StreamId);
            _pushurl = pushUrl;
// flecoqui            _url = string.Format("{0}?Streams({1}-stream{2})", pushUrl, AssetId, StreamId);
            _url = string.Format("{0}/Streams({1}-stream{2})", pushUrl, AssetId, StreamId);

            if (this.options != null)
                this.options.LogInformation("Pushing to url: " + _url + " source: " + ismvFile + " manifest: \r\n" + _liveManifest);
            
            _thread = new Thread(new ThreadStart(Worker));
            _ismvFile = ismvFile;
            _streamID = StreamId;
            _ChunkArray = ChunkArray;
            _trackID = trackID;
            options = opt;

        }

        public void Start()
        {
            _thread.Start();
        }
        private long GetAbsoluteTime()
        {
            DateTime CurrentDate = DateTime.Now;
            return CurrentDate.Ticks;
        }
        private double GetCurrentBitrate(DateTime BeginDate, long byteSent)
        {
                DateTime CurrentDate = DateTime.Now;
                long elapsedTicks = CurrentDate.Ticks - BeginDate.Ticks;
                double CurrentBitrate = 0;
                if(elapsedTicks != 0)
                    CurrentBitrate = (byteSent * 8 * 10000000) / elapsedTicks;

                return CurrentBitrate;
        }

        private IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            Console.WriteLine("File " + _ismvFile + " IPEndPoint StreamID " + _streamID.ToString());

            return new IPEndPoint(IPAddress.Any, _streamID +1000); //bind to a specific ip address on your server 
        } 
        private bool ChunkCanBeStreamed(byte[] buffer,int loopIndex) 
        {
            if (Mp4BoxHelper.IsBoxType(buffer, 0, "moof"))
                return true;           
            if(Mp4BoxHelper.IsBoxType(buffer, 0, "mdat"))
                return true;
            if(Mp4BoxHelper.IsBoxType(buffer, 0, "mfra"))
                return false;
            if(loopIndex == 0)
                return true;

            return false;
        }
        private int GetMOOFTFHDTrackID(byte[] buffer, int loopIndex)
        {
            if (Mp4BoxHelper.IsBoxType(buffer, 0, "moof"))
            {
                int trafOffset = Mp4BoxHelper.FindChild(buffer, 0, "traf");
                if(trafOffset != -1)
                {
                    int tfhdOffset = Mp4BoxHelper.FindChild(buffer, trafOffset, "tfhd");
                    if (tfhdOffset != -1)
                    {
                        if ((tfhdOffset + 16) < buffer.Length)
                        {
                            return (int)buffer[tfhdOffset + 15];
                        }
                    }
                }
            }
            return -1;
        }

        private void WaitChunkTime(long Begin, long AbsoluteTime)
        {
            DateTime LocalTime = DateTime.Now;
            long diff = LocalTime.Ticks - Begin;
            while ((diff) < AbsoluteTime)
            {
                LocalTime = DateTime.Now;
                diff = LocalTime.Ticks - Begin;
                Thread.Sleep(1);
            }
        }
        private int StreamCBR(Stream stm, byte[] buffer, int Length)
        {
            int i = 0;

            long numberofbits = (long)Length * 8 * 1000;
            long TimeToTransmitChunkMS = numberofbits / _bitrate; 
            int NumberofWaitPeriod = Length/1260;
            int WaitPeriodMS = (int) TimeToTransmitChunkMS/NumberofWaitPeriod;

            for(i = 0; i < Length; )
            { 
                int offset = ( (i+1260 < Length) ? 1260: Length -i );
                stm.Write(buffer, i, offset);
                i += offset;
                Thread.Sleep(WaitPeriodMS);
            }
            stm.Flush();
            Thread.Sleep(0);
            return i;
        }

        private void Worker()
        {
            byte[] buffer= new byte[2048];
            int length = 0;
            

            StringBuilder restURL = new StringBuilder();
            XmlDocument xDoc = new XmlDocument();
            bool bException = false;
            bool bResult = false;
            
            try
            {

                restURL.AppendFormat(_pushurl);

                // use the static Create method of the WebRequest object 
                // casting the returned WebRequest to an HttpWebRequest

             if(this.options != null)this.options.LogVerbose("File " + _ismvFile + " Create");
            Uri u = new Uri(_pushurl);
            TcpClient tc = new TcpClient();
            tc.NoDelay = true;
            tc.Connect(u.Host, 80);
            if (tc.Connected == true)
            {
                string FirstPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection: Keep-Alive\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nContent-Length: 0\r\nHost: " + u.Host + "\r\n\r\n";
                Stream stm = tc.GetStream();
                
                byte[] sb = UTF8Encoding.UTF8.GetBytes(FirstPostData);
                stm.Write(sb, 0, sb.Length);
                Thread.Sleep(1);
                byte[] rb = new byte[1024];
                int ri = stm.Read(rb, 0, rb.Length);
                if (ri > 0)
                {
                    string PostResponse = "HTTP/1.1 200 OK";
                    sb = UTF8Encoding.UTF8.GetBytes(PostResponse);
                    bool rok = true;
                    for (int j = 0; j < sb.Length; j++)
                    {
                        if (sb[j] != rb[j])
                        {
                            rok = false;
                            break;
                        }
                    }
                    //while (tc.Available > 0) stm.Read(rb, 0, rb.Length);
                    if (rok == true)
                    {
                        u = new Uri(_url);
                        string NextPostData = "POST " + u.LocalPath + " HTTP/1.1\r\nConnection : Keep-Alive\r\nTransfer-Encoding: Chunked\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nHost: " + u.Host + "\r\n\r\n";
                        sb = UTF8Encoding.UTF8.GetBytes(NextPostData);
                        stm.Write(sb, 0, sb.Length);
                        stm.Flush();
                        Thread.Sleep(1);
                        //while (tc.Available > 0) stm.Read(rb, 0, rb.Length);

                        DateTime BeginDate = DateTime.Now;

                        long CurrentChunk = 0;
                        long AbsoluteTime = 0;

                        int i;
                       // long sent = 0;

                        int loopIndex = 0;
                        do
                        {
                            SetPosFirstBox();
                            CurrentChunk = 0;
                            do
                            {
                                length = GetNextBoxSize();
                                if (length > buffer.Length)
                                {
                                    buffer = new byte[length * 2]; // double it
                                }
                                i = GetNextBox(buffer);
                                if (i > 0)
                                {
                                        bool bContinue = true;
                                        int trackID = GetMOOFTFHDTrackID(buffer, 0);
                                        while ((trackID >= 0) && (trackID != _trackID))
                                        {
                                            bContinue = false;
                                            length = GetNextBoxSize();
                                            if (length > buffer.Length)
                                            {
                                                buffer = new byte[length * 2]; // double it
                                            }
                                            i = GetNextBox(buffer);
                                            if (i > 0)
                                            {
                                                if (Mp4BoxHelper.IsBoxType(buffer, 0, "mdat"))
                                                {
                                                    length = GetNextBoxSize();
                                                    if (length > buffer.Length)
                                                    {
                                                        buffer = new byte[length * 2]; // double it
                                                    }
                                                    i = GetNextBox(buffer);
                                                    if (i > 0)
                                                    {
                                                        bContinue = true;
                                                        trackID = GetMOOFTFHDTrackID(buffer, 0);
                                                    }
                                                }

                                            }
                                        }
                                        if ((bContinue == true) && (ChunkCanBeStreamed(buffer, loopIndex) == true))
                                        {
                                            int Len = Mp4BoxHelper.ReadMp4BoxLength(buffer, 0);
                                            string Title = Mp4BoxHelper.ReadMp4BoxTitle(buffer, 0);
                                            {
                                                if(this.options!=null)
                                                    this.options.LogVerbose("File " + _ismvFile + " Streaming MP4 Box " + Title + " " + Len.ToString() + " Bytes");
                                                //i = GetData(buffer, length);
                                                if (Mp4BoxHelper.IsBoxType(buffer, 0, "moof"))
                                                {

                                                    if (CurrentChunk < _ChunkArray.Length)
                                                    {
                                                        int trafOffset = Mp4BoxHelper.FindChild(buffer, 0, "traf");
                                                        if (-1 != trafOffset)
                                                        {
                                                            // need to insert data here
                                                            byte[] extendedData = new byte[16];
                                                            if(CurrentChunk==0)
                                                            {
                                                                if ((_ChunkArray[CurrentChunk].time != 0) &&
                                                                    (_ChunkArray[CurrentChunk].time < _ChunkArray[CurrentChunk].duration))
                                                                    AbsoluteTime += _ChunkArray[CurrentChunk].time;
                                                            }
                                                            long duration = _ChunkArray[CurrentChunk].duration;
                                                            extendedData[0] = (byte)(AbsoluteTime >> 56);
                                                            extendedData[1] = (byte)(AbsoluteTime >> 48);
                                                            extendedData[2] = (byte)(AbsoluteTime >> 40);
                                                            extendedData[3] = (byte)(AbsoluteTime >> 32);
                                                            extendedData[4] = (byte)(AbsoluteTime >> 24);
                                                            extendedData[5] = (byte)(AbsoluteTime >> 16);
                                                            extendedData[6] = (byte)(AbsoluteTime >> 8);
                                                            extendedData[7] = (byte)(AbsoluteTime >> 0);

                                                            extendedData[8] = (byte)(duration >> 56);
                                                            extendedData[9] = (byte)(duration >> 48);
                                                            extendedData[10] = (byte)(duration >> 40);
                                                            extendedData[11] = (byte)(duration >> 32);
                                                            extendedData[12] = (byte)(duration >> 24);
                                                            extendedData[13] = (byte)(duration >> 16);
                                                            extendedData[14] = (byte)(duration >> 8);
                                                            extendedData[15] = (byte)(duration >> 0);
                                                            // flecoqui
                                                            uint flags = 0;
                                                            //  if (InsertNtp)
                                                            //  {
                                                            //      flags |= 8; // @@ TODO: not setting 8 crashes in MP4Parser
                                                            //  }
                                                            i += Mp4BoxHelper.InsertExtendedBoxHead(
                                                                buffer, kTrackFragExtHeaderBoxGuid,
                                                                1, flags,
                                                                extendedData,
                                                                0, trafOffset);
                                                        }

                                                        WaitChunkTime(BeginDate.Ticks, AbsoluteTime);
                                                        /*
                                                        DateTime LocalTime = DateTime.Now;
                                                        long diff = LocalTime.Ticks - BeginDate.Ticks;
                                                        while ((diff) < AbsoluteTime)
                                                        {
                                                            LocalTime = DateTime.Now;
                                                            diff = LocalTime.Ticks - BeginDate.Ticks;
                                                            Thread.Sleep(1);
                                                        }*/
                                                        AbsoluteTime += _ChunkArray[CurrentChunk++].duration;
                                                    }
                                                    else
                                                    {
                                                        System.Diagnostics.Debug.WriteLine("Error while streaming moof");
                                                        CurrentChunk++;
                                                    }
                                                    if (CurrentChunk > _ChunkArray.Length)
                                                        CurrentChunk = 0;

                                                }


                                                string StringLength = String.Format("{0:X}", i) + "\r\n";
                                                sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                                                stm.Write(sb, 0, sb.Length);
                                                stm.Flush();
                                                Thread.Sleep(0);

                                               // if (i < 1260)
                                                {
                                                    stm.Write(buffer, 0, i);
                                                    stm.Flush();
                                                    Thread.Sleep(0);
                                                }
                                                //else
                                                //{
                                                //    StreamCBR(stm, buffer, i);
                                                //}
                                                StringLength = "\r\n";
                                                sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                                                stm.Write(sb, 0, sb.Length);
                                                stm.Flush();
                                                Thread.Sleep(0);

                                                // while (tc.Available > 0) stm.Read(rb, 0, rb.Length);
                                                this.OutputBytes += (ulong) i;
                                                this.OutputChunks++;

                                                if (null != _ismFile && Mp4BoxHelper.IsBoxType(buffer, 0, "ftyp"))
                                                {
                                                    // insert a live manifest
                                                    int uuidHeaderSize = 4 + 4 + 16 + 4;
                                                    uint flags = 0;
                                                    string outerXml = _liveManifest;
                                                    byte[] extendedData = UTF8Encoding.UTF8.GetBytes(outerXml);
                                                    //byte[] extendedData = UnicodeEncoding.Unicode.GetBytes(outerXml);
                                                    if (buffer.Length < (uuidHeaderSize + extendedData.Length))
                                                    {
                                                        // increase our internal buffers
                                                        buffer = new byte[(uuidHeaderSize + extendedData.Length) * 2]; // double it
                                                    }
                                                    i = Mp4BoxHelper.WriteExtendedBox(
                                                       buffer, LiveServerManBoxGuid,
                                                       1, flags,
                                                       extendedData,
                                                       0);
                                                    if (i > 0)
                                                    {
                                                        Len = Mp4BoxHelper.ReadMp4BoxLength(buffer, 0);
                                                        Title = Mp4BoxHelper.ReadMp4BoxTitle(buffer, 0);
                                                        if (this.options != null)
                                                            this.options.LogVerbose("File " + _ismvFile + " Streaming Manifest at " + _bitrate.ToString() + " b/s MP4 Box " + Title + " " + Len.ToString() + " Bytes");
                                                        //i = GetData(buffer, length);

                                                        StringLength = String.Format("{0:X}", i) + "\r\n";
                                                        sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                                                        stm.Write(sb, 0, sb.Length);
                                                        stm.Flush();
                                                        Thread.Sleep(0);

                                                        stm.Write(buffer, 0, i);
                                                        stm.Flush();
                                                        Thread.Sleep(0);

                                                        StringLength = "\r\n";
                                                        sb = UTF8Encoding.UTF8.GetBytes(StringLength);
                                                        stm.Write(sb, 0, sb.Length);
                                                        stm.Flush();
                                                        Thread.Sleep(0);
                                                        //       while (tc.Available > 0) stm.Read(rb, 0, rb.Length);
                                                        this.OutputBytes += (ulong)i;
                                                        this.OutputChunks++;

                                                    }

                                                }
                                            }
                                        }
                                }
                                //Thread.Sleep(1);
                                /*
                                double CurrentBitrate = GetCurrentBitrate(BeginDate, sent);
                                Console.WriteLine("File " + _ismvFile + "Current bitrate  " + CurrentBitrate + " b/s");
                                while (CurrentBitrate > (_bitrate * 6) / 4)
                                {
                                    Thread.Sleep(0);
                                    CurrentBitrate = GetCurrentBitrate(BeginDate, sent);
                                }
                                 */
                            }
                            while ((i != 0) && (i != -1));
                            loopIndex++;
                        }
                        while (Loop == true);

                    }
                }
            }
                /*
            restRequest = (HttpWebRequest)WebRequest.Create(restURL.ToString());
            restRequest.Method = "POST";
            restRequest.UserAgent = "NSPlayer/7.0 IIS-LiveStream/7.0";
            restRequest.ContentLength = 0;
            restRequest.KeepAlive = true;
          //  restRequest.Pipelined = true;
//            restRequest.SendChunked = true;
//            restRequest.AllowWriteStreamBuffering = false;
            ServicePoint sp = restRequest.ServicePoint;
            //sp.Expect100Continue = false;
            sp.SetTcpKeepAlive(true, 10000, 5000);
            sp.BindIPEndPointDelegate = new System.Net.BindIPEndPoint(BindIPEndPointCallback);
           // sp.ConnectionLimit = 6;
            Console.WriteLine("File " + _ismvFile + "Binding 1");

            string header = restRequest.Headers.ToString();
            Console.WriteLine("File " + _ismvFile + " Header 1" + restRequest.Headers.ToString());

            restResponse = (HttpWebResponse)restRequest.GetResponse();
            header = restRequest.Headers.ToString();
            Console.WriteLine("File " + _ismvFile + " Header 2" + restRequest.Headers.ToString());
            restResponse.Close();
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                sp.BindIPEndPointDelegate = null;
                Console.WriteLine("File " + _ismvFile + "HTTP POST Response OK");
                bResult = true;
            }
            else
            {
                sp.BindIPEndPointDelegate = null;
                Console.WriteLine("File " + _ismvFile + "HTTP POST Response NON OK");
                return;
            }
               
            
        restURL.Clear();
        restURL.AppendFormat(_url);
        //restURL = restURL.Replace("http://mr20ingestor.contoso.com","");
        //string PostData = "POST " + restURL + " HTTP /1.1\r\nConnection : Keep-Alive\r\nTransfer-Encoding: Chunked\r\nUser-Agent: NSPlayer/7.0 IIS-LiveStream/7.0\r\nHost: mr20ingestor.contoso.com\r\n\r\n";
        //byte[] extendedPostData = UTF8Encoding.UTF8.GetBytes(PostData);
        restRequest = (HttpWebRequest)WebRequest.Create(restURL.ToString());


            BeginDate = DateTime.Now;

            restRequest.Method = "POST";
            restRequest.UserAgent = "NSPlayer/7.0 IIS-LiveStream/7.0";
            restRequest.SendChunked = true;
            restRequest.KeepAlive = true;
            restRequest.Pipelined = true;
            restRequest.AllowWriteStreamBuffering = true;

            sp = restRequest.ServicePoint;
            //sp.Expect100Continue = false;
            sp.SetTcpKeepAlive(true, 10000, 5000);
            sp.BindIPEndPointDelegate = new System.Net.BindIPEndPoint(BindIPEndPointCallback);
          //  sp.ConnectionLimit = 6;


            Console.WriteLine("File " + _ismvFile + "Binding 2");

//            sp.Expect100Continue = false;
                
            Console.WriteLine("File " + _ismvFile + " Connection limit: " + sp.ConnectionLimit.ToString() + " pipe: " + sp.SupportsPipelining);
            header = restRequest.Headers.ToString();
            Console.WriteLine("File " + _ismvFile + " Header 3" + restRequest.Headers.ToString());
            Stream newStream = restRequest.GetRequestStream();
            //newStream.Write(extendedPostData, 0, extendedPostData.Length);
            int i;
            long sent = 0;
            do
            {
                length = GetNextBoxSize();
                if (length > buffer.Length)
                {
                    buffer = new byte[length * 2]; // double it
                }
                i = GetNextBox(buffer);
                if (i > 0)
                {
                    int Len = Mp4BoxHelper.ReadMp4BoxLength(buffer, 0);
                    string Title = Mp4BoxHelper.ReadMp4BoxTitle(buffer, 0);
                    Console.WriteLine("File " +_ismvFile + " Streaming MP4 Box " + Title + " " + Len.ToString() + " Bytes to ");
                    //i = GetData(buffer, length);
                    if (Mp4BoxHelper.IsBoxType(buffer, 0, "moof"))
                    {
                        int trafOffset = Mp4BoxHelper.FindChild(buffer, 0, "traf");
                        if (-1 != trafOffset)
                        {
                            // need to insert data here
                            byte[] extendedData = new byte[16];
                            uint flags = 0;
                            if (InsertNtp)
                            {
                                flags |= 8; // @@ TODO: not setting 8 crashes in MP4Parser
                            }
                            i += Mp4BoxHelper.InsertExtendedBoxHead(
                                buffer, kTrackFragExtHeaderBoxGuid,
                                0, flags,
                                extendedData,
                                0, trafOffset);
                        }
                    }

                    newStream.Write(buffer, 0, i);
                    sent += i;

                    if (null != _ismFile && Mp4BoxHelper.IsBoxType(buffer, 0, "ftyp"))
                    {
                        // insert a live manifest
                        int uuidHeaderSize = 4 +4 +16 +4;
                        uint flags = 0;
                        string outerXml = _liveManifest;
                        byte[] extendedData = UTF8Encoding.UTF8.GetBytes(outerXml);
                        //byte[] extendedData = UnicodeEncoding.Unicode.GetBytes(outerXml);
                        if (buffer.Length < (uuidHeaderSize + extendedData.Length))
                        {
                            // increase our internal buffers
                            buffer = new byte[(uuidHeaderSize + extendedData.Length) * 2]; // double it
                        }
                         i = Mp4BoxHelper.WriteExtendedBox(
                            buffer, LiveServerManBoxGuid,
                            0, flags,
                            extendedData,
                            0);
                         if (i > 0)
                         {
                             Len = Mp4BoxHelper.ReadMp4BoxLength(buffer, 0);
                             Title = Mp4BoxHelper.ReadMp4BoxTitle(buffer, 0);
                             Console.WriteLine("File " + _ismvFile + "Streaming Manifest at " + _bitrate.ToString() + " b/s MP4 Box " + Title + " " + Len.ToString() + " Bytes");
                             //i = GetData(buffer, length);
                             newStream.Write(buffer, 0, i);
                             sent += i;
                         }              

                    }
                }
                //Thread.Sleep(1);
                double CurrentBitrate = GetCurrentBitrate(BeginDate, sent);
                Console.WriteLine("File " + _ismvFile + "Current bitrate  " + CurrentBitrate + " b/s");
                while (CurrentBitrate > (_bitrate*6)/4)
                {
                    Thread.Sleep(0);
                    CurrentBitrate = GetCurrentBitrate(BeginDate, sent);
                }
            }
            while ((i !=0) && (i != -1));
            newStream.Close();


            // use the GetResponse method to obtain a WebResponse object
            // for the request casting to an HttpWebResponse
          /*  restResponse = (HttpWebResponse)restRequest.GetResponse();
            ReadStream = restResponse.GetResponseStream();
            ReadStream.Close();
    
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("File " + _ismvFile + "HTTP POST POST Response OK");
                bResult = true;
            }*/
            }
            catch (Exception we)
            {
                Console.WriteLine("File " + _ismvFile + "HTTP POST Exception: " + we.Message);
                bException = true;

            }
            finally
            {
                if ((bException == false) && (bResult == false))
                    Console.WriteLine("File " + _ismvFile + "HTTP POST Error");

            }

 //           _server.Post(_url, new IisDataHandler(GetData), new IisDataHandler(OutputData));
        }


        private void SourceBitsCopied(byte [] buffer, int offset, int length)
        {
            if (null != PipedSource)
            {
                PipedSource.Write(buffer, offset, length);
            }
        }

        private void SourceCopyBits(int sourceOffset, byte[] buffer, int destOffset, int length)
        {
            Buffer.BlockCopy(_mp4BoxBuffer, sourceOffset, buffer, destOffset, length);
            SourceBitsCopied(buffer, destOffset, length);
        }

        private int SetPosFirstBox()
        {
            _ismvStream.Position = 0;
            return 1;
        }
        private int GetNextBox(byte[] buffer)
        {
            bool bException = false;
            int length = -1;
            try
            {
                if (_ismvStream.Position == _ismvStream.Length)
                    return 0;
                    
                int ret = _ismvStream.Read(_mp4BoxBuffer, 0, 4);
                length = Mp4BoxHelper.ReadMp4BoxLength(_mp4BoxBuffer, 0);

                if (buffer.Length < length)
                {
                    _ismvStream.Position -= 4;
                    return -1;
                }
                if (length > _mp4BoxBuffer.Length)
                {
                    // increase our internal buffers
                    byte[] newBuf = new byte[length * 2]; // double it
                    Buffer.BlockCopy(_mp4BoxBuffer, 0, newBuf, 0, 4);
                    _mp4BoxBuffer = newBuf;
                }
                _ismvStream.Read(_mp4BoxBuffer, 4, (int)(length - 4));
                Buffer.BlockCopy(_mp4BoxBuffer, 0, buffer, 0, length);
            }
            catch (Exception we)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting next box: " + we.Message);
                bException = true;
            }
            if (bException == true)
                return -1;
            else
                return length;
        }
        private int GetNextBoxSize()
        {
            int length = -1;
            try
            {
                if (_ismvStream.Position == _ismvStream.Length)
                    length = 0;
                else
                {
                    int ret = _ismvStream.Read(_mp4BoxBuffer, 0, 4);
                    _ismvStream.Position -= 4;
                    length = Mp4BoxHelper.ReadMp4BoxLength(_mp4BoxBuffer, 0);
                }
            }
            catch (Exception we)
            {
                System.Diagnostics.Debug.WriteLine("Exception while getting next box size: " + we.Message);
                length = -1;
            }
            return length;
        }
        private int GetData(byte[] buffer, int length)
        {
            int ret;
            if (this._mp4BoxBufferOffset < this._mp4BoxBufferLength)
            {
                // we have outstanding data still
                ret = (_mp4BoxBufferLength - _mp4BoxBufferOffset);
                if (ret > length)
                {
                    ret = length;
                }
                SourceCopyBits(_mp4BoxBufferOffset, buffer, 0, ret);
                _mp4BoxBufferOffset += ret;
                return ret;
            }

            int mp4BoxLen;
            do
            {
                if (_ismvStream.Position == _ismvStream.Length)
                {
                    if (Loop)
                    {
                        _ismvStream.Position = 0;
                    }
                    else
                    {
                        return 0;
                    }
                }

                // read the length

                ret = _ismvStream.Read(_mp4BoxBuffer, 0, 4);
                mp4BoxLen = Mp4BoxHelper.ReadMp4BoxLength(_mp4BoxBuffer, 0);
                if (mp4BoxLen > _mp4BoxBuffer.Length)
                {
                    // increase our internal buffers
                    byte[] newBuf = new byte[mp4BoxLen * 2]; // double it
                    Buffer.BlockCopy(_mp4BoxBuffer, 0, newBuf, 0, 4);
                    _mp4BoxBuffer = newBuf;
                }
                //if (mp4BoxLen <= length)
                //{
                //    // the incoming buffer can handle this chunk, send it all up
                //    SourceCopyBits(0, buffer, 0, 4);
                //    _ismvStream.Read(buffer, 4, (int)(mp4BoxLen - 4));
                //    SourceBitsCopied(buffer, 4, (int)(mp4BoxLen - 4));
                //    return (int)mp4BoxLen;
                //}
                // store this all off in our internal buffers
                _ismvStream.Read(_mp4BoxBuffer, 4, (int)(mp4BoxLen - 4));

                if (Mp4BoxHelper.IsBoxType(_mp4BoxBuffer, 0, "moov"))
                {
                    if (!_seenMoov)
                    {
                        _seenMoov = true;
                        break;
                    }
                    
                }
            } while(
                Mp4BoxHelper.IsBoxType(_mp4BoxBuffer, 0, "mfra") ||
                Mp4BoxHelper.IsBoxType(_mp4BoxBuffer, 0, "moov"));

            if (InsertBoxes && null != _ismFile && Mp4BoxHelper.IsBoxType(_mp4BoxBuffer, 0, "ftyp"))
            {
                // insert a live manifest
                uint flags = 0;
                XmlDocument d = new XmlDocument();
                string outerXml = _ismFile.ToString();
                byte[] extendedData = UnicodeEncoding.Unicode.GetBytes(outerXml);
                if (_mp4BoxBuffer.Length < (mp4BoxLen + extendedData.Length))
                {
                    // increase our internal buffers
                    byte[] newBuf = new byte[(mp4BoxLen + extendedData.Length) * 2]; // double it
                    Buffer.BlockCopy(_mp4BoxBuffer, 0, newBuf, 0, mp4BoxLen);
                    _mp4BoxBuffer = newBuf;
                }
                mp4BoxLen += Mp4BoxHelper.WriteExtendedBox(
                    _mp4BoxBuffer, LiveServerManBoxGuid,
                    0, flags,
                    extendedData, 
                    mp4BoxLen);
            }
            if (InsertBoxes && Mp4BoxHelper.IsBoxType(_mp4BoxBuffer, 0, "moof"))
            {
                int trafOffset = Mp4BoxHelper.FindChild(_mp4BoxBuffer, 0, "traf");
                if (-1 != trafOffset)
                {
                    // need to insert data here
                    byte[] extendedData = new byte[16];
                    uint flags = 0;
                    if (InsertNtp)
                    {
                        flags |= 8; // @@ TODO: not setting 8 crashes in MP4Parser
                    }
                    mp4BoxLen += Mp4BoxHelper.InsertExtendedBoxHead(
                        _mp4BoxBuffer, kTrackFragExtHeaderBoxGuid,
                        0, flags,
                        extendedData,
                        0, trafOffset);
                }
            }
            // update our new length
            _mp4BoxBufferLength = (int)mp4BoxLen;
            ret = (length < _mp4BoxBufferLength) ? length : _mp4BoxBufferLength;
            SourceCopyBits(0, buffer, 0, ret);
            _mp4BoxBufferOffset = ret;
            return ret;
        }

        private int OutputData(object sender, byte[] buffer, int length)
        {
            return 0;
        }

        public void WaitForCompletion()
        {
            _thread.Join();

        }
        public bool IsRunning()
        {
            System.Diagnostics.Debug.WriteLine("Status: " + _thread.ThreadState.ToString());
            if ((_thread.ThreadState == System.Threading.ThreadState.Running)||
                (_thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin))

                return true;
            else
                return false;
        }
    }
}
