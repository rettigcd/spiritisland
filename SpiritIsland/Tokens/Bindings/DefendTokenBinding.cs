namespace SpiritIsland;

public class DefendTokenBinding( Space _space ) : IDefendTokenBinding {

	public int Count => _space[Token.Defend];

	public void Add( int count ) {
		_space.Adjust( Token.Defend, count ); // this should NOT trigger NORMAL token-added event

		// Keep SYNC - if Async needed, use End-Of-Action
		// only thing that hooks into this is Intensity Aspect
		foreach(var mod in _space.OfType<IHandleSpaceDefended>().ToArray() )
			mod.OnDefend(_space,count);
	}

	public void Clear() => _space.Init( Token.Defend, 0 ); // DO NOT Trigger token events, not real token

}