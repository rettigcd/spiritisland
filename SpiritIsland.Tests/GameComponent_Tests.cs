using System.Reflection;

namespace SpiritIsland.Tests {

	public  class GameComponent_Tests {

		[Fact]
		public void BaseGame() {
			ValidateGameComponentProvider( new SpiritIsland.Basegame.GameComponentProvider() );
		}

		[Fact]
		public void BranchAndClaw() {
			ValidateGameComponentProvider( new SpiritIsland.BranchAndClaw.GameComponentProvider() );
		}

		[Fact]
		public void PromoPack1() {
			ValidateGameComponentProvider( new SpiritIsland.PromoPack1.GameComponentProvider() );
		}

		[Fact]
		public void JaggedEarth() {
			ValidateGameComponentProvider( new SpiritIsland.JaggedEarth.GameComponentProvider() );
		}


		static void ValidateGameComponentProvider( IGameComponentProvider provider ) {
			Type assemblyType = provider.GetType();
			FindSpiritTypes( assemblyType ).ShouldAllBe( x => provider.Spirits.Contains( x ) );
			FindMinorCards( assemblyType ).Select( x => x.Name ).ShouldAllBe( x => provider.MinorCards.Select( x => x.Name ).ToArray().Contains( x ) );
			FindMajorCards( assemblyType ).Select( x => x.Name ).ShouldAllBe( x => provider.MajorCards.Select( x => x.Name ).ToArray().Contains( x ) );
			FindFearCards( assemblyType ).Select( x => x.Name ).ShouldAllBe( x => provider.FearCards.Select( x => x.Name ).ToArray().Contains( x ) );
			FindBlightCards( assemblyType ).Select( x => x.Name ).ShouldAllBe( x => provider.BlightCards.Select( x => x.Name ).ToArray().Contains( x ) );
		}

		static Type[] FindSpiritTypes( Type assemblyType ) {
			return assemblyType.Assembly.GetTypes()
				.Where( t => t.IsAssignableTo( typeof(Spirit) ))
				.ToArray();
		}

		static PowerCard[] FindMajorCards( Type assemblyType ) {
			static bool HasMajorAttribute( MethodBase m ) => m.GetCustomAttributes<MajorCardAttribute>().Any();
			static bool HasMajorMethod( Type type ) => type.GetMethods().Any( HasMajorAttribute );
			return assemblyType.Assembly.GetTypes()
				.Where( HasMajorMethod )
				.Select( PowerCard.For )
				.ToArray();
		}

		static PowerCard[] FindMinorCards( Type assemblyType ) {
			static bool HasMinorAttribute( MethodBase m ) => m.GetCustomAttributes<MinorCardAttribute>().Any();
			static bool HasMinorMethod( Type type ) => type.GetMethods().Any( HasMinorAttribute );
			return assemblyType.Assembly.GetTypes()
				.Where( HasMinorMethod )
				.Select( PowerCard.For )
				.ToArray();
		}

		static IFearOptions[] FindFearCards( Type assemblyType ) {
			return assemblyType.Assembly.GetTypes()
				.Where( t => t.IsAssignableTo( typeof(IFearOptions) ) )
				.Select( t => (IFearOptions)System.Activator.CreateInstance(t) )
				.ToArray();
		}

		static IBlightCard[] FindBlightCards( Type assemblyType ) {
			return assemblyType.Assembly.GetTypes()
				.Where( t => t.IsAssignableTo( typeof(IBlightCard) ))
				.Select( t => (IBlightCard)System.Activator.CreateInstance(t) )
				.ToArray();

		}

	}

}
