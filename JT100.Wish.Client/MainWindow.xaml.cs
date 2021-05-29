using JT100.Wish.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace JT100.Wish.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Login login;
        private BaseControl control;
        public ObservableCollection<MenuItemModel> MenuItemSource
        {
            get { return (ObservableCollection<MenuItemModel>)GetValue(MenuItemSourceProperty); }
            set { SetValue(MenuItemSourceProperty, value); }
        }

        public static DependencyProperty MenuItemSourceProperty = DependencyProperty.Register("MenuItemSource", typeof(ObservableCollection<MenuItemModel>), typeof(MainWindow), new PropertyMetadata(null));

        public MenuItemModel SelectMenu
        {
            get { return (MenuItemModel)GetValue(SelectMenuProperty); }
            set { SetValue(SelectMenuProperty, value); }
        }
        public static DependencyProperty SelectMenuProperty = DependencyProperty.Register("SelectMenu", typeof(MenuItemModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow(Login l)
        {
            InitializeComponent();
            login = l;
            Part_Menu.SelectionChanged += Part_Menu_SelectionChanged;
            MenuItemSource = new ObservableCollection<MenuItemModel>();
            Activated += MainWindow_Activated;
            //MenuItemSource.Add(new MenuItemModel { Name = "RFID绑定", DllName = "JT100.Wish.Component.dll", ClassFullName = "JT100.Wish.Component.RfidBinding" });
            MenuItemSource.Add(new MenuItemModel { Name = "出库单", DllName = "JT100.Wish.Component", ClassFullName = "JT100.Wish.Component.WishCustom" });
            MenuItemSource.Add(new MenuItemModel { Name = "入库单清点", DllName = "JT100.Wish.Component", ClassFullName = "JT100.Wish.Component.OrderCheckView" });
            MenuItemSource.Add(new MenuItemModel { Name = "二维码绑定", DllName = "JT100.Wish.Component", ClassFullName = "JT100.Wish.Component.RfidQrCodeBind" });
            MenuItemSource.Add(new MenuItemModel { Name = "读写器配置", DllName = "JT100.Wish.Component", ClassFullName = "JT100.Wish.Component.RfidReadSet" });
            SelectMenu = MenuItemSource.FirstOrDefault();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            login.Close();
        }

        private void Part_Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SwicthMenu();
        }

        private void SwicthMenu()
        {
            if (SelectMenu == null)
            {
                return;
            }
            control?.Dispose();
            if (string.IsNullOrEmpty(SelectMenu.DllName))
            {
                return;
            }
            if (string.IsNullOrEmpty(SelectMenu.ClassFullName))
            {
                return;
            }
            Assembly ass = Assembly.Load(SelectMenu.DllName);
            Type tp = ass.GetType(SelectMenu.ClassFullName, false);
            control = Activator.CreateInstance(tp) as BaseControl;
            Part_Content.Content = control;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState== WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定退出系统", "操作提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if(result== MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }
    }
    public class MenuItemModel : ViewModelBase
    {
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }
        public string DllName
        {
            get { return GetValue(() => DllName); }
            set { SetValue(() => DllName, value); }
        }
        public string ClassFullName
        {
            get { return GetValue(() => ClassFullName); }
            set { SetValue(() => ClassFullName, value); }
        }
    }
}
