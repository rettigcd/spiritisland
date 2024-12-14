namespace SpiritIsland;

public abstract partial class Spirit {

	class DoGrowthClass {

		public DoGrowthClass(Spirit spirit,GameState _) {
			_spirit = spirit;
			_inst = _spirit.GrowthTrack.GetInstance();
		}

		public async Task Execute() {
			AllowUserToSelectNewGrowthOptions();

			while(HasGrowthActions) {
				// Select
				IActionFactory selectedAction = await _spirit.SelectGrowth( PROMPT, Consolidated, Present.Always );

				if(_shouldInitNewGrowthOption)
					await InitSelectedGrowthOption( selectedAction );

				// Go
				await _spirit.ResolveActionAsync( selectedAction, Phase.Growth );

				// Next
				_availableNewGrowthOptions = _spirit.GetAvailableActions( Phase.Growth ).ToArray();
				if(!HasGrowthActions)
					AllowUserToSelectNewGrowthOptions();

			}// while

		}// execute

		async Task InitSelectedGrowthOption( IActionFactory selectedAction ) {
			// Find Growth Option
			GrowthGroup option = _growthOptions.SingleOrDefault( o => o.GrowthActionFactories.Contains( selectedAction ) );
			if(option == null) return; // not one the Growth Actions, but came from somewhere else - may Behemoth Rise

			_inst.MarkAsUsed( option );

			// Add Action to spirit
			foreach(IHelpGrowActionFactory action in option.GrowthActionFactories)
				_spirit._availableActions.Add( action );

			// run auto-runs.
			foreach(var autoAction in option.GrowthActionFactories.Where(x => x.AutoRun) )
				if(autoAction != selectedAction)
					await _spirit.ResolveActionAsync( autoAction, Phase.Growth );

			_availableNewGrowthOptions = [];
			_shouldInitNewGrowthOption = false;
		}

		void AllowUserToSelectNewGrowthOptions() {
			_growthOptions = _inst.RemainingOptions( _spirit.Energy );
			_availableNewGrowthOptions = _growthOptions
				.SelectMany( opt => opt.GrowthActionFactories )
				.ToArray();

			_shouldInitNewGrowthOption = true;
		}

		IActionFactory[] Consolidated => _availableNewGrowthOptions.Union(_spirit.GetAvailableActions(Phase.Growth)).ToArray();

		bool HasGrowthActions => Consolidated.OfType<IHelpGrowActionFactory>().Any();

		#region private fields

		const string PROMPT = "Select Growth";

		// When starting a new growth option, pulls actions from all the actions in the remaining groups
		// When continuing a growth option, pulls from actions stored in spirit's AvailableOptions
		IActionFactory[] _availableNewGrowthOptions;
		GrowthGroup[] _growthOptions;
		bool _shouldInitNewGrowthOption;

		readonly Spirit _spirit;
		readonly IGrowthPhaseInstance _inst;

		#endregion

	}

}
