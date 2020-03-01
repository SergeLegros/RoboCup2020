﻿using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utilities;

namespace WpfWorldMapDisplay
{
    public class RobotDisplay
    {
        private PolygonExtended shape;
        private Random rand = new Random();

        private Location location;
        private Location destinationLocation;
        private Location waypointLocation;
        public double[,] heatMap;
        List<PointD> lidarMap;
        List<Location> opponentLocationList;
        List<Location> teamLocationList;
        List<PolarPointListExtended> lidarObjectList;

        System.Drawing.Color displayColor;
        int displayTransparency = 0xFF;

        public RobotDisplay(PolygonExtended pe, System.Drawing.Color color, double transparency)
        {
            location = new Location(0, 0, 0, 0, 0, 0);
            destinationLocation = new Location(0, 0, 0, 0, 0, 0);
            waypointLocation = new Location(0, 0, 0, 0, 0, 0);
            shape = pe;
            lidarMap = new List<PointD>();
            displayTransparency = (int)(transparency * 255);
            displayColor = System.Drawing.Color.FromArgb((byte)displayTransparency, color.R, color.G, color.B);
        }

        public void SetPosition(double x, double y, double theta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
        }
        public void SetSpeed(double vx, double vy, double vTheta)
        {
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }
        public void SetDestination(double x, double y, double theta)
        {
            destinationLocation.X = x;
            destinationLocation.Y = y;
            destinationLocation.Theta = theta;
        }
        public void SetWayPoint(double x, double y, double theta)
        {
            waypointLocation.X = x;
            waypointLocation.Y = y;
            waypointLocation.Theta = theta;
        }

        public void SetHeatMap(double[,] heatMap)
        {
            this.heatMap = heatMap;
        }

        public void SetLidarMap(List<PointD> lidarMap)
        {
            this.lidarMap = lidarMap;
        }
        public void SetLidarObjectList(List<PolarPointListExtended> lidarObjectList)
        {
            this.lidarObjectList = lidarObjectList;
        }

        public void SetOpponentLocationList(List<Location> list)
        {
            this.opponentLocationList = list;
        }

        public void SetTeamLocationList(List<Location> list)
        {
            this.teamLocationList = list;
        }
        public void SetPositionAndSpeed(double x, double y, double theta, double vx, double vy, double vTheta)
        {
            location.X = x;
            location.Y = y;
            location.Theta = theta;
            location.Vx = vx;
            location.Vy = vy;
            location.Vtheta = vTheta;
        }

        public PolygonExtended GetRobotPolygon()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            foreach (var pt in shape.polygon.Points)
            {
                Point polyPt = new Point(pt.X * Math.Cos(location.Theta) - pt.Y * Math.Sin(location.Theta), pt.X * Math.Sin(location.Theta) + pt.Y * Math.Cos(location.Theta));
                polyPt.X += location.X;
                polyPt.Y += location.Y;
                polygonToDisplay.polygon.Points.Add(polyPt);
                polygonToDisplay.backgroundColor = displayColor;// shape.backgroundColor;
                polygonToDisplay.borderColor = shape.borderColor;
                polygonToDisplay.borderWidth = shape.borderWidth;
            }
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotSpeedArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double LongueurFleche = Math.Sqrt(location.Vx * location.Vx + location.Vy * location.Vy);
            double headingAngle = Math.Atan2(location.Vy, location.Vx) + location.Theta;
            double xTete = LongueurFleche * Math.Cos(headingAngle);
            double yTete = LongueurFleche * Math.Sin(headingAngle);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche1), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete - longueurTeteFleche * Math.Cos(angleTeteFleche2), location.Y + yTete - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(location.X + xTete, location.Y + yTete));
            polygonToDisplay.borderWidth = 2;
            polygonToDisplay.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
            polygonToDisplay.borderDashPattern = new double[] { 3, 3 };
            polygonToDisplay.borderOpacity = 1;
            polygonToDisplay.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotDestinationArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double headingAngle = Math.Atan2(destinationLocation.Y - location.Y, destinationLocation.X - location.X);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche1), destinationLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche2), destinationLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(destinationLocation.X, destinationLocation.Y));
            polygonToDisplay.borderWidth = 5;
            polygonToDisplay.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            polygonToDisplay.borderDashPattern = new double[] { 5, 5 };
            polygonToDisplay.borderOpacity = 0.4;
            polygonToDisplay.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            return polygonToDisplay;
        }
        public PolygonExtended GetRobotWaypointArrow()
        {
            PolygonExtended polygonToDisplay = new PolygonExtended();
            double angleTeteFleche = Math.PI / 6;
            double longueurTeteFleche = 0.30;
            double headingAngle = Math.Atan2(waypointLocation.Y - location.Y, waypointLocation.X - location.X);

            polygonToDisplay.polygon.Points.Add(new Point(location.X, location.Y));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));
            double angleTeteFleche1 = headingAngle + angleTeteFleche;
            double angleTeteFleche2 = headingAngle - angleTeteFleche;
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche1), waypointLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche1)));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X - longueurTeteFleche * Math.Cos(angleTeteFleche2), waypointLocation.Y - longueurTeteFleche * Math.Sin(angleTeteFleche2)));
            polygonToDisplay.polygon.Points.Add(new Point(waypointLocation.X, waypointLocation.Y));

            return polygonToDisplay;
        }

        public XyDataSeries<double, double> GetRobotLidarPoints()
        {
            var dataSeries = new XyDataSeries<double, double>();
            if (lidarMap == null)
                return dataSeries;


            //lock (lidarMap)
            {
                var listX = lidarMap.Select(e => e.X);
                var listY = lidarMap.Select(e => e.Y);

                if (listX.Count() == listY.Count())
                {
                    dataSeries.AcceptsUnsortedData = true;
                    dataSeries.Append(listX, listY);
                }
            }
            return dataSeries;
        }

        public List<PolygonExtended> GetRobotLidarObjects()
        {
            var polygonExtendedList = new List<PolygonExtended>();
            if (this.lidarObjectList == null)
                return polygonExtendedList;

            foreach (var obj in this.lidarObjectList)
            {
                PolygonExtended polygonToDisplay = new PolygonExtended();
                foreach (var pt in obj.polarPointList)
                {
                    polygonToDisplay.polygon.Points.Add(new Point(location.X + pt.Distance * Math.Cos(pt.Angle), location.Y + pt.Distance * Math.Sin(pt.Angle)));
                }
                polygonToDisplay.borderColor = obj.displayColor;
                polygonToDisplay.borderWidth = (float)obj.displayWidth;
                polygonToDisplay.backgroundColor = obj.displayColor;
                polygonExtendedList.Add(polygonToDisplay);
            }
            return polygonExtendedList;
        }
    }
}