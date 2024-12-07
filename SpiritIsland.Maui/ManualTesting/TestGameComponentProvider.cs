#if DEBUG

namespace SpiritIsland.Maui.ManualTesting;

public class TestGameComponentProvider : IGameComponentProvider {

	#region Spirits

	public string[] SpiritNames => [.. SpiritTypes.Keys];

	public Spirit? MakeSpirit(string spiritName) {
		return SpiritTypes.TryGetValue(spiritName, out Type? spiritType)
			? (Spirit?)Activator.CreateInstance(spiritType)
			: null;
	}

	static Dictionary<string, Type> SpiritTypes => new() {
		[SchoolSpirit.Name] = typeof(SchoolSpirit),
	};

	#endregion Spirits

	#region Aspects

	public AspectConfigKey[] AspectNames => [];

	public IAspect? MakeAspect(AspectConfigKey aspectName) => null;

	#endregion Aspects

	#region Adversaries

	public string[] AdversaryNames => [];
	public IAdversaryBuilder? MakeAdversary(string adversaryName) => null;

	#endregion Adversaries

	#region Cards

	public PowerCard[] MinorCards => [];
	public PowerCard[] MajorCards => [];
	public IFearCard[] FearCards => [];
	public BlightCard[] BlightCards => [];

	SpiritIsland.BlightCard[] IGameComponentProvider.BlightCards => [];

	#endregion Cards

}

#endif