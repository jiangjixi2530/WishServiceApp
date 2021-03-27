using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JT100.Wish.WeiXin
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
            TemplateIds.Add(MessageType.OrderSignFor, "Sop4fyd8aYZV73wp-NML7L_IgvLFnUNrsAlWiC9HAXo");
            TemplateIds.Add(MessageType.BalanceChanged, "jRoIVxcihzHirG-x_z72AavdiEjUfw6-r4YHdiFfBIY");
            TemplateIds.Add(MessageType.BalanceCharged, "m2wfUuCri5HVpW6aoc4dHO55GPm623ohpAitB85P1y8");
            TemplateIds.Add(MessageType.OrderDelivery, "vKfqUEMP84KS9tSC-EohDyoTUowmAALRdg7eYXgz1SY");
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
        internal static SendMessageResult SendMessage(Message message)
        {
            //如果失效 重新获取Token
            if (Token.InvalidTime.Subtract(DateTime.Now).TotalSeconds < 30)
            {
                GetToken();
            }
            if (!TemplateIds.ContainsKey(message.MessageType))
            {
                return SendMessageResult.ToFail("未找到对应的模板Id");
            }
            JObject msgObject = new JObject();
            msgObject["touser"] = message.ToUser;
            msgObject["template_id"] = TemplateIds[message.MessageType];
            JObject data = new JObject();
            msgObject["data"] = data;
            data["first"] = message.Title.ToJObject();
            for (int i = 0; i < message.Msgs.Count; i++)
            {
                string key = "keyword" + (i + 1).ToString();
                data[key] = message.Msgs[i].ToJObject();
            }
            data["remark"] = message.Remark.ToJObject();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["access_token"] = Token.Token;
            try
            {
                var result = HttpPost(SendTemplateMsgUrl, JsonConvert.SerializeObject(msgObject), dict);
                JObject resultObj = JsonConvert.DeserializeObject<JObject>(result);
                if (resultObj["errmsg"].ToString().ToUpper() == "OK")
                {
                    return SendMessageResult.ToSuccess();
                }
                else
                {
                    return SendMessageResult.ToFail(resultObj["errmsg"].ToString());
                }
            }
            catch (Exception ex)
            {

                return SendMessageResult.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 余额不足推送
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题 first.DATA</param>
        /// <param name="storeName">商户名称</param>
        /// <param name="account">账户</param>
        /// <param name="balance">账户余额</param>
        /// <param name="serviceName">客服专员</param>
        /// <param name="servicePhone">客服电话</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转链接，可不传</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendBalanceNotEnough(string toUser, string Title, string storeName, string account, string balance, string serviceName, string servicePhone, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.BalanceNotEnough;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(storeName));
            msg.Msgs.Add(new MessageModel(account));
            msg.Msgs.Add(new MessageModel(balance));
            msg.Msgs.Add(new MessageModel(serviceName));
            msg.Msgs.Add(new MessageModel(servicePhone));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
        }

        /// <summary>
        /// 商品揽收推送
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="productInfo">商品信息</param>
        /// <param name="telephone">收货电话</param>
        /// <param name="address">收货地址</param>
        /// <param name="status">物流状态</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转URL 可为空</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendProductCollect(string toUser, string Title, string orderNo, string productInfo, string telephone, string address, string status, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.ProductCollect;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(orderNo));
            msg.Msgs.Add(new MessageModel(productInfo));
            msg.Msgs.Add(new MessageModel(telephone));
            msg.Msgs.Add(new MessageModel(address));
            msg.Msgs.Add(new MessageModel(status));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
        }

        /// <summary>
        /// 订单签收推送
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题</param>
        /// <param name="orderNo">订单号</param>
        /// <param name="productInfo">商品信息</param>
        /// <param name="deliveryMode">配送方式</param>
        /// <param name="deliveryTime">签收时间</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转URL 可为空</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendOrderSignFor(string toUser, string Title, string orderNo, string productInfo, string deliveryMode, string deliveryTime, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.OrderSignFor;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(orderNo));
            msg.Msgs.Add(new MessageModel(productInfo));
            msg.Msgs.Add(new MessageModel(deliveryMode));
            msg.Msgs.Add(new MessageModel(deliveryTime));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
        }

        /// <summary>
        /// 余额变动提醒
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题</param>
        /// <param name="changeType">变动类型</param>
        /// <param name="changeAmount">变动金额</param>
        /// <param name="balance">卡上余额</param>
        /// <param name="changeStore">变动门店</param>
        /// <param name="changeTime">变动时间</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转URL 可为空</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendBalanceChanged(string toUser, string Title, string changeType, string changeAmount, string balance, string changeStore, string changeTime, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.BalanceChanged;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(changeType));
            msg.Msgs.Add(new MessageModel(changeAmount));
            msg.Msgs.Add(new MessageModel(balance));
            msg.Msgs.Add(new MessageModel(changeStore));
            msg.Msgs.Add(new MessageModel(changeTime));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
        }

        /// <summary>
        /// 会员充值到账推送
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题</param>
        /// <param name="storeName">酒店名称</param>
        /// <param name="account">会员卡号</param>
        /// <param name="chargeAmount">到账金额</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转URL 可为空</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendBalanceCharged(string toUser, string Title, string storeName, string account, string chargeAmount, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.BalanceCharged;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(storeName));
            msg.Msgs.Add(new MessageModel(account));
            msg.Msgs.Add(new MessageModel(chargeAmount));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
        }

        /// <summary>
        /// 出库成功推送
        /// </summary>
        /// <param name="toUser">微信用户唯一Id</param>
        /// <param name="Title">标题</param>
        /// <param name="orderNo">出库单号</param>
        /// <param name="storeName">发货公司</param>
        /// <param name="productInfo">商品数量</param>
        /// <param name="deliveryTime">业务日期</param>
        /// <param name="remark">备注</param>
        /// <param name="url">跳转URL 可为空</param>
        /// <returns></returns>
        public static async Task<SendMessageResult> SendOrderDelivery(string toUser, string Title, string orderNo, string storeName, string productInfo, string deliveryTime, string remark, string url = null)
        {
            Message msg = new Message();
            msg.ToUser = toUser;
            msg.MessageType = MessageType.OrderDelivery;
            msg.Title = new MessageModel(Title);
            msg.Msgs = new List<MessageModel>();
            msg.Msgs.Add(new MessageModel(orderNo));
            msg.Msgs.Add(new MessageModel(storeName));
            msg.Msgs.Add(new MessageModel(productInfo));
            msg.Msgs.Add(new MessageModel(deliveryTime));
            msg.Url = url;
            msg.Remark = new MessageModel(remark);
            return await Task.Run(() => SendMessage(msg));
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
        public MessageModel(string text)
        {
            Text = text;
        }
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 显示的字体颜色
        /// <para>示例：#173177</para>
        /// </summary>
        public string ColorBrush { get; set; } = "#173177";

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
