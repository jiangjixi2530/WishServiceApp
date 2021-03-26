using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace JT100.Wish.Tool
{
    /// <summary>
    /// 消息接收
    /// </summary>
    /// <param name="btAryBuffer"></param>
    public delegate void MessageReceivedEventHandler(byte[] btAryBuffer);
    interface ITalker
    {
        /// <summary>
        /// 数据接收
        /// </summary>
        event MessageReceivedEventHandler MessageReceived;    // 接收到发来的消息

        /// <summary>
        /// IP连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="strException"></param>
        /// <returns></returns>
        bool Connect(IPAddress ip, int port, out string strException);  
        
        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="btAryBuffer"></param>
        /// <returns></returns>
        bool SendMessage(byte[] btAryBuffer);      
        
        /// <summary>
        /// 注销连接
        /// </summary>
        void SignOut();                                       

        /// <summary>
        /// 是否已连接
        /// </summary>
        /// <returns></returns>
        bool IsConnect();
    }
}
