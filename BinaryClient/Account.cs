using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace BinaryClient
{
    public class Account: DependencyObject
    {
        private string _key;
        public BinaryWs Bws { get; } = new BinaryWs();
        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public string Username { get; set; }
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(bool), typeof(Account), new UIPropertyMetadata(false));

        public string Key {
            get { return _key; }
            set
            {
                _key = value;
                if (string.IsNullOrEmpty(value)) return;
                var auth = Task.Run(() => Bws.Authorize(_key)).Result;
                Username = auth.authorize.loginid;
            }
        }

        public Account()
        {
            Key = string.Empty;
        }

        public Account(string key)
        {
            Key = key;
        }
    }
}