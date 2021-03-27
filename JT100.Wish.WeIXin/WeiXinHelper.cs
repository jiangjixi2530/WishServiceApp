using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JT100.Wish.WeIXin
{
    public class WeiXinHelper
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        private readonly static int TimeOut = 60 * 1000;
        private const string AppID = "wxcd4bcd53d8d51be7";
        private const string AppSecret = "317e657c276bc9694a77542264bf2f0b";
        private const string TokenUrl = "https://api.weixin.qq.com/cgi-bin/token";
        private const string SendTemplateMsgUrl = "https://api.weixin.qq.com/cgi-bin/message/template/send";
        private static Dictionary<MessageType, string> TemplateIds;
        private static TokenControl Token;
        static WeiXinHelper()
        {
            TemplateIds = new Dictionary<MessageType, string>();
            TemplateIds.Add(MessageType.BalanceNotEnough, "-Uh1QrzRflCRkQteVu-hMzUCrmj6iucSO0L_del6DtI");
            TemplateIds.Add(MessageType.ProductCollect, "129HxLdpoj1cAPfJqMwu5LfC3orOJvlOYNzKI2mW97c");
            TemplateIds.Add(MessageType.OrderDelivery, "Sop4fyd8aYZV73wp-NML7L_IgvLFnUNrsAlWiC9HAXo");
            TemplateIds.Add(MessageType.BalanceChanged, "jRoIVxcihzHirG-x_z72AavdiEjUfw6-r4YHdiFfBIY");
            TemplateIds.Add(MessageType.BalanceCharged, "m2wfUuCri5HVpW6aoc4dHO55GPm623ohpAitB85P1y8");
            TemplateIds.Add(MessageType.BalanceCharged, "vKfqUEMP84KS9tSC-EohDyoTUowmAALRdg7eYXgz1SY");
            Token = new TokenControl() { InvalidTime = DateTime.Now };
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <returns></returns>
        internal static void GetToken()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("grant_type", "client_credential");
            dict.Add("appid", AppID);
            dict.Add("secret", AppSecret);
            try
            {
                var result = HttpGet(TokenUrl, dict);
                JObject jObject = JsonConvert.DeserializeObject<JObject>(result);
                Token.Token = jObject["access_token"].ToString();
                Token.RefreshTime = DateTime.Now;
                Token.InvalidTime = Token.RefreshTime.AddSeconds(Convert.ToInt32(jObject["expires_in"].ToString()));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="message"></param>
        public static async Task<SendMessageResult> SendMessage(Message message)
        {
            //如果失效 重新获取Token
            if (Token.InvalidTime.Subtract(DateTime.Now).TotalSeconds < 30)
            {
                await Task.Run(() => GetToken());
            }
            if (!TemplateIds.ContainsKey(message.MessageType))
            {
                return SendMessageResult.ToFail("未找到对应的模板Id");
            }
            JObject msgObject = new JObject();
            msgObject["touser"] = message.ToUser;
            msgObject["template_id"] = TemplateIds[message.MessageType];
            JArray array = new JArray();
            JObject firstObj = new JObject();
            firstObj["first"] = message.Title.ToJObject();
            array.Add(firstObj);
            for (int i = 0; i < message.Msgs.Count; i++)
            {
                string key = "keyword" + (i + 1).ToString();
                JObject obj = new JObject();
                obj[key] = message.Msgs[i].ToJObject();
                array.Add(obj);
            }
            JObject remarkObj = new JObject();
            remarkObj["remark"] = message.Remark.ToJObject();
            array.Add(remarkObj);
            msgObject["data"] = array;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["access_token"] = Token.Token;
            var result = await Task.Run(() => HttpPost(SendTemplateMsgUrl, JsonConvert.SerializeObject(msgObject), dict));
            JObject resultObj = JsonConvert.DeserializeObject<JObject>(result);
            if (remarkObj["errmsg"].ToString().ToUpper() == "OK")
            {
                return SendMessageResult.ToSuccess();
            }
            else
            {
                return SendMessageResult.ToFail(remarkObj["errmsg"].ToString());
            }
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramsDict"></param>
        /// <returns></returns>
        internal static string HttpGet(string url, Dictionary<string, object> paramsDict = null)
        {
            var param = paramsDict.Select(x => string.Concat(x.Key, "=", x.Value)).ToArray();
            var requestParams = string.Empty;
            if (param.Any())
            {
                requestParams = string.Join('&', param);
            }
            var request = HttpWebRequest.Create(url + (string.IsNullOrEmpty(requestParams) ? "" : ("?" + requestParams)));
            request.Method = "Get";
            request.Timeout = TimeOut;
            request.ContentType = "text/html;charset=UTF-8";
            string text = string.Empty;
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                text = reader.ReadToEnd();
            }
            response.Close();
            request = null;
            return text;
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="paramsDict"></param>
        /// <returns></returns>
        internal static string HttpPost(string url, string json = null, Dictionary<string, object> paramsDict = null)
        {
            var param = paramsDict.Select(x => string.Concat(x.Key, "=", x.Value)).ToArray();
            var requestParams = string.Empty;
            if (param.Any())
            {
                requestParams = string.Join('&', param);
            }
            var request = HttpWebRequest.Create(url + (string.IsNullOrEmpty(requestParams) ? "" : ("?" + requestParams)));
            request.Method = "Post";
            request.Timeout = TimeOut;
            request.ContentType = "application/json";
            string text = string.Empty;
            if (!string.IsNullOrEmpty(json))
            {
                var byteArray = System.Text.Encoding.UTF8.GetBytes(json);
                using (var dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                text = reader.ReadToEnd();
            }
            response.Close();
            request = null;
            return text;
        }
    }

    /// <summary>
    /// Token管理
    /// </summary>
    internal class TokenControl
    {
        public string Token { get; set; }

        /// <summary>
        /// 刷新时间
        /// </summary>
        public DateTime RefreshTime { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime InvalidTime { get; set; }
    }

    public class Message
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 指定用户
        /// <para>微信用户OpenId</para>
        /// </summary>
        public string ToUser { get; set; }

        /// <summary>
        /// 详情跳转地址 
        ///<para>不跳转可空</para>
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public MessageModel Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public List<MessageModel> Msgs { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public MessageModel Remark { get; set; }
    }

    /// <summary>
    /// 消息具体结构
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 显示的字体颜色
        /// <para>示例：#173177</para>
        /// </summary>
        public string ColorBrush { get; set; }

        public JObject ToJObject(string key = null)
        {
            JObject jObject = new JObject();
            if (string.IsNullOrEmpty(key))
            {
                key = "value";
            }
            jObject[key] = Text;
            jObject["color"] = ColorBrush;
            return jObject;
        }
    }

    public enum MessageType
    {
        /// <summary>
        /// 余额不足提醒
        /// </summary>
        BalanceNotEnough,
        /// <summary>
        /// 商品揽收成功
        /// </summary>
        ProductCollect,
        /// <summary>
        /// 订单签收
        /// </summary>
        OrderSignFor,
        /// <summary>
        /// 余额变动提醒
        /// </summary>
        BalanceChanged,
        /// <summary>
        /// 充值到账提醒
        /// </summary>
        BalanceCharged,

        /// <summary>
        /// 订单出库提醒
        /// </summary>
        OrderDelivery,
    }

    public class SendMessageResult
    {
        /// <summary>
        /// 结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static SendMessageResult ToSuccess()
        {
            return new SendMessageResult() { Result = true };
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static SendMessageResult ToFail(string msg = null)
        {
            return new SendMessageResult() { Result = false, ErrorMsg = msg };
        }
    }
}
