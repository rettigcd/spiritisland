
using System.Linq;

namespace SpiritIsland {


	public class GameState_BranchAndClaw : GameState {

		static GameState_BranchAndClaw() {
			// Register new filters needed for Branch and Claw
			SpaceFilterMap.Register(Target.Beast,            ( ctx ) => ctx.Beasts.Any );
			SpaceFilterMap.Register(Target.BeastOrJungle,    ( ctx ) => ctx.Terrain == Terrain.Jungle || ctx.Beasts.Any );
			SpaceFilterMap.Register(Target.PresenceOrWilds,  ( ctx ) => ctx.IsPresent || ctx.Wilds > 0 );
			SpaceFilterMap.Register(Target.CoastalOrWetlands,( ctx ) => ctx.Terrain == Terrain.Wetland || ctx.IsCoastal );
			SpaceFilterMap.Register(Target.City, (ctx) => ctx.Tokens.Has(Invader.City) );

			// Jagged Earth
			SpaceFilterMap.Register(Target.MountainOrPresence, ( ctx ) => ctx.Terrain == Terrain.Mountain || ctx.HasSelfPresence );

			// Don't use TerrainMapper, Inland should ignore terrain modifications (I think)
			SpaceFilterMap.Register(Target.Inland,           ( ctx ) => ctx.Space.Terrain != Terrain.Ocean && !ctx.Space.IsCoastal );
		}

		public GameState_BranchAndClaw(Spirit spirit,Board board ) : base( spirit, board ) {}

		public override void Initialize() {
			base.Initialize();
			foreach(var board in Island.Boards) {
				Tokens[board[2]].Disease.Count++;
				var lowest = board.Spaces.Skip(1).First(s=>s.StartUpCounts.Empty);
				Tokens[lowest][TokenType.Beast]++;
			}
		}

	}

}
