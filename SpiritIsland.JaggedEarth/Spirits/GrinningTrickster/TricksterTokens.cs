namespace SpiritIsland.JaggedEarth;

// == Tokens ==
// * Runs things at max.  (Pusher,Gatherer)
// * Enables adding the extra strife. (this could pulled out)
public class TricksterTokens( Space src ) 
	: Space( src )
{

	public override TokenMover Gather( Spirit self ) 
		=> base.Gather( self ).RunAtMax( true );

	public override TokenMover Pusher( Spirit self, SourceSelector sourceSelector, DestinationSelector? dest = null ) 
		=> base.Pusher( self, sourceSelector, dest ).RunAtMax( true );

}