﻿using HeatMap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;
using ZeroFormatter;

namespace WorldMap
{
    [ZeroFormattable]
    public class LocalWorldMap:WorldMap
    {// UnionKey value must return constant value(Type is free, you can use int, string, enum, etc...)
        public override WorldMapType Type
        {
            get
            {
                return WorldMapType.LocalWM;
            }
        }

        [Index(1)]
        public virtual int RobotId { get; set; }
        [Index(2)]
        public virtual int TeamId { get; set; }
        [Index(3)]
        public virtual Location robotLocation { get; set; }
        [Index(4)]
        public virtual RobotRole robotRole { get; set; }
        [Index(5)]
        public virtual string messageDisplay { get; set; }

        [Index(6)]
        public virtual PlayingSide playingSide { get; set; }
        [Index(7)]
        public virtual Location robotGhostLocation { get; set; }
        [Index(8)]
        public virtual Location destinationLocation { get; set; }
        [Index(9)]
        public virtual Location waypointLocation { get; set; }
        [Index(10)]
        public virtual List<Location> ballLocationList { get; set; }
        [Index(11)]
        public virtual List<LocationExtended> obstaclesLocationList { get; set; }
        [IgnoreFormat]
        public virtual List<PolarPointListExtended> lidarObjectList { get; set; }

        [JsonIgnore]
        [IgnoreFormat]
        public virtual List<PointD> lidarMap { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual Heatmap heatMapStrategy { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual Heatmap heatMapWaypoint { get; set; }

        public LocalWorldMap()
        {
            //Type = "LocalWorldMap";
        }
    }

    public enum GameState
    {
        STOPPED,
        STOPPED_GAME_POSITIONING,
        PLAYING,
    }

    public enum StoppedGameAction
    {
        NONE,
        KICKOFF,
        KICKOFF_OPPONENT,
        FREEKICK,
        FREEKICK_OPPONENT,
        GOALKICK,
        GOALKICK_OPPONENT,
        THROWIN,
        THROWIN_OPPONENT,
        CORNER,
        CORNER_OPPONENT,
        PENALTY,
        PENALTY_OPPONENT,
        PARK,
        DROPBALL,
        GOTO_0_1,
        GOTO_0_1_OPPONENT,
        GOTO_1_0,
        GOTO_1_0_OPPONENT,
        GOTO_0_M1,
        GOTO_0_M1_OPPONENT,
        GOTO_M1_0,
        GOTO_M1_0_OPPONENT,
        GOTO_0_0,
        GOTO_0_0_OPPONENT,
    }
}
