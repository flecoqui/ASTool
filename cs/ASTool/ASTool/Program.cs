using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using ASTool;
using ASTool.CacheHelper;
namespace ASTool
{

    public partial class Program
    {
        static Int32 Version = ASVersion.SetVersion(0x01, 0x00, 0x00, 0x00);

        static bool Route(Options opt)
        {
            bool result = false;

            DiskCache d = new DiskCache();
            d.Initialize("C:\\TEMP\\ASTOOL\\");
            ManifestCache mc = ManifestCache.CreateManifestCache(new Uri(opt.InputUri), true, -1, -1, 512000, 40);
            mc.SetDiskCache(d);
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
        static void Main(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Console.WriteLine("ASTool: Internal Error");
                return;
            }
            if(!string.IsNullOrEmpty(opt.GetErrorMessage()))
            {
                Console.WriteLine(opt.GetErrorMessagePrefix() + opt.GetErrorMessage());
                Console.WriteLine(opt.GetInformationMessage(Version));
                return;
            }
            if(opt.ASToolAction == Options.Action.Help)
            {
                Console.WriteLine(opt.GetInformationMessage(Version));
                return;
            }
            if (opt.ASToolAction == Options.Action.PullPush)
            {
                Route(opt);
                return;
            }
            if (opt.ASToolAction == Options.Action.Pull)
            {
                Pull(opt);
                return;
            }
            if (opt.ASToolAction == Options.Action.Push)
            {
                Push(opt);
                return;
            }
            if (opt.ASToolAction == Options.Action.PullPush)
            {
                PullPush(opt);
                return;
            }
            if (opt.ASToolAction == Options.Action.Parse)
            {
                Parse(opt);
                return;
            }


        }
    }
}
