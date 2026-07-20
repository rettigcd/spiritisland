namespace SpiritIsland;

/// <summary>
/// The "no adversary selected" placeholder builder - a real, empty-effect AdversaryLevel[] so
/// Adversary's normal machinery (ActiveLevels, Init/AdjustPlacedTokens) works unmodified rather than
/// needing a special-cased null-adversary branch elsewhere. Was a private nested class of GameBuilder;
/// promoted to its own public type so AdversaryRegistry (deserializing AdversaryConfig.NullAdversary)
/// can reuse it instead of re-deriving the same "no adversary" AdversaryLevel definition.
/// </summary>
public class NullAdversaryBuilder : IAdversaryBuilder {
	public string Name => "No Adversary";
	static public AdversaryLevel Level => new AdversaryLevel( _level: 0, _difficulty: 0, _fear1: 3, _fear2: 3, _fear3: 3, string.Empty );
	public AdversaryLevel[] Levels => [Level];
	public AdversaryLossCondition? LossCondition => null;
	public IAdversary Build( int _ ) => new Adversary( this, 0 );
}
