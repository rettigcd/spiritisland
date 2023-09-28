using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class TokenImageProvider {
	public TokenImageProvider( ResourceImages images) {
		_tokenImages = new Dictionary<ISpaceEntity, Image?> {
			[Token.Blight] = images.GetImage( Img.Blight ),
			[Token.Beast] = images.GetImage( Img.Beast ),
			[Token.Wilds] = images.GetImage( Img.Wilds ),
			// [Token.Disease] = images.GetImage( Img.Disease ),
			[Token.Badlands] = images.GetImage( Img.Badlands ),
			[Token.Vitality] = images.GetImage( Img.Vitality ),
			// assign slot so we can access via key when we need to ?.Dispose() when initializing spirit
			[Token.Defend] = null,
			[Token.Isolate] = null,
		};
		_strife = images.Strife();
		_images = images;

		_fearTokenImage = images.Fear();
		_grayFear = images.FearGray();

	}

	public Dictionary<ISpaceEntity, Image?> _tokenImages; // because we need different images for different damaged invaders.
	public Image _presenceImg;
	public Image _strife;
	public Image _fearTokenImage;
	public Image _grayFear;


	public void InitNewSpirit( PresenceTokenAppearance presenceAppearance ) {
		DisposeOldSpirit();

		_presenceImg = ResourceImages.Singleton.GetPresenceImage( presenceAppearance.BaseImage );
		_tokenImages[Token.Defend] = ResourceImages.Singleton.GetImage( Img.Defend );
		_tokenImages[Token.Isolate] = ResourceImages.Singleton.GetImage( Img.Isolate );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_presenceImg );
		presenceAppearance.Adjustment?.Adjust( (Bitmap?)_tokenImages[Token.Defend] );
		presenceAppearance.Adjustment?.Adjust( (Bitmap?)_tokenImages[Token.Isolate] );
	}

	public Image AccessTokenImage( IToken imageToken ) {
		if( imageToken is SpiritPresenceToken )
			return _presenceImg;

		if(!_tokenImages.ContainsKey( imageToken ))
			_tokenImages[imageToken] = ResourceImages.Singleton.GetTokenImage( imageToken );
		return _tokenImages[imageToken];
	}

	public Image GetElementImage( Element element ) {
		if(!_elementImages.ContainsKey( element )) {
			Image image = _images.GetImage( element.GetTokenImg() );
			_elementImages.Add( element, image );
		}
		return _elementImages[element];
	}

	readonly Dictionary<Element, Image> _elementImages = new();
	readonly ResourceImages _images;


	void DisposeOldSpirit() {
		_presenceImg?.Dispose();
		_tokenImages[Token.Defend]?.Dispose();
		_tokenImages[Token.Isolate]?.Dispose();
	}
}
