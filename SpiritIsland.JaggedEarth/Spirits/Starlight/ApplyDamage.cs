namespace SpiritIsland.JaggedEarth;

class ApplyDamage : SpiritAction {

	public ApplyDamage():base( "Apply Damage" ) { }

	public override async Task ActAsync( Spirit self ) {
		var space = await self.SelectAlways("Select land to apply 2 Damage.", self.Presence.Lands);
		await self.Target(space).DamageInvaders(2);
	}

}
