using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 上位机温度控制系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread readThread;
        private SerialPort _port;
        public MainWindow()
        {
            InitializeComponent();

            readThread = new Thread(Read);
            _port = new SerialPort("COM1", 9600, Parity.None,8);
            _port.ReadTimeout = 500;
            _port.WriteTimeout = 500;

            _port.DataReceived += _port_DataReceived; ;
        }
        public void Read()
        {
            try
            {
                string message = _port.ReadLine();
                Console.WriteLine(message);
            }
            catch (TimeoutException) { }

        }
        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            tbData.Text=_port.ReadLine();
        }

        private void btnPortControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_port.IsOpen)
                {

                    _port.Open();
                    readThread.Start();
                }
                else
                {
                    _port.Close();
                    readThread.Join();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (_port.IsOpen)
            {
                btnPortControl.Content = "Close";
            }
            else
            {
                btnPortControl.Content = "Open";
            }
        }

        private void cbPortname_Selected(object sender, RoutedEventArgs e)
        {
            if (!_port.IsOpen)
            {
                _port.PortName = ((ComboBoxItem)cbPortname.SelectedValue).Content as string;
            }
        }
    }
}
