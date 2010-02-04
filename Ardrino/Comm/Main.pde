#ifndef _Main_h
#define _Main_h
#endif

//#include "Main.h"
#include "Comm.h"
#include "Queue.h"

void setup()
{
  Serial.begin(MessageRate);
  pinMode(12,OUTPUT);
}

void loop()
{
  //Priority of actions: (EStop)->(Send next motor action)->(Read incomming Message)->(Request more messges if needed)
  long Message = 0;  
  if(FlagEStop)
  {
    //**************************
    //Stop Everything
    //**************************
  }
  else if(!FlagMotorDelay) //if the motors are not doing anything and are ready to move again.
  {
    FlagMotorDelay=1;  //set the motor flag as not being ready.
    //********************************
    //Process Queue
    //somehow figure out when the motors are done, delay?
    //FlagMotorDelay=0;
    //********************************
  }
  else if(Serial.available()>0) //get message on serial buffer if one exists
  {
    MessageFilter((long*) Serial.read());
  }
  else if(QueueLength<250)
  {
    Serial.print("MoreMessages");  //Ask computer for more messages.
  }
  
}