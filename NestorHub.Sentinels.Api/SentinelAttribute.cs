using System;
using System.Composition;
using NestorHub.Common.Api.Mef;

namespace NestorHub.Sentinels.Api
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SentinelAttribute : ExportAttribute
    {
        public string Package { get; set; }

        public SentinelAttribute(string package) : base(package, typeof(ISentinel))
        {
            Package = package;
        }
    }
}
