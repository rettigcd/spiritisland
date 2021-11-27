using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FearOfTheUnseen : IFearOptions {

		public const string Name = "Fear of the Unseen";

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
		public async Task Level1( FearCtx ctx ) {
			foreach(SpiritGameStateCtx spiritCtx in ctx.Spirits)
				await Remove1ExplorerOrTownFromLandWithSacredSite(spiritCtx);
		}

		static async Task Remove1ExplorerOrTownFromLandWithSacredSite( SpiritGameStateCtx ctx ) {
			var options = ctx.Self.Presence.SacredSites.Where( s => ctx.GameState.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
			await ctx.RemoveTokenFromOneSpace(options,1,Invader.Town, Invader.Explorer);
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in ctx.Spirits) {
				var options = spirit.Self.Presence.Spaces.Where( s => gs.Tokens[s].HasAny( Invader.Explorer, Invader.Town ) )
					.Union(spirit.Self.Presence.SacredSites.Where(s=>gs.Tokens[s].Has(Invader.City)))
					.ToArray();
				await spirit.RemoveTokenFromOneSpace( options, 1, Invader.Town, Invader.Explorer );
			}
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public async Task Level3( FearCtx ctx ) {
			Space[] sacredSites = ctx.Spirits.SelectMany( spirit => spirit.Self.Presence.SacredSites ).Distinct().ToArray();
			Space[] presences = ctx.Spirits.SelectMany( spirit => spirit.Self.Presence.Spaces ).Distinct().ToArray();

			foreach(var spiritCtx in ctx.Spirits) {

				var cityOptions         = sacredSites.Where( s=> spiritCtx.Target(s).Tokens.Has(Invader.City)).ToArray();
				var townExplorerOptions = presences.Where( s => spiritCtx.Target(s).Tokens.HasAny( Invader.Town, Invader.Explorer) ).ToArray();

				await spiritCtx.SelectActionOption(
					"Select invader to remove",
					new ActionOption( 
						"remove 1 explorer/town from land with presence", 
						() => spiritCtx.RemoveTokenFromOneSpace( townExplorerOptions, 1, Invader.Town,Invader.Explorer ),
						townExplorerOptions.Length>0
					),
					new ActionOption( 
						"remove 1 city from land with sacred site",  
						() => spiritCtx.RemoveTokenFromOneSpace( cityOptions, 1, Invader.City ),
						cityOptions.Length > 0
					)
				);

			}

		}
	}
}

