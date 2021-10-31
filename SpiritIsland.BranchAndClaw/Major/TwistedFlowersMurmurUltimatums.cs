using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TwistedFlowersMurmurUltimatums {

        [MajorCard("Twisted Flowers Murmur Ultimatums", 4, Element.Sun, Element.Moon, Element.Fire, Element.Air, Element.Water, Element.Earth, Element.Plant, Element.Animal)]
		[Slow]
        [FromPresence(0)]
        static public async Task ActAsync(TargetSpaceCtx ctx) {

			// 4 fear
			ctx.AddFear(4);
			
			// add 1 strife
			await ctx.AddStrife();

			// if you have 3moon, 2 air, 3 plant (before the terror level check)
			if(await ctx.YouHave( "3 moon,2 air,3 plant" )) {
				ctx.AddFear( 3 );
				await ctx.DamageInvaders( 3 );
			}

			// if terror level is 2 or higher, remove 2 invaders
			if(2 <= ctx.GameState.Fear.TerrorLevel)
				for(int i = 0; i < 2; ++i) {
					var invader = await ctx.Self.Action.Decision(new Decision.InvaderToDowngrade(ctx.Space,ctx.Tokens.Invaders(),Present.Always));
					ctx.Tokens[invader]--;
				}

        }

    }
}
