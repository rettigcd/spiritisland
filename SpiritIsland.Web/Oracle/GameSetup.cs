namespace SpiritIsland.Web.Oracle;

/// <summary> What may be chosen when starting a game. </summary>
/// <param name="Seed">Null picks a random one.</param>
public record NewGameRequest( string Spirit, string Board, int? Seed = null, string? Adversary = null, int AdversaryLevel = 0 );

/// <summary> The expansions available, and the setup choices they expose. </summary>
public static class GameSetup {

	public static readonly GameBuilder Builder = new(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider(),
		new Horizons.GameComponentProvider()
	);

	public static string[] Spirits => Builder.SpiritNames;
	public static string[] Boards => BoardFactory.AvailableBoards;

	public static Dictionary<string, int[]> Adversaries => Builder.AdversaryNames.ToDictionary(
		name => name,
		name => Builder.GetAdversaryBuilder( name ).Levels.Select( l => l.Level ).ToArray()
	);

	/// <exception cref="ArgumentException">the request names something that doesn't exist</exception>
	public static GameConfiguration Configure( NewGameRequest req ) {
		if( !Spirits.Contains( req.Spirit ) )
			throw new ArgumentException( $"Unknown spirit '{req.Spirit}'. Options: {string.Join( ", ", Spirits )}" );
		if( !Boards.Contains( req.Board ) )
			throw new ArgumentException( $"Unknown board '{req.Board}'. Options: {string.Join( ", ", Boards )}" );

		var cfg = new GameConfiguration { ShuffleNumber = req.Seed ?? Random.Shared.Next() }
			.ConfigSpirits( req.Spirit )
			.ConfigBoards( req.Board );

		if( !string.IsNullOrEmpty( req.Adversary ) ) {
			if( !Adversaries.TryGetValue( req.Adversary, out int[]? levels ) )
				throw new ArgumentException( $"Unknown adversary '{req.Adversary}'. Options: {string.Join( ", ", Adversaries.Keys )}" );
			if( !levels.Contains( req.AdversaryLevel ) )
				throw new ArgumentException( $"Adversary level must be one of: {string.Join( ", ", levels )}" );
			cfg.ConfigAdversary( req.Adversary, req.AdversaryLevel );
		}
		return cfg;
	}
}
