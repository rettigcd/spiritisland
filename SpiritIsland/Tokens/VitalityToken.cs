namespace SpiritIsland;

public class VitalityToken : TokenClassToken, IModifyAddingTokenAsync {
	public VitalityToken( string label, char k, Img img ) : base( label, k, img ) { }

	public async Task ModifyAddingAsync( AddingTokenArgs args ) {
		if( args.Token == Token.Blight		// adding blight
			&& args.To[Token.Blight]==0		// no blight yet
		) {
			args.Count--; // don't add the blight
			await args.To.RemoveAsync(Token.Vitality,1,RemoveReason.UsedUp);
		}
	}

}