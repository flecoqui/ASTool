using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace ASTool
{
    public class PluginDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Int32 version { get; set; }
        public string Path { get; set; }
    }
    class Program
    {
        static Dictionary<string, PluginDescription> InputPlugins;
        static Dictionary<string, PluginDescription> OutputPlugins;

        static dynamic GetPluginInstance(string TypeName, string Name, string path)
        {
            dynamic myInstance;
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (myAssembly != null)
            {
                var myInterface = myAssembly.GetType(TypeName);
                if ((myInterface != null))
                {
                    foreach (TypeInfo type in myAssembly.DefinedTypes)
                    {
                        try
                        {
                            var myType = myAssembly.GetType(type.FullName);
                            if (myType != null)
                            {
                                myInstance = Activator.CreateInstance(myType);
                                if (myInstance != null)
                                {
                                    string name = myInstance.GetPluginName();
                                    if (name == Name)
                                        return myInstance;
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            return null;
        }
        static bool GetPluginName(string TypeName, string path, out string Name, out string Description, out Int32 version)
        {
            Name = string.Empty;
            Description = string.Empty;
            version = 0;
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (myAssembly != null)
            {
                var myInterface = myAssembly.GetType(TypeName);
                if ((myInterface != null))
                {
                    foreach (TypeInfo type in myAssembly.DefinedTypes)
                    {
                        try
                        {
                            var myType = myAssembly.GetType(type.FullName);
                            if (myType != null)
                            {
                                dynamic myInstance = Activator.CreateInstance(myType);
                                if (myInstance != null)
                                {
                                    Name = myInstance.GetPluginName();
                                    Description = myInstance.GetPluginDescription();
                                    version = myInstance.GetPluginVersion();
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                        if ((!string.IsNullOrEmpty(Name)) &&
                            (!string.IsNullOrEmpty(Description)) &&
                            version>0 )
                            return true;
                    }
                }
            }
            return false;
        }
        static Dictionary<string, PluginDescription> GetPlugins(string path, string TypeName)
        {
            Dictionary<string, PluginDescription> result = new Dictionary<string, PluginDescription>();
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
                    string PluginName = string.Empty;
                    string PluginDescription = string.Empty;
                    Int32 version = 0;

                    if (GetPluginName(TypeName, dir,out PluginName, out PluginDescription, out version) ==true)
                    {
                        PluginDescription desc = new PluginDescription();
                        desc.Name = PluginName;
                        desc.Description = PluginDescription;
                        desc.version = version;
                        desc.Path = dir;
                        result.Add(PluginName, desc);
                    }
                }
            }
            return result;
        }
        static void LoadPlugins(string path)
        {
            InputPlugins = GetPlugins(path, "ASTool.ASInput");
            OutputPlugins = GetPlugins(path, "ASTool.ASOutput");
        }
        static void ListPlugins(string path)
        {
            Console.WriteLine("Loading Input Plugins");
            if ((InputPlugins != null) && (InputPlugins.Count > 0))
            {
                Console.WriteLine(InputPlugins.Count.ToString() + " Input Plugins found");
                foreach (var value in InputPlugins)
                {
                    Console.WriteLine("Plugin Name: " + value.Key + " version: " + ASVersion.GetVersionString(value.Value.version) + " path: " + value.Value.Path);
                }
            }
            else
                Console.WriteLine("0 Output Plugins found");

            Console.WriteLine("Loading Output Plugins");
            if ((OutputPlugins != null) && (OutputPlugins.Count > 0))
            {
                Console.WriteLine(OutputPlugins.Count.ToString() + " Output Plugins found");
                foreach (var value in OutputPlugins)
                {
                    Console.WriteLine("Plugin Name: " + value.Key + " version: " + ASVersion.GetVersionString(value.Value.version) + " path: " + value.Value.Path);
                }
            }
            else
                Console.WriteLine("0 Output Plugins found");
        }
        static bool RouteByByte(Options opt, dynamic InputInstance, dynamic OutputInstance)
        {
            bool result = false;
            int StreamCount = InputInstance.GetInputStreamCount();
            if (StreamCount > 0)
            {
                if (OutputInstance.SetOuputStreamCount(StreamCount))
                {
                    for (int i = 0; i < StreamCount; i++)
                    {
                        int TrackCount = InputInstance.GetInputStreamTrackCount(i);
                        if (TrackCount > 0)
                        {
                            if (!OutputInstance.SetOuputStreamTrackCount(StreamCount, TrackCount))
                                return result;
                        }
                    }

                    // ready to copy
                    Int64 WriteIndex = 0;
                    for (int i = 0; i < StreamCount; i++)
                    {
                        int TrackCount = InputInstance.GetInputStreamTrackCount(i);
                        if (TrackCount > 0)
                        {
                            for (int j = 0; j < TrackCount; j++)
                            {
                                byte[] buffer = InputInstance.GetInputDataByRange(i, j, WriteIndex, opt.ByteBufferSize);
                                if (buffer != null)
                                    OutputInstance.SetOutputDataByRange(i, j, opt.ByteBufferSize, buffer);
                            }
                        }
                    }


                }
            }
            return result;
        }
        static bool Route(Options opt)
        {
            bool result = false;
            if( InputPlugins.ContainsKey(opt.InputPluginName) &&
                OutputPlugins.ContainsKey(opt.OutputPluginName))
            {
                dynamic InputInstance = GetPluginInstance("ASTool.ASInput", opt.InputPluginName, InputPlugins[opt.InputPluginName].Path);
                dynamic OutputInstance = GetPluginInstance("ASTool.ASOutput", opt.OutputPluginName, OutputPlugins[opt.OutputPluginName].Path);
                if((InputInstance != null)&&
                   (OutputInstance != null))
                {
                    // Loading the Input
                    if(InputInstance.LoadInput(opt.InputUri))
                    {
                        if (OutputInstance.LoadOutput(opt.OutputUri))
                        {
                            ASIndexType InputIndexType = InputInstance.GetInputIndexType();
                            ASIndexType OutputIndexType = OutputInstance.GetOutputIndexType();
                            if(OutputIndexType == InputIndexType)
                            {
                                if(OutputIndexType == ASIndexType.Byte)
                                {
                                   return RouteByByte(opt, InputInstance, OutputInstance);
                                }
                                else if (OutputIndexType == ASIndexType.Time)
                                {

                                }
                                else if (OutputIndexType == ASIndexType.ByteAndTime)
                                {

                                }

                            }
                            else
                                Console.WriteLine("Input and Output are not using the same index type (Time, Byte,...)");

                        }
                    }
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
                ListPlugins(opt.PluginDirectory);
                return;
            }
            if (opt.ASToolAction == Options.Action.Route)
            {
                LoadPlugins(opt.PluginDirectory);
                Route(opt);
                return;
            }


        }
    }
}
