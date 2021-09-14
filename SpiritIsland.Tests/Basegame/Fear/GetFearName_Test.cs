using Xunit;
using SpiritIsland.Basegame;
using Shouldly;

namespace SpiritIsland.Tests.Core {

	public class GetFearName_Test { // !!!
		[Fact]
		public void GetName() {
			new DisplayFearCard( new AvoidTheDahan() )
				.Text.ShouldBe( "Avoid the Dahan" );
		}
	}

}
