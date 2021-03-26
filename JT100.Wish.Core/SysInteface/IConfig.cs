using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Core.SysInteface
{
    /// <summary>
    /// 应用程序配置
    /// </summary>
    interface IConfig
    {
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set<T>(string key, T value);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        bool Commit();
    }
}
