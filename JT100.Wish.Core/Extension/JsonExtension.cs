using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Core
{
    public static class JsonExtension
    {
        /// <summary>
        /// 转Json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            if (obj is string)
            {
                return obj.ToString();
            }
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json转实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ToModel<T>(this string json)
        {
            var t = System.Activator.CreateInstance<T>();
            if (string.IsNullOrEmpty(json))
                return t;
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                return t;
            }
        }
    }
}
