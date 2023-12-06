namespace XMagnetSearch
{
    public class SearchBean
    {
        public SearchBean(string title, string magnetUrl, string size, string from, string dateTime)
        {
            Title = title;
            MagnetUrl = magnetUrl;
            Size = size;
            From = from;
            DateTime = dateTime;
        }

        public string Title { get; set; } = string.Empty;
        public string MagnetUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
    }
}
