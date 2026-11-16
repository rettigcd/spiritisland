namespace SpiritIsland.Tests.Core;

public class TypedDecision_Tests {

	[Fact]
	public void ChoosingCustomCancelText_GivesNoResult() {
		// Given: a decision with its own cancel text
		var decision = new A.TypedDecision<TextOption>( "Pick", [new TextOption("a"), new TextOption("b")], "Destroy it!" );
		IOption cancel = decision.Options.Last();
		cancel.Text.ShouldBe( "Destroy it!" );

		// When: the cancel option is chosen
		bool ok = decision.TryGetResultFromOption( cancel, out TextOption? result );

		// Then: accepted, with no result (and no InvalidCastException)
		ok.ShouldBeTrue();
		result.ShouldBeNull();
	}

	[Fact]
	public void ChoosingRealOption_GivesThatOption() {
		var a = new TextOption("a");
		var decision = new A.TypedDecision<TextOption>( "Pick", [a], "Destroy it!" );
		decision.TryGetResultFromOption( a, out TextOption? result ).ShouldBeTrue();
		result.ShouldBeSameAs( a );
	}
}
