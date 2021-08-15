using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class DelusionsOfDanger {

		[MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
		[FromPresence(1,Target.Explorer)]
		static public async Task ActionAsync(ActionEngine engine, Space target){

			const string PushKey = "Push 1 Explorer";
			const string FearKey = "2 fear";
			string text = await engine.SelectText("Select power",PushKey,FearKey);
			switch(text){
				case PushKey: 
					await engine.PushInvader(target,Invader.Explorer);
					break;
				case FearKey: 
					engine.GameState.AddFear(2); 
					break;
			}

		}

	}
}
