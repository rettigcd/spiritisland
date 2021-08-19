using System.Linq;
using Shouldly;

namespace SpiritIsland.Tests {
	static class DecisionAssertionExtens {

		static public void AssertDecision( this TargetSpaceCtx ctx, string prompt, string optionsString, string select, bool done = false ) 
			=> ctx.Self.Action.AssertDecision(prompt,optionsString,select,done);

		static public void AssertDecision( this IDecision decision, string prompt, string optionsString, string select, bool done = false ) {
			string msg = $"{prompt}:{optionsString}:{select}";
			decision.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			decision.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = decision.Options.Single( x => x.Text == select );
			decision.Select( match );
			if(done)
				decision.Options.Length.ShouldBe( 0, msg );
			else
				decision.Options.Length.ShouldBeGreaterThan( 0, msg );
		}
	}

}
