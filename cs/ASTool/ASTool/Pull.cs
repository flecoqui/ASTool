using System;
using System.Collections.Generic;
using System.Text;
using ASTool.CacheHelper;
namespace ASTool
{
    public partial class Program
    {
        static bool Pull(Options opt)
        {
            bool result = false;

            DiskCache d = new DiskCache();
            d.Initialize(opt.OutputUri);
            ManifestManager mc = ManifestManager.CreateManifestCache(new Uri(opt.InputUri), (ulong) opt.MinBitrate, (ulong)opt.MaxBitrate, opt.AudioTrackName, opt.TextTrackName, opt.Duration,(ulong) opt.BufferSize, 20);
            mc.SetManifestOutput(d);
            var t = d.RemoveAsset(mc);
            t.Wait();
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


            return result;
        }
    }
}
