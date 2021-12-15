using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Element.Water,Element.Plant,Element.Animal)]
		[Slow]
		[FromPresence(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new SpaceAction( "remove 1 blight", ctx => ctx.RemoveBlight() ),
				new SpaceAction( "push up to 3 dahan", ctx => ctx.PushUpToNDahan( 3 ) )
			);

		}

	}
}
