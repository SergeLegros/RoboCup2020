using Emgu.CV;
using HeatMap;
using PerceptionManagement;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using WorldMap;

namespace EventArgsLibrary
{
    public class DataReceivedArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    public class StringArgs : EventArgs
    {
        public string Value { get; set; }
    }
    public class DoubleArgs : EventArgs
    {
        public double Value { get; set; }
    }
    public class BitmapImageArgs : EventArgs
    {
        public Bitmap Bitmap { get; set; }
        public string Descriptor { get; set; }
    }

    //[ProtoContract]
    //[ProtoInclude(502, typeof(OpenCvMatImageArgsLog))]
    //public class OpenCvMatImageArgs : EventArgs
    //{
    //    [ProtoMember(1)]
    //    public Mat Mat { get; set; }
    //    [ProtoMember(2)]
    //    public string Descriptor { get; set; }
    //    public void Dispose()
    //    {
    //        Mat.Dispose();
    //    }
    //}

    public class MessageDecodedArgs : EventArgs
    {
        public int MsgFunction { get; set; }
        public int MsgPayloadLength { get; set; }
        public byte[] MsgPayload { get; set; }
    }

    public class MessageEncodedArgs : EventArgs
    {
        public byte[] Msg { get; set; }
    }

    public class MessageToRobotArgs : EventArgs
    {
        public Int16 MsgFunction { get; set; }
        public Int16 MsgPayloadLength { get; set; }
        public byte[] MsgPayload { get; set; }
    }

    [ProtoContract]
    [ProtoInclude(503, typeof(SpeedDataEventArgs))]
    public class SpeedConsigneArgs : EventArgs
    {
        [ProtoMember(1)]
        public int RobotId { get; set; }
        [ProtoMember(2)]
        public float Vx { get; set; }
        [ProtoMember(3)]
        public float Vy { get; set; }
        [ProtoMember(4)]
        public float Vtheta { get; set; }
    }

    [ProtoContract]
    public class SpeedDataEventArgs : SpeedConsigneArgs
    {
        [ProtoMember(1)]
        public uint EmbeddedTimeStampInMs;
    }

    public class TirEventArgs : EventArgs
    {
        public int RobotId { get; set; }
        public float Puissance { get; set; }
    }

    [ProtoContract]
    public class IMUDataEventArgs : EventArgs
    {
        [ProtoMember(1)]
        public uint EmbeddedTimeStampInMs;
        [ProtoMember(2)]
        public double accelX;
        [ProtoMember(3)]
        public double accelY;
        [ProtoMember(4)]
        public double accelZ;
        [ProtoMember(5)]
        public double gyrX;
        [ProtoMember(6)]
        public double gyrY;
        [ProtoMember(7)]
        public double gyrZ;
        [ProtoMember(8)]
        public double magX;
        [ProtoMember(9)]
        public double magY;
        [ProtoMember(10)]
        public double magZ;
    }
    public class MotorsCurrentsEventArgs : EventArgs
    {
        public uint timeStampMS;
        public double motor1;
        public double motor2;
        public double motor3;
        public double motor4;
        public double motor5;
        public double motor6;
        public double motor7;
    }
    public class EncodersRawDataEventArgs : EventArgs
    {
        public uint timeStampMS;
        public int motor1;
        public int motor2;
        public int motor3;
        public int motor4;
        public int motor5;
        public int motor6;
        public int motor7;
    }
    public class MotorsPositionDataEventArgs : MotorsCurrentsEventArgs
    {

    }

    public class MotorsVitesseDataEventArgs : EventArgs
    {
        public uint timeStampMS;
        public double vitesseMotor1;
        public double vitesseMotor2;
        public double vitesseMotor3;
        public double vitesseMotor4;
        public double vitesseMotor5;
        public double vitesseMotor6;
        public double vitesseMotor7;
    }
    public class PIDDebugDataArgs : EventArgs
    {
        public uint timeStampMS;
        public double xErreur;
        public double yErreur;
        public double thetaErreur;
        public double xCorrection;
        public double yCorrection;
        public double thetaCorrection;

        public double xConsigneFromRobot;
        public double yConsigneFromRobot;
        public double thetaConsigneFromRobot;
    }
    public class PIDDataArgs : EventArgs
    {
        public double P_x;
        public double I_x;
        public double D_x;
        public double P_y;
        public double I_y;
        public double D_y;
        public double P_theta;
        public double I_theta;
        public double D_theta;
    }

    public class AccelEventArgs : EventArgs
    {
        public int timeStampMS;
        public double accelX;
        public double accelY;
        public double accelZ;
    }
    public class BoolEventArgs : EventArgs
    {
        public bool value { get; set; }
    }
    public class StringEventArgs : EventArgs
    {
        public string value { get; set; }
    }
    public class SpeedConsigneToMotorArgs : EventArgs
    {
        public float V { get; set; }
        public byte MotorNumber { get; set; }
    }
    public class PositionArgs : EventArgs
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public float Reliability { get; set; }
    }

    public class LocationArgs : EventArgs
    {
        public int RobotId { get; set; }

        public Location Location { get; set; }
    }
    public class LocationListArgs : EventArgs
    {
        public List<Location> LocationList { get; set; }
    }
    public class PerceptionArgs : EventArgs
    {
        public int RobotId { get; set; }
        public Perception Perception { get; set; }
    }
    public class HeatMapArgs : EventArgs
    {
        public int RobotId { get; set; }
        public Heatmap HeatMap { get; set; }
    }

    public class LocalWorldMapArgs : EventArgs
    {
        public int RobotId { get; set; }
        public int TeamId { get; set; }
        public LocalWorldMap LocalWorldMap { get; set; }
    }

    public class GlobalWorldMapArgs : EventArgs
    {
        public GlobalWorldMap GlobalWorldMap { get; set; }
    }

    [ProtoContract]
    [ProtoInclude(501, typeof(RawLidarArgsLog))]
    public class RawLidarArgs : EventArgs
    {
        [ProtoMember(1)]
        public int RobotId { get; set; }
        [ProtoMember(2)]
        public List<PolarPoint> PtList { get; set; }
    }

    public class PolarPointListExtendedListArgs : EventArgs
    {
        public int RobotId { get; set; }
        public List<PolarPointListExtended> ObjectList { get; set; }
    }

    [ProtoContract]
    public class RawLidarArgsLog : RawLidarArgs
    {
        [ProtoMember(1)]
        public string Type = "RawLidar";
        [ProtoMember(2)]
        public double InstantInMs;
    }

    [ProtoContract]
    public class SpeedDataEventArgsLog : SpeedDataEventArgs
    {
        [ProtoMember(1)]
        public string Type = "SpeedFromOdometry";
        [ProtoMember(2)]
        public double InstantInMs;
    }
    [ProtoContract]
    public class IMUDataEventArgsLog : IMUDataEventArgs
    {
        [ProtoMember(1)]
        public string Type = "ImuData";
        [ProtoMember(2)]
        public double InstantInMs;
    }

    //[ProtoContract]
    //public class OpenCvMatImageArgsLog : OpenCvMatImageArgs
    //{
    //    [ProtoMember(1)]
    //    public string Type = "CameraOmni";
    //    [ProtoMember(2)]
    //    public double InstantInMs;
    //}

    [ProtoContract]
    public class BitmapImageArgsLog : BitmapImageArgs
    {
        [ProtoMember(1)]
        public string Type = "CameraOmni";
        [ProtoMember(2)]
        public double InstantInMs;
    }
}

