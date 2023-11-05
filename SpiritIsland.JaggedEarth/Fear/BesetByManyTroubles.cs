namespace SpiritIsland.JaggedEarth;

public class BesetByManyTroubles : FearCardBase, IFearCard {

	public const string Name = "Beset by Many Troubles";
	public string Text => Name;

	[FearLevel(1, "In each land with Badlands/Beast/Disease/Wilds/Strife, Defend 3." )]
	public Task Level1( GameCtx ctx ) {
		return Cmd.Defend(3)
			.In().EachActiveLand().Which( Has.AnyBacToken )
			.ActAsync(ctx);
	}

	[FearLevel(2, "In each land with Badlands/Beast/Disease/Wilds/Strife, or adjacent to 3 or more such tokens, Defend 5." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Defend(5)
			.In().EachActiveLand().Which( Has.AnyBacTokenOrAdjacentTo(3)  )
			.ActAsync( ctx );

	[FearLevel(3, "Every Badlands/Beast/Disease/Wilds/Strife grants Defend 3 in its land and adjacent lands." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Defend(3)
			.In().EachActiveLand().Which( Has.AnyBacTokenOrAdjacentTo(1) )
			.ActAsync( ctx );
}