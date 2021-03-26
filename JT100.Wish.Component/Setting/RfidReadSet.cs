using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace JT100.Wish.Component
{
    public class RfidReadSet : BaseControl
    {
        /// <summary>
        /// 保存
        /// </summary>
        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// 读写器配置
        /// </summary>
        public RfidReadConfig ReadConfig
        {
            get
            {
                return (RfidReadConfig)GetValue(ReadConfigProperty);
            }
            set
            {
                SetValue(ReadConfigProperty, value);
            }
        }

        public static readonly DependencyProperty ReadConfigProperty = DependencyProperty.Register("ReadConfig", typeof(RfidReadConfig), typeof(RfidReadSet), new PropertyMetadata(null));

        public ObservableCollection<string> ComSource
        {
            get
            {
                return (ObservableCollection<string>)GetValue(ComSourceProperty);
            }
            set { SetValue(ComSourceProperty, value); }
        }
        public readonly static DependencyProperty ComSourceProperty = DependencyProperty.Register("ComSource", typeof(ObservableCollection<string>), typeof(RfidReadSet));

        public ObservableCollection<int> BaudrateSource
        {
            get
            {
                return (ObservableCollection<int>)GetValue(BaudrateSourceProperty);
            }
            set { SetValue(BaudrateSourceProperty, value); }
        }
        public readonly static DependencyProperty BaudrateSourceProperty = DependencyProperty.Register("BaudrateSource", typeof(ObservableCollection<int>), typeof(RfidReadSet));

        static RfidReadSet()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidReadSet), new FrameworkPropertyMetadata(typeof(RfidReadSet)));
        }
        public RfidReadSet()
        {
            ReadConfig = UserContext.UserXmlProvider.GetConfig<RfidReadConfig>("RfidReadConfig") ?? new RfidReadConfig() { Baudrate = 115200 };
            ComSource = new ObservableCollection<string>();
            foreach (var name in System.IO.Ports.SerialPort.GetPortNames())
            {
                ComSource.Add(name);
            }
            BaudrateSource = new ObservableCollection<int>();
            BaudrateSource.Add(38400);
            BaudrateSource.Add(115200);
            SaveCommand = new RelayCommand(() =>
            {
                SaveConfig();
            });
        }
        private void SaveConfig()
        {
            try
            {
                UserContext.UserXmlProvider.SetConfig<RfidReadConfig>("RfidReadConfig", ReadConfig);
                if (ReadConfig != null && !string.IsNullOrEmpty(ReadConfig.ComPort))
                {
                    List<int> antennas = new List<int>();
                    if (ReadConfig.AntennaOne)
                    {
                        antennas.Add(1);
                    }
                    if (ReadConfig.AntennaTwo)
                    {
                        antennas.Add(2);
                    }
                    if (ReadConfig.AntennaThree)
                    {
                        antennas.Add(3);
                    }
                    if (ReadConfig.AntennaFour)
                    {
                        antennas.Add(4);
                    }
                   UserContext.RfidReadProvider.InitializeCom(ReadConfig.ComPort, ReadConfig.Baudrate, antennas);
                }
            }
            catch
            {

            }
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
    public class RfidReadConfig : ViewModelBase
    {
        /// <summary>
        /// 串口号
        /// </summary>
        [XmlAttribute("ComPort")]
        public string ComPort
        {
            get { return GetValue(() => ComPort); }
            set { SetValue(() => ComPort, value); }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        [XmlAttribute("Baudrate")]
        public int Baudrate
        {
            get { return GetValue(() => Baudrate); }
            set { SetValue(() => Baudrate, value); }
        }

        /// <summary>
        /// 天线1
        /// </summary>
        [XmlAttribute("AntennaOne")]
        public bool AntennaOne
        {
            get { return GetValue(() => AntennaOne); }
            set { SetValue(() => AntennaOne, value); }
        }

        /// <summary>
        /// 天线2
        /// </summary>
        [XmlAttribute("AntennaTwo")]
        public bool AntennaTwo
        {
            get { return GetValue(() => AntennaTwo); }
            set { SetValue(() => AntennaTwo, value); }
        }

        /// <summary>
        /// 天线3
        /// </summary>
        [XmlAttribute("AntennaThree")]
        public bool AntennaThree
        {
            get { return GetValue(() => AntennaThree); }
            set { SetValue(() => AntennaThree, value); }
        }

        /// <summary>
        /// 天线4
        /// </summary>
        [XmlAttribute("AntennaFour")]
        public bool AntennaFour
        {
            get { return GetValue(() => AntennaFour); }
            set { SetValue(() => AntennaFour, value); }
        }
    }
}
