using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Core
{
    /// <summary>
    /// Api结果集
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public ApiCodeType Status { get; set; }

        /// <summary>
        /// 业务是否执行成果
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回内容
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// 执行成果结果集
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ApiResult ToSuccess(string response)
        {
            return response.ToModel<ApiResult>();
        }

        /// <summary>
        /// 返回失败结果集
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult ToFail(int statusCode, string msg = "")
        {
            ApiResult result = new ApiResult();
            result.Success = false;
            result.Msg = msg;
            return result;
        }
    }


    /// <summary>
    /// Api结果集
    /// </summary>
    public class ApiResult<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public ApiCodeType Status { get; set; }

        /// <summary>
        /// 业务是否执行成果
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 返回内容
        /// </summary>
        public T Response { get; set; }

        /// <summary>
        /// 执行成果结果集
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ApiResult<T> ToSuccess(string response)
        {
            return response.ToModel<ApiResult<T>>();
        }

        /// <summary>
        /// 返回失败结果集
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult<T> ToFail(string msg = "")
        {
            ApiResult<T> result = new ApiResult<T>();
            result.Success = false;
            result.Msg = msg;
            return result;
        }
    }

    public enum ApiCodeType : int
    {
        /// <summary>
        /// 请求成
        /// </summary>
        Success = 200,

        /// <summary>
        /// 登录验证错误
        /// </summary>
        LoginError = 403,

        /// <summary>
        /// 请求响应错误
        /// </summary>
        ResponseError = 500,

    }
}
