namespace SpiritIsland.NatureIncarnate;

// fast, range 0, ANY
// 2 moon, 2 air Push up to half (rounded down) of Invaders from target land.  Do likewise for Dahan, Presence, and Beast (each separately).
// 2 sun, 2 air Push up to 1 Invader/Dahan/Presence/Beast.
// 2 moon, 4 air, 3 water Repeat this Power


[InnatePower( "Swirl and Spill" ), Slow]
[FromPresence( 1 )]
public class SwirlAndSpill {

	[InnateTier( "2 water", "Push up to 2 Explorer/Dahan/Blight.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		await ctx.PushUpTo(2,Human.Explorer,Human.Dahan,Token.Blight);
	}

	[InnateTier( "3 water,1 animal", "1 Fear. Push up to 2 Town/Presence/Beast.", 0 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await ctx.AddFear( 1 );
		await BuildTier2Source( ctx )
			.PushUpToN( ctx.Self );
	}

	[InnateTier( "5 water,2 plant,2 animal", "In one land pushed into, Downgrade all Town and all City." )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		await ctx.AddFear( 1 ); // from Tier-2

		await BuildTier2Source( ctx )
			.ConfigDestination( InOneLandPushedInto_DowngradeAllTownsAndCities(ctx.Self) )
			.PushUpToN(ctx.Self);
	}

	static Action<DestinationSelector> InOneLandPushedInto_DowngradeAllTownsAndCities( Spirit self ) {
		return d=>{
			bool used = false;
			d.Track( async to => {
				Space toSpace = (Space)to;
				if(!used 
					&& to.HasAny( Human.Town_City ) 
					&& await self.UserSelectsFirstText("Downgrade All Towns/Cities on "+to.Label, "Yes, Downgrade those suckers!", "No, let's ruin someone else's day." )
				) {
					used = true;
					await ReplaceInvader.DowngradeAll( self, to, Human.Town_City );
				}
			}); 
		};
	}


	static SourceSelector BuildTier2Source( TargetSpaceCtx ctx ) {
		return ctx.SourceSelector
			.AddGroup( 2, Human.Explorer, Human.Dahan, Token.Blight ) // from Tier 1
			.AddGroup( 2, Human.Town, ctx.Self.Presence, Token.Beast ); // Tier 2;
	}


}
