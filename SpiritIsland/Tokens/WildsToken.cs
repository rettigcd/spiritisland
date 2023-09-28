namespace SpiritIsland;

public class WildsToken : TokenClassToken, ISkipExploreTo {
	public WildsToken(string label, char k, Img img) : base(label,k, img) { }
	public UsageCost Cost => UsageCost.Something; // we do lose the token
	public async Task<bool> Skip( SpaceState space ) {
		await space.Wilds.Remove(1, RemoveReason.UsedUp);
		return true;
	}
}
