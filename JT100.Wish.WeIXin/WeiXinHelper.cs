using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.WeIXin
{
    public class WeiXinHelper
    {
        private const string AppID = "wxcd4bcd53d8d51be7";
        private const string AppSecret = "317e657c276bc9694a77542264bf2f0b";
        private const string TokenUrl = "https://api.weixin.qq.com/cgi-bin/token";
        public static string GetToken()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("grant_type", "client_credential");
            dict.Add("appid", AppID);
            dict.Add("secret", AppSecret);
            //var result = WishContext.Api.HttpGet<ApiResult>(TokenUrl, dict);
            return null;
        }
    }
}
