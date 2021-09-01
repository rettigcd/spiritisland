using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToIsolation {

		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task Act(TargetSpaceCtx ctx){

			int pushCount = ctx.DahanCount; // push 1 explorer/town per dahan

			bool pushDahan = !ctx.PowerInvaders.Counts.Has(Invader.Explorer) && !ctx.PowerInvaders.Counts.Has(Invader.Town)
				|| await ctx.Self.UserSelectsFirstText("Select option", "push 1 dahan", $"push {pushCount} explorer or towns");

			if( pushDahan )
				await ctx.PowerPushUpToNDahan(1);
			else
				// push 1 explorer/town per dahan
				await ctx.PowerPushUpToNInvaders(pushCount,Invader.Town,Invader.Explorer);

		}

	}
}
