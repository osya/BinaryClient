using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace BinaryClient.JSONTypes
{
    public class TradingTimesResponse
    {
        public EchoReqTradingTimes echo_req { get; set; }
        public TradingTimes trading_times { get; set; }
        public string msg_type { get; set; }
        public int req_id { get; set; }
    }

    public class EchoReqTradingTimes
    {
        public string trading_times { get; set; }
        public int req_id { get; set; }
    }

    public class TradingTimes
    {
        public List<Market> markets { get; set; } = new List<Market>();
    }

    public class Market
    {
        public List<Submarket> submarkets { get; set; } = new List<Submarket>();
        public string name { get; set; }
    }

    public class Submarket
    {
        public string name { get; set; }
        public Symbol[] symbols { get; set; }
    }

    public class Symbol
    {
        public string symbol { get; set; }
        public Event[] events { get; set; }
        public string name { get; set; }
        public string settlement { get; set; }
        public Times times { get; set; }
    }

    public class Event
    {
        public string descrip { get; set; }
        public string dates { get; set; }
    }

    public class Times
    {
        public string[] open;
        public string[] close;
        public string settlement { get; set; }
    }
}