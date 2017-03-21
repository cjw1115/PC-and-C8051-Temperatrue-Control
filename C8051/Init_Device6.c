//---------------------------------------------------------------------------------------------------------------------//     
//	    "Init_Device6.c"，题目6设备初始化函数组，在主程序中调用Init_Device6()        // 
//---------------------------------------------------------------------------------------------------------------------//

/*禁止看门狗函数,源码来自C8051F020的config2的reset sources设置。程序调试阶段需
  要关闭watch_doog功能，否则会干扰调试过程。程序调试完成以后，主程序中加入看门
  狗操作语句的情况下可以开启看门狗，以便在死机后系统能够复位并重新启动。*/

void Reset_Sources_Init(void)
{
  WDTCN     = 0xDE;
  WDTCN     = 0xAD;
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

//交叉开关配置函数，完成基本功能选择和端口资源的分配
void Port_IO_Init(void)
{
	XBR0      = 0x04;					//P0.0  -  TX0  (SPI0), Open-Drain, Digital
    								//P0.1  -  RX0 (SPI0), Open-Drain, Digital
    								
	XBR1      = 0x04;					//P0.2  -  INT0 (Tmr0), Open-Drain, Digital
	XBR2      = 0xC0;					//禁止弱上拉	
										//交叉开关使能
										//使用低端EMI
EMI0CF    = 0x0B;					//0-0xfff片内RAM可用，外部设备须避免使用该地址空间
}

// 设备初始化函数，在主程序中调用Init_Device6()
void Init_Device6(void)
{
  Reset_Sources_Init();					//复位源初始化
  Oscillator_Init();						//时钟源初始化
	Port_IO_Init();						//端口初始化
}
