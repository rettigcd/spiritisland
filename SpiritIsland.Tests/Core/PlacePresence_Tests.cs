using Xunit.Sdk;

namespace SpiritIsland.Tests.Core; 
public class PlacePresence_Tests {

	#region helper classes

	class TestGameCtx{
		public readonly VirtualUser User;
		public readonly Spirit Spirit;
		public readonly GameState GameState;
		public readonly Board Board;

		public TestGameCtx(Spirit spirit){
			Spirit = spirit;
			User = new VirtualUser(Spirit);
			Board = Board.BuildBoardA();
			GameState = new GameState(Spirit, Board );
			GameState.Given_InitializedMinorDeck();
		}
	}

	class TestSpirit : Spirit {

		public TestSpirit():base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack( Track.Energy0, Track.Energy0, Track.Energy0 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
			),
			new GrowthTrack( new GrowthGroup( new PlacePresence( 1, Filter.Any ) ) )
		) {}

		public override string SpiritName => "Test Spirit";

		protected override void InitializeInternal( Board _, GameState _1 ){
			throw new NotImplementedException();
		}

	}

	#endregion helper classes

	[Fact]
	public async Task PullsFrom_EnergyTrack() {
		var ctx = new TestGameCtx(new TestSpirit());

		// Given: spirit has one place presence action
		ctx.Spirit.Given_IsOn(ctx.Board[1]);

		await ctx.Spirit.When_Growing(0, user => {
			user.PlacePresenceLocations( ctx.Spirit.Presence.Energy.RevealOptions.Single(), "A1;A2" );
		} );

		ctx.Spirit.Presence.Energy.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public async Task PullsFrom_CardTrack(){
		var ctx = new TestGameCtx(new TestSpirit());

		ctx.Spirit.Given_IsOn(ctx.Board[1]);

		await ctx.Spirit.When_Growing( 0, user => {
			user.PlacePresenceLocations( ctx.Spirit.Presence.CardPlays.RevealOptions.Single(), "A1;A2" );
		} );

		ctx.Spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public void AllStandardFilterStringsHaveMatcher() {
		var ctx = new TestGameCtx(new TestSpirit());

		ctx.Spirit.Given_IsOn(ctx.Board[1]);

		var space = ctx.Board[8];
		string[] standardFilters = typeof( Filter )
			.GetFields( BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy )
			.Where( fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof( string ) )
			.Select( x => (string)x.GetRawConstantValue() )
			.ToArray();

		foreach(string filter in standardFilters ) {
			var targetCriteria = new TargetCriteria(0,ctx.Spirit,filter);
			// don't care about results
			// just shouldn't throw exception about the filter we are using.
			targetCriteria.Matches(space.ScopeSpace);
		}

	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]	
	public async Task RevealingTrack_DoesNotRunAction( bool duringGrowth ){
		// AFTER tracks have been revealed, Actions are called independently by Spirit Growth Phase

		var ctx = new TestGameCtx(new RiverSurges());
		ctx.GameState.Initialize();

		// Given: spirit track is emptied up to an action Track (reclaim 1)
		ctx.Spirit.Presence.CardPlays.Given_SlotsRevealed(4);
		//   And: the action would have an effect
		ctx.Spirit.Given_HalfOfHandDiscarded();

		// Given: Spirit_Power or Spirit_Growth
		(ActionCategory category,Phase phase) = duringGrowth
			? (ActionCategory.Spirit_Growth,Phase.Growth)
			: (ActionCategory.Spirit_Power,Phase.Fast);
		await using ActionScope scope = await ActionScope.StartSpiritAction(category,ctx.Spirit);
		ctx.GameState.Phase = phase;

		// When: user places presence from that space
		await new PlacePresence(2).ActAsync(ctx.Spirit).AwaitUser(u=>{
			u.NextDecision.HasPrompt("Select Presence to place").HasOptions("2 energy,reclaim 1,RSiS on A5").Choose("reclaim 1");
			u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A1,A2,A3,A4,A5,A6,A7,A8").Choose("A8");
			// Then: Reclaim does not trigger.
			// No user prompt presented.
		}).ShouldComplete();

		
	}

	[Fact]
	public async Task RevealElementDuringPower_AddsIt(){

		var ctx = new TestGameCtx(new Thunderspeaker());
		ctx.GameState.Initialize();

		// Given: Spirit Power (Fast) Phase
		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,ctx.Spirit);
		ctx.GameState.Phase = Phase.Fast;

		//   And: no activated elements
		ctx.Spirit.Elements.Elements.BuildElementString(false).ShouldBe("");

		// When: user places presence from that space
		await new PlacePresence(2).ActAsync(ctx.Spirit).AwaitUser(u=>{
			u.NextDecision.HasPrompt("Select Presence to place").HasOptions("air energy,2 cardplay,Ts on A3,Ts on A7").Choose("air energy");
			u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A1,A2,A3,A4,A5,A6,A7,A8").Choose("A8");
		}).ShouldComplete();

		// Then: Air is activated
		ctx.Spirit.Elements.Elements.BuildElementString(false).ShouldBe("air");

	}

}

