﻿using AdvancedTimers;
using Constants;
using MessageDecoder;
using MessageEncoder;
using RobotInterface;
using SciChart.Charting.Visuals;
using System;
using System.Threading;
using TrajectoryGenerator;
using WayPointGenerator;
using WorldMapManager;
using PerceptionManagement;
using EventArgsLibrary;
using WpfReplayNavigator;
using System.Runtime.InteropServices;
using MessageGeneratorNS;
using LidaRxR2000NS;
using MessageProcessorNS;
using HerkulexManagerNS;
using Staudt.Engineering.LidaRx;
using Staudt.Engineering.LidaRx.Drivers.R2000;
using StrategyManagerEurobotNS;
using Utilities;
using System.Collections.Generic;
using UdpMulticastInterpreter;
using UDPMulticast;

namespace Robot
{

    //public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
    //// A delegate type to be used as the handler routine
    //// for SetConsoleCtrlHandler.
    //public delegate bool HandlerRoutine(CtrlTypes CtrlType);

    //// An enumerated type for the control messages
    //// sent to the handler routine.
    //public enum CtrlTypes
    //{
    //    CTRL_C_EVENT = 0,
    //    CTRL_BREAK_EVENT,
    //    CTRL_CLOSE_EVENT,
    //    CTRL_LOGOFF_EVENT = 5,
    //    CTRL_SHUTDOWN_EVENT
    //}

    //private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
    //{
    //    // Put your own handler here
    //    return true;
    //}

    //SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

    enum RobotMode
    {
        Acquisition,
        Replay,
        Standard,
        NoLidar,
        NoCamera
    }
    class Robot_Eurobot_2021
    {
        #region Gestion Arret Console (Do not Modify)
        // Declare the SetConsoleCtrlHandler function 
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here            
            t1.Abort();
            return true;
        }
        #endregion

        static GameMode competition = GameMode.Eurobot;

        static bool usingXBoxController;
        static bool usingLidar = true;        
        static bool usingRobotInterface = true;

        static HighFreqTimer timerStrategie;
        static USBVendor.USBVendor usbDriver;
        static MsgDecoder msgDecoder;
        static MsgEncoder msgEncoder;
        static MsgGenerator robotMsgGenerator;
        static MsgProcessor robotMsgProcessor;
        static TrajectoryPlanner trajectoryPlanner;
        static KalmanPositioning.KalmanPositioning kalmanPositioning;
        static LocalWorldMapManager localWorldMapManager;

        //Lien de transmission par socket
        static UDPMulticastSender robotUdpMulticastSender = null;
        static UDPMulticastReceiver robotUdpMulticastReceiver = null;
        static UDPMulticastInterpreter robotUdpMulticastInterpreter = null;

        static GlobalWorldMapManager globalWorldMapManager;
                
        static ImuProcessor.ImuProcessor imuProcessor;
        static StrategyManagerNS.StrategyManager strategyManager;

        static PerceptionManager perceptionManager;        
        static LidaRxR2000 lidar_OMD60M_TCP;
        static XBoxController.XBoxController xBoxManette;

        static HerkulexManager herkulexManager;

        static object ExitLock = new object();

        static WpfRobotInterface interfaceRobot;
        static LogRecorder.LogRecorder logRecorder;
        static LogReplay.LogReplay logReplay;
        static ReplayNavigator replayNavigator;


        [STAThread] //à ajouter au projet initial

