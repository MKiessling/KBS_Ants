using AntMe.English;
using System;
using System.Collections.Generic;

namespace AntMe.Player.DHBW
{

    // The Player-Attribute allows to define the colony name and the name of the player.
    [Player(
        ColonyName = "DHBW ants",
        FirstName = "DHBW Mannheim",
        LastName = "KBS Project"
    )]

    // The Type-Attribute allows to change the properties of the ant.
    // To activate the type a name has to be specified and it has to be returned
    // in the method ChooseType. The attribute can be copied and reused with different names.
    // A more detailed description is in Section 6 of the Ant-Tutorial.
    [Caste(
        Name = "KillerAnt",
        SpeedModificator = 1,
        LoadModificator = -1,
        AttackModificator = 2,
        EnergyModificator = 1,
        RangeModificator = -1,
        ViewRangeModificator = -1,
        RotationSpeedModificator = -1
    )]

    [Caste(
        Name = "SugarAnt",
        SpeedModificator = 2,
        LoadModificator = 0,
        AttackModificator = -1,
        EnergyModificator = -1,
        RangeModificator = 0,
        ViewRangeModificator = 1,
        RotationSpeedModificator = -1
    )]

    

	public class DHBWAnt : BaseAnt
	{

        // Liste in die alle Ameisen eingetragen werden
        private static List<DHBWAnt> Antlist = new List<DHBWAnt>();
        // Liste in die alle Zuckerhaufen eingetragen werden
        private Sugar CurrentSugar;
        // jede Ameise erhält eine eigene ID
        private int AntID;
        private bool hasSugarTarget = false;
        private static bool needSugarAnts = true;
        private static int MARKER_FIGHT = 1;
        private static int MARKER_FOOD = 2;

        #region FlockingBoids

        private int Rule1()
        {

            // Für alle Ameisen die sich in Reichweite befinden wird Festgestellt, 
            // in welcher Richtung sie sich zum Individuum befindet
            // die Ameise versucht genau in die Gegenrichtung zu laufen
            // dieser Wert wird für alle Ameisen berechnet und ein Mittelwert zurückgegeben

            int DegreeSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                int Distance = Coordinate.GetDistanceBetween(this, Ant);
                if (Ant.getID() != this.getID() && (Distance < 40) && Ant.Caste.Equals(this.Caste))
                {
                    DegreeSum += (Distance - 180);
                }
            }
            if (DegreeSum == 0)
            {
                DegreeSum = new Random().Next(0, 180);
            }
            int ListSize = Antlist.Count;
            return ListSize != 0 ? DegreeSum / Antlist.Count : DegreeSum / 1;
        }

