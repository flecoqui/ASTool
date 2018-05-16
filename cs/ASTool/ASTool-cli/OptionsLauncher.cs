using ASTool;
using ASTool_cli;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASTool_cli
{
    public class OptionsLauncher
    {
        public delegate bool OptionsThread(Options opt);
        public Options Option;
        public OptionsThread Proc;

        public OptionsLauncher(OptionsThread t, Options opt)
        {
            Proc = t;
            Option = opt;
        }
        public void ThreadEntry()
        {
            if ((Proc != null) && (Option != null))
            {
                Proc(Option);
            }
        }
        public static bool LaunchThread(OptionsThread ot, Options o)
        {
            bool result = false;
            OptionsLauncher ol = new OptionsLauncher(ot, o);
            System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(ol.ThreadEntry);

            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(ol.ThreadEntry));
            if (t != null)
            {
                t.Start();
                result = true;
            }

            return result;

        }

    }
}
