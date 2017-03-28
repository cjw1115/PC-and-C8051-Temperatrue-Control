 //------------------------------------------------------------------------------//     
 //	              "data_defineA.c"������Ӳ����Դ�������ļ�                        // 
 //------------------------------------------------------------------------------//

typedef unsigned char  uchar;
typedef unsigned int   uint;
#define  ADC  XBYTE[0x2000]
#define  DAC  XBYTE[0x4000]
#define  LED1         XBYTE[0x0000]
#define  LED2         XBYTE[0x0001]
#define  LED3         XBYTE[0x0002]
#define  LED4         XBYTE[0x0003]
#define  KEY_WR  XBYTE[0x0004]
#define  KEY_RD   XBYTE[0x0005]
#define  TIMER  	0x8000

//uchar counter0,counter1;
//uchar set_high,set_low,set_loc;
uchar temperature_set;
int temperatureInt;
int temperatureFloat;
uchar a[4];
bit display_flag;
uchar table[]={0xC0,0xF9,0xA4,0xB0,0x99,0x92,0x82,0xF8,0x80,0x90,0x7f};

unsigned char data table1[]={0x50,0xF9,0x4A,0x49,0xE1,0x45,0x44,0xD9,0x40,0x41,0xbf};
unsigned char data table2[]={0x0c,0xAF,0xC8,0x8a,0x2b,0x1a,0x18,0x8F,0x08,0x0a,0xf7};
unsigned char data table3[]={0x44,0xF5,0x86,0x85,0x35,0x0d,0x0c,0xE5,0x04,0x05,0xfb};
unsigned char data table4[]={0x0c,0xEE,0x58,0x4A,0xAA,0x0B,0x09,0x6E,0x08,0x0a,0xf7};


int delayStartMin=0;
int delayStartSec=0;
int allDelayStartTime=0;
uchar isDelayStart=0;

int delayStopMin=0;
int delayStopSec=0;
int allDelayStopTime=0;
uchar isDelayStop=0;


uchar workStatus=0;