using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JT100.Wish.Component
{
    /// <summary>
    /// WishCustomOutOrder.xaml 的交互逻辑
    /// </summary>
    public partial class WishCustomOutOrder : Window
    {
        public ICommand ShowDetailCommand
        {
            get; set;
        }

        public bool ShowDetail
        {
            get
            {
                return (bool)GetValue(ShowDetailProperty);
            }
            set { SetValue(ShowDetailProperty, value); }
        }
        public static readonly DependencyProperty ShowDetailProperty = DependencyProperty.Register("ShowDetail", typeof(bool), typeof(WishCustomOutOrder));

        public CustomViewModel CurrentCustom
        {
            get
            {
                return (CustomViewModel)GetValue(CurrentCustomProperty);
            }
            set { SetValue(CurrentCustomProperty, value); }
        }

        public static readonly DependencyProperty CurrentCustomProperty = DependencyProperty.Register("CurrentCustom", typeof(CustomViewModel), typeof(WishCustomOutOrder));

        public ObservableCollection<OutOrderVM> OutOrderSource
        {
            get
            {
                return (ObservableCollection<OutOrderVM>)GetValue(OutOrderSourceProperty);
            }
            set { SetValue(OutOrderSourceProperty, value); }
        }
        public static readonly DependencyProperty OutOrderSourceProperty = DependencyProperty.Register("OutOrderSource", typeof(ObservableCollection<OutOrderVM>), typeof(WishCustomOutOrder));

        /// <summary>
        /// 出库订单明细
        /// </summary>
        public ObservableCollection<OutOrderDetail> OrderDetails
        {
            get
            {
                return (ObservableCollection<OutOrderDetail>)GetValue(OrderDetailsProperty);
            }
            set { SetValue(OrderDetailsProperty, value); }
        }
        public static readonly DependencyProperty OrderDetailsProperty = DependencyProperty.Register("OrderDetails", typeof(ObservableCollection<OutOrderDetail>), typeof(WishCustomOutOrder));

        public WishCustomOutOrder(CustomModel custom)
        {
            InitializeComponent();
            OutOrderSource = new ObservableCollection<OutOrderVM>();
            CurrentCustom = new CustomViewModel();
            OrderDetails = new ObservableCollection<OutOrderDetail>();
            CurrentCustom.Id = custom.Id;
            CurrentCustom.CustomName = custom.Name;
            CurrentCustom.StoreName = custom.StoreName;
            CurrentCustom.Address = custom.StoreAddress;
            ShowDetailCommand = new RelayCommand<OutOrderVM>((obj) =>
            {
                ShowDetail = true;
                OrderDetails?.Clear();
                for (int i = 0; i < obj.Details.Count; i++)
                {
                    OrderDetails.Add(new OutOrderDetail { Index = i + 1, WareName = obj.Details[i].WareName });
                }
            });
            Loaded += WishCustomOutOrder_Loaded;
        }

        private void WishCustomOutOrder_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private async void LoadOrders()
        {
            var customId = CurrentCustom.Id;
            var orders = await Task.Run(() => UserContext.ApiHelper.GetCustomOutOrders(customId));
            if (orders.Count > 0)
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    var vm = new OutOrderVM()
                    {
                        Index = i + 1,
                        OrderNo = orders[i].OrderNo,
                        CreateTime = orders[i].CreateTime,
                        Amount = orders[i].Amount,
                        PayAmount = orders[i].PayAmount,
                        Status = orders[i].Status == 1 ? "已出库" : "已签收"
                    };
                    vm.Details = new List<OutOrderDetail>();
                    for (int j = 0; j < orders[i].Details.Count; j++)
                    {
                        vm.Details.Add(new OutOrderDetail
                        {
                            Index = j + 1,
                            WareName = orders[i].Details[j].WareTypeName
                        });
                    }
                    OutOrderSource.Add(vm);
                }
            }
        }

    }
    public class OutOrderVM : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }

        public string OrderNo
        {
            get { return GetValue(() => OrderNo); }
            set { SetValue(() => OrderNo, value); }
        }

        public DateTime CreateTime
        {
            get { return GetValue(() => CreateTime); }
            set { SetValue(() => CreateTime, value); }
        }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal Amount
        {
            get { return GetValue(() => Amount); }
            set { SetValue(() => Amount, value); }
        }
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount
        {
            get { return GetValue(() => PayAmount); }
            set { SetValue(() => PayAmount, value); }
        }
        /// <summary>
        /// 订单状态 1 已出库2 已签收
        /// </summary>
        public string Status
        {
            get { return GetValue(() => Status); }
            set { SetValue(() => Status, value); }
        }

        /// <summary>
        /// 出库单明细
        /// </summary>
        public List<OutOrderDetail> Details
        {
            get { return GetValue(() => Details); }
            set { SetValue(() => Details, value); }
        }
    }

    /// <summary>
    /// 出库明细
    /// </summary>
    public class OutOrderDetail : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }
        public string WareName
        {
            get { return GetValue(() => WareName); }
            set { SetValue(() => WareName, value); }
        }
        public int Count
        {
            get { return GetValue(() => Count); }
            set { SetValue(() => Count, value); }
        }
    }
}
