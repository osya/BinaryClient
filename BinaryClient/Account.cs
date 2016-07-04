using System.Windows.Input;
using System.Threading.Tasks;

namespace BinaryClient
{
    public class Account
    {
        private string _key;
        readonly BinaryWs _bws = new BinaryWs();

        public bool Selected { get; set; }
        public string Key {
            get { return _key; }
            set
            {
                _key = value;
                if (!string.IsNullOrEmpty(value))
                {
                    Task.Run(() => _bws.Authorize(_key)).Wait();
                }
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