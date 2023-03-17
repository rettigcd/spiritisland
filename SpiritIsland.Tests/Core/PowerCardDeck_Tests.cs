namespace SpiritIsland.Tests.Core; 

public class PowerCardDeck_Tests {

	[Theory]
	[InlineData(BaseGame,36)]
	[InlineData(BranchAndClaw,31)]
	[InlineData(JaggedEarth,33)]
	public void MinorCount(string edition, int expectedCount) {
		var minorCards = GetEditionType(edition).GetMinors();
		// minorCards.Length.ShouldBe( 36 ); // Basegame
		minorCards.Length.ShouldBeGreaterThanOrEqualTo( expectedCount );
	}

	[Theory]
	[InlineData(BaseGame,22)]
	[InlineData(BranchAndClaw,21)]
	// [InlineData(JaggedEarth,0)]
	public void MajorCount(string edition, int expectedCount) {
		var majorCards = GetEditionType( edition ).GetMajors();
		majorCards.Length.ShouldBeGreaterThanOrEqualTo( expectedCount );
	}

	[Fact]
	public void MementoRestoresDeck() {
		PowerCardDeck deck = new PowerCardDeck(new PowerCard[] {
			PowerCard.For<Drought>(),
			PowerCard.For<DevouringAnts>(),
			PowerCard.For<DriftDownIntoSlumber>(),
			PowerCard.For<EnticingSplendor>(),
			PowerCard.For<LureOfTheUnknown>(),
			PowerCard.For<NaturesResilience>(),
			PowerCard.For<PullBeneathTheHungryEarth>(),
			PowerCard.For<QuickenTheEarthsStruggles>(),
			PowerCard.For<RainOfBlood>(),
			PowerCard.For<SapTheStrengthOfMultitudes>(),
			PowerCard.For<SongOfSanctity>(),
			PowerCard.For<SteamVents>(),
			PowerCard.For<UncannyMelting>(),
		}, new Random().Next() );

		// Given: saving state
		var memento = deck.SaveToMemento();

		//   And: draw first 4 cards
		string originalFirstFourCards = deck.Flip(4).Select(c=>c.Name).Join(",");
		//   And: draw some more we don't care about
		deck.Flip(4);

		//  When: restore state using memento
		deck.RestoreFrom(memento);

		//  Then: first 4 cards should be the same
		deck.Flip(4).Select(c=>c.Name).Join(",").ShouldBe(originalFirstFourCards);
	}

	#region Target-Space

	[Fact]
	public void PowerCard_Targets_Space_CorrectParameters() {
		// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.
		static bool MethodTargetsSpace( MethodBase m ) => m.GetCustomAttributes<TargetSpaceAttribute>().Any();
		var methods = typeof( PowerCard ).Assembly.GetTypes()
			.OrderBy( x => x.Namespace )
			.ThenBy( x => x.Name )
			.SelectMany( x => x.GetMethods() )
			.Where( MethodTargetsSpace )
			.ToArray();

		ShouldHave_TargetSpace_MethodSignature( methods );
	}

	[Fact]
	public void Innate_Targets_Space_CorrectParameters() {
		// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.

		static bool TypeTargetsSpace( Type m ) => m.GetCustomAttributes<TargetSpaceAttribute>().Any();
		static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateOptionAttribute>().Any();
		var methods = typeof( PowerCard ).Assembly.GetTypes()
			.Where( TypeTargetsSpace )
			.OrderBy( x => x.Namespace )
			.ThenBy( x => x.Name )
			.SelectMany( x => x.GetMethods() )
			.Where( MethodIsOption )
			.ToArray();

		ShouldHave_TargetSpace_MethodSignature( methods );

	}

	#endregion

	#region Target - Spirit

	[Fact]
	public void PowerCard_Targets_Spirit_CorrectParameters() {
		// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.
		static bool MethodTargetsSpace( MethodBase m ) => m.GetCustomAttributes<AnySpiritAttribute>().Any();
		var methods = typeof( PowerCard ).Assembly.GetTypes()
			.OrderBy( x => x.Namespace )
			.ThenBy( x => x.Name )
			.SelectMany( x => x.GetMethods() )
			.Where( MethodTargetsSpace )
			.ToArray();

		ShouldHave_TargetSpirit_MethodSignature( methods );
	}

