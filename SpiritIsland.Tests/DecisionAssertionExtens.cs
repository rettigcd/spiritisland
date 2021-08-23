﻿using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace SpiritIsland.Tests {
	static class DecisionAssertionExtens {

		static public void AssertDecision( this TargetSpaceCtx ctx, string prompt, string optionsString, string select, bool done = false ) 
			=> ctx.Self.Action.AssertDecision(prompt,optionsString,select,done);

		static public void AssertDecision( this IDecisionStream decision, string prompt, string optionsString, string select, bool _ = false ) {
			string msg = $"{prompt}:{optionsString}:{select}";

			// we might get an error if the engine isn't here yet
			if( decision.IsResolved ) 
				System.Threading.Thread.Sleep(50);
			decision.IsResolved.ShouldBeFalse($"Dude! Decision [{prompt}] is not there.");

			var current = decision.GetCurrent();
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			current.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = current.Options.First( x => x.Text == select ); // sometimes we will have double
			decision.Choose( match );
			//if(done)
			//	decision.IsResolved.ShouldBeTrue( msg );
			//else
			//	decision.IsResolved.ShouldBeFalse( msg );
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

		static public void Old_SelectOptionContains( this IDecisionStream decision, string prompt, string substring ) {

			if(!decision.GetCurrent().Prompt.StartsWith( prompt ))
				Assert.Equal( prompt, decision.GetCurrent().Prompt );

			var option = decision.GetCurrent().Options.FirstOrDefault( o => o.Text.ToLower().Contains( substring.ToLower() ) );
			if(option == null)
				throw new Exception( $"option ({substring} not found in "
					+ decision.GetCurrent().Options.Select( x => x.Text ).Join( ", " )
				);
			decision.Choose( option );
		}

		static public void Old_PlacePresence1( this IDecisionStream decision, string sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.Old_SelectOptionContains( "Select Presence to place", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_PlacePresence1( this IDecisionStream decision, Track sourceTrack, string destinationSpace ) {
			decision.Old_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			decision.Old_SelectOption( "Select Presence to place", sourceTrack );
			decision.Old_SelectOption( "Where would you like", destinationSpace );
		}

		static public void Old_Reclaim1( this IDecisionStream decision, string cardToReclaim ) {
			decision.Old_SelectOption( "Select Growth to resolve", "Reclaim(1)" );
			decision.Old_SelectOption( "Select card to reclaim.", cardToReclaim );
		}



	}

}
