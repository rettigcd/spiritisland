namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsIncarna : IIncarnaToken, IEntityClass
	, IHandleTokenAdded	// Track current space of Incarna
	, IStopInvaderDamage, IStopDahanDamage, IModifyRemovingToken	// Stop damage to dahan,invaders,beast
	, ISkipBuilds
	, ITrackMySpaces
{
	public Img Img => Empowered ? Img.TRotJ_Incarna_Empowered : Img.TRotJ_Incarna;
	public bool Empowered { get; set; }

	public IEntityClass Class => this;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => "TRotJ" + (Empowered ? "+" : "-");

	#region IEntityClass properties
	public string Label => "My incarna???";

	public TokenCategory Category => TokenCategory.Incarna;

	#endregion

	#region tracking location
	public SpaceState? Space => GameState.Current.Tokens.Spaces_Existing(this).FirstOrDefault();

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		if( !Empowered && args.Added == Token.Vitality && args.To[Token.Vitality] == 3)
			Empowered = true;
	}

	#endregion

	#region Don't damage Beasts

	public void ModifyRemoving( RemovingTokenArgs args ) {
		if(args.Token == Token.Beast)
			args.Count = 0;
	}

	#endregion

	#region ISkipBuilds
	public UsageCost Cost => UsageCost.Free;
	public Task<bool> Skip( SpaceState space ) => Task.FromResult( Empowered );
	#endregion

}
