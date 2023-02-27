namespace SpiritIsland;

public interface IModifyAddingToken {
	void ModifyAdding( AddingTokenArgs args );
}

public interface IModifyAddingTokenAsync {
	Task ModifyAddingAsync( AddingTokenArgs args );
}
