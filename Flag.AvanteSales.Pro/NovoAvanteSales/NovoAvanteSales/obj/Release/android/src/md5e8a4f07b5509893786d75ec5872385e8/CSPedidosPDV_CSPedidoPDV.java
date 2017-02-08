package md5e8a4f07b5509893786d75ec5872385e8;


public class CSPedidosPDV_CSPedidoPDV
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
		mono.android.Runtime.register ("AvanteSales.CSPedidosPDV+CSPedidoPDV, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CSPedidosPDV_CSPedidoPDV.class, __md_methods);
	}


	public CSPedidosPDV_CSPedidoPDV () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CSPedidosPDV_CSPedidoPDV.class)
			mono.android.TypeManager.Activate ("AvanteSales.CSPedidosPDV+CSPedidoPDV, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
