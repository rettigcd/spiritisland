namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Repeats the played card endlessly, costing 1 more each time.
/// </summary>
public class RelentlessRepeater : IActionFactory {

	static public SpecialRule Rule => new SpecialRule("Relentless Punishment","If you had at least 3 Presence in the origin land, you may Repeat a Power Card any number of times on the same target land(s) by paying its cost +1/previous use.");

	int _cost;
	readonly PowerCard _powerCard;
	readonly Space _space;

	public string Name => $"Repeat {_powerCard.Name} on {_space.Text} for {_cost} energy.";

	public string Text => Name;

	public RelentlessRepeater(PowerCard powerCard, Space space ) {
		_powerCard = powerCard;
		_cost = powerCard.Cost + 1; // already used once
		_space = space;
	}

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => _cost <= spirit.Energy && _powerCard.CouldActivateDuring( speed, spirit );

	public async Task ActivateAsync( SelfCtx ctx ) {
		// if we can affort it
		if( _cost <= ctx.Self.Energy) {
			// Pay and use
			ctx.Self.Energy -= _cost;
			await _powerCard.InvokeOn( ctx.Target(_space) );
		}
		// Next
		++_cost;
		ctx.Self.AddActionFactory( this );
	}

}
