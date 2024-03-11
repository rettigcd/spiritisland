namespace SpiritIsland.JaggedEarth;

class ApplyDamage : SpiritAction {

	public ApplyDamage():base( "Apply Damage" ) { }

	public override async Task ActAsync( Spirit self ) {
		var space = await self.SelectAsync(new A.SpaceDecision("Select land to apply 2 Damage.", self.Presence.Lands, Present.Always));
		await self.Target(space).DamageInvaders(2);
	}

}
