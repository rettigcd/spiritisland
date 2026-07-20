using System.Reflection;

namespace SpiritIsland.Serialization;

/// <summary>
/// Looks up the stable, process-wide `ITokenClass` singletons (Token.Beast, Human.Town, etc.)
/// by their `Label`, so serialized references don't need to hold the instance directly.
/// </summary>
/// <remarks>
/// Scans the well-known static holder classes once. Add a type here if a project introduces
/// its own static holder of `ITokenClass`/`HumanTokenClass` singletons.
/// </remarks>
public static class TokenClassRegistry {

	public static ITokenClass ByLabel( string label ) => _byLabel.Value[label];

	static readonly Type[] _holderTypes = [ typeof( Token ), typeof( Human ) ];

	static readonly Lazy<Dictionary<string, ITokenClass>> _byLabel = new( BuildRegistry );

	static Dictionary<string, ITokenClass> BuildRegistry() {
		var result = new Dictionary<string, ITokenClass>();
		foreach( Type holder in _holderTypes ) {
			foreach( FieldInfo field in holder.GetFields( BindingFlags.Public | BindingFlags.Static ) )
				if( typeof( ITokenClass ).IsAssignableFrom( field.FieldType ) && field.GetValue( null ) is ITokenClass fromField )
					result[fromField.Label] = fromField;

			// Some holders (e.g. Human.Explorer/Town/City) expose singletons as properties, not fields.
			foreach( PropertyInfo prop in holder.GetProperties( BindingFlags.Public | BindingFlags.Static ) )
				if( typeof( ITokenClass ).IsAssignableFrom( prop.PropertyType ) && prop.GetValue( null ) is ITokenClass fromProp )
					result[fromProp.Label] = fromProp;
		}
		return result;
	}

}
