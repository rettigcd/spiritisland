using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToIsolation {

		[MinorCard("Call to Isolation",0,Element.Sun,Element.Air,Element.Animal)]
		[Fast]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){

			return ctx.SelectActionOption(
				Cmd.PushNDahan(1),
				Cmd.PushExplorersOrTowns( ctx.Dahan.Count )
			);

		}

	}
}
