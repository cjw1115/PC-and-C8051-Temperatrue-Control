void display(uchar loc,uchar tm_data)
{
    //通用显示模块；
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
    //通用温度显示模块；
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


uchar ADCConvert(void)	//数据采集函数
{
    uchar  x;		//定义变量存放结果
    ADC = 0;		//启动A/D转换
    delay();		//调用延时函数等待转换结束
    x = ADC;		//读取A/D转换结果
    return x;		//返回温度数据采集结果
}
uint  Temperature(uchar x)	//温度计算函数
{
    uint  y;		//定义变量存放计算结果
    y = x*1000/255;	//计算扩大10倍的温度数值
    return y;		//返回扩大10倍的温度数值
}
//==============采集数据，获取温度==================//


void Temperature_measure(void)
{
    uchar a;
    uint b;
    a=ADCConvert();//温度数据采集；
    b=Temperature(a);//结果存入temperature
		temperatureInt=b/100;
		temperatureFloat=b%100;
}          