using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;
public class InnateLayout {

	public Rectangle Bounds;
	public Rectangle TitleBounds;			// holds the name of the innate power
	public Rectangle AttributeBounds;		// Draws Attributes box
	public Rectangle[] AttributeRows;
	public Rectangle[] AttributeLabelCells;
	public Rectangle[] AttributeValueCells;
	public WrappingText GeneralInstructions;
	public WrappingText_InnateOptions[] Options;

	readonly ImageSizeCalculator _imageSizeCalculator;


	#region constructor

	public InnateLayout(InnatePower power, int x, int y, int width, float textHeightMultiplier, Graphics graphics ) {
		textEmSize = width * textHeightMultiplier;

		rowHeight     = (int)(textEmSize * 2.1f);
		_imageSizeCalculator = new ImageSizeCalculator( 
			iconDimension:    (int)(textEmSize * 1.9f),
			elementDimension: (int)(textEmSize * 2.4f)
		);

		int margin = (int)(width * .02f); // 2% of width
		int workingWidth = width - margin * 2;

		// "All-Enveloping Green"
		TitleBounds = new Rectangle( x + margin, y + margin, workingWidth, (int)(workingWidth * .12f) ); // Height is 10% of width;				// output - titleRect

		// brown box
		AttributeBounds = new Rectangle( x + margin, TitleBounds.Bottom, workingWidth, (int)(workingWidth * .15f) );
		AttributeRows       = AttributeBounds.SplitVerticallyAt( 0.40f );
		AttributeLabelCells = AttributeRows[0].SplitHorizontally( 3 );
		AttributeValueCells = AttributeRows[1].SplitHorizontally( 3 );

		// Options
		var options = new List<WrappingText_InnateOptions>();
		int optionY = AttributeBounds.Bottom + (int)(rowHeight * 0.5f);

		// General instructions
		if(!string.IsNullOrEmpty( power.GeneralInstructions )) {
			GeneralInstructions = CalcGeneralInstructionsLayout( power.GeneralInstructions, AttributeBounds.Left, optionY, workingWidth, graphics );
			optionY = GeneralInstructions.Bounds.Bottom;
		}

		foreach( var innatePowerOption in power.DrawableOptions ) {
			var wrapInfo = CalcInnateOptionLayout( innatePowerOption, AttributeBounds.Left, optionY, workingWidth, graphics );
			options.Add( wrapInfo );
			optionY = wrapInfo.Bounds.Bottom + (int)(rowHeight*.5f);
		}
		Options = options.ToArray();
		Bounds = new Rectangle( x, y, width, optionY - y );

	}

	#endregion

	// !!! group these with the GameFont somehow
	public Font UsingRegularFont() => new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Regular, GraphicsUnit.Pixel );
	public Font UsingBoldFont()    => new Font( FontFamily.GenericSansSerif, textEmSize, FontStyle.Bold, GraphicsUnit.Pixel );

	#region private methods

	WrappingText CalcGeneralInstructionsLayout( string description, int left, int top, int width, Graphics graphics ) {

		var wrappingLayout = new WrappingLayout(
			topLeft: new Point( left, top ),
			rowSize: new Size( width, rowHeight ),
			_imageSizeCalculator,
			graphics
		);

		// Text
		using Font font = UsingRegularFont();
		wrappingLayout.MeasuringFont = font;
		var (tokens, regularTexts) = wrappingLayout.CalcWrappingString( description );

		return new WrappingText {
			tokens = tokens,
			regularTexts = regularTexts,
			Bounds = wrappingLayout.FinalizeBounds()
		};

	}

	WrappingText_InnateOptions CalcInnateOptionLayout( IDrawableInnateOption option, int left, int originalY, int width, Graphics graphics ) {

		// Elements Thresholds
		var wrappingLayout = new WrappingLayout( 
			topLeft: new Point(left, originalY),
			rowSize: new Size( width, rowHeight ),
			_imageSizeCalculator, 
			graphics
		) { Indent = rowHeight/2 };

		// Elements in bold
		using var boldFont = UsingBoldFont();
		wrappingLayout.MeasuringFont = boldFont; 
		var (elementTokens, elementText) = wrappingLayout.CalcWrappingString( option.ThresholdString );

		// margin
		wrappingLayout.Tab(2);

		// Text
		using var font = UsingRegularFont(); //
		wrappingLayout.MeasuringFont = font;
		var (tokens, regularTexts) = wrappingLayout.CalcWrappingString( option.Description );


		return new WrappingText_InnateOptions {
			InnateOption = option,
			tokens       = elementTokens.Union( tokens ).ToList(),
			boldTexts    = elementText,
			regularTexts = regularTexts,
			Bounds       = wrappingLayout.FinalizeBounds(),
		};

	}

	#endregion

	#region temp / private fields

	public readonly float textEmSize;
	readonly int rowHeight;

	#endregion

}

public class TokenPosition {
	public Img TokenImg;
	public Rectangle Rect;
}

public class TextPosition {
	public string Text;
	public RectangleF Bounds;
}

public class WrappingText {
	public List<TokenPosition> tokens = new List<TokenPosition>();

	public List<TextPosition> boldTexts = new List<TextPosition>();

	public List<TextPosition> regularTexts = new List<TextPosition>();

	public Rectangle Bounds;

}

public class WrappingText_InnateOptions : WrappingText {
	public IDrawableInnateOption InnateOption;
}


