namespace SpiritIsland;

using XFilter = TargetSpaceCtxFilter;

public static class Has {

	// Dahan
	static public XFilter Dahan => new XFilter( "has dahan", x => x.Dahan.Any );
	static public XFilter TwoOrMoreDahan => new XFilter( "has 2 or more Dahan", x => 2 <= x.Space.Dahan.CountAll );
	static public XFilter DahanOrIsAdjacentTo3 => new XFilter( "has Dahan or is adjacent to 3", ( TargetSpaceCtx ctx ) => ctx.Space.Dahan.Any|| 3 <= ctx.AdjacentCtxs.Sum( adj => adj.Dahan.CountAll ) );

	// Invaders
	static public XFilter Invaders           => new XFilter( "has invaders", x => x.HasInvaders );
	static public XFilter Only1ExplorerTown  => new XFilter( "has only 1 explorer or town", x => x.Space.SumAny( Human.Explorer_Town ) == 1 && !x.Space.Has( Human.City ) );
	static public XFilter TwoOrFewerInvaders => new XFilter( "has 2 or fewer invaders", x => x.Space.SumAny( Human.Invader ) <= 2 );
	static public XFilter AtLeastN( int count, params ITokenClass[] tokenClasses ) => new TargetSpaceCtxFilter( $"{count} or more {tokenClasses.Select( x => x.Label ).Join( "/" )}", x => count <= x.Space.SumAny( tokenClasses ) );
	static public XFilter TownOrCity         => new XFilter( "has town/city", ctx => ctx.Space.HasAny( Human.Town_City ) );
	static public XFilter NoCity             => No(Human.City);
	static public XFilter No( ITag tag )     => new XFilter( $"has no {tag.Label}", x=>!x.Space.Has( tag ) );
	static public XFilter City               => new XFilter( "has city", x => x.Space.Has( Human.City ) );
	static public XFilter Strife             => new XFilter( "has city", x => x.Space.HumanOfAnyTag().Any(x=>0<x.StrifeCount) );
	static public XFilter Blight             => new XFilter( "has blight", x => x.Space.Blight.Any );
	static public TargetSpaceCtxFilter InlandWithNoTownOrCity => new TargetSpaceCtxFilter(	"an inland land with no town/city",	x => x.IsInland && !x.Space.HasAny( Human.Town_City ));


	// Presence
	static public XFilter AnySpiritPresence => new XFilter( "has spirit presence", ctx => ctx.Space.OfTag(TokenCategory.Presence).Length != 0 );
	static public XFilter YourPresence => new XFilter( "with presence", x => x.Presence.IsHere );
	static public XFilter MySacredSite => new XFilter( "with sacred site", x => x.Presence.IsSelfSacredSite );

	// BAC tokens
	static public XFilter BeastDiseaseOrDahan                   => new XFilter( "has beast, disease, or dahan", ctx => ctx.Space.Dahan.Any || ctx.Space.Beasts.Any || ctx.Space.Disease.Any );
	static public XFilter BeastStrifeOrDahan                   => new XFilter( "has beast, strife, or dahan", ctx => ctx.Space.Dahan.Any || ctx.Space.Beasts.Any || ctx.Space.HasStrife );
	static public XFilter BeastOrIsAdjacentToBeast              => new XFilter( "has beast or is adjacent to beast", ctx => ctx.Range( 1 ).Any( x => x.Beasts.Any ) );
	static public XFilter Token(ITokenClass tokenClass)          => new XFilter( "has "+tokenClass.Label, ctx => ctx.Space.Has(tokenClass) );
	static public XFilter Beast                                 => Token( SpiritIsland.Token.Beast);
	static public XFilter AnyBacToken                           => new XFilter( "has Badlands / Beasts / Disease / Wilds / Strife", ctx=>{ var t=ctx.Space; return t.Badlands.Any || t.Beasts.Any || t.Disease.Any || t.Wilds.Any || t.HasStrife; } );
	static public XFilter AnyBacTokenOrAdjacentTo(int adjCount) => new XFilter( "has Badlands / Beasts / Disease / Wilds / Strife or adjacent to "+adjCount,  ctx => 1 <= Count(ctx) || adjCount <= ctx.AdjacentCtxs.Sum(Count) );
	static int Count( TargetSpaceCtx ctx )                     => ctx.Space.SumAny( SpiritIsland.Token.Badlands, SpiritIsland.Token.Beast, SpiritIsland.Token.Disease, SpiritIsland.Token.Wilds ) + ctx.Space.StrifeCount;

	// Dahan / Invader Mix
	static public XFilter DahanAndExplorerOrTown => new XFilter( "has dahan", ctx => ctx.Space.Dahan.Any && ctx.Space.HasAny( Human.Explorer_Town ) );
	static public XFilter Two2DahanAndCity => new XFilter( "has 2 dahan", ctx => 2 <= ctx.Space.Dahan.CountAll && ctx.Space.Has( Human.City ) );
	static public XFilter DahanAndExplorers => new XFilter( "land with Dahan", x => x.Dahan.Any && x.Space.Has( Human.Explorer ) );
	static public XFilter BeastDiseaseOr2Dahan => new XFilter( "land with beast, disease or at last 2 dahan", (TargetSpaceCtx spaceCtx ) => { var tokens = spaceCtx.Space; return tokens.Beasts.Any || tokens.Disease.Any || 2 <= tokens.Dahan.CountAll; } );
	static public XFilter Disease => new XFilter( "disease", spaceCtx => spaceCtx.Space.Disease.Any );

	static public XFilter DiseaseAndInvaders => new XFilter("disease", spaceCtx => spaceCtx.Space.Disease.Any && spaceCtx.Space.HasInvaders());

	static public XFilter DahanOrAdjacentTo( int adjacentDahanThreshold ) => new XFilter(
		$"a land with dahan or adjacent to at least {adjacentDahanThreshold} dahan",
		ctx => ctx.Dahan.Any || adjacentDahanThreshold <= ctx.AdjacentCtxs.Sum( x => x.Dahan.CountAll )
	);

	static public XFilter DangerousLands => new TargetSpaceCtxFilter( "a land with Badlands/Wilds/Dahan.", ( TargetSpaceCtx ctx ) => ctx.Space.Badlands.Any || ctx.Space.Wilds.Any || ctx.Space.Dahan.Any );

	static public SpiritFilter AtLeastNPresenceOnIsland(int count) => new SpiritFilter(
		$"spirit who has {count} Presence on the isalnd",
		spirit => count <= spirit.Presence.TotalOnIsland()
	);


}
