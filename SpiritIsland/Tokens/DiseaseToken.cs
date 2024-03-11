namespace SpiritIsland;

public class DiseaseToken : TokenClassToken, ISkipBuilds {
	const string DiseaseText = "Disease";
	public DiseaseToken() : base( DiseaseText, 'Z', Img.Disease ) {}

	public UsageCost Cost => UsageCost.Something; // we do lose the token

	public string Text => DiseaseText;

	public virtual async Task<bool> Skip( Space space ) {
		var result = await space.Disease.Remove( 1, RemoveReason.UsedUp );
		return result.Count == 1;
	}

}