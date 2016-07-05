using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;

namespace BinaryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly Stopwatch _watch = new Stopwatch();
        public Accounts Accounts { get; } = new Accounts();

        private void CheckBox_Checked (object sender, RoutedEventArgs e)
        {
            foreach (var acc in Accounts.Where(acc => !acc.Selected))
            {
                acc.Selected = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var acc in Accounts.Where(acc => acc.Selected))
            {
                acc.Selected = false;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataAccounts.ItemsSource = Accounts;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            Accounts.Remove(acc => acc.Selected);
            // TODO: Investigate how to update labelSelected on some event during deletin account
            LabelSelected.Content = $"Selected: {Accounts.Count(m => m.Selected)}";
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            foreach (var acc in Accounts)
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
                basis = "payout",
                contract_type = contractType,
                currency = ComboCurrency.Text,
                duration = TextDuration.Text,
                duration_unit = ComboTimeUnit.SelectedValue.ToString(),
                symbol = "R_100"
            };
            var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);
            foreach (var acc in Accounts.Where(m => m.Selected))
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
            LabelSelected.Content = $"Selected: {Accounts.Count(m => m.Selected)}";
        }
    }
}
