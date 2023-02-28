namespace SpiritIsland;

public abstract partial class Spirit {
	class DoGrowthClass {

		#region private fields

		const string PROMPT = "Select Growth";

		readonly Spirit spirit;
		readonly IGrowthPhaseInstance inst;

		GrowthOption[] growthOptions;
		IActionFactory[] actionOptions;

		Func<IActionFactory,Task> execute;
		#endregion

		public DoGrowthClass(Spirit spirit,GameState _) {
			this.spirit = spirit;
			inst = spirit.GrowthTrack.GetInstance();
			InitActionsForAllAvailableOptions();
		}

		public async Task Execute() {

			while(HasActions) {
				// Select
				IActionFactory selectedAction = await spirit.Select( PROMPT, actionOptions, Present.Always );
				// Execute
				await execute( selectedAction );
			}

		}

		async Task ExecuteRemainingGrowth( IActionFactory selectedAction ) {
			await using var action = await ActionScope.Start(ActionCategory.Spirit_Growth);
			await spirit.TakeAction( selectedAction, Phase.Growth );
			await InitRemainingActionsFromOption();
		}

		async Task ExecuteFirstGrowth( IActionFactory selectedAction ) {
			// Find Growth Option
			GrowthOption option = growthOptions.Single( o => o.GrowthActions.Contains( selectedAction ) );
			inst.MarkAsUsed( option );

			// Resolve Growth Option
			await using var actionScope = await ActionScope.Start(ActionCategory.Spirit_Growth);
			var ctx = spirit.BindSelf();

			// Auto run the auto-runs.
			foreach(var autoAction in option.AutoRuns)
				await autoAction.ActivateAsync( ctx );

			if(option.AutoRuns.Contains( selectedAction ))
				// selected item was an auto-run
				// queue up the user runs
				foreach(GrowthActionFactory action in option.UserRuns)
					spirit.availableActions.Add( action );
			else {
				// selected item was a user-run
				// run it
				await selectedAction.ActivateAsync( ctx );
				// queue up all the others
				foreach(GrowthActionFactory action in option.UserRuns)
					if(action != selectedAction)
						spirit.availableActions.Add( action );
			}

			// resolve actions
			await InitRemainingActionsFromOption();
		}

		bool HasActions => actionOptions.Length > 0;

		async Task InitRemainingActionsFromOption() {
			// Combine these into a Class that executes
			actionOptions = spirit.GetAvailableActions( Phase.Growth ).ToArray();
			execute = ExecuteRemainingGrowth;

			if(!HasActions)
				await ApplyRevealedPResenceTracks();
		}

		void InitActionsForAllAvailableOptions() {
			growthOptions = inst.RemainingOptions( spirit.Energy );

			// Combine these into a Class that executes
			actionOptions = growthOptions.SelectMany( opt => opt.GrowthActions ).ToArray();
			execute = ExecuteFirstGrowth;
		}

		async Task ApplyRevealedPResenceTracks() {
			InitActionsForAllAvailableOptions();
			if(!HasActions) {
				await using var action = await ActionScope.Start(ActionCategory.Spirit_PresenceTrackIcon);
				await spirit.ApplyRevealedPresenceTracks( spirit.BindSelf() );
			}
		}

	}

}
