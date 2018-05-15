//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTool.SmoothHelper;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
namespace ASTool
{

    public interface ManifestOutput 
    {
        /// <summary>
        /// Initialize
        /// Initialize the output to receive the audio/video/text chunks based on the manifest content
        /// </summary>
        Task<bool> ProcessManifest(ManifestManager manifest);
        /// <summary>
        /// Process
        /// Process the received the audio/video/text chunks
        /// </summary>
        Task<bool> ProcessChunks(ManifestManager manifest);
    }
}
