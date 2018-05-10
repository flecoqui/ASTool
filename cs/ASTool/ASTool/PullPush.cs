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
            opt.LogInformation("ASTool starting...");
            opt.LogInformation("PullPush");
            opt.LogInformation("Pulling from : " + opt.InputUri);
            opt.LogInformation("Pushing to : " + opt.OutputUri);

            SmoothLiveOutput d = new SmoothLiveOutput();
            if (d != null)
            {
                var t = d.Initialize(opt.OutputUri);
                t.Wait();
                if(t.Result == true)
                {
                    ManifestManager mc = ManifestManager.CreateManifestCache(new Uri(opt.InputUri), (ulong)opt.MinBitrate, (ulong)opt.MaxBitrate, opt.AudioTrackName, opt.TextTrackName, opt.Duration, (ulong)opt.BufferSize, opt.LiveOffset);
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
                                    UInt64 InputChunks = 0;
                                    UInt64 OutputChunks = 0;
                                    UInt64 InputBytes = 0;
                                    UInt64 OutputBytes = 0;


                                    foreach (ChunkList cl in mc.AudioChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        opt.LogInformation("\r\nSource: " + source + 
                                            "\r\n  Number of chunks in InputQueue:  " + cl.ChunksToReadQueue.Count.ToString() + 
                                            "\r\n  Number of chunks in OutputQueue: " + cl.ChunksQueue.Count.ToString() + 
                                            "\r\n  Number of chunks to process:     " + cl.TotalChunks.ToString() +
                                            "\r\n  Input Chunks:  " + cl.InputChunks.ToString() +
                                            "\r\n  Input Bytes:   " + cl.InputBytes.ToString() +
                                            "\r\n  Output Chunks: " + cl.OutputChunks.ToString() +
                                            "\r\n  Output Bytes:  " + cl.OutputBytes.ToString()
                                            );
                                        InputChunks += cl.InputChunks;
                                        OutputChunks += cl.OutputChunks;
                                        InputBytes += cl.InputBytes;
                                        OutputBytes += cl.OutputBytes;

                                    }
                                    foreach (ChunkList cl in mc.VideoChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        opt.LogInformation("\r\nSource: " + source +
                                            "\r\n  Number of chunks in InputQueue:  " + cl.ChunksToReadQueue.Count.ToString() +
                                            "\r\n  Number of chunks in OutputQueue: " + cl.ChunksQueue.Count.ToString() +
                                            "\r\n  Number of chunks to process:     " + cl.TotalChunks.ToString() +
                                            "\r\n  Input Chunks:  " + cl.InputChunks.ToString() +
                                            "\r\n  Input Bytes:   " + cl.InputBytes.ToString() +
                                            "\r\n  Output Chunks: " + cl.OutputChunks.ToString() +
                                            "\r\n  Output Bytes:  " + cl.OutputBytes.ToString()
                                            );
                                        InputChunks += cl.InputChunks;
                                        OutputChunks += cl.OutputChunks;
                                        InputBytes += cl.InputBytes;
                                        OutputBytes += cl.OutputBytes;
                                    }
                                    foreach (ChunkList cl in mc.TextChunkListList)
                                    {
                                        string source = cl.Configuration.GetSourceName();
                                        opt.LogInformation("\r\nSource: " + source +
                                            "\r\n  Number of chunks in InputQueue:  " + cl.ChunksToReadQueue.Count.ToString() +
                                            "\r\n  Number of chunks in OutputQueue: " + cl.ChunksQueue.Count.ToString() +
                                            "\r\n  Number of chunks to process:     " + cl.TotalChunks.ToString() +
                                            "\r\n  Input Chunks:  " + cl.InputChunks.ToString() +
                                            "\r\n  Input Bytes:   " + cl.InputBytes.ToString() +
                                            "\r\n  Output Chunks: " + cl.OutputChunks.ToString() +
                                            "\r\n  Output Bytes:  " + cl.OutputBytes.ToString()
                                            );
                                        InputChunks += cl.InputChunks;
                                        OutputChunks += cl.OutputChunks;
                                        InputBytes += cl.InputBytes;
                                        OutputBytes += cl.OutputBytes;
                                    }
                                    opt.LogInformation("\r\nTotal: " + 
                                        "\r\n  Input Chunks:  " + InputChunks.ToString() +
                                        "\r\n  Input Bytes:   " + InputBytes.ToString() +
                                        "\r\n  Output Chunks: " + OutputChunks.ToString() +
                                        "\r\n  Output Bytes:  " + OutputBytes.ToString()
                                        );
                                }
                            }
                        }
                    }
                }
            }

            opt.LogInformation("PullPush done");
            return result;
        }
    }
}
