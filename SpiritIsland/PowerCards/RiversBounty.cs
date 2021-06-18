using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.PowerCards
{

	[PowerCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
	public class RiversBounty : IAction {
		public const string Name = "River's Bounty";

		// target: range 0 (any)

		// Gather up to 2 Dahan
		// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy

		public RiversBounty(Spirit spirit,GameState gs) {
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

			foreach(var source in sources){
				gameState.AddDahan(source,-1);
				gameState.AddDahan(target);
			}

			if( gameState.GetDahanOnSpace(target)>=2 )
				gameState.AddDahan(target);
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
		IOption[] TargetLandOptions() => spirit.Presence.Distinct().ToArray();
		void SelectTargetLand(IOption opt){
			target = (Space)opt;

			int neighboringDahanCount = target.SpacesExactly(1)
				.Select(gameState.GetDahanOnSpace)
				.Sum();
			int numToMove = Math.Min(neighboringDahanCount,2);
			while(0<numToMove--)
				decisions.Push(SourceLandDecision());
		}

		#endregion

		#region Select Source
		Decision SourceLandDecision() 
			=> new Decision { options = SourceLandOptions, select = SelectSource };
		
		bool HasDahan(Space s){
			int preCount = gameState.GetDahanOnSpace(s);
			return preCount>0
				&& preCount > sources.Count(x=>x==s); // tracks ones removed
		}

		IOption[] SourceLandOptions() => target.SpacesExactly(1)
			.Where(HasDahan)
			.ToArray();
		void SelectSource(IOption opt) {
			sources.Add((Space)opt);
		}
		#endregion Select Explorer

		class Decision {
			public Func<IOption[]> options;
			public Action<IOption> select;
		}

		readonly Stack<Decision> decisions = new Stack<Decision>();

		readonly Spirit spirit;
		readonly GameState gameState;

		Space target;
		readonly List<Space> sources = new List<Space>();

	}

}



