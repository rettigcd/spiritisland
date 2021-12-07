using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class InfiniteVitality {

		[MajorCard( "Infinite Vitality", 3, Element.Earth, Element.Plant, Element.Animal )]
		[Fast]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			if( await ctx.YouHave( "4 earth" )) {
				ctx.ModifyRavage( cfg => {
					// whenever blight would be added to target land, instead leave it on the card
					cfg.ShouldDamageLand = false;
					// dahan ignore damage and destruction effects, 
					cfg.ShouldDamageDahan = false;
				} );
				await RemoveBlightFromLandOrAdjacent( ctx );
			} else {
				ctx.ModifyRavage( cfg => {
					// whenever blight would be added to target land, instead leave it on the card
					cfg.ShouldDamageLand = false;

					// dahan have +4 health while in target land.
					//					cfg.DahanHitpoints += 4;

				} );

				// dahan have +4 health while in target land. (If played twice, would increase dahan health to +8)
				BoostDahanHealth( ctx, 4 );

				// if dahan is moved out of land, reduce to Healthy
				ctx.GameState.Tokens.TokenMoved.ForRound.Add( ( gs, args ) => {
					if(args.Token.Generic == TokenType.Dahan && args.From == ctx.Space)
						ReduceDahanToMax( ctx, gs, args );
				} );
			}

		}

		private static void BoostDahanHealth( TargetSpaceCtx ctx, int boost ) {
			foreach(var token in ctx.Dahan.Keys.ToArray()) {
				int newHealth = token.Health + boost;
				TokenType.Dahan.ExtendHealthRange( newHealth );
				ctx.Tokens[TokenType.Dahan[newHealth]] += ctx.Tokens[token];
				ctx.Tokens[token] = 0;
			}
		}

		private static void ReduceDahanToMax( TargetSpaceCtx ctx, GameState gs, TokenMovedArgs args ) {
			var tokens = gs.Tokens[args.To];
			foreach(var token in tokens.OfType( TokenType.Dahan ).ToArray()) {
				if(token.Health > 2) {
					ctx.Tokens[TokenType.Dahan.Default] += tokens[token];
					tokens[token] = 0;
				}
			}
		}

		static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
			// remove 1 blight from target or adjacent land
			var blightedLands = ctx.Space.Range( 1 ).Where( s=>ctx.Target(s).HasBlight ).ToArray();
			var unblightLand = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Remove 1 blight from", blightedLands, Present.Always ));
			if(unblightLand != null)
				await ctx.Target( unblightLand ).RemoveBlight();
		}
	}

}
