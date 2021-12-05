using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpeedChanger {

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly string prompt;
		readonly Phase toChangeFrom;
		readonly Phase resultingSpeed;
		int countToChange;

		public SpeedChanger( Spirit spirit, GameState gs, Phase resultingSpeed, int maxCountToChange ) {
			this.spirit = spirit;
			this.gameState = gs; // for Time-Passes Hook, to reset
			this.countToChange = maxCountToChange;
			this.resultingSpeed = resultingSpeed;

			prompt = resultingSpeed switch {
				Phase.Fast => "Select action to make fast.",
				Phase.Slow => "Select action to make slow.",
				Phase.FastOrSlow => "Select action to toggle fast/slow.",
				_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
			};

			toChangeFrom = resultingSpeed switch {
				Phase.Fast => Phase.Slow,
				Phase.Slow => Phase.Fast,
				Phase.FastOrSlow => Phase.FastOrSlow,
				_ => throw new System.ArgumentException( "can't toggle " + resultingSpeed, nameof( resultingSpeed ) )
			};

		}

		public async Task Exec() {

			// clip count to available slow stuff
			countToChange = System.Math.Min( countToChange, FindSouceFactories().Length );

			while(countToChange > 0)
				await FindAndChange();

		}

		protected IFlexibleSpeedActionFactory[] FindSouceFactories()
			=> spirit.GetAvailableActions( toChangeFrom ).OfType<IFlexibleSpeedActionFactory>().ToArray();

		async Task FindAndChange( ) {
			var changeableFactories = FindSouceFactories();
			IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await spirit.SelectFactory( prompt + $" (remaining: {countToChange})",
				changeableFactories,
				Present.Done
			);

			if(factory != null) {
				Change( factory );
				--countToChange;
			} else
				countToChange = 0;
		}

		class SpeedOverrideBehavior : ISpeedBehavior {
			readonly Phase newSpeed;
			public SpeedOverrideBehavior(Phase newSpeed ) { this.newSpeed = newSpeed; }

			public bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit )
				=> requestSpeed == newSpeed;
			
			public Task<bool> IsActiveFor( Phase requestSpeed, Spirit spirit ) 
				=> Task.FromResult(requestSpeed == newSpeed);
		}

		void Change( IFlexibleSpeedActionFactory factory ) {
			factory.OverrideSpeedBehavior = new SpeedOverrideBehavior( resultingSpeed );

			Task Restore(GameState _) { 
				factory.OverrideSpeedBehavior = null;
				return Task.CompletedTask;
			}
			gameState.TimePasses_ThisRound.Push( Restore );

		}

	}

}