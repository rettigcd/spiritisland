namespace SpiritIsland.Horizons;

public class MysteriousAbductions {

	public const string Name = "Mysterious Abductions";

	[SpiritCard(Name, 1, Element.Moon, Element.Fire, Element.Plant), Fast, FromPresence(0)]
	[Instructions("1 Fear. 1 Damage."), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 1 Fear.
		await ctx.AddFear(1);
		// 1 Damage.
		await ctx.DamageInvaders(1);
	}

}
