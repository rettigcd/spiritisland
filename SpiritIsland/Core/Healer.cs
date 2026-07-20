namespace SpiritIsland;

public sealed class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public Task TimePasses( GameState gameState ) {
		foreach(Space ss in ActionScope.Current.Spaces_Unfiltered)
			HealSpace( ss );
		_skipInvadersOn.Clear();
		_skipDahanOn.Clear();
		return Task.CompletedTask;
	}

	public void HealSpace( Space space ) {
		// Invaders
		if( !_skipInvadersOn.Contains(space) ){
			HealGroup( Human.Town );
			HealGroup( Human.City );
		}

		// Dahan
		if( !_skipDahanOn.Contains(space) )
			HealGroup( Human.Dahan );

		void HealGroup( HumanTokenClass group ) {
			foreach(HumanToken humanToken in space.HumanOfTag( group ).ToArray())
				if(0<humanToken.Damage)
					space.AllHumans(humanToken).Adjust(x=>x.Healthy);
		}

	}

	public void SkipInvadersOn( Space space ) => _skipInvadersOn.Add( space );
	public void SkipDahanOn( Space space ) => _skipDahanOn.Add( space );

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	readonly HashSet<Space> _skipInvadersOn = [];
	readonly HashSet<Space> _skipDahanOn = [];

	#region Json

	/// <summary>
	/// [ skipInvadersOnLabels, skipDahanOnLabels ] - cleared every TimePasses(), so this only matters
	/// for a save landing mid-round. Space compares by SpaceSpec.Label (see SpaceSpec.Equals), so a
	/// freshly-built Space wrapper for the same label round-trips correctly through the HashSet.
	/// </summary>
	public JsonArray ToJson() => new JsonArray(
		new JsonArray( _skipInvadersOn.Select( s => (JsonNode)s.SpaceSpec.Label ).ToArray() ),
		new JsonArray( _skipDahanOn.Select( s => (JsonNode)s.SpaceSpec.Label ).ToArray() )
	);

	/// <summary> Restores onto this existing Healer (GameState owns exactly one) - see GameState.Healer. </summary>
	public void RestoreFromJson( JsonArray json, ISerializationContext ctx ) {
		_skipInvadersOn.Clear();
		foreach( JsonNode? label in (JsonArray)json[0]! )
			_skipInvadersOn.Add( ctx.Tokens[ ctx.SpaceSpecOrFakeByLabel( label!.GetValue<string>() ) ] );

		_skipDahanOn.Clear();
		foreach( JsonNode? label in (JsonArray)json[1]! )
			_skipDahanOn.Add( ctx.Tokens[ ctx.SpaceSpecOrFakeByLabel( label!.GetValue<string>() ) ] );
	}

	#endregion

}
