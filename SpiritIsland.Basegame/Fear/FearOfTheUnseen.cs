using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FearOfTheUnseen : IFearOptions {

		public const string Name = "Fear of the Unseen";

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
		public async Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits)
				await Remove1ExplorerOrTownFromLandWithSacredSite(spirit,gs);
		}

		static async Task Remove1ExplorerOrTownFromLandWithSacredSite(Spirit spirit,GameState gs ) {
			var options = spirit.SacredSites.Where( s => gs.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select SS land to remove 1 explorer/town.", options, Present.Always ));
			var grp = gs.Tokens[target];
			var invaderToRemove = grp.PickBestInvaderToRemove( Invader.Town, Invader.Explorer );
			grp.Adjust( invaderToRemove, -1 );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = spirit.Presence.Spaces.Where( s => gs.Tokens[s].HasAny( Invader.Explorer, Invader.Town ) )
					.Union(spirit.SacredSites.Where(s=>gs.Tokens[s].Has(Invader.City)))
					.ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select land to remove 1 explorer/town/city.", options, Present.Always ));
				var grp = gs.Tokens[ target ];
				var invaderToRemove = grp.PickBestInvaderToRemove(Invader.Town,Invader.Explorer);
				grp.Adjust( invaderToRemove, -1 );
			}
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public async Task Level3( FearCtx ctx ) {
			Space[] sacredSites = ctx.Spirits.SelectMany( spirit => spirit.Self.SacredSites ).Distinct().ToArray();
			Space[] presences = ctx.Spirits.SelectMany( spirit => spirit.Self.Presence.Spaces ).Distinct().ToArray();

			foreach(var spiritCtx in ctx.Spirits) {

				var cityOptions = sacredSites.Where(s=>spiritCtx.Target(s).Tokens.Has(Invader.City)).ToArray();
				var townOptions = presences.Where( s => spiritCtx.Target(s).Tokens.Has( Invader.Town) ).ToArray();
				var explorerOptions = presences.Where( s => spiritCtx.Target(s).Tokens.Has( Invader.Explorer ) ).ToArray();

				async Task Remove(Space[] options, TokenGroup removeType ) {
					var space = await spiritCtx.Self.Action.Decision(new Decision.TargetSpace("Select land to remove "+removeType.Label, options, Present.Always ));
					spiritCtx.Target(space).Tokens.Remove(removeType);
				}

				await spiritCtx.SelectActionOption(
					"Select invader to remove",
					new ActionOption( "remove 1 explorer from land with presence", ()=>Remove(explorerOptions,Invader.Explorer), explorerOptions.Length>0 ),
					new ActionOption( "remove 1 town from land with presence",     () => Remove( townOptions, Invader.Town ), townOptions.Length > 0 ),
					new ActionOption( "remove 1 city from land with sacred site",  () => Remove( cityOptions, Invader.City ), cityOptions.Length > 0 )
				);

				var options = sacredSites.Where( s => spiritCtx.Target(s).Tokens.HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await spiritCtx.Self.Action.Decision( new Decision.TargetSpace( "Select SS land to remove 1 explorer/town.", options, Present.Always ));
				var tokens = spiritCtx.Target(target).Tokens;
				var invaderToRemove = tokens.PickBestInvaderToRemove( Invader.Town, Invader.Explorer );
				tokens.Adjust( invaderToRemove, -1 );

			}

		}
	}
}

