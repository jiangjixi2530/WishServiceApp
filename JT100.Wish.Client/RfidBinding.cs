using GalaSoft.MvvmLight.Command;
using JT100.Wish.Tool;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JT100.Wish.Component
{
    public class RfidBinding : Control
    {
        public ICommand OpenMachineCommand;
        RfidReadProvider helper = new RfidReadProvider();
        static RfidBinding()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RfidBinding), new FrameworkPropertyMetadata(typeof(RfidBinding)));
        }
        public RfidBinding()
        {
            OpenMachineCommand = new RelayCommand(() =>
            {
                OpenMachine();
            });
        }


        private void OpenMachine()
        {
            helper.DisconnectByCom();
            helper.ConnectByCom("Com3", 115200);
        }
    }
}
