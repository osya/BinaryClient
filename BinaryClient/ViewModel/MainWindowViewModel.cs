using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows;
using BinaryClient.JSONTypes;
using BinaryClient.Model;
using Newtonsoft.Json;

namespace BinaryClient.ViewModel
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        public static BinaryWs Bws { get; } = new BinaryWs();
        public static Accounts Accounts { get; } = new Accounts();

        public static ObservableCollection<KeyValuePair<int, string>> StartTimeList { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        private static KeyValuePair<int, string> _selectedStartTime;
        public KeyValuePair<int, string> SelectedStartTime {
            get { return _selectedStartTime; }
            set
            {
                _selectedStartTime = value;
                OnPropertyChanged("SelectedStartTime");
            }
        }
       
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
                OnPropertyChanged("SelectedMarket");
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
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        public static ObservableCollection<KeyValuePair<string, string>> TimeUnitList { get; } = new ObservableCollection<KeyValuePair<string, string>>{
            new KeyValuePair<string, string>("t", "ticks"),
            new KeyValuePair<string, string>("s", "seconds"),
            new KeyValuePair<string, string>("m", "minutes"),
            new KeyValuePair<string, string>("h", "hours"),
            new KeyValuePair<string, string>("d", "days"),
        };

        private static KeyValuePair<string, string> _selectedTimeUnit;
        public KeyValuePair<string, string> SelectedTimeUnit {
            get { return _selectedTimeUnit;  }
            set
            {
                _selectedTimeUnit = value;
                OnPropertyChanged("SelectedTimeUnit");
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        public static ObservableCollection<KeyValuePair<string, string>> BasisList { get; } = new ObservableCollection<KeyValuePair<string, string>>{
            new KeyValuePair<string, string>("payout", "Payout"),
            new KeyValuePair<string, string>("stake", "Stake")
        };

        private static KeyValuePair<string, string> _selectedBasis;
        public KeyValuePair<string, string> SelectedBasis {
            get { return _selectedBasis; }
            set
            {
                _selectedBasis = value;
                OnPropertyChanged("SelectedBasis");
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        private static double _basisValue;
        public double BasisValue {
            get { return _basisValue; }
            set
            {
                _basisValue = value;
                OnPropertyChanged("BasisValue");
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        // It is Stake value, which is calculated based on Payout value
        public string CallDisplayValue;
        public string CallStake => "stake" == SelectedBasis.Key ? BasisValue.ToString(CultureInfo.InvariantCulture) : CallDisplayValue;

        public string PutDisplayValue;
        public string PutStake => "stake" == SelectedBasis.Key ? BasisValue.ToString(CultureInfo.InvariantCulture) : PutDisplayValue;

        private string _callPayout;
        public string CallPayout => "payout" == SelectedBasis.Key ? BasisValue.ToString(CultureInfo.InvariantCulture) : _callPayout;
        private string _putPayout;
        public string PutPayout => "payout" == SelectedBasis.Key ? BasisValue.ToString(CultureInfo.InvariantCulture) : _putPayout;

        private double CallNetProfit => string.IsNullOrEmpty(CallStake) || string.IsNullOrEmpty(CallPayout) ? double.NaN : Convert.ToDouble(CallPayout) - Convert.ToDouble(CallStake);
        private double CallReturn => string.IsNullOrEmpty(CallStake) ? double.NaN : CallNetProfit/Convert.ToDouble(CallStake);
        public string CallLabel => $"Stake: {SelectedCurrency.Value} {CallStake} Payout: {SelectedCurrency.Value} {CallPayout}\nNet profit: {SelectedCurrency.Value} {CallNetProfit.ToString(CultureInfo.InvariantCulture)} | Return: {CallReturn.ToString("P2", CultureInfo.InvariantCulture)}";
        private double PutNetProfit => string.IsNullOrEmpty(PutStake) || string.IsNullOrEmpty(PutPayout) ? double.NaN : Convert.ToDouble(PutPayout) - Convert.ToDouble(PutStake);
        private double PutReturn => string.IsNullOrEmpty(PutStake) ? double.NaN : PutNetProfit / Convert.ToDouble(PutStake);
        public string PutLabel => $"Stake: {SelectedCurrency.Value} {PutStake} Payout: {SelectedCurrency.Value} {PutPayout}\nNet profit: {SelectedCurrency.Value} {PutNetProfit.ToString(CultureInfo.InvariantCulture)} | Return: {PutReturn.ToString("P2", CultureInfo.InvariantCulture)}";

        public string CallProposalId;
        public string PutProposalId;

        public ObservableCollection<KeyValuePair<string, string>> CurrencyList { get; } = new ObservableCollection<KeyValuePair<string, string>>();
        private KeyValuePair<string, string> _selectedCurrency;
        public KeyValuePair<string, string> SelectedCurrency {
            get { return _selectedCurrency; }
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged("SelectedCurrency");
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        private static int _duration;
        public int Duration {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged("Duration");
                PriceProposalRequest("CALL");
                PriceProposalRequest("PUT");
            }
        }

        public void PriceProposalRequest(string contractType)
        {
            if ((string.IsNullOrEmpty(SelectedBasis.Key)) ||
                (string.IsNullOrEmpty(SelectedCurrency.Key)) || (string.IsNullOrEmpty(SelectedTimeUnit.Key)) || 
                string.IsNullOrEmpty(SelectedSymbol?.symbol) || 0 == Duration) return;

            var priceProposalRequest = new PriceProposalRequest
            {
                proposal = 1,
                amount = BasisValue.ToString(CultureInfo.InvariantCulture),
                basis = SelectedBasis.Key,
                contract_type = contractType,
                currency = SelectedCurrency.Key,
                duration = Duration.ToString(),
                duration_unit = SelectedTimeUnit.Key,
                symbol = SelectedSymbol.symbol
                // TODO: date_start commented cause it should be tested more carefully
                // date_start = ViewModel.SelectedStartTime.Key !=0 ? ViewModel.SelectedStartTime.Key : (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
            };
            var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);

            Task.Run(() => Bws.SendRequest(jsonPriceProposalRequest)).Wait();
            var jsonPriceProposalResponse = Task.Run(() => Bws.StartListen()).Result;
            var priceProposal = JsonConvert.DeserializeObject<PriceProposalResponse>(jsonPriceProposalResponse);

            switch (contractType)
            {
                case "CALL":
                    CallDisplayValue = priceProposal.proposal != null ? priceProposal.proposal.display_value : string.Empty;
                    CallProposalId = priceProposal.proposal != null ? priceProposal.proposal.id : string.Empty;
                    _callPayout = priceProposal.proposal != null ? priceProposal.proposal.payout : string.Empty;
                    OnPropertyChanged("CallLabel");
                    break;
                case "PUT":
                    PutDisplayValue = priceProposal.proposal != null ? priceProposal.proposal.display_value : string.Empty;
                    PutProposalId = priceProposal.proposal != null ? priceProposal.proposal.id : string.Empty;
                    _putPayout = priceProposal.proposal != null ? priceProposal.proposal.payout : string.Empty;
                    OnPropertyChanged("PutLabel");
                    break;
            }
        }

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
            SelectedBasis = BasisList[0];
            CurrencyList.Add(new KeyValuePair<string, string>("USD", "USD"));
            SelectedCurrency = CurrencyList[0];
            SelectedTimeUnit = TimeUnitList[1];
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