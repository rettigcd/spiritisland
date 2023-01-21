
namespace SpiritIsland;

public class TargetSpaceCtxFilter {

	public readonly Func<TargetSpaceCtx, bool> Filter;
	public readonly string Description;

	// !!! pull these all into the Has/Is classes.
	public TargetSpaceCtxFilter( string description, Func<TargetSpaceCtx, bool> filter ) {
		Description = description;
		Filter = filter;
	}

}

public static class Has {

	// Dahan
	static public TargetSpaceCtxFilter Dahan => new TargetSpaceCtxFilter( "has dahan", x => x.Dahan.Any );
	static public TargetSpaceCtxFilter TwoOrMoreDahan => new TargetSpaceCtxFilter( "has 2 or more Dahan", x => 2 <= x.Tokens.Dahan.CountAll );
	static public TargetSpaceCtxFilter DahanOrIsAdjacentTo3 => new TargetSpaceCtxFilter( "has Dahan or is adjacent to 3", ( TargetSpaceCtx ctx ) => ctx.Tokens.Dahan.Any|| 3 <= ctx.AdjacentCtxs.Sum( adj => adj.Dahan.CountAll ) );

	// Invaders
	static public TargetSpaceCtxFilter Invaders => new TargetSpaceCtxFilter( "has invaders", x => x.HasInvaders );
	static public TargetSpaceCtxFilter Only1ExplorerTown => new TargetSpaceCtxFilter( "has only 1 explorer or town", x => x.Tokens.SumAny( Invader.Explorer_Town ) == 1 && !x.Tokens.Has( Invader.City ) );
	static public TargetSpaceCtxFilter TwoOrFewerInvaders => new TargetSpaceCtxFilter( "has 2 or fewer invaders", x => x.Tokens.SumAny( Invader.Any ) <= 2 );
	static public TargetSpaceCtxFilter TwoOrMoreInvaders => new TargetSpaceCtxFilter( "has at least 2 invaders", x => 2 <= x.Tokens.SumAny( Invader.Any ) );
	static public TargetSpaceCtxFilter TownOrCity => new TargetSpaceCtxFilter( "has town/city", ctx => ctx.Tokens.HasAny( Invader.Town_City ) );
	static public TargetSpaceCtxFilter NoCity => new TargetSpaceCtxFilter( "has no City", x => !x.Tokens.Has( Invader.City ) );
	static public TargetSpaceCtxFilter City => new TargetSpaceCtxFilter( "has city", x => x.Tokens.Has( Invader.City ) );
	static public TargetSpaceCtxFilter Strife => new TargetSpaceCtxFilter( "has city", x => x.Tokens.OfAnyHealthClass().Any(x=>0<x.StrifeCount) );

	// Presence
	static public TargetSpaceCtxFilter AnySpiritPresence => new TargetSpaceCtxFilter( "has spirit presence", ctx => ctx.Tokens.OfCategory(TokenCategory.Presence ).Any() );
	static public TargetSpaceCtxFilter YourPresence => new TargetSpaceCtxFilter( "with presence", x => x.Presence.IsHere );
	static public TargetSpaceCtxFilter MySacredSite => new TargetSpaceCtxFilter( "with sacred site", x => x.Presence.IsSelfSacredSite );

	// BAC tokens
	static public TargetSpaceCtxFilter BeastDiseaseOrDahan => new TargetSpaceCtxFilter( "has beast, disease, or dahan", ctx => ctx.Tokens.Dahan.Any || ctx.Tokens.Beasts.Any || ctx.Tokens.Disease.Any );
	static public TargetSpaceCtxFilter BeastOrIsAdjacentToBeast => new TargetSpaceCtxFilter( "has beast or is adjacent to beast", ctx => ctx.Range( 1 ).Any( x => x.Beasts.Any ) );
	static public TargetSpaceCtxFilter Token(TokenClass tokenClass) => new TargetSpaceCtxFilter( "has "+tokenClass.Label, ctx => ctx.Tokens.Has(tokenClass) );
	static public TargetSpaceCtxFilter Beast => Token(TokenType.Beast);
	static public TargetSpaceCtxFilter AnyBacToken => new TargetSpaceCtxFilter( "has Badlands / Beasts / Disease / Wilds / Strife", ctx=>{ var t=ctx.Tokens; return t.Badlands.Any || t.Beasts.Any || t.Disease.Any || t.Wilds.Any || t.HasStrife; } );
	static public TargetSpaceCtxFilter AnyBacTokenOrAdjacentTo(int adjCount) => new TargetSpaceCtxFilter( "has Badlands / Beasts / Disease / Wilds / Strife or adjacent to "+adjCount,  ctx => 1 <= Count(ctx) || adjCount <= ctx.AdjacentCtxs.Sum(Count) );
	static int Count( TargetSpaceCtx ctx ) => ctx.Tokens.SumAny( TokenType.Badlands, TokenType.Beast, TokenType.Disease, TokenType.Wilds ) + ctx.Tokens.StrifeCount;

	// Dahan / Invader Mix
	static public TargetSpaceCtxFilter DahanAndExplorerOrTown => new TargetSpaceCtxFilter( "has dahan", ctx => ctx.Tokens.Dahan.Any && ctx.Tokens.HasAny( Invader.Explorer_Town ) );
	static public TargetSpaceCtxFilter Two2DahanAndCity => new TargetSpaceCtxFilter( "has 2 dahan", ctx => 2 <= ctx.Tokens.Dahan.CountAll && ctx.Tokens.Has( Invader.City ) );
	static public TargetSpaceCtxFilter DahanAndExplorers => new TargetSpaceCtxFilter( "land with Dahan", x => x.Dahan.Any && x.Tokens.Has( Invader.Explorer ) );
	static public TargetSpaceCtxFilter BeastDiseaseOr2Dahan => new TargetSpaceCtxFilter( "land with beast, disease or at last 2 dahan", (TargetSpaceCtx spaceCtx ) => { var tokens = spaceCtx.Tokens; return tokens.Beasts.Any || tokens.Disease.Any || 2 <= tokens.Dahan.CountAll; } );
	static public TargetSpaceCtxFilter Disease => new TargetSpaceCtxFilter( "disease", spaceCtx => spaceCtx.Tokens.Disease.Any && spaceCtx.Tokens.HasInvaders() );

	static public TargetSpaceCtxFilter DahanOrAdjacentTo( int adjacentDahanThreshold ) => new TargetSpaceCtxFilter(
		$"a land with dahan or adjacent to at least {adjacentDahanThreshold} dahan",
		ctx => ctx.Dahan.Any || adjacentDahanThreshold <= ctx.AdjacentCtxs.Sum( x => x.Dahan.CountAll )
	);

}

public static class Is {
	static public TargetSpaceCtxFilter Inland => new TargetSpaceCtxFilter( "Inland", x => !x.IsCoastal && x.IsInPlay );
	static public TargetSpaceCtxFilter Coastal => new TargetSpaceCtxFilter( "coastal land", ctx => ctx.IsCoastal );
	static public TargetSpaceCtxFilter AdjacentToBlight => new TargetSpaceCtxFilter( "land adjacent to blight", spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx => adjCtx.Tokens.Blight.Any ) );
	static public TargetSpaceCtxFilter NotRavageCardMatch => new TargetSpaceCtxFilter( "land that does not match Ravage card", ( TargetSpaceCtx spaceCtx ) => !spaceCtx.GameState.InvaderDeck.Ravage.Cards.Any( card => card.MatchesCard( spaceCtx.Tokens ) ) );
}