using JT100.Wish.Component;
using JT100.Wish.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JT100.Wish.Client
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {

        public string Pwd { get; set; }
        public Login()
        {
            InitializeComponent();
            Loaded += Login_Loaded;
            Part_AutoLogin.Checked += Part_AutoLogin_Checked;
            Part_RememberPwd.Checked += Part_RememberPwd_Checked;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            var config = UserContext.UserXmlProvider.GetConfig<LoginConfig>("LoginConfig");
            if (config != null)
            {
                Txt_Account.Text = config.Account;
                if (!string.IsNullOrEmpty(config.Pwd))
                {
                    Txt_Pwd.Password = DESEncrypt.DesDecrypt(config.Pwd);
                }
                Part_AutoLogin.IsChecked = config.IsAutoLogin;
                Part_RememberPwd.IsChecked = config.IsRememberPwd;
                if (config.IsAutoLogin)
                {
                    AccountLogin(Txt_Account.Text, Txt_Pwd.Password);
                }
            }
        }

        private void Part_RememberPwd_Checked(object sender, RoutedEventArgs e)
        {
            if (Part_RememberPwd.IsChecked != true)
            {
                Part_AutoLogin.IsChecked = true;
            }
        }

        private void Part_AutoLogin_Checked(object sender, RoutedEventArgs e)
        {
            if (Part_AutoLogin.IsChecked == true)
            {
                Part_RememberPwd.IsChecked = true;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string account = Txt_Account.Text;
            string pwd = Txt_Pwd.Password;
            AccountLogin(account, pwd);
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private async void AccountLogin(string account, string pwd)
        {
            //md5加密
            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(account))
            {
                return;
            }
            var md5Pwd = DESEncrypt.Md5(pwd);
            var result = await Task.Run(() => UserContext.ApiHelper.Login(account, md5Pwd));
            if (result.Success)
            {
                LoginConfig config = new LoginConfig();
                config.Account = account;
                config.IsRememberPwd = Part_RememberPwd.IsChecked == true;
                if (config.IsRememberPwd)
                {
                    config.Pwd = DESEncrypt.DesEncrypt(pwd);
                }
                config.IsAutoLogin = Part_AutoLogin.IsChecked == true;
                UserContext.UserXmlProvider.SetConfig<LoginConfig>("LoginConfig", config);
                MainWindow main = new MainWindow(this);
                main.Show();
            }
            else
            {
                MessageBox.Show(result.Msg);
            }
        }
    }

    [Serializable]
    public class LoginConfig
    {
        public string Account { get; set; }
        public string Pwd { get; set; }

        public bool IsRememberPwd { get; set; }
        public bool IsAutoLogin { get; set; }
    }
}
