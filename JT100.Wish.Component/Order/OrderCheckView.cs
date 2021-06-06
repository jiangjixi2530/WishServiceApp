using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace JT100.Wish.Component
{
    public class OrderCheckView : BaseControl
    {
        private static readonly object lockObj = new object();
        Queue<string> UnDealRfid = new Queue<string>();
        private AutoResetEvent _mainEvent = new AutoResetEvent(false);
        private List<string> OrderEPC = new List<string>();
        private List<string> CheckedEPC = new List<string>();
        /// <summary>
        /// 异常的SN号
        /// </summary>
        private HashSet<string> exceptionEPC = new HashSet<string>();
        public ObservableCollection<OrderVM> OrderSource
        {
            get
            {
                return (ObservableCollection<OrderVM>)GetValue(OrderSourceProperty);
            }
            set { SetValue(OrderSourceProperty, value); }
        }
        public readonly static DependencyProperty OrderSourceProperty = DependencyProperty.Register("OrderSource", typeof(ObservableCollection<OrderVM>), typeof(OrderCheckView));

        public ICommand RefreshCommand { get; set; }
        public ICommand SubmitCommand { get; set; }

        public ICommand CheckCommand { get; set; }

        static OrderCheckView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OrderCheckView), new FrameworkPropertyMetadata(typeof(OrderCheckView)));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderCheckView()
        {
            OrderSource = new ObservableCollection<OrderVM>();
            RefreshCommand = new RelayCommand(() =>
              {
                  InitOrderSource();
              });
            SubmitCommand = new RelayCommand(() =>
              {
                  CheckOrder();
              });
            CheckCommand = new RelayCommand<OrderVM>((order) =>
              {
                  CheckOrder(order);
              });
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitOrderSource();
            Task.Run(() => DealRfid());
        }

        private async void InitOrderSource()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            lock (lockObj)
            {
                OrderEPC.Clear();
                CheckedEPC.Clear();
                OrderSource.Clear();
                exceptionEPC.Clear();
            }
            var orders = await Task.Run(() => UserContext.ApiHelper.GetCustomInOrders());
            int index = 1;
            foreach (var order in orders)
            {
                var orderVm = InOrderToOrderVM(order, index++);
                OrderEPC = OrderEPC.Concat(orderVm.OrderDetailRfids).ToList();
                OrderSource.Add(orderVm);
            }
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
        }


        private OrderVM InOrderToOrderVM(InOrder order, int index)
        {
            var orderVm = new OrderVM();
            orderVm.Index = index++;
            orderVm.OrderNum = order.OrderNo;
            orderVm.OrderDate = order.CreateTime;
            orderVm.CustomId = order.CustomerId;
            orderVm.CustomName = order.CustomerName;
            orderVm.StoreName = order.StoreName;
            orderVm.InType = order.InType == 1 ? "日常入库" : "反洗入库";
            orderVm.OrderDetailRfids = new List<string>();
            foreach (var detail in order.Details)
            {
                orderVm.OrderDetailRfids.AddRange(detail.WareSNCodes.Split(','));
            }
            orderVm.RfidCount = orderVm.OrderDetailRfids.Count;
            orderVm.CheckInfo = "待清点";
            return orderVm;
        }

        /// <summary>
        /// 信息接收
        /// </summary>
        /// <param name="o"></param>
        /// <param name="epc"></param>
        private void RfidReadProvider_OnDataReceived(object o, string epc)
        {
            var exists = false;
            lock (lockObj)
            {
                exists = CheckedEPC.Exists(_ => _.Equals(epc)) || exceptionEPC.Contains(epc);
            }
            if (!exists)
            {
                UnDealRfid.Enqueue(epc);
                _mainEvent.Set();
            }
        }

        private async void DealRfid()
        {
            while (true)
            {
                _mainEvent.WaitOne();
                while (UnDealRfid.Count > 0)
                {
                    var rfid = UnDealRfid.Dequeue();
                    var exists = false;
                    lock (lockObj)
                    {
                        exists = OrderEPC.Exists(_ => _.Equals(rfid));
                        if (exists)
                        {
                            OrderEPC.Remove(rfid);
                        }
                    }
                    //
                    if (exists)
                    {
                        //查找对应订单
                        await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                         {
                             var order = OrderSource.FirstOrDefault(_ => _.OrderDetailRfids.Exists(r => r.Equals(rfid)));
                             order.CheckCount++;
                             order.CheckInfo = "匹配中...";
                             order.CheckDetailRfids.Add(rfid);
                             CheckedEPC.Add(rfid);
                         }));
                    }
                    else
                    {
                        //获取接口 添加订单信息
                        var customId = await Task.Run(() => UserContext.ApiHelper.GetCustomIdBySN(rfid));
                        if (customId > 0)
                        {
                            await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                var order = OrderSource.FirstOrDefault(_ => _.CustomId == customId);
                                if (order != null)
                                {
                                    order.CheckCount++;
                                    order.CheckInfo = "匹配中...";
                                    order.CheckDetailRfids.Add(rfid);
                                    CheckedEPC.Add(rfid);
                                }
                                else
                                {
                                    if (!exceptionEPC.Contains(rfid))
                                    {
                                        exceptionEPC.Add(rfid);
                                    }
                                }
                            }));
                        }
                        else
                        {
                            if (!exceptionEPC.Contains(rfid))
                            {
                                exceptionEPC.Add(rfid);
                            }
                        }
                    }

                }
                _mainEvent.Reset();
            }
        }

        private async void CheckOrder()
        {
            foreach (var order in OrderSource)
            {
                if (order.CheckDetailRfids.Count == 0)
                {
                    continue;
                }
                ApiResult<string> result = new ApiResult<string>();
                if (order.InType == "反洗入库")
                {
                    result = await Task.Run(() => UserContext.ApiHelper.CheckRewashOrder(order.OrderNum, order.CheckDetailRfids));
                }
                else
                {
                    result = await Task.Run(() => UserContext.ApiHelper.CheckInOrder(order.OrderNum, order.CheckDetailRfids));
                }
                if (result.Success)
                {
                    order.CheckInfo = "清点完成";
                }
            }
        }

        private async void CheckOrder(OrderVM order)
        {
            if (MessageBox.Show("确认直接二次清点吗？", "提示", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                ApiResult<string> result = new ApiResult<string>();
                if (order.InType == "反洗入库")
                {
                    result = await Task.Run(() => UserContext.ApiHelper.CheckRewashOrder(order.OrderNum, order.OrderDetailRfids));
                }
                else
                {
                    result = await Task.Run(() => UserContext.ApiHelper.CheckInOrder(order.OrderNum, order.OrderDetailRfids));
                }
                if (result.Success)
                {
                    MessageBox.Show("清点成功");
                    InitOrderSource();
                }
                else
                {
                    MessageBox.Show("清点失败");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorLog(LogType.BASE, ex);
            }
        }

        public override void Dispose()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
        }
    }
    public class OrderVM : ViewModelBase
    {
        public OrderVM()
        {
            OrderDetailRfids = new List<string>();
            CheckDetailRfids = new List<string>();
        }
        /// <summary>
        /// 序号
        /// </summary>
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNum
        {
            get { return GetValue(() => OrderNum); }
            set { SetValue(() => OrderNum, value); }
        }

        public int CustomId { get; set; }

        /// <summary>
        /// 客户信息
        /// </summary>
        public string CustomName
        {
            get { return GetValue(() => CustomName); }
            set { SetValue(() => CustomName, value); }
        }

        /// <summary>
        /// 酒店名称
        /// </summary>
        public string StoreName
        {
            get { return GetValue(() => StoreName); }
            set { SetValue(() => StoreName, value); }
        }

        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime OrderDate
        {
            get { return GetValue(() => OrderDate); }
            set { SetValue(() => OrderDate, value); }
        }

        /// <summary>
        /// 入库类型
        /// </summary>
        public string InType
        {
            get { return GetValue(() => InType); }
            set { SetValue(() => InType, value); }
        }

        /// <summary>
        /// 订单明细中的Rfid
        /// </summary>
        public List<string> OrderDetailRfids { get; set; }

        /// <summary>
        /// 已校验
        /// </summary>
        public List<string> CheckDetailRfids { get; set; }

        /// <summary>
        /// rfid数量
        /// </summary>
        public int RfidCount
        {
            get { return GetValue(() => RfidCount); }
            set { SetValue(() => RfidCount, value); }
        }

        /// <summary>
        /// 已核验数量
        /// </summary>
        public int CheckCount
        {
            get { return GetValue(() => CheckCount); }
            set { SetValue(() => CheckCount, value); }
        }

        /// <summary>
        /// 已核验信息
        /// </summary>
        public string CheckInfo
        {
            get { return GetValue(() => CheckInfo); }
            set { SetValue(() => CheckInfo, value); }
        }
    }

    public class CheckCountForegroundConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2)
            {
                int checkCount = int.Parse(values[0].ToString());
                int totalCount = int.Parse(values[1].ToString());
                if (checkCount == totalCount)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.Red;
                }
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
