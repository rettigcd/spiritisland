namespace SpiritIsland.FeatherAndFlame;

class ScotlandInvaderDeckBuilder : InvaderDeckBuilder {

	public const int Coastal = 99;

	public ScotlandInvaderDeckBuilder( params int[] levelSelection ):base( levelSelection ) {}

	protected override IEnumerable<InvaderCard> SelectLevel2Cards()
		=> base.SelectLevel2Cards().Where(x=>x.Text != CoastalFilter.Name );

	protected override void SelectCard( List<InvaderCard> dst, Queue<InvaderCard>[] src, int level ) {
		if( level == Coastal )
			dst.Add( InvaderCard.Stage2Costal() );
		else 
			base.SelectCard( dst, src, level );
	}

}
