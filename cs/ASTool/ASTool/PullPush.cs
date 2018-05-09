using System;
using System.Collections.Generic;
using System.Text;
using ASTool.CacheHelper;
namespace ASTool
{
    public partial class Program
    {
        static bool PullPush(Options opt)
        {
            bool result = false;


            SmoothLiveOutput d = new SmoothLiveOutput();
            if (d != null)
            {
                var t = d.Initialize(opt.OutputUri);
                t.Wait();
                if(t.Result == true)
                {
                    ManifestManager mc = ManifestManager.CreateManifestCache(new Uri(opt.InputUri), (ulong)opt.MinBitrate, (ulong)opt.MaxBitrate, opt.AudioTrackName, opt.TextTrackName, opt.Duration, (ulong)opt.BufferSize, 20);
                    if (mc != null)
                    {
                        if (mc.SetManifestOutput(d) == true)
                        {
                            t = mc.DownloadManifest();
                            t.Wait();
                            result = t.Result;
                            if (result == true)
                            {
                                var tt = mc.StartDownloadChunks();
                                tt.Wait();
                                result = tt.Result;
                                while (mc.GetAssetStatus() != AssetStatus.ChunksDownloaded)
                                {
                                    System.Threading.Tasks.Task.Delay(10000).Wait();

                                    foreach(ChunkList cl in mc.AudioChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        ulong numberofchunks = (ulong)cl.ChunksList.Count;
                                        ulong TotalChunks = cl.TotalChunks;
                                        Console.WriteLine("Audio source: " + source  + "\r\n  Number of chunks: " + numberofchunks.ToString() + "\r\n  TotalChunks: " + TotalChunks.ToString() +
                                            "\r\n  InputChunks: " + cl.InputChunks.ToString() + "\r\n  Outputchunks:  " + cl.OutputChunks.ToString());
                                    }
                                    foreach (ChunkList cl in mc.VideoChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        ulong numberofchunks = (ulong)cl.ChunksList.Count;
                                        ulong TotalChunks = cl.TotalChunks;
                                        Console.WriteLine("Video source: " + source + "\r\n  Number of chunks: " + numberofchunks.ToString() + "\r\n  TotalChunks: " + TotalChunks.ToString() +
                                            "\r\n  InputChunks: " + cl.InputChunks.ToString() + "\r\n  Outputchunks:  " + cl.OutputChunks.ToString());
                                    }
                                    foreach (ChunkList cl in mc.TextChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        ulong numberofchunks = (ulong)cl.ChunksList.Count;
                                        ulong TotalChunks = cl.TotalChunks;
                                        Console.WriteLine("Text source: " + source + "\r\n  Number of chunks: " + numberofchunks.ToString() + "\r\n  TotalChunks: " + TotalChunks.ToString() +
                                            "\r\n  InputChunks: " + cl.InputChunks.ToString() + "\r\n  Outputchunks:  " + cl.OutputChunks.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return result;
        }
    }
}
