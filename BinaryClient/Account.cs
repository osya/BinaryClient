using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BinaryClient;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;

namespace BinaryClient
{
    public class Accounts : ObservableCollection<Account>
    {
        public static BinaryWs Bws { get; } = new BinaryWs();
        public static List<KeyValuePair<string, string>> MarketList { get; set; } = new List<KeyValuePair<string, string>>();
        public static KeyValuePair<string, string>[] TimeUnitList { get; } = {
            new KeyValuePair<string, string>("t", "ticks"),
            new KeyValuePair<string, string>("s", "seconds"),
            new KeyValuePair<string, string>("m", "minutes"),
            new KeyValuePair<string, string>("h", "hours"),
            new KeyValuePair<string, string>("d", "days"),
        };
        public static KeyValuePair<string, string>[] BasisList { get; } = {
            new KeyValuePair<string, string>("payout", "Payout"),
            new KeyValuePair<string, string>("stake", "Stake")
        };

        public void Init()
        {
            // Open websocket connection to get Market lists for all accounts
            Task.Run(() => Bws.Connect()).Wait();
            Task.Run(() => Bws.SendRequest("{\"trading_times\":\"2015-09-14\"}")).Wait();
            var jsonTradingTimesResponse = Task.Run(() => Bws.StartListen()).Result;
            var tradingTime = JsonConvert.DeserializeObject<TradingTimesResponse>(jsonTradingTimesResponse);

            foreach (var market in tradingTime.trading_times.markets)
            {
                MarketList.Add(new KeyValuePair<string, string>(market.name, market.name));
                foreach (var submarket in market.submarkets)
                {
                    MarketList.Add(new KeyValuePair<string, string>(submarket.name, $"  {submarket.name}"));
                }
            }

            Add(new Account("3EjSVBls8OS4NqJ"));
        }
    }

    public class Account: IDataErrorInfo, INotifyPropertyChanged
    {
        private string _key;
        private string _username;
        private string _name;
        private string _balance;
        private bool _selected;
        private Auth _auth;
        public BinaryWs Bws { get; } = new BinaryWs();
        public bool Selected {
            get { return _selected; }
            set {
                if (value == _selected) return;
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        public string Username {
            get { return _username; }
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Key {
            get { return _key; }
            set
            {
                // There is no condtion if (value == _key) return; to be able to refresh accounts via reassigning key
                _key = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _auth = Task.Run(() => Bws.Authorize(_key)).Result;
                }
                OnPropertyChanged("Key");
            }
        }

        public string Balance
        {
            get { return _balance; }
            set
            {
                if (value == _balance) return;
                _balance = value;
                OnPropertyChanged("Balance");
            }
        }

        public Account()
        {
            _auth = null;
            Key = string.Empty;
            Task.Run(() => Bws.Connect()).Wait();
        }

        public Account(string key): this()
        {
            Key = key;
        }

        #region IDataErrorInfo Members
        public string Error
        {
            get { throw new NotImplementedException();}
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                if (("Key" != columnName) || (string.IsNullOrEmpty(Key))) return errorMessage;
                if (_auth.authorize == null)
                {
                    errorMessage = "auth.authorize == null";
                }
                else
                {
                    Username = _auth.authorize.loginid;
                    Name = _auth.authorize.fullname;
                    Balance = "USD" == _auth.authorize.currency
                        ? $"${_auth.authorize.balance}"
                        : _auth.authorize.balance;
                }
                return errorMessage;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}