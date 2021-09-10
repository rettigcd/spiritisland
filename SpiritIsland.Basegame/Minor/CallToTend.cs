using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Speed.Slow,Element.Water,Element.Plant,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "remove 1 blight", () => ctx.RemoveBlight() ),
				new ActionOption( "push up to 3 dahan", () => ctx.PushUpToNDahan( 3 ) )
			);

		}

	}
}