        LidarStatusEvent methodeStatus()
        {
            LidarStatusEvent ev=null;

            return ev;
        }
        static void Main(string[] args)
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("RJWA77RbaJDdCRJpg4Iunl5Or6/FPX1xT+Gzu495Eaa0ZahxWi3jkNFDjUb/w70cHXyv7viRTjiNRrYqnqGA+Dc/yzIIzTJlf1s4DJvmQc8TCSrH7MBeQ2ON5lMs/vO0p6rBlkaG+wwnJk7cp4PbOKCfEQ4NsMb8cT9nckfdcWmaKdOQNhHsrw+y1oMR7rIH+rGes0jGGttRDhTOBxwUJK2rBA9Z9PDz2pGOkPjy9fwQ4YY2V4WPeeqM+6eYxnDZ068mnSCPbEnBxpwAldwXTyeWdXv8sn3Dikkwt3yqphQxvs0h6a8Dd6K/9UYni3o8pRkTed6SWodQwICcewfHTyGKQowz3afARj07et2h+becxowq3cRHL+76RyukbIXMfAqLYoT2UzDJNsZqcPPq/kxeXujuhT4SrNF3444MU1GaZZ205KYEMFlz7x/aEnjM6p3BuM6ZuO3Fjf0A0Ki/NBfS6n20E07CTGRtI6AsM2m59orPpI8+24GFlJ9xGTjoRA==");

            //On ajoute un gestionnaire d'évènement pour détecter la fermeture de l'application
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            //serialPort1 = new ReliableSerialPort(cfgSerialPort.CommName, cfgSerialPort.ComBaudrate, cfgSerialPort.Parity, cfgSerialPort.DataByte, cfgSerialPort.StopByte);
            //serialPort1 = new ReliableSerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            int teamId = (int)TeamId.Team1;
            int robotId = (int)RobotId.Robot1 + teamId;
            usbDriver = new USBVendor.USBVendor();
            msgDecoder = new MsgDecoder();
            msgEncoder = new MsgEncoder();
            robotMsgGenerator = new MsgGenerator();
            robotMsgProcessor = new MsgProcessor(robotId, competition);
                       
            imuProcessor = new ImuProcessor.ImuProcessor(robotId);
            if (usingLidar)
                lidar_OMD60M_TCP = new LidaRxR2000(50, R2000SamplingRate._72kHz);
            perceptionManager = new PerceptionManager(robotId, competition);
            kalmanPositioning = new KalmanPositioning.KalmanPositioning(robotId, 50, 0.2, 0.2, 0.2, 0.1, 0.1, 0.1, 0.02);
            trajectoryPlanner = new TrajectoryPlanner(robotId, competition);

            localWorldMapManager = new LocalWorldMapManager(robotId, teamId, bypassMulticast:false);
            globalWorldMapManager = new GlobalWorldMapManager(robotId);
            strategyManager = new StrategyManagerNS.StrategyManager(robotId, teamId, "224.16.32.79", competition);
            
            robotUdpMulticastSender = new UDPMulticastSender(robotId, "224.16.32.79");
            robotUdpMulticastReceiver = new UDPMulticastReceiver(robotId, "224.16.32.79");
            robotUdpMulticastInterpreter = new UDPMulticastInterpreter(robotId);

            herkulexManager = new HerkulexManager();
            herkulexManager.AddServo(ServoId.BrasCentral, HerkulexDescription.JOG_MODE.positionControlJOG);
            herkulexManager.AddServo(ServoId.BrasDroit, HerkulexDescription.JOG_MODE.positionControlJOG);
            herkulexManager.AddServo(ServoId.BrasGauche, HerkulexDescription.JOG_MODE.positionControlJOG);
            herkulexManager.AddServo(ServoId.PorteDrapeau, HerkulexDescription.JOG_MODE.positionControlJOG);
                                   
            xBoxManette = new XBoxController.XBoxController(robotId);
            
            //Démarrage des interface de visualisation
            if (usingRobotInterface)
                StartRobotInterface();

            //if (usingLogReplay)
            //    StartReplayNavigatorInterface();

            //Initialisation du logger
            logRecorder = new LogRecorder.LogRecorder();
            //Démarrage du log replay si l'interface est utilisée et existe ou si elle n'est pas utilisée, sinon on bloque
            logReplay = new LogReplay.LogReplay();
            
