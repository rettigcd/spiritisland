namespace SpiritIsland.Basegame;

public class PlaceInOcean : SpiritAction {

	public PlaceInOcean() : base( "PlaceInOcean" ) { }

	public override Task ActAsync( SelfCtx ctx ) {
		var oceanSpaces = GameState.Current.Island.Boards
			.Select( b=>b.Spaces_Existing.Single(s=>s.IsOcean ) )
			.Tokens()
			.ToArray();
		return Cmd.PlacePresenceOn( oceanSpaces ).ActAsync( ctx );
	}

}