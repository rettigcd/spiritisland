using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpeedChanger {

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly string prompt;
		readonly Speed toChangeFrom;
		readonly Speed resultingSpeed;
		int countToChange;

		public SpeedChanger( SpiritGameStateCtx ctx, Speed resultingSpeed, int maxCountToChange )
			:this(ctx.Self,ctx.GameState,resultingSpeed,maxCountToChange) 
		{}

		public SpeedChanger( Spirit spirit, GameState gs, Speed resultingSpeed, int maxCountToChange ) {
			this.spirit = spirit;
			this.gameState = gs; // for Time-Passes Hook, to reset
			this.countToChange = maxCountToChange;
			this.resultingSpeed = resultingSpeed;

			prompt = resultingSpeed switch {
				Speed.Fast => "Select action to make fast.",
				Speed.Slow => "Select action to make slow.",
				Speed.FastOrSlow => "Select action to toggle fast/slow.",
				_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
			};

			toChangeFrom = resultingSpeed switch {
				Speed.Fast => Speed.Slow,
				Speed.Slow => Speed.Fast,
				Speed.FastOrSlow => Speed.FastOrSlow,
				_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
			};

		}

		public async Task Exec() {

			// clip count to available slow stuff
			countToChange = System.Math.Min( countToChange, FindSouceFactories().Length );

			while(countToChange > 0)
				await FindAndChange();

		}

		protected IActionFactory[] FindSouceFactories()
			=> spirit.GetAvailableActions( toChangeFrom ).ToArray();

		async Task FindAndChange( ) {
			var changeableFactories = FindSouceFactories();
			IActionFactory factory = await spirit.SelectFactory( prompt + $" (remaining: {countToChange})",
				changeableFactories,
				Present.Done
			);

			if(factory != null) {
				Change( factory );
				--countToChange;
			} else
				countToChange = 0;
		}

		void Change( IActionFactory factory ) {
			Task Restore(GameState _) { factory.OverrideSpeed = null; return Task.CompletedTask; }
			gameState.TimePasses_ThisRound.Push( Restore );
			factory.OverrideSpeed = new SpeedOverride( resultingSpeed, "SpeedChanger" );
		}

	}

}