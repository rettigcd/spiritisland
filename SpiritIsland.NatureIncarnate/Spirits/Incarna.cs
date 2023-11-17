namespace SpiritIsland.NatureIncarnate;

public class Incarna : IIncarnaToken, ITokenClass, ITrackMySpaces {

	readonly string _abreviation;
	readonly Img _notEmpowered;
	readonly Img _empowered;

	public Incarna( string abrev, Img notEmpowered, Img empowered ) {
		_abreviation = abrev;
		_notEmpowered = notEmpowered;
		_empowered = empowered;
	}

	public SpaceState? Space => GameState.Current.Tokens.Spaces_Existing( this )
		.Select( s => s.Tokens )
		.FirstOrDefault();

	public bool Empowered { get; set; }

	public Img Img => Empowered ? _empowered: _notEmpowered;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => _abreviation + (Empowered ? "+" : "-");

	ITokenClass IToken.Class => this;

	#region IEntityClass properties
	public string Label => "My incarna???";

	public bool HasTag(ITag tag) => tag == TokenCategory.Incarna; // Class and entity

	#endregion

	public async Task MoveTo( Space destination, bool allowAdd ) {
		if(Space != null)
			await this.On( Space.Space ).MoveTo( destination );
		else if(allowAdd)
			await destination.Tokens.Add( this, 1 );

	}

}
