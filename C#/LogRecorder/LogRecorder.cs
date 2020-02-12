using EventArgsLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;


namespace LogRecorder
{
    public class LogRecorder
    {
        private Thread logThread;
        private StreamWriter sw;
        private Queue<string> logQueue = new Queue<string>();
        private Queue<Object> logQueueT = new Queue<Object>();
        public string logLock = "";
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        DateTime initialDateTime;
        // *** you need some mechanism to map types to fields
        static readonly IDictionary<int, Type> typeLookup = new Dictionary<int, Type>
        {
            {1, typeof(RawLidarArgsLog)}, {2, typeof(SpeedDataEventArgsLog)}, {3, typeof(IMUDataEventArgsLog)}, {4, typeof(OpenCvMatImageArgsLog)}
        };

        public LogRecorder()
        {
            logThread = new Thread(LogLoop);
            logThread.IsBackground = true;
            logThread.Name = "Logging Thread";
            logThread.Start();
            initialDateTime = DateTime.Now;
        }

        //private void LogLoop()
        //{
        //    string currentFileName = "logFilePath_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".rbt";
        //    sw = new StreamWriter(currentFileName, true);
        //    sw.AutoFlush = true;
        //    while (true)
        //    {
        //        while (logQueue.Count > 0)
        //        {
        //            string s = "";
        //            lock (logLock) // get a lock on the queue
        //            {
        //                s = logQueue.Dequeue();
        //            }
        //            sw.WriteLine(s);

        //            //Vérification de la taille du fichier
        //            if (sw.BaseStream.Length > 90 * 1000000)
        //            {
        //                //On split le fichier
        //                sw.Close();
        //                currentFileName = "logFilePath_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".rbt";
        //                sw = new StreamWriter(currentFileName, true);
        //            }
        //        }
        //        Thread.Sleep(10);
        //    }
        //}

        private void LogLoop()
        {
            string currentFileName = "logFilePath_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".rbt";
            sw = new StreamWriter(currentFileName, true);
            sw.AutoFlush = true;
            while (true)
            {
                while (logQueueT.Count > 0)
                {
                    object s;
                    lock (logLock) // get a lock on the queue
                    {
                        s = logQueueT.Dequeue();
                    }
                    WriteNext(sw.BaseStream, s);
                    //sw.WriteLine(s);

                    //Vérification de la taille du fichier
                    if (sw.BaseStream.Length > 90 * 1000000)
                    {
                        //On split le fichier
                        sw.Close();
                        currentFileName = "logFilePath_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".rbt";
                        sw = new StreamWriter(currentFileName, true);
                    }
                }
                Thread.Sleep(10);
            }
        }

        public void Log(string contents)
        {
            lock (logLock) // get a lock on the queue
            {
                logQueue.Enqueue(contents);
            }
        }

        public void Log<T>(T value)
        {
            lock(logLock)
            {
                logQueueT.Enqueue(value);
            }
        }

        public void OnRawLidarDataReceived(object sender, RawLidarArgs e)
        {
            RawLidarArgsLog data = new RawLidarArgsLog();
            data.PtList = e.PtList;
            data.RobotId = e.RobotId;
            data.InstantInMs = DateTime.Now.Subtract(initialDateTime).TotalMilliseconds;

            //Methode 1
            //string json = JsonConvert.SerializeObject(data);
            //Log(json);
            //Methode 2
            Log(data);
        }

        public void OnIMUDataReceived(object sender, IMUDataEventArgs e)
        {
            IMUDataEventArgsLog data = new IMUDataEventArgsLog();
            data.accelX = e.accelX;
            data.accelY = e.accelY;
            data.accelZ = e.accelZ;
            data.gyrX = e.gyrX;
            data.gyrY = e.gyrY;
            data.gyrZ = e.gyrZ;
            data.magX = e.magX;
            data.magY = e.magY;
            data.magZ = e.magZ;
            data.EmbeddedTimeStampInMs = e.EmbeddedTimeStampInMs;
            data.InstantInMs = DateTime.Now.Subtract(initialDateTime).TotalMilliseconds;
            //Methode 1
            //string json = JsonConvert.SerializeObject(data);
            //Log(json);
            //Methode 2
            Log(data);
        }

        public void OnSpeedDataReceived(object sender, SpeedDataEventArgs e)
        {
            SpeedDataEventArgsLog data = new SpeedDataEventArgsLog();
            data.Vx = e.Vx;
            data.Vy = e.Vy;
            data.Vtheta = e.Vtheta;
            data.RobotId = e.RobotId;
            data.EmbeddedTimeStampInMs = e.EmbeddedTimeStampInMs;
            data.InstantInMs = DateTime.Now.Subtract(initialDateTime).TotalMilliseconds;
            //string json = JsonConvert.SerializeObject(data);
            //Log(json);
            //Methode 2
            Log(data);
        }

        public void OnOpenCVMatImageReceived(object sender, OpenCvMatImageArgs e)
        {
            OpenCvMatImageArgsLog data = new OpenCvMatImageArgsLog();
            data.Mat = e.Mat;
            data.Descriptor = e.Descriptor;
            data.InstantInMs = DateTime.Now.Subtract(initialDateTime).TotalMilliseconds;
            //string json = JsonConvert.SerializeObject(data);
            //Log(json);
            //Methode 2
            Log(data);
        }


        static void WriteNext<T>(Stream stream, T value)
        {
            LogHeader header = new LogHeader()
            {
                Guid = Guid.NewGuid(),
                Type = typeof(T)
            };
            Serializer.SerializeWithLengthPrefix<LogHeader>(stream, header, PrefixStyle.Base128);
            Serializer.SerializeWithLengthPrefix<T>(stream, value, PrefixStyle.Base128);
        }
    }

    [ProtoContract]
    public class LogHeader
    {
        public LogHeader() { }

        [ProtoMember(1, IsRequired = true)]
        public Guid Guid { get; set; }

        [ProtoIgnore]
        public Type Type { get; set; }
        [ProtoMember(2, IsRequired = true)]
        public string TypeName
        {
            get { return this.Type.FullName; }
            set { this.Type = Type.GetType(value); }
        }
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
    [ProtoContract]
    public class OpenCvMatImageArgsLog : OpenCvMatImageArgs
    {
        [ProtoMember(1)]
        public string Type = "CameraOmni";
        [ProtoMember(2)]
        public double InstantInMs;
    }
}
