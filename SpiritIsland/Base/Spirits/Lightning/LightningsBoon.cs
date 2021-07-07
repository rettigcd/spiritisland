
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
	public class LightningsBoon : TargetSpiritAction {
		public const string Name = "Lightning's Boon";

		public LightningsBoon(Spirit spirit,GameState gs):base(spirit,gs){}

		protected override void SelectSpirit( Spirit spirit ) {
			engine.decisions.Push(new SelectActionsToMakeFast(engine,spirit,2));
		}

		// any spirt
		// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.

	}

	public class SelectActionsToMakeFast : IDecision {

		readonly int additional;
		readonly Spirit spirit;
		readonly ActionEngine engine;

		public SelectActionsToMakeFast(ActionEngine engine, Spirit spirit, int count){
			this.engine = engine;
			this.spirit = spirit;
			this.additional = count-1;
			var options = spirit.GetUnresolvedActionFactories(Speed.Slow)
				.Cast<IOption>()
				.ToList();
			options.Add(new TextOption("Done"));
			Options = options.ToArray();
		}
		public string Prompt => "Select action to make fast.";

		public IOption[] Options { get; }

		public void Select( IOption option ) {
			if(option is TextOption txt && txt.Text=="Done") return;

			IActionFactory factory = (IActionFactory)option;
			spirit.Resolve( factory );
			spirit.AddActionFactory( new ChangeSpeed(factory,Speed.Fast) );

			if(additional>0)
				engine.decisions.Push(new SelectActionsToMakeFast(engine,spirit,additional) );
		}
	}

}
