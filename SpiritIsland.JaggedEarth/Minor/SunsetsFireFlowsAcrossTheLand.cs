using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class SunsetsFireFlowsAcrossTheLand{ 

		[MinorCard("Sunset's Fire Flows Across the Land",1,Element.Sun,Element.Moon,Element.Fire,Element.Water),Slow,FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// 1 fear.
			ctx.AddFear(1);
			// 1 damage.
			await ctx.DamageInvaders(1);

			// You may pay 1 Energy to deal 1 Damage in an adjacent land.
			if( 1 <= ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Pay 1 energy to deal 1 Damage in an adjacent land?", "Yes, pay 1 energy for 1 damage", "No, thank you" )) {
				ctx.Self.Energy--;

				var adjInvaders = ctx.Adjacent.SelectMany( s => ctx.GameState.Tokens[s].Invaders().Select(t=>new SpaceToken(s,t) ) ).ToArray();
				var adjInvader = await ctx.Self.Action.Decision( new Decision.SpaceTokens("Select invader for 1 damage", adjInvaders, Present.Always ));
				await ctx.Target(adjInvader.Space).Invaders.ApplyDamageTo1(1,adjInvader.Token);
			}
		}
	}

}
