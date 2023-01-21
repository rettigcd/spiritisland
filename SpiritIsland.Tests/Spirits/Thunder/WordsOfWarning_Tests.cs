namespace SpiritIsland.Tests.Spirits.Thunder;

public class WordsOfWarning_Tests : ThunderCards  {

	[Fact]
	public async Task DeadDahanDoDamage() {

		gs.Phase = Phase.Fast;

		// Disable destroying presence
		// gs.AddBlightSideEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };
		gs.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// Given: 2 dahan on a2
		gs.DahanOn( a[2] ).Init(2);
		// and: dahan on a4 so it doesn't auto-select the only target available
		gs.DahanOn( a[4] ).Init(1);

		//  and: 4 explorers + 1 city
		var counts = gs.Tokens[ a[2] ];
		counts.AdjustDefault( Invader.Explorer, 4 );
		counts.AdjustDefault( Invader.City, 1 );
		// and activate card
		When_ActivateCard( WordsOfWarning.Name );
			
		User.TargetsLand(WordsOfWarning.Name,"[A2],A4");

		// When: ravaging on A2
		gs.IslandWontBlight();
		await a[2].DoARavage( gs );

		// Then: 1 explorer left
		// Words of Warning defend 3 cancelling out City attack leaving only 4 damage from explorers
		// 2 Dahan attack simultaneously doing 4 points of damage, killing City and 1 explorer leaving 3 explorers
		gs.Assert_Invaders(a[2], "3E@1" );
	}

}