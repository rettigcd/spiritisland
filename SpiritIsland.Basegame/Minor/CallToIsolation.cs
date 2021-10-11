using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToIsolation {

		[MinorCard("Call to Isolation",0,Element.Sun,Element.Air,Element.Animal)]
		[Fast]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			return ctx.SelectActionOption(
				new ActionOption( "push 1 dahan", () => ctx.PushDahan( 1 ) ),
				new ActionOption( "push {pushCount} explorer or towns", () => ctx.Push( ctx.DahanCount, Invader.Town, Invader.Explorer ) )
			);
		}

	}
}
