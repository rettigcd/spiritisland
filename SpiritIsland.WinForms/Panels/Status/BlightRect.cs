using System;

namespace SpiritIsland.WinForms;

public class BlightRect{

	static public IPaintableRect GetBlightRect( GameState gameState ){

		var card = gameState.BlightCard;
		int count = gameState.Tokens[BlightCard.Space].Blight.Count;

		// Determine # of blight / player
		int approximateMaxBlightPerPlayer = 2 + 1; // Let's say 2 on the card + 1 on the board
		if(card.CardFlipped) 
			approximateMaxBlightPerPlayer += card.Side2BlightPerPlayer;

		int maxSpaces = approximateMaxBlightPerPlayer * gameState.Spirits.Length + 1;

		return new RowRect( Align.Far,
			new BlightCardRect( gameState.BlightCard ),
			BlightPoolRect( maxSpaces, count )
		);
	}

	static IPaintableRect BlightPoolRect(int poolMax, int blightCount){
		if(poolMax < blightCount) poolMax = blightCount;

		PoolRect pool = new PoolRect{ WidthRatio=1.5f };

		const float iconReductionFactor = .65f; // use 1.0f for full icon size
		float iconWidth = 1/pool.WidthRatio.Value * iconReductionFactor; // this is necessary to make slots apear square

		float step = (1f-iconWidth) // exclude the width of 1 icon which we will show in full 
			/(poolMax-1); // remove the 1 we are showing in full from the count.

		// Draw Earned Fear, 2nd and ascending, so last-earned fear will be on top.
		for(int i = 0; i < blightCount; ++i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Blight), x,0f,iconWidth,1f);
		}

		return pool;
	}


}
