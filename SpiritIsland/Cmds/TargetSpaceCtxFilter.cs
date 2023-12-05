namespace SpiritIsland;

using XFilter = TargetSpaceCtxFilter;

public class TargetSpaceCtxFilter {

	public readonly Func<TargetSpaceCtx, bool> Filter;
	public readonly string Description;

	public TargetSpaceCtxFilter( string description, Func<TargetSpaceCtx, bool> filter ) {
		Description = description;
		Filter = filter;
	}

}

public class SpiritFilter {

	public readonly Func<Spirit, bool> Filter;
	public readonly string Description;

	public SpiritFilter( string description, Func<Spirit, bool> filter ) {
		Description = description;
		Filter = filter;
	}

}


public static class Has {

	// Dahan
	static public XFilter Dahan => new XFilter( "has dahan", x => x.Dahan.Any );
	static public XFilter TwoOrMoreDahan => new XFilter( "has 2 or more Dahan", x => 2 <= x.Tokens.Dahan.CountAll );
	static public XFilter DahanOrIsAdjacentTo3 => new XFilter( "has Dahan or is adjacent to 3", ( TargetSpaceCtx ctx ) => ctx.Tokens.Dahan.Any|| 3 <= ctx.AdjacentCtxs.Sum( adj => adj.Dahan.CountAll ) );

	// Invaders
	static public XFilter Invaders           => new XFilter( "has invaders", x => x.HasInvaders );
	static public XFilter Only1ExplorerTown  => new XFilter( "has only 1 explorer or town", x => x.Tokens.SumAny( Human.Explorer_Town ) == 1 && !x.Tokens.Has( Human.City ) );
	static public XFilter TwoOrFewerInvaders => new XFilter( "has 2 or fewer invaders", x => x.Tokens.SumAny( Human.Invader ) <= 2 );
	static public XFilter AtLeastN( int count, params ITokenClass[] tokenClasses ) => new TargetSpaceCtxFilter( $"{count} or more {tokenClasses.Select( x => x.Label ).Join( "/" )}", x => count <= x.Tokens.SumAny( tokenClasses ) );
	static public XFilter TownOrCity         => new XFilter( "has town/city", ctx => ctx.Tokens.HasAny( Human.Town_City ) );
	static public XFilter NoCity             => new XFilter( "has no City", x => !x.Tokens.Has( Human.City ) );
	static public XFilter City               => new XFilter( "has city", x => x.Tokens.Has( Human.City ) );
	static public XFilter Strife             => new XFilter( "has city", x => x.Tokens.HumanOfAnyTag().Any(x=>0<x.StrifeCount) );
	static public XFilter Blight             => new XFilter( "has blight", x => x.Tokens.Blight.Any );
	static public TargetSpaceCtxFilter InlandWithNoTownOrCity => new TargetSpaceCtxFilter(	"an inland land with no town/city",	x => x.IsInland && !x.Tokens.HasAny( Human.Town_City ));


	// Presence
	static public XFilter AnySpiritPresence => new XFilter( "has spirit presence", ctx => ctx.Tokens.OfTag(TokenCategory.Presence).Any() );
	static public XFilter YourPresence => new XFilter( "with presence", x => x.Presence.IsHere );
	static public XFilter MySacredSite => new XFilter( "with sacred site", x => x.Presence.IsSelfSacredSite );

	// BAC tokens
	static public XFilter BeastDiseaseOrDahan                   => new XFilter( "has beast, disease, or dahan", ctx => ctx.Tokens.Dahan.Any || ctx.Tokens.Beasts.Any || ctx.Tokens.Disease.Any );
	static public XFilter BeastStrifeOrDahan                   => new XFilter( "has beast, strife, or dahan", ctx => ctx.Tokens.Dahan.Any || ctx.Tokens.Beasts.Any || ctx.Tokens.HasStrife );
	static public XFilter BeastOrIsAdjacentToBeast              => new XFilter( "has beast or is adjacent to beast", ctx => ctx.Range( 1 ).Any( x => x.Beasts.Any ) );
	static public XFilter Token(ITokenClass tokenClass)          => new XFilter( "has "+tokenClass.Label, ctx => ctx.Tokens.Has(tokenClass) );
	static public XFilter Beast                                 => Token( SpiritIsland.Token.Beast);
	static public XFilter AnyBacToken                           => new XFilter( "has Badlands / Beasts / Disease / Wilds / Strife", ctx=>{ var t=ctx.Tokens; return t.Badlands.Any || t.Beasts.Any || t.Disease.Any || t.Wilds.Any || t.HasStrife; } );
	static public XFilter AnyBacTokenOrAdjacentTo(int adjCount) => new XFilter( "has Badlands / Beasts / Disease / Wilds / Strife or adjacent to "+adjCount,  ctx => 1 <= Count(ctx) || adjCount <= ctx.AdjacentCtxs.Sum(Count) );
	static int Count( TargetSpaceCtx ctx )                     => ctx.Tokens.SumAny( SpiritIsland.Token.Badlands, SpiritIsland.Token.Beast, SpiritIsland.Token.Disease, SpiritIsland.Token.Wilds ) + ctx.Tokens.StrifeCount;

