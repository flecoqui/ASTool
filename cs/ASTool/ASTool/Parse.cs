using System;
using System.Collections.Generic;
using System.Text;
using ASTool.CacheHelper;
using ASTool.ISMHelper;
namespace ASTool
{
    public partial class Program
    {
        static bool Parse(Options opt)
        {
            bool result = false;
            Console.WriteLine("Parsing file: " + opt.InputUri);
            Console.Write(Mp4Box.ParseFile(opt.InputUri));
            Console.WriteLine("Parsing file: " + opt.InputUri + " done");
            return result;
        }
    }
}
