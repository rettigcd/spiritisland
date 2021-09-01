using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TallTalesOfSavagery : IFearCard {

		public const string Name = "Tall Tales of Savagery";

		[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
		public async Task Level1( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where(s => gs.Dahan.AreOn(s) && gs.Invaders.Counts[s].Has(Invader.Explorer) ).ToArray();
				if(options.Length==0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan to remove explorer", options ));
				gs.Invaders.Counts[target].Adjust( Invader.Explorer[1], -1 );
			}
		}

		[FearLevel( 2, "Each player removes 2 Explorer or 1 Town from a land with Dahan." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => gs.Dahan.AreOn( s ) && gs.Invaders.Counts[ s ].Has(Invader.Explorer) ).ToArray();
				if(options.Length == 0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear:select land with dahan to remove explorer", options ));
				RemoveTownOr2Explorers( gs, target );
			}
		}

		[FearLevel( 3, "Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan." )]
		public Task Level3( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces.Where(gs.Dahan.AreOn))
				RemoveTownOr2Explorers( gs, space );
			foreach(var space in gs.Island.AllSpaces.Where( s=>gs.Dahan.GetCount(s)>=2 && gs.Invaders.Counts[s].Has(Invader.City) ))
				gs.Invaders.Counts[space].Remove(Invader.City);
			return Task.CompletedTask;
		}

		static void RemoveTownOr2Explorers( GameState gs, Space target ) {
			var grp = gs.Invaders.Counts[ target ];
			if(grp.Has(Invader.Town))
				grp.Remove( Invader.Town );
			else
				grp.Adjust( Invader.Explorer[1], -2 );
		}

	}
}

