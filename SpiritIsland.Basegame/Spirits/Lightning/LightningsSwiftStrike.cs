using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

/*
===================================================
Lightning's Swift Strike

 * reclaim, +1 power card, +1 energy
* +1 presence range 2, +1 presence range 0
* +1 presense range 1, +3 energy

1 1 2 2 3 4 4 5
2 3 4 5 6

Special Rules: Switftnes of Lightning - for every air you have, you may use 1 slow power as if it were fast (cards or innate)

Ligning's Boon => 1 => fast, any spirt => fire, air => Taret spirit may use up to 2 powers as if they were fast powers this turn.
Harbinger of the Lighning => 0 => slow, range 1, any => fire, air => Push up to 2 dahan.  1 fear if you pushed any dahan into a land with town or city.
Shatter homesteads => 2 => slow, range 2 from sacred site, any => fire, air => 1 fear.  Destroy 1 town
Raging Storm => 3 => slow, range 1, any => fire, air, water => 1 damange to each invader.

*/

	public class LightningsSwiftStrike : Spirit {
		public const string Name = "Lightning's Swift Strike";

		public override string SpecialRules => "SWIFTNESS OF LIGHTNING - For every Simple air you have, you may use 1 Slow Power as if it were fast";

		public LightningsSwiftStrike():base(
			new MyPresence(
				new PresenceTrack( Track.Energy1, Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
				new PresenceTrack( Track.Card2, Track.Card3, Track.Card4, Track.Card5, Track.Card6 )
			),
			PowerCard.For<HarbingersOfTheLightning>(),
			PowerCard.For<LightningsBoon>(),
			PowerCard.For<RagingStorm>(),
			PowerCard.For<ShatterHomesteads>()
		){
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( 
					new ReclaimAll(), 
					new DrawPowerCard(1), 
					new GainEnergy(1)
				),
				// +1 presence range 2, +1 presence range 0( 
				new GrowthOption(
					new PlacePresence(2),
					new PlacePresence(0) 
				),
				// +1 presense range 1, +3 energy
				new GrowthOption( new GainEnergy(3), new PlacePresence(1) ),
			};

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<ThunderingDestruction>()
			};

		}

		protected override PowerProgression GetPowerProgression() =>
			new PowerProgression(
				PowerCard.For<DelusionsOfDanger>(),
				PowerCard.For<CallToBloodshed>(),
				PowerCard.For<PowerStorm>(),
				PowerCard.For<PurifyingFlame>(),
				PowerCard.For<PillarOfLivingFlame>(),
				PowerCard.For<EntrancingApparitions>(),
				PowerCard.For<CallToIsolation>()
			);


		public override string Text => Name;

//		public override void PurchaseAvailableCards( params PowerCard[] cards ) {
//			base.PurchaseAvailableCards( cards );
////			swiftness.OnActivateCards( this );
//		}

		protected override void InitializeInternal( Board board, GameState gs ) {
			// Setup: put 2 pressence in highest numbered sands
			var space = board.Spaces.Reverse().First(x=>x.Terrain==Terrain.Sand);
			Presence.PlaceOn(space);
			Presence.PlaceOn(space);

			gs.TimePassed += Gs_TimePassed;
		}

		private void Gs_TimePassed( GameState obj ) {
			usedAirForFastCount = 0;
		}

		const string SwiftnessOfLightning = "Swiftness of Lightning";

		public override IEnumerable<IActionFactory> GetAvailableActions( Speed speed ) {
			var availableActions = AvailableActions.ToArray();

			// Update each default
			foreach(var action in availableActions)
				action.UpdateFromSpiritState( this.Elements );

			// in Fast phase
			if(speed == Speed.Fast){
				SpeedOverride slowOverride = Elements[Element.Air] > usedAirForFastCount ? new SpeedOverride(Speed.FastOrSlow, SwiftnessOfLightning ) : null;
				foreach(var action in availableActions)
					if(action.DefaultSpeed == Speed.Slow)
						action.OverrideSpeed = slowOverride;
			}

			return AvailableActions.Where( GetFilter( speed ) );
		}

		public override Task TakeAction( IActionFactory factory, GameState gameState ) {
			// check if we are using up an air
			// Only slow cards should get the override
			// If the card was used Slow, it just may increment higher than Air count
			if(factory.OverrideSpeed != null && factory.OverrideSpeed.Source == SwiftnessOfLightning)
				++usedAirForFastCount;

			return base.TakeAction(factory,gameState);
		}


		int usedAirForFastCount = 0;

	//	static readonly SwiftnessOfLightning swiftness = new SwiftnessOfLightning();

	}

}
