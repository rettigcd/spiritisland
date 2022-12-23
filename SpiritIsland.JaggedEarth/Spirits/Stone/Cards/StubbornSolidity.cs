namespace SpiritIsland.JaggedEarth;

public class StubbornSolidity {

	public const string Name = "Stubborn Solidity";

	[SpiritCard(StubbornSolidity.Name,1,Element.Sun, Element.Earth, Element.Animal), Fast, FromPresence(1)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Defend 1 per dahan  (protects the land)
		ctx.Defend( ctx.Dahan.CountAll );

		// Dahan in target land cannot be changed. ( when they would be damaged, destroyed, removed, replaced, or moved, instead don't)

		// Freeze existing
		foreach(var x in ctx.Tokens.Dahan.NormalKeys)
			ctx.Tokens.ReplaceAllWith(x,x.SwitchClass( FrozenDahan ));

		// Freeze future dahn moved into this land. 
		ctx.GameState.Tokens.TokenMoved.ForRound.Add( args=> {
			if(args.Class == TokenType.Dahan && args.AddedTo == ctx.Tokens && args.Token is HealthToken ht)
				args.AddedTo.ReplaceWith( ht, ht.SwitchClass( FrozenDahan ), args.Count );
		} );
		ctx.GameState.Tokens.TokenAdded.ForRound.Add( args => {
			if(args.Token.Class == TokenType.Dahan && args.Space == ctx.Tokens && args.Token is HealthToken ht)
				args.Space.ReplaceWith( ht, ht.SwitchClass( FrozenDahan ), args.Count );
		} );

		// Restore at end of round
		ctx.GameState.TimePasses_ThisRound.Push( (gs)=>{
			foreach( HealthToken x in ctx.Tokens.OfClass(FrozenDahan).ToArray() )
				ctx.Tokens.ReplaceAllWith( x, x.SwitchClass( TokenType.Dahan ) );
			return Task.CompletedTask;
		} );

		return Task.CompletedTask;
	}

	static public readonly HealthTokenClass FrozenDahan = new HealthTokenClass( "Dahan", 2, TokenCategory.Dahan, 0, Img.Dahan, 2 );

}