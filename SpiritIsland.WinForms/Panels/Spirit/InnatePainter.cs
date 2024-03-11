using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class InnatePainter {

	static readonly FontSpec InnateTitle = "Arial Narrow;.7;bold|italic";
	static readonly PenSpec   AttrBorder = "Black;.03";
	static readonly BrushSpec InnateBackground = Brushes.AliceBlue;

	/// <summary>
	/// Returns ALL Innates as a 2 or 3 column table.
	/// </summary>
	static public IPaintableRect GetAllInnatesClump( SharedCtx ctx, ClickableContainer cc ){ // Ctx, ClickContainer
		var colCountInfo = new ColumnCountInfo( ctx._spirit.InnatePowers.Length );
		return ArrangeInnates( [..ctx._spirit.InnatePowers.Select( 
			innate => GetInnateRect( ctx, innate, colCountInfo, cc )
		)]);
	}

	/// <summary>
	/// Places Innate Rects into a table.
	/// </summary>
	static HorizontalStackRect ArrangeInnates( IPaintableRect[] innateRects ){

		int columnCount = new ColumnCountInfo(innateRects.Length).columnCount;

		var columns = new List<IPaintableRect>[columnCount];
		for(int i=0;i<columnCount;++i) columns[i] = [];
		
		for(int i=0; i<innateRects.Length; ++i)
			columns[i%columnCount].Add(innateRects[i]);

		return new HorizontalStackRect( [..columns.Select(Wrap)] );

		static IPaintableRect Wrap(List<IPaintableRect> children ) => new RowRect(FillFrom.Top,[..children]).FloatSelf(3,3,94,94);
	}

	/// <summary>
	/// Generates the PaintableRect for 1 Innate
	/// </summary>
	static public RowRect GetInnateRect(SharedCtx ctx, InnatePower power, ColumnCountInfo ccInfo, ClickableContainer cc){

		var innateRect = new ClickableColRect([
			// Title 
			new TextRect( power.Title.ToUpper() ){ WidthRatio=12f, Padding=.1f, Horizontal=StringAlignment.Near, Font=InnateTitle },

			// Header
			new RowRect( FillFrom.Top,
				PowerHeaderDrawer.AttributeTitlesRow( power),
				PowerHeaderDrawer.AttributeValuesRow( power )
			){	Border= AttrBorder, WidthRatio = 7f, Margin = (.0f,.05f) },

			// General Instructions
			string.IsNullOrEmpty( power.GeneralInstructions )
				? new NullRect { WidthRatio = 100 }
				: new GeneralInstructions( power.GeneralInstructions, ccInfo.giRowSize ) { Padding = (0, .15f) },

			// Options
			..power.DrawableOptions.Select(option => new InnateTierBtn( ctx._spirit, option, ccInfo.optionRowSize, cc ) )
		]){ 
			Background = InnateBackground,
			Padding = .03f
		};

		innateRect.Clicked += () => ctx.SelectOption( power );
		cc.RegisterOption(power,innateRect);

		return innateRect;

	}

}