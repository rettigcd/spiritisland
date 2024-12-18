namespace SpiritIsland;

/// <summary>
/// Runs the provide action on a space targeted by the spirit during the current round.
/// </summary>
/// <param name="spirit"></param>
/// <param name="spaceAction"></param>
public class RunSpaceActionOnceOnFutureTarget( Spirit spirit, SpaceAction spaceAction ) : IRunAtStartOfEveryAction {

	static public void Trigger( Spirit spirit, SpaceAction spaceAction) {
		ActionScope.StartOfActionHandlers.Add(new RunSpaceActionOnceOnFutureTarget(spirit, spaceAction));
	}

	Task IRunAtStartOfEveryAction.Start( ActionScope startingScope ) {
		// Add to the end of EVERY Action this round.
		if(_invokedRound == GameState.Current.RoundNumber)
			startingScope.AtEndOfThisAction( EndOfRoundCheck );
		else
			ActionScope.StartOfActionHandlers.Remove( this );
		return Task.CompletedTask;
	}

	async Task EndOfRoundCheck( ActionScope endScope ) {

		TargetSpaceResults? targetLandDetails = null;
		bool isCandidate = _used
			&& endScope.Category == ActionCategory.Spirit_Power     // is power
			&& endScope.Owner == spirit                      // matches spirit
			&& (targetLandDetails = TargetSpaceAttribute.TargettedSpace) is not null;  // targetted land.
		if( !isCandidate ) return;

		Space ss = targetLandDetails!.Space;
		if( await spirit.UserSelectsFirstText($"Apply {spaceAction.Description} on ({ss.SpaceSpec.Label})", "Yes", "No thank you") ) {
			_used = true;
			await spaceAction.ActAsync(spirit.Target(ss));
		}
	}

	#region private 
	readonly int _invokedRound = GameState.Current.RoundNumber;
	bool _used = false;
	#endregion

}