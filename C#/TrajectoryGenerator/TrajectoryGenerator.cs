﻿using AdvancedTimers;
using EventArgsLibrary;
using PerceptionManagement;
using System;
using Utilities;
using WorldMap;

namespace TrajectoryGenerator
{
    public class TrajectoryPlanner
    {
        int robotId = 0;

        Location currentLocation;
        Location wayPointLocation;
        Location ghostLocation;

        GameState currentGameState = GameState.STOPPED;

        double FreqEch = 30.0;

        double accelLineaireMax = 2; //en m.s-2
        double accelRotationCapVitesseMax = 5; //en rad.s-2
        double accelRotationOrientationRobotMax = 5; //en rad.s-2
                
        double vitesseLineaireMax = 1; //en m.s-1
        double vitesseRotationCapVitesseMax = 10; //en rad.s-1
        double vitesseRotationOrientationRobotMax = 10; //en rad.s-1

        double capVitesseRefTerrain = 0;
        double vitesseRotationCapVitesse = 0;
        



        public TrajectoryPlanner(int id)
        {
            robotId = id;
            InitPositionPID();
        }

        public void InitRobotPosition(double x, double y, double theta)
        {
            currentLocation = new Location(x, y, theta, 0, 0, 0);
            wayPointLocation = new Location(x, y, theta, 0, 0, 0);
            ghostLocation = new Location(x, y, theta, 0, 0, 0);
        }
        
        //Input Events
        public void OnWaypointReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            wayPointLocation = e.Location;
        }

