namespace SpiritIsland.Serialization;

public class GameStateSerializationContext( GameState gameState ) : ISerializationContext {

	public int IndexOf( Spirit spirit ) => Array.IndexOf( gameState.Spirits, spirit );

	public Spirit SpiritAt( int index ) => gameState.Spirits[index];

	public SpaceSpec SpaceSpecByLabel( string label ) => gameState.SpaceSpecs_Unfiltered.First( s => s.Label == label );

	public SpaceSpec SpaceSpecOrFakeByLabel( string label )
		=> gameState.SpaceSpecs_Unfiltered.FirstOrDefault( s => s.Label == label ) ?? new FakeSpace( label );

	public Board BoardByName( string name ) => gameState.Island.Boards.First( b => b.Name == name );

	public InvaderSlot InvaderSlotByLabel( string label ) => gameState.InvaderDeck.ActiveSlots.First( s => s.Label == label );

	public Tokens_ForIsland Tokens => gameState.Tokens;
	public Healer Healer => gameState.Healer;
	public BlightCard BlightCard => gameState.BlightCard;

	#region ISpaceEntity lookup table (see ISerializationContext's remarks on InternSpaceEntity)

	public string InternSpaceEntity( ISpaceEntity entity ) {
		if( _keysByEntity.TryGetValue( entity, out string? existing ) ) return existing;

		ISerializationContext ctx = this;
		string kind;
		string baseKey;
		JsonNode? detail;
		switch( entity ) {
			case HumanToken humanToken:
				kind = "Human";
				baseKey = humanToken.ToString();
				detail = ctx.SerializeHumanTokenDetails( humanToken );
				break;
			// ISerializableSpaceEntity checked before ITokenClass - some types (e.g. a spirit's own
			// Incarna) implement both, and those are per-instance entities (register via
			// SpaceEntitySerialization), not shared process-wide singletons keyed by Label.
			case ISerializableSpaceEntity serializable:
				kind = "Entity";
				JsonArray fullJson = serializable.ToJson( this );
				baseKey = fullJson[0]!.GetValue<string>();
				detail = fullJson;
				break;
			case ITokenClass tokenClass:
				kind = "Class";
				baseKey = tokenClass.Label; // key IS the label - nothing else needs storing
				detail = null;
				break;
			default:
				throw new NotSupportedException( $"Don't know how to serialize ISpaceEntity of type {entity.GetType()}" );
		}

		string key = baseKey;
		for( int suffix = 2; _lookup.ContainsKey( key ); ++suffix )
			key = $"{baseKey}#{suffix}";

		_keysByEntity[entity] = key;
		_lookup[key] = detail is null ? new JsonArray( kind ) : new JsonArray( kind, detail );
		return key;
	}

	public ISpaceEntity ResolveSpaceEntity( string key ) {
		var entry = (JsonArray)_lookup[key]!;
		ISerializationContext ctx = this;
		return entry[0]!.GetValue<string>() switch {
			"Human" => ctx.DeserializeHumanTokenDetails( (JsonArray)entry[1]! ),
			"Class" => (ISpaceEntity)ctx.TokenClassByLabel( key ),
			"Entity" => (ISpaceEntity)SpaceEntitySerialization.Deserialize( (JsonArray)entry[1]!, this ),
			var kind => throw new NotSupportedException( $"Unknown ISpaceEntity kind '{kind}'" )
		};
	}

	public JsonObject LookupTableToJson() {
		var obj = new JsonObject();
		foreach( var (key, entry) in _lookup )
			obj[key] = entry.DeepClone();
		return obj;
	}

	public void LoadLookupTable( JsonObject json ) {
		foreach( var (key, entry) in json )
			_lookup[key] = entry!.DeepClone();
	}

	readonly Dictionary<ISpaceEntity, string> _keysByEntity = [];
	readonly Dictionary<string, JsonNode> _lookup = [];

	#endregion

}
