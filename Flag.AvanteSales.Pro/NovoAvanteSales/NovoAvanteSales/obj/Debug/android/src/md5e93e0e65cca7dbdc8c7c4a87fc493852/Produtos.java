package md5e93e0e65cca7dbdc8c7c4a87fc493852;


public class Produtos
	extends android.support.v7.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\n" +
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onStart:()V:GetOnStartHandler\n" +
			"n_onCreateOptionsMenu:(Landroid/view/Menu;)Z:GetOnCreateOptionsMenu_Landroid_view_Menu_Handler\n" +
			"n_onPrepareOptionsMenu:(Landroid/view/Menu;)Z:GetOnPrepareOptionsMenu_Landroid_view_Menu_Handler\n" +
			"n_onOptionsItemSelected:(Landroid/view/MenuItem;)Z:GetOnOptionsItemSelected_Landroid_view_MenuItem_Handler\n" +
			"n_onBackPressed:()V:GetOnBackPressedHandler\n" +
			"n_onSearchRequested:()Z:GetOnSearchRequestedHandler\n" +
			"n_finish:()V:GetFinishHandler\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.Pro.Activities.Produtos, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Produtos.class, __md_methods);
	}


	public Produtos () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Produtos.class)
			mono.android.TypeManager.Activate ("AvanteSales.Pro.Activities.Produtos, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onActivityResult (int p0, int p1, android.content.Intent p2)
	{
		n_onActivityResult (p0, p1, p2);
	}

	private native void n_onActivityResult (int p0, int p1, android.content.Intent p2);


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onStart ()
	{
		n_onStart ();
	}

	private native void n_onStart ();


	public boolean onCreateOptionsMenu (android.view.Menu p0)
	{
		return n_onCreateOptionsMenu (p0);
	}

	private native boolean n_onCreateOptionsMenu (android.view.Menu p0);


	public boolean onPrepareOptionsMenu (android.view.Menu p0)
	{
		return n_onPrepareOptionsMenu (p0);
	}

	private native boolean n_onPrepareOptionsMenu (android.view.Menu p0);


	public boolean onOptionsItemSelected (android.view.MenuItem p0)
	{
		return n_onOptionsItemSelected (p0);
	}

	private native boolean n_onOptionsItemSelected (android.view.MenuItem p0);


	public void onBackPressed ()
	{
		n_onBackPressed ();
	}

	private native void n_onBackPressed ();


	public boolean onSearchRequested ()
	{
		return n_onSearchRequested ();
	}

	private native boolean n_onSearchRequested ();


	public void finish ()
	{
		n_finish ();
	}

	private native void n_finish ();

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
