namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsIncarna( Spirit spirit )
	: Incarna(spirit, "TRotJ", Img.TRotJ_Incarna_Empowered, Img.TRotJ_Incarna )
	, IHandleTokenAdded	// Empowers if it meets 3 vitality
	, IAdjustDamageToInvaders_ByStoppingIt, IAdjustDamageToDahan, IModifyRemovingToken	// Stop damage to dahan,invaders,beast
	, ISkipBuilds
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

	// Serialization (ISerializableSpaceEntity) is inherited from Incarna as-is: it already tags
	// uniformly as "Incarna" and resolves back to ctx.SpiritAt(idx).Presence.Incarna - the spirit's
	// one true Incarna instance, whatever its concrete subclass. This class previously registered
	// its own "ToweringRootsIncarna" tag that constructed a brand-new instance on every restore
	// instead of reusing that singleton, silently orphaning the real spirit.Presence.Incarna (empty,
	// untracked) while a disconnected duplicate sat on the board - the root cause of Incarna ending
	// up "in 2 places at the same time" after a rewind.

}
