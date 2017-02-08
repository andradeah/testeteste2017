package md5be85f49a2a54dc2575cc37e6e65d238c;


public class CSListViewItem
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("AvanteSales.SystemFramework.CSListViewItem, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CSListViewItem.class, __md_methods);
	}


	public CSListViewItem () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CSListViewItem.class)
			mono.android.TypeManager.Activate ("AvanteSales.SystemFramework.CSListViewItem, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
