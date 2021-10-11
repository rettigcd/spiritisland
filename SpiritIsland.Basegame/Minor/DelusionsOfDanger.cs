using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DelusionsOfDanger {

		[MinorCard("Delusions of Danger",1,Element.Sun,Element.Moon,Element.Air)]
		[Fast]
		[FromPresence(1,Target.Any)]
		static public Task ActionAsync(TargetSpaceCtx ctx){

			return ctx.SelectActionOption(
				new ActionOption( "Push 1 Explorer", () => ctx.Push( 1, Invader.Explorer ) ),
				new ActionOption( "2 fear", () => ctx.AddFear( 2 ) )
			);

		}

	}
}
