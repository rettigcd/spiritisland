using SpiritIsland.Core;
using System.Threading.Tasks;


namespace SpiritIsland.Basegame {

	[InnatePower(LeadTheFuriousAssult.Name,Speed.Slow)]
	[FromPresence(0)]
	public class LeadTheFuriousAssult {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "2 sun, 1 fire" )]
		static public Task Option1Async(ActionEngine engine, Space target ) {
			// Destroy 1 town for every 2 dahan
			engine.GameState.InvadersOn(target)
				.DestroyType(Invader.Town,engine.GameState.GetDahanOnSpace(target)/2);
			return Task.CompletedTask;
		}

		[InnateOption( "4 sun, 3 fire" )]
		static public Task Option2Async( ActionEngine engine, Space target ) {
			// Destroy 1 city for every 3 dahan
			_ = engine.GameState.InvadersOn( target )
				.DestroyType( Invader.City, engine.GameState.GetDahanOnSpace( target ) / 3 );

			return Option1Async(engine,target);
		}
	}

	[InnatePower( LeadTheFuriousAssult.Name, Speed.Fast )]
	[FromPresence( 0 )]
	public class LeadTheFuriousAssult_Fast {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "4 air,2 sun, 1 fire" )]
		static public Task Option1Async( ActionEngine engine, Space target ) {
			GatherTheWarriors_Fast.RemoveSlow( engine.Self, GatherTheWarriors.Name );
			return LeadTheFuriousAssult.Option1Async(engine, target);
		}

		[InnateOption( "4 air, 4 sun, 3 fire" )]
		static public Task Option2Async( ActionEngine engine, Space target ) {
			GatherTheWarriors_Fast.RemoveSlow( engine.Self, GatherTheWarriors.Name );
			return LeadTheFuriousAssult.Option2Async( engine, target );
		}

	}


}
