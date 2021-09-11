//using System.Threading.Tasks;

//namespace SpiritIsland.BranchAndClaw {
//	public class PentUpCalamity {

//		[MajorCard( "Pent-Up Calamity", 3, Speed.Fast, Element.Moon, Element.Fire, Element.Earth,Element.Plant,Element.Animal )]
//		[FromPresence( 2 )]
//		static public async Task ActAsync( TargetSpaceCtx ctx ) {

//			// add 1 disease and 1 strife
//			ctx.Tokens.Disease().Count++;
//			await ctx.AddStrife();

//			// OR



//			// remove any number of beast / disease/strife/wilds.  For each token removed, 1 fear and 3 damage

//			// if you have 2 moon, 3 fire: if you have remvoed tokens, return up to 2 of them.  Otherwise, add 2 strife
//			return Task.CompletedTask;
//		}


//		Task SelectTokens( TargetSpaceCtx ctx, System.Action<Token> callBack, params TokenGroup[] group ) {
//			var token = await ctx.Self.Action.Decide(new D)
//		}

//	}
//}
