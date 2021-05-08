﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ds.enovia.document.model
{
    public class Document : DocumentCreated
    {
        public string identifier { get; set; }
        public string source { get; set; }
        public string relativePath { get; set; }
        public string relId { get; set; }
        public new DocumentDataElements dataelements { get; set; }
    }
}
