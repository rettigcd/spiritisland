using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class FearRect {

	static public IPaintableRect GetFearRect( GameState gameState ){

		NullRect spacer = new NullRect{ WidthRatio = .1f };
		return new RowRect( FillFrom.Right,
			GetTerrorLevelRect( gameState.Fear.TerrorLevel ),
			GetActivatedFearRect(gameState.Fear.ActivatedCards),
			spacer,
			GetFutureFearRect( gameState.Fear ),
			GetPoolRect(gameState.Fear.PoolMax,gameState.Fear.EarnedFear)
		);
	}

	static PoolRect GetTerrorLevelRect(int terrorLevel){
		var img = terrorLevel switch{ 1=>Img.TerrorLevel1, 2=>Img.TerrorLevel2, _ => Img.TerrorLevel3 };
		return new PoolRect{ WidthRatio = .8f }
			.Float( new ImgRect(img));
	}

	static PoolRect GetPoolRect(int poolMax, int fearCount){
		PoolRect pool = new PoolRect{ WidthRatio=2f };

		const float iconReductionFactor = .75f; // use 1.0f for full icon size
		float iconWidth = 1/pool.WidthRatio.Value*iconReductionFactor; // this is necessary to make slots apear square

		float step = (1f-iconWidth) // exclude the width of 1 icon which we will show in full 
			/(poolMax-1); // remove the 1 we are showing in full from the count.

		// Draw un-earned Fear, 1st and descending, so it will be underneath earned and next slot to earn will be on top.
		for(int i = poolMax-1; fearCount <= i; --i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Gray_Fear), x,0,iconWidth,1f);
		}
		// Draw Earned Fear, 2nd and ascending, so last-earned fear will be on top.
		for(int i = 0; i < fearCount; ++i){
			float x = step*i;
			pool.Float(new ImgRect(Img.Fear), x,0,iconWidth,1f);
		}

		return pool;
	}

	static PoolRect GetFutureFearRect( Fear fear ){
		if(fear.Deck.Count == 0)
			return new PoolRect(); // empty
		float cardWidth = 5f/7f;
		float numWidth = 1f-cardWidth;
		var pool = new PoolRect(){ WidthRatio = 1f};

		(string fillColor,float y) = fear.TerrorLevel switch {
			1 => ("#EFC018",.00f),
			2 =>  ("#C62B00",.33f),
			_ =>  ("#7B0000",.66f)
		};
		pool.Float( new RectRect(){ Fill = fillColor, WidthRatio = .7f }, 0f, y, numWidth,.3f);

		 int[] remaining = fear.CardsPerLevelRemaining;
		pool
			.Float( new FearCardRect(fear.Deck.Peek(),0),numWidth,0f,cardWidth,1f )
			.Float( new TextRect(remaining[0]), 0f, .03f, numWidth,.3f )
			.Float( new TextRect(remaining[1]), 0f, .36f, numWidth,.3f )
			.Float( new TextRect(remaining[2]), 0f, .69f, numWidth,.3f );
		return pool;
	}

	static IPaintableRect GetActivatedFearRect( Stack<IFearCard> activated ){
		return (0 < activated.Count)
			? new FearCardRect(activated.Peek(),activated.Count)
			: new PoolRect{WidthRatio = 5f/7f }
				.Float(new RectRect{ Fill = EmptySlotBrush, WidthRatio = 5f/7f })
				.Float(new TextRect("Activated"){ Brush = EmptySlotTextBrush},0,0.25f,1,.25f)
				.Float(new TextRect("Fear"){ Brush = EmptySlotTextBrush},0,.55f,1,.25f);
	}

	static Brush EmptySlotBrush => Brushes.DarkGray;
	static Brush EmptySlotTextBrush => Brushes.Gray;

}
