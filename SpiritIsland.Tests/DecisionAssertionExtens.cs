using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace SpiritIsland.Tests {
	static class DecisionAssertionExtens {

		static public void AssertPrompt_ChooseFirst( this IDecisionStream decision, string prompt ) {
			string msg = $"{prompt}:[any]:[first]";
			if(decision.IsResolved)
				System.Threading.Thread.Sleep( 50 );
			decision.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			var current = decision.GetCurrent();
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );

			IOption choice = current.Options[0];
			decision.Choose( choice );

		}

		static public void AssertDecision( this IDecisionStream decision, string prompt, string optionsString, string select ) {
			string msg = $"{prompt}:{optionsString}:{select}";

			var current = decision.Assert_HasCurrent( prompt );
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			current.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = current.FindRequiredOptionByText( select );
			decision.Choose( match );
		}

		static public void AssertDecision( this IDecisionStream decision, string prompt, string select ) {
			string msg = $"{prompt}:{select}";

			var current = decision.Assert_HasCurrent(msg);
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			IOption match = current.FindRequiredOptionByText( select );
			decision.Choose( match );
		}

		static IDecision Assert_HasCurrent( this IDecisionStream decision, string prompt ) {
			if(decision.IsResolved)
				System.Threading.Thread.Sleep( 50 );
			decision.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			return decision.GetCurrent();
		}

		static IOption FindRequiredOptionByText( this IDecision current, string select ) {
			return current.Options.FirstOrDefault( x => x.Text == select ) // sometimes we will have double
				?? throw new Exception( $"option ({select} not found in " + current.Options.Select( x => x.Text ).Join( ", " ) );
		}

		// === older ===
		static public void Old_SelectOption( this IDecisionStream decision, string prompt, string optionText ) {

			if(!decision.GetCurrent().Prompt.StartsWith( prompt ))
				decision.GetCurrent().Prompt.ShouldBe( prompt );

			var option = decision.GetCurrent().Options.FirstOrDefault( o => o.Text == optionText );
			if(option == null)
				throw new Exception( $"option ({optionText} not found in "
					+ decision.GetCurrent().Options.Select( x => x.Text ).Join( ", " )
				);
			decision.Choose( option );
		}

		static public void Old_SelectGrowthOption(this IDecisionStream decision, int optionIndex ) {
			Assert.Equal( "Select Growth Option", decision.GetCurrent().Prompt );
			decision.Choose( decision.GetCurrent().Options[optionIndex] );
		}

		static public void Old_BuyPowerCards( this IDecisionStream decision, string text ) => decision.Old_SelectOption( "Buy power cards:", text );

		static public void Old_DoneWith( this IDecisionStream decision, Speed speed ) {
			decision.Old_SelectOption( $"Select {speed} to resolve", "Done" );
		}

		static public void Old_SelectOption( this IDecisionStream decision, string prompt, IOption option ) {

			if(!decision.GetCurrent().Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, decision.GetCurrent().Prompt );
			decision.Choose( option );
		}

		static public void Old_PlacePresence1( this IDecisionStream decision, string sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.AssertDecision( "Select Presence to place.", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_PlacePresence1( this IDecisionStream decision, Track sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.Old_SelectOption( "Select Presence to place", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_Reclaim1( this IDecisionStream decision, string cardToReclaim ) {
			decision.Old_SelectOption( "Select card to reclaim.", cardToReclaim );
		}

		static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
			gameState.Tokens[ space ].ToSummary().ShouldBe( expectedString );
		}


	}

}
