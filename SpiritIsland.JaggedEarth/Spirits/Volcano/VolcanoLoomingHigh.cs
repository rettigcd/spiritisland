namespace SpiritIsland.JaggedEarth {

	public class VolcanoLoomingHigh : Spirit {

		public const string Name = "Volcano Looming High";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[]{MountainHome, CollapseInABlastOfLAvaAndSteam,VolcanicPeaksTowerOverTheLandscape};
		static readonly SpecialRule MountainHome = new SpecialRule("Mountain Home","Your presence may only be added/moved into Mountain.");
		static readonly SpecialRule CollapseInABlastOfLAvaAndSteam = new SpecialRule("Collapse in a Blast of Lava and Steam","When your presense is destroyed, in that land, deal 1 Damage per destroyed presence to both Invaders and to dahan.");
		static readonly SpecialRule VolcanicPeaksTowerOverTheLandscape = new SpecialRule("Volcanic Peaks Tower Over the Landscape","Your Power Cards gain +1 range if you have 3 or more presence in the origin land.");

		public VolcanoLoomingHigh():base(
			new SpiritPresence(
				new PresenceTrack(Track.Energy1,Track.Energy2,Track.EarthEnergy,Track.Energy3,Track.Energy4,Track.Energy5),
				new PresenceTrack(Track.Card1,Track.FireEnergy,Track.MkElement(Element.Fire),Track.Card2,Track.AirEnergy,Track.Card3,Track.FireEnergy,Track.Card4)
			)
			,PowerCard.For<ExaltationOfMoltenStone>()
			,PowerCard.For<LavaFlows>()
			,PowerCard.For<PyroclasticBombardment>()
			,PowerCard.For<RainOfAsh>()
		) {
			growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(), new DrawPowerCard(), new GainEnergy(3)),
				new GrowthOption(new PlacePresence(0), new PlacePresence(0)),
				new GrowthOption(new DrawPowerCard(), new PlacePresence(4), new PlayExtraCardThisTurn(), new GainEnergy(2))
			);

			InnatePowers = new InnatePower[] {
				InnatePower.For<ExplosiveEruption>(),
				InnatePower.For<PoweredByTheFurnaceOfTheEarth>()
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 1 presence on your starting board in a bountain of your choice.
			// !!!
			// Push all dahan from that land.

		}

	}

	// !!! Mountain Home - can only move presence in Mountains

	// !!! collapse in a blast of lava and steam - when presence is destroyed, 1 damage to both invaders and dahan

	// !!! Power Cards gain +1 range if you ahve 3 or more presence in the original land.

	// !!! Many Minds - Move allows Targetting ocean.

}
