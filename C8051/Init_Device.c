/**********************************************************************************
 *                                                                                *
 *	    	EM-023单片机仿真头测试例程---北京工业大学电工电子实验教学中心         *
 *                                                                                *
 **********************************************************************************/

 //------------------------------------------------------------------------------//     
 //	        "Init_DeviceA.c"，设备初始化函数组，在主程序中调用Init_DeviceA()       // 
 //------------------------------------------------------------------------------//

/*禁止看门狗函数,源码来自C8051F020的config2的reset sources设置。程序调试阶段需
  要关闭watch_doog功能，否则会干扰调试过程。程序调试完成以后，主程序中加入看门
  狗操作语句的情况下可以开启看门狗，以便在死机后系统能够复位并重新启动。*/

void Reset_Sources_Init()
{
  WDTCN     = 0xDE;
  WDTCN     = 0xAD;
}


//交叉开关配置函数，完成基本功能选择和端口资源的分配
void Port_IO_Init()
{
  // P2MDOUT   = 0xFF;
  // P3MDOUT   = 0xFF;
  XBR0      = 0x0F;																//UART0的TX0,RX0 连到端口引脚P0.0,P0.1
																									//SPI0的SCK, MISO, MOSI连接到端口引脚P0.2,P0.3,P0.4
																									//SPI0的NSS连接到端口引脚P1.0
																									//MSBus的SDA, SCL连接到端口引脚P1.1,P1.2
    
	XBR1      = 0x1E;																//T0,T1 连到端口引脚P1.4,P1.6
																									//INT0 连到端口引脚P1.5
																									//INT1 连到端口引脚P1.7
																									//CEX0 连到端口引脚P1.3
    
	XBR2      = 0xC2;																//禁止弱上拉	
																									//交叉开关使能
																									//使用低端EMI

	  	  
	EMI0CF    = 0x0F;																//选择P2-P3作为总线端口，复用方式，仅使用外部RAM 
  EMI0TC    = 0xDF;																//总线访问地址建立时间3个SYS周期
																									//总线访问读/写脉冲宽度8个SYS周期
																									//总线访问地址保持时间3个SYS周期
/*
	XBR0      = 0x04;					//P0.0  -  TX0  (SPI0), Open-Drain, Digital
    								//P0.1  -  RX0 (SPI0), Open-Drain, Digital
    								
	XBR1      = 0x04;					//P0.2  -  INT0 (Tmr0), Open-Drain, Digital
	XBR2      = 0xC0;					//禁止弱上拉	
										//交叉开关使能
										//使用低端EMI
	EMI0CF    = 0x0B;					//0-0xfff片内RAM可用，外部设备须避免使用该地址空间
*/
}

//振荡器初始化函数,使用片外晶体振荡器
void Oscillator_Init(void)
{
  uint i = 0;
  OSCXCN    = 0x67;			  	//使用片外晶体谐振器，f>6.7MHz
  for (i = 0; i < 3000; i++);   			//等待1mS完成操作
  while ((OSCXCN & 0x80) == 0); 		//检测外部振荡器是否有效
  OSCICN    = 0x08;			  		//选择外部振荡器作为系统时钟，禁止内部时钟
}								

//参考电压源初始化函数
void Voltage_Reference_Init()
{
  REF0CN    = 0x03;
}

//ADC0初始化函数
void ADC0_Init()
{
	AMX0CF	= 0x00;
	AMX0SL	= 0x00;
	ADC0CF	= 0x28;
	ADC0CN	= 0x81;
}

//ADC1初始化函数
void ADC1_Init()
{
	AMX1SL	= 0x07;
	ADC1CF  = 0x19;
  ADC1CN  = 0x80;
}

void DAC_Init()
{
  DAC0CN    = 0x84;
	DAC1CN    = 0x84;

}

//设备初始化函数，在主程序中调用Init_DeviceA()
void Init_Device()
{
  Reset_Sources_Init();														//复位源初始化
  Port_IO_Init();																	//端口初始化
  Oscillator_Init();															//时钟源初始化
	Voltage_Reference_Init();
	ADC0_Init();
	ADC1_Init();
	DAC_Init();
}