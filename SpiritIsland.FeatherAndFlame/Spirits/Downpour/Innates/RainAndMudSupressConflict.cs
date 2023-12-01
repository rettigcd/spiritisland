namespace SpiritIsland.FeatherAndFlame;


[InnatePower( RainAndMudSupressConflict.Name), Fast, Yourself]
internal class RainAndMudSupressConflict {

	const string Name = "Rain and Mud Suppress Conflict";

	// Use unit of work to coordinate between tier levels
	static bool WasUsed() => ActionScope.Current.ContainsKey(Name);
	static void MarkAsUsed() => ActionScope.Current[ Name ] = true;

	const string Tier1Elements = "1 air,3 water";

	[InnateTier( Tier1Elements, "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1. (Total, in its land.)", 0 )]
	static public Task Option1( Spirit self ) {
		MakeThingsMuddy( self );
		return Task.CompletedTask;
	}


	[InnateTier( "5 water,1 earth", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1.", 1 )]
	static public async Task Option2( Spirit self ) {
		if( await DontWantMoreMud(self) ) return;
		MakeThingsMuddy( self );
	}

	[InnateTier( "3 air,9 water,2 earth", "2 Fear. In your lands, Invaders and Dahan have -1 Health (min 1)", 0 )]
	static public async Task Option3( Spirit self ) {
		if( await DontWantMoreMud( self ) ) return; 

		// 2 fear
		self.AddFear(2);

		// In your lands, Invaders and Dahan have -1 Health( min 1 )
		foreach(var space in self.Presence.Lands.Tokens()) {
			var targetCtx = self.Target( space.Space );
			await targetCtx.AdjustTokensHealthForRound( -1,Human.Dahan );
			await targetCtx.AdjustTokensHealthForRound( -1, Human.Invader );
		}
	}

	static public void MakeThingsMuddy( Spirit self ) {
		var gs = GameState.Current;
		// Each of your Presence grants Defend 1
		gs.Tokens.Dynamic.ForRound.Register( sp => sp[self.Presence.Token], Token.Defend );
		// lowers Dahan counterattack damage by 1
		gs.AddIslandMod( new MudToken( self, 1 ) );
		MarkAsUsed();
	}

	// Replace with DoRavage
	//	 sets based on presence
	//   doesn't remove self (so works for multiple ravages)

	static async Task<bool> DontWantMoreMud( Spirit self ) => WasUsed()
			&& !await self.UserSelectsFirstText( "Apply Additional Tier?", "Yes, we want more mud.", "No, thank you." );

}


class MudToken : BaseModEntity, IEndWhenTimePasses, IConfigRavages {
	readonly Spirit _self;
	readonly int _count;
	public MudToken( Spirit self, int count ):base() { // removes itself
		_self = self;
		_count = count;
	}

	void IConfigRavages.Config( SpaceState space ) {
		space.RavageBehavior.AttackersDefend += _self.Presence.CountOn( space ) * _count;
	}
}
