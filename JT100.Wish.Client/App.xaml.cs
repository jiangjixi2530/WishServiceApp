using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JT100.Wish.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\CheckUp.bat";
            Process process = new Process();//创建进程对象  
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = strPath;//设定需要执行的命令  
            startInfo.Arguments = "\"" + AppDomain.CurrentDomain.BaseDirectory + "\"";//“/C”表示执行完命令后马上退出  
            startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
            startInfo.RedirectStandardInput = false;//不重定向输入  
            startInfo.RedirectStandardOutput = true; //重定向输出  
            startInfo.CreateNoWindow = true;//不创建窗口  
            process.StartInfo = startInfo;
            try
            {
                process.Start();
            }
            catch
            {
            }
            Thread.Sleep(1000);
            while (Process.GetProcessesByName("SimpleUpdater").Length > 0)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
