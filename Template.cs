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
        Name = "Default",
        SpeedModificator = 0,
        LoadModificator = 0,
        AttackModificator = 0,
        EnergyModificator = 0,
        RangeModificator = 0,
        ViewRangeModificator = 0,
        RotationSpeedModificator = 0
    )]

    

	public class DHBWAnt : BaseAnt
	{

        // Liste in die alle Ameisen eingetragen werden
        private List<DHBWAnt> Antlist = new List<DHBWAnt>();
        // jede Ameise erhält eine eigene ID
        private int AntID;

        #region FlockingBoids

        private int Rule1(int AntID)
        {

            // Für alle Ameisen die sich in Reichweite befinden wird Festgestellt, 
            // in welcher Richtung sie sich zum Individuum befindet
            // die Ameise versucht genau in die Gegenrichtung zu laufen
            // dieser Wert wird für alle Ameisen berechnet und ein Mittelwert zurückgegeben

            int DegreeSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                int Distance = Coordinate.GetDistanceBetween(this, Ant);
                if (Ant.getID() != AntID && (Distance < 40))
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

        private int Rule2(int AntID)
        {
            // Es wird der Mittelwert aus der Bewegunsrichtung aller Ameisen gebildet

            int DirectionSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                if (Ant.getID() != AntID)
                {
                    DirectionSum += Ant.Direction;
                }
            }
            int ListSize = Antlist.Count;
            return ListSize != 0 ? DirectionSum / Antlist.Count : DirectionSum / 1;
        }

        private int Rule3(int AntID)
        {
            int PositionSum = 0;
            foreach (DHBWAnt Ant in Antlist)
            {
                int DegreesBetween = Coordinate.GetDegreesBetween(this, Ant);
                if (Ant.getID() != AntID)
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

        private int BoidUpdate(int AntID)
        {
            int R1 = Rule1(AntID);
            int R2 = Rule1(AntID);
            int R3 = Rule1(AntID);
            int Weight1 = 60;
            int Weight2 = 30;
            int Weigth3 = 10;

            // die einzelnen Regeln können unterschiedlich gewichtet werden

            return (R1 * Weight1 + R2 * Weight2 + R3 * Weigth3) / (Weight1 + Weight2 + Weigth3);

        }

        #endregion

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
            
            return "Default";
        }

        #endregion

       

        #region Movement

        /// <summary>
        /// Repeated if the ant don't know where to go.
		/// </summary>
        public override void Waits()
        {

            if (CurrentLoad == 0)
            {

                if (AntID == 1)
                {
                    GoAhead(10);
                }
                else
                {
                    // nach 10 Schritten versucht die Ameise erneut, sie an die Boid Theorie zu halten
                    // mit diesem Wert lässt sich noch herumexperimentieren
                    TurnToDirection(BoidUpdate(this.getID()));
                    GoAhead(10);
                    
                }
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
            if (CurrentLoad == 0)
            {
                GoToTarget(sugar);
            }
		}

		/// <summary>
        /// Repeated if the ant spots at least one fruit.
		/// </summary>
        /// <param name="fruit">Nearest fruit</param>
		public override void Spots(Fruit fruit)
		{
            if (CurrentLoad == 0)
            {
                //MakeMark(0, 100);
                GoToTarget(fruit);
            }
		}

		/// <summary>
        /// Called once if the ant has reached the targeted sugar pile.
		/// </summary>
        /// <param name="sugar">Sugar pile</param>
        public override void TargetReached(Sugar sugar)
        {
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
            if (CurrentLoad == 0)
            {
                if (marker.Information == 0)
                {
                    GoToTarget(marker);
                }
                else
                {
                    //TurnToDirection(marker.Information);
                    //GoAhead(20);
                    GoToTarget(marker);
                }
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
            if (FriendlyAntsFromSameCasteInViewrange < 20)
            {
                GoToTarget(bug);
            }
            else
            {
                GoAwayFromTarget(bug);
            } 
            //GoAwayFromTarget(bug);
		}

		/// <summary>
        /// Repeated if the ant spots at least one ant of an enemy colony.
		/// </summary>
        /// <param name="ant">Nearest enemy ant</param>
        public override void SpotsEnemy(Ant ant)
		{
		}

		/// <summary>
        /// Repeated if the ant is attacked by a bug.
		/// </summary>
        /// <param name="bug">Attacking bug</param>
        public override void UnderAttack(Bug bug)
        {
            Attack(bug);
		}

		/// <summary>
        /// Repeated if the ant is attacked by an ant of an enemy colony.
		/// </summary>
		/// <param name="ant">Attacking enemy ant</param>
        public override void UnderAttack(Ant ant)
		{
           
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
           /* if (CurrentLoad > 0 && Target != null)
            {
                MakeMark(Direction + 180, 5);
                //MakeMark(1, 5);
            }*/
		}

		#endregion
		 
	}
}