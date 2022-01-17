using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    public class BaiduAccessToken
    {
        public string refresh_token { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
        public string session_key { get; set; }
        public string access_token { get; set; }
        public string session_secret { get; set; }
    }
}
