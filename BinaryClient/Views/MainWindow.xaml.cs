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

//            ViewModel.PriceProposalRequest(contractType);

            foreach (var acc in MainWindowViewModel.Accounts.Where(m => m.Selected))
            {
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
