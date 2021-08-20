using Xunit;
using SpiritIsland.Basegame;
using Shouldly;

namespace SpiritIsland.Tests {
	public class GetFearName_Test {
		[Fact]
		public void GetName() {
			new NamedFearCard { Card = new AvoidTheDahan() }
				.CardName.ShouldBe( "Avoid the Dahan" );
		}
	}

}
