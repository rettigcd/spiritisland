namespace SpiritIsland;

public class Reclaim1InsteadOfDiscard : IRunWhenTimePasses, ISerializableTimePassesAction {

	readonly Spirit spirit;
	readonly PowerCard[] _purchased;

	public Reclaim1InsteadOfDiscard( Spirit spirit ) {
		this.spirit = spirit;
		_purchased = [.. spirit.InPlay];
	}

	// Used by FromJson to restore the exact captured snapshot, which may no longer match the target
	// spirit's current InPlay by the time a game is restored.
	Reclaim1InsteadOfDiscard( Spirit spirit, PowerCard[] purchased ) {
		this.spirit = spirit;
		_purchased = purchased;
	}

	async Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		var reclaimCard = await spirit.SelectPowerCard( "Reclaim 1 played card", 1, _purchased, CardUse.Reclaim, Present.Done );
		if(reclaimCard != null)
			spirit.Reclaim(reclaimCard);
	}
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Early;

	#region Json

	const string Tag = "Reclaim1InsteadOfDiscard";

	/// <summary>
	/// [ Tag, spiritIndex, purchasedCards ] - no identity problem to solve (unlike most of this
	/// section): this is a fresh value snapshot taken at construction, not a shared live reference, so a
	/// freshly-reconstructed instance is completely equivalent. spirit resolves via ctx.IndexOf/SpiritAt,
	/// _purchased resolves each card via PowerCardRegistry (section 4 - PowerCard is fully immutable).
	/// </summary>
	JsonArray ISerializableTimePassesAction.ToJson( ISerializationContext ctx ) => new JsonArray(
		Tag, ctx.IndexOf( spirit ), new JsonArray( _purchased.Select( c => (JsonNode)c.ToJson() ).ToArray() )
	);

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> TimePassesActionRegistry.Register( Tag, ( json, ctx ) => new Reclaim1InsteadOfDiscard(
			ctx.SpiritAt( json[1]!.GetValue<int>() ),
			( (JsonArray)json[2]! ).Select( n => PowerCardRegistry.Deserialize( n! ) ).ToArray()
		) );

	#endregion Json

}