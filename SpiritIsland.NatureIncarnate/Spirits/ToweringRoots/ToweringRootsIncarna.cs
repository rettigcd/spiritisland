namespace SpiritIsland.NatureIncarnate;

public class ToweringRootsIncarna : IIncarnaToken, IEntityClass, IHandleTokenAdded, IHandleTokenRemoved  {
	public Img Img => Img.TRotJ_Incarna;

	public IEntityClass Class => this;

	public string Text => "#";

	#region IEntityClass properties
	public string Label => "My incarna???";

	public TokenCategory Category => TokenCategory.Incarna;

	#endregion

	#region tracking location
	public SpaceState? Space { get; set; }

	public void HandleTokenAdded( ITokenAddedArgs args ) {
		TrackAdding( args );
		if( args.Added == Token.Vitality && args.To[Token.Vitality] == 3) {
			// Do Power-Up here
		}
	}

	protected void TrackAdding( ITokenAddedArgs args ) {
		if(Equals( args.Added )) {
			if(Space != null) throw new InvalidOperationException( "Must first remove token before adding it to another space." );
			Space = args.To;
		}
	}

	public void HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(!Equals( args.Removed )) return;
		if(Space == null) throw new InvalidOperationException( "Can't remove.  Space is already null." );
		Space = null;
	}
	#endregion
}
