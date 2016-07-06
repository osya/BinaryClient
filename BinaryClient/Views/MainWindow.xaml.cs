using System;
using System.Diagnostics;
using System.Linq;
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

            var priceProposalRequest = new PriceProposalRequest
            {
                proposal = 1,
                amount = TextPayout.Text,
                basis = ComboBasis.SelectedValue.ToString(),
                contract_type = contractType,
                currency = ComboCurrency.Text,
                duration = TextDuration.Text,
                duration_unit = ComboTimeUnit.SelectedValue.ToString(),
                symbol = ViewModel.SelectedSymbol.symbol
                // TODO: date_start commented cause it should be tested more carefully
//                date_start = ViewModel.SelectedStartTime.Key !=0 ? ViewModel.SelectedStartTime.Key : (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds
            };
            var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);
            foreach (var acc in MainWindowViewModel.Accounts.Where(m => m.Selected))
            {
                await acc.Bws.SendRequest(jsonPriceProposalRequest);
                var jsonPriceProposalResponse = await acc.Bws.StartListen();
                var priceProposal = JsonConvert.DeserializeObject<PriceProposalResponse>(jsonPriceProposalResponse);
                if (priceProposal.proposal == null) continue;
                var id = priceProposal.proposal.id;
                var price = priceProposal.proposal.display_value;

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
