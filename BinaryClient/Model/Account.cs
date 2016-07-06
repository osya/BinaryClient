using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;

namespace BinaryClient.Model
{
    public class Accounts : ObservableCollection<Account>
    {
        public void Init()
        {
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

        public string Status { get; set; }
        public int Opens { get; set; }

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
            Status = "Inactive";
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
                    Status = "Inactive";
                    OnPropertyChanged("Status");
                    errorMessage = "auth.authorize == null";
                }
                else
                {
                    Status = "Active";
                    OnPropertyChanged("Status");
                    Username = _auth.authorize.loginid;
                    OnPropertyChanged("Username");
                    Name = _auth.authorize.fullname;
                    OnPropertyChanged("Name");
                    Balance = "USD" == _auth.authorize.currency
                        ? $"${_auth.authorize.balance}"
                        : _auth.authorize.balance;
                    OnPropertyChanged("Balance");

                    // Request for opens positions
                    Task.Run(() => Bws.SendRequest($"{{\"portfolio\":1}}")).Wait();
                    var jsonPortfolio = Task.Run(() => Bws.StartListen()).Result;
                    var portfolio = JsonConvert.DeserializeObject<PortfolioResponse>(jsonPortfolio);
                    Opens = portfolio.portfolio.contracts.Length;
                    OnPropertyChanged("Opens");
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