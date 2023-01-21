namespace SpiritIsland.FeatherAndFlame;

class ScotlandInvaderDeckBuilder : InvaderDeckBuilder {

	const int Coastal = 99;

	static int[] GetOrder( int level ) => level switch {
		2 or
		3 => new int[] { 1, 1, 2, 2, 1, Coastal, 2, 3, 3, 3, 3, 3 },
		4 or
		5 or
		6 => new int[] { 1, 1, 2, 2, 3, Coastal, 2, 3, 3, 3, 3 },
		_ => null
	};
	readonly int _level2Take;

	public ScotlandInvaderDeckBuilder( int level ):base( GetOrder( level ) ) {
		_level2Take = level<2 
			? 5 // all level 2 cards
			: 4;// skip Coastal
	}

	protected override IEnumerable<InvaderCard> SelectLevel2Cards()
		=> base.SelectLevel2Cards().Take( _level2Take );

	protected override void SelectCard( List<InvaderCard> dst, Queue<InvaderCard>[] src, int level ) {
		if( level == Coastal )
			dst.Add( InvaderCard.Stage2Costal() );
		else 
			base.SelectCard( dst, src, level );
	}

}
