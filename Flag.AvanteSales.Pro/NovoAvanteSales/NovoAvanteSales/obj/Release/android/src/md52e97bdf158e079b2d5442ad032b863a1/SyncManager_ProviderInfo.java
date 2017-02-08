package md52e97bdf158e079b2d5442ad032b863a1;


public class SyncManager_ProviderInfo
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
		mono.android.Runtime.register ("Master.CompactFramework.Sync.SyncManager+ProviderInfo, AvanteSales.SyncManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SyncManager_ProviderInfo.class, __md_methods);
	}


	public SyncManager_ProviderInfo () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SyncManager_ProviderInfo.class)
			mono.android.TypeManager.Activate ("Master.CompactFramework.Sync.SyncManager+ProviderInfo, AvanteSales.SyncManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
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
