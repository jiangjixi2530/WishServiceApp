using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
        static RfidQrCodeBind()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidQrCodeBind), new FrameworkPropertyMetadata(typeof(RfidQrCodeBind)));
        }
        public RfidQrCodeBind()
        {
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _txtRfid = VisualTreeProvider.FindFirstChild<TextBlock>(this, "Txt_Rfid");
            _txtQrCode = VisualTreeProvider.FindFirstChild<TextBox>(this, "Txt_QrCode");
            _txtQrCode.KeyUp += TxtQrCode_KeyUp;
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
        }

        private void TxtQrCode_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {

            }
        }

        private void RfidReadProvider_OnDataReceived(object o, string epc)
        {
            if (!string.IsNullOrEmpty(lastRfid))
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
}
