namespace SpiritIsland.Tests.Core; 
public class PlacePresence_Tests : GrowthTests {

	class TestSpirit : Spirit {

		public override SpecialRule[] SpecialRules => throw new NotImplementedException();

		public TestSpirit():base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack( Track.Energy0, Track.Energy0, Track.Energy0 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
			)
		) {
			GrowthTrack = new GrowthTrack( new GrowthOption( new PlacePresence( 1, Target.Any ) ) );
		}

		public override string Text => "Test Spirit";

		protected override void InitializeInternal( Board _, GameState _1 ){
			throw new NotImplementedException();
		}

	}

	public PlacePresence_Tests():base(new TestSpirit()){
		Given_HasPresence(_board[1]);
	}


	[Fact]
	public async Task PullsFrom_EnergyTrack() {
		// Given: spirit has one place presence action

		await _spirit.When_Growing(0, ()=> {
			User.PlacePresenceLocations( _spirit.Presence.Energy.RevealOptions.Single(), "A1;A2" );
		} );

		_spirit.Presence.Energy.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public async Task PullsFrom_CardTrack(){
		await _spirit.When_Growing( 0, () => {
			User.PlacePresenceLocations( _spirit.Presence.CardPlays.RevealOptions.Single(), "A1;A2" );
		} );

		_spirit.Presence.CardPlays.Revealed.Count().ShouldBe(2);
	}

	[Fact]
	public void AllTargetsHaveFilter() {

		var space = _board[8];
		foreach(string filter in GetAllPublicConstantValues<string>( typeof( Target ) ) ) {
			var targetCriteria = new TargetCriteria(0,_spirit,filter);
			// don't care about results
			// just shouldn't throw exception about the filter we are using.
			targetCriteria.Matches(space);
		}

	}

	static List<T> GetAllPublicConstantValues<T>( Type type ) {
		return type
			.GetFields( BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy )
			.Where( fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof( T ) )
			.Select( x => (T)x.GetRawConstantValue() )
			.ToList();
	}

}

