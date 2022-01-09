using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpeedChanger {

		readonly SelfCtx ctx;
		Spirit Spirit => ctx.Self;
		GameState GameState => ctx.GameState;
		readonly string prompt;
		readonly Phase toChangeFrom;
		readonly Phase resultingSpeed;
		int countToChange;

		public SpeedChanger( SelfCtx ctx, Phase resultingSpeed, int maxCountToChange ) {
			this.ctx = ctx;
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

		public async Task FindAndChange() {

			// clip count to available slow stuff
			countToChange = System.Math.Min( countToChange, FindSouceFactories().Length );

			while(countToChange > 0)
				await FindAndChange1();

		}

		public async Task FindAndExecute() {

			// clip count to available slow stuff
			countToChange = System.Math.Min( countToChange, FindSouceFactories().Length );

			while(countToChange > 0)
				await FindAndExecute1();

		}

		protected IFlexibleSpeedActionFactory[] FindSouceFactories()
			=> Spirit.GetAvailableActions( toChangeFrom ).OfType<IFlexibleSpeedActionFactory>().ToArray();

		async Task FindAndChange1( ) {
			var changeableFactories = FindSouceFactories();
			IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await Spirit.SelectFactory( prompt + $" (remaining: {countToChange})",
				changeableFactories,
				Present.Done
			);

			if(factory != null) {
				TemporarySpeed.Override( factory, resultingSpeed, GameState );
				--countToChange;
			} else
				countToChange = 0;
		}

		async Task FindAndExecute1( ) {
			var changeableFactories = FindSouceFactories();
			IFlexibleSpeedActionFactory factory = (IFlexibleSpeedActionFactory)await Spirit.SelectFactory( prompt + $" (remaining: {countToChange})",
				changeableFactories,
				Present.Done
			);

			if(factory != null) {
				TemporarySpeed.Override( factory, resultingSpeed, GameState );
				await Spirit.TakeAction( factory, ctx );
				--countToChange;
			} else
				countToChange = 0;
		}

	}

	public class TemporarySpeed : ISpeedBehavior {

		static public void Override( IFlexibleSpeedActionFactory factory, Phase phase, GameState gameState ) {
			factory.OverrideSpeedBehavior = new TemporarySpeed( phase );

			Task Restore(GameState _) { 
				factory.OverrideSpeedBehavior = null;
				return Task.CompletedTask;
			}
			gameState.TimePasses_ThisRound.Push( Restore );
		}


		readonly Phase newSpeed;
		public TemporarySpeed(Phase newSpeed ) { this.newSpeed = newSpeed; }

		public bool CouldBeActiveFor( Phase requestSpeed, Spirit _ )
			=> requestSpeed == newSpeed;
			
		public Task<bool> IsActiveFor( Phase requestSpeed, Spirit _ ) 
			=> Task.FromResult(requestSpeed == newSpeed);
	}


}