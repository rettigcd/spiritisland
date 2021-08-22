using System;
using System.Linq;
using Shouldly;
using Xunit;

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

		// === older ===
		static public void Old_SelectOption( this IDecision decision, string prompt, string optionText ) {

			if(!decision.Prompt.StartsWith( prompt ))
				decision.Prompt.ShouldBe( prompt );

			var option = decision.Options.FirstOrDefault( o => o.Text == optionText );
			if(option == null)
				throw new Exception( $"option ({optionText} not found in "
					+ decision.Options.Select( x => x.Text ).Join( ", " )
				);
			decision.Select( option );
		}

		static public void Old_SelectGrowthOption(this IDecision decision, int optionIndex ) {
			Assert.Equal( "Select Growth Option", decision.Prompt );
			decision.Select( decision.Options[optionIndex] );
		}


		static public void Old_BuyPowerCards( this IDecision decision, string text ) => decision.Old_SelectOption( "Buy power cards:", text );

		static public void Old_DoneWith( this IDecision decision, Speed speed ) {
			decision.Old_SelectOption( $"Select {speed} to resolve", "Done" );
		}

		static public void Old_SelectOption( this IDecision decision, string prompt, IOption option ) {

			if(!decision.Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, decision.Prompt );
			decision.Select( option );
		}

		static public void Old_SelectOptionContains( this IDecision decision, string prompt, string substring ) {

			if(!decision.Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, decision.Prompt );

			var option = decision.Options.FirstOrDefault( o => o.Text.ToLower().Contains( substring.ToLower() ) );
			if(option == null)
				throw new Exception( $"option ({substring} not found in "
					+ decision.Options.Select( x => x.Text ).Join( ", " )
				);
			decision.Select( option );
		}

		static public void Old_PlacePresence1( this IDecision decision, string sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.Old_SelectOptionContains( "Select Presence to place", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_PlacePresence1( this IDecision decision, Track sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.Old_SelectOption( "Select Presence to place", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_Reclaim1( this IDecision decision, string cardToReclaim ) {
			decision.Old_SelectOption( "Select Growth to resolve", "Reclaim(1)" );
			decision.Old_SelectOption( "Select card to reclaim.", cardToReclaim );
		}



	}

}
