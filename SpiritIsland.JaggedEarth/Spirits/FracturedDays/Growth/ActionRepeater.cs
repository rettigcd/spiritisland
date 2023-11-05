namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Binds to multiple actions, allowing them be repeated as a group.
/// </summary>
/// <remarks>	Fractured Days Growth Option 2 & 3 </remarks>
public class ActionRepeater {

	public readonly int repeats;
	readonly List<IHelpGrow> factories = new List<IHelpGrow>();

	public int currentRepeats;

	public ActionRepeater(int repeats) { this.repeats = repeats; }

	public void Register( IHelpGrow factory ) {
		factories.Add(factory);
	}

	public void BeginAction() {
		if(currentRepeats == 0) 
			currentRepeats = repeats;
	}

	public void EndAction( Spirit spirit ) {
		--currentRepeats;

		if(currentRepeats > 0)
			Restore( spirit );
		else
			CleanUp( spirit );
	}

	void Restore(Spirit spirit ) {
		var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
		foreach(var factory in factories)
			if( !remaining.Contains(factory) )
				spirit.AddActionFactory( factory );
	}

	void CleanUp(Spirit spirit ) {
		var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
		foreach(var factory in factories)
			if( remaining.Contains(factory) )
				spirit.RemoveFromUnresolvedActions( factory );
	}

	public SpiritAction BindSelfCmd( SpiritAction inner ) => new RepeatableSelfCmd( inner, this );

}