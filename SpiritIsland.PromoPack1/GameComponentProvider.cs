namespace SpiritIsland.PromoPack1;

public class GameComponentProvider : IGameComponentProvider {

	public Type[] Spirits => new Type[] {
		typeof(HeartOfTheWildfire),
		typeof(SerpentSlumbering),
		typeof(DownpourDrenchesTheWorld),
		typeof(FinderOfPathsUnseen),
	};

	public PowerCard[] MinorCards => Array.Empty<PowerCard>();

	public PowerCard[] MajorCards => Array.Empty<PowerCard>();

	public IFearCard[] FearCards => Array.Empty<IFearCard>();

	public IBlightCard[] BlightCards => Array.Empty<BlightCardBase>();

}