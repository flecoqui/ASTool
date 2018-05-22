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
using ASTool.ISMHelper;
namespace ASTool
{
    public partial class Program
    {
        static void CreatePushCounters(Options opt, List<IsmPushEncoder> pushers, string source)
        {
            UInt64 OutputChunks = 0;
            UInt64 OutputBytes = 0;

            foreach (IsmPushEncoder cl in pushers)
            {
                opt.SetCounter(cl.GetSourceName() + "_Source", "Counters for source", cl.GetSourceName(), string.Empty, "Counters for source");
                opt.SetCounter(cl.GetSourceName() + "_OutputChunks", "Number of Output Chunks", cl.OutputChunks, string.Empty, "Number of Output Chunks");
                opt.SetCounter(cl.GetSourceName() + "_OutputBytes", "Number of Output Bytes", cl.OutputBytes, string.Empty, "Number of Output Bytes");
                OutputChunks += cl.OutputChunks;
                OutputBytes += cl.OutputBytes;
            }
            
            opt.SetCounter(source + "_Source", "Total Counters for source", opt.InputUri, string.Empty, "Total Counters for source");
            opt.SetCounter(source + "_OutputChunks", "Total Number of Output Chunks", OutputChunks, string.Empty, "Total Number of Output Chunks");
            opt.SetCounter(source + "_OutputBytes", "Total Number of Output Bytes", OutputBytes, string.Empty, "Total Number of Output Bytes");
            opt.SetCounter(source + "_Bitrate", "Current input bitrate", (int)(OutputBytes * 8 / (DateTime.Now - opt.ThreadStartTime).TotalSeconds), "b/s", "Current input bitrate");

        }
        static void UpdatePushCounters(Options opt, List<IsmPushEncoder> pushers, string source)
        {
            UInt64 OutputChunks = 0;
            UInt64 OutputBytes = 0;

            foreach (IsmPushEncoder cl in pushers)
            {
                opt.SetCounter(cl.GetSourceName() + "_OutputChunks", cl.OutputChunks);
                opt.SetCounter(cl.GetSourceName() + "_OutputBytes", cl.OutputBytes);
                OutputChunks += cl.OutputChunks;
                OutputBytes += cl.OutputBytes;
            }
            opt.SetCounter(source + "_Source", opt.InputUri);
            opt.SetCounter(source + "_OutputChunks", OutputChunks);
            opt.SetCounter(source + "_OutputBytes", OutputBytes);
            opt.SetCounter(source  + "_Bitrate", (int)(OutputBytes * 8 / (DateTime.Now - opt.ThreadStartTime).TotalSeconds));

        }

        static bool Push(Options opt)
        {
            bool result = false;
            opt.Status = Options.TheadStatus.Running;
            opt.ThreadStartTime = DateTime.Now;
            opt.ThreadCounterTime = DateTime.Now;


            opt.LogInformation("\r\nPush " + opt.Name + "\r\n Pushing from : " + opt.InputUri + "\r\n Pushing to   : " + opt.OutputUri);

            int AssetId = 4516;
            int streamId = 0;
            IsmPushEncoder encoder;
            List<IsmPushEncoder> pushers = new List<IsmPushEncoder>();
            FakeLOTTServer server = new FakeLOTTServer(opt.OutputUri);
            IsmFile ism = null;
            if (!string.IsNullOrEmpty(opt.InputUri))
            {
                ism = new IsmFile(File.OpenRead(opt.InputUri));
                if (ism.Tracks.Length > 0)
                {
                    string ismcFilePath = Path.Combine(Path.GetDirectoryName(opt.InputUri), ism.IsmcFilePath);
                    IsmcFile ismc = new IsmcFile(File.OpenRead(ismcFilePath));

                    for (int i = 0/*ism.Tracks.Length -2*/; i < ism.Tracks.Length; i++)
                    {
                        string ismvFile = Path.Combine(Path.GetDirectoryName(opt.InputUri), ism.Tracks[i].Source);
                        string LiveManifest = string.Empty;
                        if (ism.Tracks[i].MediaType == IsmTrackType.audio)
                            LiveManifest = ism.GetAudioManifest(ism.Tracks[i].TrackId, ism.Tracks[i].TrackName, ism.Tracks[i].Bitrate, ism.Tracks[i].Source, ismc);
                        if (ism.Tracks[i].MediaType == IsmTrackType.video)
                            LiveManifest = ism.GetVideoManifest(ism.Tracks[i].TrackId, ism.Tracks[i].TrackName, ism.Tracks[i].Bitrate, ism.Tracks[i].Source, ismc);
                        if (ism.Tracks[i].MediaType == IsmTrackType.textstream)
                            LiveManifest = ism.GetTextManifest(ism.Tracks[i].TrackId, ism.Tracks[i].TrackName, ism.Tracks[i].Bitrate, ism.Tracks[i].Source, ismc);
                        ISMHelper.Chunk[] ChunkArray = ism.GetChunkList(ism.Tracks[i].TrackName, ismc);
                        encoder = new IsmPushEncoder(
                            server,
                            ism,
                            ismvFile,
                            opt.OutputUri,
                            AssetId,
                            streamId++,
                            ism.Tracks[i].Bitrate,
                            LiveManifest,
                            ChunkArray,
                            ism.Tracks[i].TrackId,
                            opt);
                        encoder.InsertNtp = true;
                        encoder.InsertBoxes = true;
                        encoder.Loop = (opt.Loop == 0);
                        pushers.Add(encoder);
                    }
                }
            }

            foreach (IsmPushEncoder pusher in pushers)
            {
                pusher.Start();
            }
            bool IsRunning = true;
            while (IsRunning)
            {
                if (opt.CounterPeriod > 0)
                {
                    System.Threading.Tasks.Task.Delay(opt.CounterPeriod * 1000 / 10).Wait();
                    if ((opt.ListCounters == null) || (opt.ListCounters.Count == 0))
                        CreatePushCounters(opt, pushers, ism.IsmcFilePath);
                    else
                        UpdatePushCounters(opt, pushers, ism.IsmcFilePath);
                }
                else
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                IsRunning = false;
                foreach (IsmPushEncoder pusher in pushers)
                {
                    if (pusher.IsRunning() == true)
                    { 
                        IsRunning = true;
                        break;
                    }
                }

            }
            opt.LogInformation("Push " + opt.Name + " done");
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
