namespace BinaryClient.JSONTypes
{
    public class PriceProposalRequest
    {
        public int proposal { get; set; }
        public string amount { get; set; }
        public string basis { get; set; }
        public string contract_type { get; set; }
        public string currency { get; set; }
        public string duration { get; set; }
        public string duration_unit { get; set; }
        public string symbol { get; set; }
        public int date_start { get; set; }
    }


    public class PriceProposalResponse
    {
        public EchoReq echo_req { get; set; }
        public Proposal proposal { get; set; }
        public string msg_type { get; set; }
    }

    public class EchoReq
    {
        public string symbol { get; set; }
        public string duration { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public int proposal { get; set; }
        public string basis { get; set; }
        public string duration_unit { get; set; }
        public string contract_type { get; set; }
    }

    public class Proposal
    {
        public string longcode { get; set; }
        public string spot { get; set; }
        public string display_value { get; set; }
        public string ask_price { get; set; }
        public string spot_time { get; set; }
        public int date_start { get; set; }
        public string id { get; set; }
        public string payout { get; set; }
    }

}