#include"c8051f020.h"
#include"absacc.h"
#include"data_define.c"
#include "Init_Device.c"

void data_tran(void);
void PostTemp(void);

uchar tran_buf[3]={0,0,0};
uchar rece_buf[3];

int isControlling=0;
int TempInt=0;

void  delay(void)		      //延时函数
{
    uint  i;			      //定义循环控制变量
    for(i=0;i<TIMER;i++)   i=i;   //空操作循环等待
}


void display(uchar loc,uchar tm_data)
{
    //ͨ����ʾģ�飻
    switch(loc)
    {
        case 1:  LED1=table1[tm_data];break;
        case 2:  LED2=table2[tm_data];break;
        case 3:  LED3=table3[tm_data];break;
        case 4:  LED4=table4[tm_data];
    }
}
void Temperature_display(uchar fun)
{
    //ͨ���¶���ʾģ�飻
    uchar  x;
    switch(fun)
    {
        case 1: 
            x=temperatureInt/10;
            display(3,x);	
            x=temperatureInt%10;    
            display(4,x); 
	        break;
        case 2:  
            x=temperature_set/10;
            display(1,x);
            x=temperature_set%10;
            display(2,x);			
    }
}


uchar ADCConvert(void)	//���ݲɼ�����
{
    uchar  x;		//�����������Ž���
    ADC = 0;		//����A/Dת��
    delay();		//������ʱ�����ȴ�ת������
    x = ADC;		//��ȡA/Dת������
    return x;		//�����¶����ݲɼ�����
}
int Temperature(uchar x)	//�¶ȼ��㺯��
{
	 int  y;		//�����������ż�������
    y = x*100/255;	//��������10�����¶���ֵ
    return y;		//��������10�����¶���ֵ
}
//==============�ɼ����ݣ���ȡ�¶�==================//


void Temperature_measure(void)
{
    uchar a;
    int b;
    a=ADCConvert();//�¶����ݲɼ���
    b=Temperature(a);//��������temperature
		temperatureInt=b;
}

       
void pid_control(void)
{
	int error;
	int pid_out;
	float error_sumH;
	float error_old;
	float error_sumM;
	float error_sumL;

	Temperature_measure();
	//PostTemp();
	error=temperature_set-temperatureInt;
	if (error>2)
		{
		  pid_out=127;
		}
	else if (error<=2&&error>-2)
		{
			if (temperatureInt>=70)
				{
					error_sumH = error_sumH+error;
					pid_out=35*error + 0.01*error_sumH + 1.0*(error-error_old);
					error_old = error;
				}
			else if (temperatureInt>=25)
				{
					error_sumM = error_sumM + error;
					pid_out = 35*error + 0.01*error_sumM + 1.0*(error-error_old);
					error_old = error;
				}
			else
				{
			               error_sumL = error_sumL + error;
					pid_out = 35*error + 0.01*error_sumL + 1.0*(error-error_old);
					error_old = error;
				}
			
		}
            else
				{
					pid_out=-127;
				}
	DAC= pid_out+128;

}
void InitPort()
{
	EA=1;
	ES0=1;																											//��ֹUART0�ж�
	EIE2=0x00;																										//��ֹUART1�ж�
	CKCON   = 0x00; 																						//ѡ��ϵͳʱ��Ϊʱ��Դ
	T2CON   = 0x00;																							//C/T2ʹ��
	SCON0   = 0x50;																							//UART0��ʽ1��8λ�ɱ䲨���ʣ���������Ч
	TMOD    = 0x21;																						//C/T0��ʽ1,16λ����
	TL1 	= 0xFD;																								//C/T1������9600����ֵ
  TH1 	= 0xFD;																								//C/T1������9600����ֵ
	TR1=1;	

	ET0=1;TR0=1;
}
void data_tran(void)
{
	int tran_count=6;  							//���巢�ͼ���������ֵΪ5
	REN0=0;									//UART0��ֹ���գ�������ң�ص����ݷ��ͱ�������
	while(tran_count)
	{
		switch(tran_count)							//���Է��ͼ�������ȷ����ǰ���͵��ֽ�λ��
		{
			case 6: 	
					SBUF0='C';					//��1�ֽڣ�A���ķ��ʹ���
					tran_count--;					//���ͼ�������1
					break;	

			case 5:	SBUF0='O';					//��2�ֽڣ�B���ķ��ʹ���
					tran_count--;					//���ͼ�������1
					break;		
			
			case 4:	SBUF0='M';					//��2�ֽڣ�B���ķ��ʹ���
					tran_count--;					//���ͼ�������1
					break;		
			case 3:	
					SBUF0=tran_buf[0];		//��������
					tran_count--;		
					break;
			case 2:	
			
					SBUF0=tran_buf[1];		//��������
					tran_count--;		
					break;
			case 1:	
					SBUF0=tran_buf[2];		//��������
					tran_count--;		
					break;
		}

		while(TI0==0);								//�ȴ�UART0���ͽ�����UART0�����жϱ�־TI0=1��
		TI0=0;									//����UART0�����жϱ�־
	}
	REN0=1;										//UART0�������գ�REN0=1��
}
void Parse()
{
	switch(rece_buf[0])
	{
		case 0xA0:
		case 0xA1://temp++
		case 0xA2://temp--;
		temperature_set=rece_buf[1];
		Temperature_display(2);
		break;
		case 0xA3://start or stop control temp
			if(isControlling==0){
				isControlling=1;
			}
			else{
				isControlling=0;
			}
		break;
		case 0xA4://delay statup time set 
			delayStartMin=rece_buf[1];
			delayStartSec=rece_buf[2];
			allDelayStartTime=delayStartMin*60+delayStartSec;
			isDelayStart=1;

		break;
		case 0xA5://delay end time set
			delayStopMin=rece_buf[1];
			delayStopSec=rece_buf[2];
			allDelayStopTime=delayStopMin*60+delayStartSec;
			isDelayStop=1;

		break;
		case 0xA6://D/A convert
		break;
		case 0xA8://query temp
				Temperature_measure();
				PostTemp();
			break;
		case 0xAA:
			tran_buf[0]=0xAA;
			tran_buf[1]=allDelayStartTime/60;;
			tran_buf[2]=allDelayStartTime%60;;
			data_tran();
			break;
		case 0xAB:
			tran_buf[0]=0xAB;
			tran_buf[1]=allDelayStopTime/60;;
			tran_buf[2]=allDelayStopTime%60;;
			data_tran();
			break;
		case 0xAE:
			workStatus=0;
			if(isControlling==1){
				workStatus|=0x80;
			}
			if(isDelayStart){
				workStatus|=0x40;
			}
			if(isDelayStop){
				workStatus|=0x20;
			}
			tran_buf[0]=0xAE;
			tran_buf[1]=workStatus;
			tran_buf[2]=0xFF;
			data_tran();
			break;
		default:break;
	}
}

