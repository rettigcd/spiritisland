namespace SpiritIsland.NatureIncarnate;

public class BreathIncarna : IIncarnaToken, IEntityClass
	, IHandleTokenAdded
	, ITrackMySpaces
{
	public Img Img => Empowered ? Img.BoDDYS_Incarna_Empowered : Img.BoDDYS_Incarna;
	public bool Empowered { get; set; }

	public IEntityClass Class => this;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => "BoDDyS" + (Empowered ? "+" : "-");

	#region IEntityClass properties
	public string Label => "My incarna???";

	public TokenCategory Category => TokenCategory.Incarna;

	#endregion

	#region tracking location
	public SpaceState? Space => GameState.Current.Tokens.Spaces_Existing( this )
		.Select( s => s.Tokens )
		.FirstOrDefault(); 

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		if( !Empowered && args.Added == Token.Vitality && args.To[Token.Vitality] == 3)
			Empowered = true;
	}

	#endregion


	/// <param name="allowAdd">If is not on board, may be added.</param>
	public async Task MoveTo( Space space, bool allowAdd ) {
		if(Space != null)
			await new SpaceToken( Space.Space, this ).MoveTo( space );
		else if( allowAdd )
			await space.Tokens.Add( this, 1 );

	}

}
