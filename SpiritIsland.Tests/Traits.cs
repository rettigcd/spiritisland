/* 

using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpiritIsland.Tests;

This is how custom Traits are supposed to work, but it doesn't...

public class CategoryDiscoverer : ITraitDiscoverer {
	public const string KEY = "Category";
	public IEnumerable<KeyValuePair<string, string>> GetTraits( IAttributeInfo traitAttribute ) {
		var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
		yield return new KeyValuePair<string, string>( KEY, ctorArgs[0].ToString() );
	}
}

//NOTICE: Take a note that you must provide appropriate namespace here
[TraitDiscoverer( nameof( SpiritIsland ) + "." + nameof( SpiritIsland.Tests ) + "." + nameof( CategoryDiscoverer ), nameof(SpiritIsland.Tests) )]
[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
public class CategoryAttribute : Attribute, ITraitAttribute {
	public CategoryAttribute( string category ) { }
}


public class Bob {
	[Category("dude")]
	[Fact]
	public void MyTest() {
		typeof(CategoryDiscoverer).FullName.ShouldBe( "SpiritIsland.Tests.CategoryDiscoverer" );
		typeof( CategoryDiscoverer ).FullName.ShouldBe( nameof(SpiritIsland)+"."+nameof(SpiritIsland.Tests)+"."+nameof(CategoryDiscoverer) );
	}
}

*/
