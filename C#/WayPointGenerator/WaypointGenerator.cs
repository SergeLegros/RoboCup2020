﻿using AdvancedTimers;
using Constants;
using EventArgsLibrary;
using HeatMap;
using PerceptionManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Utilities;
using WorldMap;

namespace WayPointGenerator
{
    public class WaypointGenerator
    {
        int robotId;

        //Timer timerWayPointGeneration;

        Location destinationLocation;
        GlobalWorldMap globalWorldMap;
        //double[,] strategyManagerHeatMap = new double[0, 0];

        //double heatMapCellsize = 2; //doit être la même que celle du strategy manager
        //double fieldLength = 22;
        //double fieldHeight = 14;

        Heatmap StrategyHeatmap; 

        public WaypointGenerator(int id, string competition)
        {
            robotId = id;
            switch(competition)
            {
                case "RoboCup":
                    waypointHeatMap = new Heatmap(22.0, 14.0, (int)Math.Pow(2, 8), 2);
                    break;
                case "Eurobot":
                    waypointHeatMap = new Heatmap(3.0, 2.0, (int)Math.Pow(2, 5), 1);
                    break;
                default:
                    waypointHeatMap = new Heatmap(22.0, 14.0, (int)Math.Pow(2, 8), 2);
                    break;
            }
        }

        public void SetNextWayPoint(Location waypointLocation)
        {
            OnWaypoint(robotId, waypointLocation);
        }

        public void OnDestinationReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            if (e.RobotId == robotId)
            {
                destinationLocation = e.Location;
            }
        }

        public void OnStrategyHeatMapReceived(object sender, EventArgsLibrary.HeatMapArgs e)
        {            
            if (robotId == e.RobotId)
            {
                StrategyHeatmap = e.HeatMap;
                CalculateOptimalWayPoint();
            }
        }
        
        public void OnGlobalWorldMapReceived(object sender, GlobalWorldMapArgs e)
        {
            globalWorldMap = e.GlobalWorldMap;
        }

