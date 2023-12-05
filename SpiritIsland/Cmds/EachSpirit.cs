namespace SpiritIsland;

// Commands that act on: GameState
public class EachSpirit : IActOn<GameState> {

	#region constructor
	public EachSpirit( IActOn<Spirit> spiritAction ) {
		_spiritAction = spiritAction;
	}
	#endregion

	public string Description => $"For each {_filter.Description}, {_spiritAction.Description}.";

	public async Task ActAsync( GameState gameState ) {
		ActionScope parentScope = ActionScope.Current;
		foreach(Spirit spirit in gameState.Spirits) {
			// Page 10 of JE says Each Spirit is a new action
			await using ActionScope actionScope = await ActionScope.Start( parentScope.Category );
			await _spiritAction.ActAsync( spirit );
		}
	}

	public EachSpirit Who( SpiritFilter filter ) {
		_filter = filter;
		return this;
	}


	public bool IsApplicable( GameState ctx ) => true;

	#region private
	readonly IActOn<Spirit> _spiritAction;
	SpiritFilter _filter = Is.AnySpirit; // alt: Each, All
	#endregion

}