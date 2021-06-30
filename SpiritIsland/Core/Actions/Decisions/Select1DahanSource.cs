using System.Linq;

namespace SpiritIsland.Core {
	public class Select1DahanSource : IDecision {
		readonly GatherDahanCtx ctx;
		readonly int numberToGather;
		readonly ActionEngine engine;

		public Select1DahanSource(ActionEngine engine, GatherDahanCtx ctx,int numberToGather){
			this.engine = engine;
			this.ctx = ctx;
			this.numberToGather = numberToGather;
		}
		public IOption[] Options => ctx.neighborCounts.Keys.ToArray();

		public string Prompt => "Select source land to gather Dahan into "+ctx.Target.Label;

		public void Select( IOption option ) {
			Space source = (Space)option;
			// update ctx
			++ctx.destinationCount;
			--ctx.neighborCounts[source];

			// add official move
			engine.actions.Add(new MoveDahan(source,ctx.Target));
			int remaining=numberToGather-1;
			if(remaining>0 && ctx.neighborCounts.Keys.Any())
				engine.decisions.Push(new Select1DahanSource(engine,ctx,remaining));
		}

	}

}



