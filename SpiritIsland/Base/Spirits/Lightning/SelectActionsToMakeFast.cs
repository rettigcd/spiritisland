
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
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
			options.Add(TextOption.Done);
			Options = options.ToArray();
		}
		public string Prompt => "Select action to make fast.";

		public IOption[] Options { get; }

		public void Select( IOption option ) {
			if(TextOption.Done.Matches(option) ) return;

			IActionFactory factory = (IActionFactory)option;
			spirit.Resolve( factory );
			spirit.AddActionFactory( new ChangeSpeed(factory,Speed.Fast) );

			if(additional>0)
				engine.decisions.Push(new SelectActionsToMakeFast(engine,spirit,additional) );
		}
	}

}
