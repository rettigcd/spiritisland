using Shouldly;
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

		#region Growth

		public void Growth_SelectsOption( string growthOption ) {
			var current = userPortal.GetCurrent();
			Assert.Equal( "Select Growth Option", current.Prompt );
			Choose( growthOption );
		}

		public void Growth_SelectsOption(int growthOptionIndex) {
			var current = userPortal.GetCurrent();
			Assert.Equal( "Select Growth Option", current.Prompt );
			Choose( current.Options[growthOptionIndex] );
		}

		public void Growth_DrawsPowerCard() {
			Choose( "DrawPowerCard" );
		}

		public void Growth_ReclaimsAll() {
			Choose( "ReclaimAll" );
		}

		public void Growth_Reclaims1(string cards) {
			Choose( "Reclaim(1)" );
			AssertDecisionX( Reclaim1.Prompt,  cards, "{}" );
		}

		public void Growth_GainsEnergy() {
			var current = userPortal.GetCurrent();
			var selection = current.Options.First(x=>x.Text.StartsWith("GainEnergy"));
			Choose( selection );
		}

		public void Growth_PlacesEnergyPresence( string placeOptions ) => PlacesPresence( spirit.Presence.Energy.Next, placeOptions );

		public void Growth_ActivatesExtraCardPlay() { // Rampant Green
			var current = userPortal.GetCurrent();
			var selection = current.Options.First( x => x.Text.StartsWith( "PlayExtra" ) );
			Choose( selection );
		}

		public void Growth_PlacesPresence( string placeOptions ) {
			string[] parts = placeOptions.Split('>');
			Track source = parts[0].ToLower() switch {
				"energy" => spirit.Presence.Energy.Next,
				"cardplay" => spirit.Presence.CardPlays.Next,
				"cardplays" => spirit.Presence.CardPlays.Next,
				_ => throw new ArgumentOutOfRangeException(nameof(placeOptions)),
			};
			PlacesPresence( source, parts[1] );
		}

		public void PlacesCardPlayPresence( string placeOptions ) => PlacesPresence( spirit.Presence.CardPlays.Next, placeOptions );

		public void PlacesPresence( Track source, string placeOptions ) {

			var current = userPortal.GetCurrent();
			var op = current.Options.First( o => o.Text.StartsWith( "PlacePre" ) );
			Choose( op );

			PlacePresenceLocations( source, placeOptions );
		}

		public void PlacePresenceLocations( Track source, string placeOptions ) {
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

		public void Reclaims1FromTrackBonus(string cards) {
			AssertDecisionX( "Select card to reclaim.",  cards, "{}" );
		}

		public void Reclaims1CardIfAny() {
			var current = userPortal.GetCurrent();
			if(current.Options.Length>0)
				userPortal.Choose( current.Options[0] );
		}

		#endregion

		public void BuysPowerCard( string cardName ) {
			var card = this.spirit.Hand.First(c=>c.Name == cardName);
			BuysPowerCard( card );
		}

		public void BuysPowerCard( PowerCard card ) {
			SelectOption( "Buy power cards:", card.Text );
		}

		#region Fear 

		public void AcknowledgesFearCard( string fearCard ) {
			AssertDecision( "Activating Fear", fearCard, fearCard ); // some of the fear cards have commas in them
		}


		#endregion

		#region Fast / Slow Actions

		public void SelectsSlowAction(string actions) {
			var (options,choice) = SplitOptionsAndChoice( actions );
			choice = PowerNameToText( choice );
			options = options.Split(',').Select( PowerNameToText ).Join(",");
			AssertDecision( "Select Slow to resolve:", options+",Done", choice );
		}

		public void SelectsFastAction(string actions) {
			var (options, choice) = SplitOptionsAndChoice( actions );

			choice = PowerNameToText( choice );
			options = options.Split(',').Select( PowerNameToText ).Join(",");

			AssertDecision( "Select Fast to resolve:", options + ",Done", choice );
		}

		string PowerNameToText( string choice ) {
			return this.spirit.PurchasedCards.FirstOrDefault( c => c.Name == choice )?.Text 
				?? choice; // it is an Innate Power and not in the purchased card list.
		}

		#endregion

		#region IsDone / Skip

		public void IsDoneBuyingCards() {
			SelectOption( "Buy power cards:", "Done" );
		}

		public void IsDoneWith( Phase speed ) {
			SelectOption( $"Select {speed} to resolve", "Done" );
		}

		#endregion

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

		#region SharpFangs-spirit Specific

#pragma warning disable CA1822 // Mark members as static
		/// <summary> for Sharp Fangs </summary>
		public void SkipsPresenceReplacementWithBeasts() {
#pragma warning restore CA1822 // Mark members as static

			//// !!! pop this out and put in Sharp Fangs
			//var current = userPortal.GetCurrent();
			//var selection = current.Options.First(x=>x.Text.StartsWith("ReplacePresenceWithBeast"));
			//Choose( selection );

			//System.Threading.Thread.Sleep(5);
			//current = userPortal.GetCurrent();
			//selection = current.Options.First(x=>x.Text.StartsWith("Done"));
			//Choose( selection );

		}

		#endregion

		#region Ocean-Spirit Specific

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

		#endregion

		public void TargetsSpirit( string actionName, string spirits ) {
			AssertDecisionX( actionName + ": Target Spirit", spirits );
		}

		public void TargetsLand( string powerName, string space ) {
			AssertDecisionX( powerName + ": Target Space", space );
		}

		public void TargetsLand_IgnoreOptions( string space ) {
			var current = Assert_HasCurrent( "Select space to target." );
			IOption match = FindRequiredOptionByText( current, space );
			Choose( match );
		}

		public void SelectsDamageRecipient( int damageAvailable, string tokens ) {
			AssertDecisionX( "Damage ("+damageAvailable+" remaining)", tokens );
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
			var current = Assert_HasCurrent( prompt );

			string msg = $"{prompt}:{optionsString}:{select} => " + current.Options.Select(x=>x.Text).Join(",");

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
