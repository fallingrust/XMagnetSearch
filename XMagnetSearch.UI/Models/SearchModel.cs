using Prism.Mvvm;

namespace XMagnetSearch.UI.Models
{
    public class SearchModel : BindableBase
    {
        private string _title = string.Empty;
        private string _magnetUrl= string.Empty;
        private string _size = string.Empty;
        private string _from = string.Empty;
        private string _dateTime = string.Empty;
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string MagnetUrl { get => _magnetUrl; set => SetProperty(ref _magnetUrl, value); }
        public string Size { get => _size; set => SetProperty(ref _size, value); }
        public string From { get => _from; set => SetProperty(ref _from, value); }
        public string DateTime { get => _dateTime; set => SetProperty(ref _dateTime, value); }

        public SearchModel(string title, string magnetUrl, string size, string from, string dateTime)
        {
            Title = title;
            MagnetUrl = magnetUrl;
            Size = size;
            From = from;
            DateTime = dateTime;
        }

        public static SearchModel Converter(SearchBean searchBean)
        {
            return new(searchBean.Title, searchBean.MagnetUrl, searchBean.Size, searchBean.From, searchBean.DateTime);
        }
    }
}
