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
    public class Mp4BoxFTYP : Mp4Box
    {
        static public Mp4BoxFTYP CreateFTYPBox(string major_brand, Int32 minor_version, List<string> compatible_brands)
        {
            if ((!string.IsNullOrEmpty(major_brand) && (major_brand.Length == 4)) &&
                (compatible_brands != null) && (compatible_brands.Count > 0)
                )
            {
                Mp4BoxFTYP box = new Mp4BoxFTYP();
                if (box != null)
                {
                    box.Length = 8 + 8 + compatible_brands.Count * 4;
                    box.Type = "ftyp";
                    byte[] Buffer = new byte[8 + compatible_brands.Count * 4];
                    if (Buffer != null)
                    {
                        WriteMp4BoxString(Buffer, 0, major_brand, 4);
                        WriteMp4BoxInt32(Buffer, 4, minor_version);
                        for (int i = 0; i < compatible_brands.Count; i++)
                            WriteMp4BoxString(Buffer, 8 + 4 * i, compatible_brands[i], 4);
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
