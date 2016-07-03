namespace BinaryClient.JSONTypes
{

    public class Status
    {
        public Get_Account_Status get_account_status { get; set; }
        public Echo_Req echo_req { get; set; }
        public string msg_type { get; set; }
    }

    public class Get_Account_Status
    {
        public string risk_classification { get; set; }
        public object[] status { get; set; }
    }

    public class Echo_Req
    {
        public string get_account_status { get; set; }
    }

}