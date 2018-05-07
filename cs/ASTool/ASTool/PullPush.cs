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
                                    System.Threading.Tasks.Task.Delay(1000);
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
