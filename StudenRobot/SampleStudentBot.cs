using Robocode;
using Robocode.Util;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CAP4053.Student
{
    public class SampleStudentBot : TeamRobot
    {
        public class Enermy
        {
            internal string name; // name of the scanned robot
            internal double velocity; // velocity of the scanned robot from the last update
            internal double heading; // heading of the scanned robot from the last update
            internal double energy;
            internal double bearing;
            internal double distance;
            internal double futureX; // predicated x coordinate to aim our gun at, when firing at the robot
            internal double futureY; // predicated y coordinate to aim our gun at, when firing at the robot
            internal double x; // x coordinate of the scanned robot based on the last update
            internal double y; // y coordinate of the scanned robot based on the last update
            internal int moveDirection = 1;

            internal Enermy(ScannedRobotEvent e)
            {
                this.name = e.Name;
                this.velocity = e.Velocity;
                this.heading = e.HeadingRadians;
                this.energy = e.Energy;
                this.bearing = e.BearingRadians;
                this.distance = e.Distance;
            }
        }

        Dictionary<string, Enermy> enermies;
        Enermy target;
        Dictionary<string, Action> gameState = new Dictionary<string, Action>();

        private int turnDirection;
        private int moveDirection = 1;
        private double moveAmount;

        Random r = new Random();

        public override void Run()
        {
            initialState();
            while (true)
            {
                SetTurnRadarRight(double.PositiveInfinity);
                if (target != null)
                    gameState["attack"]();
                Execute();
            }
            /*
            while (true)
            {
                //SetAhead(moveAmount * moveDirection);
                //moveAmount = Math.Max(0, moveAmount - 1);

                // Sets the robot to turn right or turn left (at maximum speed) or
                // stop turning depending on the turn direction
                //SetTurnRight(45 * turnDirection); // degrees
                //SetTurnRadarRight(10);

                int n = r.Next(75,100);
                SetAhead(n);
                SetBack(n);
                SetTurnRadarRight(360);
                Execute();
            }*/
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            /*
            double turnRadar = Utils.NormalRelativeAngleDegrees(Heading - RadarHeading + e.Bearing);
            TurnRadarRight(turnRadar);
            SetTurnRight(e.Bearing + 90);
            if (GunHeat == 0 && e.Distance < 200)
            {
                double firePower = Math.Min((1200 / e.Distance), 3);
                double bulletSpeed = 20 - firePower * 3;
                double enermyHeading = Utils.NormalRelativeAngleDegrees(e.Heading);
                double eRelativeAngle = Utils.NormalRelativeAngleDegrees(Heading + e.Bearing);
                double enermyX = getEnermyX(eRelativeAngle, e);
                double enermyY = getEnermyY(eRelativeAngle, e);

                long time = (long)(e.Distance / bulletSpeed);
                double enermyFutureX = getEnermyFutureX(enermyX, time, e);
                double enermyFutureY = getEnermyFutureY(enermyY, time, e);
                double futureBearing = getFutureBearing(enermyFutureX, enermyFutureY);
                futureBearing = Utils.NormalRelativeAngleDegrees(futureBearing - Heading);
                SetTurnGunRight(Utils.NormalRelativeAngleDegrees(futureBearing - GunHeading + Heading));
                SetFire(firePower);
            }
            */
            Enermy enermy = new Enermy(e);
            if (enermies.ContainsKey(e.Name)) //if we already have the enermy in the dict
            {
                enermies[e.Name] = enermy;
                Console.WriteLine(enermies[e.Name].distance);
            }
            else
                enermies.Add(e.Name, enermy);
            if(target == null || target.name == enermy.name)
            {
                target = enermy;
            }

        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnRight(45);
            Back(100);
        }

        public override void OnHitWall(HitWallEvent e)
        {
            moveDirection *= -1;
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            //Back(50);
           // Ahead(100);
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            enermies.Remove(e.Name);
            if (target != null && target.name == e.Name)
                target = null;
        }

        public void initialState()
        {
            enermies = new Dictionary<string, Enermy>();
            gameState.Add("attack", () => handleAttack(target));

            target = null;
            SetColors(Color.Red, Color.Red, Color.Red, Color.Red, Color.Red);
            //SetTurnRadarRight(double.PositiveInfinity);
            IsAdjustRadarForRobotTurn = true;
            IsAdjustGunForRobotTurn = true;
            SetTurnRadarRight(double.PositiveInfinity);
        }

        public void handleAttack(Enermy e)
        {
            double absbearing = e.bearing + HeadingRadians;
            double latVel = e.velocity * Math.Sin(e.heading - absbearing);
            double gunTurnAmount;
            SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
            double rand = r.Next(0, 1);
            double firePower = Math.Min((600 / e.distance), 3);
            double aheadDistance = e.distance - 50;
            if (rand > 0.9)
            {
                MaxVelocity = (12 * rand) + 12;
            }
            if (e.distance > 60)
            {
                gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + latVel / 22);
                SetTurnGunRightRadians(gunTurnAmount);
                SetTurnRightRadians(Utils.NormalRelativeAngle(absbearing - HeadingRadians + latVel / Velocity));
                SetAhead(aheadDistance * moveDirection);
                SetFire(firePower);
            }
            else
            {
                gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + latVel / 15);
                SetTurnGunRightRadians(gunTurnAmount);
                SetTurnLeft(-90 - e.bearing);
                SetAhead(aheadDistance * moveDirection);
                SetFire(firePower);
            }
        }
        private double getEnermyX(double eRelativeAngle, ScannedRobotEvent e)
        {
            double enermyX = 0;
            if (eRelativeAngle < 0) // enermy on the left
            {
                if (eRelativeAngle <= 90)
                {
                    enermyX = X - Math.Cos(toRadians(eRelativeAngle + 90)) * e.Distance;
                }
                else if (eRelativeAngle > 90)
                {
                    enermyX = X - Math.Cos(toRadians((eRelativeAngle + 90) * -1)) * e.Distance;
                }
            }
            if (eRelativeAngle > 0) //engermy on the right
            {
                if (eRelativeAngle <= 90)
                {
                    enermyX = X + Math.Cos(toRadians(90 - eRelativeAngle)) * e.Distance;
                }
                else if (eRelativeAngle > 90)
                {
                    enermyX = X + Math.Cos(toRadians(eRelativeAngle - 90)) * e.Distance;
                }
            }
            return enermyX;
        }
        private double getEnermyY(double eRelativeAngle, ScannedRobotEvent e)
        {
            double enermyY = 0;
            if (Math.Abs(eRelativeAngle) <= 90) // enermy on top
            {
                enermyY = Y + Math.Cos(toRadians(Math.Abs(eRelativeAngle))) * e.Distance;

            }
            if (Math.Abs(eRelativeAngle) > 90) //engermy is below
            {
                enermyY = Y - Math.Cos(toRadians(180 - Math.Abs(eRelativeAngle))) * e.Distance;
            }
            return enermyY;
        }
        private double getEnermyFutureX(double x, double time, ScannedRobotEvent e)
        {
            double futureX = 0;
            if (e.Heading < 0)  //going left
            {
                if (e.Heading <= 90)
                {
                    futureX = x - Math.Cos(toRadians(e.Heading + 90)) * e.Velocity * time;
                }
                else if (e.Heading > 90)
                {
                    futureX = x - Math.Cos(toRadians((e.Heading + 90) * -1)) * e.Velocity * time;
                }
            }
            if (e.Heading > 0) //going right
            {
                if (e.Heading <= 90)
                {
                    futureX = x + Math.Cos(toRadians(90 - e.Heading)) * e.Velocity * time;
                }
                else if (e.Heading > 90)
                {
                    futureX = x + Math.Cos(toRadians(e.Heading - 90)) * e.Velocity * time;
                }
            }
            return futureX;
        }
        private double getEnermyFutureY(double y, double time, ScannedRobotEvent e)
        {
            double futureY = 0;
            if (Math.Abs(e.Heading) <= 90) // going up
            {
                futureY = y + Math.Cos(toRadians(Math.Abs(e.Heading))) * e.Velocity * time;

            }
            if (Math.Abs(e.Heading) > 90) //going down
            {
                futureY = y - Math.Cos(toRadians(180 - Math.Abs(e.Heading))) * e.Velocity * time;
            }
            return futureY;
        }
        private double getFutureBearing(double enermyX, double enermyY)
        {
            double deltaX = enermyX - X;
            double deltaY = enermyY - Y;
            double bearing = Math.Atan2(deltaY,deltaX);
             if (deltaX >= 0 && deltaY >= 0)
                bearing = 90.0 - toDegree(bearing);
            else if (deltaX >= 0 && deltaY <= 0)
                bearing = Math.Abs(toDegree(bearing)) + 90.0;
            else if (deltaX <= 0 && deltaY <= 0)
                bearing = -(toDegree(bearing)-90);
            else if (deltaX <= 0 && deltaY >= 0)
                bearing = -(toDegree(bearing)+270);
            return bearing;
        }
        private double toDegree(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private double toRadians(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
