using System.Collections.Generic;
using System.Drawing;
using SpiritIsland.WinForms;

namespace SpiritIsland.WinForms;

public class InnateLayout {

	public Rectangle Bounds;
	public Rectangle TitleBounds;			// holds the name of the innate power
	public Rectangle AttributeBounds;		// Draws Attributes box
	public Rectangle[] AttributeRows;
	public Rectangle[] AttributeLabelCells;
	public Rectangle[] AttributeValueCells;
	public GeneralInstructions GeneralInstructions;

	#region constructor

	public InnateLayout( InnatePower power, int x, int y, int width, float textHeightMultiplier, VisibleButtonContainer buttonContainer ) {

		_textEmSize = width * textHeightMultiplier;

		_rowHeight                    = (int)(_textEmSize * 2.0f);
		_generalInstructionsRowHeight = (int)(_textEmSize * 1.5f);

		int margin = (int)(width * .02f); // 2% of width
		int workingWidth = width - margin * 2;

		// "All-Enveloping Green"
		TitleBounds = new Rectangle( x + margin, y + margin, workingWidth, (int)(workingWidth * .12f) ); // Height is 10% of width;				// output - titleRect

		// brown box
		AttributeBounds = new Rectangle( x + margin, TitleBounds.Bottom, workingWidth, (int)(workingWidth * .15f) );
		AttributeRows       = AttributeBounds.SplitVerticallyAt( 0.40f );
		AttributeLabelCells = AttributeRows[0].SplitHorizontally( 3 );
		AttributeValueCells = AttributeRows[1].SplitHorizontally( 3 );

		int optionY = AttributeBounds.Bottom + (int)(_rowHeight * 0.5f);

		// General instructions
		if(!string.IsNullOrEmpty( power.GeneralInstructions )) {
			GeneralInstructions = CalcGeneralInstructionsLayout( power.GeneralInstructions, AttributeBounds.Left, optionY, workingWidth );
			optionY = GeneralInstructions.Bounds.Bottom
				+ (int)(_rowHeight * .4f); // spacer between items
		}

		// Options
		foreach(IDrawableInnateOption innatePowerOption in power.DrawableOptions ) {
			var optionBtn = (InnateOptionsBtn)buttonContainer[innatePowerOption];

			// Update Position
			optionBtn.SetPosition( _textEmSize, rowSize: new Size( workingWidth, _rowHeight ), new Point( AttributeBounds.Left, optionY ) );

			// Next
			optionY = optionBtn.Bounds.Bottom 
				+ (int)(_rowHeight*.4f); // spacer between items
		}

		Bounds = new Rectangle( x, y, width, optionY - y );
		((InnateButton)buttonContainer[power]).Bounds= Bounds;
	}

	#endregion

	// !!! group these with the GameFont somehow
	public Font UsingBoldFont()    => new Font( FontFamily.GenericSansSerif, _textEmSize, FontStyle.Bold, GraphicsUnit.Pixel );

	#region private methods

	GeneralInstructions CalcGeneralInstructionsLayout( string description, int left, int top, int width) {
		return new GeneralInstructions( description, _textEmSize, rowSize: new Size( width, _generalInstructionsRowHeight ), topLeft: new Point( left, top ) );
	}

	#endregion

	#region temp / private fields

	public readonly float _textEmSize;
	readonly int _rowHeight;
	readonly int _generalInstructionsRowHeight;

	#endregion

}

public class TokenPosition {
	public Img TokenImg;
	public Rectangle Rect;
}

public class TextPosition {
	public TextPosition( string text, RectangleF bounds ) { Text = text; Bounds = bounds; }
	public readonly string Text;
	public readonly RectangleF Bounds;
}


