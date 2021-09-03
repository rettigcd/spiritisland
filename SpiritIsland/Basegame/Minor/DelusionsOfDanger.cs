using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DelusionsOfDanger {

		[MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
		[FromPresence(1,Target.Explorer)]
		static public Task ActionAsync(TargetSpaceCtx ctx){

			return ctx.SelectPowerOption(
				new PowerOption( "Push 1 Explorer", ctx => ctx.PushUpToNTokens( 1, Invader.Explorer ) ),
				new PowerOption( "2 fear", ctx => ctx.AddFear( 2 ) )
			);

		}

	}
}
