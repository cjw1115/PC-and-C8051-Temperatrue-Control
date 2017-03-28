using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using 上位机温度控制系统.Model;

namespace 上位机温度控制系统
{
    public class MainPageVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<string> ComList { get; set; } = new ObservableCollection<string> { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
        private string _currentCom;
        public string CurrentCom
        {
            get { return _currentCom; }
            set { _currentCom = value;
                OnPropertyChanged();
            }
        }

        private int _baudRate = 9600;
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value;
                OnPropertyChanged();
            }
        }
        private string _defaultFillField = "FF";

        public string DefaultFillField
        {
            get { return _defaultFillField; }
            set { _defaultFillField = value;
                OnPropertyChanged();
            }
        }

        private bool _isOpen = false;
        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value;
                OnPropertyChanged();
            }
        }
        private SerialPort _port;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _parseCts;
        public ICommand OpenCommand { get; set; }
        public void Open()
        {
            try
            {
                var comIndex = 0;
                if (IsOpen == false)
                {
                    while (comIndex < ComList.Count)
                    {
                        try
                        {
                            _port = new SerialPort();
                            _port.Parity = Parity.None;

                            _port.PortName = ComList[comIndex];
                            _port.BaudRate = BaudRate;

                            _port.Open();
                            CurrentCom = ComList[comIndex];

                            _cts = new CancellationTokenSource();
                            _parseCts = new CancellationTokenSource();
                            Task.Run(() =>
                            {
                                ParseCommand();
                            }, _parseCts.Token);
                            Task.Run(() => {
                                Recieve();
                            }, _cts.Token);
                            IsOpen = true;
                            break;
                        }
                        catch (IOException io)
                        {
                            comIndex++;
                        }
                    }
                    if (!IsOpen)
                    {
                        MainPage.Notify("没有可用的端口");
                    }

                    
                }
                else
                {
                    _cts.Cancel(false);
                    _parseCts.Cancel(false);
                    _port.Close();

                    IsOpen = false;
                }


            }
            catch(Exception e)
            {
                MainPage.Notify(e.Message);
            }
            
        }
        public void Recieve()
        {
            int recieveCount = 0;
            while (!_cts.IsCancellationRequested)
            {
                var re=_port.ReadByte();
                switch (recieveCount)
                {
                    case 0:
                        if (re == 'C')
                        {
                            OutputCommand?.Clear();
                            OutputCommand = new CommandModel();
                            
                            OutputCommand.SyncCode1 = Convert.ToChar(re).ToString();
                            recieveCount++;
                        }
                        else
                        {
                            recieveCount = 0;
                        }
                        break;
                    case 1:
                        if (re == 'O')
                        {
                            OutputCommand.SyncCode2 = Convert.ToChar(re).ToString();
                            recieveCount++;
                        }
                        else
                        {
                            recieveCount = 0;
                        }
                        break;
                    case 2:
                        if (re == 'M')
                        {
                            OutputCommand.SyncCode3 = Convert.ToChar(re).ToString();
                            recieveCount++;
                        }
                        else
                        {
                            recieveCount = 0;
                        }
                        break;
                    case 3:
                        recieveCount++;
                        OutputCommand.ControlCode = re.ToString("x2");
                        break;
                    case 4:
                        recieveCount++;
                        OutputCommand.ControlVal1 = re.ToString("x2");
                        break;
                    case 5:
                        recieveCount++;
                        OutputCommand.ControlVal2 = re.ToString("x2");

                        var command = new CommandModel();
                        command.SyncCode1 = OutputCommand.SyncCode1;
                        command.SyncCode2 = OutputCommand.SyncCode2;
                        command.SyncCode3 = OutputCommand.SyncCode3;
                        command.ControlCode = OutputCommand.ControlCode;
                        command.ControlVal1= OutputCommand.ControlVal1;
                        command.ControlVal2 = OutputCommand.ControlVal2;
                        _commandQueue.Enqueue(command);

                        break;
                    default:
                        recieveCount = 0;
                        break;
                }

            }
        }

        public ICommand ExcuteCommand { get; set; }
        public ICommand ContinuesExcuteCommand { get; set; }
        public void Excute()
        {
            if (IsOpen)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(InputCommand.SyncCode1[0]);
                sb.Append(InputCommand.SyncCode2[0]);
                sb.Append(InputCommand.SyncCode3[0]);
                var syncs = Encoding.ASCII.GetBytes(sb.ToString());

                var controlCode = Convert.ToByte(InputCommand.ControlCode, 16);
                var controlVal1= Convert.ToByte(InputCommand.ControlVal1,16);
                var controlVal2 = Convert.ToByte(InputCommand.ControlVal2, 16);
                
                _buffer = new byte[6];
                for (int i = 0; i < 3; i++)
                {
                    _buffer[i] = syncs[i];
                }
                _buffer[3] = controlCode;
                _buffer[4] = controlVal1;
                _buffer[5] = controlVal2;
                _port.Write(_buffer, 0, _buffer.Length);
            }
            else
            {
                MainPage.Notify("端口未打开！");
            }
        }

        private bool _isContinuesExcute = false;

        public bool IsContinuesExcute
        {
            get { return _isContinuesExcute; }
            set { _isContinuesExcute = value;OnPropertyChanged(); }
        }

        private DispatcherTimer _timer;
        private byte[] _buffer;
        public void ContinuesExcute()
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer( DispatcherPriority.Normal);
                _timer.Interval = TimeSpan.FromMilliseconds(300);
                _timer.Tick += _timer_Tick;
            }

            if (IsOpen && IsContinuesExcute == false)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(InputCommand.SyncCode1[0]);
                sb.Append(InputCommand.SyncCode2[0]);
                sb.Append(InputCommand.SyncCode3[0]);
                var syncs = Encoding.ASCII.GetBytes(sb.ToString());

                var controlCode = Convert.ToByte(InputCommand.ControlCode, 16);
                var controlVal1 = Convert.ToByte(InputCommand.ControlVal1, 16);
                var controlVal2 = Convert.ToByte(InputCommand.ControlVal2, 16);

                _buffer = new byte[6];
                for (int i = 0; i < 3; i++)
                {
                    _buffer[i] = syncs[i];
                }
                _buffer[3] = controlCode;
                _buffer[4] = controlVal1;
                _buffer[5] = controlVal2;

                _timer.Start();
                IsContinuesExcute = true;
            }
            else
            {
                _timer.Stop();
                IsContinuesExcute = false;
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if(InputCommand.ControlCode != "A8")
            {
                IsContinuesExcute = false;

                _timer.Stop();
                return;
            }
            if (IsOpen&& IsContinuesExcute)
            {
                _port.Write(_buffer, 0, _buffer.Length);
            }
            else
            {
                IsContinuesExcute = false;

                _timer.Stop();
                _timer.Tick -= _timer_Tick;
            }
            
        }

        private CommandModel _inputCommand;

        public CommandModel InputCommand
        {
            get { return _inputCommand; }
            set { _inputCommand = value; OnPropertyChanged(); }
        }

        private CommandModel _outputCommand;

        public CommandModel OutputCommand
        {
            get { return _outputCommand; }
            set { _outputCommand = value; OnPropertyChanged(); }
        }

        private string _recieveContent;

        public string RecieveContent
        {
            get { return _recieveContent; }
            set { _recieveContent = value;
                OnPropertyChanged();
            }
        }

        private int _tempCellectionCount = 100;
        public int TempCellectionCount
        {
            get
            {
                return _tempCellectionCount;
            }
            set { _tempCellectionCount = value; OnPropertyChanged(); }
        }
        
        private List<double> _tempCellection;
        public List<double> TempCellection
        {
            get { return _tempCellection; }
            set { _tempCellection = value;OnPropertyChanged(); }
        }
        #region 温度控制相关命令
        private CommandModel _selectedCommand;
        public CommandModel SelectedCommand
        {
            get { return _selectedCommand; }
            set { _selectedCommand = value; OnPropertyChanged(); }
        }

        private int _readTempInt;
        public int ReadTempInt
        {
            get { return _readTempInt; }
            set { _readTempInt = value; OnPropertyChanged(); }
        }
        private int _readTempFloat;
        public int ReadTempFloat
        {
            get { return _readTempFloat; }
            set { _readTempFloat = value; OnPropertyChanged(); }
        }

        private int _setTempInt;
        public int SetTempInt
        {
            get { return _setTempInt; }
            set { _setTempInt = value; OnPropertyChanged(); }
        }
        private int _setTempFloat;
        public int SetTempFloat
        {
            get { return _setTempFloat; }
            set { _setTempFloat = value; OnPropertyChanged(); }
        }

        public ICommand SetTempCommand{ get; set; }
        public void SetTemp()
        {
            if (SelectedCommand == null)
            {
                SelectedCommand = new CommandModel();
            }
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A0";
            SelectedCommand.ControlVal1 =  SetTempInt.ToString("x2");
            SelectedCommand.ControlVal2 = SetTempFloat.ToString("x2");
         
            InputCommand = SelectedCommand;

            Excute();
        }
        public ICommand UpTempCommand { get; set; }
        public void UpTemp()
        {
            SetTempInt++;
            if (SetTempInt >= 100)
            {
                SetTempInt = 99;
            }
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A1";
            SelectedCommand.ControlVal1 = SetTempInt.ToString("x2");
            SelectedCommand.ControlVal2 = SetTempFloat.ToString("x2");

            InputCommand = SelectedCommand;

            Excute();
        }
        public ICommand DownTempCommand { get; set; }
        public void DownTemp()
        {
            SetTempInt--;
            if (SetTempInt <= 0)
            {
                SetTempInt = 0;
            }
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A2";
            SelectedCommand.ControlVal1 = SetTempInt.ToString("x2");
            SelectedCommand.ControlVal2 = SetTempFloat.ToString("x2");

            InputCommand = SelectedCommand;

            Excute();
        }

        #endregion

        #region 命令解释器
        ConcurrentQueue<CommandModel> _commandQueue = new ConcurrentQueue<CommandModel>();
        public void ParseCommand()
        {
            while (!_parseCts.IsCancellationRequested)
            {
                CommandModel command;
                _commandQueue.TryDequeue(out command);
                if (command == null)
                    continue;
                switch (command.ControlCode.ToUpper())
                {
                    case "A8"://当前温度查询
                        //Convert.ToInt32(command.ControlVal1.ToString(), 16);
                        ReadTempInt = int.Parse(command.ControlVal1, System.Globalization.NumberStyles.AllowHexSpecifier);
                        ReadTempFloat = int.Parse(command.ControlVal2,System.Globalization.NumberStyles.AllowHexSpecifier);

                        var list = TempCellection;
                        if (list.Count > TempCellectionCount)
                        {
                            for (int i = 0; i < list.Count- TempCellectionCount; i++)
                            {
                                list.RemoveAt(0);
                            }
                            
                        }
                        list.Add(ReadTempInt);
                        var newList = new List<double>();
                        foreach (var item in list)
                        {
                            newList.Add(item);
                        }
                        TempCellection = newList;
                        break;
                    case "AA"://延时启动时间查询
                        var startMin = int.Parse(command.ControlVal1, System.Globalization.NumberStyles.AllowHexSpecifier);
                        var startSec = int.Parse(command.ControlVal2, System.Globalization.NumberStyles.AllowHexSpecifier);
                        StartTime = $"{startMin}:{startSec}";
                        break;
                    case "AB"://延时停止时间查询
                        var stoptMin = int.Parse(command.ControlVal1, System.Globalization.NumberStyles.AllowHexSpecifier);
                        var stoptSec = int.Parse(command.ControlVal2, System.Globalization.NumberStyles.AllowHexSpecifier);
                        StopTime = $"{stoptMin}:{stoptSec}";
                        break;
                    case "AE"://控制工作状态查询
                        WorkStatus = "";
                        var status = int.Parse(command.ControlVal1, System.Globalization.NumberStyles.AllowHexSpecifier);
                        if ((status & 0x80) == 0x80)
                        {
                            WorkStatus += "控温启动   ";
                        }
                        else
                        {
                            WorkStatus += "控温停止   ";
                        }
                        if ((status & 0x40) == 0x40)
                        {
                            WorkStatus += "延时启动开启   ";
                        }
                        else
                        {
                            WorkStatus += "延时启动关闭   ";
                        }
                        if ((status & 0x20) == 0x20)
                        {
                            WorkStatus += "延时停止开启  ";
                        }
                        else
                        {
                            WorkStatus += "延时停止关闭  ";
                        }
                        break;
                    default:
                        break;
                }
            }
            
        }
        #endregion

        #region 温度控制
        private bool _isControlling;

        public bool IsControlling
        {
            get { return _isControlling; }
            set { _isControlling = value; OnPropertyChanged(); }
        }

        public ICommand ControlTempCommand { get; set; }
        public void ControlTemp()
        {
            IsControlling = !IsControlling;

            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A3";
            SelectedCommand.ControlVal1 = "FF";
            SelectedCommand.ControlVal2 = "FF";

            InputCommand = SelectedCommand;

            Excute();

        }
