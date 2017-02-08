package md5e8a4f07b5509893786d75ec5872385e8;


public class CSGruposProduto_CSGrupoProduto
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_hashCode:()I:GetGetHashCodeHandler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.CSGruposProduto+CSGrupoProduto, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CSGruposProduto_CSGrupoProduto.class, __md_methods);
	}


	public CSGruposProduto_CSGrupoProduto () throws java.lang.Throwable
	{
		super ();
		if (getClass () == CSGruposProduto_CSGrupoProduto.class)
			mono.android.TypeManager.Activate ("AvanteSales.CSGruposProduto+CSGrupoProduto, AvanteSales.SystemFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


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