        Stopwatch sw = new Stopwatch();
        Heatmap waypointHeatMap;
        private void CalculateOptimalWayPoint()
        {
            //Heatmap StrategyHeatmap = StrategyHeatmap.Copy();

            //Génération de la HeatMap
            //Heatmap heatMap = new Heatmap(22, 14, 0.01);    
            sw.Reset();
            sw.Start(); // début de la mesure

            //Génération de la HeatMap
            waypointHeatMap.ReInitHeatMapData();
            int[] nbComputationsList = new int[waypointHeatMap.nbIterations];

            //On construit le heatMap en mode multi-résolution :
            //On commence par une heatmap très peu précise, puis on construit une heat map de taille réduite plus précise autour du point chaud,
            //Puis on construit une heatmap très précise au cm autour du point chaud.
            double optimizedAreaSize;

            PointD OptimalPosition = new PointD(0, 0);
            PointD OptimalPosInBaseHeatMapCoordinates = waypointHeatMap.GetBaseHeatMapPosFromFieldCoordinates(0, 0);

            for (int n = 0; n < waypointHeatMap.nbIterations; n++)
            {
                double subSamplingRate = waypointHeatMap.SubSamplingRateList[n];
                if (n >= 1)
                    optimizedAreaSize = waypointHeatMap.nbCellInSubSampledHeatMapWidthList[n] / waypointHeatMap.nbCellInSubSampledHeatMapWidthList[n - 1];
                else
                    optimizedAreaSize = waypointHeatMap.nbCellInSubSampledHeatMapWidthList[n];

                optimizedAreaSize /= 2;

                double minY = Math.Max(OptimalPosInBaseHeatMapCoordinates.Y / subSamplingRate - optimizedAreaSize, 0);
                double maxY = Math.Min(OptimalPosInBaseHeatMapCoordinates.Y / subSamplingRate + optimizedAreaSize, Math.Min(waypointHeatMap.nbCellInSubSampledHeatMapHeightList[n], waypointHeatMap.nbCellInBaseHeatMapHeight));
                double minX = Math.Max(OptimalPosInBaseHeatMapCoordinates.X / subSamplingRate - optimizedAreaSize, 0);
                double maxX = Math.Min(OptimalPosInBaseHeatMapCoordinates.X / subSamplingRate + optimizedAreaSize, Math.Min(waypointHeatMap.nbCellInSubSampledHeatMapWidthList[n], waypointHeatMap.nbCellInBaseHeatMapWidth));

                double max = double.NegativeInfinity;
                int maxXpos = 0;
                int maxYpos = 0;

                //Parallel.For((int)minY, (int)maxY+1, (y) =>
                for (double y = (int)minY; y < (int)maxY + 1; y += 1)
                {
                    //Parallel.For((int)minX, (int)maxX+1, (x) =>
                    for (double x = (int)minX; x < (int)maxX + 1; x += 1)
                    {
                        //Attention, le remplissage de la HeatMap se fait avec une inversion des coordonnées
                        //double value = Math.Max(0, 1 - Toolbox.Distance(theoreticalOptimalPos, heatMap.GetFieldPosFromSubSampledHeatMapCoordinates(x, y)) / 20.0);
                        var heatMapPos = waypointHeatMap.GetFieldPosFromSubSampledHeatMapCoordinates(x, y, n);
                        double pen = CalculPenalisation(heatMapPos);
                        //double value = EvaluateStrategyCostFunction(robotRole, heatMapPos);
                        //heatMap.SubSampledHeatMapData1[y, x] = value;
                        int yBase = (int)(y * subSamplingRate);
                        int xBase = (int)(x * subSamplingRate);
                        double value = StrategyHeatmap.BaseHeatMapData[yBase, xBase] - pen;
                        waypointHeatMap.BaseHeatMapData[yBase, xBase] = value;
                        nbComputationsList[n]++;

                        if (value > max)
                        {
                            max = value;
                            maxXpos = xBase;
                            maxYpos = yBase;
                        }

                        //Code ci-dessous utile si on veut afficher la heatmap complete(video), mais consommateur en temps
                        for (int i = 0; i < waypointHeatMap.SubSamplingRateList[n]; i += 1)
                        {
                            for (int j = 0; j < waypointHeatMap.SubSamplingRateList[n]; j += 1)
                            {
                                if ((xBase + j < waypointHeatMap.nbCellInBaseHeatMapWidth) && (yBase + i < waypointHeatMap.nbCellInBaseHeatMapHeight))
                                    waypointHeatMap.BaseHeatMapData[yBase + i, xBase + j] = value;
                            }
                        }
                    }
                }
                //    });
                //});
                //OptimalPosInBaseHeatMapCoordinates = heatMap.GetMaxPositionInBaseHeatMapCoordinates();
                OptimalPosInBaseHeatMapCoordinates = new PointD(maxXpos, maxYpos);
            }

            //var OptimalPosition = heatMap.GetMaxPositionInBaseHeatMap();
            OptimalPosition = waypointHeatMap.GetFieldPosFromBaseHeatMapCoordinates(OptimalPosInBaseHeatMapCoordinates.X, OptimalPosInBaseHeatMapCoordinates.Y);

            //var OptimalPosition = destinationLocation;

            OnHeatMap(robotId, waypointHeatMap);
            if (OptimalPosition != null && destinationLocation != null)
                SetNextWayPoint(new Location((float)OptimalPosition.X, (float)OptimalPosition.Y, (float)destinationLocation.Theta, 0, 0, 0));

            sw.Stop(); // Fin de la mesure
            //for (int n = 0; n < nbComputationsList.Length; n++)
            //{
            //    Console.WriteLine("Calcul WayPoint - Nb Calculs Etape " + n + " : " + nbComputationsList[n]);
            //}
            //Console.WriteLine("Temps de calcul de la heatMap WayPoint : " + sw.Elapsed.TotalMilliseconds.ToString("N4")+" ms"); // Affichage de la mesure
        }

