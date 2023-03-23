namespace SpiritIsland.Basegame;

public class RitualsOfDestruction {

	[SpiritCard("Rituals of Destruction",3,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant),Slow,FromSacredSite(1,Target.Dahan)]
	[Instructions( "2 Damage. If target land has at least 3 Dahan, +3 Damage and 2 Fear." ), Artist( Artists.SydniKruger )]
	static public Task ActAsync(TargetSpaceCtx ctx){

		// 2 damage
		int damage = 2;

		if( 3 <= ctx.Dahan.CountAll ) {
			damage += 3;
			ctx.AddFear( 2 );
		}

		return ctx.DamageInvaders( damage );

	}

}