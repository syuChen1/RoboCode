using Robocode;
using Robocode.Util;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace CAP4053.Student
{
    public class FiniteStateMachine
    {
        public enum GameState
        {
            start,
            attack,
            ram,
            flee,
        }
        public enum Events
        {
            hasTarget,
            noTarget,
            ramable,
            notRamable,
            lowEnergy,
            highEnergy,
        }

        public GameState gameState { get; set; }
        private Action[,] transition;
        Queue<Events> queue = new Queue<Events>();

        public FiniteStateMachine()
        {
            this.transition = new Action[4, 6]
            {   //hasTarget,      //noTarget      //ramable     //notRamable    //lowEnergy    //highEnergy 
                {this.toAttack,   this.toStart,   this.toRam,   this.toAttack,  this.toFlee,   this.toStart}, //start
                {this.toAttack,   this.toStart,   this.toRam,   this.toAttack,  this.toFlee,   this.toAttack}, //attack
                {this.toRam,      this.toStart,   this.toRam,   this.toAttack,  this.toFlee,   this.toRam}, //ram
                {this.toFlee,     this.toStart,   this.toFlee,  this.toFlee,    this.toFlee,   this.toAttack } //flee
            };
        }
        public void enqueEvent(Events _event)
        {
            queue.Enqueue(_event);
        }
        public void Transition()
        {
            if (queue.Count != 0)
                this.transition[(int)this.gameState, (int)queue.Dequeue()].Invoke();
        }
        private void toAttack() { this.gameState = GameState.attack; }
        private void toStart() { this.gameState = GameState.start; }
        private void toRam() { this.gameState = GameState.ram; }
        private void toFlee() { this.gameState = GameState.flee; }

    }

    public class SampleStudentBot : TeamRobot
    {
        public class Enermy
        {
            internal string name; // name of the scanned robot

            internal Enermy(ScannedRobotEvent e)
            {
                this.name = e.Name;
            }
        }
        Enermy target;

        private int turnDirection;
        private int moveDirection = 1;
        private double moveAmount;
        private double wallMargin = 60;

        Random r = new Random();
        FiniteStateMachine fsm = new FiniteStateMachine();

        public void initialState()
        {
            SetColors(Color.Red, Color.Red, Color.Red, Color.Red, Color.Red);
            //SetTurnRadarRight(double.PositiveInfinity);
            IsAdjustRadarForRobotTurn = true;
            IsAdjustGunForRobotTurn = true;
        }

        public override void Run()
        {
            initialState();
            while (true)
            {
                if (target != null)
                    fsm.enqueEvent(FiniteStateMachine.Events.hasTarget);
                if (target == null)
                {
                    SetTurnRadarRight(double.PositiveInfinity);
                    fsm.enqueEvent(FiniteStateMachine.Events.noTarget);
                }
                fsm.Transition();
                Console.WriteLine(fsm.gameState);
                Execute();
            }
        }
        public override void OnScannedRobot(ScannedRobotEvent e)
        {

            target = new Enermy(e);
            double firePower = Math.Min((600 / e.Distance), 3);
            if (Energy < firePower)
            {
                fsm.enqueEvent(FiniteStateMachine.Events.lowEnergy);
            }
            else if (e.Energy == 0 || (e.Energy < 45 && e.Energy < Energy))
            {
                fsm.enqueEvent(FiniteStateMachine.Events.ramable);
            }
            else if (e.Energy > 45 || e.Energy > Energy)
            {
                fsm.enqueEvent(FiniteStateMachine.Events.notRamable);
            }
            else if (Energy > firePower)
            {
                fsm.enqueEvent(FiniteStateMachine.Events.highEnergy);
            }

            fsm.Transition();
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
            if (fsm.gameState == FiniteStateMachine.GameState.attack)
            {

                double absbearing = e.BearingRadians + HeadingRadians;
                double latVel = e.Velocity * Math.Sin(e.HeadingRadians - absbearing);
                double gunTurnAmount;
                SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
                double rand = r.Next(0, 1);
                double aheadDistance = e.Distance - 140;
                if (rand > 0.9)
                {
                    MaxVelocity = (12 * rand) + 12;
                }
                if (e.Distance > 150)
                {
                    gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + latVel / 22);
                    SetTurnRightRadians(Utils.NormalRelativeAngle(absbearing - HeadingRadians + latVel / Velocity));
                }
                else
                {
                    gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + latVel / 15);
                    SetTurnLeft(-90 - e.Bearing);
                }
                SetTurnGunRightRadians(gunTurnAmount);
                SetAhead(aheadDistance * moveDirection);
                if (Energy > firePower)
                {
                    SetFire(firePower);
                }

            }
            if (fsm.gameState == FiniteStateMachine.GameState.ram)
            {

                double absbearing = e.BearingRadians + HeadingRadians;
                double latVel = e.Velocity * Math.Sin(e.HeadingRadians - absbearing);
                double gunTurnAmount;
                gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + latVel / 22);
                SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
                SetTurnGunRightRadians(gunTurnAmount);
                if (Energy > firePower)
                {
                    SetFire(firePower);
                }
                SetTurnRightRadians(Utils.NormalRelativeAngle(e.BearingRadians));
                SetAhead((e.Distance + 500) * moveDirection);

            }

            if (fsm.gameState == FiniteStateMachine.GameState.flee)
            {
                SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
                SetTurnRight(Utils.NormalRelativeAngle(e.Bearing + 90));
                if (tooCloseToWall())
                {
                    MaxVelocity = 2.9;
                }
                else
                {
                    MaxVelocity = Math.Min((0.7 + r.NextDouble()), 1.0) * 12;
                }
                SetAhead(Math.Max(e.Distance, 100) * moveDirection * -1);
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            //TurnRight(45);
            //Back(100);
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
            if (e.Name == target.name)
                target = null;
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
            double bearing = Math.Atan2(deltaY, deltaX);
            if (deltaX >= 0 && deltaY >= 0)
                bearing = 90.0 - toDegree(bearing);
            else if (deltaX >= 0 && deltaY <= 0)
                bearing = Math.Abs(toDegree(bearing)) + 90.0;
            else if (deltaX <= 0 && deltaY <= 0)
                bearing = -(toDegree(bearing) - 90);
            else if (deltaX <= 0 && deltaY >= 0)
                bearing = -(toDegree(bearing) + 270);
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
        private bool tooCloseToWall()
        {
            return (X <= wallMargin || X >= BattleFieldWidth - wallMargin
                 || Y <= wallMargin || Y >= BattleFieldHeight - wallMargin);
        }

    }
}
