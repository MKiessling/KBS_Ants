using System;
using System.Collections.Generic;

using AntMe.English;

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

        #region Castes
        private static LinkedList<DHBWAnt> Antlist;
        // Liste in die alle Ameisen eingetragen werden
        private int Ant_ID;
        // jede Ameise erhält eine eigene ID

        public int getID()
        {
            return Ant_ID;
        }
        /// <summary>
        /// Chooses a specific caste.
        /// </summary>
        /// <param name="typeCount">List of available ants from caste</param>
        /// <returns>Name of chosen caste</returns>
        public override string ChooseType(Dictionary<string, int> typeCount)
        {

         

            if (Antlist == null)
            {
                Antlist = new LinkedList<DHBWAnt>();
                Ant_ID = 1;
                Antlist.AddLast(this);
                // 1. Ameise wird erstellt
            }
            else
            {
                Ant_ID = Antlist.Last.Value.getID() + 1;
                Antlist.AddLast(this);
                //weitere Ameisen werden erstellt
            }
            
            return "Default";
        }

        #endregion
        #region FlockingBoids

        private int rule1(int ID)
        {

            // Für alle Ameisen die sich in Reichweite befinden wird Festgestellt, 
            // in welcher Richtung sie sich zum Individuum befindet
            // die Ameise versucht genau in die Gegenrichtung zu laufen
            // dieser Wert wird für alle Ameisen berechnet und ein Mittelwert zurückgegeben


            LinkedList<DHBWAnt> copy = new LinkedList<DHBWAnt>();
            int degreeSum = 0;
            int antSum = 0;
            
            while (Antlist.Count > 0)
            {
                if (Antlist.First.Value.Ant_ID != this.getID() && (Coordinate.GetDistanceBetween(this, Antlist.First.Value) < 40))
                {
                    degreeSum = degreeSum + (Coordinate.GetDegreesBetween(this, Antlist.First.Value) - 180);
                    antSum++;
                }
                copy.AddLast(Antlist.First.Value);
                Antlist.RemoveFirst();
            }
            if (antSum == 0) { antSum = 1; }
            Antlist = copy;
            if (degreeSum == 0)
            {
                // es kann vorkommen das keine Ameisen in der nähe sind, dann sollte hier ein Zufallswert stehen
                degreeSum = 180; 
            }
            return degreeSum / antSum;
        }

        private int rule2(int ID)
        {
            // Es wird der Mittelwert aus der Bewegunsrichtung aller Ameisen gebildet
            
            LinkedList<DHBWAnt> copy = new LinkedList<DHBWAnt>();
            int directionSum = 0;
            int antSum = 0;

            while (Antlist.Count > 0)
            {
                if (Antlist.First.Value.Ant_ID != this.getID())
                {
                    directionSum = directionSum + Antlist.First.Value.Direction;
                    antSum++;
                }
                copy.AddLast(Antlist.First.Value);
                Antlist.RemoveFirst();
            }
            if (antSum == 0) { antSum = 1; }
            Antlist = copy;

            return directionSum / antSum;
        }

        private int rule3(int ID)
        {

            // Es wird ein Mittelwert aus den Positionene aller Ameisen gebildet
            LinkedList<DHBWAnt> copy = new LinkedList<DHBWAnt>();
            int positionSum = 0;
            int antSum = 0;

            while (Antlist.Count > 0)
            {
                if (Antlist.First.Value.Ant_ID != this.getID())
                {
                    positionSum = positionSum + (Coordinate.GetDegreesBetween(this, Antlist.First.Value));
                    antSum++;
                }
                copy.AddLast(Antlist.First.Value);
                Antlist.RemoveFirst();
            }
            if (antSum == 0) { antSum = 1; }
            Antlist = copy;
            if (positionSum == 0)
            {
                positionSum = 180;
            }
            return positionSum / antSum;
        }

        private int boidUpdate(int ID)
        {
            int r1 = rule1(ID);
            int r2 = rule1(ID);
            int r3 = rule1(ID);
            int weight1 = 60;
            int weight2 = 30;
            int weigth3 = 10;

            // die einzelnen Regeln können unterschiedlich gewichtet werden

            return (r1 * weight1 + r2 * weight2 + r3 * weigth3) / (weight1+weight2+weigth3);

        }


        #endregion
        #region Movement

        /// <summary>
        /// Repeated if the ant don't know where to go.
		/// </summary>
        public override void Waits()
        {




            if (Ant_ID == 1)
            {
                GoAhead(10);
            }
            else
            {
                TurnToDirection(boidUpdate(this.getID()));
                GoAhead(10);
                // nach 10 Schritten versucht die Ameise erneut, sie an die Boid Theorie zu halten
                // mit diesem Wert lässt sich noch herumexperimentieren
            }
        }
		

		/// <summary>
        /// Called once if the ant has exceeded 1/3 of its maximum range.
		/// </summary>
		public override void BecomesTired()
		{
		}

		#endregion

		#region Food

		/// <summary>
        /// Repeated if the ant spots at least one sugar pile.
		/// </summary>
        /// <param name="sugar">Nearest sugar pile</param>
        public override void Spots(Sugar sugar)
		{
		}

		/// <summary>
        /// Repeated if the ant spots at least one fruit.
		/// </summary>
        /// <param name="fruit">Nearest fruit</param>
		public override void Spots(Fruit fruit)
		{
		}

		/// <summary>
        /// Called once if the ant has reached the targeted sugar pile.
		/// </summary>
        /// <param name="sugar">Sugar pile</param>
        public override void TargetReached(Sugar sugar)
		{
		}

		/// <summary>
        /// Called once if the ant has reached the targeted fruit.
		/// </summary>
        /// <param name="fruit">Fruit</param>
        public override void TargetReached(Fruit fruit)
		{
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
		}

		#endregion
		 
	}
}