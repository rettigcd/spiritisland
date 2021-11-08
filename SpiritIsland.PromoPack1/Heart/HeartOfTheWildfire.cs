using System.Linq;

namespace SpiritIsland.PromoPack1 {

	public class HeartOfTheWildfire : Spirit {

		static HeartOfTheWildfire() {
			SpaceFilterMap.Register(ThreateningFlames.BlightAndInvaders,ctx=>ctx.HasBlight && ctx.HasInvaders);
		}

		public const string Name = "Heart of the Wildfire";

		readonly static Track FirePresence = Track.MkElement(Element.Fire); // same as FireEnergy but we need separate object so user can distingquish top/bottom.

		public HeartOfTheWildfire() : base( 
			new HeartPresence(
				new PresenceTrack( Track.Energy0, Track.FireEnergy, Track.Energy1, Track.Energy2, new Track("", Element.Fire, Element.Plant ), Track.Energy3 ),
				new PresenceTrack( Track.Card1, FirePresence, Track.Card2, Track.Card3, FirePresence, Track.Card4 )
			)
			,PowerCard.For<AsphyxiatingSmoke>()
			,PowerCard.For<FlashFires>()
			,PowerCard.For<ThreateningFlames>()
			,PowerCard.For<FlamesFury>()
		) {
			((HeartPresence)Presence).spirit = this;
			InnatePowers = new InnatePower[] {
				InnatePower.For<FireStorm>(),
				InnatePower.For<TheBurnedLandRegrows>()
			};


			growthOptionGroup = new(
				new GrowthOption(
					new ReclaimAll(),
					new DrawPowerCard(1),
					new GainEnergy(1)
				),
				new GrowthOption(
					new DrawPowerCard(1),
					new PlacePresence(3)
				),
				new GrowthOption(
					new PlacePresence(1),
					new GainEnergy(2),
					new EnergyForFire()
				)
			);


		}

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] {
			new SpecialRule("BLAZING PRESENCE","After you add or move presence after Setup, in the land it foes to: For each fire showingo n your presence Tracks, do 1 Damage.  If 2 fire or more are showing on your presence Tracks, add 1 blight.  Push al beasts and any number of dahan.  Added blight does not destory your presence.")
		} ;

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Put 3 presence and 2 blight on your starting board in the hightest-numbered Sands. 
			var space = board.Spaces.Where(x=>x.Terrain==Terrain.Sand).Last();
			for(int i=0;i<3;++i)
				Presence.PlaceOn(space,gameState);

			gameState.Tokens[space][TokenType.Blight] += 2; // Blight goes from the box, not the blight card
		}
	}



}
