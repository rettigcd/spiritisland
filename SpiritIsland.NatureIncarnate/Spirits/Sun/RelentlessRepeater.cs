namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Repeats the played card endlessly, costing 1 more each time.
/// </summary>
public class RelentlessRepeater( PowerCard powerCard, Space space ) : IActionFactory {

	static public SpecialRule Rule => new SpecialRule("Relentless Punishment","If you had at least 3 Presence in the origin land, you may Repeat a Power Card any number of times on the same target land(s) by paying its cost +1/previous use.");

	int _cost = powerCard.Cost + 1;
	readonly PowerCard _powerCard = powerCard;
	readonly Space _space = space;

	public string Title => $"Repeat {_powerCard.Title} on {_space.Label} for {_cost} energy.";

	public string Text => Title;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => _cost <= spirit.Energy && _powerCard.CouldActivateDuring( speed, spirit );

	public async Task ActivateAsync( Spirit self ) {
		// if we can affort it
		if( _cost <= self.Energy) {
			// Pay and use
			self.Energy -= _cost;
			await _powerCard.InvokeOn( self.Target(_space) );
		}
		// Next
		++_cost;
		self.AddActionFactory( this );
	}

}
