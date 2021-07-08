using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[Core.MinorCard("Delusions of Danger",1,Speed.Fast,Element.Sun,Element.Moon,Element.Air)]
	class DelusionsOfDanger : BaseAction {

		readonly Spirit spirit;

		public DelusionsOfDanger(Spirit spirit,GameState gs)
			:base(gs)
		{
			this.spirit = spirit;
			engine.decisions.Push(new SelectText(engine,SelectPower,"Push 1 Explorer", "2 fear" ));
		}

		const string PushKey = "Push 1 Explorer";
		const string FearKey = "2 fear";

		void SelectPower(string option, ActionEngine engine){
			switch(option){
				case PushKey: DoPush(); break;
				case FearKey: DoFear(); break;
			}
		}

		void DoPush(){
			// Option 1 - push 1 explorer
			// range 1
			engine.decisions.Push( new SelectSpaceRangeFromPresence(spirit,1,HasExplorer,SelectPushTarget) );
		}
		bool HasExplorer(Space space) => gameState.InvadersOn(space).HasExplorer;

		void SelectPushTarget(Space target){
			var grp = gameState.InvadersOn(target);
			engine.decisions.Push(new SelectInvadersToPush(engine,grp,1,false,"E@1"));
		}

		// Option 2 - 2 fear
		void DoFear() => gameState.AddFear(2);

	}
}
