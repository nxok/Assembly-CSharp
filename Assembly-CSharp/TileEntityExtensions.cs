using System;

public static class TileEntityExtensions
{
	public static T GetSelfOrFeature<T>(this ITileEntity _te) where T : class
	{
		T t;
		_te.TryGetSelfOrFeature(out t);
		return t;
	}

	public static bool TryGetSelfOrFeature<T>(this ITileEntity _te, out T _typedTe) where T : class
	{
		if (_te == null)
		{
			_typedTe = default(T);
			return false;
		}
		T t = _te as T;
		if (t != null)
		{
			_typedTe = t;
			return true;
		}
		TileEntityComposite tileEntityComposite = _te as TileEntityComposite;
		if (tileEntityComposite != null)
		{
			_typedTe = tileEntityComposite.GetFeature<T>();
			return _typedTe != null;
		}
		ITileEntityFeature tileEntityFeature = _te as ITileEntityFeature;
		if (tileEntityFeature != null)
		{
			_typedTe = tileEntityFeature.Parent.GetFeature<T>();
			return _typedTe != null;
		}
		_typedTe = default(T);
		return false;
	}
}
