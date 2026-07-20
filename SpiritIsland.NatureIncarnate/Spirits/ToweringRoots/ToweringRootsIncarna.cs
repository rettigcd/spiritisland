namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsIncarna( Spirit spirit )
	: Incarna(spirit, "TRotJ", Img.TRotJ_Incarna_Empowered, Img.TRotJ_Incarna )
	, IHandleTokenAdded	// Empowers if it meets 3 vitality
	, IAdjustDamageToInvaders_ByStoppingIt, IAdjustDamageToDahan, IModifyRemovingToken	// Stop damage to dahan,invaders,beast
	, ISkipBuilds
	, ISerializableSpaceEntity
{
	string ISkipBuilds.Text => SpaceAbreviation;

	public Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		if( !Empowered && args.Added == Token.Vitality && to[Token.Vitality] == 3)
			Empowered = true;
		return Task.CompletedTask;
	}

	#region Don't damage Beasts

	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if(args.Reason == RemoveReason.Destroyed
			&& args.Token.Class.IsOneOf( Token.Beast, Human.Dahan, Human.Explorer, Human.Town, Human.City )
		)
			args.Count = 0;
		return Task.CompletedTask;
	}

	#endregion

	#region Don't damage Dahan
	void IAdjustDamageToDahan.Modify( DamagingTokens notification ) => notification.TokenCountToReceiveDamage = 0;
	#endregion

	#region ISkipBuilds
	public UsageCost Cost => UsageCost.Free;
	public Task<bool> Skip( Space space ) => Task.FromResult( Empowered );
	#endregion

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( Self ), Empowered );

	const string Tag = "ToweringRootsIncarna";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new ToweringRootsIncarna( ctx.SpiritAt( (int)json[1]! ) ) { Empowered = json[2]!.GetValue<bool>() } );

}
