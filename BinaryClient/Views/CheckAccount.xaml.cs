using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;
using NLog;

namespace BinaryClient.Views
{
    /// <summary>
    /// Interaction logic for CheckAccount.xaml
    /// </summary>
    public partial class CheckAccount
    {
        private readonly Dictionary<string, string> _status = new Dictionary<string, string>();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        readonly BinaryWs _bws = new BinaryWs();
        readonly Stopwatch _watch = new Stopwatch();

        public CheckAccount()
        {
            InitializeComponent();
            Task.Run(() => _bws.Connect()).Wait();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            textTime.Text = "";
            _watch.Start();

            _status["Key"] = TextKey.Text;
            var auth = await _bws.Authorize(TextKey.Text);

            await _bws.SendRequest($"{{\"get_settings\":\"1\"}}");
            var jsonSettings = await _bws.StartListen();
            var settings = JsonConvert.DeserializeObject<Settings>(jsonSettings);
            await _bws.SendRequest($"{{\"get_account_status\":\"1\"}}");

            if (auth.authorize == null) return;

            _status["Username"] = auth.authorize.loginid;
            _status["Name"] = auth.authorize.fullname;
            _status["Email"] = auth.authorize.email;

            var typesUpperList = auth.authorize.scopes.Select(scope => scope.First().ToString().ToUpper() + string.Join("", scope.Skip(1))).ToList();

            _status["Type"] = string.Join(", ", typesUpperList);
            _status["Currency"] = auth.authorize.currency;
            if (settings.get_settings != null)
            {
                _status["Country"] = settings.get_settings.country_code;
            }
            
            _status["Balance"] = auth.authorize.balance;

            var list = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var entry in _status)
            {
                list.Add(entry);
            }
            DataGrid1.ItemsSource = list;

            _watch.Stop();
            textTime.Text = _watch.ElapsedMilliseconds.ToString();
            _watch.Reset();
        }
    }
}
