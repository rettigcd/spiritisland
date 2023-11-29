namespace SpiritIsland;


public class BlightTokenBinding : TokenBinding {

	public BlightTokenBinding( SpaceState tokens ) : base( tokens, Token.Blight ) { }

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public void Block() => _tokens.Init( new BlockBlightToken(), 1 );

	public override async Task AddAsync( int count, AddReason reason = AddReason.Added ) {
		await _tokens.AddAsync( Token.Blight, count, reason );
	}

}

class BlockBlightToken : IModifyAddingToken, IEndWhenTimePasses {
	public void ModifyAdding( AddingTokenArgs args ) {
		if(args.Token == Token.Blight)
			args.Count = 0;
	}
}
