using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinaryClient
{
    /// <summary>
    /// Interaction logic for CheckAccount.xaml
    /// </summary>
    public partial class CheckAccount : Window
    {
        private Dictionary<string, string> _status = new Dictionary<string, string>();

        public CheckAccount()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            _status["Key"] = TextKey.Text;
            var bws = new BinaryWs();
            await bws.Connect();
            await bws.SendRequest($"{{\"authorize\":\"{TextKey.Text}\"}}");
            var jsonAuth = await bws.StartListen();
            var auth = JsonConvert.DeserializeObject<Auth>(jsonAuth);
            await bws.SendRequest($"{{\"get_settings\":\"1\"}}");
            var jsonSettings = await bws.StartListen();
            var settings = JsonConvert.DeserializeObject<Settings>(jsonSettings);
            await bws.SendRequest($"{{\"get_account_status\":\"1\"}}");

            _status["Username"] = auth.authorize.loginid;
            _status["Name"] = auth.authorize.fullname;
            _status["Email"] = auth.authorize.email;

            var typesUpperList = auth.authorize.scopes.Select(scope => scope.First().ToString().ToUpper() + string.Join("", scope.Skip(1))).ToList();

            _status["Type"] = string.Join(", ", typesUpperList);
            _status["Currency"] = auth.authorize.currency;
            _status["Country"] = settings.get_settings.country_code;
            _status["Balance"] = auth.authorize.balance;

            var list = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var entry in _status)
            {
                list.Add(entry);
            }
            DataGrid1.ItemsSource = list;
        }
    }
}