#endregion

        #region 温度查询
        public ICommand QueryTempCommand { get; set; }
        public void QueryTmep()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A8";
            SelectedCommand.ControlVal1 = "Ff";
            SelectedCommand.ControlVal2 = "FF";

            InputCommand = SelectedCommand;

            Excute();

        }
#endregion

        #region 延时控制相关
        private int _delayStartMin;
        public int DelayStartMin
        {
            get { return _delayStartMin; }
            set { _delayStartMin = value; OnPropertyChanged(); }
        }
        private int _delayStartSec;
        public int DelayStartSec
        {
            get { return _delayStartSec; }
            set { _delayStartSec = value; OnPropertyChanged(); }
        }

        private int _delayStopMin;
        public int DelayStopMin
        {
            get { return _delayStopMin; }
            set { _delayStopMin = value; OnPropertyChanged(); }
        }
        private int _delayStopSec;
        public int DelayStopSec
        {
            get { return _delayStopSec; }
            set { _delayStopSec = value; OnPropertyChanged(); }
        }

        public ICommand DelayStartCommand { get; set; }
        public void DelayStart()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A4";
            SelectedCommand.ControlVal1 = DelayStartMin.ToString("x2");
            SelectedCommand.ControlVal2 = DelayStartSec.ToString("x2");

            InputCommand = SelectedCommand;

            Excute();
        }
        public ICommand DelayStopCommand { get; set; }
        public void DelayStop()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "A5";
            SelectedCommand.ControlVal1 = DelayStopMin.ToString("x2");
            SelectedCommand.ControlVal2 = DelayStopSec.ToString("x2");

            InputCommand = SelectedCommand;

            Excute();
        }

        private string _startTime;

        public string StartTime
        {
            get { return _startTime; }
            set { _startTime = value;OnPropertyChanged(); }
        }
        
        public ICommand QueryStartTimeCommand { get; set; }
        public void QueryStartTime()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "AA";
            SelectedCommand.ControlVal1 = "FF";
            SelectedCommand.ControlVal2 = "FF";

            InputCommand = SelectedCommand;

            Excute();

        }
        private string _stopTime;

        public string StopTime
        {
            get { return _stopTime; }
            set { _stopTime = value;OnPropertyChanged(); }
        }

        public ICommand QueryStopTimeCommand { get; set; }
        public void QueryStopTime()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "AB";
            SelectedCommand.ControlVal1 = "FF";
            SelectedCommand.ControlVal2 = "FF";

            InputCommand = SelectedCommand;

            Excute();

        }
        #endregion

        #region 控制工作状态查询
        private string _workStauts;
        public string WorkStatus
        {
            get { return _workStauts; }
            set { _workStauts = value; OnPropertyChanged(); }
        }
        public ICommand QueryWorkStatusCommand { get; set; }
        public void QueryWorkStatus()
        {
            SelectedCommand.SyncCode1 = InputCommand.SyncCode1;
            SelectedCommand.SyncCode2 = InputCommand.SyncCode2;
            SelectedCommand.SyncCode3 = InputCommand.SyncCode3;
            SelectedCommand.ControlCode = "AE";
            SelectedCommand.ControlVal1 = "FF";
            SelectedCommand.ControlVal2 = "FF";

            InputCommand = SelectedCommand;

            Excute();
        }
        #endregion

        public MainPageVM()
        {
            TempCellection = new List<double>();

            InputCommand = new CommandModel { SyncCode1="C", SyncCode2 = "O",SyncCode3 = "M"};
            OutputCommand = new CommandModel();

            OpenCommand = new Command(Open);
            ExcuteCommand = new Command(Excute);
            ContinuesExcuteCommand = new Command(ContinuesExcute);

            UpTempCommand = new Command(UpTemp);
            DownTempCommand = new Command(DownTemp);
            SetTempCommand = new Command(SetTemp);

            SelectedCommand = new CommandModel();

            ControlTempCommand = new Command(ControlTemp);

            QueryTempCommand = new Command(QueryTmep);

            DelayStartCommand = new Command(DelayStart);
            DelayStopCommand = new Command(DelayStop);

            QueryStartTimeCommand = new Command(QueryStartTime);
            QueryStopTimeCommand = new Command(QueryStopTime);

            QueryWorkStatusCommand = new Command(QueryWorkStatus);

            MakeTempCommand = new Command(MakeTemp);
            SpeedDownCommand = new Command(SpeedDown);
            SpeedUpCommand = new Command(SpeedUp);
        }
        private double speed = 100;
        public ICommand SpeedDownCommand { get; set; }
        public ICommand SpeedUpCommand
        {
            get;set;
        }
        public void SpeedUp()
        {
            speed /= 10;
        }
        public void SpeedDown()
        {
            speed *= 10;
        }
        public ICommand MakeTempCommand { get; set; }

        Random rand = new Random();
        public void MakeTemp()
        {
            Task.Run(async () =>
            {
                var angel = 0;
                while (true)
                {
                    var list = TempCellection;
                    if (list.Count > TempCellectionCount)
                    {
                        for (int i = 0; i <= list.Count - TempCellectionCount; i++)
                        {
                            list.RemoveAt(0);
                        }

                    }

                    list.Add(Math.Sin(2*Math.PI*angel/360) * 50 + 50);
                    angel +=1;
                    if (angel >= 360)
                    {
                        angel = 0;
                    }
                    var newList = new List<double>();
                    foreach (var item in list)
                    {
                        newList.Add(item);
                    }
                    TempCellection = newList;
                    await Task.Delay(TimeSpan.FromMilliseconds(speed));

                }
            }
            );
        }

    }
}
