using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[Core.MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
	class DelusionsOfDanger : BaseAction {

		readonly Spirit spirit;

		public DelusionsOfDanger(Spirit spirit,GameState gs)
			:base(gs)
		{
			this.spirit = spirit;
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

			var target = await engine.SelectSpace("Select target",
				spirit.Presence.Range(1).Where(HasExplorer)
			);
			var destination = await engine.SelectSpace("Select explorer destination",target.Neighbors);
			new MoveInvader(Invader.Explorer, target, destination).Apply(gameState);

		}

		// Option 2 - 2 fear
		void DoFear() => gameState.AddFear(2);

	}
}
