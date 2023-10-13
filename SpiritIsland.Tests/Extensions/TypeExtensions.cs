using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests;

static public class TypeExtensions {

	static public PowerCard[] GetMajors( this Type assemblyRefType ) {
		static bool HasMajorAttribute( MethodBase m ) => m.GetCustomAttributes<MajorCardAttribute>().Any();
		static bool HasMajorMethod( Type type ) => type.GetMethods().Any( HasMajorAttribute );
		return assemblyRefType.Assembly.GetTypes().Where( HasMajorMethod ).Select( PowerCard.For ).ToArray();
	}
	static public PowerCard[] GetMinors( this Type assemblyRefType ) {
		static bool HasMinorAttribute( MethodBase m ) => m.GetCustomAttributes<MinorCardAttribute>().Any();
		static bool HasMinorMethod( Type type ) => type.GetMethods().Any( HasMinorAttribute );
		return assemblyRefType.Assembly.GetTypes().Where( HasMinorMethod ).Select( PowerCard.For ).ToArray();
	}

	static public Spirit[] GetSpirits( this Type assemblyRefType ) {
		static bool IsSpirit( Type type ) => type.IsAssignableTo( typeof( Spirit ) );
		return assemblyRefType.Assembly.GetTypes()
			.Where( IsSpirit )
			.Select( t => (Spirit)Activator.CreateInstance(t) )
			.ToArray();
	}

}

class AssemblyType {
	public static Type GetEditionType( string edition ) {
		return edition switch {
			BaseGame => typeof( RiverSurges ),
			BranchAndClaw => typeof( SharpFangs ),
			JaggedEarth => typeof( ShiftingMemoryOfAges ),
			FeatherAndFlame => typeof( DownpourDrenchesTheWorld ),
			NatureIncarnate => typeof( ToweringRootsOfTheJungle ),
			_ => throw new ArgumentException( "Edition not found", nameof( edition ) ),
		};
	}

	public const string BaseGame = "Basegame";
	public const string BranchAndClaw = "Branch and Claw";
	public const string JaggedEarth = "Jagged Earth";
	public const string FeatherAndFlame = "Feather and Flame";
	public const string NatureIncarnate = "Nature Incarnate";

}

