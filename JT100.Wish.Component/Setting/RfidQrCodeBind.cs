﻿using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JT100.Wish.Component
{
    /// <summary>
    /// RFID和二维码绑定
    /// </summary>
    public class RfidQrCodeBind : BaseControl
    {
        private TextBlock _txtRfid;
        private TextBox _txtQrCode;
        private string lastRfid;
        public ObservableCollection<QrCodeBindVM> DataSource
        {
            get { return (ObservableCollection<QrCodeBindVM>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public readonly static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<QrCodeBindVM>), typeof(RfidQrCodeBind));
        static RfidQrCodeBind()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidQrCodeBind), new FrameworkPropertyMetadata(typeof(RfidQrCodeBind)));
        }
        public RfidQrCodeBind()
        {
            DataSource = new ObservableCollection<QrCodeBindVM>();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _txtRfid = VisualTreeProvider.FindFirstChild<TextBlock>(this, "Txt_Rfid");
            _txtQrCode = VisualTreeProvider.FindFirstChild<TextBox>(this, "Txt_QrCode");
            _txtQrCode.KeyUp += TxtQrCode_KeyUp;
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
        }

        private async void TxtQrCode_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var qrCode = _txtQrCode.Text;
                if (!string.IsNullOrEmpty(qrCode) && !string.IsNullOrEmpty(lastRfid))
                {
                    var result = await Task.Run(() => UserContext.ApiHelper.BindWareSNCode(lastRfid, qrCode));
                    if (result.Success)
                    {
                        var vm = new QrCodeBindVM();
                        vm.Index = (DataSource.Count + 1).ToString();
                        vm.SN = lastRfid;
                        vm.QrCode = qrCode;
                        lastRfid = null;
                        _txtQrCode.Text = string.Empty;
                    }
                    else
                    {
                        _txtQrCode.SelectAll();
                    }
                }
            }
        }

        private void RfidReadProvider_OnDataReceived(object o, string epc)
        {
            if (!string.IsNullOrEmpty(lastRfid) && lastRfid == epc)
            {
                return;
            }
            this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate ()
            {
                lastRfid = epc;
                _txtRfid.Text = epc;
                Thread.Sleep(10);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        public override void Dispose()
        {
            base.Dispose();
            lastRfid = string.Empty;
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            _txtQrCode.KeyUp -= TxtQrCode_KeyUp;
        }
    }
    public class QrCodeBindVM : ViewModelBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        public string Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }

        /// <summary>
        /// SN号
        /// </summary>
        public string SN
        {
            get { return GetValue(() => SN); }
            set { SetValue(() => SN, value); }
        }
        /// <summary>
        /// 二维码
        /// </summary>
        public string QrCode
        {
            get { return GetValue(() => QrCode); }
            set { SetValue(() => QrCode, value); }
        }
    }
}
