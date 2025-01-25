namespace SpiritIsland.JaggedEarth;

// Boards
// S:	E,B,C
// A:	F,H,G
// B:	A,D


public class ShiftingMemoryOfAges : Spirit, IHaveSecondaryElements {

	public const string Name = "Shifting Memory Of Ages";
	public override string SpiritName => Name;

	#region Presence Track Helpers

	static Track Prepare(int energy) => PrepareElement.MakeTrack(energy);
	static Track DiscardElements => DiscardElementsForCardPlay.MakeTrack(2);

	#endregion Presence Track Helpers

	public ShiftingMemoryOfAges() 
		:base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.Energy2,Prepare(3),Track.Energy4,Track.Reclaim1Energy,Track.Energy5,Prepare(6)),
				new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,DiscardElements,Track.Card3)
			)
			,new GrowthTrack(
				new GrowthGroup( new ReclaimAll(), new PlacePresence( 0 ) ),
				new GrowthGroup( new GainPowerCard(), new PlacePresence( 2 ) ),
				new GrowthGroup( new PlacePresence( 1 ), new GainEnergy( 2 ) ),
				new GrowthGroup( new GainEnergy( 9 ) )
			)
			,PowerCard.ForDecorated(BoonOfAncientMemories.ActAsync)
			,PowerCard.ForDecorated(ElementalTeachings.ActAsync)
			,PowerCard.ForDecorated(ShareSecretsOfSurvival.ActAsync)
			,PowerCard.ForDecorated(StudyTheInvadersFears.ActAsync)
		) {

		InnatePowers = [
			InnatePower.For(typeof(LearnTheInvadersTactics)),
			InnatePower.For(typeof(ObserveTheEverChangingWorld))
		];

		SpecialRules = [LongAgesOfKnowledgeAndForgetfulness.Rule, InsightsIntoTheWorldsNature.Rule];
		Elements = new InsightsIntoTheWorldsNature(this);
		Forget = new LongAgesOfKnowledgeAndForgetfulness(this);
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 2 presence on your starting board in the highest-number land that is Sands or Mountain.
		var space = board.Spaces.Last(x => x.IsOneOf(Terrain.Sands, Terrain.Mountain)).ScopeSpace;
		space.Setup(Presence.Token, 2);

		// Prepare 1 moon, 1 air, and 1 earth marker. (++ allows us to use SMOA for testing, where =1 overwrites testing values)
		PreparedElementMgr.PreparedElements[Element.Moon]++;
		PreparedElementMgr.PreparedElements[Element.Air]++;
		PreparedElementMgr.PreparedElements[Element.Earth]++;
	}

	#region Elements

	/// <summary>
	/// Casts .Elements to PreparedElementMgr
	/// </summary>
	public PreparedElementMgr PreparedElementMgr => (PreparedElementMgr)Elements;
	CountDictionary<Element> IHaveSecondaryElements.SecondaryElements => PreparedElementMgr.PreparedElements;

	#endregion Elements

	protected override object? CustomMementoValue { 
		get => PreparedElementMgr.PreparedElements.ToArray();
		set { 
			if(value is KeyValuePair<Element,int>[] elDict ) 
				InitFromArray(PreparedElementMgr.PreparedElements, elDict ); 
		}
	}

}
