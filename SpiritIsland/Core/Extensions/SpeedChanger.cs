using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {


	public class SpeedChanger {

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly string prompt;
		Speed toChangeFrom;
		Speed resultingSpeed;
		int countToChange;

		public SpeedChanger( Spirit spirit, GameState gameState, Speed resultingSpeed, int maxCountToChange ) {
			this.spirit = spirit;
			this.gameState = gameState; // for Time-Passes Hook, to reset
			this.countToChange = maxCountToChange;
			this.resultingSpeed = resultingSpeed;

			prompt = resultingSpeed switch {
				Speed.Fast => "Select action to make fast.",
				Speed.Slow => "Select action to make slow.",
				Speed.FastOrSlow => "Select action to toggle fast/slow.",
				_ => throw new System.ArgumentException( nameof(resultingSpeed), "can't toggle " + resultingSpeed )
			};

			toChangeFrom = resultingSpeed switch {
				Speed.Fast => Speed.Slow,
				Speed.Slow => Speed.Fast,
				Speed.FastOrSlow => Speed.FastOrSlow,
				_ => throw new System.ArgumentException( nameof( resultingSpeed ), "can't toggle " + resultingSpeed )
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
			IActionFactory factory = await spirit.Select( prompt + $" (remaining: {countToChange})",
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
			factory.Speed = resultingSpeed;

			var speedReseter = new RememberFactorySpeed( factory );
			gameState.TimePasses_ThisRound.Push( speedReseter.Reset );

		}

		/// <summary> This provides a javascript-like closure to capture the factory that needs reset to fast;</summary>
		class RememberFactorySpeed {
			readonly IActionFactory factory;
			readonly Speed originalSpeed;
			public RememberFactorySpeed( IActionFactory factory ) {
				this.factory = factory;
				this.originalSpeed = factory.Speed;
			}
			public Task Reset( GameState _ ) {
				factory.Speed = originalSpeed;
				return Task.CompletedTask;
			}
		}

	}

}