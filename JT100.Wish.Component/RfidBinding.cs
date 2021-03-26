using JT100.Wish.Core;
using JT100.Wish.Tool;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace JT100.Wish.Component
{
    public class RfidBinding : Control
    {
        public ICommand SendCommand { get; set; }
        RfidReadProvider rfidHelper;
        public ObservableCollection<BindingData> DataSource
        {
            get
            {
                return (ObservableCollection<BindingData>)GetValue(DataSourceroperty);
            }
            set { SetValue(DataSourceroperty, value); }
        }
        public readonly static DependencyProperty DataSourceroperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<BindingData>), typeof(RfidBinding));
        static RfidBinding()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidBinding), new FrameworkPropertyMetadata(typeof(RfidBinding)));
        }
        public RfidBinding()
        {
            DataSource = new ObservableCollection<BindingData>();
            rfidHelper = new RfidReadProvider();
            rfidHelper.OnDataReceived += RfidHelper_OnDataReceived;
            rfidHelper.ConnectByCom("COM3");
            rfidHelper.AddAntena(1);
            SendCommand = new RelayCommand(() =>
            {
                SendData();
            });
        }

        private void RfidHelper_OnDataReceived(object o, string epc)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                foreach (var item in DataSource)
                {
                    if (item.Rfid.Equals(epc))
                    {
                        item.Count++;
                        item.BindTime = DateTime.Now.ToString("HH:mm:ss");
                        return;
                    }
                }
                BindingData bindingData = new BindingData();
                bindingData.Index = DataSource.Count + 1;
                bindingData.Rfid = epc;
                bindingData.Count = 1; ;
                bindingData.BindTime = DateTime.Now.ToString("HH:mm:ss");
                DataSource.Add(bindingData);
            }),
DispatcherPriority.Render);

        }

        private void SendData()
        {
            //WeiXinHelper.GetToken();
            rfidHelper.SendReadCommand();
        }
    }
    public class BindingData : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }
        public string Rfid
        {
            get { return GetValue(() => Rfid); }
            set { SetValue(() => Rfid, value); }
        }
        public int Count
        {
            get { return GetValue(() => Count); }
            set { SetValue(() => Count, value); }
        }
        public string BindTime
        {
            get { return GetValue(() => BindTime); }
            set { SetValue(() => BindTime, value); }
        }
    }
}
