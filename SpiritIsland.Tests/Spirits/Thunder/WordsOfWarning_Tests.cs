namespace SpiritIsland.Tests.Spirits.Thunder;

public class WordsOfWarning_Tests : ThunderCards  {

	[Fact]
	public async Task DeadDahanDoDamage() {

		gs.Phase = Phase.Fast;

		// Disable destroying presence
		// gs.AddBlightSideEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };
		gs.DisableBlightEffect();

		// Given: 2 dahan on a2
		a[2].ScopeSpace.Dahan.Init(2);
		// and: dahan on a4 so it doesn't auto-select the only target available
		a[4].ScopeSpace.Dahan.Init(1);

		//  and: 4 explorers + 1 city
		var counts = gs.Tokens[ a[2] ];
		counts.AdjustDefault( Human.Explorer, 4 );
		counts.AdjustDefault( Human.City, 1 );
		// and activate card
		await spirit.When_ResolvingCard<WordsOfWarning>( (user) => {
			user.TargetsLand( WordsOfWarning.Name, "[A2],A4" );
		} );

		// When: ravaging on A2
		gs.IslandWontBlight();
		await a[2].When_Ravaging();

		// Then: 1 explorer left
		// Words of Warning defend 3 cancelling out City attack leaving only 4 damage from explorers
		// 2 Dahan attack simultaneously doing 4 points of damage, killing City and 1 explorer leaving 3 explorers
		a[2].Assert_HasInvaders( "3E@1" );
	}

}