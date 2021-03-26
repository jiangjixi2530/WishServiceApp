using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JT100.Wish.Core
{
    public class ApiManager
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        private readonly int TimeOut = 60 * 1000;

        public ApiManager()
        {
        }
        /// <summary>
        /// Post请求接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public ApiResult<T> HttpPost<T>(string url,string json, int timeOut = 0)
        {
           
            var request = HttpWebRequest.Create(url);
            request.Method = "Post";
            request.Timeout = timeOut == 0 ? TimeOut : timeOut;
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
            if (string.IsNullOrWhiteSpace(text))
            {
                return ApiResult<T>.ToFail($"接口返回内容异常！");
            }
            return ApiResult<T>.ToSuccess(text);
        }
        /// <summary>
        /// Post请求接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="paramsDict"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public ApiResult<T> HttpPost<T>(string url, Dictionary<string, object> paramsDict = null, int timeOut = 0)
        {
            var param = paramsDict.Select(x => string.Concat(x.Key, "=", x.Value)).ToArray();
            var requestParams = string.Empty;
            if (param.Any())
            {
                requestParams = string.Join('&', param);
            }
            var request = HttpWebRequest.Create(url);
            request.Method = "Post";
            request.Timeout = timeOut == 0 ? TimeOut : timeOut;
            request.ContentType = "application/json";
            string text = string.Empty;
            if (!string.IsNullOrEmpty(requestParams))
            {
                var byteArray = Encoding.UTF8.GetBytes(requestParams);
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
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return ApiResult<T>.ToFail("服务器响应失败");
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                return ApiResult<T>.ToFail($"接口返回内容异常！");
            }
            return ApiResult<T>.ToSuccess(text);
        }

        public ApiResult<T> HttpGet<T>(string url, Dictionary<string, object> paramsDict = null, int timeOut = 0)
        {
            var param = paramsDict.Select(x => string.Concat(x.Key, "=", x.Value)).ToArray();
            var requestParams = string.Empty;
            if (param.Any())
            {
                requestParams = string.Join('&', param);
            }
            var request = HttpWebRequest.Create(url + (string.IsNullOrEmpty(requestParams) ? "" : ("?" + requestParams)));
            request.Method = "Get";
            request.Timeout = timeOut == 0 ? TimeOut : timeOut;
            request.ContentType = "text/html;charset=UTF-8";
            string text = string.Empty;
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                text = reader.ReadToEnd();
            }
            response.Close();
            request = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return ApiResult<T>.ToFail($"接口返回内容异常！");
            }
            return ApiResult<T>.ToSuccess(text);
        }
    }
}
