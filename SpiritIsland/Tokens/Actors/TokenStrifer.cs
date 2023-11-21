namespace SpiritIsland;

public class TokenStrifer {
	public TokenStrifer(Spirit spirit, SourceSelector ss ) { 
		_spirit = spirit;
		_ss = ss;
	}

	public async Task DoN() {
		while(true) {
			var before = await _ss.GetSource( _spirit, "Add Strife", Present.Always );
			if(before==null) return;
			var after = await before.Space.Tokens.Add1StrifeTo( before.Token.AsHuman() );
			await NotifyAsync(before,after);
		}

	}

	#region Event / Callback

	public TokenStrifer Track( Action<SpaceToken,SpaceToken> strifed ) {
		_onStrifed.Add( (before,after)=>{ strifed(before,after); return Task.CompletedTask; } );
		return this;
	}

	public TokenStrifer Track( Func<SpaceToken,SpaceToken,Task> onMoved ) {
		_onStrifed.Add(onMoved);
		return this;
	}

	async Task NotifyAsync( SpaceToken before, SpaceToken after ) {
		foreach(var onStrifed in _onStrifed)
			await onStrifed( before, after );
	}

	List<Func<SpaceToken, SpaceToken, Task>> _onStrifed = new();

	#endregion


	readonly SourceSelector _ss;
	readonly Spirit _spirit;
}
