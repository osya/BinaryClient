using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;

namespace BinaryClient.Views
{
    /// <summary>
    /// Interaction logic for TestCallPutButtons.xaml
    /// </summary>
    public partial class TestCallPutButtons
    {
        const string Key = "3EjSVBls8OS4NqJ";
        readonly BinaryWs _bws = new BinaryWs();
        readonly Stopwatch _watch = new Stopwatch();

        public TestCallPutButtons()
        {
            Task.Run(() => _bws.Connect()).Wait();
            Task.Run(() => _bws.Authorize(Key)).Wait();

            InitializeComponent();
        }

        private async void buttonCall_Click(object sender, RoutedEventArgs e)
        {
            textTime.Text = "";
            _watch.Start();

            var priceProposalRequest = new PriceProposalRequest
            {
                proposal = 1,
                amount = "100",
                basis = "payout",
                contract_type = "CALL",
                currency = "USD",
                duration = "60",
                duration_unit = "s",
                symbol = "R_100"
            };
            var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);
            await _bws.SendRequest(jsonPriceProposalRequest);
            var jsonPriceProposalResponse = await _bws.StartListen();
            var priceProposal = JsonConvert.DeserializeObject<PriceProposalResponse>(jsonPriceProposalResponse);
            var id = priceProposal.proposal.id;
            var price = priceProposal.proposal.display_value;

            await _bws.SendRequest($"{{\"buy\":\"{id}\", \"price\": {price}}}");
            var jsonBuy = await _bws.StartListen();

            _watch.Stop();
            textTime.Text = _watch.ElapsedMilliseconds.ToString();
        }

        private async void buttonPut_Click(object sender, RoutedEventArgs e)
        {
            textTime.Text = "";
            _watch.Start();

            var priceProposalRequest = new PriceProposalRequest
            {
                proposal = 1,
                amount = "100",
                basis = "payout",
                contract_type = "PUT",
                currency = "USD",
                duration = "60",
                duration_unit = "s",
                symbol = "R_100"
            };
            var jsonPriceProposalRequest = JsonConvert.SerializeObject(priceProposalRequest);
            await _bws.SendRequest(jsonPriceProposalRequest);
            var jsonPriceProposalResponse = await _bws.StartListen();
            var priceProposal = JsonConvert.DeserializeObject<PriceProposalResponse>(jsonPriceProposalResponse);
            var id = priceProposal.proposal.id;
            var price = priceProposal.proposal.display_value;

            await _bws.SendRequest($"{{\"buy\":\"{id}\", \"price\": 100}}");
            var jsonBuy = await _bws.StartListen();

            _watch.Stop();
            textTime.Text = _watch.ElapsedMilliseconds.ToString();
            _watch.Reset();
        }
    }
}
