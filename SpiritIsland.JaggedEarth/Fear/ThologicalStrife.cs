using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ThologicalStrife : IFearOptions {

		public const string Name = "Theological Strife";

		[FearLevel(1, "Each player adds 1 Strife in a land with Presence." )]
		public Task Level1( FearCtx ctx ) {
			return ctx.EachPlayerTakesActionInALand(
				Cmd.AddStrife(1)
				,spaceCtx => spaceCtx.Presence.IsHere
			);
		}

		[FearLevel(2, "Each player adds 1 Strife in a land with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders." )]
		public async Task Level2( FearCtx ctx ) { 

			// Each player adds 1 Strife in a land with Presence
			await ctx.EachPlayerTakesActionInALand( Cmd.AddStrife(1), spaceCtx => spaceCtx.Presence.IsHere );

			// Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders.
			await ctx.EachSpiritTakesAction( new SelfAction(
				"Gains 1 Energy per SacredSite spirit has in lands with Invaders"
				, spiritCtx => spiritCtx.Self.Energy += spiritCtx.Self.Presence.SacredSites.Count( ss => spiritCtx.Target(ss).HasInvaders )
			));

		}

		[FearLevel(3, "Each player adds 1 Strife in a land with Presence. Then, each Invader with Strife deals Damage to other Invaders in its land." )]
		public async Task Level3( FearCtx ctx ) {

			// Each player adds 1 Strife in a land with Presence
			await ctx.EachPlayerTakesActionInALand( Cmd.AddStrife(1), spaceCtx => spaceCtx.Presence.IsHere );

			// Each Invader with Strife deals Damage to other Invaders in its land.
			await ctx.InEachLand( StrifedRavage.Cmd, space=>ctx.GameState.Tokens[space].HasStrife() );
		}
	}

}
