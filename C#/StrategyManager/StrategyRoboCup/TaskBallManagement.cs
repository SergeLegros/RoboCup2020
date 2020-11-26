﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyManagerNS.StrategyRoboCupNS
{
    enum TaskBallManagementState
    {
        PasDeBalle,
        PasDeBalleEnCours,
        PossessionBalle,
        PossessionBalleEnCours,
    }

    class TaskBallManagement
    {
        /// <summary>
        /// Une tache est un processus permettant de gérer une partie de code de manière autonome et asynchrone.
        /// On la balaie périodiquement (mais souvent) à une vitesse définie par le Thread.Sleep(ms) en sortie du switch case
        /// 
        /// Afin d'assurer qu'une tache ne sera jamais bloquée, il est FORMELLEMENT INTERDIT d'y mettre des while bloquants
        /// 
        /// Pour éviter de flooder à chaque appel (par exemple si on veut lancer un ordre sur une liaison série),
        ///     on passe l'ordre dans une étape ETAPE_NAME et on passe juste après dans une étape ETAPE_NAME_ATTENTE
        ///     
        /// Pour implanter une temporisation, on a donc recours à une stopwatch initialisée (restart) dans ETAPE_NAME
        ///     et lue dans ETAPE_NAME_ENCOURS
        /// 
        /// La tâche a une référence vers le StrategyXXXX parent qui est passée au constructeur de manière à simplifier
        ///     les accès aux propriétés de ce StrategyXXXX parent sans passer par des events 
        ///     C'est une dérogation à la règle en vigueur dans l'ensemble du projet mais on l'accepte ici vu 
        ///     le nombre d'appel potentiels
        /// 
        /// TaskThread.IsBackground = true; signifie que le Thread est détruit quand on ferme l'application de quelque manière
        ///     que ce soit. Il faut le laisser impérativement.
        ///     
        /// </summary>

        StrategyRoboCup parent;
        Thread TaskThread;
        TaskBallManagementState state = TaskBallManagementState.PasDeBalle;
        Stopwatch sw = new Stopwatch();
        
        public TaskBallManagement(StrategyRoboCup parent)
        {
            this.parent = parent;
            TaskThread = new Thread(TaskThreadProcess);
            TaskThread.IsBackground = true;
            TaskThread.Start();
            sw.Stop();
            sw.Reset();
        }

     

        void TaskThreadProcess()
        {
            while (true)
            {
                switch (state)
                {
                    case TaskBallManagementState.PasDeBalle:
                        sw.Restart();
                        state = TaskBallManagementState.PasDeBalleEnCours;
                        break;
                    case TaskBallManagementState.PasDeBalleEnCours:
                        parent.MessageDisplay = "No Ball : " + (sw.ElapsedMilliseconds / 1000).ToString();
                        if(parent.isHandlingBall)
                            state = TaskBallManagementState.PossessionBalle;
                        break;
                    case TaskBallManagementState.PossessionBalle:
                        sw.Restart();
                        state = TaskBallManagementState.PossessionBalleEnCours;
                        break;
                    case TaskBallManagementState.PossessionBalleEnCours:
                        parent.MessageDisplay = "Ball : " + (sw.ElapsedMilliseconds / 1000).ToString();
                        if (sw.ElapsedMilliseconds>3000)
                        {
                            /// On demande un tir ou une passe
                            parent.OnShootRequest(parent.robotId, 2);
                        }
                        if (!parent.isHandlingBall)
                            state = TaskBallManagementState.PasDeBalle;
                        break;

                    default:
                        break;
                }
                Thread.Sleep(100);
            }
        }
    }
}