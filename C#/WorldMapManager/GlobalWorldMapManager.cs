﻿using EventArgsLibrary;
using Newtonsoft.Json;
using RefereeBoxAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Utilities;
using WorldMap;

namespace WorldMapManager
{
    public class GlobalWorldMapManager
    {
        int TeamId;
        string TeamIpAddress = "";
        double freqRafraichissementWorldMap = 30;

        Dictionary<int, LocalWorldMap> localWorldMapDictionary = new Dictionary<int, LocalWorldMap>();
        GlobalWorldMapStorage globalWorldMapStorage = new GlobalWorldMapStorage();
        GlobalWorldMap globalWorldMap = new GlobalWorldMap();
        Timer globalWorldMapSendTimer;

        GameState currentGameState = GameState.STOPPED;
        StoppedGameAction currentStoppedGameAction = StoppedGameAction.NONE;
        

        public GlobalWorldMapManager(int teamId, string ipAddress)
        {
            TeamId = teamId;
            TeamIpAddress = ipAddress;
            globalWorldMapSendTimer = new Timer(1000/freqRafraichissementWorldMap);
            globalWorldMapSendTimer.Elapsed += GlobalWorldMapSendTimer_Elapsed;
            globalWorldMapSendTimer.Start();
        }

        private void GlobalWorldMapSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MergeLocalWorldMaps();
        }

        public void OnLocalWorldMapReceived(object sender, EventArgsLibrary.LocalWorldMapArgs e)
        {
            AddOrUpdateLocalWorldMap(e.LocalWorldMap);
        }

