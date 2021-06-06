using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JT100.Wish.Component
{
    public class OutOrderView : BaseControl
    {
        #region 私有属性
        private List<string> Rfids = new List<string>();
        private List<CustomModel> _customs;
        #endregion
        #region  公共属性
        /// <summary>
        /// 选中的客户
        /// </summary>
        public CustomViewModel SelectedCustom
        {
            get
            {
                return GetValue(() => SelectedCustom);
            }
            set
            {
                SetValue(() => SelectedCustom, value);
            }
        }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount
        {
            get
            {
                return GetValue(() => TotalCount);
            }
            set
            {
                SetValue(() => TotalCount, value);
            }
        }
        public ICommand TransferWare { get; set; }
        public ICommand ShowOutWareOrder { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand MatchWareType { get; set; }
        public ICommand SaveMatchWare { get; set; }

        public ICommand StartRead { get; set; }
        public ICommand CreateOutOrder { get; set; }

        public ICommand DeleteWareInfo { get; set; }
        #endregion 

        #region 依赖项属性
        /// <summary>
        /// 客户数据源
        /// </summary>
        public ObservableCollection<CustomViewModel> CustomSource
        {
            get
            {
                return (ObservableCollection<CustomViewModel>)GetValue(CustomSourceProperty);
            }
            set { SetValue(CustomSourceProperty, value); }
        }
        public readonly static DependencyProperty CustomSourceProperty = DependencyProperty.Register("CustomSource", typeof(ObservableCollection<CustomViewModel>), typeof(OutOrderView));

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

        public readonly static DependencyProperty ShowCustomListProperty = DependencyProperty.Register("ShowCustomList", typeof(bool), typeof(OutOrderView), new PropertyMetadata(true));

        /// <summary>
        /// 出库数据源
        /// </summary>
        public ObservableCollection<WareOutDetailVM> WareOutDetailSource
        {
            get
            {
                return (ObservableCollection<WareOutDetailVM>)GetValue(WareOutDetailSourceProperty);
            }
            set { SetValue(WareOutDetailSourceProperty, value); }
        }
        public readonly static DependencyProperty WareOutDetailSourceProperty = DependencyProperty.Register("WareOutDetailSource", typeof(ObservableCollection<WareOutDetailVM>), typeof(OutOrderView));

        public ObservableCollection<WareOutDetailType> WareOutDetailTypeSource
        {
            get
            {
                return (ObservableCollection<WareOutDetailType>)GetValue(WareOutDetailTypeSourceProperty);
            }
            set { SetValue(WareOutDetailTypeSourceProperty, value); }
        }

        public readonly static DependencyProperty WareOutDetailTypeSourceProperty = DependencyProperty.Register("WareOutDetailTypeSource", typeof(ObservableCollection<WareOutDetailType>), typeof(OutOrderView));
        #endregion
        static OutOrderView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OutOrderView), new FrameworkPropertyMetadata(typeof(OutOrderView)));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OutOrderView()
        {
            WareOutDetailTypeSource = new ObservableCollection<WareOutDetailType>();
            CustomSource = new ObservableCollection<CustomViewModel>();
            TransferWare = new RelayCommand<int>((customId) =>
            {
                CheckOutWare(customId);

            });
            MatchWareType = new RelayCommand(() =>
            {
                MatchWare();
            });
            SaveMatchWare = new RelayCommand(() =>
            {
                SaveWareType();
            });
            StartRead = new RelayCommand(() =>
            {
                StartReadRfid();
            });
            CreateOutOrder = new RelayCommand(() =>
            {
                CreateOutOrderInfo();
            });
            DeleteWareInfo = new RelayCommand<WareOutDetailType>((obj) =>
            {
                DeleteWareInfoByClick(obj);
            });
            ShowOutWareOrder = new RelayCommand<CustomViewModel>((obj) =>
            {
                var custom = _customs.FirstOrDefault(_ => _.Id == obj.Id);
                WishCustomOutOrder show = new WishCustomOutOrder(custom);
                Window onwer = Window.GetWindow(this);
                show.Owner = onwer;
                show.ShowDialog();
            });
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitSource();
        }

        #region 函数
        /// <summary>
        /// 初始化数据源
        /// </summary>
        private async void InitSource()
        {
            _customs = await Task.Run(() => UserContext.ApiHelper.GetCustomList());
            var index = 0;
            while (index < _customs.Count)
            {
                CustomSource.Add(new CustomViewModel() { Index = index + 1, Id = _customs[index].Id, CustomName = _customs[index].Name, StoreName = _customs[index].StoreName, Address = _customs[index].StoreAddress });
                index++;
            }
        }
        public async void MatchWare()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
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

        public void SaveWareType()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            if (WareOutDetailSource.FirstOrDefault(_ => _.RfidCount > 0) == null)
            {
                MessageBox.Show("请先匹配物料类型");
                return;
            }
            var totalCount = WareOutDetailSource.Sum(_ => _.RfidCount);
            if (totalCount != TotalCount || totalCount == 0)
            {
                MessageBox.Show("识别数量与匹配数量不一致，请检查物料绑定情况");
                return;
            }
            WareOutDetailType wareOutDetail = new WareOutDetailType();
            wareOutDetail.Name = $"第{(WareOutDetailTypeSource.Count + 1).ToString()}车";
            wareOutDetail.TotalCount = totalCount;
            wareOutDetail.Details = new ObservableCollection<WareOutDetailBaseVM>();
            wareOutDetail.Rfids = Rfids.ToArray().ToList();
            Rfids.Clear();
            TotalCount = 0;
            foreach (var detail in WareOutDetailSource)
            {
                if (detail.RfidCount == 0)
                {
                    continue;
                }
                WareOutDetailBaseVM vm = new WareOutDetailBaseVM();
                vm.WareTypeName = detail.WareTypeName;
                vm.WareTypeId = detail.WareTypeId;
                vm.RfidCount = detail.RfidCount;
                wareOutDetail.Details.Add(vm);
                detail.RfidCount = 0;
            }
            WareOutDetailTypeSource.Add(wareOutDetail);
        }

        /// <summary>
        /// 出库
        /// </summary>
        /// <param name="customId"></param>
        public void CheckOutWare(int customId)
        {
            ShowCustomList = false;
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
            SelectedCustom = CustomSource.FirstOrDefault(_ => _.Id == customId);
            WareOutDetailSource = new ObservableCollection<WareOutDetailVM>();
            WareOutDetailTypeSource = new ObservableCollection<WareOutDetailType>();
            var custom = _customs.Find(_ => _.Id == customId);
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
        /// <summary>
        /// 开始识别
        /// </summary>
        private void StartReadRfid()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            Rfids.Clear();
            TotalCount = 0;
            foreach (var detail in WareOutDetailSource)
            {
                detail.RfidCount = 0;
            }
            UserContext.RfidReadProvider.OnDataReceived += RfidReadProvider_OnDataReceived;
        }
        private async void CreateOutOrderInfo()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
            if (WareOutDetailSource.FirstOrDefault(_ => _.RfidCount > 0) != null)
            {
                MessageBox.Show("请先匹配物料类型并保存");
                return;
            }
            int totalCount = WareOutDetailTypeSource.Sum(_ => _.TotalCount) + WareOutDetailSource.Sum(_ => _.RfidCount);
            if (totalCount == 0)
            {
                MessageBox.Show("请先匹配物料类型");
                return;

            }
            foreach (var wareOutDetailType in WareOutDetailTypeSource)
            {
                foreach (var detail in wareOutDetailType.Details)
                {
                    var exists = WareOutDetailSource.FirstOrDefault(_ => _.WareTypeId == detail.WareTypeId);
                    if (exists != null)
                    {
                        exists.RfidCount += detail.RfidCount;
                    }
                }
            }
            foreach (var ware in WareOutDetailSource)
            {
                if (ware.RfidCount > ware.MaxCount)
                {
                    MessageBox.Show($"{ware.WareTypeName}超出数量");
                    return;
                }
            }
            List<string> rfids = new List<string>();
            foreach (var ware in WareOutDetailTypeSource)
            {
                rfids.AddRange(ware.Rfids);
            }
            var customId = SelectedCustom.Id;
            var result = await Task.Run(() => UserContext.ApiHelper.CreateOutOrder(customId, rfids));
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

        public void DeleteWareInfoByClick(WareOutDetailType wareOutDetailType)
        {
            WareOutDetailTypeSource.Remove(wareOutDetailType);
            for (int i = 0; i < WareOutDetailTypeSource.Count; i++)
            {
                WareOutDetailTypeSource[i].Name = $"第{i + 1}车";
            }
        }
        public override void Dispose()
        {
            UserContext.RfidReadProvider.OnDataReceived -= RfidReadProvider_OnDataReceived;
        }
        #endregion

        #region event
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
        #endregion

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

    /// <summary>
    /// 出库明细
    /// </summary>
    public class WareOutDetailVM : WareOutDetailBaseVM
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
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
    }

    public class WareOutDetailBaseVM : ViewModelBase
    {
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
        public int RfidCount
        {
            get { return GetValue(() => RfidCount); }
            set { SetValue(() => RfidCount, value); }
        }
    }

    public class WareOutDetailType : ViewModelBase
    {
        public int Index
        {
            get { return GetValue(() => Index); }
            set { SetValue(() => Index, value); }
        }

        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        public int TotalCount
        {
            get { return GetValue(() => TotalCount); }
            set { SetValue(() => TotalCount, value); }
        }

        public ObservableCollection<WareOutDetailBaseVM> Details
        {
            get { return GetValue(() => Details); }
            set { SetValue(() => Details, value); }
        }

        public List<string> Rfids
        {
            get; set;
        }

    }
}
