using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class SkyStretchesToShore {

		[MinorCard( "Sky Stretches to Shore", 1, Speed.Fast, Element.Sun, Element.Air, Element.Water, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// this turn, target spirit may use 1 slow power as if it wer fast or vice versa
			ctx.Target.AddActionFactory( new ChangeSpeed() );

			// Target Spirit gains +3 range for targeting costal lands only

			throw new NotImplementedException();
		}

		// When Making Cards fast / slow, need to effect discard cards also
		// OR
		// Repeat Carding -> Select played card and bring it back into active, then switch its speeed.

		// !!! do powers that repeat cards allow change of speed??

	}

	public class ChangeSpeed : IActionFactory {
		public Speed Speed {
			get => Speed.FastOrSlow;
			set => throw new InvalidOperationException();
		}

		public string Name => "Change Speed";

		public IActionFactory Original => this;

		public string Text => Name;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return new SpeedChanger( spirit, gameState, Speed.Fast, 2 ).Exec();
		}
	}
