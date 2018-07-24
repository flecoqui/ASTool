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
using System.Runtime.Serialization;
using System.Collections.Concurrent;
namespace ASTool
{

    // class used to store the offset of each moof box in the file 
    // based on the  time associated with the moof box
    public class TimeMoofOffset
    {
        public TimeMoofOffset(ulong Time, ulong Offset)
        {
            time = Time;
            offset = Offset;
        }
        public ulong time;
        public ulong offset;
    }

    [DataContract(Name = "ChunkListConfiguration")]
    public class ChunkListConfiguration 
    {
        public ChunkListConfiguration()
        {
            CreateTimeOffsetList();
        }
        [DataMember]
        public int TrackID { get; set; }
        [DataMember]
        public string FourCC { get; set; }
        [DataMember]
        public int TimeScale { get; set; }
        [DataMember]
        public long Duration { get; set; }
        [DataMember]
        public int Bitrate { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string TrackName { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public string CustomAttributes { get; set; }

        /// <summary>
        /// Get Protection Guid.
        /// </summary>
        [DataMember]
        public Guid ProtectionGuid { get; set; }
        /// <summary>
        /// Get Protection Data.
        /// </summary>
        [DataMember]
        public string ProtectionData { get; set; }

        public byte[] GetFTYPData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateFTYPBox();
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
        public virtual byte[] GetMOOVData()
        {
            return null;
        }
        public virtual string GetSourceName()
        {
            return Source + "_" + Bitrate.ToString() + "_" + (string.IsNullOrEmpty(TrackName) ? "unknown" :TrackName.ToString() );
        }
        protected List<TimeMoofOffset> ListTimeOffset;
        public bool CreateTimeOffsetList()
        {
            ListTimeOffset = new List<TimeMoofOffset>();
            return ListTimeOffset != null;
        }
        public bool AddTimeOffset(ulong Time, ulong Offset)
        {
            try
            {
                if (ListTimeOffset != null)
                    ListTimeOffset.Add(new TimeMoofOffset(Time, Offset));
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }
        public virtual byte[] GetMFRAData(ulong OffsetWithVideo)
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateMFRABox((Int16)TrackID,ListTimeOffset, OffsetWithVideo);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "VideoChunkListConfiguration")]
    public class VideoChunkListConfiguration : ChunkListConfiguration
    {
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public string CodecPrivateData { get; set; }
        public override  byte[] GetMOOVData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateVideoMOOVBox((Int16) TrackID, (Int16)Width, (Int16)Height, TimeScale, Duration, Language, CodecPrivateData,ProtectionGuid,ProtectionData);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "AudioChunkListConfiguration")]
    public class AudioChunkListConfiguration : ChunkListConfiguration
    {
        [DataMember]
        public int BitsPerSample { get; set; }
        [DataMember]
        public int Channels { get; set; }
        [DataMember]
        public int SamplingRate { get; set; }
        [DataMember]
        public string CodecPrivateData { get; set; }
        [DataMember]
        public int MaxFramesize { get; set; }

        [DataMember]
        public string AudioTag { get; set; }
        [DataMember]
        public int PacketSize { get; set; }


