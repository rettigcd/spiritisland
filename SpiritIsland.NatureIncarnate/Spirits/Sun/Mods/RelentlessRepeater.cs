namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Repeats the played card endlessly, costing 1 more each time.
/// </summary>
public class RelentlessRepeater( PowerCard powerCard, Space space ) : IActionFactory, ISerializableActionFactory {

	int _cost = powerCard.Cost + 1;
	readonly PowerCard _powerCard = powerCard;
	readonly Space _space = space;

	// Used by FromJson to restore the exact captured cost, which increments on every activation and so
	// may no longer match powerCard.Cost+1 by the time a game is restored.
	RelentlessRepeater( PowerCard powerCard, Space space, int cost ) : this( powerCard, space ) {
		_cost = cost;
	}

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

	#region Json

	/// <summary>
	/// [ Tag, cost, powerCard, spaceLabel ] - no identity problem: ActivateAsync re-adds `this` (the
	/// same live instance) to keep the chain going, but nothing outside this class ever compares a
	/// RelentlessRepeater by reference (unlike PourDownPower.RepeatLandCardForCost/MarkedBeastMover,
	/// whose owning mods count used actions by reference - see docs/GameSerialization-Roadmap.md
	/// section 2), so a freshly-reconstructed instance is fully equivalent. _powerCard resolves via
	/// PowerCardRegistry (section 4); _space resolves via ISerializationContext.Tokens/SpaceSpecByLabel.
	/// </summary>
	public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( "RelentlessRepeater", _cost, _powerCard.ToJson(), _space.SpaceSpec.Label );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> ActionFactoryRegistry.Register( "RelentlessRepeater", ( json, ctx ) => new RelentlessRepeater(
			PowerCardRegistry.Deserialize( json[2]! ),
			ctx.Tokens[ ctx.SpaceSpecByLabel( json[3]!.GetValue<string>() ) ],
			json[1]!.GetValue<int>()
		) );

	#endregion Json

}
