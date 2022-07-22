namespace SpiritIsland.Tests;

public class GrowthTests {

	protected Spirit spirit;
	protected VirtualUser User {get; }

	protected GameState gameState;
	protected Board board;

	protected GrowthTests(Spirit spirit):base(){
		// PlayerState requires Spirit to be known because Spirit creates playerState.
		this.spirit = spirit;
		this.User = new VirtualUser(spirit);
		board = BoardA;
		gameState = new GameState(spirit, board );
	}

	#region Board factories

	static protected Board BoardA => Board.BuildBoardA();
	static protected Board BoardB => Board.BuildBoardB();
	static protected Board BoardC => Board.BuildBoardC();
	static protected Board BoardD => Board.BuildBoardD();

	#endregion

	#region Given

	protected void Given_HasPresence( params Space[] spaces ) {
		foreach(var x in spaces)
			spirit.Presence.PlaceOn( x, gameState );
	}

	protected void Given_HasPresence( string presenceString ) {
		Dictionary<string,Space> spaceLookup = gameState.Island.Boards
			.SelectMany(b=>b.Spaces)
			.ToDictionary(s=>s.Label,s=>s);
		var spaces = new Space[presenceString.Length/2];
		for(int i=0;i*2<presenceString.Length;i++)
			spaces[i] = spaceLookup[presenceString.Substring(i*2,2)];
		Given_HasPresence(spaces);
	}

	protected void Given_HalfOfPowercardsPlayed() {
		Discard(spirit.Hand.Count-1);
		Discard(spirit.Hand.Count-1);
	}
	void Discard(int idx){
		spirit.DiscardPile.Add( spirit.Hand[idx] );
		spirit.Hand.RemoveAt( idx );
	}

	#endregion

	protected Task When_Growing( int option) {
		try {
			gameState.Phase = Phase.Growth;
			return spirit.GrowAndResolve( spirit.GrowthTrack.Options[option],gameState);
		} catch {
			throw;
		}
	}

	#region Asserts

	protected void Assert_BoardPresenceIs( string expected ) {
		var actual = spirit.Presence.Placed.Select(s=>s.Label).OrderBy(l=>l).Join();
		Assert.Equal(expected, actual); // , Is.EqualTo(expected),"Presence in wrong place");
	}

	protected void Assert_HasCardAvailable( string name ){
		bool nameMatches( PowerCard card ) => string.Compare(name,card.Name,true) == 0;
		Assert.True(spirit.Hand.Any( nameMatches ),$"Hand does not contain {name}");
	}

	protected void Assert_HasPowerProgressionCard( int index ) {
		Assert_HasCardAvailable( (spirit.CardDrawer as PowerProgression).Cards[index].Name );
	}

	protected void Assert_AllCardsAvailableToPlay(int expectedAvailableCount = 4) {
		// Then: all cards reclaimed (including unplayed)
		Assert.Empty( spirit.DiscardPile ); // , "Should not be any cards in 'played' pile" );
		spirit.Hand.Count.ShouldBe( expectedAvailableCount );
	}

	protected void Assert_HasEnergy( int expectedChange ) {
		spirit.Energy.ShouldBe( expectedChange );
	}

	#endregion

	#region Resolve_

	protected void Assert_PresenceTracksAre(int expectedEnergy,int expectedCards) {
		Assert_EnergyTrackIs( expectedEnergy );
		Assert_CardTrackIs( expectedCards );
	}

	public void Assert_CardTrackIs( int expectedCards ) {
		Assert.Equal( expectedCards, spirit.NumberOfCardsPlayablePerTurn );
	}

	public void Assert_EnergyTrackIs( int expectedEnergy ) {
		Assert.Equal( expectedEnergy, spirit.EnergyPerTurn );
	}

	protected void When_StartingGrowth() {
		gameState.Phase = Phase.Growth;
		_ = spirit.DoGrowth( gameState );
	}

	protected void PlayCard(params PowerCard[] cards ) {
		foreach(var card in cards)
			spirit.PlayCard( card );
	}

	#endregion

}