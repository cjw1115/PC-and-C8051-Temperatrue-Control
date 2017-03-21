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
    uchar  x;		//���������Ž��
    ADC = 0;		//����A/Dת��
    delay();		//������ʱ�����ȴ�ת������
    x = ADC;		//��ȡA/Dת�����
    return x;		//�����¶����ݲɼ����
}
uint  Temperature(uchar x)	//�¶ȼ��㺯��
{
    uint  y;		//���������ż�����
    y = x*1000/255;	//��������10�����¶���ֵ
    return y;		//��������10�����¶���ֵ
}
//==============�ɼ����ݣ���ȡ�¶�==================//


void Temperature_measure(void)
{
    uchar a;
    uint b;
    a=ADCConvert();//�¶����ݲɼ���
    b=Temperature(a);//�������temperature
		temperatureInt=b/100;
		temperatureFloat=b%100;
}          