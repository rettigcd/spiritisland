using System.Linq;

namespace SpiritIsland.Core {

	public class SelectPushedDahanDestination : IDecision {

		readonly int additional;
		readonly ActionEngine engine;
		readonly bool allowShortCircuit;
		readonly Space space;
		readonly GameState gameState;

		public SelectPushedDahanDestination(ActionEngine engine, Space space, GameState gameState, int count, bool allowShortCircuit){
			this.engine = engine;
			this.space = space;
			this.gameState = gameState;
			this.additional = count-1;
			this.allowShortCircuit = allowShortCircuit;

			var options = space.SpacesExactly(1)
				.Where(s=>s.IsLand)
				.Cast<IOption>()
				.ToList();
			if(options.Count>0)
				options.Add(TextOption.Done);
			Options = options.ToArray();
		}

		public string Prompt => $"Select land to push dahan to.";

		public IOption[] Options {get;}

		public void Select( IOption option ) {

			if(TextOption.Done.Matches(option))
				return;

			Space destination = (Space)option;
			gameState.AddDahan(space,-1);
			gameState.AddDahan(destination,1);

			// if we need more, push next
			if(additional>0)
				engine.decisions.Push(new SelectPushedDahanDestination( engine, space, gameState, additional, allowShortCircuit));
		}

	}

}
