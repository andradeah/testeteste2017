package md5e93e0e65cca7dbdc8c7c4a87fc493852;


public class Sincronizacao_ThreadControleAcesso
	extends android.os.AsyncTask
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_doInBackground:([Ljava/lang/Object;)Ljava/lang/Object;:GetDoInBackground_arrayLjava_lang_Object_Handler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.Pro.Activities.Sincronizacao+ThreadControleAcesso, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Sincronizacao_ThreadControleAcesso.class, __md_methods);
	}


	public Sincronizacao_ThreadControleAcesso () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Sincronizacao_ThreadControleAcesso.class)
			mono.android.TypeManager.Activate ("AvanteSales.Pro.Activities.Sincronizacao+ThreadControleAcesso, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public java.lang.Object doInBackground (java.lang.Object[] p0)
	{
		return n_doInBackground (p0);
	}

	private native java.lang.Object n_doInBackground (java.lang.Object[] p0);

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
