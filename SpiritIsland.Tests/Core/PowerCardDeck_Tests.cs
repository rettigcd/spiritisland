namespace SpiritIsland.Tests.Core;

public class PowerCardDeck_Tests {

	[Theory]
	[InlineData( AssemblyType.BaseGame,36)]
	[InlineData( AssemblyType.BranchAndClaw,31)]
	[InlineData( AssemblyType.JaggedEarth,33)]
	public void MinorCount(string edition, int expectedCount) {
		var minorCards = AssemblyType.GetEditionType(edition).ScanForMinors();
		minorCards.Length.ShouldBeGreaterThanOrEqualTo( expectedCount );
	}

	[Theory]
	[InlineData( AssemblyType.BaseGame,22)]
	[InlineData( AssemblyType.BranchAndClaw,21)]
	[InlineData( AssemblyType.JaggedEarth,23)]
	public void MajorCount(string edition, int expectedCount) {
		var majorCards = AssemblyType.GetEditionType( edition ).ScanForMajors();
		majorCards.Length.ShouldBeGreaterThanOrEqualTo( expectedCount );
	}

	[Fact]
	public void MementoRestoresDeck() {
		PowerCardDeck deck = new PowerCardDeck(new PowerCard[] {
			PowerCard.ForDecorated(Drought.ActAsync),
			PowerCard.ForDecorated(DevouringAnts.ActAsync),
			PowerCard.ForDecorated(DriftDownIntoSlumber.ActAsync),
			PowerCard.ForDecorated(EnticingSplendor.ActAsync),
			PowerCard.ForDecorated(LureOfTheUnknown.ActAsync),
			PowerCard.ForDecorated(NaturesResilience.ActAsync),
			PowerCard.ForDecorated(PullBeneathTheHungryEarth.ActAsync),
			PowerCard.ForDecorated(QuickenTheEarthsStruggles.ActAsync),
			PowerCard.ForDecorated(RainOfBlood.ActAsync),
			PowerCard.ForDecorated(SapTheStrengthOfMultitudes.ActAsync),
			PowerCard.ForDecorated(SongOfSanctity.ActAsync),
			PowerCard.ForDecorated(SteamVents.ActAsync),
			PowerCard.ForDecorated(UncannyMelting.ActAsync),
		}, new Random().Next(), PowerType.Minor );

		// Given: saving state
		var memento = deck.Memento;

		//   And: draw first 4 cards
		string originalFirstFourCards = deck.Flip(4).Select(c=>c.Title).Join(",");
		//   And: draw some more we don't care about
		deck.Flip(4);

		//  When: restore state using memento
		deck.Memento = memento;

		//  Then: first 4 cards should be the same
		deck.Flip(4).Select(c=>c.Title).Join(",").ShouldBe(originalFirstFourCards);
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
		static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateTierAttribute>().Any();
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
		static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateTierAttribute>().Any();
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
	public async Task DrawingMajor_ForgetACard(bool drawDirect) {
		var randomizer = new Random();
		var gs = new SoloGameState() {
			MajorCards = new PowerCardDeck( typeof(RiversBounty).ScanForMajors(), randomizer.Next(), PowerType.Major )
//			MinorCards = new PowerCardDeck( typeof( RiversBounty ).ScanForMinors(), randomizer.Next(), PowerType.Minor )
		};
		gs.InitMinorDeck();
		gs.Initialize();

		if(drawDirect) {
			await gs.Spirit.Draw.Major( true ).AwaitUser( user => {
				user.SelectMajorPowerCard();
				user.SelectCardToForget();
			} );

		}  else { 
			await gs.Spirit.Draw.Card().AwaitUser( user => {
				user.SelectsMajorDeck();
				user.SelectMajorPowerCard();
				user.SelectCardToForget();
			} );
		}
	}

	[Theory]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	public void PowerCards_HaveNames(string edition) {
		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = [.. refObject.ScanForMajors()];
		cards.AddRange( refObject.ScanForMinors() );

		foreach(var card in cards)
			card.Title.ShouldNotBeNullOrEmpty();

	}


}
