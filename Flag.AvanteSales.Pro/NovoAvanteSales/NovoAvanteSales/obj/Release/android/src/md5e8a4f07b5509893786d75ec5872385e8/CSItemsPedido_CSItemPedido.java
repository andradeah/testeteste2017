package md5e8a4f07b5509893786d75ec5872385e8;


public class CSItemsPedido_CSItemPedido
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_toString:()Ljava/lang/String;:GetToStringHandler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.CSItemsPedido+CSItemPedido, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CSItemsPedido_CSItemPedido.class, __md_methods);
	}


	public CSItemsPedido_CSItemPedido () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CSItemsPedido_CSItemPedido.class)
			mono.android.TypeManager.Activate ("AvanteSales.CSItemsPedido+CSItemPedido, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public java.lang.String toString ()
	{
		return n_toString ();
	}

	private native java.lang.String n_toString ();

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
