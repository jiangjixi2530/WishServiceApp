using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JT100.Wish.Component
{
    /// <summary>
    /// 绑定RFID和二维码地址
    /// </summary>
    public class BindRfidWidthQR : Control, IDisposable
    {
        #region 依赖项属性

        /// <summary>
        /// 绑定数据源
        /// </summary>
        public ObservableCollection<BindRfidQrVM> DataSource
        {
            get { return (ObservableCollection<BindRfidQrVM>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        /// <summary>
        /// 绑定数据源
        /// </summary>
        public static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<BindRfidQrVM>), typeof(BindRfidWidthQR), new PropertyMetadata(null));
        #endregion
        static BindRfidWidthQR()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BindRfidWidthQR), new FrameworkPropertyMetadata(typeof(BindRfidWidthQR)));
        }
        public BindRfidWidthQR()
        {

        }
        public async void LoadData()
        {
            //var result = await Task.Run(() => WishContext.Api.HttpPost<BindRfidQrVM>("", new Dictionary<string, object>()));
        }

        public void Dispose()
        {

        }
    }

    public class BindRfidQrVM : ViewModelBase
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName
        {
            get { return GetValue(() => ProductName); }
            set { SetValue(() => ProductName, value); }
        }

        /// <summary>
        /// RFID标签
        /// </summary>
        public string RfidCode
        {
            get { return GetValue(() => RfidCode); }
            set { SetValue(() => RfidCode, value); }
        }

        /// <summary>
        /// 二维码地址
        /// </summary>
        public string QrCode
        {
            get { return GetValue(() => QrCode); }
            set { SetValue(() => QrCode, value); }
        }

    }
}
