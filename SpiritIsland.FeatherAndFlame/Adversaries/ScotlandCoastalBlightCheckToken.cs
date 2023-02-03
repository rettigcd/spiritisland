namespace SpiritIsland.FeatherAndFlame;

class ScotlandCoastalBlightCheckToken : IHandleTokenAdded, IToken {

	const string Name = "Runoff and Bilgewater";

	public ScotlandCoastalBlightCheckToken() {}

	public TokenClass Class => ActionModTokenClass.Singleton;

	public async Task HandleTokenAdded( ITokenAddedArgs args ) {
		// After a Ravage Action adds Blight to a Coastal Land,
		// add 1 Blight to that board's Ocean (without cascading).
		if(args.Token == Token.Blight && args.Reason == AddReason.Ravage) {
			var space = args.AddedTo.Adjacent
				.First( adj => adj.Space.IsOcean && adj.Space.Board == args.AddedTo.Space.Board );
			await space.Blight.Bind( args.ActionScope ).Add( 1, AddReason.Ravage ); // !!! won't this cause cascading???
			args.GameState.Log(new SpiritIsland.Log.Debug( $"{Name} Blight on {args.AddedTo.Space.Text} caused additional blight on {space.Space.Text}"));
		}
	}
}
