namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Leave a Trail of Deathly Silence" ), Fast, Yourself]
public class LeaveATrailOfDeathlySilence {

	[InnateTier( "2 moon,1 animal", "1 Damage at Incarna. You may Push Incarna.", 0 )]
	static public async Task Option1( Spirit self ) {
		if( self.Presence is not IHaveIncarna ihi || ihi.Incarna.Space == null ) return;

		var incarnaCtx = self.Target(ihi.Incarna.Space.Space );
		await incarnaCtx.DamageInvaders(1,Human.Invader);

		await incarnaCtx.PushUpTo(1,ihi.Incarna.Class);
	}

	[InnateTier( "3 moon,1 air, 1 animal", "1 Damage at Incarna. You may Push Incarna.", 1 )]
	static public Task Option2( Spirit self ) => Option1( self );

	[InnateTier( "4 moon,3 air, 3 animal", "1 Damage at Incarna. You may Push Incarna.", 2 )]
	static public Task Option3( Spirit self ) => Option1( self );

	[InnateTier( "5 moon,2 air,3 animal", "Move Incarna to Endless-Dark. It Brings 1 Invader (from its land).", 3 )]
	static public async Task Option4( Spirit self ) {
		if(self.Presence is not IHaveIncarna ihi || ihi.Incarna.Space == null) return;

		var from = ihi.Incarna.Space.Space;
		var to = EndlessDark.Space;

		await ihi.Incarna.On(from).MoveTo(to); // !!! if Incarna inheritted from SpaceToken, this would be seriously simplified

		await new TokenMover(self,"Bring",from,to)
			.AddGroup( 1, Human.Invader )
			.DoN();

	}


}