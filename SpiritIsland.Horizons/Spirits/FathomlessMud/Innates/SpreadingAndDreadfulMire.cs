namespace SpiritIsland.Horizons;

[InnatePower(Name)]
[Slow,FromSacredSite(1,Filter.Invaders)]
public class SpreadingAndDreadfulMire {

	public const string Name = "Spreading and Dreadful Mire";

	[InnateTier("1 water","Move 1 presence from the origin Sacred Site to target land.",0)]
	static public async Task Option1(TargetSpaceCtx ctx) {
		var options = ctx.Self.Presence.Token
			.On( TargetSpaceAttribute.TargettedSpace!.Sources)
			.Select(x=>new Move(x,ctx.Space));
		var move = await ctx.Self.SelectAlways("Move presence to Target", options);
		await move.Apply();
	}

	[InnateTier("1 moon,2 water,1 earth","1 Fear. 1 Damage. Push 1 Dahan",1)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return FearDamagePushDahan(ctx,1,1,1);
	}

	[InnateTier("2 moon,3 water,2 earth", "1 Fear. 1 Damage. Push 1 Dahan", 1)]
	static public Task Option3(TargetSpaceCtx ctx) {
		return FearDamagePushDahan(ctx, 2, 2, 2);
	}

	[InnateTier("3 moon,4 water,3 earth,2 plant", "2 Damage", 1)]
	static public Task Option4(TargetSpaceCtx ctx) {
		return FearDamagePushDahan(ctx, 2, 4, 2);
	}

	static async Task FearDamagePushDahan(TargetSpaceCtx ctx,int fear, int damage,int dahanToPush) {
		await ctx.AddFear(fear);
		await ctx.DamageInvaders(damage);
		await ctx.PushDahan(dahanToPush);
	}

}
