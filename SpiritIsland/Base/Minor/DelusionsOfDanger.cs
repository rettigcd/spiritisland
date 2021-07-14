using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[Core.MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
	class DelusionsOfDanger : BaseAction {

		public DelusionsOfDanger(Spirit spirit,GameState gs)
			:base(spirit,gs)
		{
			_ = ActionAsync();
		}

		async Task ActionAsync(){

			const string PushKey = "Push 1 Explorer";
			const string FearKey = "2 fear";
			string text = await engine.SelectText("Select power",PushKey,FearKey);
			switch(text){
				case PushKey: await DoPush(); break;
				case FearKey: DoFear(); break;
			}

		}

		// Option 1 - push 1 explorer
		async Task DoPush(){
			bool HasExplorer(Space space) => gameState.InvadersOn(space).HasExplorer;

			var target = await engine.Api.TargetSpace_Presence(1,HasExplorer);
			await engine.PushInvader(target,Invader.Explorer);
		}

		// Option 2 - 2 fear
		void DoFear() => gameState.AddFear(2);

	}
}
