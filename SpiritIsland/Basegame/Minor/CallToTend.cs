using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Speed.Slow,Element.Water,Element.Plant,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActAsync(ActionEngine engine,Space target ) {
			// remove 1 blight OR push up to 3 dahan
			if( await engine.Self.SelectFirstText("Select power","remove 1 blight","push up to 3 dahan") )
				engine.GameState.RemoveBlight( target );
			else
				await engine.PushUpToNDahan(target, 3);
		}

	}
}
