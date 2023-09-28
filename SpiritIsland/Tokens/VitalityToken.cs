namespace SpiritIsland;

public class VitalityToken : TokenClassToken,IModifyAddingToken {
	public VitalityToken( string label, char k, Img img ) : base( label, k, img ) { }
	public UsageCost Cost => UsageCost.Something; // we do lose the token

	public void ModifyAdding( AddingTokenArgs args ) {
		if( args.Token == Token.Blight		// adding blight
			&& args.To[Token.Blight]==0		// no blight yet
		) {
			args.Count--; // don't add the blight
			args.To.Adjust(Token.Vitality,-1); // remove 1 vitality token
		}
	}

}