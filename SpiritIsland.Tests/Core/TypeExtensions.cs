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

	static public TargetSpaceCtx TargetSpace( this SelfCtx ctx, string spaceLabel ) 
		=> ctx.Target( ctx.GameState.AllSpaces.Downgrade().First(s=>s.Label==spaceLabel) ); // !!! Testing extension - move to testing project

}
