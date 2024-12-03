namespace SpiritIsland;


public class BlightTokenBinding( Space space ) : TokenBinding( space, Token.Blight ) {

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public void Block() => _space.Init( new BlockBlightToken(), 1 );

	public override async Task AddAsync( int count, AddReason reason = AddReason.Added ) {
		await _space.AddAsync( Token.Blight, count, reason );
	}

}

class BlockBlightToken : IModifyAddingToken, IEndWhenTimePasses {
	public Task ModifyAddingAsync( AddingTokenArgs args ) {
		if(args.Token == Token.Blight)
			args.Count = 0;
		return Task.CompletedTask;
	}
}
