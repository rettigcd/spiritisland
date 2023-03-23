using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class ImageSizeCalculator {

	public int IconDimension { get; set; }
	public int ElementDimension { get; set; }

	public ImageSizeCalculator(int iconDimension, int elementDimension ) {
		IconDimension = iconDimension;
		ElementDimension = elementDimension;
	}

	public (Size,Img) GetTokenDetails( string tokenName ) {
		var img = SimpleWordToIcon( tokenName );
		var size = "sun|moon|air|fire|water|plant|animal|earth".Contains( tokenName )
			? new Size( ElementDimension, ElementDimension )          // elements get special size
			: CalcIconSize( img, IconDimension ); // non-elements must fit inside iconDimension
		return (size,img);
	}


	Size CalcIconSize( Img img, int maxDimension ) {
		if(!iconSizes.ContainsKey( img )) {
			using Image image = ResourceImages.Singleton.GetImage( img );
			iconSizes.Add( img, image.Size );
		}
		var sz = iconSizes[img];

		return sz.Width < sz.Height
			? new Size( maxDimension * sz.Width / sz.Height, maxDimension )
			: new Size( maxDimension, maxDimension * sz.Height / sz.Width );
	}

	static Img SimpleWordToIcon( string token ) {
		return token switch {
			"dahan" => Img.Icon_Dahan,
			"sacred site" => Img.Icon_Sacredsite,
			"city" => Img.Icon_City,
			"town" => Img.Icon_Town,
			"explorer" => Img.Icon_Explorer,
			"blight" => Img.Icon_Blight,
			"beast" => Img.Icon_Beast,
			"fear" => Img.Icon_Fear,
			"wilds" => Img.Icon_Wilds,
			"fast" => Img.Icon_Fast,
			"presence" => Img.Icon_Presence,
			"slow" => Img.Icon_Slow,
			"disease" => Img.Icon_Disease,
			"strife" => Img.Icon_Strife,
			"badlands" => Img.Icon_Badlands,
			"destroyedpresence" => Img.Icon_DestroyedPresence,
			_ => ElementCounts.ParseEl( token ).GetIconImg(),
		};
	}

	readonly Dictionary<Img, Size> iconSizes = new Dictionary<Img, Size>();

}


