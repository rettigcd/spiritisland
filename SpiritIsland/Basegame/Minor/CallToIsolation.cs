using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class CallToIsolation {

		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (_,gameState) = ctx;

			var grp = gameState.InvadersOn(target);
			int pushCount = gameState.GetDahanOnSpace(target); // push 1 explorer/town per dahan

			bool pushDahan = !grp.HasExplorer && !grp.HasTown
				|| await ctx.Self.SelectFirstText("Select option", "push 1 dahan", $"push {pushCount} explorer or towns");

			if( pushDahan )
				await ctx.PushUpToNDahan(target,1);
			else
				// push 1 explorer/town per dahan
				await ctx.PushUpToNInvaders(target,pushCount,Invader.Town,Invader.Explorer);

		}

	}
}