            //Liens entre modules
            //strategyManager.strategy.OnRefereeBoxCommandEvent += globalWorldMapManager.OnRefereeBoxCommandReceived;
            strategyManager.strategy.OnGameStateChangedEvent += trajectoryPlanner.OnGameStateChangeReceived;
            strategyManager.strategy.OnWaypointEvent += trajectoryPlanner.OnWaypointReceived;

            //Kalman
            perceptionManager.OnAbsolutePositionEvent += kalmanPositioning.OnAbsolutePositionCalculatedEvent;
            imuProcessor.OnGyroSpeedEvent += kalmanPositioning.OnGyroRobotSpeedReceived;
            robotMsgProcessor.OnSpeedPolarOdometryFromRobotEvent += kalmanPositioning.OnOdometryRobotSpeedReceived;
            kalmanPositioning.OnKalmanLocationEvent += trajectoryPlanner.OnPhysicalPositionReceived;
            kalmanPositioning.OnKalmanLocationEvent += perceptionManager.OnPhysicalRobotPositionReceived;
            kalmanPositioning.OnKalmanLocationEvent += strategyManager.strategy.OnPositionRobotReceived;

            //Update des données de la localWorldMap
            perceptionManager.OnPerceptionEvent += localWorldMapManager.OnPerceptionReceived;
            strategyManager.strategy.OnDestinationEvent += localWorldMapManager.OnDestinationReceived;
            strategyManager.strategy.OnRoleEvent += localWorldMapManager.OnRoleReceived; //Utile pour l'affichage
            strategyManager.strategy.OnMessageDisplayEvent += localWorldMapManager.OnMessageDisplayReceived; //Utile pour l'affichage
            strategyManager.strategy.OnHeatMapStrategyEvent += localWorldMapManager.OnHeatMapStrategyReceived;
            strategyManager.strategy.OnWaypointEvent += localWorldMapManager.OnWaypointReceived;
            strategyManager.strategy.OnHeatMapWayPointEvent += localWorldMapManager.OnHeatMapWaypointReceived;
            trajectoryPlanner.OnGhostLocationEvent += localWorldMapManager.OnGhostLocationReceived;


            //Gestion des events liés à une détection de collision soft
            trajectoryPlanner.OnCollisionEvent += kalmanPositioning.OnCollisionReceived;
            //trajectoryPlanner.OnSpeedConsigneEvent += robotMsgGenerator.GenerateMessageSetSpeedConsigneToRobot;


            if (usingLidar)
                strategyManager.strategy.OnMessageEvent += lidar_OMD60M_TCP.OnMessageReceivedEvent;


            strategyManager.strategy.OnSetRobotSpeedPolarPIDEvent += robotMsgGenerator.GenerateMessageSetupSpeedPolarPIDToRobot;
            strategyManager.strategy.OnSetRobotSpeedIndependantPIDEvent += robotMsgGenerator.GenerateMessageSetupSpeedIndependantPIDToRobot;
            strategyManager.strategy.OnSetAsservissementModeEvent += robotMsgGenerator.GenerateMessageSetAsservissementMode;
            //  strategyEurobot.OnHerkulexPositionRequestEvent += herkulexManager.OnHerkulexPositionRequestEvent;
            strategyManager.strategy.OnSetSpeedConsigneToMotor += robotMsgGenerator.GenerateMessageSetSpeedConsigneToMotor;
            strategyManager.strategy.OnEnableDisableMotorCurrentDataEvent += robotMsgGenerator.GenerateMessageEnableMotorCurrentData;
            herkulexManager.OnHerkulexSendToSerialEvent += robotMsgGenerator.GenerateMessageForwardHerkulex;

            
            if (usingLidar)
            {
                lidar_OMD60M_TCP.OnLidarDecodedFrameEvent += perceptionManager.OnRawLidarDataReceived;
                perceptionManager.OnLidarRawDataEvent += localWorldMapManager.OnRawLidarDataReceived;
                perceptionManager.OnLidarProcessedDataEvent += localWorldMapManager.OnProcessedLidarDataReceived;

            }

