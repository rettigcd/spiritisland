namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Manages a group of Repeatable-Actions:
///  - Restoring the repeatable actions when repeats remain
///  - Clearing out all repeatable actions when repeats are used up.
/// </summary>
/// <remarks>	Fractured Days Growth Option 2 & 3 </remarks>
public class ActionRepeater( int repeats ) {

	public readonly int _repeats = repeats;
	public int _remainingRepeats;

	#region constructor

	#endregion

	public void Register( IHelpGrow factory ) {
		_factories.Add(factory);
	}

	public void BeginAction() {
		if(_remainingRepeats == 0) 
			_remainingRepeats = _repeats;
	}

	public void EndAction( Spirit spirit ) {
		--_remainingRepeats;

		if(0 < _remainingRepeats)
			RestoreActionFactoryToAvailableActions( spirit );
		else
			RemoveUnusedActions( spirit );
	}

	public SpiritAction BindSelfCmd( SpiritAction inner ) => new RepeatableSelfCmd( inner, this );

	#region private

	void RestoreActionFactoryToAvailableActions(Spirit spirit ) {
		var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
		foreach(var factory in _factories)
			if( !remaining.Contains(factory) )
				spirit.AddActionFactory( factory );
	}

	void RemoveUnusedActions(Spirit spirit ) {
		var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
		foreach(var factory in _factories)
			if( remaining.Contains(factory) )
				spirit.RemoveFromUnresolvedActions( factory );
	}

	readonly List<IHelpGrow> _factories = [];

	#endregion

}