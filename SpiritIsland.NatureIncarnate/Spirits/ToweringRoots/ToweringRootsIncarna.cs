namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsIncarna 
	: Incarna
	, IHandleTokenAdded	// Empowers if it meets 3 vitality
	, IStopInvaderDamage, IModifyDahanDamage, IModifyRemovingToken	// Stop damage to dahan,invaders,beast
	, ISkipBuilds
	
{
	public ToweringRootsIncarna():base("TRotJ", Img.TRotJ_Incarna_Empowered, Img.TRotJ_Incarna ) { }

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		if( !Empowered && args.Added == Token.Vitality && args.To[Token.Vitality] == 3)
			Empowered = true;
	}

	#region Don't damage Beasts

	public void ModifyRemoving( RemovingTokenArgs args ) {
		if(args.Reason == RemoveReason.Destroyed
			&& args.Token.Class.IsOneOf( Token.Beast, Human.Dahan, Human.Explorer, Human.Town, Human.City )
		)
			args.Count = 0;
	}

	#endregion

	#region Don't damage Dahan
	void IModifyDahanDamage.Modify( DamagingTokens notification ) => notification.TokenCountToReceiveDamage = 0;
	#endregion

	#region ISkipBuilds
	public UsageCost Cost => UsageCost.Free;
	public Task<bool> Skip( SpaceState space ) => Task.FromResult( Empowered );
	#endregion

}