        public void OnPhysicalPositionReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            if (robotId == e.RobotId)
            {
                currentLocation = e.Location;
                CalculateGhostPosition();
                PIDPosition();
                //CalculateSpeedOrders();
            }
        }

        public void OnGameStateChangeReceived(object sender, EventArgsLibrary.GameStateArgs e)
        {
            if (e.RobotId == robotId)
            {
                currentGameState = e.gameState;
            }
        }

        void CalculateGhostPosition()
        {
            if (wayPointLocation == null)
                return;
            if (ghostLocation == null)
                return;

            /************************* Début du calcul préliminaire des infos utilisées ensuite ****************************/
            
            //Calcul du cap du Waypoint dans les référentiel terrain et robot
            double CapWayPointRefTerrain;
            if (wayPointLocation.X - ghostLocation.X != 0)
                CapWayPointRefTerrain = Math.Atan2(wayPointLocation.Y - ghostLocation.Y, wayPointLocation.X - ghostLocation.X);
            else
                CapWayPointRefTerrain = Math.Atan2(wayPointLocation.Y - ghostLocation.Y, 0.0001);

            //double CapWayPointRefRobot = CapWayPointRefTerrain - ghostLocation.Theta;
            CapWayPointRefTerrain = Toolbox.ModuloByAngle(capVitesseRefTerrain, CapWayPointRefTerrain);

            //Calcul de l'écart de cap
            double ecartCapVitesse = CapWayPointRefTerrain - capVitesseRefTerrain;

            ghostLocation.Theta = Toolbox.ModuloByAngle(wayPointLocation.Theta, ghostLocation.Theta);
            double ecartOrientationRobot = wayPointLocation.Theta - ghostLocation.Theta;
            
            //Calcul de la distance au WayPoint
            double distanceWayPoint = Math.Sqrt(Math.Pow(wayPointLocation.Y - ghostLocation.Y, 2) + Math.Pow(wayPointLocation.X - ghostLocation.X, 2));
            if (distanceWayPoint < 0.05)
                distanceWayPoint = 0;

            //Calcul de la vitesse linéaire du robot
            double vitesseLineaireRobot = Math.Sqrt(Math.Pow(ghostLocation.Vx, 2) + Math.Pow(ghostLocation.Vy, 2));

            //Calacul de la distance de freinage 
            double distanceFreinageLineaire = Math.Pow(vitesseLineaireRobot, 2) / (2 * accelLineaireMax);


            /* Fin du calcul des variéables intermédiaires */

            /************************ Ajustement de la vitesse linéaire du robot *******************************/
            // Si le robot a un cap vitesse à peu près aligné sur son Waypoint ou une vitesse presque nulle 
            // et que la distance au Waypoint est supérieure à la distance de freinage : on accélère en linéaire
            // sinon on freine
            double nouvelleVitesseLineaire;
            if (Math.Abs(ecartCapVitesse) < Math.PI / 2) //Le WayPoint est devant
            {
                if (distanceWayPoint > distanceFreinageLineaire)
                    nouvelleVitesseLineaire = Math.Min(vitesseLineaireMax, vitesseLineaireRobot + accelLineaireMax / FreqEch); //On accélère
                else
                    //On détermine la valeur du freinage en fonction des conditions
                    nouvelleVitesseLineaire = Math.Max(0, vitesseLineaireRobot - accelLineaireMax / FreqEch); //On freine
            }
            else //Le WayPoint est derrière
            {
                if (distanceWayPoint > distanceFreinageLineaire)
                    nouvelleVitesseLineaire = Math.Max(-vitesseLineaireMax, vitesseLineaireRobot - accelLineaireMax / FreqEch); //On accélère
                else
                    //On détermine la valeur du freinage en fonction des conditions
                    nouvelleVitesseLineaire = Math.Min(0, vitesseLineaireRobot + accelLineaireMax / FreqEch); //On freine
            }

            double ecartCapModuloPi = Toolbox.ModuloPiAngleRadian(ecartCapVitesse);

            /************************ Rotation du vecteur vitesse linéaire du robot *******************************/
            //Si le robot a un écart de cap vitesse supérieur à l'angle de freinage en rotation de cap vitesse, on accélère la rotation, sinon on freine
            double angleArretRotationCapVitesse = Math.Pow(vitesseRotationCapVitesse, 2) / (2 * accelRotationCapVitesseMax);
            if (ecartCapModuloPi > 0)
            {
                if (ecartCapModuloPi > angleArretRotationCapVitesse)
                    vitesseRotationCapVitesse = Math.Min(vitesseRotationCapVitesseMax, vitesseRotationCapVitesse + accelRotationCapVitesseMax / FreqEch); //on accélère
                else
                    vitesseRotationCapVitesse = Math.Max(0, vitesseRotationCapVitesse - accelRotationCapVitesseMax / FreqEch); //On freine
            }
            else
            {
                if (ecartCapModuloPi < -angleArretRotationCapVitesse)
                    vitesseRotationCapVitesse = Math.Max(-vitesseRotationCapVitesseMax, vitesseRotationCapVitesse - accelRotationCapVitesseMax / FreqEch); //On accélère en négatif
                else
                    vitesseRotationCapVitesse = Math.Min(0, vitesseRotationCapVitesse + accelRotationCapVitesseMax / FreqEch); //On freine en négatif
            }

            //On regarde si la vitesse linéaire est élevée ou pas. 
            //Si c'est le cas, on update le cap vitesse normalement en rampe
            //Sinon, on set le capvitesse à la valeur du cap WayPoint directement
            if (vitesseLineaireRobot > 0.5)
                capVitesseRefTerrain += vitesseRotationCapVitesse / FreqEch;
            else
            {
                capVitesseRefTerrain = CapWayPointRefTerrain; //Si la vitesse linéaire est faible, on tourne instantanément
                vitesseRotationCapVitesse = 0;
            }

            //On regarde si la vitesse linéaire est négative, on la repasse en positif en ajoutant PI au cap Vitesse
            if (nouvelleVitesseLineaire < 0)
            {
                nouvelleVitesseLineaire = -nouvelleVitesseLineaire;
                capVitesseRefTerrain += Math.PI;
                capVitesseRefTerrain = Toolbox.Modulo2PiAngleRad(capVitesseRefTerrain);
            }

            /************************ Orientation angulaire du robot *******************************/
            double angleArretRotationOrientationRobot = Math.Pow(ghostLocation.Vtheta, 2) / (2 * accelRotationOrientationRobotMax);
            double nouvelleVitesseRotationOrientationRobot = 0;
            if (ecartOrientationRobot > 0)
            {
                if (ecartOrientationRobot > angleArretRotationOrientationRobot)
                    nouvelleVitesseRotationOrientationRobot = Math.Min(vitesseRotationOrientationRobotMax, ghostLocation.Vtheta + accelRotationOrientationRobotMax / FreqEch); //on accélère
                else
                    nouvelleVitesseRotationOrientationRobot = Math.Max(0, ghostLocation.Vtheta - accelRotationOrientationRobotMax / FreqEch); //On freine
            }
            else
            {
                if (ecartOrientationRobot < -angleArretRotationOrientationRobot)
                    nouvelleVitesseRotationOrientationRobot = Math.Max(-vitesseRotationOrientationRobotMax, ghostLocation.Vtheta - accelRotationOrientationRobotMax / FreqEch); //On accélère en négatif
                else
                    nouvelleVitesseRotationOrientationRobot = Math.Min(0, ghostLocation.Vtheta + accelRotationOrientationRobotMax / FreqEch); //On freine en négatif
            }
            ////On traite à présent l'orientation angulaire du robot pour l'aligner sur l'angle demandé
            ////wayPointLocation.Theta = Toolbox.ModuloByAngle(ghostLocation.Theta, wayPointLocation.Theta);
            ////double ecartOrientationRobot = CapWayPointRefRobot - ghostLocation.Theta;
            ////double nouvelleVitesseAngulaire = 100.0 * CapWayPointRefRobot / FreqEch;


            /************************ Gestion des ordres d'arrêt global des robots *******************************/
            if (currentGameState != GameState.STOPPED)
            {
                //On génère les vitesses dans le référentiel du robot.
                ghostLocation.Vx = nouvelleVitesseLineaire * Math.Cos(capVitesseRefTerrain);
                ghostLocation.Vy = nouvelleVitesseLineaire * Math.Sin(capVitesseRefTerrain);
                ghostLocation.Vtheta = nouvelleVitesseRotationOrientationRobot;

                //Nouvelle orientation du robot
                //ghostLocation.Vtheta = 50* (wayPointLocation.Theta - ghostLocation.Theta)/FreqEch;

                ghostLocation.X += ghostLocation.Vx / FreqEch;
                ghostLocation.Y += ghostLocation.Vy / FreqEch;
                ghostLocation.Theta += ghostLocation.Vtheta / FreqEch;
                //ghostLocation.Theta = Toolbox.ModuloByAngle(nouveauCapRobot, ghostLocation.Theta);
            }
            else
            {
                //Si on est à l'arrêt, on ne change rien
            }

            OnGhostLocation(robotId, ghostLocation);

        }

        AsservissementPID PID_X;
        AsservissementPID PID_Y;
        AsservissementPID PID_Theta;
        void InitPositionPID()
        {
            PID_X  = new AsservissementPID(FreqEch, 100.0, 10, 0, 50, 50, 5);
            PID_Y = new AsservissementPID(FreqEch, 100.0, 10, 0, 50, 50, 5);
            PID_Theta = new AsservissementPID(FreqEch, 100, 0, 0, 50, 5, 5);
        }

        void PIDPosition()
        {
            double erreurXRefTerrain = ghostLocation.X - currentLocation.X;
            double erreurYRefTerrain = ghostLocation.Y - currentLocation.Y;
            currentLocation.Theta = Toolbox.ModuloByAngle(ghostLocation.Theta, currentLocation.Theta);
            double erreurTheta = ghostLocation.Theta - currentLocation.Theta;

            //Changement de repère car les asservissements se font dans le référentiel du robot
            double erreurXRefRobot = erreurXRefTerrain * Math.Cos(currentLocation.Theta) + erreurYRefTerrain * Math.Sin(currentLocation.Theta);
            double erreurYRefRobot = -erreurXRefTerrain * Math.Sin(currentLocation.Theta) + erreurYRefTerrain * Math.Cos(currentLocation.Theta);

            double vxRefRobot = PID_X.CalculatePIDoutput(erreurXRefRobot);
            double vyRefRobot = PID_Y.CalculatePIDoutput(erreurYRefRobot);
            double vtheta = PID_Theta.CalculatePIDoutput(erreurTheta);            

            OnSpeedConsigneToRobot(robotId, (float)vxRefRobot, (float)vyRefRobot, (float)vtheta);
        }
        
        //Output events
        public event EventHandler<SpeedConsigneArgs> OnSpeedConsigneEvent;
        public virtual void OnSpeedConsigneToRobot(int id, float vx, float vy, float vtheta)
        {
            var handler = OnSpeedConsigneEvent;
            if (handler != null)
            {
                handler(this, new SpeedConsigneArgs { RobotId = id, Vx = vx, Vy = vy, Vtheta = vtheta });
            }
        }


        public event EventHandler<LocationArgs> OnGhostLocationEvent;
        public virtual void OnGhostLocation(int id, Location loc)
        {
            var handler = OnGhostLocationEvent;
            if (handler != null)
            {
                handler(this, new LocationArgs { RobotId = id, Location=loc});
            }
        }
    }
}
