namespace SpiritIsland.Tests; 
public class VirtualUser {

	#region constructor

	public VirtualUser(Spirit spirit ) { 
		this.spirit = spirit;
		this.userPortal = spirit.Gateway;
	}

	#endregion

	#region Growth

	DecisionContext NextDecision => spirit.NextDecision();

	public void Growth_SelectsOption( string growthOption ) {
		NextDecision.HasPrompt( "Select Growth Option" )
			.Choose( growthOption );
	}

	public void Growth_SelectAction( string growthOption ) {
		NextDecision.HasPrompt( "Select Growth" )
			.Choose( growthOption );
	}

	public void Growth_SelectAction( string growthOption, int index ) {
		NextDecision.HasPrompt( "Select Growth" )
			.ChooseFirst( growthOption, index );
	}


	public void Growth_SelectsOption(int growthOptionIndex) {
		NextDecision.HasPrompt( "Select Growth Option" )
			.Choose( growthOptionIndex );
	}

	public void Growth_DrawsPowerCard() {
		NextDecision.ChooseFirst( "DrawPowerCard" );
	}

	public void Growth_ReclaimsAll() {
		NextDecision.Choose( "ReclaimAll" );
	}

	public void Growth_Reclaims1(string cards) {
		NextDecision.Choose( "Reclaim(1)" );
		AssertDecisionInfo( ReclaimN.Prompt,  cards );
	}

	public void Growth_GainsEnergy() {
		NextDecision.ChooseFirstThatStartsWith( "GainEnergy" );
	}

	public void Growth_PlacesEnergyPresence( string placeOptions ) 
		=> PlacesPresence( spirit.Presence.Energy.RevealOptions.Single(), placeOptions );

	public void Growth_PlacesPresence( string placeOptions ) {
		string[] parts = placeOptions.Split('>');
		Track source = parts[0].ToLower() switch {
			"energy" => spirit.Presence.Energy.RevealOptions.Single(),
			"cardplay" => spirit.Presence.CardPlays.RevealOptions.Single(),
			"cardplays" => spirit.Presence.CardPlays.RevealOptions.Single(),
			_ => throw new ArgumentOutOfRangeException(nameof(placeOptions)),
		};
		PlacesPresence( source, parts[1] );
	}

	public void PlacesCardPlayPresence( string placeOptions ) => PlacesPresence( spirit.Presence.CardPlays.RevealOptions.Single(), placeOptions );

	public void PlacesPresence( Track source, string placeOptions ) {

		// If Place Presence is the only action in the growth option, it will auto-select and it will skip over this step
		var current = userPortal.Next;
		var op = current.Options.FirstOrDefault( o => o.Text.StartsWith( "PlacePre" ) );
		if(op != null)
			NextDecision.Choose( op );

		PlacePresenceLocations( source, placeOptions );
	}

	public void PlacePresenceLocations( Track source, string placeOptions ) {
		// Source
		PullsPresenceFromTrack( source );

		// place on board - first option
		string[] expectedOptions = placeOptions.Split( ';' );
		var destinationDecision = userPortal.Next;
		var actualOptions = destinationDecision.Options;
		var choice = actualOptions.SingleOrDefault( o => o.Text == expectedOptions[0] );
		if(choice == null)
			throw new System.ArgumentOutOfRangeException( nameof( placeOptions ), $"'{expectedOptions[0]}' not found in " + actualOptions.Select( o => o.Text ).Join( "," ) );
		NextDecision.Choose( choice );
	}

	public void PullsPresenceFromTrack( Track source ) {
		NextDecision.Choose( source );
	}

	public void Reclaims1FromTrackBonus(string cards) {
		AssertDecisionInfo( "Select card to reclaim.",  cards );
	}

	public void Reclaims1CardIfAny() {
		var current = userPortal.Next;
		if(current.Options.Length>0)
			userPortal.Choose( current, current.Options[0] );
	}

	#endregion

	public void PlaysCard( string cardName ) {
		var card = this.spirit.Hand.First(c=>c.Name == cardName);
		BuysPowerCard( card );
	}

	public void BuysPowerCard( PowerCard card ) {
		NextDecision.HasPromptPrefix( "Play power card" ).Choose( card.Text );
	}

	#region Fear 

	public void AcknowledgesFearCard( string fearCard ) {
		var parts = fearCard.Split(" : ");
		var cardName = parts[0];// We used to include Level and Description but now we only use the Fear Card Name
		AssertDecision( "Activating Fear", cardName, cardName ); // some of the fear cards have commas in them
	}


	#endregion

	#region Fast / Slow Actions

	public void SelectsSlowAction(string actions) {
		var (options,choice) = SplitOptionsAndChoice( actions );
		choice = PowerNameToText( choice );
		options = options.Split(',').Select( PowerNameToText ).Join(",");
		AssertDecision( "Select Slow to resolve", options+",Done", choice );
	}

	/// <example>Fast-0,(Fast-1),Fast-2,Gift of Strength</example>
	public void SelectsFastAction(string actions) {
		var (options, choice) = SplitOptionsAndChoice( actions );

		choice = PowerNameToText( choice );
		options = options.Split(',').Select( PowerNameToText ).Join(",");

		AssertDecision( "Select Fast to resolve", options + ",Done", choice );
	}

