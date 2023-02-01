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
			if(args.TokenAdded.Class == Human.Dahan && args.AddedTo == ctx.Tokens && args.TokenAdded is HumanToken ht)
				args.AddedTo.ReplaceNWith( args.Count, ht, ht.SwitchClass( FrozenDahan ) );
		} );
		ctx.Tokens.Adjust( new TokenAddedHandler(Name, args => {
			if(args.Token.Class == Human.Dahan && args.Token is HumanToken ht)
				args.AddedTo.ReplaceNWith( args.Count, ht, ht.SwitchClass( FrozenDahan ) );
		} ), 1 );

		// Restore at end of round - !!! Could instead create a custom token that cleans up its own mess.
		ctx.GameState.TimePasses_ThisRound.Push( (gs)=>{
			foreach( HumanToken x in ctx.Tokens.OfClass(FrozenDahan).ToArray().Cast<HumanToken>())
				ctx.Tokens.ReplaceAllWith( x, x.SwitchClass( Human.Dahan ) );
			return Task.CompletedTask;
		} );

		return Task.CompletedTask;
	}

	static public readonly HumanTokenClass FrozenDahan = new HumanTokenClass( "Dahan", TokenCategory.Dahan, 0, Img.Dahan, 2, TokenVariant.Frozen );

}