uchar rece_state;
void data_rece(void) interrupt  4
{
	uchar temp, rece_counter;
		if(TI0==1)  TI0=0; 				   					//发送中断标志为1则清除该标志
		else													//接收中断标志为1进行数据接收处理
		{
			temp = SBUF0;									//读取UART0接收缓冲器数据
			switch(rece_state)									//测试UART0接收操作状态	
			{
				case 0:
					if(temp == 'C')
					rece_state = 1;
					else
						rece_state=0;			//接收的字符为A,接收状态更改为1
				break;
				case 1:
					if(temp == 'O')
					rece_state = 2;	
					else
						rece_state=0;		//接收的字符为A,接收状态更改为1
					break;
				case 2:
					if(temp == 'M')
					{
						rece_counter=3;
						rece_state = 3;	
					}
					else
						rece_state=0;		//接收的字符为A,接收状态更改为1
					break;
				case 3:
				case 4:			
				case 5:	
					rece_state++;
					rece_buf[3-rece_counter] = temp;			//读取的数据送数据接收寄存器
						rece_counter--;						//接收字节计数器减1
						if(rece_counter == 0) 
						 {
							 rece_state = 0;
							 Parse();
						 }		//接收计数器为0, 接收状态复位为0
			}
			RI0=0;											//清除接收中断标志
		}	

}

void Timer_Init(void)
{
    //��ʱ����ʼ����ģ�飻
    TMOD |= 0x02;	//��ʱ��0����ʱģʽ����ʽ��
    TH0 = 0x94;	//100uS����ֵ

}
int counter0;
void Timer_int() interrupt  1
 {
     //��ʱ�жϷ���ģ�飻		
     if(counter0++>10000) 	
     {
		 if(isDelayStart==1){
			 allDelayStartTime--;
			 if(allDelayStartTime==0){
				 isControlling=1;
				 isDelayStart=0;
			 }
		 }

		 if(isDelayStop==1){
			 allDelayStopTime--;
			 if(allDelayStopTime==0){
				 isControlling=0;
				 isDelayStop=0;
			 }
		 }
		 counter0=0;
	 }
}          

void main()
{
	int i=0;
	Init_Device();
	InitPort();
	Timer_Init();
	while(1)
	{
		if(isControlling==1)
		{
			pid_control();
		}
		else
			DAC=128;

			
		Temperature_measure();
		Temperature_display(1);
		//PostTemp();
	}
}
void PostTemp(void)
{
	tran_buf[0]=0xA8;
	tran_buf[1]=temperatureInt;
	tran_buf[2]=00;

	data_tran();
}   
