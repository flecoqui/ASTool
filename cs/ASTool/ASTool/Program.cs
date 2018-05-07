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
                PullPush(opt);
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