            //L'envoi des commandes dépend du fait qu'on soit en mode manette ou pas. 
            //Il faut donc enregistrer les évènement ou pas en fonction de l'activation
            //C'est fait plus bas dans le code avec la fonction que l'on appelle
            ConfigControlEvents(usingXBoxController);
            
            //Gestion des messages envoyé par le robot
            robotMsgGenerator.OnMessageToRobotGeneratedEvent += msgEncoder.EncodeMessageToRobot;
            //msgEncoder.OnMessageEncodedEvent += serialPort1.SendMessage;
            msgEncoder.OnMessageEncodedEvent += usbDriver.SendUSBMessage;

            //Gestion des messages reçu par le robot
            //serialPort1.OnDataReceivedEvent += msgDecoder.DecodeMsgReceived;
            usbDriver.OnUSBDataReceivedEvent += msgDecoder.DecodeMsgReceived;
            msgDecoder.OnMessageDecodedEvent += robotMsgProcessor.ProcessRobotDecodedMessage;
            robotMsgProcessor.OnIMURawDataFromRobotGeneratedEvent += imuProcessor.OnIMURawDataReceived;
            robotMsgProcessor.OnIOValuesFromRobotGeneratedEvent += strategyManager.strategy.OnIOValuesFromRobotEvent;
            robotMsgProcessor.OnIOValuesFromRobotGeneratedEvent += perceptionManager.OnIOValuesFromRobotEvent;


            //  robotMsgProcessor.OnMotorsCurrentsFromRobotGeneratedEvent += strategyManager.OnMotorCurrentReceive;


            //Le local Manager n'est là que pour assurer le stockage de ma local world map avant affichage et transmission des infos, il ne doit pas calculer quoique ce soit, 
            //c'est le perception manager qui le fait.
            trajectoryPlanner.OnPidSpeedResetEvent += robotMsgGenerator.GenerateMessageResetSpeedPid;

            ////Event d'interprétation d'une globalWorldMap à sa réception dans le robot
            robotUdpMulticastInterpreter.OnRefBoxMessageEvent += strategyManager.strategy.OnRefBoxMsgReceived;
            robotUdpMulticastInterpreter.OnGlobalWorldMapEvent += strategyManager.strategy.OnGlobalWorldMapReceived;
            robotUdpMulticastInterpreter.OnLocalWorldMapEvent += globalWorldMapManager.OnLocalWorldMapReceived;

            globalWorldMapManager.OnMulticastSendGlobalWorldMapEvent += robotUdpMulticastSender.OnMulticastMessageToSendReceived;

            ////Event de Transmission des Local World Map du robot vers le multicast
            localWorldMapManager.OnMulticastSendLocalWorldMapEvent += robotUdpMulticastSender.OnMulticastMessageToSendReceived;
            //Event de Réception de data Multicast sur le robot
            robotUdpMulticastReceiver.OnDataReceivedEvent += robotUdpMulticastInterpreter.OnMulticastDataReceived;
            
            /// LOGGER related events
            perceptionManager.OnLidarRawDataEvent += logRecorder.OnRawLidarDataReceived;
            robotMsgProcessor.OnIMURawDataFromRobotGeneratedEvent += logRecorder.OnIMURawDataReceived;
            robotMsgProcessor.OnSpeedPolarOdometryFromRobotEvent += logRecorder.OnPolarSpeedDataReceived;
                       
            //omniCamera.OpenCvMatImageEvent += logRecorder.OnOpenCVMatImageReceived;

            //strategyManagerDictionary.Add(robotId, strategyManager);
            trajectoryPlanner.InitRobotPosition(0, 0, 0);

