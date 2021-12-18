using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Bringer : Spirit {

		public const string Name = "Bringer of Dreams and Nightmares";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] {
			new SpecialRule("TO DREAM A THOUSAND DEATHS","Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders")
		} ;

		public Bringer():base(
			new SpiritPresence(
				new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
				new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
			),
			PowerCard.For<CallOnMidnightsDream>(),
			PowerCard.For<DreadApparitions>(),
			PowerCard.For<DreamsOfTheDahan>(),
			PowerCard.For<PredatoryNightmares>()
		) {

			Growth = new(
				// reclaim, +1 power card
				new GrowthOption(new ReclaimAll(),new DrawPowerCard(1)),
				// reclaim 1, add presence range 0
				new GrowthOption(new Reclaim1(), new PlacePresence(0) ),
				// +1 power card, +1 pressence range 1
				new GrowthOption(new DrawPowerCard(1), new PlacePresence(1) ),
				// add presense range Dahan or Invadors, +2 energy
				new GrowthOption(new GainEnergy(2), new PlacePresence(4,Target.DahanOrInvaders))
			);

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<SpiritsMayYetDream>(),
				InnatePower.For<NightTerrors>()
			};

		}

		protected override void InitializeInternal( Board board, GameState gs ) {
			// Setup: 2 presense in highest numbered sands
			var startingIn = board.Spaces.Where(x=>x.Terrain==Terrain.Sand).Last();
			Presence.PlaceOn( startingIn, gs );
			Presence.PlaceOn( startingIn, gs );
		}

		/// <summary>
		/// Swaps out the effected tokens so real tokens don't get destoryed.
		/// Swaps out what happens when invaders get 'destroyed'
		/// </summary>
		public override InvaderBinding BuildInvaderGroupForPowers( GameState gs, Space space ) {
			var normalTokens = gs.Tokens[space];
			var detached = new TokenCountDictionary( normalTokens );

			return new InvaderBinding( 
				detached,
				new ToDreamAThousandDeaths_DestroyStrategy( 
					gs.Fear.AddDirect, 
//					Cause.Power,  
					new SelfCtx(this,gs,Cause.Power)
				),
				CustomDamageStrategy
			);
		}

		public override Task DestroyInvaderForPowers( GameState gs, Space space, int count, Token dahanToken ) {
			return Task.CompletedTask;
		}

	}

}
