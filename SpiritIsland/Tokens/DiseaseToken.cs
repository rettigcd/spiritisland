using SpiritIsland.Select;

namespace SpiritIsland;

public class DiseaseToken : UniqueToken, ISkipBuilds {
	public DiseaseToken(string label, char k, Img img) : base(label,k, img) {
		Text= label;
	}

	public UsageCost Cost => UsageCost.Something; // we do lose the token

	public string Text { get; }

	public Task<bool> Skip( GameCtx gameCtx, SpaceState space, TokenClass buildClass ) {
		return gameCtx.GameState.Disease_StopBuildBehavior( gameCtx, space, buildClass);
	}
}