            strategyManager.strategy.InitStrategy( robotId,  teamId);
            while (!exitSystem)
            {
                Thread.Sleep(500);
            }

        }

        
        static void ChangeUseOfXBoxController(object sender, BoolEventArgs e)
        {
            ConfigControlEvents(e.value);
        }

        private static void ConfigControlEvents(bool useXBoxController)
        {
            usingXBoxController = useXBoxController;
            if (usingXBoxController)
            {
                //xBoxManette.OnSpeedConsigneEvent += physicalSimulator.SetRobotSpeed;
                trajectoryPlanner.OnSpeedConsigneEvent -= robotMsgGenerator.GenerateMessageSetSpeedConsigneToRobot;
                xBoxManette.OnSpeedConsigneEvent += robotMsgGenerator.GenerateMessageSetSpeedConsigneToRobot;
                //if (interfaceRobot != null)
                //{
                //    xBoxManette.OnSpeedConsigneEvent += interfaceRobot.UpdateSpeedConsigneOnGraph;
                //}
                xBoxManette.OnPriseBalleEvent += robotMsgGenerator.GenerateMessageSetSpeedConsigneToMotor;
                xBoxManette.OnMoveTirUpEvent += robotMsgGenerator.GenerateMessageMoveTirUp;
                xBoxManette.OnMoveTirDownEvent += robotMsgGenerator.GenerateMessageMoveTirDown;
                xBoxManette.OnTirEvent += robotMsgGenerator.GenerateMessageTir;
                xBoxManette.OnStopEvent += robotMsgGenerator.GenerateMessageSTOP;

                //Gestion des events liés à une détection de collision soft
                trajectoryPlanner.OnCollisionEvent -= kalmanPositioning.OnCollisionReceived;
                strategyManager.strategy.OnCollisionEvent -= kalmanPositioning.OnCollisionReceived;
            }
            else
            {
                //On se desabonne aux evenements suivants:
                //xBoxManette.OnSpeedConsigneEvent -= physicalSimulator.SetRobotSpeed;
                trajectoryPlanner.OnSpeedConsigneEvent += robotMsgGenerator.GenerateMessageSetSpeedConsigneToRobot;
                xBoxManette.OnSpeedConsigneEvent -= robotMsgGenerator.GenerateMessageSetSpeedConsigneToRobot;
                
                xBoxManette.OnPriseBalleEvent -= robotMsgGenerator.GenerateMessageSetSpeedConsigneToMotor;
                xBoxManette.OnMoveTirUpEvent -= robotMsgGenerator.GenerateMessageMoveTirUp;
                xBoxManette.OnMoveTirDownEvent -= robotMsgGenerator.GenerateMessageMoveTirDown;
                xBoxManette.OnTirEvent -= robotMsgGenerator.GenerateMessageTir;
                xBoxManette.OnStopEvent -= robotMsgGenerator.GenerateMessageSTOP;

                //Gestion des events liés à une détection de collision soft
                trajectoryPlanner.OnCollisionEvent += kalmanPositioning.OnCollisionReceived;
                strategyManager.strategy.OnCollisionEvent += kalmanPositioning.OnCollisionReceived;
            }
        }


        /******************************************* Trap app termination ***************************************/
        static bool exitSystem = false;
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        enum CtrlType
        {
            CTRL_C_EVENT=0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 3,
            CTRL_SHUTDOWN_EVENT = 4
        }

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;
        //Gestion de la terminaison de l'application de manière propre
        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Existing on CTRL+C or process kill or shutdown...");

            //Nettoyage des process à faire ici
            //serialPort1.Close();

            Console.WriteLine("Nettoyage effectué");
            exitSystem = true;

            //Sortie
            Environment.Exit(-1);
            return true;
        }

        static Thread t1;
        static void StartRobotInterface()
        {
            t1 = new Thread(() =>
            {
                //Attention, il est nécessaire d'ajouter PresentationFramework, PresentationCore, WindowBase and your wpf window application aux ressources.
                interfaceRobot = new RobotInterface.WpfRobotInterface(competition);
                interfaceRobot.Loaded += RegisterRobotInterfaceEvents;
                interfaceRobot.ShowDialog();
            });
            t1.SetApartmentState(ApartmentState.STA);
            t1.Start();
        }

        static void RegisterRobotInterfaceEvents(object sender, EventArgs e)
        {
            //Sur evenement xx        -->>        Action a effectuer
            msgDecoder.OnMessageDecodedEvent += interfaceRobot.DisplayMessageDecoded;
            msgDecoder.OnMessageDecodedErrorEvent += interfaceRobot.DisplayMessageDecodedError;

            if (usingLidar)
            {
                perceptionManager.OnLidarRawDataEvent += interfaceRobot.OnRawLidarDataReceived;
                perceptionManager.OnLidarBalisePointListForDebugEvent += interfaceRobot.OnRawLidarBalisePointsReceived;
            }

            robotMsgGenerator.OnMessageToDisplaySpeedPolarPidSetupEvent += interfaceRobot.OnMessageToDisplayPolarSpeedPidSetupReceived;
            robotMsgGenerator.OnMessageToDisplaySpeedIndependantPidSetupEvent += interfaceRobot.OnMessageToDisplayIndependantSpeedPidSetupReceived;
            trajectoryPlanner.OnMessageToDisplayPositionPidSetupEvent += interfaceRobot.OnMessageToDisplayPositionPidSetupReceived;
            trajectoryPlanner.OnMessageToDisplayPositionPidCorrectionEvent += interfaceRobot.OnMessageToDisplayPositionPidCorrectionReceived;

            //On récupère les évènements de type refbox, qui sont ici des tests manuels dans le globalManager pour lancer à la main des actions ou stratégies
            //interfaceRobot.OnRefereeBoxCommandEvent +=  globalWorldMapManager.OnRefereeBoxCommandReceived;
            interfaceRobot.OnMulticastSendRefBoxCommandEvent += robotUdpMulticastSender.OnMulticastMessageToSendReceived;
            
            //REPLAY EVENTS
            logReplay.OnIMURawDataFromReplayGeneratedEvent += interfaceRobot.UpdateImuDataOnGraph;
            logReplay.OnSpeedPolarOdometryFromReplayEvent += interfaceRobot.UpdateSpeedPolarOdometryOnInterface;

            //REAL SENSOR PROCESSED EVENTS
            imuProcessor.OnIMUProcessedDataGeneratedEvent += interfaceRobot.UpdateImuDataOnGraph;

            robotMsgProcessor.OnMotorsCurrentsFromRobotGeneratedEvent += interfaceRobot.UpdateMotorsCurrentsOnGraph;
            robotMsgProcessor.OnEncoderRawDataFromRobotGeneratedEvent += interfaceRobot.UpdateMotorsEncRawDataOnGraph;

            robotMsgProcessor.OnEnableDisableMotorsACKFromRobotGeneratedEvent += interfaceRobot.ActualizeEnableDisableMotorsButton;
            robotMsgProcessor.OnEnableDisableTirACKFromRobotGeneratedEvent += interfaceRobot.ActualizeEnableDisableTirButton;

            robotMsgProcessor.OnAsservissementModeStatusFromRobotGeneratedEvent += interfaceRobot.UpdateAsservissementMode;
            robotMsgProcessor.OnSpeedPolarOdometryFromRobotEvent += interfaceRobot.UpdateSpeedPolarOdometryOnInterface;
            
            robotMsgProcessor.OnIndependantOdometrySpeedFromRobotEvent += interfaceRobot.UpdateSpeedIndependantOdometryOnInterface;
            robotMsgProcessor.OnSpeedPolarPidErrorCorrectionConsigneDataFromRobotGeneratedEvent += interfaceRobot.UpdateSpeedPolarPidErrorCorrectionConsigneDataOnGraph;
            robotMsgProcessor.OnSpeedIndependantPidErrorCorrectionConsigneDataFromRobotGeneratedEvent += interfaceRobot.UpdateSpeedIndependantPidErrorCorrectionConsigneDataOnGraph;
            robotMsgProcessor.OnSpeedPolarPidCorrectionDataFromRobotEvent += interfaceRobot.UpdateSpeedPolarPidCorrectionData;
            robotMsgProcessor.OnSpeedIndependantPidCorrectionDataFromRobotEvent += interfaceRobot.UpdateSpeedIndependantPidCorrectionData;

            robotMsgProcessor.OnErrorTextFromRobotGeneratedEvent += interfaceRobot.AppendConsole;
            robotMsgProcessor.OnPowerMonitoringValuesFromRobotGeneratedEvent += interfaceRobot.UpdatePowerMonitoringValues;
            robotMsgProcessor.OnEnableMotorCurrentACKFromRobotGeneratedEvent += interfaceRobot.ActualizeEnableMotorCurrentCheckBox;
            //robotMsgProcessor.OnEnableEncoderRawDataACKFromRobotGeneratedEvent += interfaceRobot.ActualizeEnableEncoderRawDataCheckBox;
            robotMsgProcessor.OnEnableAsservissementDebugDataACKFromRobotEvent += interfaceRobot.ActualizeEnableAsservissementDebugDataCheckBox;
            //robotMsgProcessor.OnEnableMotorSpeedConsigneDataACKFromRobotGeneratedEvent += interfaceRobot.ActualizEnableMotorSpeedConsigneCheckBox;
            robotMsgProcessor.OnEnablePowerMonitoringDataACKFromRobotGeneratedEvent += interfaceRobot.ActualizEnablePowerMonitoringCheckBox;

            robotMsgProcessor.OnMessageCounterEvent += interfaceRobot.MessageCounterReceived;


            robotMsgGenerator.OnSetSpeedConsigneToRobotReceivedEvent += interfaceRobot.UpdatePolarSpeedConsigneOnGraph; //Valable quelque soit la source des consignes vitesse
            interfaceRobot.OnEnableDisableMotorsFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnableDisableMotors;
            interfaceRobot.OnEnableDisableServosFromInterfaceGeneratedEvent += herkulexManager.OnEnableDisableServosRequestEvent;
            interfaceRobot.OnEnableDisableTirFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnableDisableTir;
            interfaceRobot.OnEnableDisableControlManetteFromInterfaceGeneratedEvent += ChangeUseOfXBoxController;
            interfaceRobot.OnEnableDisableLoggingEvent += logRecorder.OnEnableDisableLoggingReceived;
            interfaceRobot.OnSetAsservissementModeFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageSetAsservissementMode;
            interfaceRobot.OnEnableEncodersRawDataFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnableEncoderRawData;
            interfaceRobot.OnEnableMotorCurrentDataFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnableMotorCurrentData;
            interfaceRobot.OnEnableMotorsSpeedConsigneDataFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnableMotorSpeedConsigne;
            interfaceRobot.OnSetRobotPIDFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageSetupSpeedPolarPIDToRobot;
            interfaceRobot.OnEnableSpeedPIDEnableDebugInternalFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageSpeedPIDEnableDebugInternal;
            interfaceRobot.OnEnableSpeedPIDEnableDebugErrorCorrectionConsigneFromInterfaceEvent += robotMsgGenerator.GenerateMessageSpeedPIDEnableDebugErrorCorrectionConsigne;
            interfaceRobot.OnCalibrateGyroFromInterfaceGeneratedEvent += imuProcessor.OnCalibrateGyroFromInterfaceGeneratedEvent;
            interfaceRobot.OnEnablePowerMonitoringDataFromInterfaceGeneratedEvent += robotMsgGenerator.GenerateMessageEnablePowerMonitoring;

            //Activation désactivation du mode replay
            interfaceRobot.OnEnableDisableLogReplayEvent += InterfaceRobot_OnEnableDisableLogReplayEvent;
            interfaceRobot.OnEnableDisableLogReplayEvent += logReplay.OnEnableDisableLogReplayEvent;

            localWorldMapManager.OnLocalWorldMapForDisplayOnlyEvent += interfaceRobot.OnLocalWorldMapStrategyEvent;
            localWorldMapManager.OnLocalWorldMapForDisplayOnlyEvent += interfaceRobot.OnLocalWorldMapWayPointEvent;

            //strategyManager.Init();
        }
        
        private static void InterfaceRobot_OnEnableDisableLogReplayEvent(object sender, BoolEventArgs e)
        {
            /// Fonction lancée lors d'un appui sur Enable / Disable de l'interface
            /// On fait deux choses : 
            ///     On suspend le msgProcessor
            ///     On reroute les évènements Lidar - IMU - SpeedPolar

            if (e.value)
            {
                //On enable le Replay
                /// On fait sauter le lidar et l'USB entrant
                lidar_OMD60M_TCP.OnLidarDecodedFrameEvent -= perceptionManager.OnRawLidarDataReceived;
                usbDriver.OnUSBDataReceivedEvent -= msgDecoder.DecodeMsgReceived;

                logReplay.OnIMURawDataFromReplayGeneratedEvent += imuProcessor.OnIMURawDataReceived;
                logReplay.OnLidarEvent += perceptionManager.OnRawLidarDataReceived;
                logReplay.OnSpeedPolarOdometryFromReplayEvent += kalmanPositioning.OnOdometryRobotSpeedReceived;
            }
            else
            {
                //On disable le Replay
                /// On remet le lidar et l'USB entrant
                lidar_OMD60M_TCP.OnLidarDecodedFrameEvent += perceptionManager.OnRawLidarDataReceived;
                usbDriver.OnUSBDataReceivedEvent += msgDecoder.DecodeMsgReceived;

                logReplay.OnIMURawDataFromReplayGeneratedEvent -= imuProcessor.OnIMURawDataReceived;
                logReplay.OnLidarEvent -= perceptionManager.OnRawLidarDataReceived;
                logReplay.OnSpeedPolarOdometryFromReplayEvent -= kalmanPositioning.OnOdometryRobotSpeedReceived;
            }



        }



        //static Thread t3;
        //static void StartReplayNavigatorInterface()
        //{
        //    t3 = new Thread(() =>
        //    {
        //        //Attention, il est nécessaire d'ajouter PresentationFramework, PresentationCore, WindowBase and your wpf window application aux ressources.

        //        replayNavigator = new ReplayNavigator();
        //        replayNavigator.Loaded += RegisterReplayInterfaceEvents;
        //        replayNavigator.ShowDialog();

        //    });
        //    t3.SetApartmentState(ApartmentState.STA);
        //    t3.Start();
        //}

        //static void RegisterReplayInterfaceEvents(object sender, EventArgs e)
        //{
        //    //if (usingLogReplay)
        //    //{
        //    //    replayNavigator.OnPauseEvent += logReplay.PauseReplay;
        //    //    replayNavigator.OnPlayEvent += logReplay.StartReplay;
        //    //    replayNavigator.OnLoopEvent += logReplay.LoopReplayChanged;
        //    //    logReplay.OnUpdateFileNameEvent += replayNavigator.UpdateFileName;
        //    //    replayNavigator.OnNextEvent += logReplay.NextReplay;
        //    //    replayNavigator.OnPrevEvent += logReplay.PreviousReplay;
        //    //    replayNavigator.OnRepeatEvent += logReplay.RepeatReplayChanged;
        //    //    replayNavigator.OnOpenFileEvent += logReplay.OpenReplayFile;
        //    //    replayNavigator.OnOpenFolderEvent += logReplay.OpenReplayFolder;
        //    //    replayNavigator.OnSpeedChangeEvent += logReplay.ReplaySpeedChanged;
        //    //}

        //    //imageProcessingPositionFromOmniCamera.OnOpenCvMatImageProcessedEvent += ConsoleCamera.DisplayOpenCvMatImage;
        //}
    }

}
