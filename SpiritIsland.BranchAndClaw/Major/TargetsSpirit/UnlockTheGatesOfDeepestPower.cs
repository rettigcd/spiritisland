using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnlockTheGatesOfDeepestPower {

		[MajorCard( "Unlock the Gates of Deepenst Power", 4, Speed.Fast, Element.Sun,Element.Moon,Element.Fire,Element.Air,Element.Water,Element.Earth,Element.Plant,Element.Animal )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target Spirit gains a major power by drawing 2 and keeping 1, without having to forget another power card
			var card = await ctx.Target.CardDrawer.DrawMajor( ctx.Target, ctx.GameState, null, 2 );
			ctx.Target.AddCardToHand( card );

			// if 2 of each element,
			if(ctx.Self.Elements.Contains("2 sun,2 moon,2 fire,2 air,2 water,2 earth,2 plant,2 animal" )) {
				// target spirit may now play the major power they keep by:
				//    * paying half its cost (round up) OR
				int cost = card.Cost/2;
				var payingHalfCostOption = new ActionOption(
					$"paying {cost}", 
					()=> { 
						ctx.Target.AddActionFactory(card); 
						ctx.Target.Energy -= cost;
					},
					cost <= ctx.Target.Energy
				);
				//    * forgetting it at the end of turn.
				var forgettingCardOption = new ActionOption( 
					$"forgetting at end of turn", 
					() => {
						ctx.Target.AddActionFactory( card );
						ctx.GameState.TimePasses_ThisRound.Push( ( _ ) => { 
							ctx.Target.Forget(card); 
							return Task.CompletedTask; 
						} );
					}
				);
				// It gains all elmemental thresholds.

				await ctx.Target.SelectOptionalAction( $"Play {card.Name} now by:",payingHalfCostOption, forgettingCardOption );
			}

		}

	}

}
