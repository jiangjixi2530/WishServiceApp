
using JT100.Wish.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class ApiHelper
    {
        private ApiManager ApiManager { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        private string ServerUrl;

        private string Token { get; set; }
        /// <summary>
        /// API帮助类
        /// </summary>
        public ApiHelper(string serverUrl)
        {
            ServerUrl = serverUrl;
            ApiManager = new ApiManager();
        }

        /// <summary>
        ///登录
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public ApiResult<string> Login(string accountNo, string pwd)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("name", accountNo);
                dict.Add("pwd", pwd);
                var result = ApiManager.HttpGet<string>(ServerUrl + "/api/Login/Login", dict);
                //登录成功 自动更新Token
                if (result.Success)
                {
                    Token = result.Response;
                }
                return result;
            }
            catch (Exception ex)
            {
                return ApiResult<string>.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 获取客户列表
        /// </summary>
        /// <returns></returns>
        public List<CustomModel> GetCustomList()
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("token", Token);
                dict.Add("page", 1);
                dict.Add("pageSize", 999);
                var result = ApiManager.HttpGet<PageModel<CustomModel>>(ServerUrl + "/api/Customer/GetCustomerList", dict);
                if (result.Success)
                {
                    return result.Response.Data;
                }
            }
            catch (Exception ex)
            {
            }
            return new List<CustomModel>();
        }

        public List<OutOrder> GetCustomOutOrders(int customId)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("token", Token);
                dict.Add("customerId", customId);
                dict.Add("page", 1);
                dict.Add("pageSize", 100);
                var result = ApiManager.HttpGet<PageModel<OutOrder>>(ServerUrl + "/api/OutOrder/GetCustomerOutOrderList", dict);
                if (result.Success)
                {
                    return result.Response.Data;
                }
            }
            catch (Exception ex)
            {
            }
            return new List<OutOrder>();
        }

        public List<WareTypeModel> GetGroupDataBySN(List<string> sns)
        {
            try
            {
                JObject jObject = new JObject();
                jObject["token"] = Token;
                JArray jArray = new JArray();
                foreach (var sn in sns)
                {
                    jArray.Add(sn);
                }
                jObject["wareSNCodes"] = jArray;
                var result = ApiManager.HttpPost<List<WareTypeModel>>(ServerUrl + "/api/OutOrder/GetGroupDataBySNCodes", JsonConvert.SerializeObject(jObject));
                if (result.Success)
                {
                    return result.Response;
                }
            }
            catch (Exception ex)
            {
            }
            return new List<WareTypeModel>();
        }

        /// <summary>
        /// 创建出库单
        /// </summary>
        /// <param name="customId"></param>
        /// <param name="sns"></param>
        /// <returns></returns>
        public ApiResult<string> CreateOutOrder(int customId, List<string> sns)
        {
            try
            {
                JObject jObject = new JObject();
                jObject["token"] = Token;
                jObject["customerId"] = customId;
                JArray jArray = new JArray();
                foreach (var sn in sns)
                {
                    jArray.Add(sn);
                }
                jObject["wareSNCodes"] = jArray;
                return ApiManager.HttpPost<string>(ServerUrl + "/api/OutOrder/CreateOutOrder", JsonConvert.SerializeObject(jObject));
            }
            catch (Exception ex)
            {
                return ApiResult<string>.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 获取入库单列表
        /// </summary>
        /// <returns></returns>
        public List<InOrder> GetCustomInOrders()
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("token", Token);
                dict.Add("page", 1);
                dict.Add("pageSize", 100);
                var result = ApiManager.HttpGet<PageModel<InOrder>>(ServerUrl + "/api/InOrder/GetCustomerInOrderList", dict);
                if (result.Success)
                {
                    return result.Response.Data;
                }
            }
            catch (Exception ex)
            {
            }
            return new List<InOrder>();
        }

        /// <summary>
        /// 查询SN号属于哪个客户
        /// </summary>
        /// <param name="SN"></param>
        /// <returns></returns>
        public int GetCustomIdBySN(string SN)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("token", Token);
                dict.Add("wareSNCode", SN);
                var result = ApiManager.HttpGet<int>(ServerUrl + "/api/Ware/GetCustomerIdByWareSNCode", dict);
                if (result.Success)
                {
                    return result.Response;
                }
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        /// <summary>
        /// 清点入库单
        /// </summary>
        /// <param name="SNs"></param>
        /// <returns></returns>
        public ApiResult<string> CheckInOrder(string orderNum, List<string> SNs)
        {
            try
            {
                JObject jObject = new JObject();
                jObject["token"] = Token;
                jObject["orderNo"] = orderNum;
                JArray jArray = new JArray();
                foreach (var sn in SNs)
                {
                    jArray.Add(sn);
                }
                jObject["wareSNcodes"] = jArray;
                return ApiManager.HttpPost<string>(ServerUrl + "/api/InOrder/CheckInOrder", JsonConvert.SerializeObject(jObject));
            }
            catch (Exception ex)
            {
                return ApiResult<string>.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 清点反洗单
        /// </summary>
        /// <param name="SNs"></param>
        /// <returns></returns>
        public ApiResult<string> CheckRewashOrder(string orderNum, List<string> SNs)
        {
            try
            {
                JObject jObject = new JObject();
                jObject["token"] = Token;
                jObject["orderNo"] = orderNum;
                JArray jArray = new JArray();
                foreach (var sn in SNs)
                {
                    jArray.Add(sn);
                }
                jObject["wareSNcodes"] = jArray;
                return ApiManager.HttpPost<string>(ServerUrl + "/api/InOrder/CheckRewashOrder", JsonConvert.SerializeObject(jObject));
            }
            catch (Exception ex)
            {
                return ApiResult<string>.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 标签二维码绑定
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="qrCode"></param>
        /// <returns></returns>
        public ApiResult<string> BindWareSNCode(string sn, string qrCode)
        {
            try
            {
                JObject jObject = new JObject();
                jObject["token"] = Token;
                jObject["wareCodes"] = qrCode;
                jObject["wareSNCodes"] = sn;
                return ApiManager.HttpPost<string>(ServerUrl + "/api/Ware/BindWareSNCode", JsonConvert.SerializeObject(jObject));
            }
            catch (Exception ex)
            {
                return ApiResult<string>.ToFail(ex.Message);
            }
        }

        /// <summary>
        /// 获取所有商品信息
        /// </summary>
        /// <returns></returns>
        public List<WareInfo> GetAllWareInfo()
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("token", Token);
                dict.Add("page", 1);
                dict.Add("pageSize", 10000);
                var result = ApiManager.HttpGet<PageModel<WareInfo>>(ServerUrl + "/api/Ware/Get", dict);
                if (result.Success)
                {
                    return result.Response.Data;
                }
            }
            catch (Exception ex)
            {
            }
            return new List<WareInfo>();
        }
    }
}
