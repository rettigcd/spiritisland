
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GameState_BranchAndClaw : GameState {

		static GameState_BranchAndClaw() {
			// Register new filters needed for Branch and Claw
			SpaceFilterMap.Register(Target.Beast,            ( ctx ) => ctx.Tokens.Beasts().Any );
			SpaceFilterMap.Register(Target.BeastOrJungle,    ( ctx ) => ctx.Terrain == Terrain.Jungle || ctx.Tokens.Beasts().Any );
			SpaceFilterMap.Register(Target.PresenceOrWilds,  ( ctx ) => ctx.IsPresent || ctx.Tokens.Wilds() > 0 );
			SpaceFilterMap.Register(Target.CoastalOrWetlands,( ctx ) => ctx.Terrain == Terrain.Wetland || ctx.IsCoastal );
			SpaceFilterMap.Register(Target.City, (ctx) => ctx.Tokens.Has(Invader.City) );

			// Don't use TerrainMapper, Inland should ignore terrain modifications (I think)
			SpaceFilterMap.Register(Target.Inland,           ( ctx ) => ctx.Space.Terrain != Terrain.Ocean && !ctx.Space.IsCoastal );
		}

		public GameState_BranchAndClaw(Spirit spirit,Board board ) : base( spirit, board ) {
		}

		public override void Initialize() {
			base.Initialize();
			foreach(var board in Island.Boards) {
				Tokens[board[2]].Disease().Count++;
				var lowest = board.Spaces.Skip(1).First(s=>s.StartUpCounts.Empty);
				Tokens[lowest][BacTokens.Beast]++;
			}
		}

		// only gets called when explorer is actually going to explore
		protected override async Task ExploresSpace( Space space ) {
			var wilds = Tokens[space].Wilds();
			if(wilds == 0)
				await base.ExploresSpace( space );
			else
				wilds.Count--;
		}

		protected override async Task<string> Build( TokenCountDictionary tokens, BuildingEventArgs.BuildType buildType ) {
			// ! Instead of overriding this, we could handle the pre-build event
			var disease = tokens.Disease();
			if(disease.Any) {
				disease.Count--;
				return tokens.Space.Label +" build stopped by disease";
			}
			return await base.Build( tokens, buildType );
		}

		public override async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = GetRavageConfiguration( grp.Space );
			var eng = new RavageEngineWithStrife( this, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

	}

}
