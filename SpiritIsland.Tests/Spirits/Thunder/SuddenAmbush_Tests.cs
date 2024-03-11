namespace SpiritIsland.Tests.Spirits.Thunder;

public class SuddenAmbush_Tests : ThunderCards {

	public SuddenAmbush_Tests() {
		gs.Phase = Phase.Fast;
	}

	[Fact]
	public void IsFastAndCost1() {
		var suddenAmbush = PowerCard.For(typeof(SuddenAmbush));
		suddenAmbush.DisplaySpeed.ShouldBe(Phase.Fast);
		suddenAmbush.Cost.ShouldBe( 2 );
	}

	[Fact]
	public async Task NoDahanToGather() {
		await spirit.When_ResolvingCard<SuddenAmbush>( (user) => {
			user.TargetsLand( SuddenAmbush.Name, "A1,[A2],A4,A5,A6" );
		} );
	}

	[Fact]
	public async Task Gather1_Kill1() {
		// Given: dahan on a3
		a[3].ScopeTokens.Dahan.Init(1);
		//  and: 2 explorers on a2
		gs.Tokens[a[2]].AdjustDefault(Human.Explorer,2);

		await spirit.When_ResolvingCard<SuddenAmbush>( (user) => {
			user.TargetsLand( SuddenAmbush.Name, "A1,[A2],A4,A5,A6" );
			user.GathersOptionalToken( "D@2" );
		} );

		// Then: 1 explorer left
		a[2].Assert_HasInvaders( "1E@1" );
	}

	[Fact]
	public async Task Gather1_Kills3() {
		// Given: 1 dahan on a2 & 2 dahan on a1
		a[2].ScopeTokens.Dahan.Init(1);
		a[1].ScopeTokens.Dahan.Init(2);
		//  and: 5 explorers on a1
		gs.Tokens[a[1]].AdjustDefault( Human.Explorer, 5 );

		await spirit.When_ResolvingCard<SuddenAmbush>( (user) => {
			user.TargetsLand( SuddenAmbush.Name, "[A1],A2,A4,A5,A6" );
			user.GathersOptionalToken( "D@2" );
		} );

		// Then: 5-2-1 = 2 explorers left
		a[1].Assert_HasInvaders( "2E@1" );
	}

	[Trait("Feature","Gather")]
	[Fact]
	public async Task DoesntKillTown() {
		// Given: 1 dahan on a2 & 2 dahan on a1
		a[2].ScopeTokens.Dahan.Init(1);
		a[1].ScopeTokens.Dahan.Init(2);
		//  and: 1 town on a1
		gs.Tokens[a[1]].AdjustDefault( Human.Town, 1 );

		await spirit.When_ResolvingCard<SuddenAmbush>( (user) => {
			user.TargetsLand( SuddenAmbush.Name, "[A1],A2,A4,A5,A6" );
			user.GathersOptionalToken( "D@2" );
		} );

		// Then: 5-2-1 = 2 explorers left
		a[1].Assert_HasInvaders( "1T@2" );
	}

}
