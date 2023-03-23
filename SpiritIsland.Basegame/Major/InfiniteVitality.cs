namespace SpiritIsland.Basegame;

public class InfiniteVitality {

	const string Name = "Infinite Vitality";

	[MajorCard( Name, 3, Element.Earth, Element.Plant, Element.Animal ),Fast,FromSacredSite( 1 )]
	[Instructions( "Dahan have +4 Health while in target land. Whenever Blight would be added to target land, instead leave it on the card. -If you have- 4 Earth: Dahan ignore Damage and Destruction effects. Remove 1 Blight from target or adjacent land." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Dahan have +4 health while in target land.
		await ctx.AdjustTokensHealthForRound( 4, Human.Dahan );

		// whenever blight would be added to target land, instead leave it on the card
		ctx.Blight.Block();

		if( await ctx.YouHave( "4 earth" )) {
			// Dahan ignore damage and destruction effects.
			ctx.ModifyRavage( behavior => behavior.DamageDefenders = ( _, _1, _2 ) => Task.CompletedTask ); // this stops dahan destruction during RAVAGE.

			ctx.Tokens.Init( new StopDamageAndDestruction(Name), 1 ); // stop damage other times

			// Remove 1 blight from target or adjacent
			await RemoveBlightFromLandOrAdjacent( ctx );
		}

	}

	static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
		// remove 1 blight from target or adjacent land
		var blightedLands = ctx.Tokens.InOrAdjacentTo.Where( s=>s.Blight.Any ).ToArray();
		var unblightLand = await ctx.Decision( new Select.ASpace( "Remove 1 blight from", blightedLands, Present.Always ));
		if(unblightLand != null)
			await ctx.Target( unblightLand ).RemoveBlight();
	}

}

class StopDamageAndDestruction 
	: IStopDahanDamage
	, IModifyRemovingTokenAsync 
	, IEndWhenTimePasses
{

	readonly string _sourceName;
	public StopDamageAndDestruction(string sourceName ) {
		_sourceName = sourceName;
	}

	public IEntityClass Class => ActionModTokenClass.Mod;

	public Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		if(args.Token.Class == Human.Dahan && args.Reason == RemoveReason.Destroyed) {
			if(args.Mode == RemoveMode.Live)
				GameState.Current.Log( new Log.Debug( $"{_sourceName} stopping {args.Count} Dahan from being destroyed." ) );
			args.Count = 0;
		}
		return Task.CompletedTask;
	}

}
