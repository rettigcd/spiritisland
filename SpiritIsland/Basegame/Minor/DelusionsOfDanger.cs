using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class DelusionsOfDanger {

		[MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
		[FromPresence(1,Target.Explorer)]
		static public async Task ActionAsync(ActionEngine engine, Space target){

			if(await engine.Self.SelectFirstText( "Select power", "Push 1 Explorer", "2 fear" ))
				await engine.PushUpToNInvaders(target, 1, Invader.Explorer);
			else
				engine.AddFear(2); 

		}

	}
}
