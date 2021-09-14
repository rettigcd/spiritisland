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
			var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select SS land to remove 1 explorer/town.", options ));
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
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select land to remove 1 explorer/town/city.", options ));
				var grp = gs.Tokens[ target ];
				var invaderToRemove = grp.PickBestInvaderToRemove(Invader.Town,Invader.Explorer);
				grp.Adjust( invaderToRemove, -1 );
			}
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public async Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			Space[] sacredSites = gs.Spirits.SelectMany( spirit => spirit.SacredSites ).Distinct().ToArray();
			Space[] presences = gs.Spirits.SelectMany( spirit => spirit.Presence.Spaces ).Distinct().ToArray();

			foreach(var spirit in gs.Spirits) {

				var cityOptions = sacredSites.Where(s=>gs.Tokens[s].Has(Invader.City)).ToArray();
				var townOptions = presences.Where( s => gs.Tokens[s].Has( Invader.Town) ).ToArray();
				var explorerOptions = presences.Where( s => gs.Tokens[s].Has( Invader.Explorer ) ).ToArray();

				async Task Remove(Space[] options, TokenGroup removeType ) {
					var space = await spirit.Action.Decision(new Decision.TargetSpace("Select land to remove "+removeType.Label, options, Present.Always ));
					gs.Tokens[space].Remove(removeType);
				}

				await spirit.SelectAction(
					"Select invader to remove",
					new ActionOption( "remove 1 explorer from land with presence", ()=>Remove(explorerOptions,Invader.Explorer), explorerOptions.Length>0 ),
					new ActionOption( "remove 1 town from land with presence",     () => Remove( townOptions, Invader.Town ), townOptions.Length > 0 ),
					new ActionOption( "remove 1 city from land with sacred site",  () => Remove( cityOptions, Invader.City ), cityOptions.Length > 0 )
				);

				var options = sacredSites.Where( s => gs.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select SS land to remove 1 explorer/town.", options ));
				var grp = gs.Tokens[target];
				var invaderToRemove = grp.PickBestInvaderToRemove( Invader.Town, Invader.Explorer );
				grp.Adjust( invaderToRemove, -1 );

			}

		}
	}
}