	// Dahan / Invader Mix
	static public XFilter DahanAndExplorerOrTown => new XFilter( "has dahan", ctx => ctx.Tokens.Dahan.Any && ctx.Tokens.HasAny( Human.Explorer_Town ) );
	static public XFilter Two2DahanAndCity => new XFilter( "has 2 dahan", ctx => 2 <= ctx.Tokens.Dahan.CountAll && ctx.Tokens.Has( Human.City ) );
	static public XFilter DahanAndExplorers => new XFilter( "land with Dahan", x => x.Dahan.Any && x.Tokens.Has( Human.Explorer ) );
	static public XFilter BeastDiseaseOr2Dahan => new XFilter( "land with beast, disease or at last 2 dahan", (TargetSpaceCtx spaceCtx ) => { var tokens = spaceCtx.Tokens; return tokens.Beasts.Any || tokens.Disease.Any || 2 <= tokens.Dahan.CountAll; } );
	static public XFilter Disease => new XFilter( "disease", spaceCtx => spaceCtx.Tokens.Disease.Any && spaceCtx.Tokens.HasInvaders() );

	static public XFilter DahanOrAdjacentTo( int adjacentDahanThreshold ) => new XFilter(
		$"a land with dahan or adjacent to at least {adjacentDahanThreshold} dahan",
		ctx => ctx.Dahan.Any || adjacentDahanThreshold <= ctx.AdjacentCtxs.Sum( x => x.Dahan.CountAll )
	);

	static public XFilter DangerousLands => new TargetSpaceCtxFilter( "a land with Badlands/Wilds/Dahan.", ( TargetSpaceCtx ctx ) => ctx.Tokens.Badlands.Any || ctx.Tokens.Wilds.Any || ctx.Tokens.Dahan.Any );

	static public SpiritFilter AtLeastNPresenceOnIsland(int count) => new SpiritFilter(
		$"spirit who has {count} Presence on the isalnd",
		spirit => count <= spirit.Presence.TotalOnIsland()
	);

}

public static class Is {
	static public XFilter AnyLand => new XFilter( "any land", _ => true );
	static public XFilter Inland => new XFilter( "Inland", x => x.IsInland );
	static public XFilter Coastal => new XFilter( "coastal land", ctx => ctx.IsCoastal );
	static public XFilter AdjacentToBlight => new XFilter( "land adjacent to blight", spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx => adjCtx.Tokens.Blight.Any ) );
	static public XFilter NotRavageCardMatch => new XFilter( "land that does not match Ravage card", ( TargetSpaceCtx spaceCtx ) => !GameState.Current.InvaderDeck.Ravage.Cards.Any( card => card.MatchesCard( spaceCtx.Tokens ) ) );
	static public XFilter NotBuildCardMatch => new XFilter( "land that does not match Build card", ( TargetSpaceCtx spaceCtx ) => !GameState.Current.InvaderDeck.Build.Cards.Any( card => card.MatchesCard( spaceCtx.Tokens ) ) );
	static public XFilter RavageCardMatch => new XFilter( "matching a Ravage card", MatchingRavageCardImp );
	static bool MatchingRavageCardImp( TargetSpaceCtx ctx ) => GameState.Current.InvaderDeck.Ravage.Cards.Any( card => card.MatchesCard( ctx.Tokens ) );

	// Spirits 
	static public SpiritFilter AnySpirit => new SpiritFilter( "Spirit", _ => true );
}