
namespace SpiritIsland.Basegame;

public class DarkAndFireAsOne(Spirit spirit):ElementMgr(spirit) {

	public const string Name = "Dark and Fire as One";
	const string Description = "You may treat each Moon available to you as being Fire, or vice versa. You may discard or Forget Powers that grant Moon to pay for Fire Choice Events, and vice versa.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) { spirit.Elements = new DarkAndFireAsOne(spirit); }

	public override Task<int> CommitToCount(Element single) => single switch {
		// for Moon / fire, doesn't handle Multi-Elements
		Element.Moon or Element.Fire => Task.FromResult(Elements[Element.Moon] + Elements[Element.Fire]),
		// everythin else handles multi-elements like ANY
		_ => base.CommitToCount(single)
	};

	public override ECouldHaveElements CouldHave(CountDictionary<Element> subset)
		=> Contains(subset)	? ECouldHaveElements.Yes : ECouldHaveElements.No;

	public override Task<bool> MeetThreshold(CountDictionary<Element> subset, string _)
		=> Task.FromResult(Contains(subset));

	#region private

	// Merges Fire & Moon and calls .Contains
	bool Contains(CountDictionary<Element> subset)
		=> CombineMoonAndFire(Elements).Contains(CombineMoonAndFire(subset));

	/// <summary> Combines Moon and Fire counts into a new Dictionary </summary>
	static CountDictionary<Element> CombineMoonAndFire(CountDictionary<Element> orig) {
		var combined = new CountDictionary<Element>();
		foreach(var pair in orig)
			combined[CombinedElement(pair.Key)] += pair.Value;
		return combined;
	}
	static Element CombinedElement( Element orig ) => orig switch { Element.Moon or Element.Fire => MoonFire, _ => orig };
	const Element MoonFire = Element.Moon | Element.Fire;

	#endregion private

}