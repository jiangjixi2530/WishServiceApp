using JT100.Wish.Component.Common.Models;
using JT100.Wish.Core;
using JT100.Wish.Tool;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT100.Wish.Component
{
    public class UserContext
    {
        static UserContext()
        {
            LogHelper.WriteLog(LogType.BASE, "初始化");
            UserContext.UserXmlProvider = new UserXmlProvider();
            UserContext.RfidReadProvider = new RfidReadProvider();
            UserContext.ApiHelper = new ApiHelper(UserXmlProvider.GetSysConfig<SysConfig>().ServerUrl);
            var readConfig = UserXmlProvider.GetConfig<RfidReadConfig>("RfidReadConfig");
            LogHelper.WriteLog(LogType.BASE, "读取配置");
            if (readConfig != null && !string.IsNullOrEmpty(readConfig.ComPort))
            {
                List<int> antennas = new List<int>();
                if (readConfig.AntennaOne)
                {
                    antennas.Add(1);
                }
                if (readConfig.AntennaTwo)
                {
                    antennas.Add(2);
                }
                if (readConfig.AntennaThree)
                {
                    antennas.Add(3);
                }
                if (readConfig.AntennaFour)
                {
                    antennas.Add(4);
                }
                LogHelper.WriteLog(LogType.BASE, "串口号：" + readConfig.ComPort);
                try
                {
                    RfidReadProvider.InitializeCom(readConfig.ComPort, readConfig.Baudrate, antennas);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(LogType.BASE, ex.Message);
                }
            }
        }
        /// <summary>
        /// 网关接口调用
        /// </summary>
        public static ApiHelper ApiHelper { get; private set; }

        /// <summary>
        /// XML操作
        /// </summary>
        public static UserXmlProvider UserXmlProvider { get; private set; }

        /// <summary>
        /// Rfid读写器
        /// </summary>
        public static RfidReadProvider RfidReadProvider { get; private set; }
    }
}
