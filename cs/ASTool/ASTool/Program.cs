using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace ASTool
{
    class Program
    {
        static Dictionary<string, string> InputPlugins;
        static Dictionary<string, string> OutputPlugins;

        static string GetPluginName(string TypeName, string path)
        {
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (myAssembly != null)
            {
                var myInterface = myAssembly.GetType(TypeName);
                if ((myInterface != null))
                {
                    foreach (TypeInfo type in myAssembly.DefinedTypes)
                    {
                        string Name = string.Empty;
                        try
                        {
                            var myType = myAssembly.GetType(type.FullName);
                            if (myType != null)
                            {
                                dynamic myInstance = Activator.CreateInstance(myType);
                                if (myInstance != null)
                                    Name = myInstance.GetInputPluginName();
                            }
                        }
                        catch (Exception)
                        {

                        }
                        if (!string.IsNullOrEmpty(Name))
                            return Name;
                    }
                }
            }
            return string.Empty;
        }
        static Dictionary<string, string> GetPlugins(string path, string TypeName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string directory = null;
            if (string.IsNullOrEmpty(path))
                directory = Directory.GetCurrentDirectory();
            else
                directory = path;
            if(!string.IsNullOrEmpty(directory))
            {
                
                List<string> dirs = new List<string>(Directory.EnumerateFiles(directory,"*.dll"));
                foreach (var dir in dirs)
                {
                    string PluginName = GetPluginName(TypeName, dir);
                    if(!string.IsNullOrEmpty(PluginName))
                    {
                        result.Add(PluginName, dir);
                    }
                }
            }
            return result;
        }
        static void LoadPlugins(string path)
        {
            Console.WriteLine("Loading Input Plugins");
            InputPlugins = GetPlugins(path,"ASTool.ASInput");
            if ((InputPlugins != null) && (InputPlugins.Count > 0))
            {
                Console.WriteLine(InputPlugins.Count.ToString() + " Input Plugins found");
                foreach (var value in InputPlugins)
                {
                    Console.WriteLine("Plugin Name: " + value.Key + " path: " + value.Value);
                }
            }
            else
                Console.WriteLine("0 Output Plugins found");

            Console.WriteLine("Loading Output Plugins");
            OutputPlugins = GetPlugins(path,"ASTool.ASOutput");
            if ((OutputPlugins != null) && (OutputPlugins.Count > 0))
            {
                Console.WriteLine(OutputPlugins.Count.ToString() + " Output Plugins found");
                foreach (var value in OutputPlugins)
                {
                    Console.WriteLine("Plugin Name: " + value.Key + " path: " + value.Value);
                }
            }
            else
                Console.WriteLine("0 Output Plugins found");
        }
        static void Main(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Console.WriteLine("ASTool: Internal Error");
                return;
            }
            if(!string.IsNullOrEmpty(opt.ErrorMessage))
            {
                Console.WriteLine(opt.ErrorMessagePrefix + opt.ErrorMessage);
                Console.WriteLine(opt.InformationMessage);
                return;
            }
            if(opt.ASToolAction == Options.Action.Help)
            {
                Console.WriteLine(opt.InformationMessage);
                return;
            }
            if (opt.ASToolAction == Options.Action.ListPlugin)
            {
                LoadPlugins(opt.PluginDirectory);
                return;
            }
            if (opt.ASToolAction == Options.Action.Route)
            {
                LoadPlugins(opt.PluginDirectory);
                return;
            }


        }
    }
}
