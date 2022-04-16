﻿using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class PropertyAttribute : Attribute
    {
        public const string Name = "Property";
    }
}