namespace SpiritIsland;

public class WildsToken( string label, char k, Img img ) : TokenClassToken(label,k, img), ISkipExploreTo {
	public UsageCost Cost => UsageCost.Something; // we do lose the token
	public async Task<bool> Skip( Space space ) {
		await space.Wilds.Remove(1, RemoveReason.UsedUp);
		return true;
	}
}
