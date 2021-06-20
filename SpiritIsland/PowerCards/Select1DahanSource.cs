using System.Linq;

namespace SpiritIsland.PowerCards {
	public class Select1DahanSource : IDecision {
		readonly GatherDahanCtx ctx;
		readonly int numberToGather;

		public Select1DahanSource(GatherDahanCtx ctx,int numberToGather){
			this.ctx = ctx;
			this.numberToGather = numberToGather;
		}
		public IOption[] Options => ctx.neighborCounts.Keys.ToArray();

		public string Prompt => "Select source land to gather Dahan into "+ctx.Target.Label;

		public void Select( IOption option, ActionEngine engine ) {
			Space source = (Space)option;
			// update ctx
			++ctx.destinationCount;
			--ctx.neighborCounts[source];

			// add official move
			engine.actions.Add(new MoveDahan(source,ctx.Target));
			int remaining=numberToGather-1;
			if(remaining>0 && ctx.neighborCounts.Keys.Any())
				engine.decisions.Push(new Select1DahanSource(ctx,remaining));
		}

	}

}



