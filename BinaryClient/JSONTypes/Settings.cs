namespace BinaryClient.JSONTypes
{
    public class Settings
    {
        public EchoReqSettings echo_req { get; set; }
        public string msg_type { get; set; }
        public GetSettings get_settings { get; set; }
    }

    public class GetSettings
    {
        public string email { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }

    public class EchoReqSettings
    {
        public string get_settings { get; set; }
    }
}