using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FearOfTheUnseen : IFearCard {

		public const string Name = "Fear of the Unseen";

		[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with SacredSite." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits)
				await Remove1ExplorerOrTownFromLandWithSacredSite(spirit,gs);
		}

		static async Task Remove1ExplorerOrTownFromLandWithSacredSite(Spirit spirit,GameState gs ) {
			var options = spirit.SacredSites.Where( s => gs.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Action.Choose( new TargetSpaceDecision( "Select SS land to remove 1 explorer/town.", options ));
			var grp = gs.Tokens[target];
			var invaderToRemove = grp.PickBestInvaderToRemove( Invader.Town, Invader.Explorer );
			grp.Adjust( invaderToRemove, -1 );
		}

		[FearLevel( 2, "Each player removes 1 Explorer / Town from a land with Presence." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = spirit.Presence.Spaces.Where( s => gs.Tokens[s].HasAny( Invader.Explorer, Invader.Town ) )
					.Union(spirit.SacredSites.Where(s=>gs.Tokens[s].Has(Invader.City)))
					.ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Select land to remove 1 explorer/town/city.", options ));
				var grp = gs.Tokens[ target ];
				var invaderToRemove = grp.PickBestInvaderToRemove(Invader.Town,Invader.Explorer);
				grp.Adjust( invaderToRemove, -1 );
			}
		}

		[FearLevel( 3, "Each player removes 1 Explorer / Town from a land with Presence, or 1 City from a land with SacredSite." )]
		public async Task Level3( GameState gs ) {
			Space[] sacredSites = gs.Spirits.SelectMany( spirit => spirit.Presence.Spaces ).Distinct().ToArray(); // !!! is this SS or Presence?
			foreach(var spirit in gs.Spirits) {
				var options = sacredSites.Where( s => gs.Tokens[ s ].HasAny( Invader.Explorer, Invader.Town ) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Select SS land to remove 1 explorer/town.", options ));
				var grp = gs.Tokens[target];
				var invaderToRemove = grp.PickBestInvaderToRemove( Invader.Town, Invader.Explorer );
				grp.Adjust( invaderToRemove, -1 );
			}
		}
	}
}

