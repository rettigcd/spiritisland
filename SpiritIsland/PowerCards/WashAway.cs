using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.PowerCards {

	[PowerCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
	public class WashAway : IAction {

		public const string Name = "Wash Away";

		public WashAway(Spirit spirit,GameState gs) {
			this.spirit = spirit;
			this.gameState = gs;

			decisions.Push(TargetLandDecision());

			AutoSelectSingleOptions();
		}

		void AutoSelectSingleOptions() {
			var opt = Options;
			while (opt.Length == 1) {
				InnerSelect(opt[0]);
				opt = Options;
			}
		}

		public bool IsResolved => Options.Length == 0;

		public void Apply() {
			if(target==null) return;

			foreach(var move in moves){
				gameState.Adjust(move.Invader,target,-1);
				gameState.Adjust(move.Invader,move.Destination,1);
			}
		}

		public void Select(IOption option) {
			InnerSelect(option);
			AutoSelectSingleOptions();
		}

		public IOption[] Options {
			get{
				if(decisions.Count>0)
					return decisions.Peek().options();
	
				return new IOption[0];
			}
		}

		void InnerSelect(IOption option) {
			if(decisions.Count>0){
				var descision = decisions.Pop();
				descision.select(option);
				return;
			}

			throw new System.NotImplementedException();
		}

		#region Select Target Land
		Decision TargetLandDecision() => new Decision { options = TargetLandOptions, select = SelectTargetLand };
		IOption[] TargetLandOptions() =>
			spirit.Presence
				.SelectMany(x => x.SpacesWithin(1))
				.Distinct()
				.Where(space => {
					var sum = gameState.GetInvaderGroup(space);
					return sum.HasExplorer || sum.HasTown;  // !!! what about damaged towns???
				})
				.ToArray();
		void SelectTargetLand(IOption opt){
			target = (Space)opt;
			targetGroup = gameState.GetInvaderGroup(target);

			int invaderCount = targetGroup[Invader.Explorer] 
						+ targetGroup[Invader.Town]
						+ targetGroup[Invader.Town1];
			int numToMove = Math.Min(invaderCount,3);
			while(0<numToMove--)
				decisions.Push(ExplorerToPushDecision());
		}
		#endregion

		#region Select Explorer
		Decision ExplorerToPushDecision() 
			=> new Decision { options = ExplorerSelectionOptions, select = SelectExplorer };
		
		IOption[] ExplorerSelectionOptions() {
			return targetGroup
				.InvaderTypesPresent
				.Where(i=>i.Label != "City")
				.ToArray();
		}
		void SelectExplorer(IOption opt) {
			invaderToPush = (Invader)opt;
			decisions.Push( SelectExplorerDestination() );
		}
		#endregion Select Explorer

		#region Select Explorer Destination
		Decision SelectExplorerDestination()
			=> new Decision { options = ExplorerDestinationOptions, select = SelectExplorerDestination };
		IOption[] ExplorerDestinationOptions() => target.SpacesExactly(1)
			.Where(x=>x.IsLand)
			.ToArray();
		void SelectExplorerDestination(IOption opt){
			moves.Add(new Move{Invader=invaderToPush, Destination=(Space)opt});
			targetGroup[invaderToPush]--;
		}
		#endregion Select Explorer Destination

		class Decision {
			public Func<IOption[]> options;
			public Action<IOption> select;
		}

		readonly Stack<Decision> decisions = new Stack<Decision>();

		readonly Spirit spirit;
		readonly GameState gameState;

		Space target;
		InvaderGroup targetGroup;

		Invader invaderToPush;
		readonly List<Move> moves = new List<Move>();

		struct Move {
			public Invader Invader;
			public Space Destination;
		}
	}
}
