using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class StubbornSolidity {

		[SpiritCard("Stubborn Solidity",1,Element.Sun, Element.Earth, Element.Animal), Fast, FromPresence(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// Defend 1 per dahan  (protects the land)
			ctx.Defend( ctx.Dahan.Count );

			// dahan in target land cannot be changed. (when they would be damaged, destoryed, removed, replaced, or moved, instead don't)
			ctx.Tokens.Dahan.Frozen = true;

			ctx.GameState.TimePasses_ThisRound.Push( (gs)=>{
				gs.Tokens[ctx.Space].Dahan.Frozen=false;
				return Task.CompletedTask;
			} );

			return Task.CompletedTask;
		}

	}

}
