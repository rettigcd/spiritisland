using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiritIsland.JaggedEarth {

	public class StonesUnyieldingDefiance : Spirit {

		public const string Name = "Stone's Unyielding Defiance";
		public override string Text => Name;

		public override string SpecialRules => "Bestorw the Undurance of BedRock - When blight is added to one of your lands, unless the blight then outnumbers your presence, it does not cascade or destory presence (yours or others')."
			+ "   Deep Layers Expposed to the Surface - The first time you uncover each of your +1 Card Play presence spaces, gain a Minor Power.";


		static Track AddCardPlay => new Track( "PlayExtraCardThisTurn" ) { Action = new DrawMinorOnceAndPlayExtraCardThisTurn() }; // ! Must create separate instances so the draw Minor card works
		static Track EarthReclaim => new Track( "earth energy", Element.Earth ) { Action = new Reclaim1() };

		public StonesUnyieldingDefiance() : base(
			new SpiritPresence(
				new PresenceTrack( Track.Energy2, Track.Energy3, AddCardPlay, Track.Energy4, AddCardPlay, Track.Energy6, AddCardPlay ),
				new PresenceTrack( Track.Card1, Track.EarthEnergy, Track.EarthEnergy, EarthReclaim, Track.MkElement(Element.Earth,Element.Any), new Track("2 cardplay,earth", Element.Earth){ CardPlay=2 } )
			)
			,PowerCard.For<JaggedShardsPushFromTheEarth>()
			,PowerCard.For<PlowsShatterOnRockyGround>()
			,PowerCard.For<ScarredAndStonyLand>()
			,PowerCard.For<StubbornSolidity>()
		) {

			this.growthOptionGroup = new GrowthOptionGroup(
				new GrowthOption(new ReclaimAll(),new PlacePresence(3,Target.MountainOrPresence),new GainElement(Element.Earth,Element.Earth)),
				new GrowthOption(new PlacePresence(2), new GainEnergy(3)),
				new GrowthOption(new DrawPowerCard(), new PlacePresence(1))
			);
			InnatePowers = new InnatePower[] {
				InnatePower.For<HoldTheIslandFastWithABulwarkOfWill>(),
				InnatePower.For<LetThemBreakThemselvesAgainstTheStone>()
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// place presence in lowest-numbered Mountain without dahan
			var space = board.Spaces.Skip(1).Where(s=>gameState.Tokens[s].Dahan.Count==0).First();
			Presence.PlaceOn(space);
			var space2 = space.Adjacent.FirstOrDefault(s=>gameState.Tokens[s][TokenType.Blight]>0)
				?? space.Adjacent.First(s=>s.Terrain==Terrain.Sand);
			Presence.PlaceOn(space2);

		}

	}

}
