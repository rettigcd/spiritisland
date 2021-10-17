using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TallTalesOfSavagery : IFearOptions {

		public const string Name = "Tall Tales of Savagery";

		[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
		public async Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => gs.DahanIsOn( s ) && gs.Tokens[s].Has( Invader.Explorer ) ).ToArray();
				if(options.Length == 0) return;
				await RemoveExplorerFromOneOfThese( gs, spirit, options );
			}
		}

		static async Task RemoveExplorerFromOneOfThese( GameState gs, Spirit spirit, Space[] options ) {
			var target = await spirit.Action.Decision( new Decision.TargetSpace( "Select land to remove explorer", options, Present.Always ) );
			gs.Tokens[target].Adjust( Invader.Explorer[1], -1 );
		}

		[FearLevel( 2, "Each player removes 2 Explorer or 1 Town from a land with Dahan." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces.Where( s => gs.DahanIsOn( s ) && gs.Tokens[ s ].Has(Invader.Explorer) ).ToArray();
				if(options.Length == 0) return;
				var space = await spirit.Action.Decision( new Decision.TargetSpace( "Fear:select land with dahan to remove explorer", options, Present.Always ));
				RemoveTownOr2Explorers( gs.Invaders.On( space, Cause.Fear ) );
			}
		}

		[FearLevel( 3, "Remove 2 Explorer or 1 Town from each land with Dahan. Then, remove 1 City from each land with at least 2 Dahan." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var space in gs.Island.AllSpaces.Where(gs.DahanIsOn))
				RemoveTownOr2Explorers( gs.Invaders.On( space, Cause.Fear ) );
			foreach(var space in gs.Island.AllSpaces.Where( s=>gs.DahanGetCount(s)>=2 && gs.Tokens[s].Has(Invader.City) ))
				gs.Invaders.On(space,Cause.Fear).Remove(Invader.City);
			return Task.CompletedTask;
		}

		static void RemoveTownOr2Explorers( InvaderGroup grp ) {
			if(grp.Tokens.Has(Invader.Town))
				grp.Remove( Invader.Town );
			else
				grp.Tokens.Adjust( Invader.Explorer[1], -2 );
		}

	}
}

