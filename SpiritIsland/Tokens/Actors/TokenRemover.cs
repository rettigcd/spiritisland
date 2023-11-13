namespace SpiritIsland;

public sealed class TokenRemover {

	public TokenRemover( TargetSpaceCtx ctx ) {
		_self = ctx.Self;
		_sourceSelector = new SourceSelector( ctx.Tokens );
	}

	public TokenRemover( Spirit self, SourceSelector sourceSelector ) {
		_self = self;
		_sourceSelector = sourceSelector;
	}


	public TokenRemover AddGroup(int count,params IEntityClass[] groups ) {
		_sourceSelector.AddGroup( count, groups );
		return this;
	}

	public Task RemoveUpToN() => RemoveN( Present.Done );

	public async Task RemoveN( Present present = Present.Always ) {

		while(true) {
			var token = (await _sourceSelector.GetSource( _self, "Remove", present ));
			if(token == null) break;

			// Remove
			await token.Remove();

		}

	}

	#region private

	readonly Spirit _self;
	readonly SourceSelector _sourceSelector;

	#endregion

}