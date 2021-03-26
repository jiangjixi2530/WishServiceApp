using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Core.SysInteface
{
    /// <summary>
    /// AppConfig
    /// </summary>
    interface IAppConfig : IConfig
    {
        IConfig SysConfig { get; set; }

        IConfig UserConfig { get; set; }
    }
}