	[Fact]
	public void Innate_Targets_Spirit_CorrectParameters() {
		// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.

		static bool TypeTargetsSpace( Type m ) => m.GetCustomAttributes<AnySpiritAttribute>().Any();
		static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateOptionAttribute>().Any();
		var methods = typeof( PowerCard ).Assembly.GetTypes()
			.Where( x=>x!=typeof( GiftOfStrength ) ) // ! this has a different signature - !! review it and see if it is still relavent
			.Where( TypeTargetsSpace )
			.OrderBy( x => x.Namespace )
			.ThenBy( x => x.Name )
			.SelectMany( x => x.GetMethods() )
			.Where( MethodIsOption )
			.ToArray();

		ShouldHave_TargetSpirit_MethodSignature( methods );

	}

	#endregion

	static void ShouldHave_TargetSpace_MethodSignature( MethodInfo[] methods ) {
		string[] problems = methods.Where( m => {
			var methodParams = m.GetParameters();
			return m.ReturnType.Name != "Task"
				|| methodParams.Length != 1
				|| methodParams[0].Name != "ctx"
				|| methodParams[0].ParameterType.Name != "TargetSpaceCtx";
		} )
			.Select( m => {
				var methodParams = m.GetParameters();
				var paramString = methodParams.Select( p => p.ParameterType.Name + " " + p.Name ).Join( "," );
				return $"{m.ReturnType.Name} {m.DeclaringType.Name}.{m.Name}({paramString})";
			} )
			.ToArray();

		problems.Length.ShouldBe( 0, problems.Take( 5 ).Join( "\r\n" ) );
	}

	static void ShouldHave_TargetSpirit_MethodSignature( MethodInfo[] methods ) {
		string[] problems = methods.Where( m => {
			var methodParams = m.GetParameters();
			return m.ReturnType.Name != "Task"
				|| methodParams.Length != 1
				|| methodParams[0].Name != "ctx"
				|| methodParams[0].ParameterType.Name != "TargetSpiritCtx";
		} )
			.Select( m => {
				var methodParams = m.GetParameters();
				var paramString = methodParams.Select( p => p.ParameterType.Name + " " + p.Name ).Join( "," );
				return $"{m.ReturnType.Name} {m.DeclaringType.Name}.{m.Name}({paramString})";
			} )
			.ToArray();

		problems.Length.ShouldBe( 0, problems.Take( 5 ).Join( "\r\n" ) );
	}

	[Theory]
	[InlineData(true)]
	[InlineData( false )]
	public void DrawingMajor_ForgetACard(bool drawDirect) {
		var spirit = new RiverSurges();
		var user = new VirtualUser(spirit);
		var randomizer = new Random();
		var gs = new GameState( spirit, Board.BuildBoardC() ) {
			MajorCards = new PowerCardDeck( typeof(RiversBounty).GetMajors(), randomizer.Next(), PowerType.Major ),
			MinorCards = new PowerCardDeck( typeof( RiversBounty ).GetMinors(), randomizer.Next(), PowerType.Minor )
		};
		gs.Initialize();

		if(drawDirect) {
			spirit.DrawMajor( true ).AwaitUser( spirit, user => {
				user.SelectMajorPowerCard();
				user.SelectCardToForget();
			} ).Wait(10);

		}  else { 
			spirit.Draw().AwaitUser( spirit, user => {
				user.SelectsMajorDeck();
				user.SelectMajorPowerCard();
				user.SelectCardToForget();
			} ).Wait(10);
		}
	}

	[Theory]
	[InlineData(BaseGame)]
	[InlineData(BranchAndClaw)]
	[InlineData(JaggedEarth)]
	public void PowerCards_HaveNames(string edition) {
		Type refObject = GetEditionType( edition );
		List<PowerCard> cards = refObject.GetMajors().ToList();
		cards.AddRange( refObject.GetMinors() );

		foreach(var card in cards)
			card.Name.ShouldNotBeNullOrEmpty();

	}

	static Type GetEditionType( string edition ) {
		return edition switch {
			BaseGame => typeof( RiverSurges ),
			BranchAndClaw => typeof( SharpFangs ),
			JaggedEarth => typeof( ShiftingMemoryOfAges ),
			_ => throw new ArgumentException( "Edition not found", nameof( edition ) ),
		};
	}

	const string BaseGame = "Basegame";
	const string BranchAndClaw = "Branch and Claw";
	const string JaggedEarth = "Jagged Earth";

}

