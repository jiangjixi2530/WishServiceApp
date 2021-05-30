using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace JT100.Wish.Component
{
    public class RfidBindList : BaseControl
    {

        private List<WareInfo> wareInfos;

        private List<WareType> wareTypes;

        public ObservableCollection<RfidBindVM> DataSource
        {
            get { return (ObservableCollection<RfidBindVM>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public readonly static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(ObservableCollection<RfidBindVM>), typeof(RfidBindList));

        public ObservableCollection<string> HoursSource
        {
            get { return (ObservableCollection<string>)GetValue(HoursSourceProperty); }
            set { SetValue(HoursSourceProperty, value); }
        }

        public readonly static DependencyProperty HoursSourceProperty = DependencyProperty.Register("HoursSource", typeof(ObservableCollection<string>), typeof(RfidBindList));
        public ObservableCollection<string> MinutesSource
        {
            get { return (ObservableCollection<string>)GetValue(MinutesSourceProperty); }
            set { SetValue(MinutesSourceProperty, value); }
        }

        public readonly static DependencyProperty MinutesSourceProperty = DependencyProperty.Register("MinutesSource", typeof(ObservableCollection<string>), typeof(RfidBindList));
        public ObservableCollection<string> WareTypeSource
        {
            get { return (ObservableCollection<string>)GetValue(WareTypeSourceProperty); }
            set { SetValue(WareTypeSourceProperty, value); }
        }

        public readonly static DependencyProperty WareTypeSourceProperty = DependencyProperty.Register("WareTypeSource", typeof(ObservableCollection<string>), typeof(RfidBindList));

        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }
        public readonly static DependencyProperty StartTimeProperty = DependencyProperty.Register("StartTime", typeof(DateTime), typeof(RfidBindList));

        public string StartHour
        {
            get { return (string)GetValue(StartHourProperty); }
            set { SetValue(StartHourProperty, value); }
        }
        public readonly static DependencyProperty StartHourProperty = DependencyProperty.Register("StartHour", typeof(string), typeof(RfidBindList));

        public string StartMinute
        {
            get { return (string)GetValue(StartMinuteProperty); }
            set { SetValue(StartMinuteProperty, value); }
        }
        public readonly static DependencyProperty StartMinuteProperty = DependencyProperty.Register("StartMinute", typeof(string), typeof(RfidBindList));

        public DateTime EndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
        public readonly static DependencyProperty EndTimeProperty = DependencyProperty.Register("EndTime", typeof(DateTime), typeof(RfidBindList));

        public string EndHour
        {
            get { return (string)GetValue(EndHourProperty); }
            set { SetValue(EndHourProperty, value); }
        }
        public readonly static DependencyProperty EndHourProperty = DependencyProperty.Register("EndHour", typeof(string), typeof(RfidBindList));

        public string EndMinute
        {
            get { return (string)GetValue(EndMinuteProperty); }
            set { SetValue(EndMinuteProperty, value); }
        }
        public readonly static DependencyProperty EndMinuteProperty = DependencyProperty.Register("EndMinute", typeof(string), typeof(RfidBindList));

        public string WareType
        {
            get { return (string)GetValue(WareTypeProperty); }
            set { SetValue(WareTypeProperty, value); }
        }
        public readonly static DependencyProperty WareTypeProperty = DependencyProperty.Register("WareType", typeof(string), typeof(RfidBindList));
        public string TotalCount
        {
            get { return (string)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }
        public readonly static DependencyProperty TotalCountProperty = DependencyProperty.Register("TotalCount", typeof(string), typeof(RfidBindList));

        static RfidBindList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidBindList), new FrameworkPropertyMetadata(typeof(RfidBindList)));
        }
        public RfidBindList()
        {
            WareTypeSource = new ObservableCollection<string>();
            DataSource = new ObservableCollection<RfidBindVM>();
            HoursSource = new ObservableCollection<string>();
            for (int i = 0; i < 24; i++)
            {
                HoursSource.Add(i.ToString().PadLeft(2, '0'));
            }
            MinutesSource = new ObservableCollection<string>();
            for (int i = 0; i < 60; i++)
            {
                MinutesSource.Add(i.ToString().PadLeft(2, '0'));
            }
            StartTime = DateTime.Now.Date;
            EndTime = DateTime.Now.Date;
            StartHour = "00";
            EndHour = "23";
            StartMinute = "00";
            EndMinute = "59";
            WareType = "全部";
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitType();
            Button button = VisualTreeProvider.FindFirstChild<Button>(this, "Btn_Query");
            button.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void InitType()
        {
            wareTypes = new List<WareType>();
            wareTypes.Add(new WareType { Id = 1, TypeName = "枕套" });
            wareTypes.Add(new WareType { Id = 2, TypeName = "毛巾" });
            wareTypes.Add(new WareType { Id = 3, TypeName = "浴巾" });
            wareTypes.Add(new WareType { Id = 4, TypeName = "地巾" });
            //wareTypes.Add(new WareType { Id = 5, TypeName = "毛巾" });
            wareTypes.Add(new WareType { Id = 6, TypeName = "床单1.2M" });
            wareTypes.Add(new WareType { Id = 7, TypeName = "床单1.5M" });
            wareTypes.Add(new WareType { Id = 8, TypeName = "床单1.8M" });
            wareTypes.Add(new WareType { Id = 9, TypeName = "被套1.2M" });
            wareTypes.Add(new WareType { Id = 10, TypeName = "被套1.5M" });
            wareTypes.Add(new WareType { Id = 11, TypeName = "被套1.8M" });
            WareTypeSource.Add("全部");
            wareTypes.ForEach(_ => WareTypeSource.Add(_.TypeName));
        }

        private async void Query()
        {
            DateTime start = StartTime;
            start = start.AddHours(Convert.ToInt32(StartHour));
            start = start.AddMinutes(Convert.ToInt32(StartMinute));
            DateTime end = EndTime;
            end = end.AddHours(Convert.ToInt32(EndHour));
            end = end.AddMinutes(Convert.ToInt32(EndMinute));
            if (start >= end)
            {
                MessageBox.Show("开始查询时间不能大于或等于结束查询时间");
                return;
            }
            wareInfos = await Task.Run(() => UserContext.ApiHelper.GetWareInfoByBindTime(start, end));
            if (wareInfos == null)
            {
                return;
            }
            if (WareType != "全部")
            {
                var checktype = wareTypes.FirstOrDefault(_ => _.TypeName == WareType).Id;
                wareInfos = wareInfos.Where(_ => _.WareType == checktype).ToList();
            }
            DataSource.Clear();
            for (int i = 0; i < wareInfos.Count();)
            {
                RfidBindVM vm = new RfidBindVM();
                vm.TypeValue = wareInfos[i].WareType;
                vm.WareType = GetWareTypeName(vm.TypeValue);
                vm.RFID = wareInfos[i].SNCode;
                vm.WareCode = wareInfos[i].WareCode;
                vm.BindTime = wareInfos[i].BindTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                vm.Index = ++i;
                DataSource.Add(vm);
            }
            TotalCount = "总数量：" + DataSource.Count;
        }

        public string GetWareTypeName(int type)
        {
            return wareTypes?.FirstOrDefault(_ => _.Id == type)?.TypeName ?? string.Empty;
        }
    }
    public class RfidBindVM : ViewModelBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }

        public int TypeValue
        {
            get; set;
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
        /// 芯片
        /// </summary>
        public string RFID
        {
            get { return GetValue(() => RFID); }
            set { SetValue(() => RFID, value); }
        }

        /// <summary>
        /// 二维码序号
        /// </summary>
        public string WareCode
        {
            get { return GetValue(() => WareCode); }
            set { SetValue(() => WareCode, value); }
        }

        /// <summary>
        /// 未绑定数量
        /// </summary>
        public string BindTime
        {
            get { return GetValue(() => BindTime); }
            set { SetValue(() => BindTime, value); }
        }
    }
}
