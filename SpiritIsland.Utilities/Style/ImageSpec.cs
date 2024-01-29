namespace SpiritIsland;

public abstract class ImageSpec {
	static public implicit operator ImageSpec(Img img) => new ImgImage(img);
	static public implicit operator ImageSpec(IconDescriptor icon) => new IconImage(icon);
	static public implicit operator ImageSpec(Func<Bitmap> img) => new FuncImage(img);
	static public implicit operator ImageSpec(Bitmap bitmap) => new BitmapImage(bitmap);

	public abstract ResourceMgr<Bitmap> GetResourceMgr();

	class ImgImage(Img img) : ImageSpec{
		public override ResourceMgr<Bitmap> GetResourceMgr() => new ResourceMgr<Bitmap>( ResourceImages.Singleton.GetImg(img), true );
	}
	class FuncImage(Func<Bitmap> generator) : ImageSpec{
		public override ResourceMgr<Bitmap> GetResourceMgr() => new ResourceMgr<Bitmap>( generator(), true );
	}
	class IconImage(IconDescriptor icon) : ImageSpec {
		public override ResourceMgr<Bitmap> GetResourceMgr() => new ResourceMgr<Bitmap>( ResourceImages.Singleton.GetTrackSlot( icon ), true);
	}

	class BitmapImage(Bitmap bitmap) : ImageSpec {
		public override ResourceMgr<Bitmap> GetResourceMgr() => new ResourceMgr<Bitmap>( bitmap, false);
	}

}
