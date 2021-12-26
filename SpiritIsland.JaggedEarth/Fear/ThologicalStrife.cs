using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ThologicalStrife : IFearOptions {

		public const string Name = "Theological Strife";
		string IFearOptions.Name => Name;

		[FearLevel(1, "Each player adds 1 Strife in a land with Presence." )]
		public Task Level1( FearCtx ctx ) {
			return GameCmd.EachPlayerTakesActionInALand( Cmd.AddStrife(1), spaceCtx => spaceCtx.Presence.IsHere, Cause.Fear ).Execute( ctx.GameState );
		}

		[FearLevel(2, "Each player adds 1 Strife in a land with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders." )]
		public async Task Level2( FearCtx ctx ) { 

			// Each player adds 1 Strife in a land with Presence
			await GameCmd.EachPlayerTakesActionInALand( Cmd.AddStrife(1), spaceCtx => spaceCtx.Presence.IsHere, Cause.Fear ).Execute( ctx.GameState );

			// Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders.
			await GameCmd.EachSpirit( Cause.Fear, new SelfAction(
				"Gain 1 Energy per SacredSite Spirit has in lands with Invaders"
				, spiritCtx => spiritCtx.Self.Energy += spiritCtx.Self.Presence.SacredSites.Count( ss => spiritCtx.Target(ss).HasInvaders )
			)).Execute( ctx.GameState );

		}

		[FearLevel(3, "Each player adds 1 Strife in a land with Presence. Then, each Invader with Strife deals Damage to other Invaders in its land." )]
		public async Task Level3( FearCtx ctx ) {

			// Each player adds 1 Strife in a land with Presence
			await GameCmd.EachPlayerTakesActionInALand( Cmd.AddStrife(1), spaceCtx => spaceCtx.Presence.IsHere, Cause.Fear ).Execute( ctx.GameState );

			// Each Invader with Strife deals Damage to other Invaders in its land.
			await GameCmd.InEachLand( Cause.Fear, StrifedRavage.Cmd, tokens=>tokens.HasStrife ).Execute( ctx.GameState );
		}
	}

}
