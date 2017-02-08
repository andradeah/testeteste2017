package md5e93e0e65cca7dbdc8c7c4a87fc493852;


public class Sincronizacao_ThreadBeginCarga
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
		mono.android.Runtime.register ("AvanteSales.Pro.Activities.Sincronizacao+ThreadBeginCarga, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Sincronizacao_ThreadBeginCarga.class, __md_methods);
	}


	public Sincronizacao_ThreadBeginCarga () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Sincronizacao_ThreadBeginCarga.class)
			mono.android.TypeManager.Activate ("AvanteSales.Pro.Activities.Sincronizacao+ThreadBeginCarga, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
