using System.ComponentModel;
using System.ComponentModel.Composition;

namespace XMagnetSearch
{
    public interface IMetadata
    {
        string Source { get; }
        string Description { get; }
        string Version { get; }
        bool Enable { get; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SearchMetadata : ExportAttribute, IMetadata
    {
        [DefaultValue("Unknown")]
        public string Source { get; private set; } = string.Empty;
        [DefaultValue("Unknown")]
        public string Description { get; private set; } = string.Empty;
        [DefaultValue("Unknown")]
        public string Version { get; private set; } = string.Empty;
        [DefaultValue("Unknown")]
        public bool Enable { get; private set; } = true;
        public SearchMetadata()
        {
        }

        public SearchMetadata(string source) : this()
        {
            Source = source;
        }

        public SearchMetadata(string source, string description) : this(source)
        {
            Description = description;
        }

        public SearchMetadata(string source, string description, string version) : this(source, description)
        {
            Version = version;
        }
        public SearchMetadata(string source, string description, string version, bool enable) : this(source, description, version)
        {
            Enable = enable;
        }
    }
}
