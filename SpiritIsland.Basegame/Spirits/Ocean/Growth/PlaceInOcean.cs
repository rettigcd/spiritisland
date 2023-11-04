namespace SpiritIsland.Basegame;

public class PlaceInOcean : GrowthActionFactory {

	public override Task ActivateAsync( SelfCtx ctx ) {
		var oceanSpaces = GameState.Current.Island.Boards
			.Select( b=>b.Spaces_Existing.Single(s=>s.IsOcean ) )
			.Tokens()
			.ToArray();
		return Cmd.PlacePresenceOn( oceanSpaces ).Execute( ctx );
	}

}