	string PowerNameToText( string choice ) {
		return this.spirit.InPlay.FirstOrDefault( c => c.Name == choice )?.Text 
			?? choice; // it is an Innate Power and not in the purchased card list.
	}

	#endregion

	#region IsDone / Skip

	public void IsDoneBuyingCards() {
		// The prompt for choosing power card has spirits Energy embeded in it
		// so we need to let the engine catch up before we generate the prompt.
		NextDecision.HasPromptPrefix( $"Play power card" ).Choose( "Done" );
	}

	public void IsDoneWith( Phase speed ) {
		NextDecision.HasPromptPrefix( $"Select {speed} to resolve" ). Choose( "Done" );
	}

	#endregion

	public void PushesTokensTo( string invaders, string destinations, int numToPush=1 ) {
		var (_,tokenToPush) = SplitOptionsAndChoice( invaders );
		AssertDecisionInfo( "Push ("+numToPush+")", invaders );
		AssertDecisionInfo( "Push "+tokenToPush+" to", destinations );
	}

	public void PushSelectedTokenTo( string invaders, string destinations ) {
		var (_, tokenToPush) = SplitOptionsAndChoice( invaders );
		AssertDecisionInfo( "Push " + tokenToPush + " to", destinations );
	}

	public void OptionallyPushesInvaderTo( string invaders, string destinations, int countToPush=1 ) {
		var (invaderOptions,invaderChoice) = SplitOptionsAndChoice( invaders );
		AssertDecision( $"Push up to ({countToPush})", invaderOptions+",Done", invaderChoice );
		AssertDecisionInfo( "Push "+invaderChoice+" to", destinations );
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
		AssertDecisionInfo( "Select Growth to resolve", "GatherPresenceIntoOcean" );
	}

	public void PushesPresenceFromOcean(string destination) {
		AssertDecisionInfo( "Select Growth to resolve", "PushPresenceFromOcean" );
		AssertDecisionInfo( "Push Presence to", destination );
	}

	public void PlacesPresenceInOcean( string growth, string source, string destination ) {
		AssertDecisionInfo( "Select Growth to resolve", growth );
		AssertDecisionInfo( "Select Presence to place", source );
		AssertDecisionInfo( "Where would you like to place your presence?", destination );
	}

	#endregion

	public void TargetsSpirit( string actionName, string spirits ) {
		AssertDecisionInfo( actionName + ": Target Spirit", spirits );
	}

	public void TargetsLand( string powerName, string space ) {
		AssertDecisionInfo( powerName + ": Target Space", space );
	}

	public void TargetsLand_IgnoreOptions( string space ) => NextDecision.Choose( space );

	public void SelectsDamageRecipient( int damageAvailable, string tokens ) {
		AssertDecisionInfo( "Damage ("+damageAvailable+" remaining)", tokens );
	}

	public void GathersOptionalToken( string token ) {
		var (options,choice) = SplitOptionsAndChoice( token );
		NextDecision
			.HasOptions( options+",Done" )
			.Choose( choice );
	}

	public void SelectsMajorDeck() => AssertDecisionInfo( "Which type do you wish to draw", "minor,[major]");
	public void SelectsMinorDeck() => AssertDecisionInfo( "Which type do you wish to draw", "[minor],major" );
	public void SelectMinorPowerCard() => NextDecision.HasPrompt( "Select minor Power Card" ).ChooseFirst();
	public void SelectMajorPowerCard() => NextDecision.HasPrompt( "Select Major Power Card" ).ChooseFirst();
	public void SelectCardToForget()   => NextDecision.HasPrompt( "Select power card to forget" ).ChooseFirst();
	public void ActivateFear()         => NextDecision.HasPrompt( "Activating Fear" ).ChooseFirst();

	public void Assert_Done() {
		userPortal.IsResolved.ShouldBeTrue();
	}

	#region protected

	public void AssertDecisionInfo( string prompt, string optionInfo ) {

		var (options, choice) = SplitOptionsAndChoice( optionInfo );
		NextDecision
			.HasPrompt( prompt )
			.HasOptions( options )
			.ChooseFirst( choice );
	}

	public void AssertDecision( string prompt, string optionsString, string select ) {
		NextDecision
			.HasPrompt( prompt )
			.HasOptions( optionsString )
			.ChooseFirst( select );
	}

	protected static (string, string) SplitOptionsAndChoice( string options ) {
		const string markers = "[]";

		if(!options.Contains( ',' ))
			return (options, options); // only 1 option, no need for (...)

		int open = options.IndexOf( markers[0] );
		if(open == -1) throw new FormatException( "No '" + markers[0] + "' found in '" + options + "' for indicating choice" );
		int close = options.IndexOf( markers[1], open );
		if(close == -1) throw new FormatException( "No '" + markers[1] + "' found for indicating choice" );
		string choice = options.Substring( open + 1, close - open - 1 );
		options = string.Concat( options.AsSpan()[..open], options.AsSpan( open + 1, close - open - 1 ), options.AsSpan()[(close + 1)..] );
		return (options, choice);
	}

	#endregion

	readonly protected Spirit spirit;
	readonly protected IUserPortal userPortal;

}

