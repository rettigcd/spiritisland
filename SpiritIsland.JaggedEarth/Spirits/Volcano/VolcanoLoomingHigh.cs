using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// !!! Mechanism for keeping presence in Mountain not in place for Power Card use

	public class VolcanoLoomingHigh : Spirit {

		public const string Name = "Volcano Looming High";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[]{MountainHome, CollapseInABlastOfLAvaAndSteam,VolcanicPeaksTowerOverTheLandscape};
		static readonly SpecialRule MountainHome = new SpecialRule("Mountain Home","Your presence may only be added/moved into Mountain.");
		static readonly SpecialRule CollapseInABlastOfLAvaAndSteam = new SpecialRule("Collapse in a Blast of Lava and Steam","When your presense is destroyed, in that land, deal 1 Damage per destroyed presence to both Invaders and to dahan.");
		static readonly SpecialRule VolcanicPeaksTowerOverTheLandscape = new SpecialRule("Volcanic Peaks Tower Over the Landscape","Your Power Cards gain +1 range if you have 3 or more presence in the origin land.");

		public VolcanoLoomingHigh():base(
			new VolcanoPresence(
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
				new GrowthOption(new PlacePresence(0,Target.Mountain), new PlacePresence(0,Target.Mountain)),
				new GrowthOption(new DrawPowerCard(), new PlacePresence(4,Target.Mountain), new PlayExtraCardThisTurn(), new GainEnergy(2))
			);

			InnatePowers = new InnatePower[] {
				InnatePower.For<ExplosiveEruption>(),
				InnatePower.For<PoweredByTheFurnaceOfTheEarth>()
			};

			((VolcanoPresence)(Presence)).spirit = this;

			TargetLandApi = new VolcanoTargetLandApi();
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// init special growth (note - we don't want this growth in Unit tests, so only add it if we call InitializeInternal())
			this.AddActionFactory(new Setup_PlacePresenceOnMountain());
		}

	}

	class VolcanoPresence : SpiritPresence {
		public Spirit spirit;
		public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {
			IsValid = (s) => s.Terrain == Terrain.Mountain;
		}

		public override async Task Destroy( Space space, GameState gs ) {
			// collapse in a blast of lava and steam -
			// when presence is destroyed, 1 damage to both invaders and dahan
			await base.Destroy( space, gs );
			Cause cause = Cause.None; // !!! None isn't correct, we would need to pass it in.
			await gs.DahanOn(space).ApplyDamage(1,cause); 
			await gs.Invaders.On(space,cause).UserSelectedDamage(1,spirit);
			
		}
	}

}
