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
namespace ASTool.CacheHelper
{



    [DataContract(Name = "ChunkListConfiguration")]
    class ChunkListConfiguration 
    {
        [DataMember]
        public int TrackID { get; set; }
        [DataMember]
        public int TimeScale { get; set; }
        [DataMember]
        public long Duration { get; set; }
        [DataMember]
        public int Bitrate { get; set; }
        [DataMember]
        public string Language { get; set; }

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
    }
    [DataContract(Name = "VideoChunkListConfiguration")]
    class VideoChunkListConfiguration : ChunkListConfiguration
    {
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public string CodecPrivateData { get; set; }
        public override  byte[] GetMOOVData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateVideoMOOVBox((Int16) TrackID, (Int16)Width, (Int16)Height, TimeScale, Duration, Language, CodecPrivateData);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "AudioChunkListConfiguration")]
    class AudioChunkListConfiguration : ChunkListConfiguration
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

        public override byte[] GetMOOVData()
        {
            ISMHelper.Mp4Box box = ISMHelper.Mp4Box.CreateAudioMOOVBox((Int16)TrackID,MaxFramesize,Bitrate,BitsPerSample,SamplingRate,Channels, TimeScale, Duration, Language, CodecPrivateData);
            if (box != null)
                return box.GetBoxBytes();
            return null;
        }
    }
    [DataContract(Name = "TextChunkListConfiguration")]
    class TextChunkListConfiguration : ChunkListConfiguration
    {
    }
    [DataContract(Name = "ChunkList")]
    class ChunkList : IDisposable
    {

        /// <summary>
        /// Chunks 
        /// The number of chunks for this asset
        /// </summary>
        [DataMember]
        public ulong Chunks { get; set; }

        /// <summary>
        /// DownloadedChunks 
        /// The number of chunks downloaded
        /// </summary>
        [DataMember]
        public ulong DownloadedChunks { get; set; }

        /// <summary>
        /// ArchivedChunks 
        /// The number of chunks in the archive
        /// </summary>
        [DataMember]
        public ulong ArchivedChunks { get; set; }

        /// <summary>
        /// DownloadedChunks 
        /// The number of bytes downloaded
        /// </summary>
        [DataMember]
        public ulong DownloadedBytes { get; set; }

        /// <summary>
        /// ArchivedChunks 
        /// The number of bytes in the archive
        /// </summary>
        [DataMember]
        public ulong ArchivedBytes { get; set; }

        /// <summary>
        /// ChunksList 
        /// List of the chunks to download  
        /// </summary>
        [DataMember]
        public List<ChunkCache> ChunksList { get; set; }


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
        /// Bitrate 
        /// Bitrate of the track  
        /// </summary>
        [DataMember]
        public int Bitrate { get; set; }



        /// <summary>
        /// Chunklist Configuration 
        /// Contains information to create moov and ftyp chunks
        /// </summary>
        [DataMember]
        public ChunkListConfiguration Configuration { get; set; }


        /// <summary>
        /// moovData 
        /// Contains the moov box for this chunk list
        /// </summary>
        [DataMember]
        public byte[] moovData;

        /// <summary>
        /// moovData 
        /// Contains the ftyp box for this chunk list
        /// </summary>
        [DataMember]
        public byte[] ftypData;


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
            return (Chunks == DownloadedChunks);
        }
        public bool AreChunksArchived()
        {
            return (Chunks == ArchivedChunks);
        }

        public ChunkList() {
            ChunksList = new List<ChunkCache>();
        }


        public void Dispose()
        {
            if(ChunksList!=null)
            {
                foreach(var c  in ChunksList)
                {
                    c.Dispose();
                }
                ChunksList.Clear();
            }
        }
    }
}
