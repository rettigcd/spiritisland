using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Speed.Slow,Element.Water,Element.Plant,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// remove 1 blight OR push up to 3 dahan
			if( await ctx.Self.UserSelectsFirstText("Select power","remove 1 blight","push up to 3 dahan") )
				ctx.RemoveBlight();
			else
				await ctx.PowerPushUpToNDahan(3);
		}

	}
}
