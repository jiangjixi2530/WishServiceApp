using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JT100.Wish.Component
{
    /// <summary>
    /// 客户信息
    /// </summary>
    public class WishCustom : BaseControl
    {
        private List<string> Rfids = new List<string>();
        private List<CustomModel> Customs { get; set; }
        DataGrid _dataGrid;
        /// <summary>
        /// 是否显示客户列表
        /// </summary>
        public bool ShowCustomList
        {
            get
            {
                return (bool)GetValue(ShowCustomListProperty);
            }
            set { SetValue(ShowCustomListProperty, value); }
        }

        public readonly static DependencyProperty ShowCustomListProperty = DependencyProperty.Register("ShowCustomList", typeof(bool), typeof(WishCustom), new PropertyMetadata(true));

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount
        {
            get
            {
                return (int)GetValue(TotalCountProperty);
            }
            set { SetValue(TotalCountProperty, value); }
        }

        public readonly static DependencyProperty TotalCountProperty = DependencyProperty.Register("TotalCount", typeof(int), typeof(WishCustom), new PropertyMetadata(0));

        /// <summary>
        /// 选中的客户
        /// </summary>
        public CustomViewModel SelectedCustom
        {
            get
            {
                return (CustomViewModel)GetValue(SelectedCustomProperty);
            }
            set { SetValue(SelectedCustomProperty, value); }
        }

        public readonly static DependencyProperty SelectedCustomProperty = DependencyProperty.Register("SelectedCustom", typeof(CustomViewModel), typeof(WishCustom), new PropertyMetadata(null));
        public ObservableCollection<CustomViewModel> CustomSource
        {
            get
            {
                return (ObservableCollection<CustomViewModel>)GetValue(CustomSourceProperty);
            }
            set { SetValue(CustomSourceProperty, value); }
        }
        public readonly static DependencyProperty CustomSourceProperty = DependencyProperty.Register("CustomSource", typeof(ObservableCollection<CustomViewModel>), typeof(WishCustom));

        public ObservableCollection<WareOutDetailVM> WareOutDetailSource
        {
            get
            {
                return (ObservableCollection<WareOutDetailVM>)GetValue(WareOutDetailSourceProperty);
            }
            set { SetValue(WareOutDetailSourceProperty, value); }
        }
        public readonly static DependencyProperty WareOutDetailSourceProperty = DependencyProperty.Register("WareOutDetailSource", typeof(ObservableCollection<WareOutDetailVM>), typeof(WishCustom));

        public ICommand TransferWare { get; set; }

        public ICommand ShowOutWareOrder { get; set; }

        public ICommand GoBack { get; set; }

        public ICommand CreateOutOrder { get; set; }

        public ICommand MatchWareType { get; set; }
        static WishCustom()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WishCustom), new FrameworkPropertyMetadata(typeof(WishCustom)));
        }
        public WishCustom()
        {
            CustomSource = new ObservableCollection<CustomViewModel>();
            TransferWare = new RelayCommand<int>((customId) =>
            {
                CheckOutWare(customId);

            });
            ShowOutWareOrder = new RelayCommand<int>((customId) =>
             {
                 ShowOutWareOrderWindow(customId);
             });
            GoBack = new RelayCommand(() =>
            {
                UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
                Rfids.Clear();
                TotalCount = 0;
                ShowCustomList = true;
            });
            CreateOutOrder = new RelayCommand(() =>
            {
                CreateOrder();
            });
            MatchWareType = new RelayCommand(() =>
            {
                MatchWare();
            });
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _dataGrid = VisualTreeProvider.FindFirstChild<DataGrid>(this, "Part_DataGrid");
            InitSource();
        }
        private async void InitSource()
        {
            Customs = await Task.Run(() => UserContext.ApiHelper.GetCustomList());
            var index = 0;
            while (index < Customs.Count)
            {
                CustomSource.Add(new CustomViewModel() { Index = index + 1, Id = Customs[index].Id, CustomName = Customs[index].Name, StoreName = Customs[index].StoreName, Address = Customs[index].StoreAddress });
                index++;
            }
        }

        /// <summary>
        /// 出库
        /// </summary>
        /// <param name="customId"></param>
        public void CheckOutWare(int customId)
        {
            ShowCustomList = false;
            Rfids.Clear();
            TotalCount = Rfids.Count;
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
            SelectedCustom = CustomSource.FirstOrDefault(_ => _.Id == customId);
            WareOutDetailSource = new ObservableCollection<WareOutDetailVM>();
            var custom = Customs.Find(_ => _.Id == customId);
            if (custom == null)
            {
                return;
            }
            int index = 1;
            foreach (var wareInfo in custom.WareInfos)
            {
                WareOutDetailSource.Add(new WareOutDetailVM() { Index = index++, WareTypeId = wareInfo.WareTypeId, WareTypeName = wareInfo.WareTypeName, Capacity = wareInfo.Capacity, Stock = wareInfo.Stock, MaxCount = wareInfo.Capacity - wareInfo.Stock });
            }
            WeIXin.Message message = new WeIXin.Message();
            message.ToUser = "oZj5R6gdMGjC8yffq_18P1_Q_Wlg";
            message.MessageType = WeIXin.MessageType.BalanceChanged;
            message.Title = new WeIXin.MessageModel() { Text = "您的账户余额发生变动啦", ColorBrush = "#173177" };
            message.Msgs = new List<WeIXin.MessageModel>();
            message.Msgs.Add(new WeIXin.MessageModel() { Text = "消费扣款", ColorBrush = "#173177" });
            message.Msgs.Add(new WeIXin.MessageModel() { Text = "12.00元", ColorBrush = "#173177" });
            message.Msgs.Add(new WeIXin.MessageModel() { Text = "787.00元", ColorBrush = "#173177" });
            message.Msgs.Add(new WeIXin.MessageModel() { Text = "吉之住酒店", ColorBrush = "#173177" });
            message.Msgs.Add(new WeIXin.MessageModel() { Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm"), ColorBrush = "#173177" });
            message.Remark = new WeIXin.MessageModel() { Text = "若需要帮助，可联系客服", ColorBrush = "#173177" };
            var result = WeIXin.WeiXinHelper.SendMessage(message).Result;
        }

        private void RfidReadProvider_OnDataReceived(object o, string epc)
        {
            if (!Rfids.Exists(_ => _.Equals(epc)))
            {
                Rfids.Add(epc);
                this.Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate ()
                {
                    TotalCount = Rfids.Count;
                },
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        public void ShowOutWareOrderWindow(int customId)
        {
            var custom = Customs.FirstOrDefault(_ => _.Id == customId);
            WishCustomOutOrder show = new WishCustomOutOrder(custom);
            Window onwer = Window.GetWindow(this);
            show.Owner = onwer;
            show.ShowDialog();
        }
        public async void MatchWare()
        {
            if (Rfids.Count > 0)
            {
                var wareTypes = await Task.Run(() => UserContext.ApiHelper.GetGroupDataBySN(Rfids));
                foreach (var ware in wareTypes)
                {
                    var wareOut = WareOutDetailSource.FirstOrDefault(_ => _.WareTypeId == ware.WareType);
                    if (wareOut != null)
                    {
                        wareOut.RfidCount = ware.Count;
                    }
                }
            }
        }
        public async void CreateOrder()
        {
            if (TotalCount == 0)
            {
                return;
            }
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            int totalCount = 0;
            foreach (var ware in WareOutDetailSource)
            {
                if (ware.RfidCount > ware.MaxCount)
                {
                    MessageBox.Show($"{ware.WareTypeName}超出数量");
                    return;
                }
                totalCount += ware.RfidCount;
            }
            if (totalCount != Rfids.Count)
            {
                MessageBox.Show("匹配数量与总数量不一致，请重新匹配");
                return;
            }
            var customId = SelectedCustom.Id;
            var result = await Task.Run(() => UserContext.ApiHelper.CreateOutOrder(customId, Rfids));
            if (result.Success)
            {
                ShowCustomList = true;
            }
            else
            {
                MessageBox.Show("出库失败，" + result.Msg);
                UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
        }
    }
    public class CustomViewModel : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }
        public int Id
        {
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }

        public string CustomName
        {
            get { return GetValue(() => CustomName); }
            set { SetValue(() => CustomName, value); }
        }

        public string StoreName
        {
            get { return GetValue(() => StoreName); }
            set { SetValue(() => StoreName, value); }
        }

        public string Address
        {
            get { return GetValue(() => Address); }
            set { SetValue(() => Address, value); }
        }
    }

    public class WareOutDetailVM : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }
        public int WareTypeId
        {
            get { return GetValue(() => WareTypeId); }
            set { SetValue(() => WareTypeId, value); }
        }

        public string WareTypeName
        {
            get { return GetValue(() => WareTypeName); }
            set { SetValue(() => WareTypeName, value); }
        }
        public int Capacity
        {
            get { return GetValue(() => Capacity); }
            set { SetValue(() => Capacity, value); }
        }
        public int Stock
        {
            get { return GetValue(() => Stock); }
            set { SetValue(() => Stock, value); }
        }

        public int MaxCount
        {
            get { return GetValue(() => MaxCount); }
            set { SetValue(() => MaxCount, value); }
        }

        public int RfidCount
        {
            get { return GetValue(() => RfidCount); }
            set { SetValue(() => RfidCount, value); }
        }

        /// <summary>
        /// 识别的Rfid
        /// </summary>
        public List<string> Rfids
        {
            get; set;
        }
    }
}