        public void OnRefereeBoxCommandReceived(object sender, RefBoxMessageArgs e)
        {
            var command = e.refBoxMsg.command;
            var robotId = e.refBoxMsg.robotID;
            var targetTeam = e.refBoxMsg.targetTeam;

            switch (command)
            {
                case "START":
                    currentGameState = GameState.PLAYING;
                    currentStoppedGameAction = StoppedGameAction.NONE;
                    break;
                case "STOP":
                    currentGameState = GameState.STOPPED;
                    break;
                case "DROP_BALL":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    currentStoppedGameAction = StoppedGameAction.DROPBALL;
                    break;
                case "HALF_TIME":
                    break;
                case "END_GAME":
                    break;
                case "GAME_OVER":
                    break;
                case "PARK":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    currentStoppedGameAction = StoppedGameAction.PARK;
                    break;
                case "FIRST_HALF":
                    break;
                case "SECOND_HALF":
                    break;
                case "FIRST_HALF_OVER_TIME":
                    break;
                case "RESET":
                    break;
                case "WELCOME":
                    break;
                case "KICKOFF":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.KICKOFF;
                    else
                        currentStoppedGameAction = StoppedGameAction.KICKOFF_OPPONENT;
                    break;
                case "FREEKICK":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.FREEKICK;
                    else
                        currentStoppedGameAction = StoppedGameAction.FREEKICK_OPPONENT;
                    break;
                case "GOALKICK":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.GOALKICK;
                    else
                        currentStoppedGameAction = StoppedGameAction.GOALKICK_OPPONENT;
                    break;
                case "THROWIN":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.THROWIN;
                    else
                        currentStoppedGameAction = StoppedGameAction.THROWIN_OPPONENT;
                    break;
                case "CORNER":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.CORNER;
                    else
                        currentStoppedGameAction = StoppedGameAction.CORNER_OPPONENT;
                    break;
                case "PENALTY":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.PENALTY;
                    else
                        currentStoppedGameAction = StoppedGameAction.PENALTY_OPPONENT;
                    break;
                case "GOAL":
                    break;
                case "SUBGOAL":
                    break;
                case "REPAIR":
                    break;
                case "YELLOW_CARD":
                    break;
                case "DOUBLE_YELLOW":
                    break;
                case "RED_CARD":
                    break;
                case "SUBSTITUTION":
                    break;
                case "IS_ALIVE":
                    currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == TeamIpAddress)
                        currentStoppedGameAction = StoppedGameAction.KICKOFF;
                    else
                        currentStoppedGameAction = StoppedGameAction.KICKOFF_OPPONENT;
                    break;
            }
        }

        private void AddOrUpdateLocalWorldMap(LocalWorldMap localWorldMap)
        {
            int robotId = localWorldMap.RobotId;
            int teamId = localWorldMap.TeamId;
            lock (localWorldMapDictionary)
            {
                if (localWorldMapDictionary.ContainsKey(robotId))
                    localWorldMapDictionary[robotId] = localWorldMap;
                else
                    localWorldMapDictionary.Add(robotId, localWorldMap);
            }
        }

        DecimalJsonConverter decimalJsonConverter = new DecimalJsonConverter();
        private void MergeLocalWorldMaps()
        {
            //Fusion des World Map locales pour construire la world map globale
            lock (localWorldMapDictionary)
            {
                //On rassemble les infos issues des cartes locales de chacun des robots
                foreach (var localMap in localWorldMapDictionary)
                {
                    globalWorldMapStorage.AddOrUpdateRobotLocation(localMap.Key, localMap.Value.robotLocation);
                    globalWorldMapStorage.AddOrUpdateBallLocation(localMap.Key, localMap.Value.ballLocation);
                    globalWorldMapStorage.AddOrUpdateRobotDestination(localMap.Key, localMap.Value.destinationLocation);
                    globalWorldMapStorage.AddOrUpdateRobotWayPoint(localMap.Key, localMap.Value.waypointLocation);
                    globalWorldMapStorage.AddOrUpdateObstaclesList(localMap.Key, localMap.Value.obstaclesLocationList);
                }

                //Génération de la carte fusionnée à partir des perceptions des robots de l'équipe
                //La fusion porte avant tout sur la balle et sur les adversaires.

                //TODO : faire un algo de fusion robuste pour la balle
                globalWorldMap = new WorldMap.GlobalWorldMap(TeamId);

                //Pour l'instant on prend la position de balle vue par le robot 1 comme vérité, mais c'est à améliorer !
                if (localWorldMapDictionary.Count > 0)
                    globalWorldMap.ballLocation = localWorldMapDictionary.First().Value.ballLocation;
                globalWorldMap.teammateLocationList = new Dictionary<int, Location>();
                globalWorldMap.opponentLocationList = new List<Location>();

                //On place tous les robots de l'équipe dans la global map
                foreach (var localMap in localWorldMapDictionary)
                {
                    //On ajoute la position des robots de l'équipe dans la WorldMap
                    globalWorldMap.teammateLocationList.Add(localMap.Key, localMap.Value.robotLocation);
                }

                //On établit une liste des emplacements d'adversaires potentiels afin de les fusionner si possible
                List<Location> AdversairesPotentielsList = new List<Location>();
                List<int> AdversairesPotentielsMatchOccurenceList = new List<int>();
                foreach (var localMap in localWorldMapDictionary)
                {
                    //On tente de transformer les objets vus et ne correspondant pas à des robots alliés en des adversaires
                    List<Location> obstacleLocationList = new List<Location>();
                    try
                    {
                         obstacleLocationList = localMap.Value.obstaclesLocationList.ToList();
                    }
                    catch { }

                    foreach (var obstacleLocation in obstacleLocationList)
                    {
                        bool isTeamMate = false;
                        bool isAlreadyPresentInOpponentList = false;

                        //On regarde si l'obstacle est un coéquipier ou pas
                        foreach (var robotTeamLocation in globalWorldMap.teammateLocationList.Values)
                        {
                            if (obstacleLocation != null && robotTeamLocation != null)
                            {
                                if (Toolbox.Distance(obstacleLocation.X, obstacleLocation.Y, robotTeamLocation.X, robotTeamLocation.Y) < 0.4)
                                    isTeamMate = true;
                            }
                        }

                        //On regarde si l'obstacle existe dans la liste des adversaires potentiels ou pas
                        foreach (var opponentLocation in AdversairesPotentielsList)
                        {
                            if (obstacleLocation != null && opponentLocation != null)
                            {
                                if (Toolbox.Distance(obstacleLocation.X, obstacleLocation.Y, opponentLocation.X, opponentLocation.Y) < 0.4)
                                {
                                    isAlreadyPresentInOpponentList = true;
                                    var index = AdversairesPotentielsList.IndexOf(opponentLocation);
                                    AdversairesPotentielsMatchOccurenceList[index]++;
                                }
                            }
                        }

                        //Si un obstacle n'est ni un coéquipier, ni un adversaire potentiel déjà trouvé, c'est un nouvel adversaire potentiel
                        if (!isTeamMate && !isAlreadyPresentInOpponentList)
                        {
                            AdversairesPotentielsList.Add(obstacleLocation);
                            AdversairesPotentielsMatchOccurenceList.Add(1);
                        }
                    }
                }

                //On valide les adversaires potentiels si ils ont été perçus plus d'une fois par les robots
                for(int i=0; i< AdversairesPotentielsList.Count; i++)
                {
                    if (AdversairesPotentielsMatchOccurenceList[i] >= 2)
                    {
                        var opponentLocation = AdversairesPotentielsList[i];
                        globalWorldMap.opponentLocationList.Add(opponentLocation);
                    }
                }
            }

            //On ajoute les informations de stratégie utilisant les commandes de la referee box
            globalWorldMap.gameState = currentGameState;
            globalWorldMap.stoppedGameAction = currentStoppedGameAction;

            string json = JsonConvert.SerializeObject(globalWorldMap, decimalJsonConverter);
            OnMulticastSendGlobalWorldMap(json.GetBytes());
        }

        void DefineRolesAndGameState()
        {
            
        }
        
        //Output events
        public event EventHandler<DataReceivedArgs> OnMulticastSendGlobalWorldMapEvent;
        public virtual void OnMulticastSendGlobalWorldMap(byte[] data)
        {
            var handler = OnMulticastSendGlobalWorldMapEvent;
            if (handler != null)
            {
                handler(this, new DataReceivedArgs { Data = data });
            }
        }
    }
}
