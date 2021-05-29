using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JT100.Wish.Component
{
    public class RfidBindList : BaseControl
    {
        public ObservableCollection<RfidBindVM> DataSource
        {
            get { return (ObservableCollection<RfidBindVM>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public readonly static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<RfidBindVM>), typeof(RfidBindList));
        static RfidBindList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidBindList), new FrameworkPropertyMetadata(typeof(RfidBindList)));
        }
        public RfidBindList()
        {
            DataSource = new ObservableCollection<RfidBindVM>();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitOrderSource();
        }
        private async void InitOrderSource()
        {
            var wareInfos = await Task.Run(() => UserContext.ApiHelper.GetAllWareInfo());

        }
    }
    public class RfidBindVM : ViewModelBase
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
        /// 类型
        /// </summary>
        public string WareType
        {
            get { return GetValue(() => WareType); }
            set { SetValue(() => WareType, value); }
        }
        /// <summary>
        /// 总数量
        /// </summary>
        public string TotalCount
        {
            get { return GetValue(() => TotalCount); }
            set { SetValue(() => TotalCount, value); }
        }

        /// <summary>
        /// 已绑定数量
        /// </summary>
        public string BindedCount
        {
            get { return GetValue(() => BindedCount); }
            set { SetValue(() => BindedCount, value); }
        }

        /// <summary>
        /// 未绑定数量
        /// </summary>
        public string UnBindCount
        {
            get { return GetValue(() => UnBindCount); }
            set { SetValue(() => UnBindCount, value); }
        }
    }
}
