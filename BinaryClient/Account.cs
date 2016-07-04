﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BinaryClient;

namespace BinaryClient
{
    public class Accounts : ObservableCollection<Account>
    {
        public Accounts()
        {
            Add(new Account("3EjSVBls8OS4NqJ"));
        }
    }

    public class Account: IDataErrorInfo, INotifyPropertyChanged
    {
        private string _key;
        private string _username;
        private bool _selected;
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

        public string Key {
            get { return _key; }
            set
            {
                if (value == _key) return;
                _key = value;
                OnPropertyChanged("Key");
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
//                var auth = Task.Run(() => Bws.Authorize(_key)).Result;
//
//                if (auth.authorize == null)
//                {
//                    errorMessage = "auth.authorize == null";
//                }
//                else
//                {
//                    Username = auth.authorize.loginid;
//                }
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