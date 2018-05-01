using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ASTool.ISMHelper
{
    public class FakeLOTTServer : IDisposable
    {
        private Dictionary<string, string> _pubPointsToPhysFile = new Dictionary<string, string>();
        public FakeLOTTServer(string mp4ProcessorPath)
        {

            string cwd = Environment.CurrentDirectory;

        }

        public void AddPublishingPoint(string publishingPoint)
        {
            string tmpIsmlFile = Path.GetTempFileName();
            using (StreamWriter sw = File.CreateText(tmpIsmlFile))
            {
                sw.Write(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<smil xmlns=\"http://www.w3.org/2001/SMIL20/Language\">" +
                        "<head>" +
                            "<meta name=\"title\" content=\"PushEncoder.isml\" />" +
                            "<meta name=\"module\" content=\"liveSmoothStreaming\" />" +
                            "<meta name=\"sourceType\" content=\"Push\" />" +
                            "<meta name=\"publishing\" content=\"Fragments;Streams;Archives\" />" +
                            "<meta name=\"estimatedTime\" content=\"0\" />" +
                            "<meta name=\"lookaheadChunks\" content=\"2\" />" +
                            "<meta name=\"manifestWindowLength\" content=\"0\" />" +
                            "<meta name=\"startOnFirstRequest\" content=\"False\" />" +
                            "<meta name=\"archiveSegmentLength\" content=\"0\" />" +
                            "<meta name=\"restartOnEncoderReconnect\" content=\"true\" />" +
                        "</head>" +
                        "<body>" +
                        "</body>" +
                    "</smil>");
            }
            Uri url = new Uri(publishingPoint);
            DateTime now = DateTime.Now;
            lock (_pubPointsToPhysFile)
            {
                _pubPointsToPhysFile.Add(url.LocalPath.ToLower(), tmpIsmlFile);
            }
        }

        public void Start()
        {
        }

/*        public void Post(string url, IisDataHandler onInputData, IisDataHandler onOutputData)
        {
            Uri uri = new Uri(url);
            string physFile;
            lock (_pubPointsToPhysFile)
            {
                _pubPointsToPhysFile.TryGetValue(uri.LocalPath.ToLower(), out physFile);
            }
        }
        */
        #region IDisposable Members

        public void Dispose()
        {
            lock (_pubPointsToPhysFile)
            {
                foreach (string pubPoint in _pubPointsToPhysFile.Keys)
                {
                    File.Delete(_pubPointsToPhysFile[pubPoint]);
                }
                _pubPointsToPhysFile.Clear();
            }
        }

        #endregion
    }
}
