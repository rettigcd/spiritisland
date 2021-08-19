using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Speed.Slow,Element.Water,Element.Plant,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			// remove 1 blight OR push up to 3 dahan
			if( await ctx.Self.SelectFirstText("Select power","remove 1 blight","push up to 3 dahan") )
				ctx.GameState.RemoveBlight( target );
			else
				await ctx.PushUpToNDahan(target, 3);
		}

	}
}
