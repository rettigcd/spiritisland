﻿using System;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[PowerCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
	public class RiversBounty : TargetSpaceAction {
		public const string Name = "River's Bounty";

		// Gather up to 2 Dahan
		// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy

		public RiversBounty(Spirit spirit,GameState gs) 
			: base(spirit,gs,0,From.Presence) {
		}

		protected override bool FilterSpace(Space space){
			// space has dahan neighbors
			return space.SpacesExactly(1).Any(neighbor=>gameState.HasDahan(neighbor));
		}

		protected override void SelectSpace(Space space){
			var ctx = new GatherDahanCtx(space,gameState);
			engine.decisions.Push(new If2Add1(engine,ctx));  // do this last
			engine.decisions.Push(new Select1DahanSource(engine,ctx,2));
		}

		// Hack - this isn't a decision, just something I need to run at the end.
		class If2Add1 : IDecision {
			readonly GatherDahanCtx ctx;
			readonly ActionEngine engine;
			public If2Add1(ActionEngine engine, GatherDahanCtx ctx){
				this.engine = engine;
				this.ctx = ctx;
			}
			public IOption[] Options => new IOption[]{Invader.Explorer}; // not used but it can't be null

			public string Prompt => "Adding 1 Dahan if 2 are present in "+ctx.Target.Label;

			public void Select( IOption _ ) {
				if(ctx.destinationCount>=2)
					engine.actions.Add(new AddDahan(ctx.Target,1));
			}

		}

	}

}



