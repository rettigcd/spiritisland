using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class VirtualUser {

		public VirtualUser(Spirit spirit ) { this.spirit = spirit; }

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

			var current = spirit.Action.GetCurrent();

			var op = current.Options.First( o => o.Text.StartsWith( "PlacePre" ) );
			spirit.Action.Choose( op );

			// Resolve Power
			var cardDecision = spirit.Action.GetCurrent();
			cardDecision.Options.Select( x => x.Text ).Join( "," ).ShouldContain( source.Text );
			// take from precense track
			spirit.Action.Choose( source );

			// place on board - first option
			string[] expectedOptions = placeOptions.Split( ';' );
			var destinationDecision = spirit.Action.GetCurrent();
			var actualOptions = destinationDecision.Options;
			var choice = actualOptions.SingleOrDefault( o => o.Text == expectedOptions[0] );
			if(choice==null)
				throw new System.ArgumentOutOfRangeException(nameof(placeOptions), $"'{expectedOptions[0]}' not found in "+ actualOptions.Select(o=>o.Text).Join("," ));
			spirit.Action.Choose( choice );

		}

		public void DrawsPowerCard() {
			spirit.Action.Choose( "DrawPowerCard" );
		}

		public void ReclaimsAll() {
			spirit.Action.Choose( "ReclaimAll" );
		}

		public void Reclaims1FromTrackBonus(string cards) {
			AssertDecisionX( "Select card to reclaim.",  cards, "{}" );
		}

		public void Reclaims1FromGrowth(string cards) {
			spirit.Action.Choose( "Reclaim(1)" );
			AssertDecisionX( "Select card to reclaim.",  cards, "{}" );
		}
		
		//public void SelectsExplorerToPush( string explorerOptions, string explorerChoice ) {
		//	AssertDecision( "Push (1)", explorerOptions, explorerChoice );
		//}

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
			Assert.Equal( "Select Growth Option", spirit.Action.GetCurrent().Prompt );
			spirit.Action.Choose( growthOption );
		}

		public void SelectsGrowthOption(int growthOptionIndex) {
			var current = spirit.Action.GetCurrent();
			Assert.Equal( "Select Growth Option", current.Prompt );
			spirit.Action.Choose( current.Options[growthOptionIndex] );
		}

		public void GainsEnergy() {
			var current = spirit.Action.GetCurrent();
			var selection = current.Options.First(x=>x.Text.StartsWith("GainEnergy"));
			spirit.Action.Choose( selection );
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
			var current = spirit.Action.GetCurrent();
			var selection = current.Options.First( x => x.Text.StartsWith( "PlayExtra" ) );
			spirit.Action.Choose( selection );
		}

		public void TargetsSpirit( string spirits ) {
			AssertDecisionX( "Select Spirit to target", spirits );
		}

		public void TargetsLand( string space ) {
			AssertDecisionX( "Select space to target.", space );
		}

		public void SelectsDamageRecipient( int damage, string tokens ) {
			AssertDecisionX( "Apply damage("+damage+") to:", tokens );
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
				var current = spirit.Action.GetCurrent();
				Assert.Equal(
					expected.OrderBy(x=>x).Join(",")
					,current.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",")
				);
			}
			Assert_Options( options, "Done" );
			spirit.Action.Choose( choice );
		}

		public void SelectsMajorPowerCard() {
			AssertDecisionX( "Which type do you wish to draw", "minor,(major)");
		}

		public void SelectsFirstOption( string prompt ) {
			string msg = $"{prompt}:[any]:[first]";
			if(spirit.Action.IsResolved)
				System.Threading.Thread.Sleep( 50 );
			spirit.Action.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			var current = spirit.Action.GetCurrent();
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );

			IOption choice = current.Options[0];
			spirit.Action.Choose( choice );

		}

		public void Reclaims1CardIfAny() {
			var current = spirit.Action.GetCurrent();
			if(current.Options.Length>0)
				spirit.Action.Choose( current.Options[0] );
		}

		public void Assert_Done() {
			spirit.Action.IsResolved.ShouldBeTrue();
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
			var decision = spirit.Action;
			var current = decision.GetCurrent();
			if(!current.Prompt.StartsWith( prompt ))
				current.Prompt.ShouldBe( prompt );

			var option = current.Options.FirstOrDefault( o => o.Text == optionText );
			if(option == null)
				throw new Exception( $"option ({optionText} not found in "
					+ decision.GetCurrent().Options.Select( x => x.Text ).Join( ", " )
				);
			decision.Choose( option );
		}

		protected void AssertDecision( string prompt, string select ) {
			string msg = $"{prompt}:{select}";

			var current = Assert_HasCurrent(msg);
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			IOption match = FindRequiredOptionByText( current, select );
			spirit.Action.Choose( match );
		}

		protected void AssertDecisionX( string prompt, string optionInfo, string markers = "()" ) {
			var (options,choice) = SplitOptionsAndChoice( optionInfo, markers );
			AssertDecision( prompt, options, choice );
		}

		protected void AssertDecision( string prompt, string optionsString, string select ) {
			IDecisionStream decision = spirit.Action;
			string msg = $"{prompt}:{optionsString}:{select}";

			var current = Assert_HasCurrent( prompt );
			current.Prompt.ShouldBe( prompt, msg, StringCompareShould.IgnoreCase );
			current.Options.Select( x => x.Text ).Join( "," ).ShouldBe( optionsString, msg );
			IOption match = FindRequiredOptionByText( current, select );
			decision.Choose( match );
		}

		protected IDecision Assert_HasCurrent( string prompt ) {
			var decision = spirit.Action;

			// There is a problem with the Single-Player-Game that doesn't get to the next option fast enough.
			if(decision.IsResolved)
				System.Threading.Thread.Sleep( 5 );

			decision.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
			return decision.GetCurrent();
		}

		static protected IOption FindRequiredOptionByText( IDecision current, string select ) {
			return current.Options.FirstOrDefault( x => x.Text == select ) // sometimes we will have double
				?? throw new Exception( $"option ({select} not found in " + current.Options.Select( x => x.Text ).Join( ", " ) );
		}

		#endregion

		readonly protected Spirit spirit;

	}

}
