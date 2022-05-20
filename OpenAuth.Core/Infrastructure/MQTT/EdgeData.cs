using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.MQTT
{
    /// <summary>
    /// 
    /// </summary>
    public class EdgeData
    {
        /// <summary>
        /// 
        /// </summary>
        public int msg_type { get; set; }
        public string edge_guid { get; set; }
        public string token { get; set; }
        public Chl_Info chl_info { get; set; }
        public int upd_dt { get; set; }
    }
    public class Chl_Info
    {
        public bool success { get; set; }
        public string error { get; set; }
        public string edge_guid { get; set; }
        public string token { get; set; }
        public edge_info edge_info { get; set; }
        public List<Datum> data { get; set; }
    }
    public class Datum
    {
        public string bts_server_ip { get; set; }
        public string srv_guid { get; set; }
        public string bts_server_version { get; set; }
        public int bts_type { get; set; }
        public List<Mid_List> mid_list { get; set; }
    }
    public class edge_info
    {
        public string edg_name { get; set; }
        public string person_in_charge { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string public_ip { get; set; }
    }
    public class Mid_List
    {
        public int dev_type { get; set; }
        public int dev_uid { get; set; }
        public string mid_guid { get; set; }
        public string mid_ip { get; set; }
        public string mid_version { get; set; }
        public string production_serial { get; set; }
        public string st_production_time { get; set; }
        public List<Low_List> low_list { get; set; }
        public Aux_Low_List[] aux_low_list { get; set; }
    }

    public class Low_List
    {
        public int unit_id { get; set; }
        public string low_guid { get; set; }
        public int low_no { get; set; }
        public int range_volt { get; set; }
        public int[] range_curr_array { get; set; }
        public string low_version { get; set; }
        public string slave_sft_ver { get; set; }
        public string first_cali_time { get; set; }
        public string latest_cali_time { get; set; }
        public string latest_cali_worker { get; set; }
        public int?[] channel_list { get; set; }
        public int?[] pyh_chl_list { get; set; }
    }

    public class Aux_Low_List
    {
        public string low_guid { get; set; }
        public int low_no { get; set; }
        public string low_version { get; set; }
        public string slave_sft_ver { get; set; }
        public string first_cali_time { get; set; }
        public string latest_cali_time { get; set; }
        public string latest_cali_worker { get; set; }
        public Map_Chanel_List[] map_chanel_list { get; set; }
    }

    public class Map_Chanel_List
    {
        public int main_unit_id { get; set; }
        public int main_chl_id { get; set; }
        public int aux_id { get; set; }
        public int aux_pyh { get; set; }
    }


}
