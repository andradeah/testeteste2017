package md5be85f49a2a54dc2575cc37e6e65d238c;


public class CSItemCombo
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_toString:()Ljava/lang/String;:GetToStringHandler\n" +
			"n_hashCode:()I:GetGetHashCodeHandler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.SystemFramework.CSItemCombo, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CSItemCombo.class, __md_methods);
	}


	public CSItemCombo () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CSItemCombo.class)
			mono.android.TypeManager.Activate ("AvanteSales.SystemFramework.CSItemCombo, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public java.lang.String toString ()
	{
		return n_toString ();
	}

	private native java.lang.String n_toString ();


	public int hashCode ()
	{
		return n_hashCode ();
	}

	private native int n_hashCode ();

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
