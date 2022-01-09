using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("SWIFTNESS OF LIGHTNING", "For every Simple air you have, you may use 1 Slow Power as if it were fast") };

		public LightningsSwiftStrike():base(
			new SpiritPresence(
				new PresenceTrack( Track.Energy1, Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
				new PresenceTrack( Track.Card2, Track.Card3, Track.Card4, Track.Card5, Track.Card6 )
			),
			PowerCard.For<HarbingersOfTheLightning>(),
			PowerCard.For<LightningsBoon>(),
			PowerCard.For<RagingStorm>(),
			PowerCard.For<ShatterHomesteads>()
		){
			Growth = new(
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
				new GrowthOption( new GainEnergy(3), new PlacePresence(1) )
			);

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<ThunderingDestruction>()
			};

		}

		protected override PowerProgression GetPowerProgression() =>
			new(
				PowerCard.For<DelusionsOfDanger>(),
				PowerCard.For<CallToBloodshed>(),
				PowerCard.For<PowerStorm>(),
				PowerCard.For<PurifyingFlame>(),
				PowerCard.For<PillarOfLivingFlame>(),
				PowerCard.For<EntrancingApparitions>(),
				PowerCard.For<CallToIsolation>()
			);


		public override string Text => Name;

		protected override void InitializeInternal( Board board, GameState gs ) {
			// Setup: put 2 pressence in highest numbered sands
			var space = board.Spaces.Reverse().First(x=>x.IsSand);
			Presence.PlaceOn(space,gs);
			Presence.PlaceOn(space,gs);

			gs.TimePasses_WholeGame += Gs_TimePassed;
		}

		private void Gs_TimePassed( GameState obj ) {
			usedAirForFastCount = 0;
		}

		public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {

			bool canMakeSlowFast = speed == Phase.Fast 
				&& Elements[Element.Air] > usedAirForFastCount;

			foreach(var h in AvailableActions)
				if(IsActiveDuring( speed, h ) || canMakeSlowFast && IsActiveDuring( Phase.Slow, h ) )
					yield return h;

		}

		public override Task TakeAction( IActionFactory factory, SelfCtx ctx ) {

			// we can decrement any time a slow card is used,
			// even during slow because we no longer care about this
			if(ctx.GameState.Phase == Phase.Fast
				&& factory.CouldActivateDuring( Phase.Slow, this )
				&& factory is IFlexibleSpeedActionFactory flexSpeedFactory
			) {
				++usedAirForFastCount;
				TemporarySpeed.Override( flexSpeedFactory, Phase.Fast, ctx.GameState );
			}

			return base.TakeAction(factory,ctx);
		}


		int usedAirForFastCount = 0;

	}

}
