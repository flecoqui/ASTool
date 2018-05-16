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
using System.Xml;
using System.IO;

namespace ASTool.ISMHelper
{
    public class SMIL20Stream
    {
        private XmlDocument _doc = new XmlDocument();
        private XmlNamespaceManager _mgr;
        protected SMIL20Stream(Stream s)
        {
            _doc.Load(s);
            _mgr = new XmlNamespaceManager(_doc.NameTable);
            _mgr.AddNamespace("smil", "http://www.w3.org/2001/SMIL20/Language");
        }

        protected XmlNodeList SelectNodes(string xpath)
        {
            return _doc.SelectNodes(xpath, _mgr);
        }

        public override string ToString()
        {
            return _doc.OuterXml;
        }
    }
}
