
namespace SpiritIsland.Tests.Core;

public class PlacePresence_Tests {

	[Fact]
	public async Task PullsFrom_EnergyTrack() {

		// Given: spirit has one place presence action
		var gs = Given_GsWithPlacePresenceSpirit();

		gs.Spirit.Given_IsOn(gs.Board[1]);

		await gs.Spirit.When_Growing(0, user => {
			user.PlacePresenceLocations( gs.Spirit.Presence.Energy.RevealOptions.Single(), "A1;A2" );
		} );

		gs.Spirit.Presence.Energy.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public async Task PullsFrom_CardTrack(){

		// Given: spirit has one place presence action
		var gs = Given_GsWithPlacePresenceSpirit();

		gs.Spirit.Given_IsOn(gs.Board[1]);

		await gs.Spirit.When_Growing( 0, user => {
			user.PlacePresenceLocations( gs.Spirit.Presence.CardPlays.RevealOptions.Single(), "A1;A2" );
		} );

		gs.Spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public void AllStandardFilterStringsHaveMatcher() {

		// Given: spirit has one place presence action
		var gs = Given_GsWithPlacePresenceSpirit();

		gs.Spirit.Given_IsOn(gs.Board[1]);

		var space = gs.Board[8];
		string[] standardFilters = typeof( Filter )
			.GetFields( BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy )
			.Where( fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof( string ) )
			.Select( x => (string)x.GetRawConstantValue() )
			.ToArray();

		foreach(string filter in standardFilters ) {
			var targetCriteria = new TargetCriteria(0,gs.Spirit,filter);
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

		var gs = new SoloGameState(new RiverSurges());
		gs.Initialize();

		// Given: spirit track is emptied up to an action Track (reclaim 1)
		await gs.Spirit.Presence.CardPlays.Given_SlotsAreRevealed(4);
		//   And: the action would have an effect
		gs.Spirit.Given_HalfOfHandDiscarded();

		// Given: Spirit_Power or Spirit_Growth
		(ActionCategory category,Phase phase) = duringGrowth
			? (ActionCategory.Spirit_Growth,Phase.Growth)
			: (ActionCategory.Spirit_Power,Phase.Fast);
		await using ActionScope scope = await ActionScope.StartSpiritAction(category,gs.Spirit);
		gs.Phase = phase;

		// When: user places presence from that space
		await new PlacePresence(2).ActAsync(gs.Spirit).AwaitUser(u=>{
			u.NextDecision.HasPrompt("Select Presence to place")
				// .HasOptions("2 energy,reclaim 1,RSiS on A5")
				.Choose("reclaim 1 => A8");
			//u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A1,A2,A3,A4,A5,A6,A7,A8").Choose("A8");
			// Then: Reclaim does not trigger.
			// No user prompt presented.
		}).ShouldComplete();
		
	}

	[Fact]
	public async Task RevealElementDuringPower_AddsIt(){

		var gs = new SoloGameState(new Thunderspeaker());
		gs.Initialize();

		// Given: Spirit Power (Fast) Phase
		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,gs.Spirit);
		gs.Phase = Phase.Fast;

		//   And: no activated elements
		gs.Spirit.Elements.Elements.BuildElementString(false).ShouldBe("");

		// When: user places presence from that space
		await new PlacePresence(2).ActAsync(gs.Spirit).AwaitUser(u=>{
			u.NextDecision.HasPrompt("Select Presence to place")
			.Choose("air energy => A8");
			//u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A1,A2,A3,A4,A5,A6,A7,A8").Choose("A8");
		}).ShouldComplete();

		// Then: Air is activated
		gs.Spirit.Elements.Elements.BuildElementString(false).ShouldBe("air");

	}

	#region helpers

	/// <summary>
	/// Creates a GameState with a spirit that has a PlacePresence(1,Any) growth
	/// </summary>
	static SoloGameState Given_GsWithPlacePresenceSpirit()
		=> new SoloGameState(new TestSpirit(new PlacePresence(1, Filter.Any)));

	#endregion helpers
}

