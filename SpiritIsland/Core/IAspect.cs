namespace SpiritIsland;

/// <summary>
/// Used by Game Config to select Aspect and map to spirit.
/// </summary>
/// <param name="Spirit">Spirits Name</param>
/// <param name="Name">Aspect Name</param>
public record AspectConfigKey(string Spirit,string Aspect);

public interface IAspect {
	void ModSpirit( Spirit spirit );
	string[] Replaces { get; }
}

public static class AspectHelper_Extensoins {
	static public void RemoveMod<T>(this Spirit spirit) {
		var mod = spirit.Mods.OfType<T>().Single();
		spirit.Mods.Remove(mod);
	}
}