        double CalculPenalisation(PointD ptCourant)
        {
            double penalisation = 0;
            if (globalWorldMap != null)
            {
                //Si le robot existe dans le distionnaire des robots
                if (globalWorldMap.teammateLocationList.ContainsKey(robotId))
                {
                    Location robotLocation = globalWorldMap.teammateLocationList[robotId];
                    if (destinationLocation != null && robotLocation != null)
                    {
                        double angleDestination = Math.Atan2(destinationLocation.Y - robotLocation.Y, destinationLocation.X - robotLocation.X);

                        //On génère la liste des robots à éviter...
                        Dictionary<int, Location> robotToAvoidDictionary = new Dictionary<int, Location>();

                        //On ajoute la liste des robots de l'équipe à la liste des robots à éviter
                        lock (globalWorldMap.teammateLocationList)
                        {
                            foreach (var robot in globalWorldMap.teammateLocationList)
                            {
                                robotToAvoidDictionary.Add(robot.Key, robot.Value);
                            }
                        }

                        //On ajoute la liste des robots adverses à la liste des robots à éviter
                        var opponentsList = globalWorldMap.opponentLocationList.ToList(); //On évite un lock couteux en perf en faisant une copie locale
                        int i = 0;
                        foreach (var robot in opponentsList)
                        {
                            i++;
                            robotToAvoidDictionary.Add((int)TeamId.Opponents + i, robot);
                        }
                        
                        //On calcule la pénalisation sur la liste des robots à éviter
                        var robotToAvoidList = robotToAvoidDictionary.ToList();   //On évite un lock couteux en perf en faisant une copie locale                        
                        foreach (var robot in robotToAvoidList)
                        {
                            int competitorId = robot.Key;
                            Location competitorLocation = robot.Value;

                            //On itère sur tous les robots sauf celui-ci
                            if (competitorId != robotId && competitorLocation != null)
                            {
                                double angleRobotAdverse = Math.Atan2(competitorLocation.Y - robotLocation.Y, competitorLocation.X - robotLocation.X);
                                double distanceRobotAdverse = Toolbox.Distance(competitorLocation.X, competitorLocation.Y, robotLocation.X, robotLocation.Y);

                                //PointD ptCourant = GetFieldPosFromHeatMapCoordinates(x, y);
                                double distancePt = Toolbox.Distance(ptCourant.X, ptCourant.Y, robotLocation.X, robotLocation.Y);
                                double anglePtCourant = Math.Atan2(ptCourant.Y - robotLocation.Y, ptCourant.X - robotLocation.X);

                                if (Math.Abs(distanceRobotAdverse * (anglePtCourant - angleRobotAdverse)) < 0.2 && distancePt > distanceRobotAdverse - 0.2)
                                    penalisation += 1;// Math.Max(0, 1 - Math.Abs(anglePtCourant - angleRobotAdverse) *10.0);

                            }
                        }

                        //On calcule la pénalisation sur la liste des obstacles à éviter
                        foreach (var obstacle in globalWorldMap.obstacleLocationList)
                        {
                            double angleObstacle = Math.Atan2(obstacle.Y - robotLocation.Y, obstacle.X - robotLocation.X);
                            double distanceObstacle = Toolbox.Distance(obstacle.X, obstacle.Y, robotLocation.X, robotLocation.Y);

                            double distancePt = Toolbox.Distance(ptCourant.X, ptCourant.Y, robotLocation.X, robotLocation.Y);
                            double anglePtCourant = Math.Atan2(ptCourant.Y - robotLocation.Y, ptCourant.X - robotLocation.X);

                            //double distancePtObstacle = Toolbox.Distance(ptCourant.X, ptCourant.Y, obstacle.X, obstacle.Y);

                            //if (distanceObstacle> 0.3 && distancePtObstacle < 0.2)
                            if (distanceObstacle > 0.25 && Math.Abs(distanceObstacle * (anglePtCourant - angleObstacle)) < 0.4 && distancePt > distanceObstacle - 0.4)
                                penalisation += 1;// Math.Max(0, 1 - Math.Abs(anglePtCourant - angleRobotAdverse) *10.0);                                                         
                        }
                    }
                }
            }
            return penalisation;
        }


        //private PointD GetFieldPosFromHeatMapCoordinates(int x, int y)
        //{
        //    return new PointD(-fieldLength / 2 + x * heatMapCellsize, -fieldHeight / 2 + y * heatMapCellsize);
        //}

        public delegate void NewWayPointEventHandler(object sender, LocationArgs e);
        public event EventHandler<LocationArgs> OnWaypointEvent;
        public virtual void OnWaypoint(int id, Location wayPointlocation)
        {
            var handler = OnWaypointEvent;
            if (handler != null)
            {
                handler(this, new LocationArgs { RobotId = id, Location = wayPointlocation});
            }
        }


        public delegate void HeatMapEventHandler(object sender, HeatMapArgs e);
        public event EventHandler<HeatMapArgs> OnHeatMapEvent;
        public virtual void OnHeatMap(int id, Heatmap heatMap)
        {
            var handler = OnHeatMapEvent;
            if (handler != null)
            {
                handler(this, new HeatMapArgs { RobotId = id, HeatMap = heatMap });
            }
        }
    }
}
