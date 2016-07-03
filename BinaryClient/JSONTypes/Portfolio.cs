namespace BinaryClient.JSONTypes
{
    public class PortfolioResponse
    {
        public Portfolio portfolio { get; set; }
        public EchoReqPortfolio echo_req { get; set; }
        public string msg_type { get; set; }
    }

    public class Portfolio
    {
        public Contract[] contracts { get; set; }
    }

    public class Contract
    {
        public string symbol { get; set; }
        public string shortcode { get; set; }
        public string contract_id { get; set; }
        public string longcode { get; set; }
        public int expiry_time { get; set; }
        public string currency { get; set; }
        public string transaction_id { get; set; }
        public int date_start { get; set; }
        public string buy_price { get; set; }
        public int purchase_time { get; set; }
        public string contract_type { get; set; }
        public string payout { get; set; }
    }

    public class EchoReqPortfolio
    {
        public int portfolio { get; set; }
    }

}