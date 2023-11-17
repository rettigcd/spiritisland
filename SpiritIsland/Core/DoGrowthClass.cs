namespace SpiritIsland;

public abstract partial class Spirit {

	class DoGrowthClass {

		#region private fields

		const string PROMPT = "Select Growth";

		readonly Spirit _spirit;
		readonly IGrowthPhaseInstance _inst;

		GrowthOption[] _growthOptions;

		// When starting a new growth option, pulls actions from all the actions in the remaining groups
		// When continuing a growth option, pulls from actions stored in spirit's AvailableOptions
		IActionFactory[] _availableNewGrowthOptions;

		bool _shouldInitNewGrowthOption;
		#endregion

		public DoGrowthClass(Spirit spirit,GameState _) {
			_spirit = spirit;
			_inst = _spirit.GrowthTrack.GetInstance();
		}

		public async Task Execute() {
			AllowUserToSelectNewGrowthOptions();

			while(HasGrowthActions) {
				// Select
				IActionFactory selectedAction = await _spirit.Select( PROMPT, Consolidated, Present.Always );

				if(_shouldInitNewGrowthOption)
					await InitSelectedGrowthOption( selectedAction );

				// Go
				await _spirit.TakeActionAsync( selectedAction, Phase.Growth );

				// Next
				_availableNewGrowthOptions = _spirit.GetAvailableActions( Phase.Growth ).ToArray();
				if(!HasGrowthActions)
					AllowUserToSelectNewGrowthOptions();

			}// while

		}// execute

		async Task InitSelectedGrowthOption( IActionFactory selectedAction ) {
			// Find Growth Option
			GrowthOption option = _growthOptions.SingleOrDefault( o => o.GrowthActions.Contains( selectedAction ) );
			if(option == null) return; // not one the Growth Actions, but came from somewhere else - may Behemoth Rise

			_inst.MarkAsUsed( option );

			// Add Action to spirit
			foreach(IHelpGrow action in option.GrowthActions)
				_spirit._availableActions.Add( action );

			// run auto-runs.
			foreach(var autoAction in option.AutoRuns)
				if(autoAction != selectedAction)
					await _spirit.TakeActionAsync( autoAction, Phase.Growth );

			_availableNewGrowthOptions = Array.Empty<IActionFactory>();
			_shouldInitNewGrowthOption = false;
		}

		void AllowUserToSelectNewGrowthOptions() {
			_growthOptions = _inst.RemainingOptions( _spirit.Energy );
			_availableNewGrowthOptions = _growthOptions.SelectMany( opt => opt.GrowthActions ).ToArray();

			_shouldInitNewGrowthOption = true;
		}

		IActionFactory[] Consolidated => _availableNewGrowthOptions.Union(_spirit.GetAvailableActions(Phase.Growth)).ToArray();


		bool HasGrowthActions => Consolidated.OfType<IHelpGrow>().Any();

	}

}
