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
            internal string name; 
            internal Enermy(ScannedRobotEvent e)
            {
                this.name = e.Name;
            }
        }
        Enermy target;

        private int moveDirection = 1;
        private double wallMargin = 60;
        private double targetDist = 600;

        Random r = new Random();
        FiniteStateMachine fsm = new FiniteStateMachine();

        public void initialState()
        {
            SetColors(Color.Red, Color.Red, Color.Red, Color.Red, Color.Red);
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
            if (e.Energy == 0 || (e.Energy < 45 && e.Energy < Energy))
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

            if (fsm.gameState == FiniteStateMachine.GameState.attack)
            {
                double absbearing = Utils.NormalRelativeAngle(HeadingRadians + e.BearingRadians);
                double bulletSpeed = 20 - firePower * 3;
                double enermyHeading = Utils.NormalRelativeAngle(e.HeadingRadians);
                double enermyX = getEnermyX(absbearing, e);
                double enermyY = getEnermyY(absbearing, e);
                long time = (long)(e.Distance / bulletSpeed);
                double enermyFutureX = getEnermyFutureX(enermyX, time, e);
                double enermyFutureY = getEnermyFutureY(enermyY, time, e);
                double futureBearing = Utils.NormalRelativeAngle(getFutureBearing(enermyFutureX, enermyFutureY) - HeadingRadians);
                futureBearing = futureBearing > 3.5? 0: e.Velocity * getFutureRat(e);
                double gunTurnAmount;
                SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
                double aheadDistance = e.Distance - 130;
                if (e.Distance > 140)
                {
                    gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + futureBearing / 22);
                    SetTurnRightRadians(Utils.NormalRelativeAngle(e.BearingRadians + futureBearing / Velocity));
                }
                else
                {
                    gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + futureBearing / 15);
                    SetTurnRight(90 + e.Bearing - (15 * moveDirection));
                }
                if(Time %20 == 0)
                {
                    moveDirection *= -1;
                }
                SetTurnGunRightRadians(gunTurnAmount);
                SetAhead(aheadDistance * moveDirection);
                if (Energy > firePower && GunHeat == 0 && e.Distance < targetDist)
                {
                    SetFire(firePower);
                }

            }
            if (fsm.gameState == FiniteStateMachine.GameState.ram)
            {
                moveDirection = 1;
                double absbearing = Utils.NormalRelativeAngle(HeadingRadians + e.BearingRadians);
                double bulletSpeed = 20 - firePower * 3;
                double enermyHeading = Utils.NormalRelativeAngle(e.HeadingRadians);
                double enermyX = getEnermyX(absbearing, e);
                double enermyY = getEnermyY(absbearing, e);
                long time = (long)(e.Distance / bulletSpeed);
                double enermyFutureX = getEnermyFutureX(enermyX, time, e);
                double enermyFutureY = getEnermyFutureY(enermyY, time, e);
                double futureBearing = Utils.NormalRelativeAngle(getFutureBearing(enermyFutureX, enermyFutureY) - HeadingRadians);
                futureBearing = futureBearing > 3.5 ? 0 : e.Velocity * getFutureRat(e);
                double gunTurnAmount;
                gunTurnAmount = Utils.NormalRelativeAngle(absbearing - GunHeadingRadians + futureBearing / 15);
                SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
                SetTurnGunRightRadians(gunTurnAmount);
                if (Energy > firePower && GunHeat == 0 && e.Distance < targetDist)
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

        }

        public override void OnHitWall(HitWallEvent e)
        {
            moveDirection *= -1;
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            //Back(50);
            //Ahead(100);
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            if (e.Name == target.name)
                target = null;
        }


        private double getEnermyX(double absbearing, ScannedRobotEvent e)
        {
            double enermyX = 0;
            if (absbearing < 0) // enermy on the left
            {
                if (absbearing <= Math.PI/2)
                {
                    enermyX = X - Math.Cos(absbearing + Math.PI / 2) * e.Distance;
                }
                else if (absbearing > Math.PI / 2)
                {
                    enermyX = X - Math.Cos(absbearing + Math.PI / 2 * -1) * e.Distance;
                }
            }
            if (absbearing > 0) //engermy on the right
            {
                if (absbearing <= Math.PI / 2)
                {
                    enermyX = X + Math.Cos(Math.PI / 2 - absbearing) * e.Distance;
                }
                else if (absbearing > Math.PI / 2)
                {
                    enermyX = X + Math.Cos(absbearing - Math.PI / 2) * e.Distance;
                }
            }
            return enermyX;
        }
        private double getEnermyY(double absbearing, ScannedRobotEvent e)
        {
            double enermyY = 0;
            if (Math.Abs(absbearing) <= Math.PI / 2) // enermy on top
            {
                enermyY = Y + Math.Cos(Math.Abs(absbearing)) * e.Distance;

            }
            if (Math.Abs(absbearing) > Math.PI / 2) //engermy is below
            {
                enermyY = Y - Math.Cos(Math.PI - Math.Abs(absbearing)) * e.Distance;
            }
            return enermyY;
        }
        private double getEnermyFutureX(double x, double time, ScannedRobotEvent e)
        {
            double futureX = 0;
            if (e.HeadingRadians < 0)  //going left
            {
                if (e.HeadingRadians <= Math.PI / 2)
                {
                    futureX = x - Math.Cos(e.HeadingRadians + Math.PI / 2) * e.Velocity * time;
                }
                else if (e.HeadingRadians > Math.PI / 2)
                {
                    futureX = x - Math.Cos((e.HeadingRadians + Math.PI / 2) * -1) * e.Velocity * time;
                }
            }
            if (e.HeadingRadians > 0) //going right
            {
                if (e.HeadingRadians <= Math.PI / 2)
                {
                    futureX = x + Math.Cos(Math.PI / 2 - e.HeadingRadians) * e.Velocity * time;
                }
                else if (e.HeadingRadians > Math.PI / 2)
                {
                    futureX = x + Math.Cos(e.HeadingRadians - Math.PI / 2) * e.Velocity * time;
                }
            }
            return futureX;
        }
        private double getEnermyFutureY(double y, double time, ScannedRobotEvent e)
        {
            double futureY = 0;
            if (Math.Abs(e.HeadingRadians) <= Math.PI / 2) // going up
            {
                futureY = y + Math.Cos(Math.Abs(e.HeadingRadians)) * e.Velocity * time;

            }
            if (Math.Abs(e.HeadingRadians) > Math.PI / 2) //going down
            {
                futureY = y - Math.Cos(180 - Math.Abs(e.HeadingRadians)) * e.Velocity * time;
            }
            return futureY;
        }
        private double getFutureBearing(double enermyX, double enermyY)
        {
            double deltaX = enermyX - X;
            double deltaY = enermyY - Y;
            double bearing = Math.Atan2(deltaY, deltaX);
            if (deltaX >= 0 && deltaY >= 0)
                bearing = Math.PI/2 - bearing;
            else if (deltaX >= 0 && deltaY <= 0)
                bearing = Math.Abs(bearing) + Math.PI / 2;
            else if (deltaX <= 0 && deltaY <= 0)
                bearing = -(bearing - Math.PI / 2);
            else if (deltaX <= 0 && deltaY >= 0)
                bearing = -(bearing + Math.PI / 2 * 3);
            return bearing;
        }

        private double getFutureRat(ScannedRobotEvent e)
        {
            double abshearing = HeadingRadians + e.BearingRadians;
            double rat = Math.Sin(e.HeadingRadians - abshearing);
            return rat;
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
