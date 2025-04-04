﻿using System.Collections.Generic;

namespace SourceGenerator.Library.Template
{
    public class AutoServiceModel
    {
        public List<AutoServiceModelItem> ClassList { get; set; }
    }

    public class AutoServiceModelItem
    {
        public string Class { get; set; }

        public string Type { get; set; }
    }
}