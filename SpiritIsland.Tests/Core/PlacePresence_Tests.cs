namespace SpiritIsland.Tests.Core; 
public class PlacePresence_Tests {

	#region helper classes

	class TestGameCtx{
		public readonly VirtualUser User;
		public readonly Spirit Spirit;
		public readonly GameState GameState;
		public readonly Board Board;

		public TestGameCtx(Spirit spirit){
			Spirit = new TestSpirit();
			User = new VirtualUser(Spirit);
			Board = Board.BuildBoardA();
			GameState = new GameState(Spirit, Board );
			GameState.Given_InitializedMinorDeck();
		}
	}

	class TestSpirit : Spirit {

		public override SpecialRule[] SpecialRules => throw new NotImplementedException();

		public TestSpirit():base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack( Track.Energy0, Track.Energy0, Track.Energy0 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
			),
			new GrowthTrack( new GrowthOption( new PlacePresence( 1, Filter.Any ) ) )
		) {}

		public override string Text => "Test Spirit";

		protected override void InitializeInternal( Board _, GameState _1 ){
			throw new NotImplementedException();
		}

	}

	#endregion helper classes

	[Fact]
	public async Task PullsFrom_EnergyTrack() {
		var ctx = new TestGameCtx(new TestSpirit());

		// Given: spirit has one place presence action
		ctx.Spirit.Given_HasPresenceOnSpaces(ctx.Board[1]);

		await ctx.Spirit.When_Growing(0, ()=> {
			ctx.User.PlacePresenceLocations( ctx.Spirit.Presence.Energy.RevealOptions.Single(), "A1;A2" );
		} );

		ctx.Spirit.Presence.Energy.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public async Task PullsFrom_CardTrack(){
		var ctx = new TestGameCtx(new TestSpirit());

		ctx.Spirit.Given_HasPresenceOnSpaces(ctx.Board[1]);

		await ctx.Spirit.When_Growing( 0, () => {
			ctx.User.PlacePresenceLocations( ctx.Spirit.Presence.CardPlays.RevealOptions.Single(), "A1;A2" );
		} );

		ctx.Spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public void AllStandardFilterStringsHaveMatcher() {
		var ctx = new TestGameCtx(new TestSpirit());

		ctx.Spirit.Given_HasPresenceOnSpaces(ctx.Board[1]);

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
			targetCriteria.Matches(space);
		}

	}

}

