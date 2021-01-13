using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.HuaweiOCR
{
    public class HuaweiOCRResponse
    {
        public Result[] result { get; set; }
    }

    public class Result
    {
        public Status status { get; set; }

        public string Type { get; set; }

        public Content content { get; set; }

    }

    public class Status
    {
        public string error_code { get; set; }
        public string error_msg { get; set; }
    }

    public class Content
    {
        public string Type { get; set; }
        public string serial_number { get; set; }

        public string code { get; set; }

        public string check_code { get; set; }

        public string number { get; set; }

        public string issue_date { get; set; }

        public string buyer_id { get; set; }

        public string buyer_name { get; set; }

        public string seller_name { get; set; }

        public string total { get; set; }

        public string subtotal_amount { get; set; }

        public string departure_station { get; set; }

        public string destination_station { get; set; }

        public string departure_time { get; set; }

        public string ticket_id { get; set; }

        public string log_id { get; set; }

        public string ticket_price { get; set; }
        public Item_List[] item_list { get; set; }

        public string e_ticket_number { get; set; }

        public itinerary_list[] itinerary_list { get; set; }

        public string date { get; set; }

        public string time { get; set; }

        public string cashier { get; set; }

        public string entry { get; set; }

        public string exit { get; set; }

        public string boarding_time { get; set; }

        public string amount { get; set; }

    }

    public class Item_List
    {
        public string name { get; set; }
    }

    public class itinerary_list
    {
        public string destination_station { get; set; }
        public string departure_station { get; set; }
    }
}