        public override byte[] GetMOOVData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateAudioMOOVBox((Int16)TrackID,MaxFramesize,Bitrate,BitsPerSample,SamplingRate,Channels, TimeScale, Duration, Language, CodecPrivateData, ProtectionGuid, ProtectionData);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "TextChunkListConfiguration")]
    public class TextChunkListConfiguration : ChunkListConfiguration
    {
        public override byte[] GetMOOVData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateTextMOOVBox((Int16)TrackID, TimeScale,Duration,Language, ProtectionGuid, ProtectionData);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "ChunkList")]
    public class ChunkList : IDisposable
    {
        public object ListLock;
        /// <summary>
        /// LastDataOffset
        /// Last Data Offset used to build the mfra box
        /// </summary>
        public ulong LastDataOffset { get; set; }

        /// <summary>
        /// Chunks 
        /// The number of chunks for this asset
        /// </summary>
        [DataMember]
        public ulong TotalChunks { get; set; }

        /// <summary>
        /// InputChunks 
        /// The number of chunks downloaded
        /// </summary>
        [DataMember]
        public ulong InputChunks { get; set; }

        /// <summary>
        /// OutputChunks 
        /// The number of chunks in the archive
        /// </summary>
        [DataMember]
        public ulong OutputChunks { get; set; }

        /// <summary>
        /// InputChunks 
        /// The number of bytes downloaded
        /// </summary>
        [DataMember]
        public ulong InputBytes { get; set; }

        /// <summary>
        /// OutputChunks 
        /// The number of bytes in the archive
        /// </summary>
        [DataMember]
        public ulong OutputBytes { get; set; }


        /// <summary>
        /// ChunksToReadQueue 
        /// List of the chunks to read  
        /// </summary>
        
        public ConcurrentQueue<ChunkBuffer> ChunksToReadQueue { get; set; }

        /// <summary>
        /// LastTimeChunksToRead 
        /// The Time of the last Chunk to read   
        /// </summary>
        public ulong LastTimeChunksToRead { get; set; }

        /// <summary>
        /// Time of the first Chunk downloaded 
        /// </summary>
        public ulong TimeFirstChunk { get; set; }

        /// <summary>
        /// Time of the last Chunk downloaded 
        /// </summary>
        public ulong TimeLastChunk { get; set; }

        /// <summary>
        /// ChunksQueue 
        /// List of the chunks read  
        /// </summary>
        public ConcurrentQueue<ChunkBuffer> ChunksQueue { get; set; }

        /// <summary>
        /// Original TemplateUrl 
        /// The Url template to download  the chunks
        /// </summary>
        [DataMember]
        public string OriginalTemplateUrl { get; set; }
        /// <summary>
        /// TemplateUrl 
        /// The Url template to download  the chunks
        /// </summary>
        [DataMember]
        public string TemplateUrl { get; set; }
        /// <summary>
        /// TemplateUrlType 
        /// Type in the Url template to download  the chunks
        /// </summary>
        [DataMember]
        public string TemplateUrlType { get; set; }

        /// <summary>
        /// Amount of memory used to store chunks
        /// </summary>
        public ulong MemorySizeForChunks { get; set; }

        /// <summary>
        /// Bitrate 
        /// Bitrate of the track  
        /// </summary>
        [DataMember]
        public int Bitrate { get; set; }



        /// <summary>
        /// Chunklist Configuration 
        /// Contains information to create moov and ftyp chunks
        /// </summary>
        
        public ChunkListConfiguration Configuration { get; set; }


        /// <summary>
        /// moovData 
        /// Contains the moov box for this chunk list
        /// </summary>
        
        public byte[] moovData;

        /// <summary>
        /// ftypData 
        /// Contains the ftyp box for this chunk list
        /// </summary>

        public byte[] ftypData;


        /// <summary>
        /// mfraData 
        /// Contains the mfra box for this chunk list, the mfra box is inserted at the end of the chunk list.
        /// </summary>
        public byte[] mfraData;

        
        /// <summary>
        /// GetType
        /// Get Type from the url template.
        /// </summary>
        /// <param name="source">Source url</param>
        /// <returns>string Type included in the source url</returns>
        public static string GetType(string Template)
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

        public bool AreChunksDownloaded()
        {
            return (TotalChunks == InputChunks);
        }
        public bool AreChunksArchived()
        {
            return (TotalChunks == OutputChunks);
        }

        public ChunkList() {
            ChunksToReadQueue = new ConcurrentQueue<ChunkBuffer>();
            LastTimeChunksToRead = 0;
            TimeFirstChunk = 0;
            TimeLastChunk = 0;
            ChunksQueue = new ConcurrentQueue<ChunkBuffer>();
            MemorySizeForChunks = 0;
            ListLock = new object();
        }


        public void Dispose()
        {

            if(ChunksToReadQueue != null)
            {
                foreach(var c  in ChunksToReadQueue)
                {
                    c.Dispose();
                }
                ChunksToReadQueue.Clear();
            }
            if (ChunksQueue != null)
            {
                foreach (var c in ChunksQueue)
                {
                    c.Dispose();
                }
                ChunksQueue.Clear();
            }
            MemorySizeForChunks = 0;

        }
    }
}
