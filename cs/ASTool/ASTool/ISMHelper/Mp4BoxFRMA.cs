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
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxFRMA : Mp4Box
    {
        static public Mp4BoxFRMA CreateFRMABox(string message)
        {

            Mp4BoxFRMA box = new Mp4BoxFRMA();
            if ((box != null)&&(!string.IsNullOrEmpty(message)))
            {
                
                box.Length = 8 + 4  ;
                box.Type = "mfra";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxString(Buffer, 0, message);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
