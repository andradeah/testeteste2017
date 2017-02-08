package md5c792c221571b27782b48474fc0e8d40f;


public class GrupoProduto_ThreadGrupoProduto
	extends android.os.AsyncTask
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_doInBackground:([Ljava/lang/Object;)Ljava/lang/Object;:GetDoInBackground_arrayLjava_lang_Object_Handler\n" +
			"n_onPostExecute:(Ljava/lang/Object;)V:GetOnPostExecute_Ljava_lang_Object_Handler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.Pro.Fragments.GrupoProduto+ThreadGrupoProduto, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", GrupoProduto_ThreadGrupoProduto.class, __md_methods);
	}


	public GrupoProduto_ThreadGrupoProduto () throws java.lang.Throwable
	{
		super ();
		if (getClass () == GrupoProduto_ThreadGrupoProduto.class)
			mono.android.TypeManager.Activate ("AvanteSales.Pro.Fragments.GrupoProduto+ThreadGrupoProduto, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public java.lang.Object doInBackground (java.lang.Object[] p0)
	{
		return n_doInBackground (p0);
	}

	private native java.lang.Object n_doInBackground (java.lang.Object[] p0);


	public void onPostExecute (java.lang.Object p0)
	{
		n_onPostExecute (p0);
	}

	private native void n_onPostExecute (java.lang.Object p0);

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
