using AntMe.English;
using System;
using System.Collections.Generic;

namespace AntMe.Player.DHBW {

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
        SpeedModificator = 0,
        LoadModificator = -1,
        AttackModificator = 2,
        EnergyModificator = 1,
        RangeModificator = -1,
        ViewRangeModificator = 0,
        RotationSpeedModificator = -1
    )]

    [Caste(
        Name = "SugarAnt",
        SpeedModificator = 1,
        LoadModificator = 2,
        AttackModificator = -1,
        EnergyModificator = 0,
        RangeModificator = -1,
        ViewRangeModificator = 0,
        RotationSpeedModificator = -1
    )]



    public class DHBWAnt : BaseAnt {
        /* variables for all ants */
        private static List<DHBWAnt> antList = new List<DHBWAnt>(); // contains all ants

        /* variables only for this ant */
        private int antID; // ID of this ant
        private Sugar currentSugar;

        /* constants */
        private int MARKER_FIGHT = 1; // information for fight-Marker
        private double ratioSugarAnts = 0.75; // % of all ants should be SugarAnts
        private int standardSteps = 20; // standard number of steps an ant should go

        #region FlockingBoids

        // weighting variables for Boid-Rules
        private int weightSeparation = 50;
        private int weightAlignment = 30;
        private int weightCohesion = 20;

        private int Separation() {

            // Für alle Ameisen die sich in Reichweite befinden wird Festgestellt, 
            // in welcher Richtung sie sich zum Individuum befindet
            // die Ameise versucht genau in die Gegenrichtung zu laufen
            // dieser Wert wird für alle Ameisen berechnet und ein Mittelwert zurückgegeben

            int degreeSum = 0;
            foreach(DHBWAnt ant in antList) {
                int distance = Coordinate.GetDistanceBetween(this, ant);
                if(ant.getID() != getID() && (distance < 40) && hasSameCaste(ant)) {
                    degreeSum += (distance - 180);
                }
            }
            if(degreeSum == 0) {
                degreeSum = new Random().Next(0, 180);
            }
            int listSize = antList.Count;
            return listSize != 0 ? degreeSum / antList.Count : degreeSum / 1;
        }

        private int Alignment() {
            // Es wird der Mittelwert aus der Bewegunsrichtung aller Ameisen gebildet

            int directionSum = 0;
            foreach(DHBWAnt ant in antList) {
                if(ant.getID() != getID() && hasSameCaste(ant)) {
                    directionSum += ant.Direction;
                }
            }
            int listSize = antList.Count;
            return listSize != 0 ? directionSum / antList.Count : directionSum / 1;
        }

        private int Cohesion() {
            int positionSum = 0;
            foreach(DHBWAnt ant in antList) {
                int degreesBetween = Coordinate.GetDegreesBetween(this, ant);
                if(ant.getID() != getID() && hasSameCaste(ant)) {
                    positionSum += degreesBetween;
                }
            }
            if(positionSum == 0) {
                positionSum = 180;
            }
            int listSize = antList.Count;
            return listSize != 0 ? positionSum / antList.Count : positionSum / 1;
        }

        private int BoidUpdate() {
            return (Separation() * weightSeparation + Alignment() * weightAlignment + Cohesion() * weightCohesion) / 100;
        }

        #endregion


        #region Castes

        /// <summary>
        /// Chooses a specific caste.
        /// </summary>
        /// <param name="typeCount">List of available ants from caste</param>
        /// <returns>Name of chosen caste</returns>
        public override string ChooseType(Dictionary<string, int> typeCount) {
            setID(antList.Count + 1);
            antList.Add(this);
            return (getSugarAntDistribution() < ratioSugarAnts) ? "SugarAnt" : "KillerAnt";
        }

        #endregion



        #region Movement

        /// <summary>
        /// Repeated if the ant don't know where to go.
        /// </summary>
        public override void Waits() {
            if(CurrentLoad == 0) {
                TurnToDirection(BoidUpdate());
                GoAhead(standardSteps);
            }
        }


        /// <summary>
        /// Called once if the ant has exceeded 1/3 of its maximum range.
        /// </summary>
        public override void BecomesTired() {
            if(CurrentLoad == 0) GoBackToAnthill();
        }

        #endregion

        #region Food

        /// <summary>
        /// Repeated if the ant spots at least one sugar pile.
        /// </summary>
        /// <param name="sugar">Nearest sugar pile</param>
        public override void Spots(Sugar sugar) {
            MakeMark(0, 160);
            MakeMark(sugar);
            switch(Caste) {
                case "SugarAnt":
                    if(currentSugar == null) {
                        currentSugar = sugar;
                        GoToTarget(sugar);
                    }
                    break;
                case "KillerAnt":
                    break;
            }
        }

        /// <summary>
        /// Repeated if the ant spots at least one fruit.
        /// </summary>
        /// <param name="fruit">Nearest fruit</param>
        public override void Spots(Fruit fruit) {
            MakeMark(0, 160);
            MakeMark(fruit);
            switch(Caste) {
                case "SugarAnt":
                    if(CurrentLoad == 0) GoToTarget(fruit);
                    break;
                case "KillerAnt":
                    break;
            }
        }

        /// <summary>
        /// Called once if the ant has reached the targeted sugar pile.
        /// </summary>
        /// <param name="sugar">Sugar pile</param>
        public override void TargetReached(Sugar sugar) {
            Take(sugar);
            GoBackToAnthill();
        }

        /// <summary>
        /// Called once if the ant has reached the targeted fruit.
        /// </summary>
        /// <param name="fruit">Fruit</param>
        public override void TargetReached(Fruit fruit) {
            if(NeedsCarrier(fruit)) {
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
        public override void SmellsFriend(Marker marker) {
            switch(Caste) {
                case "SugarAnt":
                    if(Target == null) {
                        TurnToDirection(marker.Information);
                        GoAhead();
                    }
                    break;
                case "KillerAnt":
                    if(Target == null && marker.Information == MARKER_FIGHT) {
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
        public override void SpotsFriend(Ant ant) {

        }

        /// <summary>
        /// Called if the ant spots a friendly ant of another team.
        /// </summary>
        /// <param name="ant">Nearest friendly ant of another team</param>
        public override void SpotsPartner(Ant ant) {
        }

        #endregion

        #region Fight

        /// <summary>
        /// Repeated if the ant spots at least one bug.
        /// </summary>
        /// <param name="bug">Nearest bug</param>
        public override void SpotsEnemy(Bug bug) {
            switch(Caste) {
                case "SugarAnt":
                    if(FriendlyAntsInViewrange > 10 && currentSugar == null) Attack(bug);
                    break;
                case "KillerAnt":
                    MakeMark(MARKER_FIGHT, 100);
                    if(FriendlyAntsFromSameCasteInViewrange > 5
                        && 10 * MaximumEnergy > bug.CurrentEnergy) {
                        MakeMark(bug);
                        Attack(bug);

                    } else {
                        GoAwayFromTarget(bug);
                    }
                    break;

            }
        }

        /// <summary>
        /// Repeated if the ant spots at least one ant of an enemy colony.
        /// </summary>
        /// <param name="ant">Nearest enemy ant</param>
        public override void SpotsEnemy(Ant ant) {
            MakeMark(ant);
            switch(Caste) {
                case "SugarAnt":
                    if(currentSugar == null) GoAwayFromTarget(ant);
                    break;
                case "KillerAnt":
                    if(FriendlyAntsFromSameCasteInViewrange > 5
                        && 10 * MaximumEnergy > ant.CurrentEnergy
                        && MaximumSpeed > ant.MaximumSpeed) {
                        Attack(ant);
                    }
                    break;
            }
        }

        /// <summary>
        /// Repeated if the ant is attacked by a bug.
        /// </summary>
        /// <param name="bug">Attacking bug</param>
        public override void UnderAttack(Bug bug) {
            Drop();
            MakeMark(bug);
            if(FriendlyAntsInViewrange > 5) {
                Attack(bug);
            } else {
                GoAwayFromTarget(bug);
            }
        }

        /// <summary>
        /// Repeated if the ant is attacked by an ant of an enemy colony.
        /// </summary>
        /// <param name="ant">Attacking enemy ant</param>
        public override void UnderAttack(Ant ant) {
            switch(Caste) {
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
        public override void HasDied(KindOfDeath kindOfDeath) {
        }

        /// <summary>
        /// Called every round. Is not influenced by other environmental parameters.
        /// </summary>
        public override void Tick() {
            switch(Caste) {
                case "SugarAnt":
                    // if current sugar pile is empty go back to anthill
                    if(currentSugar != null && currentSugar.Amount <= 0) {
                        currentSugar = null;
                        GoBackToAnthill();
                    }

                    // if no load and has a sugar pile as target go to that pile
                    if(CurrentLoad == 0 && currentSugar != null) {
                        GoToTarget(currentSugar);
                    }

                    // create an ant street
                    if(CurrentLoad > 0 && Target is Anthill && CarringFruit == null) {
                        MakeMark(Direction + 180);
                    }

                    // if carrying a fruit go back to anthill
                    if(CarringFruit != null) {
                        GoBackToAnthill();
                    }
                    break;
                case "KillerAnt":

                    break;
            }
        }

        /* make a Mark pointing towards an Item */
        private void MakeMark(Item item) {
            int direction = Coordinate.GetDegreesBetween(this, item);
            int distance = Coordinate.GetDistanceBetween(this, item);
            MakeMark(direction, distance);
        }

        /* return true if the other ant has the same caste as this ant */
        private bool hasSameCaste(DHBWAnt ant) {
            return (Caste.Equals(ant.Caste));
        }

        /* return ration on SugarAnts */
        private double getSugarAntDistribution() {
            double sugarAnts = 0;
            foreach(DHBWAnt ant in antList) {
                if(ant.Caste.Equals("SugarAnt")) sugarAnts++;
            }
            double listSize = (double) antList.Count;
            System.Diagnostics.Debug.WriteLine("Ratio: " + sugarAnts / listSize);
            return sugarAnts / listSize;
        }

        /* return ID of this ant */
        public int getID() {
            return this.antID;
        }

        /* set ID of this ant */
        private void setID(int id) {
            this.antID = id;
        }

        #endregion

    }
}