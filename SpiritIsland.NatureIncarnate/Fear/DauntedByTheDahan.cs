namespace SpiritIsland.NatureIncarnate;

public class DauntedByTheDahan : FearCardBase, IFearCard {

	public const string Name = "Daunted by the Dahan";
	public string Text => Name;

	[FearLevel(1, "1 Fear / board with Invaders & Dahan. Invaders do -6 Damage (per land) during Ravage." )]
	public Task Level1( GameState ctx ) 
		=> Cmd.Multiple(
			OneFearPerBoardWithInvadersAndDahan,
			LessInvaderDamage
		).ActAsync(ctx);

	[FearLevel( 2, "1 Fear / board with Invaders & Dahan. Defend 3 in Dahan lands. Invaders do -6 Damage (per land)." )]
	public Task Level2(	GameState ctx )
		=> Cmd.Multiple(
			OneFearPerBoardWithInvadersAndDahan,
			LessInvaderDamage,
			DahanDefend3
		).ActAsync(ctx);

	[FearLevel( 3, "1 Fear / board with Invaders & Dahan. Defend 3 & Isolate Dahan lands. Invaders do -6 Damage (per land)." )]
	public Task Level3( GameState ctx )
		=> Cmd.Multiple(
			OneFearPerBoardWithInvadersAndDahan,
			LessInvaderDamage,
			DahanDefend3,
			DahanIsolate
		).ActAsync(ctx);

	static IActOn<GameState> OneFearPerBoardWithInvadersAndDahan => new BaseCmd<GameState>(
		"1 Fear/board with Invaders & Dahan",
		gs => {
			SpaceState[] spaces = gs.Spaces_Existing.ToArray();
			IEnumerable<Board> withDahan = spaces.Where(s=>s.Dahan.Any).SelectMany(s=>s.Space.Boards);
			IEnumerable<Board> withInvaders = spaces.Where(s=>s.HasInvaders()).SelectMany(s=>s.Space.Boards);
			int fearCount = withDahan.Intersect(withInvaders).Distinct().Count();
			gs.Fear.AddDirect(new FearArgs( fearCount ) );
		}
	);

	static IActOn<GameState> LessInvaderDamage => new BaseCmd<GameState>(
		"Attacker do -6 Damage per land.",
		gs => gs.Tokens.AddIslandMod(new AdjustDamageFromAttackers( _=> -6 ))
	);

	static IActOn<GameState> DahanDefend3 => new BaseCmd<GameState>(
		"Each land with Dahan, Defend 3",
		gs => {
			foreach(SpaceState? ss in gs.Spaces_Existing)
				if(ss.Dahan.Any)
					ss.Defend.Add(3);
		}
	);

	static IActOn<GameState> DahanIsolate => new BaseCmd<GameState>(
		"Each land with Dahan, Isolate",
		gs => {
			foreach(SpaceState? ss in gs.Spaces_Existing)
				if(ss.Dahan.Any)
					ss.Isolate();
		}
	);

}

