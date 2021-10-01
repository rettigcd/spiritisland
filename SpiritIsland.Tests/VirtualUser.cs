﻿using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class VirtualUser {

		#region constructor

		public VirtualUser(Spirit spirit ) { 
			this.spirit = spirit;
			this.userPortal = spirit.Action;
		}

		#endregion

		public void AcknowledgesFearCard( string fearCard ) {
			AssertDecision( "Activating Fear", fearCard, fearCard ); // some of the fear cards have commas in them
		}

		public void PlacesPresence( string placeOptions ) {
			string[] parts = placeOptions.Split('>');
			Track source = parts[0].ToLower() switch {
				"energy" => spirit.Presence.Energy.Next,
				"cardplay" => spirit.Presence.CardPlays.Next,
				"cardplays" => spirit.Presence.CardPlays.Next,
				_ => throw new ArgumentOutOfRangeException(nameof(placeOptions)),
			};
			PlacesPresence( source, parts[1] );
		}

		public void PlacesEnergyPresence( string placeOptions ) => PlacesPresence( spirit.Presence.Energy.Next, placeOptions );
		public void PlacesCardPlayPresence( string placeOptions ) => PlacesPresence( spirit.Presence.CardPlays.Next, placeOptions );

		public void PlacesPresence( Track source, string placeOptions ) {

			var current = userPortal.GetCurrent();

			var op = current.Options.First( o => o.Text.StartsWith( "PlacePre" ) );
			Choose( op );

			// Source
			PullsPresenceFromTrack( source );

			// place on board - first option
			string[] expectedOptions = placeOptions.Split( ';' );
			var destinationDecision = userPortal.GetCurrent();
			var actualOptions = destinationDecision.Options;
			var choice = actualOptions.SingleOrDefault( o => o.Text == expectedOptions[0] );
			if(choice == null)
				throw new System.ArgumentOutOfRangeException( nameof( placeOptions ), $"'{expectedOptions[0]}' not found in " + actualOptions.Select( o => o.Text ).Join( "," ) );
			Choose( choice );
		}

		public void PullsPresenceFromTrack( Track source ) {
			var sourceDecision = userPortal.GetCurrent();
			sourceDecision.Options.Select( x => x.Text ).Join( "," ).ShouldContain( source.Text );
			Choose( source );
		}

		public void DrawsPowerCard() {
			Choose( "DrawPowerCard" );
		}

		public void ReclaimsAll() {
			Choose( "ReclaimAll" );
		}

		public void Reclaims1FromTrackBonus(string cards) {
			AssertDecisionX( "Select card to reclaim.",  cards, "{}" );
		}

		public void Reclaims1FromGrowth(string cards) {
			Choose( "Reclaim(1)" );
			AssertDecisionX( "Select card to reclaim.",  cards, "{}" );
		}
		
		public void PushesTokensTo( string invaders, string destinations, int numToPush=1 ) {
			var (_,tokenToPush) = SplitOptionsAndChoice( invaders );
			AssertDecisionX( "Push ("+numToPush+")", invaders );
			AssertDecisionX( "Push "+tokenToPush+" to", destinations );
		}

		public void OptionallyPushesInvaderTo( string invaders, string destinations, int countToPush=1 ) {
			var (invaderOptions,invaderChoice) = SplitOptionsAndChoice( invaders );
			AssertDecision( $"Push up to ({countToPush})", invaderOptions+",Done", invaderChoice );
			AssertDecisionX( "Push "+invaderChoice+" to", destinations );
		}

		public void SelectsGrowthOption( string growthOption ) {
			Assert.Equal( "Select Growth Option", userPortal.GetCurrent().Prompt );
			Choose( growthOption );
		}

		public void SelectsGrowthOption(int growthOptionIndex) {
			var current = userPortal.GetCurrent();
			Assert.Equal( "Select Growth Option", current.Prompt );
			Choose( current.Options[growthOptionIndex] );
		}

		public void GainsEnergy() {
			var current = userPortal.GetCurrent();
			var selection = current.Options.First(x=>x.Text.StartsWith("GainEnergy"));
			Choose( selection );
		}

		public void GathersPresenceIntoOcean() {
			AssertDecisionX( "Select Growth to resolve:", "GatherPresenceIntoOcean" );
		}

		public void PushesPresenceFromOcean(string destination) {
			AssertDecisionX( "Select Growth to resolve:", "PushPresenceFromOcean" );
			AssertDecisionX( "Select target of Presence to Push from A0", destination );
		}

		public void PlacesPresenceInOcean( string growth, string source, string destination ) {
			AssertDecisionX( "Select Growth to resolve:", growth );
			AssertDecisionX( "Select Presence to place.", source );
			AssertDecisionX( "Where would you like to place your presence?", destination );
		}

		public void PlacesPresence( string source, string destination ) {
			SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			AssertDecision( "Select Presence to place.", source );
			SelectOption( "Where would you like", destination );
		}

		public void ActivatesExtraCardPlay() {
			var current = userPortal.GetCurrent();
			var selection = current.Options.First( x => x.Text.StartsWith( "PlayExtra" ) );
			Choose( selection );
		}

		public void TargetsSpirit( string spirits ) {
			AssertDecisionX( "Select Spirit to target", spirits );
		}

		public void TargetsLand( string space ) {
			AssertDecisionX( "Select space to target.", space );
		}

		public void TargetsLand_IgnoreOptions( string space ) {
			var current = Assert_HasCurrent( "Select space to target." );
			IOption match = FindRequiredOptionByText( current, space );
			Choose( match );
		}

		public void SelectsDamageRecipient( int damageAvailable, string tokens ) {
			AssertDecisionX( "Damage ("+damageAvailable+" remaining)", tokens );
		}

		public void BuysPowerCard( string powerCardName ) {
			SelectOption( "Buy power cards:", powerCardName );
		}

		public void IsDoneBuyingCards() {
			SelectOption( "Buy power cards:", "Done" );
		}

		public void IsDoneWith( Speed speed ) {
			SelectOption( $"Select {speed} to resolve", "Done" );
		}

		public void SelectsSlowAction(string actions) {
			var (options,choice) = SplitOptionsAndChoice( actions );
			AssertDecision( "Select Slow to resolve:", options+",Done", choice );
		}

		public void SelectsFastAction(string actions) {
			var (options,choice) = SplitOptionsAndChoice( actions );
			AssertDecision( "Select Fast to resolve:", options+",Done", choice );
		}

		public void GathersOptionalToken( string token ) {
			var (options,choice) = SplitOptionsAndChoice( token );

			void Assert_Options( params string[] expected ) {
				// This is kind of crappy
				var current = userPortal.GetCurrent();
				Assert.Equal(
					expected.OrderBy(x=>x).Join(",")
					,current.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",")
				);
			}
			Assert_Options( options, "Done" );
			userPortal.Choose( choice );
		}

		public void SelectsMajorPowerCard() {
			AssertDecisionX( "Which type do you wish to draw", "minor,(major)");
		}

		public void SelectsFirstOption( string prompt ) {
			string msg = $"{prompt}:[any]:[first]";
			userPortal.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			var current = userPortal.GetCurrent();
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );

			IOption choice = current.Options[0];
			userPortal.Choose( choice );

		}

		public void Reclaims1CardIfAny() {
			var current = userPortal.GetCurrent();
			if(current.Options.Length>0)
				userPortal.Choose( current.Options[0] );
		}

		public void Assert_Done() {
			userPortal.IsResolved.ShouldBeTrue();
		}

		#region protected

		protected static (string,string) SplitOptionsAndChoice(string options,string markers = "()") {
			if(!options.Contains(','))
				return (options,options); // only 1 option, no need for (...)

			int open = options.IndexOf(markers[0]);
			if(open==-1) throw new FormatException("No '"+markers[0]+"' found for indicating choice");
			int close = options.IndexOf(markers[1],open);
			if(close==-1) throw new FormatException("No '"+markers[1]+"' found for indicating choice");
			string choice = options.Substring(open+1,close-open-1);
			options = options.Substring(0,open)+options.Substring(open+1,close-open-1)+options[(close + 1)..];
			return (options,choice);
		}

		protected void SelectOption( string prompt, string optionText ) {
			var current = userPortal.GetCurrent();
			if(!current.Prompt.StartsWith( prompt ))
				current.Prompt.ShouldBe( prompt );

			var option = current.Options.FirstOrDefault( o => o.Text == optionText );
			if(option == null)
				throw new Exception( $"option ({optionText} not found in "
					+ userPortal.GetCurrent().Options.Select( x => x.Text ).Join( ", " )
				);
			Choose( option );
		}

		protected void AssertDecision( string prompt, string select ) {
			string msg = $"{prompt}:{select}";

			var current = Assert_HasCurrent(msg);
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			IOption match = FindRequiredOptionByText( current, select );
			userPortal.Choose( match );
		}

		public void AssertDecisionX( string prompt, string optionInfo, string markers = "()" ) {
			var (options,choice) = SplitOptionsAndChoice( optionInfo, markers );
			AssertDecision( prompt, options, choice );
		}

		public void AssertDecision( string prompt, string optionsString, string select ) {
			string msg = $"{prompt}:{optionsString}:{select}";

			var current = Assert_HasCurrent( prompt );
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			current.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = FindRequiredOptionByText( current, select );
			Choose( match );
		}

		protected IDecision Assert_HasCurrent( string prompt ) {
			userPortal.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			return userPortal.GetCurrent();
		}

		static protected IOption FindRequiredOptionByText( IDecision current, string select ) {
			return current.Options.FirstOrDefault( x => x.Text == select ) // sometimes we will have double
				?? throw new Exception( $"option ({select} not found in " + current.Options.Select( x => x.Text ).Join( ", " ) );
		}

		protected void Choose(IOption option ) {
			userPortal.Choose( option );
			WaitForSignal();
		}
		protected void Choose( string option ) {
			userPortal.Choose( option );
			WaitForSignal();
		}

		protected void WaitForSignal() {
			userPortal.WaitForNextDecision(10);
		}

		#endregion

		readonly protected Spirit spirit;
		readonly protected IUserPortal userPortal;

	}

}
