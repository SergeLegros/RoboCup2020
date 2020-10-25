﻿using System;
using EventArgsLibrary;
using Utilities;
using Constants;

namespace RobotMessageGenerator
{
    public class RobotMsgGenerator
    {
        //Input events
        public void GenerateMessageSetSpeedConsigneToRobot(object sender, SpeedArgs e)
        {
            byte[] payload = new byte[12];
            payload.SetValueRange(((float)e.Vx).GetBytes(), 0);
            payload.SetValueRange(((float)e.Vy).GetBytes(), 4);
            payload.SetValueRange(((float)e.Vtheta).GetBytes(), 8);
            OnMessageToRobot((Int16)Commands.SetSpeedConsigne, 12, payload);
            OnSetSpeedConsigneToRobotReceived(e);
        }

        public event EventHandler<SpeedArgs> OnSetSpeedConsigneToRobotReceivedEvent;
        public virtual void OnSetSpeedConsigneToRobotReceived(SpeedArgs args)
        {
            OnSetSpeedConsigneToRobotReceivedEvent?.Invoke(this, args);
        }

        public void GenerateMessageSetIOPollingFrequencyToRobot(object sender, ByteEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0]= e.Value;
            OnMessageToRobot((Int16)Commands.SetIOPollingFrequency, 1, payload);
        }

        public void GenerateMessageSetSpeedConsigneToMotor(object sender, SpeedConsigneToMotorArgs e)
        {
            byte[] payload = new byte[5];
            payload.SetValueRange(((float)e.V).GetBytes(), 0);
            payload[4] = (byte)e.MotorNumber;
            OnMessageToRobot((Int16)Commands.SetMotorSpeedConsigne, 5, payload);
        }
        public void GenerateMessageTir(object sender, TirEventArgs e)
        {
            byte[] payload = new byte[4];
            payload.SetValueRange(((float)e.Puissance).GetBytes(), 0);
            OnMessageToRobot((Int16)Commands.TirCommand, 4, payload);
        }
        public void GenerateMessageMoveTirUp(object sender, EventArgs e)
        {
            OnMessageToRobot((Int16)Commands.MoveTirUp, 0, null);
        }
        
        public void GenerateMessageMoveTirDown(object sender, EventArgs e)
        {
            OnMessageToRobot((Int16)Commands.MoveTirDown, 0, null);
        }

        public void GenerateMessageEnablePowerMonitoring(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnablePowerMonitoring, 1, payload);
        }
        public void GenerateMessageEnableIOPolling(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableIOPolling, 1, payload);
        }

        public void GenerateMessageEnableDisableMotors(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] =Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableDisableMotors, 1, payload);
        }

        public void GenerateMessageEnableDisableTir(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableDisableTir, 1, payload);
        }

        public void GenerateMessageEnableAsservissement(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableAsservissement, 1, payload);
        }

        public void GenerateMessageEnablePIDDebugData(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnablePIDDebugData, 1, payload);
        }

        public void GenerateMessageEnableEncoderRawData(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableEncoderRawData, 1, payload);
        }

        public void GenerateMessageEnableMotorCurrentData(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableMotorCurrent, 1, payload);
        }

        public void GenerateMessageEnableMotorPositionData(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnablePositionData, 1, payload);
        }

        public void GenerateMessageEnableMotorSpeedConsigne(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);
            OnMessageToRobot((Int16)Commands.EnableMotorSpeedConsigne, 1, payload);
        }

        public void GenerateMessageSTOP(object sender, BoolEventArgs e)
        {
            byte[] payload = new byte[1];
            payload[0] = Convert.ToByte(e.value);

            OnMessageToRobot((Int16)Commands.EmergencySTOP, 1, payload);
        }

        public void GenerateMessageSetPIDValueToRobot(object sender, PIDDataArgs e)
        {
            byte[] payload = new byte[72];
            payload.SetValueRange(((float)(e.P_x)).GetBytes(), 0);
            payload.SetValueRange(((float)(e.I_x)).GetBytes(), 4);
            payload.SetValueRange(((float)(e.D_x)).GetBytes(), 8);
            payload.SetValueRange(((float)(e.P_y)).GetBytes(), 12);
            payload.SetValueRange(((float)(e.I_y)).GetBytes(), 16);
            payload.SetValueRange(((float)(e.D_y)).GetBytes(), 20);
            payload.SetValueRange(((float)(e.P_theta)).GetBytes(), 24);
            payload.SetValueRange(((float)(e.I_theta)).GetBytes(), 28);
            payload.SetValueRange(((float)(e.D_theta)).GetBytes(), 32);
            payload.SetValueRange(((float)(e.P_x_Limit)).GetBytes(), 36);
            payload.SetValueRange(((float)(e.I_x_Limit)).GetBytes(), 40);
            payload.SetValueRange(((float)(e.D_x_Limit)).GetBytes(), 44);
            payload.SetValueRange(((float)(e.P_y_Limit)).GetBytes(), 48);
            payload.SetValueRange(((float)(e.I_y_Limit)).GetBytes(), 52);
            payload.SetValueRange(((float)(e.D_y_Limit)).GetBytes(), 56);
            payload.SetValueRange(((float)(e.P_theta_Limit)).GetBytes(), 60);
            payload.SetValueRange(((float)(e.I_theta_Limit)).GetBytes(), 64);
            payload.SetValueRange(((float)(e.D_theta_Limit)).GetBytes(), 68);
            OnMessageToRobot((Int16)Commands.SetPIDValues, 72, payload);
        }
        //public void GenerateTextMessage(object sender, EventArgsLibrary.SpeedConsigneArgs e)
        //{
        //    byte[] payload = new byte[12];
        //    payload.SetValueRange(e.Vx.GetBytes(), 0);
        //    payload.SetValueRange(e.Vy.GetBytes(), 4);
        //    payload.SetValueRange(e.Vtheta.GetBytes(), 8);
        //    OnMessageToRobot(Commands., 12, payload);
        //}

        //Output events
        public delegate void SpeedConsigneEventHandler(object sender, MessageToRobotArgs e);
        public event EventHandler<MessageToRobotArgs> OnMessageToRobotGeneratedEvent;
        public virtual void OnMessageToRobot(Int16 msgFunction, Int16 msgPayloadLength, byte[] msgPayload)
        {
            OnMessageToRobotGeneratedEvent?.Invoke(this, new MessageToRobotArgs { MsgFunction = msgFunction, MsgPayloadLength = msgPayloadLength, MsgPayload = msgPayload });
        }
    }
}
