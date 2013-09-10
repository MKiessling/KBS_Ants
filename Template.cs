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

        /// <summary>
        /// Chooses a specific caste.
        /// </summary>
        /// <param name="typeCount">List of available ants from caste</param>
        /// <returns>Name of chosen caste</returns>
        public override string ChooseType(Dictionary<string, int> typeCount)
        {
            return "Default";
        }

        #endregion

        #region Movement

        /// <summary>
        /// Repeated if the ant don't know where to go.
		/// </summary>
		public override void Waits()
		{
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