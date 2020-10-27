﻿using EventArgsLibrary;
using HerkulexManagerNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManager
{
    public class TaskBrasDroit
    {
        Thread TaskThread;
        TaskBrasState state = TaskBrasState.Attente;
        public bool isFineshed = false;
        enum TaskBrasState
        {
            Init,
            Attente,         
            PrehensionGobelet,
            StockageSurSupport,
            StockageEnHauteur,
            Depose,
            Finished
        }

        enum TaskBrasPositionsInit
        {
            BrasDroit = 234,
        }
        enum TaskBrasPositionsPrehensionGobelet
        {
            BrasDroit = 477,
        }
        enum TaskBrasPositionsStockageSurSupport
        {
            BrasDroit = 93,
        }
        enum TaskBrasPositionsStockageEnHauteur
        {
            BrasDroit = 309,
        }
        enum TaskBrasPositionsDepose
        {
            BrasDroit = 477,
        }

        public TaskBrasDroit()
        {
            TaskThread = new Thread(TaskThreadProcess);
            TaskThread.IsBackground = true;
            TaskThread.Start();
        }


        Dictionary<ServoId, int> servoPositionsRequested = new Dictionary<ServoId, int>();
        public bool isSupportDroitFull { get; private set; } = false;
        private bool isRunning = false;
        private double ventouseBrasDroitCurrent = -1;
        
        public void Init()
        {
            state = TaskBrasState.Init;
            OnPilotageVentouse((byte)PilotageVentouse.BrasDroit, 0);
            isSupportDroitFull = false;
            isFineshed = false;
        }
        
        public void StartPrehension()
        {
            state = TaskBrasState.PrehensionGobelet;
            isFineshed = false;
        }

        public void StartDepose()
        {
            state = TaskBrasState.Depose;
            isFineshed = false;
        }

        void TaskThreadProcess()
        {
            while(true)
            {
                switch (state)
                {
                    case TaskBrasState.Init:
                        servoPositionsRequested = new Dictionary<ServoId, int>();
                        servoPositionsRequested.Add(ServoId.BrasDroit, (int)TaskBrasPositionsInit.BrasDroit);
                        OnHerkulexPositionRequest(servoPositionsRequested);
                        state = TaskBrasState.Attente;
                        break;
                    case TaskBrasState.Attente:
                        //On ne sort pas de cet état sans un forçage extérieur vers un autree état
                        break;
                    case TaskBrasState.PrehensionGobelet:
                        servoPositionsRequested = new Dictionary<ServoId, int>();
                        OnPilotageVentouse((byte)PilotageVentouse.BrasDroit, 50);
                        servoPositionsRequested.Add(ServoId.BrasDroit, (int)TaskBrasPositionsPrehensionGobelet.BrasDroit);
                        OnHerkulexPositionRequest(servoPositionsRequested);
                        Thread.Sleep(500);
                        if(ventouseBrasDroitCurrent > 1.0)//mesuré 0.6A à vide et 1.35A en charge
                        {
                            if(!isSupportDroitFull)
                            {
                                //state = TaskBrasState.StockageSurSupport;
                                state = TaskBrasState.StockageEnHauteur;
                            }
                            else
                            {
                                state = TaskBrasState.StockageEnHauteur;
                                //state = TaskBrasState.StockageEnHauteur;
                            }
                        }
                        else
                        {
                            state = TaskBrasState.PrehensionGobelet;
                        }
                        break;
                    case TaskBrasState.StockageSurSupport:
                        servoPositionsRequested = new Dictionary<ServoId, int>();
                        servoPositionsRequested.Add(ServoId.BrasDroit, (int)TaskBrasPositionsStockageSurSupport.BrasDroit);
                        OnHerkulexPositionRequest(servoPositionsRequested);
                        Thread.Sleep(500);
                        OnPilotageVentouse((byte)PilotageVentouse.BrasDroit, 0);
                        Thread.Sleep(500);
                        isSupportDroitFull = true;
                        state = TaskBrasState.PrehensionGobelet;
                        break;
                    case TaskBrasState.StockageEnHauteur:
                        servoPositionsRequested = new Dictionary<ServoId, int>();
                        servoPositionsRequested.Add(ServoId.BrasDroit, (int)TaskBrasPositionsStockageEnHauteur.BrasDroit);
                        OnHerkulexPositionRequest(servoPositionsRequested);
                        Thread.Sleep(500);
                        state = TaskBrasState.Finished;
                        break;
                    case TaskBrasState.Depose:
                        servoPositionsRequested = new Dictionary<ServoId, int>();
                        servoPositionsRequested.Add(ServoId.BrasDroit, (int)TaskBrasPositionsDepose.BrasDroit);
                        OnHerkulexPositionRequest(servoPositionsRequested);
                        Thread.Sleep(500);
                        state = TaskBrasState.Finished;
                        break;
                    case TaskBrasState.Finished:
                        isFineshed = true;
                        break;
                    default:
                        break;
                }
                Thread.Sleep(100);
            }
        }

        public event EventHandler<HerkulexPositionsArgs> OnHerkulexPositionRequestEvent;
        public virtual void OnHerkulexPositionRequest(Dictionary<ServoId, int> positionDictionary)
        {
            OnHerkulexPositionRequestEvent?.Invoke(this, new HerkulexPositionsArgs { servoPositions = positionDictionary });
        }

        public event EventHandler<SpeedConsigneToMotorArgs> OnPilotageVentouseEvent;
        public virtual void OnPilotageVentouse(byte motorNumber, double vitesse)
        {
            OnPilotageVentouseEvent?.Invoke(this, new SpeedConsigneToMotorArgs { MotorNumber = motorNumber, V = vitesse });
        }

        public void OnMotorCurrentReceive(object sender, MotorsCurrentsEventArgs e)
        {
            //Motor 7 is bras central pump
            ventouseBrasDroitCurrent = e.motor6;
        }
    }
}
