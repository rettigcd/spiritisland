using System.Reflection.Metadata.Ecma335;

namespace SpiritIsland;

public abstract class ImageSpec {
	static public implicit operator ImageSpec(Img img) => new ImgImage(img);
	static public implicit operator ImageSpec(IconDescriptor icon) => new IconImage(icon);
	static public implicit operator ImageSpec(Bitmap bitmap) => new BitmapImage(bitmap);
	static public implicit operator ImageSpec(Func<ResourceMgr<Bitmap>> func) => new ResourceMgrImage(func);
	static public implicit operator ImageSpec(Func<Bitmap> img) => new FuncImage(img); // !!! this seems dangerous since we don't know disposal histroy

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
	class ResourceMgrImage( Func<ResourceMgr<Bitmap>> func ) : ImageSpec {
		public override ResourceMgr<Bitmap> GetResourceMgr() => func();
	}

}