        private int Rule2()
        {
            // Es wird der Mittelwert aus der Bewegunsrichtung aller Ameisen gebildet

            int DirectionSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                if (Ant.getID() != this.getID() && Ant.Caste.Equals(this.Caste))
                {
                    DirectionSum += Ant.Direction;
                }
            }
            int ListSize = Antlist.Count;
            return ListSize != 0 ? DirectionSum / Antlist.Count : DirectionSum / 1;
        }

        private int Rule3()
        {
            int PositionSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                int DegreesBetween = Coordinate.GetDegreesBetween(this, Ant);
                if (Ant.getID() != this.getID() && Ant.Caste.Equals(this.Caste))
                {
                    PositionSum += DegreesBetween;
                }
            }
            if (PositionSum == 0)
            {
                PositionSum = 180;
            }
            int ListSize = Antlist.Count;
            return ListSize != 0 ? PositionSum / Antlist.Count : PositionSum / 1;
        }

        private int BoidUpdate()
        {
            int R1 = Rule1();
            int R2 = Rule2();
            int R3 = Rule3();
            int Weight1 = 60;
            int Weight2 = 30;
            int Weigth3 = 10;

            // die einzelnen Regeln können unterschiedlich gewichtet werden

            return (R1 * Weight1 + R2 * Weight2 + R3 * Weigth3) / (Weight1 + Weight2 + Weigth3);

        }

        #endregion

        private double ratioSugarKillerAnts()
        {
            double sugarAnts = 0;
            foreach (DHBWAnt ant in Antlist)
            {
                if (ant.Caste.Equals("SugarAnt")) sugarAnts++;
            }
            double listSize = (double)Antlist.Count;
            System.Diagnostics.Debug.WriteLine("Ratio: " + sugarAnts / listSize);
            return sugarAnts / listSize;
        }

        public int getID()
        {
            return this.AntID;
        }

        #region Castes
        
        /// <summary>
        /// Chooses a specific caste.
        /// </summary>
        /// <param name="typeCount">List of available ants from caste</param>
        /// <returns>Name of chosen caste</returns>
        public override string ChooseType(Dictionary<string, int> typeCount)
        {
            int ListSize = Antlist.Count;
            if (ListSize == 0)
            {
                // 1. Ameise wird erstellt
                Antlist = new List<DHBWAnt>();
                this.AntID = 1;
                Antlist.Add(this);
            }
            else
            {
                //weitere Ameisen werden erstellt
                this.AntID = ListSize + 1;
                Antlist.Add(this);
            }
            if (needSugarAnts || Antlist.Count < 75)
            {
                needSugarAnts =  (ratioSugarKillerAnts() > 0.7) ? false : true;
                return "SugarAnt";
            }
            else
            {
                needSugarAnts = (ratioSugarKillerAnts() < 0.5) ? true : false;
                return "KillerAnt";
            }
        }

        #endregion

       

        #region Movement

        /// <summary>
        /// Repeated if the ant don't know where to go.
		/// </summary>
        public override void Waits()
        {
            if (CurrentLoad == 0 && !hasSugarTarget)
            {
                TurnToDirection(BoidUpdate());
                GoAhead(20);
            }
        }
		

		/// <summary>
        /// Called once if the ant has exceeded 1/3 of its maximum range.
		/// </summary>
		public override void BecomesTired()
		{
            if (CurrentLoad == 0){
            GoBackToAnthill();}
		}

		#endregion

		#region Food

		/// <summary>
        /// Repeated if the ant spots at least one sugar pile.
		/// </summary>
        /// <param name="sugar">Nearest sugar pile</param>
        public override void Spots(Sugar sugar)
        {
            switch (Caste)
            {
                case "SugarAnt":
                    if (CurrentSugar != sugar) CurrentSugar = sugar;
                        hasSugarTarget = true;
                    if (CurrentLoad == 0)
                    {
                        int Direction, Distance;
                        Direction = Coordinate.GetDegreesBetween(this, sugar);
                        Distance = Coordinate.GetDistanceBetween(this, sugar);
                        MakeMark(Direction, Distance);
                        if (CurrentLoad == 0)
                        {
                           GoToTarget(sugar);
                        }
                    }
                    break;
                case "KillerAnt":
                    MakeMark(MARKER_FOOD, 100);
                    break;
            }
            
		}

		/// <summary>
        /// Repeated if the ant spots at least one fruit.
		/// </summary>
        /// <param name="fruit">Nearest fruit</param>
		public override void Spots(Fruit fruit)
		{
            MakeMark(MARKER_FOOD, 100);
            if (CurrentLoad == 0)
            {
                int Direction, Distance;
                Direction = Coordinate.GetDegreesBetween(this, fruit);
                Distance = Coordinate.GetDistanceBetween(this, fruit);
                MakeMark(Direction, Distance);
                if (CurrentLoad == 0)
                {
                    GoToTarget(fruit);
                }
            }
		}

		/// <summary>
        /// Called once if the ant has reached the targeted sugar pile.
		/// </summary>
        /// <param name="sugar">Sugar pile</param>
        public override void TargetReached(Sugar sugar)
        {
            MakeMark(0, 100);
            Take(sugar);
            GoBackToAnthill();
		}

		/// <summary>
        /// Called once if the ant has reached the targeted fruit.
		/// </summary>
        /// <param name="fruit">Fruit</param>
        public override void TargetReached(Fruit fruit)
		{
            MakeMark(0, 100);
            if (NeedsCarrier(fruit))
            {
                Take(fruit);
                GoBackToAnthill();
            }
            

		}

		#endregion

        #region Communication

        /// <summary>
        /// Called once if the ant smells a marker of the same colony. 
        /// Markers once smelled are not smelled again.
		/// </summary>
        /// <param name="marker">Next new marker</param>
		public override void SmellsFriend(Marker marker)
        {     
            switch (Caste)
            {
                case "SugarAnt":
                    if (Target == null && !hasSugarTarget && marker.Information == MARKER_FOOD)
                    {
                        TurnToDirection(marker.Information);
                        GoAhead();
                    }
                    break;
                case "KillerAnt":
                    if (Target == null && marker.Information == MARKER_FIGHT)
                    {
                        TurnToDirection(marker.Information);
                        GoAhead();
                    }
                    break;
            }
		}

		/// <summary>
        /// Repeated if the ant spots at least one ant of the the same colony.
		/// </summary>
		/// <param name="ant">Nearest friendly ant</param>
		public override void SpotsFriend(Ant ant)
		{
            
		}

		/// <summary>
        /// Called if the ant spots a friendly ant of another team.
		/// </summary>
		/// <param name="ant">Nearest friendly ant of another team</param>
        public override void SpotsPartner(Ant ant)
		{
		}

		#endregion

        #region Fight

        /// <summary>
        /// Repeated if the ant spots at least one bug.
		/// </summary>
        /// <param name="bug">Nearest bug</param>
        public override void SpotsEnemy(Bug bug)
        {
            switch (Caste)
            {
                case "SugarAnt":
                    if (!(Target is Anthill) && !hasSugarTarget)
                    {
                        MakeMark(MARKER_FIGHT, 100);
                        GoAwayFromTarget(bug);
                    }
                    break;
                case "KillerAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    if (FriendlyAntsFromSameCasteInViewrange > 5)
                    {
                        int Direction, Distance;
                        Direction = Coordinate.GetDegreesBetween(this, bug);
                        Distance = Coordinate.GetDistanceBetween(this, bug);
                        MakeMark(Direction, Distance);
                        Attack(bug);
                        
                    }
                    else
                    {
                        GoAwayFromTarget(bug);
                    }
                    break;

            }
		}

		/// <summary>
        /// Repeated if the ant spots at least one ant of an enemy colony.
		/// </summary>
        /// <param name="ant">Nearest enemy ant</param>
        public override void SpotsEnemy(Ant ant)
		{
            war = true;
            if (Caste == "KillerAnt" && war && FriendlyAntsFromSameCasteInViewrange > 5)
            {
                Attack(ant);
            }
		}

		/// <summary>
        /// Repeated if the ant is attacked by a bug.
		/// </summary>
        /// <param name="bug">Attacking bug</param>
        public override void UnderAttack(Bug bug)
        {
            switch (Caste)
            {
                case "SugarAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    Drop();
                    GoAwayFromTarget(bug);
                    break;
                case "KillerAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    Attack(bug);
                    break;
            }
		}

		/// <summary>
        /// Repeated if the ant is attacked by an ant of an enemy colony.
		/// </summary>
		/// <param name="ant">Attacking enemy ant</param>
        private bool war = false;
        public override void UnderAttack(Ant ant)
		{
            switch (Caste)
            {
                case "SugarAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    Drop();
                    GoAwayFromTarget(ant);
                    break;
                case "KillerAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    Attack(ant);
                    break;
            }
		}

		#endregion

		#region Misc

		/// <summary>
        /// Called once if the ant died.
		/// </summary>
        /// <param name="kindOfDeath">The KindOfDeath of the ant</param>
        public override void HasDied(KindOfDeath kindOfDeath)
		{
		}

		/// <summary>
		/// Called every round. Is not influenced by other environmental parameters.
		/// </summary>
		public override void Tick()
		{
            if (hasSugarTarget && CurrentSugar.Amount == 0)
            {
                CurrentSugar = null;
                hasSugarTarget = false;
                GoBackToAnthill();
            }
           if (CurrentLoad > 0 && Target is Anthill && CarringFruit == null)
            {
                MakeMark(Direction + 180);
            }
           if (CurrentLoad == 0 && !(Target is Anthill) && hasSugarTarget)
           {
               GoToTarget(CurrentSugar);
           }
		}

		#endregion
		 
	}
}