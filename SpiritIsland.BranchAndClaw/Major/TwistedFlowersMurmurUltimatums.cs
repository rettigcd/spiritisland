namespace SpiritIsland.BranchAndClaw;

public class TwistedFlowersMurmurUltimatums {

	[MajorCard("Twisted Flowers Murmur Ultimatums", 5, Element.Sun, Element.Moon, Element.Air, Element.Earth, Element.Plant),Slow,FromSacredSite(1,Filter.Invaders)]
	[Instructions( "4 Fear. Add 1 Strife. If the Terror Level is 2 or higher, remove 2 Invaders. -If you have- 3 Moon, 2 Air, 3 Plant: +3 Fear, before the Terror Level check. 3 Damage." ), Artist( Artists.KatBirmelin )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 4 fear
		await ctx.AddFear(4);
			
		// add 1 strife
		await ctx.AddStrife();

		// if you have 3moon, 2 air, 3 plant (before the terror level check)
		if(await ctx.YouHave( "3 moon,2 air,3 plant" )) {
			await ctx.AddFear( 3 );
			await ctx.DamageInvaders( 3 );
		}

		// if terror level is 2 or higher, remove 2 invaders
		if(2 <= GameState.Current.Fear.TerrorLevel)
			for(int i = 0; i < 2; ++i) {
				var st = await ctx.Self.SelectAsync( An.Invader.ToRemove( ctx.Space.InvaderTokens().On( ctx.Space) ) );
				if(st is null) break;
				await ctx.Invaders.Remove( st.Token, 1 );
			}

	}

}