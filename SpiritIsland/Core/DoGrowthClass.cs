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
		IActionFactory[] _actionOptions;

		bool _shouldInitNewGrowthOption;
		#endregion

		public DoGrowthClass(Spirit spirit,GameState _) {
			_spirit = spirit;
			_inst = _spirit.GrowthTrack.GetInstance();
		}

		public async Task Execute() {
			AllowUserToSelectNewGrowthOptions();

			while(HasActions) {
				// Select
				IActionFactory selectedAction = await _spirit.Select( PROMPT, _actionOptions, Present.Always );

				if(_shouldInitNewGrowthOption)
					await InitSelectedGrowthOption( selectedAction );

				// Go
				await _spirit.TakeActionAsync( selectedAction, Phase.Growth );

				// Next
				_actionOptions = _spirit.GetAvailableActions( Phase.Growth ).ToArray();
				if(!HasActions)
					AllowUserToSelectNewGrowthOptions();

			}// while

		}// execute

		async Task InitSelectedGrowthOption( IActionFactory selectedAction ) {
			// Find Growth Option
			GrowthOption option = _growthOptions.Single( o => o.GrowthActions.Contains( selectedAction ) );
			_inst.MarkAsUsed( option );

			// Add Action to spirit
			foreach(GrowthActionFactory action in option.GrowthActions)
				_spirit._availableActions.Add( action );

			// run auto-runs.
			foreach(var autoAction in option.AutoRuns)
				if(autoAction != selectedAction)
					await _spirit.TakeActionAsync( autoAction, Phase.Growth );

			_shouldInitNewGrowthOption = false;
		}

		void AllowUserToSelectNewGrowthOptions() {
			_growthOptions = _inst.RemainingOptions( _spirit.Energy );
			_actionOptions = _growthOptions.SelectMany( opt => opt.GrowthActions ).ToArray();
			_shouldInitNewGrowthOption = true;
		}

		bool HasActions => 0 < _actionOptions.Length;

	}

}
