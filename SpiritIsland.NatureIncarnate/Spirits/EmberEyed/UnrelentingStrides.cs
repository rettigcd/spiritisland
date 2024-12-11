
namespace SpiritIsland.NatureIncarnate;

class UnrelentingStrides : IModifyAvailableActions {

	static public SpecialRule Rule => new SpecialRule(
		"Unrelenting Strides",
		"On any turn that you don't use Innate Powers, you may use The Behemoth Rises an additional time."
	);

	#region constructor

	public UnrelentingStrides(Spirit spirit) {
		_spirit = spirit;
		_innate = spirit.InnatePowers[0];
		_behemoth = new TheBehemothRises();
	}

	#endregion constructor

	public void Modify(List<IActionFactory> orig, Phase _) {
		// Determine if we make Unreleting Strides an option (by adding Behemoth)
		if( BehemothUsed < (InnateUsed ? 1 : 2) )
			orig.Add(_behemoth);

		// Determine if we add 2nd Innate / OR make it unavailable.
		if( orig.Contains(_innate) && UsedUnrelentingStrides )
			orig.Remove(_innate);
	}

	#region private

	bool InnateUsed => _spirit.InnateWasUsed(_innate);
	bool UsedUnrelentingStrides => BehemothUsed == 2;

	int BehemothUsed => _spirit.UsedActions.Count(x => x == _behemoth);

	readonly InnatePower _innate;
	readonly Spirit _spirit;
	readonly TheBehemothRises _behemoth;

	#endregion private

}