package md5e93e0e65cca7dbdc8c7c4a87fc493852;


public class ProcuraProduto
	extends android.support.v7.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer,
		android.widget.TextView.OnEditorActionListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onOptionsItemSelected:(Landroid/view/MenuItem;)Z:GetOnOptionsItemSelected_Landroid_view_MenuItem_Handler\n" +
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onStart:()V:GetOnStartHandler\n" +
			"n_onSearchRequested:()Z:GetOnSearchRequestedHandler\n" +
			"n_onBackPressed:()V:GetOnBackPressedHandler\n" +
			"n_onEditorAction:(Landroid/widget/TextView;ILandroid/view/KeyEvent;)Z:GetOnEditorAction_Landroid_widget_TextView_ILandroid_view_KeyEvent_Handler:Android.Widget.TextView/IOnEditorActionListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("AvanteSales.Pro.Activities.ProcuraProduto, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ProcuraProduto.class, __md_methods);
	}


	public ProcuraProduto () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ProcuraProduto.class)
			mono.android.TypeManager.Activate ("AvanteSales.Pro.Activities.ProcuraProduto, AvanteSales.Pro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public boolean onOptionsItemSelected (android.view.MenuItem p0)
	{
		return n_onOptionsItemSelected (p0);
	}

	private native boolean n_onOptionsItemSelected (android.view.MenuItem p0);


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


	public boolean onSearchRequested ()
	{
		return n_onSearchRequested ();
	}

	private native boolean n_onSearchRequested ();


	public void onBackPressed ()
	{
		n_onBackPressed ();
	}

	private native void n_onBackPressed ();


	public boolean onEditorAction (android.widget.TextView p0, int p1, android.view.KeyEvent p2)
	{
		return n_onEditorAction (p0, p1, p2);
	}

	private native boolean n_onEditorAction (android.widget.TextView p0, int p1, android.view.KeyEvent p2);

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
