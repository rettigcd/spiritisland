using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToIsolation {

		[MinorCard("Call to Isolation",0,Element.Sun,Element.Air,Element.Animal)]
		[Fast]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			return ctx.SelectActionOption(
				new SpaceAction( "push 1 dahan", ctx => ctx.PushDahan( 1 ) ),
				new SpaceAction( $"push {ctx.Dahan.Count} explorer or towns", ctx => ctx.Push( ctx.Dahan.Count, Invader.Town, Invader.Explorer ) )
			);
		}

	}
}
