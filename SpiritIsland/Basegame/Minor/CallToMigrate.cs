using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class CallToMigrate {

		[MinorCard("Call to Migrate",1,Speed.Slow,Element.Fire,Element.Air,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			var target = ctx.Target;
			await ctx.GatherUpToNDahan(target,3);
			await ctx.PushUpToNDahan(target,3);
		}

	}
}
