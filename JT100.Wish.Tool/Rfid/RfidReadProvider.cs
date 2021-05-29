using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace JT100.Wish.Tool
{
    public delegate void SettingChangedHandler(object o, ReaderSetting newSetting);
    public delegate void DataReceivedHandle(object o, string epc);
    public class RfidReadProvider
    {
        private ReaderMethod reader = new ReaderMethod();
        private InventoryBuffer curInventoryBuffer = new InventoryBuffer();
        private OperateTagBuffer curOperateTagBuffer = new OperateTagBuffer();
        private ReaderSetting curSetting = new ReaderSetting();

        /// <summary>
        /// 配置变化
        /// </summary>
        public event SettingChangedHandler OnSettingChanged;
        public event DataReceivedHandle OnDataReceived;
        /// <summary>
        /// 构造函数 
        /// </summary>
        public RfidReadProvider()
        {
            reader.AnalyCallback = AnalyData;
            reader.ReceiveCallback = ReceiveData;
            reader.SendCallback = SendData;
        }

        /// <summary>
        /// 初始化串口
        /// </summary>
        public void InitializeCom(string comPort, int baudate, List<int> ans)
        {
            if (comPort == null || ans == null || ans.Count == 0)
            {
                return;
            }
            this.antennas.Clear();
            this.antennas.AddRange(ans);
            DisconnectByCom();
            if(ConnectByCom(comPort, baudate <= 0 ? 115200 : baudate))
            {
                SendReadCommand();
            }
        }

        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="port">串口号</param>
        /// <param name="baudrate">波特率</param>
        /// <returns></returns>
        public bool ConnectByCom(string port, int baudrate = 115200)
        {
            LogHelper.WriteLog(LogType.BASE, "初始化串口");
            if (string.IsNullOrEmpty(port))
            {
                LogHelper.WriteLog(LogType.BASE, "无串口号");
                throw new ArgumentNullException("Port is null");
            }
            if (baudrate <= 0)
            {
                LogHelper.WriteLog(LogType.BASE, "波特率小于0");
                throw new ArgumentNullException("Baudrate not available");
            }
            string outExceptionStr;
            try
            {
                int ret = reader.OpenCom(port, baudrate, out outExceptionStr);
                if (ret == 0)
                {
                    LogHelper.WriteLog(LogType.BASE, "初始化成功");
                    reader.GetFrequencyRegion(curSetting.btReadId);
                    Thread.Sleep(5);
                    return true;
                }
                LogHelper.WriteLog(LogType.BASE, "初始化失败");
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// 断开串口连接
        /// </summary>
        /// <returns></returns>
        public bool DisconnectByCom()
        {
            reader.CloseCom();
            return true;
        }

        /// <summary>
        /// 发送读取命令
        /// </summary>
        public void SendReadCommand()
        {
            curInventoryBuffer.ClearInventoryPar();
            if (antennas.Count == 0)
            {
                return;
            }
            //设置天线
            //天线1
            if (antennas.Contains(1))
            {
                curInventoryBuffer.lAntenna.Add(0x00);
            }
            //天线2
            if (antennas.Contains(2))
            {
                curInventoryBuffer.lAntenna.Add(0x01);
            }
            //天线3
            if (antennas.Contains(3))
            {
                curInventoryBuffer.lAntenna.Add(0x02);
            }
            //天线4
            if (antennas.Contains(4))
            {
                curInventoryBuffer.lAntenna.Add(0x03);
            }
            curInventoryBuffer.btRepeat = 1;
            curInventoryBuffer.bLoopInventoryReal = true;
            curInventoryBuffer.bLoopInventory = true;
            curInventoryBuffer.ClearInventoryRealResult();
            byte btWorkAntenna = curInventoryBuffer.lAntenna[curInventoryBuffer.nIndexAntenna];
            reader.SetWorkAntenna(curSetting.btReadId, btWorkAntenna);
        }

        #region Config

        /// <summary>
        /// 循环命令次数
        /// </summary>
        public int RealRound { get; set; } = 1;

        private List<int> antennas = new List<int>();

        /// <summary>
        /// 添加天线
        /// </summary>
        /// <param name="antenna">天线序号1,2,3,4</param>
        public void AddAntena(int antenna)
        {
            if (antennas.Contains(antenna))
            {
                return;
            }
            antennas.Add(antenna);
        }

        /// <summary>
        /// 删除天线
        /// </summary>
        /// <param name="antenna"></param>
        public void Remove(int antenna)
        {
            if (antennas.Contains(antenna))
            {
                antennas.Remove(antenna);
            }
        }
        #endregion

        #region callBack
        private void AnalyData(MessageTran msgTran)
        {
            if (msgTran.PacketType != 0xA0)
            {
                return;
            }
            switch (msgTran.Cmd)
            {
                case 0x69:
                    ProcessSetProfile(msgTran);
                    break;
                case 0x6A:
                    ProcessGetProfile(msgTran);
                    break;
                case 0x71:
                    ProcessSetUartBaudrate(msgTran);
                    break;
                case 0x72:
                    ProcessGetFirmwareVersion(msgTran);
                    break;
                case 0x73:
                    ProcessSetReadAddress(msgTran);
                    break;
                case 0x74:
                    ProcessSetWorkAntenna(msgTran);
                    break;
                case 0x75:
                    ProcessGetWorkAntenna(msgTran);
                    break;
                case 0x76:
                    ProcessSetOutputPower(msgTran);
                    break;
                case 0x77:
                    ProcessGetOutputPower(msgTran);
                    break;
                case 0x78:
                    ProcessSetFrequencyRegion(msgTran);
                    break;
                case 0x79:
                    ProcessGetFrequencyRegion(msgTran);
                    break;
                case 0x7A:
                    ProcessSetBeeperMode(msgTran);
                    break;
                case 0x7B:
                    ProcessGetReaderTemperature(msgTran);
                    break;
                case 0x7C:
                    ProcessSetDrmMode(msgTran);
                    break;
                case 0x7D:
                    ProcessGetDrmMode(msgTran);
                    break;
                case 0x7E:
                    ProcessGetImpedanceMatch(msgTran);
                    break;
                case 0x60:
                    ProcessReadGpioValue(msgTran);
                    break;
                case 0x61:
                    ProcessWriteGpioValue(msgTran);
                    break;
                case 0x62:
                    ProcessSetAntDetector(msgTran);
                    break;
                case 0x63:
                    ProcessGetAntDetector(msgTran);
                    break;
                case 0x67:
                    ProcessSetReaderIdentifier(msgTran);
                    break;
                case 0x68:
                    ProcessGetReaderIdentifier(msgTran);
                    break;

                case 0x80:
                    ProcessInventory(msgTran);
                    break;
                case 0x81:
                    ProcessReadTag(msgTran);
                    break;
                case 0x82:
                    ProcessWriteTag(msgTran);
                    break;
                case 0x83:
                    ProcessLockTag(msgTran);
                    break;
                case 0x84:
                    ProcessKillTag(msgTran);
                    break;
                case 0x85:
                    ProcessSetAccessEpcMatch(msgTran);
                    break;
                case 0x86:
                    ProcessGetAccessEpcMatch(msgTran);
                    break;

                case 0x89:
                case 0x8B:
                    ProcessInventoryReal(msgTran);
                    break;
                case 0x8A:
                    ProcessFastSwitch(msgTran);
                    break;
                case 0x8D:
                    ProcessSetMonzaStatus(msgTran);
                    break;
                case 0x8E:
                    ProcessGetMonzaStatus(msgTran);
                    break;
                case 0x90:
                    ProcessGetInventoryBuffer(msgTran);
                    break;
                case 0x91:
                    ProcessGetAndResetInventoryBuffer(msgTran);
                    break;
                case 0x92:
                    ProcessGetInventoryBufferTagCount(msgTran);
                    break;
                case 0x93:
                    ProcessResetInventoryBuffer(msgTran);
                    break;
                case 0xb0:
                    ProcessInventoryISO18000(msgTran);
                    break;
                case 0xb1:
                    ProcessReadTagISO18000(msgTran);
                    break;
                case 0xb2:
                    ProcessWriteTagISO18000(msgTran);
                    break;
                case 0xb3:
                    ProcessLockTagISO18000(msgTran);
                    break;
                case 0xb4:
                    ProcessQueryISO18000(msgTran);
                    break;
                default:
                    break;
            }
        }
        private void ReceiveData(byte[] btAryReceiveData)
        {
        }
        private void SendData(byte[] btArySendData)
        {
        }
        #endregion

        #region private

        private void SettingChanged()
        {
            OnSettingChanged?.Invoke(this, curSetting);
        }

        private void RunLoopInventroy()
        {
            //校验盘存是否所有天线均完成
            if (curInventoryBuffer.nIndexAntenna < curInventoryBuffer.lAntenna.Count - 1 || curInventoryBuffer.nCommond == 0)
            {
                if (curInventoryBuffer.nCommond == 0)
                {
                    curInventoryBuffer.nCommond = 1;

                    if (curInventoryBuffer.bLoopInventoryReal)
                    {
                        //m_bLockTab = true;
                        //btnInventory.Enabled = false;
                        if (curInventoryBuffer.bLoopCustomizedSession)//自定义Session和Inventoried Flag 
                        {
                            reader.CustomizedInventory(curSetting.btReadId, curInventoryBuffer.btSession, curInventoryBuffer.btTarget, curInventoryBuffer.btRepeat);
                        }
                        else //实时盘存
                        {
                            reader.InventoryReal(curSetting.btReadId, curInventoryBuffer.btRepeat);

                        }
                    }
                    else
                    {
                        if (curInventoryBuffer.bLoopInventory)
                            reader.Inventory(curSetting.btReadId, curInventoryBuffer.btRepeat);
                    }
                }
                else
                {
                    curInventoryBuffer.nCommond = 0;
                    curInventoryBuffer.nIndexAntenna++;

                    byte btWorkAntenna = curInventoryBuffer.lAntenna[curInventoryBuffer.nIndexAntenna];
                    reader.SetWorkAntenna(curSetting.btReadId, btWorkAntenna);
                    curSetting.btWorkAntenna = btWorkAntenna;
                }
            }
            //校验是否循环盘存
            else if (curInventoryBuffer.bLoopInventory)
            {
                curInventoryBuffer.nIndexAntenna = 0;
                curInventoryBuffer.nCommond = 0;

                byte btWorkAntenna = curInventoryBuffer.lAntenna[curInventoryBuffer.nIndexAntenna];
                reader.SetWorkAntenna(curSetting.btReadId, btWorkAntenna);
                curSetting.btWorkAntenna = btWorkAntenna;
            }
        }
        private void ProcessSetProfile(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btLinkProfile = msgTran.AryData[0];
                    SettingChanged();
                }
            }
        }
        private void ProcessGetProfile(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if ((msgTran.AryData[0] >= 0xd0) && (msgTran.AryData[0] <= 0xd3))
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btLinkProfile = msgTran.AryData[0];
                    SettingChanged();
                }
            }
        }
        private void ProcessSetUartBaudrate(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetFirmwareVersion(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 2)
            {
                curSetting.btMajor = msgTran.AryData[0];
                curSetting.btMinor = msgTran.AryData[1];
                curSetting.btReadId = msgTran.ReadId;
                SettingChanged();
            }
        }
        private void ProcessSetReadAddress(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessSetWorkAntenna(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    RunLoopInventroy();
                    SettingChanged();
                    return;
                }
            }
            curInventoryBuffer.nCommond = 1;
            curInventoryBuffer.dtEndInventory = DateTime.Now;
            SettingChanged();
            RunLoopInventroy();
        }
        private void ProcessGetWorkAntenna(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x00 || msgTran.AryData[0] == 0x01 || msgTran.AryData[0] == 0x02 || msgTran.AryData[0] == 0x03)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btWorkAntenna = msgTran.AryData[0];
                    SettingChanged();
                }
            }
        }
        private void ProcessSetOutputPower(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetOutputPower(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btOutputPower = msgTran.AryData[0];
                SettingChanged();
            }
        }
        private void ProcessSetFrequencyRegion(MessageTran msgTran)
        {
            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetFrequencyRegion(MessageTran msgTran)
        {
            string strCmd = "取得射频规范";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 3)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btRegion = msgTran.AryData[0];
                curSetting.btFrequencyStart = msgTran.AryData[1];
                curSetting.btFrequencyEnd = msgTran.AryData[2];
                SettingChanged();
            }
            else if (msgTran.AryData.Length == 6)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btRegion = msgTran.AryData[0];
                curSetting.btUserDefineFrequencyInterval = msgTran.AryData[1];
                curSetting.btUserDefineChannelQuantity = msgTran.AryData[2];
                curSetting.nUserDefineStartFrequency = msgTran.AryData[3] * 256 * 256 + msgTran.AryData[4] * 256 + msgTran.AryData[5];
                SettingChanged();
            }
        }
        private void ProcessSetBeeperMode(MessageTran msgTran)
        {
            string strCmd = "设置蜂鸣器模式";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetReaderTemperature(MessageTran msgTran)
        {
            string strCmd = "取得读写器温度";

            if (msgTran.AryData.Length == 2)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btPlusMinus = msgTran.AryData[0];
                curSetting.btTemperature = msgTran.AryData[1];
                SettingChanged();
            }
        }
        private void ProcessSetDrmMode(MessageTran msgTran)
        {
            string strCmd = "设置DRM模式";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetDrmMode(MessageTran msgTran)
        {
            string strCmd = "取得DRM模式";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x00 || msgTran.AryData[0] == 0x01)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btDrmMode = msgTran.AryData[0];
                    SettingChanged();
                }
            }
        }
        private void ProcessGetImpedanceMatch(MessageTran msgTran)
        {
            string strCmd = "测量天线端口阻抗匹配";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btAntImpedance = msgTran.AryData[0];
                SettingChanged();
            }
        }
        private void ProcessReadGpioValue(MessageTran msgTran)
        {
            string strCmd = "读取GPIO状态";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 2)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btGpio1Value = msgTran.AryData[0];
                curSetting.btGpio2Value = msgTran.AryData[1];
                SettingChanged();
            }
        }
        private void ProcessWriteGpioValue(MessageTran msgTran)
        {
            string strCmd = "设置GPIO状态";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessSetAntDetector(MessageTran msgTran)
        {
            string strCmd = "设置天线连接检测阈值";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();

                }
            }
        }
        private void ProcessGetAntDetector(MessageTran msgTran)
        {
            string strCmd = "读取天线连接检测阈值";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                curSetting.btReadId = msgTran.ReadId;
                curSetting.btAntDetector = msgTran.AryData[0];
                SettingChanged();
            }
        }
        private void ProcessSetReaderIdentifier(MessageTran msgTran)
        {
            string strCmd = "设置读写器识别标记";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    SettingChanged();
                }
            }
        }
        private void ProcessGetReaderIdentifier(MessageTran msgTran)
        {
            string strCmd = "读取读写器识别标记";
            string strErrorCode = string.Empty;
            short i;
            string readerIdentifier = "";

            if (msgTran.AryData.Length == 12)
            {
                curSetting.btReadId = msgTran.ReadId;
                for (i = 0; i < 12; i++)
                {
                    readerIdentifier = readerIdentifier + string.Format("{0:X2}", msgTran.AryData[i]) + " ";
                }
                curSetting.btReaderIdentifier = readerIdentifier;
                SettingChanged();
            }
        }
        private void SetMaxMinRSSI(int nRSSI)
        {
            if (curInventoryBuffer.nMaxRSSI < nRSSI)
            {
                curInventoryBuffer.nMaxRSSI = nRSSI;
            }

            if (curInventoryBuffer.nMinRSSI == 0)
            {
                curInventoryBuffer.nMinRSSI = nRSSI;
            }
            else if (curInventoryBuffer.nMinRSSI > nRSSI)
            {
                curInventoryBuffer.nMinRSSI = nRSSI;
            }
        }
        private string GetFreqString(byte btFreq)
        {
            string strFreq = string.Empty;

            if (curSetting.btRegion == 4)
            {
                float nExtraFrequency = btFreq * curSetting.btUserDefineFrequencyInterval * 10;
                float nstartFrequency = ((float)curSetting.nUserDefineStartFrequency) / 1000;
                float nStart = nstartFrequency + nExtraFrequency / 1000;
                string strTemp = nStart.ToString("0.000");
                return strTemp;
            }
            else
            {
                if (btFreq < 0x07)
                {
                    float nStart = 865.00f + Convert.ToInt32(btFreq) * 0.5f;

                    string strTemp = nStart.ToString("0.00");

                    return strTemp;
                }
                else
                {
                    float nStart = 902.00f + (Convert.ToInt32(btFreq) - 7) * 0.5f;

                    string strTemp = nStart.ToString("0.00");

                    return strTemp;
                }
            }
        }
        private void ProcessInventoryReal(MessageTran msgTran)
        {
            string strCmd = "";
            if (msgTran.Cmd == 0x89)
            {
                strCmd = "实时盘存";
            }
            if (msgTran.Cmd == 0x8B)
            {
                strCmd = "自定义Session和Inventoried Flag盘存";
            }
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                RunLoopInventroy();
            }
            else if (msgTran.AryData.Length == 7)
            {
                curInventoryBuffer.nReadRate = Convert.ToInt32(msgTran.AryData[1]) * 256 + Convert.ToInt32(msgTran.AryData[2]);
                curInventoryBuffer.nDataCount = Convert.ToInt32(msgTran.AryData[3]) * 256 * 256 * 256 + Convert.ToInt32(msgTran.AryData[4]) * 256 * 256 + Convert.ToInt32(msgTran.AryData[5]) * 256 + Convert.ToInt32(msgTran.AryData[6]);
                RunLoopInventroy();
            }
            else
            {
                int nLength = msgTran.AryData.Length;
                int nEpcLength = nLength - 4;
                string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, nEpcLength);
                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 2);
                string strRSSI = msgTran.AryData[nLength - 1].ToString();
                SetMaxMinRSSI(Convert.ToInt32(msgTran.AryData[nLength - 1]));
                byte btTemp = msgTran.AryData[0];
                byte btAntId = (byte)((btTemp & 0x03) + 1);
                curInventoryBuffer.nCurrentAnt = btAntId;
                string strAntId = btAntId.ToString();
                byte btFreq = (byte)(btTemp >> 2);
                string strFreq = GetFreqString(btFreq);
                strEPC = strEPC.Replace(" ", "");
                try
                {
                    OnDataReceived?.Invoke(this, strEPC);
                }
                catch (Exception ex)
                {

                }
                //DataRow row1 = curInventoryBuffer.dtTagTable.NewRow();
                //row1[0] = strPC;
                //row1[2] = strEPC;
                //row1[4] = strRSSI;
                //row1[5] = "1";
                //row1[6] = strFreq;
                //curInventoryBuffer.dtTagTable.Rows.Add(row1);
                //curInventoryBuffer.dtTagTable.AcceptChanges();
                //curInventoryBuffer.dtEndInventory = DateTime.Now;
            }
        }
        private void ProcessInventory(MessageTran msgTran)
        {
            string strCmd = "盘存标签";
            string strErrorCode = string.Empty;
            if (msgTran.AryData.Length == 9)
            {
                curInventoryBuffer.nCurrentAnt = msgTran.AryData[0];
                curInventoryBuffer.nTagCount = Convert.ToInt32(msgTran.AryData[1]) * 256 + Convert.ToInt32(msgTran.AryData[2]);
                curInventoryBuffer.nReadRate = Convert.ToInt32(msgTran.AryData[3]) * 256 + Convert.ToInt32(msgTran.AryData[4]);
                int nTotalRead = Convert.ToInt32(msgTran.AryData[5]) * 256 * 256 * 256
                    + Convert.ToInt32(msgTran.AryData[6]) * 256 * 256
                    + Convert.ToInt32(msgTran.AryData[7]) * 256
                    + Convert.ToInt32(msgTran.AryData[8]);
                curInventoryBuffer.nDataCount = nTotalRead;
                curInventoryBuffer.lTotalRead.Add(nTotalRead);
                curInventoryBuffer.dtEndInventory = DateTime.Now;
                RunLoopInventroy();
                return;
            }
            RunLoopInventroy();
        }
        private void ProcessReadTag(MessageTran msgTran)
        {
            string strCmd = "读标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                return;
            }
            else
            {
                int nLen = msgTran.AryData.Length;
                int nDataLen = Convert.ToInt32(msgTran.AryData[nLen - 3]);
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - nDataLen - 4;

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strData = CCommondMethod.ByteArrayToString(msgTran.AryData, 7 + nEpcLen, nDataLen);

                byte byTemp = msgTran.AryData[nLen - 2];
                byte byAntId = (byte)((byTemp & 0x03) + 1);
                string strAntId = byAntId.ToString();

                string strReadCount = msgTran.AryData[nLen - 1].ToString();

                DataRow row = curOperateTagBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEPC;
                row[3] = strData;
                row[4] = nDataLen.ToString();
                row[5] = strAntId;
                row[6] = strReadCount;

                curOperateTagBuffer.dtTagTable.Rows.Add(row);
                curOperateTagBuffer.dtTagTable.AcceptChanges();

                //RefreshOpTag(0x81);
            }
        }
        private void ProcessWriteTag(MessageTran msgTran)
        {
            string strCmd = "写标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                int nLen = msgTran.AryData.Length;
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - 4;

                if (msgTran.AryData[nLen - 3] != 0x10)
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[nLen - 3]);
                    string strLog = strCmd + "失败，失败原因： " + strErrorCode;
                    return;
                }

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strData = string.Empty;

                byte byTemp = msgTran.AryData[nLen - 2];
                byte byAntId = (byte)((byTemp & 0x03) + 1);
                string strAntId = byAntId.ToString();

                string strReadCount = msgTran.AryData[nLen - 1].ToString();

                DataRow row = curOperateTagBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEPC;
                row[3] = strData;
                row[4] = string.Empty;
                row[5] = strAntId;
                row[6] = strReadCount;

                curOperateTagBuffer.dtTagTable.Rows.Add(row);
                curOperateTagBuffer.dtTagTable.AcceptChanges();

                //RefreshOpTag(0x82);
            }
        }
        private void ProcessLockTag(MessageTran msgTran)
        {
            string strCmd = "锁定标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                int nLen = msgTran.AryData.Length;
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - 4;

                if (msgTran.AryData[nLen - 3] != 0x10)
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[nLen - 3]);
                    string strLog = strCmd + "失败，失败原因： " + strErrorCode;
                    return;
                }

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strData = string.Empty;

                byte byTemp = msgTran.AryData[nLen - 2];
                byte byAntId = (byte)((byTemp & 0x03) + 1);
                string strAntId = byAntId.ToString();

                string strReadCount = msgTran.AryData[nLen - 1].ToString();

                DataRow row = curOperateTagBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEPC;
                row[3] = strData;
                row[4] = string.Empty;
                row[5] = strAntId;
                row[6] = strReadCount;

                curOperateTagBuffer.dtTagTable.Rows.Add(row);
                curOperateTagBuffer.dtTagTable.AcceptChanges();

                //RefreshOpTag(0x83);
            }
        }
        private void ProcessKillTag(MessageTran msgTran)
        {
            string strCmd = "销毁标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                int nLen = msgTran.AryData.Length;
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - 4;

                if (msgTran.AryData[nLen - 3] != 0x10)
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[nLen - 3]);
                    string strLog = strCmd + "失败，失败原因： " + strErrorCode;
                    return;
                }

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strData = string.Empty;

                byte byTemp = msgTran.AryData[nLen - 2];
                byte byAntId = (byte)((byTemp & 0x03) + 1);
                string strAntId = byAntId.ToString();

                string strReadCount = msgTran.AryData[nLen - 1].ToString();

                DataRow row = curOperateTagBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEPC;
                row[3] = strData;
                row[4] = string.Empty;
                row[5] = strAntId;
                row[6] = strReadCount;

                curOperateTagBuffer.dtTagTable.Rows.Add(row);
                curOperateTagBuffer.dtTagTable.AcceptChanges();

                //RefreshOpTag(0x84);
                //WriteLog(lrtxtLog, strCmd, 0);
            }
        }
        private void ProcessSetAccessEpcMatch(MessageTran msgTran)
        {
            string strCmd = "选定/取消选定标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    return;
                }
                else
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                }
            }
            else
            {
                strErrorCode = "未知错误";
            }

            string strLog = strCmd + "失败，失败原因： " + strErrorCode;
        }
        private void ProcessGetAccessEpcMatch(MessageTran msgTran)
        {
            string strCmd = "取得选定标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x01)
                {
                    return;
                }
                else
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                }
            }
            else
            {
                if (msgTran.AryData[0] == 0x00)
                {
                    curOperateTagBuffer.strAccessEpcMatch = CCommondMethod.ByteArrayToString(msgTran.AryData, 2, Convert.ToInt32(msgTran.AryData[1]));

                    //RefreshOpTag(0x86);
                    return;
                }
                else
                {
                    strErrorCode = "未知错误";
                }
            }
        }
        private void ProcessFastSwitch(MessageTran msgTran)
        {
            string strCmd = "快速4天线盘存";
            string strErrorCode = string.Empty;

            //if (msgTran.AryData.Length == 1)
            //{
            //    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
            //    string strLog = strCmd + "失败，失败原因： " + strErrorCode;

            //    WriteLog(lrtxtLog, strLog, 1);
            //    RefreshFastSwitch(0x01);
            //    RunLoopFastSwitch();
            //}
            //else if (msgTran.AryData.Length == 2)
            //{
            //    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[1]);
            //    string strLog = strCmd + "失败，失败原因： " + strErrorCode + "--" + "天线" + (msgTran.AryData[0] + 1);

            //    WriteLog(lrtxtLog, strLog, 1);
            //}

            //else if (msgTran.AryData.Length == 7)
            //{
            //    m_nSwitchTotal = Convert.ToInt32(msgTran.AryData[0]) * 255 * 255 + Convert.ToInt32(msgTran.AryData[1]) * 255 + Convert.ToInt32(msgTran.AryData[2]);
            //    m_nSwitchTime = Convert.ToInt32(msgTran.AryData[3]) * 255 * 255 * 255 + Convert.ToInt32(msgTran.AryData[4]) * 255 * 255 + Convert.ToInt32(msgTran.AryData[5]) * 255 + Convert.ToInt32(msgTran.AryData[6]);

            //    m_curInventoryBuffer.nDataCount = m_nSwitchTotal;
            //    m_curInventoryBuffer.nCommandDuration = m_nSwitchTime;
            //    WriteLog(lrtxtLog, strCmd, 0);
            //    RefreshFastSwitch(0x02);
            //    RunLoopFastSwitch();
            //}
            //else
            //{
            //    m_nTotal++;
            //    int nLength = msgTran.AryData.Length;
            //    int nEpcLength = nLength - 4;

            //    //增加盘存明细表
            //    string strEPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, nEpcLength);
            //    string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 2);
            //    string strRSSI = msgTran.AryData[nLength - 1].ToString();
            //    SetMaxMinRSSI(Convert.ToInt32(msgTran.AryData[nLength - 1]));
            //    byte btTemp = msgTran.AryData[0];
            //    byte btAntId = (byte)((btTemp & 0x03) + 1);
            //    m_curInventoryBuffer.nCurrentAnt = (int)btAntId;
            //    string strAntId = btAntId.ToString();
            //    byte btFreq = (byte)(btTemp >> 2);

            //    string strFreq = GetFreqString(btFreq);

            //    DataRow[] drs = m_curInventoryBuffer.dtTagTable.Select(string.Format("COLEPC = '{0}'", strEPC));
            //    if (drs.Length == 0)
            //    {
            //        DataRow row1 = m_curInventoryBuffer.dtTagTable.NewRow();
            //        row1[0] = strPC;
            //        row1[2] = strEPC;
            //        row1[4] = strRSSI;
            //        row1[5] = "1";
            //        row1[6] = strFreq;
            //        row1[7] = "0";
            //        row1[8] = "0";
            //        row1[9] = "0";
            //        row1[10] = "0";
            //        switch (btAntId)
            //        {
            //            case 0x01:
            //                {
            //                    row1[7] = "1";
            //                }
            //                break;
            //            case 0x02:
            //                {
            //                    row1[8] = "1";
            //                }
            //                break;
            //            case 0x03:
            //                {
            //                    row1[9] = "1";
            //                }
            //                break;
            //            case 0x04:
            //                {
            //                    row1[10] = "1";
            //                }
            //                break;
            //            default:
            //                break;
            //        }

            //        m_curInventoryBuffer.dtTagTable.Rows.Add(row1);
            //        m_curInventoryBuffer.dtTagTable.AcceptChanges();
            //    }
            //    else
            //    {
            //        foreach (DataRow dr in drs)
            //        {
            //            dr.BeginEdit();
            //            int nTemp = 0;

            //            dr[4] = strRSSI;
            //            //dr[5] = (Convert.ToInt32(dr[5]) + 1).ToString();
            //            nTemp = Convert.ToInt32(dr[5]);
            //            dr[5] = (nTemp + 1).ToString();
            //            dr[6] = strFreq;

            //            switch (btAntId)
            //            {
            //                case 0x01:
            //                    {
            //                        //dr[7] = (Convert.ToInt32(dr[7]) + 1).ToString();
            //                        nTemp = Convert.ToInt32(dr[7]);
            //                        dr[7] = (nTemp + 1).ToString();
            //                    }
            //                    break;
            //                case 0x02:
            //                    {
            //                        //dr[8] = (Convert.ToInt32(dr[8]) + 1).ToString();
            //                        nTemp = Convert.ToInt32(dr[8]);
            //                        dr[8] = (nTemp + 1).ToString();
            //                    }
            //                    break;
            //                case 0x03:
            //                    {
            //                        //dr[9] = (Convert.ToInt32(dr[9]) + 1).ToString();
            //                        nTemp = Convert.ToInt32(dr[9]);
            //                        dr[9] = (nTemp + 1).ToString();
            //                    }
            //                    break;
            //                case 0x04:
            //                    {
            //                        //dr[10] = (Convert.ToInt32(dr[10]) + 1).ToString();
            //                        nTemp = Convert.ToInt32(dr[10]);
            //                        dr[10] = (nTemp + 1).ToString();
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }

            //            dr.EndEdit();
            //        }
            //        m_curInventoryBuffer.dtTagTable.AcceptChanges();
            //    }

            //    m_curInventoryBuffer.dtEndInventory = DateTime.Now;
            //    RefreshFastSwitch(0x00);
        }
        private void ProcessSetMonzaStatus(MessageTran msgTran)
        {
            string strCmd = "设置Impinj Monza快速读TID功能";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btAntDetector = msgTran.AryData[0];
                    return;
                }
                else
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                }
            }
            else
            {
                strErrorCode = "未知错误";
            }

            string strLog = strCmd + "失败，失败原因： " + strErrorCode;
        }
        private void ProcessGetMonzaStatus(MessageTran msgTran)
        {
            string strCmd = "读取Impinj Monza快速读TID功能";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x00 || msgTran.AryData[0] == 0x8D)
                {
                    curSetting.btReadId = msgTran.ReadId;
                    curSetting.btAntDetector = msgTran.AryData[0];
                    return;
                }
                else
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                }
            }
            else
            {
                strErrorCode = "未知错误";
            }
        }
        private void ProcessGetInventoryBuffer(MessageTran msgTran)
        {
            string strCmd = "读取缓存";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                int nDataLen = msgTran.AryData.Length;
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - 4;

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEpc = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strRSSI = msgTran.AryData[nDataLen - 3].ToString();
                SetMaxMinRSSI(Convert.ToInt32(msgTran.AryData[nDataLen - 3]));
                byte btTemp = msgTran.AryData[nDataLen - 2];
                byte btAntId = (byte)((btTemp & 0x03) + 1);
                string strAntId = btAntId.ToString();
                string strReadCnr = msgTran.AryData[nDataLen - 1].ToString();

                DataRow row = curInventoryBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEpc;
                row[3] = strAntId;
                row[4] = strRSSI;
                row[5] = strReadCnr;

                curInventoryBuffer.dtTagTable.Rows.Add(row);
                curInventoryBuffer.dtTagTable.AcceptChanges();

                //RefreshInventory(0x90);
                //WriteLog(lrtxtLog, strCmd, 0);
            }
        }
        private void ProcessGetAndResetInventoryBuffer(MessageTran msgTran)
        {
            string strCmd = "读取清空缓存";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                return;
            }
            else
            {
                int nDataLen = msgTran.AryData.Length;
                int nEpcLen = Convert.ToInt32(msgTran.AryData[2]) - 4;

                string strPC = CCommondMethod.ByteArrayToString(msgTran.AryData, 3, 2);
                string strEpc = CCommondMethod.ByteArrayToString(msgTran.AryData, 5, nEpcLen);
                string strCRC = CCommondMethod.ByteArrayToString(msgTran.AryData, 5 + nEpcLen, 2);
                string strRSSI = msgTran.AryData[nDataLen - 3].ToString();
                SetMaxMinRSSI(Convert.ToInt32(msgTran.AryData[nDataLen - 3]));
                byte btTemp = msgTran.AryData[nDataLen - 2];
                byte btAntId = (byte)((btTemp & 0x03) + 1);
                string strAntId = btAntId.ToString();
                string strReadCnr = msgTran.AryData[nDataLen - 1].ToString();

                DataRow row = curInventoryBuffer.dtTagTable.NewRow();
                row[0] = strPC;
                row[1] = strCRC;
                row[2] = strEpc;
                row[3] = strAntId;
                row[4] = strRSSI;
                row[5] = strReadCnr;

                curInventoryBuffer.dtTagTable.Rows.Add(row);
                curInventoryBuffer.dtTagTable.AcceptChanges();

                //RefreshInventory(0x91);
                //WriteLog(lrtxtLog, strCmd, 0);
            }
        }
        private void ProcessGetInventoryBufferTagCount(MessageTran msgTran)
        {
            string strCmd = "缓存标签数量";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 2)
            {
                curInventoryBuffer.nTagCount = Convert.ToInt32(msgTran.AryData[0]) * 256 + Convert.ToInt32(msgTran.AryData[1]);
                return;
            }
            else if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
            }
            else
            {
                strErrorCode = "未知错误";
            }

            string strLog = strCmd + "失败，失败原因： " + strErrorCode;
        }
        private void ProcessResetInventoryBuffer(MessageTran msgTran)
        {
            string strCmd = "清空缓存";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] == 0x10)
                {
                    return;
                }
                else
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                }
            }
            else
            {
                strErrorCode = "未知错误";
            }
        }
        private void ProcessInventoryISO18000(MessageTran msgTran)
        {
            string strCmd = "盘存标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                if (msgTran.AryData[0] != 0xFF)
                {
                    strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                    string strLog = strCmd + "失败，失败原因： " + strErrorCode;
                }
            }
            else if (msgTran.AryData.Length == 9)
            {
                string strAntID = CCommondMethod.ByteArrayToString(msgTran.AryData, 0, 1);
                string strUID = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 8);

                //    //增加保存标签列表，原未盘存则增加记录，否则将标签盘存数量加1
                //    DataRow[] drs = curOperateTagISO18000Buffer.dtTagTable.Select(string.Format("UID = '{0}'", strUID));
                //    if (drs.Length == 0)
                //    {
                //        DataRow row = m_curOperateTagISO18000Buffer.dtTagTable.NewRow();
                //        row[0] = strAntID;
                //        row[1] = strUID;
                //        row[2] = "1";
                //        m_curOperateTagISO18000Buffer.dtTagTable.Rows.Add(row);
                //        m_curOperateTagISO18000Buffer.dtTagTable.AcceptChanges();
                //    }
                //    else
                //    {
                //        DataRow row = drs[0];
                //        row.BeginEdit();
                //        row[2] = (Convert.ToInt32(row[2]) + 1).ToString();
                //        m_curOperateTagISO18000Buffer.dtTagTable.AcceptChanges();
                //    }

                //}
                //else if (msgTran.AryData.Length == 2)
                //{
                //    m_curOperateTagISO18000Buffer.nTagCnt = Convert.ToInt32(msgTran.AryData[1]);
                //    RefreshISO18000(msgTran.Cmd);

                //    //WriteLog(lrtxtLog, strCmd, 0);
                //}
                //else
                //{
                //    strErrorCode = "未知错误";
                //    string strLog = strCmd + "失败，失败原因： " + strErrorCode;

                //    WriteLog(lrtxtLog, strLog, 1);
                //}
            }
        }
        private void ProcessReadTagISO18000(MessageTran msgTran)
        {
            string strCmd = "读取标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                string strAntID = CCommondMethod.ByteArrayToString(msgTran.AryData, 0, 1);
                string strData = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, msgTran.AryData.Length - 1);

                //m_curOperateTagISO18000Buffer.btAntId = Convert.ToByte(strAntID);
                //m_curOperateTagISO18000Buffer.strReadData = strData;

                //RefreshISO18000(msgTran.Cmd);

                //WriteLog(lrtxtLog, strCmd, 0);
            }
        }
        private void ProcessWriteTagISO18000(MessageTran msgTran)
        {
            string strCmd = "写入标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                //string strAntID = CCommondMethod.ByteArrayToString(msgTran.AryData, 0, 1);
                //string strCnt = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 1);

                //m_curOperateTagISO18000Buffer.btAntId = msgTran.AryData[0];
                //m_curOperateTagISO18000Buffer.btWriteLength = msgTran.AryData[1];

                ////RefreshISO18000(msgTran.Cmd);
                //string strLength = msgTran.AryData[1].ToString();
                //string strLog = strCmd + ": " + "成功写入" + strLength + "字节";
                //WriteLog(lrtxtLog, strLog, 0);
                //RunLoopISO18000(Convert.ToInt32(msgTran.AryData[1]));
            }
        }
        private void ProcessLockTagISO18000(MessageTran msgTran)
        {
            string strCmd = "永久写保护";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                //string strAntID = CCommondMethod.ByteArrayToString(msgTran.AryData, 0, 1);
                //string strStatus = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 1);

                //m_curOperateTagISO18000Buffer.btAntId = msgTran.AryData[0];
                //m_curOperateTagISO18000Buffer.btStatus = msgTran.AryData[1];

                ////RefreshISO18000(msgTran.Cmd);
                //string strLog = string.Empty;
                //switch (msgTran.AryData[1])
                //{
                //    case 0x00:
                //        strLog = strCmd + ": " + "成功锁定";
                //        break;
                //    case 0xFE:
                //        strLog = strCmd + ": " + "已是锁定状态";
                //        break;
                //    case 0xFF:
                //        strLog = strCmd + ": " + "无法锁定";
                //        break;
                //    default:
                //        break;
                //}

                //WriteLog(lrtxtLog, strLog, 0);

            }
        }
        private void ProcessQueryISO18000(MessageTran msgTran)
        {
            string strCmd = "查询标签";
            string strErrorCode = string.Empty;

            if (msgTran.AryData.Length == 1)
            {
                strErrorCode = CCommondMethod.FormatErrorCode(msgTran.AryData[0]);
                string strLog = strCmd + "失败，失败原因： " + strErrorCode;
            }
            else
            {
                //string strAntID = CCommondMethod.ByteArrayToString(msgTran.AryData, 0, 1);
                //string strStatus = CCommondMethod.ByteArrayToString(msgTran.AryData, 1, 1);

                //m_curOperateTagISO18000Buffer.btAntId = msgTran.AryData[0];
                //m_curOperateTagISO18000Buffer.btStatus = msgTran.AryData[1];

                //RefreshISO18000(msgTran.Cmd);

                //WriteLog(lrtxtLog, strCmd, 0);
            }
        }
        #endregion
    }
}
