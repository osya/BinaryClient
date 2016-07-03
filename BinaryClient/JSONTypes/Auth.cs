namespace BinaryClient.JSONTypes
{
    public class Auth
    {
        public Authorize authorize { get; set; }
        public EchoReqAuth echo_req { get; set; }
        public string msg_type { get; set; }
    }

    public class Authorize
    {
        public string currency { get; set; }
        public string email { get; set; }
        public string[] scopes { get; set; }
        public string balance { get; set; }
        public string landing_company_name { get; set; }
        public string fullname { get; set; }
        public string loginid { get; set; }
        public int is_virtual { get; set; }
    }

    public class EchoReqAuth
    {
        public string authorize { get; set; }
    }

}