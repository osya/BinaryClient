using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using BinaryClient.JSONTypes;
using BinaryClient.Model;
using BinaryClient.ViewModel;
using Newtonsoft.Json;

namespace BinaryClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindowViewModel ViewModel { get; set; } = new MainWindowViewModel();
        readonly Stopwatch _watch = new Stopwatch();

        public MainWindow()
        {
            ViewModel.Init();
            InitializeComponent();
            DataAccounts.ItemsSource = MainWindowViewModel.Accounts;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var acc in MainWindowViewModel.Accounts.Where(acc => !acc.Selected))
            {
                acc.Selected = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var acc in MainWindowViewModel.Accounts.Where(acc => acc.Selected))
            {
                acc.Selected = false;
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Accounts.Add(new Account());
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.Accounts.Remove(acc => acc.Selected);
            // TODO: Investigate how to update labelSelected on some event during deletin account
            LabelSelected.Content = $"Selected: {MainWindowViewModel.Accounts.Count(m => m.Selected)}";
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            foreach (var acc in MainWindowViewModel.Accounts)
            {
                acc.Key = acc.Key;
            }
        }

        private async void Buy(string contractType)
        {
            TextTime.Text = "";
            _watch.Start();

            foreach (var acc in MainWindowViewModel.Accounts.Where(m => m.Selected))
            {
                var priceProposalRequest = new PriceProposalRequest
                {
                    proposal = 1,
                    amount = ViewModel.BasisValue.ToString(CultureInfo.InvariantCulture),
                    basis = ViewModel.SelectedBasis.Key,
                    contract_type = contractType,
                    currency = ViewModel.SelectedCurrency.Key,
                    duration = ViewModel.Duration.ToString(),
                    duration_unit = ViewModel.SelectedTimeUnit.Key,
                    symbol = ViewModel.SelectedSymbol.symbol
                    // TODO: date_start commented cause it should be tested more carefully
                    // date_start = ViewModel.SelectedStartTime.Key !=0 ? ViewModel.SelectedStartTime.Key : (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                };
                var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);

                acc.Bws.SendRequest(jsonPriceProposalRequest).Wait();
                var jsonPriceProposalResponse = Task.Run(() => acc.Bws.StartListen()).Result;
                var priceProposal = JsonConvert.DeserializeObject<PriceProposalResponse>(jsonPriceProposalResponse);

                switch (contractType)
                {
                    case "CALL":
                        ViewModel.CallDisplayValue = priceProposal.proposal != null ? priceProposal.proposal.display_value : string.Empty;
                        ViewModel.CallProposalId = priceProposal.proposal != null ? priceProposal.proposal.id : string.Empty;
                        break;
                    case "PUT":
                        ViewModel.PutDisplayValue = priceProposal.proposal != null ? priceProposal.proposal.display_value : string.Empty;
                        ViewModel.PutProposalId = priceProposal.proposal != null ? priceProposal.proposal.id : string.Empty;
                        break;
                }


                var price = "CALL" == contractType ? ViewModel.CallDisplayValue : ViewModel.PutDisplayValue;
                var id = "CALL" == contractType ? ViewModel.CallProposalId : ViewModel.PutProposalId;


                if ((string.IsNullOrEmpty(price)) || (string.IsNullOrEmpty(id))) continue;
                await acc.Bws.SendRequest($"{{\"buy\":\"{id}\", \"price\": {price}}}");
                var jsonBuy = await acc.Bws.StartListen();
            }

            _watch.Stop();
            TextTime.Text = _watch.ElapsedMilliseconds.ToString();
            _watch.Reset();
        }

        private void buttonPut_Click(object sender, RoutedEventArgs e)
        {
            Buy("PUT");
        }

        private void buttonCall_Click(object sender, RoutedEventArgs e)
        {
            Buy("CALL");
        }

        private void DataGrid_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            LabelSelected.Content = $"Selected: {MainWindowViewModel.Accounts.Count(m => m.Selected)}";
        }
    }
}
