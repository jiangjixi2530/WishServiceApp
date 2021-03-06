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
        private Dictionary<string, string> cacheWareInfos;
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

        public ICommand ClearCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        static WishCustom()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WishCustom), new FrameworkPropertyMetadata(typeof(WishCustom)));
        }
        public WishCustom()
        {
            cacheWareInfos = new Dictionary<string, string>();
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
            ClearCommand = new RelayCommand(() =>
            {
                Rfids.Clear();
                TotalCount = Rfids.Count;
            });
            SaveCommand = new RelayCommand(() =>
            {
                SaveInfo();
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

        private async void SaveInfo()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            int totalCount = 0;
            foreach (var ware in WareOutDetailSource)
            {
                totalCount += ware.RfidCount;
            }
            if (totalCount != Rfids.Count)
            {
                MessageBox.Show("匹配数量与总数量不一致，请重新匹配");
                return;
            }
            if (Rfids.Count > 0)
            {

            }
        }
        public override void Dispose()
        {
            base.Dispose();
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
        }
    }
}
