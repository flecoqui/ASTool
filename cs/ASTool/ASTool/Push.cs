using System;
using System.Collections.Generic;
using System.IO;
using ASTool.ISMHelper;
namespace ASTool
{
    public partial class Program
    {
        static bool Push(Options opt)
        {
            bool result = false;

            int AssetId = 4516;
            int streamId = 0;
            List<IsmPushEncoder> pushers = new List<IsmPushEncoder>();
            IsmPushEncoder encoder;

            FakeLOTTServer server = new FakeLOTTServer(opt.OutputUri);

            Console.WriteLine("Pubpoint url is {0} ", opt.OutputUri);
            if (!string.IsNullOrEmpty(opt.InputUri))
            {
                IsmFile ism = new IsmFile(File.OpenRead(opt.InputUri));
                if (ism.Tracks.Length > 0)
                {
                    string ismcFilePath = Path.Combine(Path.GetDirectoryName(opt.InputUri), ism.IsmcFilePath);
                    IsmcFile ismc = new IsmcFile(File.OpenRead(ismcFilePath));

                    Console.WriteLine("Pushing multiple streams from manifest file: {0} ", opt.InputUri);
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
                            ism.Tracks[i].TrackId);
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
            foreach (IsmPushEncoder pusher in pushers)
            {
                pusher.WaitForCompletion();
            }

            return result;
        }
    }
}
