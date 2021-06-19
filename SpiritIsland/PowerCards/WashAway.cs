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

			engine.decisions.Push(TargetLandDecision());

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

			foreach(var move in engine.moves)
				move.Apply( gameState );
		}

		public void Select(IOption option) {
			InnerSelect(option);
			AutoSelectSingleOptions();
		}

		public IOption[] Options {
			get{
				if(engine.decisions.Count>0)
					return engine.decisions.Peek().Options;
	
				return new IOption[0];
			}
		}

		void InnerSelect(IOption option) {
			if(engine.decisions.Count>0){
				var descision = engine.decisions.Pop();
				descision.Select(option,engine);
				return;
			}

			throw new System.NotImplementedException();
		}

		#region Select Target Land
		Decision TargetLandDecision() => new Decision ( TargetLandOptions, SelectTargetLand );
		IOption[] TargetLandOptions() =>
			spirit.Presence
				.SelectMany(x => x.SpacesWithin(1))
				.Distinct()
				.Where(space => {
					var sum = gameState.GetInvaderGroup(space);
					return sum.HasExplorer || sum.HasTown;  // !!! what about damaged towns???
				})
				.ToArray();
		void SelectTargetLand(IOption opt,ActionEngine engine){
			target = (Space)opt;

			targetGroup = gameState.GetInvaderGroup(target);

			int invaderCount = targetGroup[Invader.Explorer] 
				+ targetGroup[Invader.Town]
				+ targetGroup[Invader.Town1];
			int numToMove = Math.Min(invaderCount,3);

			while(0<numToMove--)
				engine.decisions.Push(new SelectInvaderToPush(targetGroup,target));
		}
		#endregion

		readonly Spirit spirit;
		readonly GameState gameState;

		Space target;
		InvaderGroup targetGroup;

		readonly ActionEngine engine = new ActionEngine();

	}

	public class ActionEngine {
		public readonly List<IAtomicAction> moves = new List<IAtomicAction>();
		public readonly Stack<IDecision> decisions = new Stack<IDecision>();
	}

	public interface IDecision {
		public IOption[] Options { get; }
		public void Select(IOption option,ActionEngine engine);
	}

	public class SelectInvaderDestination : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly Invader invader;
		readonly Space from;

		public SelectInvaderDestination(InvaderGroup invaderGroup, Invader invader, Space from){
			this.invaderGroup = invaderGroup;
			this.invader = invader;
			this.from = from;
		}

		public IOption[] Options => from.SpacesExactly(1)
			.Where(x=>x.IsLand)
			.ToArray();

		public void Select( IOption option,ActionEngine engine) {
			engine.moves.Add(new MoveInvader(invader, from, (Space)option));
			invaderGroup[invader]--;
		}

	}

	public class SelectInvaderToPush : IDecision {

		readonly InvaderGroup invaderGroup;
		readonly Space from;

		public SelectInvaderToPush(InvaderGroup invaderGroup,Space from){
			this.invaderGroup = invaderGroup;
			this.from = from;
		}

		public IOption[] Options { get {
			return invaderGroup
				.InvaderTypesPresent
				.Where(i=>i.Label != "City")  //
				.ToArray();
		}}

		public void Select( IOption option, ActionEngine engine ) {
			var invaderToPush = (Invader)option;
			engine.decisions.Push( new SelectInvaderDestination(
				invaderGroup,
				invaderToPush,
				from
			) );
		}

	}

	public interface IAtomicAction {
		void Apply(GameState gameState);
	}

	public class MoveInvader : IAtomicAction {
		public MoveInvader(Invader invader, Space from, Space to){
			this.invader = invader;
			this.from = from;
			this.to = to;
		}
		public void Apply( GameState gameState ) {
			gameState.Adjust(invader,from,-1);
			gameState.Adjust(invader,to,1);
		}
		readonly Invader invader;
		readonly Space from;
		readonly Space to;
	}

	class Decision : IDecision {
		public Decision( Func<IOption[]> options, Action<IOption,ActionEngine> select ){
			this.options = options;
			this.select = select;
		}

		readonly Func<IOption[]> options;
		readonly Action<IOption,ActionEngine> select;

		public IOption[] Options => options();
		public void Select(IOption option, ActionEngine engine){
			select(option,engine);
			Selection = option;
			this.Selected?.Invoke( option );
		}
		public IOption Selection {get; private set;}
		event Action<IOption> Selected;
	}

}
