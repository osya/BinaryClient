using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BinaryClient.JSONTypes;
using BinaryClient.Model;
using Newtonsoft.Json;

namespace BinaryClient.ViewModel
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        public static ObservableCollection<KeyValuePair<int, string>> StartTimeList { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        public KeyValuePair<int, string> SelectedStartTime { get; set; }
        public static Accounts Accounts { get; } = new Accounts();
        public static BinaryWs Bws { get; } = new BinaryWs();
        public static ObservableCollection<MarketSubmarket> MarketList { get; } = new ObservableCollection<MarketSubmarket>();

        private static MarketSubmarket _selectedMarket;
        public MarketSubmarket SelectedMarket {
            get { return _selectedMarket; }
            set
            {
                _selectedMarket = value;
                SymbolList.Clear();
                if (null != _selectedMarket.symbols)
                {       
                    foreach (var symbol in _selectedMarket.symbols)
                        SymbolList.Add(symbol);
                }
                else
                {
                    foreach (var symbol in _selectedMarket.submarkets.SelectMany(submarket => submarket.symbols))
                    {
                        SymbolList.Add(symbol);
                    }
                }
                SelectedSymbol = SymbolList[0];
            }
        }

        public static ObservableCollection<Symbol> SymbolList { get; } = new ObservableCollection<Symbol>();
        private static Symbol _selectedSymbol;
        public Symbol SelectedSymbol {
            get { return _selectedSymbol; }
            set
            {
                _selectedSymbol = value;
                OnPropertyChanged("SelectedSymbol");
            }
        }

        public static ObservableCollection<KeyValuePair<string, string>> TimeUnitList { get; } = new ObservableCollection<KeyValuePair<string, string>>{
            new KeyValuePair<string, string>("t", "ticks"),
            new KeyValuePair<string, string>("s", "seconds"),
            new KeyValuePair<string, string>("m", "minutes"),
            new KeyValuePair<string, string>("h", "hours"),
            new KeyValuePair<string, string>("d", "days"),
        };

        public static ObservableCollection<KeyValuePair<string, string>> BasisList { get; } = new ObservableCollection<KeyValuePair<string, string>>{
            new KeyValuePair<string, string>("payout", "Payout"),
            new KeyValuePair<string, string>("stake", "Stake")
        };

        public MainWindowViewModel()
        {
            var curTime = DateTime.UtcNow;
            StartTimeList.Add(new KeyValuePair<int, string>(0, "Now"));
            for (var i = 0; i < 673; i++)
            {
                curTime = curTime.AddMinutes(5);
                var curTimeS = curTime.ToLocalTime().ToString("HH:mm \"GMT\"K, ddd");
                var unixTimestamp = (int)(curTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                StartTimeList.Add(new KeyValuePair<int, string>(unixTimestamp, curTimeS));
            }
            SelectedStartTime = StartTimeList[0];
        }

        public void Init()
        {
            Accounts.Init();

            // Open websocket connection to get Market lists for all accounts
            Task.Run(() => Bws.Connect()).Wait();
            Task.Run(() => Bws.SendRequest("{\"trading_times\":\"2015-09-14\"}")).Wait();
            var jsonTradingTimesResponse = Task.Run(() => Bws.StartListen()).Result;
            var tradingTime = JsonConvert.DeserializeObject<TradingTimesResponse>(jsonTradingTimesResponse);

            foreach (var market in tradingTime.trading_times.markets)
            {
                MarketList.Add(new MarketSubmarket(market));
                foreach (var submarket in market.submarkets)
                {
                    MarketList.Add(new MarketSubmarket(submarket));
                }
            }
            SelectedMarket = MarketList[0];
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}