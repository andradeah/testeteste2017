#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Collections;

#endregion

namespace AvanteSales.SystemFramework
{
    /// <summary>
    /// Summary description for CSRegistryKey.
    /// </summary>
    // This code was developed by Elia Karagiannis of LeadingEdgeSolutions L.L.C.
    // Feel free to use this code however you may please but keep these comments as part of the code.
    // Thanks go out to Chris Tacke (with help from Neil Cowburn)

    ///	<summary>		
    ///	Represents a key level node	in the Windows registry. This class	is a registry encapsulation.		
    ///	</summary>		
    public sealed class CSRegistryKey : IDisposable
    {
        private bool m_bReadOnly = true;
        private uint m_uiHKey = 0;
        private string m_sName = null;

        ///	<summary>			
        ///	This method	should only	be called by the Registry class	to set up the root keys!			
        ///	</summary>			
        ///	<param name="rootKey"></param>			
        internal CSRegistryKey(uint rootKey, string rootKeyName)
        {
            m_uiHKey = CSRegistryImpl.OpenKey(rootKey, "");
            m_bReadOnly = true;
            m_sName = rootKeyName;
        }

        internal CSRegistryKey(uint rootKey, string rootKeyName, string subkey)
        {
            CSRegistryImpl.KeyDisposition disp = 0;
            m_uiHKey = CSRegistryImpl.CreateKey(rootKey, subkey, null, ref disp);
            m_bReadOnly = false;
            m_sName = rootKeyName + "\\" + subkey;
        }

        ///	<summary>			
        ///	Closes the key and flushes it to disk if the contents have been	modified.			
        ///	</summary>		
        ~CSRegistryKey()
        {
            Dispose();
        }

        ///	<summary>			
        ///	Closes the key and flushes it to disk if the contents have been	modified.			
        ///	</summary>			
        public void Close()
        {
            if (this.m_uiHKey != 0)
            {
                CSRegistryImpl.RegCloseKey(m_uiHKey);
                m_uiHKey = 0;
            }
        }

        ///	<summary>			
        ///	Creates	a new subkey or	opens an existing subkey. The string subKey	is not case-sensitive.				
        ///	</summary>			
        ///	<param name="subkey">Name or path of subkey	to create or open. </param>			
        ///	<returns>Returns the subkey, or	null if	the	operation failed.</returns>			
        public CSRegistryKey CreateSubKey(string subkey)
        {
            AssertIsOpen();
            return new CSRegistryKey(this.m_uiHKey, this.Name, subkey);
        }

        ///	<summary>			
        ///	Deletes	the	specified subkey. The string subkey	is not case-sensitive.			
        ///	</summary>			
        ///	<param name="subkey">Name of the subkey	to delete.</param>			
        ///	<param name="throwOnMissing">Indicates whether an exception	should be raised			
        ///	if the specified subkey	cannot be found. If	this argument is true and the			
        ///	specified subkey does not exist	then an	exception is raised. If	this argument			
        ///	is false and the specified subkey does not exist, then no action is	taken			
        ///	</param>			
        public void DeleteSubKey(string subkey, bool throwOnMissing)
        {
            AssertIsOpen();
            // make	sure we	are	not	read only				
            if (m_bReadOnly)
                throw new UnauthorizedAccessException("The subkey cannot be	deleted.  The registry key was opened as read only");
            // do the check	to see if the key actually exists				
            uint retKey = 0;
            int retVal = CSRegistryImpl.RegOpenKeyEx(this.m_uiHKey, subkey, 0, 0, ref	retKey);
            if (retVal != 0)
            {
                if (throwOnMissing)
                    throw new Exception("The subkey	" + subkey + " cannot be deleted because it	is missing.");
                return;
            }

            // check and see if	the	subkey to delete has any subkeys of	its	own				
            char[] chNull = null;
            int iNull = 0;
            int cSubKey = 1;
            CSRegistryImpl.RegQueryInfoKey(retKey, chNull, ref iNull,
                                            0, ref cSubKey, ref	iNull, ref iNull, ref iNull,
                                            ref	iNull, ref iNull, 0, 0);
            if (cSubKey != 0)
            {
                CSRegistryImpl.RegCloseKey(retKey);
                throw new InvalidOperationException("The specified subkey has child	keys");
            }

            // we are all good,	delete the key				
            CSRegistryImpl.RegDeleteKey(this.m_uiHKey, subkey);
        }

        ///	<summary>			
        ///	Deletes	the	specified subkey. The string subkey	is not case-sensitive.			
        ///	</summary>			
        ///	<param name="subkey">Name of the subkey	to delete.</param>			
        public void DeleteSubKey(string subkey)
        {
            // just	call the overloaded	function				
            DeleteSubKey(subkey, false);
        }

        ///	<summary>			
        ///	Deletes	a subkey and any child subkeys recursively.	The	string subKey is not case-sensitive.			
        ///	</summary>			
        ///	<param name="subkey">Subkey	to delete</param>			
        public void DeleteSubKeyTree(string subkey)
        {
            AssertIsOpen();
            // make	sure we	are	not	read only				
            if (m_bReadOnly)
                throw new UnauthorizedAccessException("The subkey cannot be	deleted.  The registry key was opened as read only");

            // delete the key				
            CSRegistryImpl.RegDeleteKey(this.m_uiHKey, subkey);
        }

        ///	<summary>			
        ///	Deletes	the	specified value	from this key. The string subKey is	not	case sensitive.			
        ///	</summary>			
        ///	<param name="val">Name of the value	to delete.</param>			
        ///	<param name="throwOnMissing">Indicates whether an exception	should be raised			
        ///	if the specified value cannot be found.	If this	argument is	true and the specified			
        ///	value does not exist then an exception is raised. If this argument is false	and				
        ///	the	specified value	does not exist,	then no	action is taken</param>			
        public void DeleteValue(string val, bool throwOnMissing)
        {
            AssertIsOpen();
            // make	sure we	are	not	read only.				
            if (m_bReadOnly)
                throw new UnauthorizedAccessException("The value cannot	be deleted.	 The registry key was opened as	read only");

            if (throwOnMissing)
            {
                // do the check	to see if the key actually exists					
                int type = 0;
                int size = 0;
                byte[] data = null;
                if (CSRegistryImpl.RegQueryValueEx(this.m_uiHKey, val, 0, ref type, data, ref size) != 0)
                    throw new Exception("The value " + val + " cannot be deleted because it	is missing.");
            }

            CSRegistryImpl.RegDeleteValue(this.m_uiHKey, val);
        }

        ///	<summary>
        ///	Deletes	the	specified value	from this key. The string subKey is	not	case sensitive.			
        ///	</summary>			
        ///	<param name="val">Name of the value	to delete..</param>			
        public void DeleteValue(string val)
        {
            // just	call the overloaded	function
            DeleteValue(val, false);
        }

        ///	<summary>			
        ///	Retrieves the count	of subkeys at the base level, for the current key.			
        ///	</summary>
        public int SubKeyCount
        {
            get
            {
                AssertIsOpen();
                int Subkeys = 0;
                int nullStuff = 0;
                char[] test = null;
                // query the registry key for the number of	subkeys
                int retVal = CSRegistryImpl.RegQueryInfoKey(this.m_uiHKey, test, ref nullStuff, 0,
                                                            ref Subkeys, ref nullStuff, ref nullStuff,
                                                            ref	nullStuff, ref nullStuff, ref nullStuff, 0, 0);
                return Subkeys;
            }
        }

        ///	<summary>			
        ///	Retrieves an array of strings that contains	all	the	subkey names.			
        ///	</summary>			
        ///	<returns>An	array of strings that contains the names of	the	subkeys	for	the	current	key.</returns>			
        public string[] GetSubKeyNames()
        {
            AssertIsOpen();
            // create an arraylist to temporarily hold our subkey names				
            ArrayList subKeyNames = new ArrayList();
            int nullStuff = 0;
            int iMaxSubKeyLen = 255;
            char[] chNull = null;
            int iSubKeyLen = 0;
            char[] chSubKeyName = new char[iMaxSubKeyLen];
            for (int x = 0; x < this.SubKeyCount; x++)
            {
                // call	RegEnumKeyEx for as	many subkeys there are					
                iSubKeyLen = iMaxSubKeyLen;
                CSRegistryImpl.RegEnumKeyEx(this.m_uiHKey, x, chSubKeyName, ref iSubKeyLen, 0, chNull, ref nullStuff, 0);
                subKeyNames.Add(new string(chSubKeyName, 0, iSubKeyLen));
            }
            // return the array	as a string	Array				
            return (string[])subKeyNames.ToArray(typeof(string));
        }

        ///	<summary>			
        ///	Retrieves the count	of values in the key.			
        ///	</summary>			
        public int ValueCount
        {
            get
            {
                AssertIsOpen();
                int icValueKeys = 0;
                int nullStuff = 0;
                char[] test = null;
                // query the registry for the number of	values in the current key.
                int retVal = CSRegistryImpl.RegQueryInfoKey(this.m_uiHKey, test, ref nullStuff,
                                                            0, ref nullStuff,
                                                            ref	nullStuff, ref nullStuff,
                                                            ref	icValueKeys, ref nullStuff,
                                                            ref	nullStuff, 0, 0);
                return icValueKeys;
            }
        }

        ///	<summary>			
        ///	Retrieves an array of strings that contains	all	the	value names	associated with	this key.			
        ///	</summary>			
        ///	<returns>An	array of strings that contains the value names for the current key.</returns>
        public string[] GetValueNames()
        {
            AssertIsOpen();
            // array list to temporarily hold the names	of the values				
            ArrayList valueNames = new ArrayList();
            int iMaxValueNameLen = 255;
            byte[] byData = null;
            int iDataLen = 0;
            int iType = 0;
            int iValueNameLen = 0;
            char[] chValueName = new char[iMaxValueNameLen];
            for (int x = 0; x < this.ValueCount; x++)
            {
                // call	RegEnumKeyEx for all the values	in the
                iValueNameLen = iMaxValueNameLen;
                CSRegistryImpl.RegEnumValue(this.m_uiHKey, x, chValueName, ref iValueNameLen, 0, ref iType, ref byData, ref iDataLen);
                valueNames.Add(new string(chValueName, 0, iValueNameLen));
            }
            return (string[])valueNames.ToArray(typeof(string));
        }

        ///	<summary>			
        ///	Retrieves the specified	value, or the default value	you	provide	if the specified value is not found.			
        ///	</summary>			
        ///	<param name="val">Name of the value	to retrieve.</param>			
        ///	<param name="initialValue">Value to	return if name does	not	exist.</param>			
        ///	<returns>The data associated with name,	or defaultValue	if name	is not found.</returns>			
        public object GetValue(string val, object initialValue)
        {
            AssertIsOpen();
            int iType = 0;
            int icData = 0;
            byte[] byData = null;
            string sz = "";

            // read	in the value information, if no	value exists, just return the initial value	specified				
            if (CSRegistryImpl.RegQueryValueEx(this.m_uiHKey, val, 0, ref iType, byData, ref icData) != 0)
                return initialValue;

            byData = new byte[icData];

            if (CSRegistryImpl.RegQueryValueEx(this.m_uiHKey, val, 0, ref iType, byData, ref icData) != 0)
                return initialValue;

            // Switch the type we have,	and	convert	to the proper type				
            switch (iType)
            {
                case CSRegistryImpl.REG_BINARY:
                    return byData;
                case CSRegistryImpl.REG_DWORD:
                    return System.BitConverter.ToInt32(byData, 0);
                case CSRegistryImpl.REG_SZ:
                    sz = System.Text.UnicodeEncoding.Unicode.GetString(byData, 0, icData);
                    return sz.Substring(0, sz.IndexOf("\0"));
                case CSRegistryImpl.REG_EXPAND_SZ:
                    sz = System.Text.UnicodeEncoding.Unicode.GetString(byData, 0, icData);
                    return sz.Substring(0, sz.IndexOf("\0"));
                default:
                    throw new Exception("Unknown registry type!");
            }
        }

        ///	<summary>			
        ///	Retrieves the data associated with the specified value,	or null	if the value does not exist.			
        ///	</summary>			
        ///	<param name="val">Name of the value	to retrieve.</param>			
        ///	<returns>The data associated with name , or	null if	the	value does not exist.</returns>			
        public object GetValue(string val)
        {
            // just	call the overloaded	function				
            return GetValue(val, null);
        }

        ///	<summary>			
        ///	Sets the specified value. The string subKey	is not case	sensitive.			
        ///	</summary>			
        ///	<param name="valName">Name of value	to store data in.</param>			
        ///	<param name="valData">Data to store.</param>			
        public void SetValue(string valName, object valData)
        {
            AssertIsOpen();
            // find	out	what type of data we are setting, and call the appropriate RegSetValueEx function				
            if (valData is string)
            {
                byte[] byValData = System.Text.Encoding.Unicode.GetBytes(valData + "\0");
                CSRegistryImpl.RegSetValueEx(this.m_uiHKey, valName, 0, CSRegistryImpl.REG_SZ, byValData, byValData.GetLength(0));
            }
            else if (valData is byte[])
            {
                CSRegistryImpl.RegSetValueEx(this.m_uiHKey, valName, 0, CSRegistryImpl.REG_BINARY, (byte[])valData, ((byte[])valData).GetLength(0));
            }
            else if (valData is int)
            {
                byte[] byValData = System.BitConverter.GetBytes((int)valData);
                CSRegistryImpl.RegSetValueEx(this.m_uiHKey, valName, 0, CSRegistryImpl.REG_DWORD, byValData, byValData.GetLength(0));
            }
            else
            {
                throw new Exception("Unknown type to set!");
            }
        }

        ///	<summary>			
        ///	Retrieves a	specified subkey.			
        ///	</summary>			
        ///	<param name="subkey">Name or path of subkey	to open.</param>			
        ///	<param name="writeable">Set	to true	if you need	write access to	the	key.</param>			
        ///	<returns>The subkey	requested, or null if the operation	failed.</returns>			
        public CSRegistryKey OpenSubKey(string subkey, bool writeable)
        {
            AssertIsOpen();
            uint uiSubHKey = CSRegistryImpl.OpenKey(m_uiHKey, subkey);
            CSRegistryKey rk = new CSRegistryKey(uiSubHKey, subkey);
            rk.m_bReadOnly = !writeable;
            return rk;
        }

        ///	<summary>			
        ///	Retrieves a	subkey as read-only.			
        ///	</summary>			
        ///	<param name="subkey">Name or path of subkey	to open.</param>			
        ///	<returns>The subkey	requested, or null if the operation	failed.</returns>			
        public CSRegistryKey OpenSubKey(string subkey)
        {
            // just	call the overloaded	function				
            return OpenSubKey(subkey, false);
        }

        ///	<summary>			
        ///	Retrieves the name of the key.			
        ///	</summary>			
        public string Name
        {
            get
            {
                return m_sName;
            }
        }

        #region	Implementation of IDisposable

        public void Dispose()
        {
            this.Close();
        }

        #endregion

        private void AssertIsOpen()
        {
            if (this.m_uiHKey == 0)
                throw new System.IO.IOException("The RegistryKey on	which this method is being invoked is closed");
        }

        ///	<summary>			
        ///	Retrieves a	string representation of this key.				
        ///	</summary>			
        ///	<returns>A string representing the key.	If the specified key is	invalid	(cannot	be found) then a null value	is returned.</returns>			
        public override string ToString()
        {
            return Name;
        }
    }

    ///	<summary>		
    ///	This class holds root RegistryKeys for the system registry.	 Use these keys	to manipulate the registry.		
    ///	</summary>		
    public sealed class CSRegistry
    {
        // A byte saved is a byte earned, so we don't
        // instantiate these unless someone wants them
        private static CSRegistryKey m_CurrentUser = null;
        private static CSRegistryKey m_Users = null;
        private static CSRegistryKey m_LocalMachine = null;
        private static CSRegistryKey m_ClassesRoot = null;

        public static CSRegistryKey CurrentUser
        {
            get
            {
                if (m_CurrentUser == null)
                {
                    m_CurrentUser = new CSRegistryKey((uint)CSRegistryImpl.RootKey.HKEY_CURRENT_USER, "HKEY_CURRENT_USER");
                }
                return m_CurrentUser;
            }
        }
        public static CSRegistryKey Users
        {
            get
            {
                if (m_Users == null)
                {
                    m_Users = new CSRegistryKey((uint)CSRegistryImpl.RootKey.HKEY_USERS, "HKEY_USERS");
                }
                return m_Users;
            }
        }
        public static CSRegistryKey LocalMachine
        {
            get
            {
                if (m_LocalMachine == null)
                {
                    m_LocalMachine = new CSRegistryKey((uint)CSRegistryImpl.RootKey.HKEY_LOCAL_MACHINE, "HKEY_LOCAL_MACHINE");
                }
                return m_LocalMachine;
            }
        }
        public static CSRegistryKey ClassesRoot
        {
            get
            {
                if (m_ClassesRoot == null)
                {
                    m_ClassesRoot = new CSRegistryKey((uint)CSRegistryImpl.RootKey.HKEY_CLASSES_ROOT, "HKEY_CLASSES_ROOT");
                }
                return m_ClassesRoot;
            }
        }
    }

    ///	<summary>		
    ///	Internal helper	class for registry access.		
    ///	</summary>		
    internal sealed class CSRegistryImpl
    {
        public enum RootKey : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003
        }
        public enum KeyDisposition : int
        {
            REG_CREATED_NEW_KEY = 1,
            REG_OPENED_EXISTING_KEY = 2
        }
        internal const int REG_SZ = 1;
        internal const int REG_EXPAND_SZ = 2;
        internal const int REG_BINARY = 3;
        internal const int REG_DWORD = 4;

        #region	DLL	Imports

        [DllImport("coredll.dll", EntryPoint = "RegOpenKeyEx")]
        public static extern int RegOpenKeyEx(uint hKey, string lpSubKey, int ulOptions, int samDesired, ref uint phkResult);

        [DllImport("coredll", EntryPoint = "RegCreateKeyEx")]
        public static extern int RegCreateKeyEx(uint hKey, string lpSubKey, int lpReserved, string lpClass, int dwOptions, int samDesired, ref int lpSecurityAttributes, ref uint phkResult, ref int lpdwDisposition);

        [DllImport("coredll.dll", EntryPoint = "RegEnumKeyEx")]
        public static extern int RegEnumKeyEx(uint hKey, int iIndex, char[] sKeyName, ref int iKeyNameLen, int iReservedZero, char[] sClassName, ref int iClassNameLen, int iFiletimeZero);

        [DllImport("coredll.dll", EntryPoint = "RegEnumValue")]
        public static extern int RegEnumValue(uint hKey, int iIndex, char[] sValueName, ref	int iValueNameLen, int iReservedZero, ref int iType, ref byte[] byData, ref	int iDataLen);

        [DllImport("coredll.dll", EntryPoint = "RegQueryInfoKey")]
        public static extern int RegQueryInfoKey(uint hKey, char[] lpClass, ref	int lpcbClass, int reservedZero, ref int cSubkey, ref int iMaxSubkeyLen, ref int lpcbMaxSubkeyClassLen, ref int cValueNames, ref int iMaxValueNameLen, ref int iMaxValueLen, int securityDescriptorZero, int lastWriteTimeZero);

        [DllImport("coredll.dll", EntryPoint = "RegQueryValueEx")]
        public static extern int RegQueryValueEx(uint hKey, string lpValueName, int lpReserved, ref	int lpType, byte[] lpData, ref int lpcbData);

        [DllImport("coredll.dll", EntryPoint = "RegSetValueExW")]
        public static extern int RegSetValueEx(uint hKey, string lpValueName, int lpReserved, int lpType, byte[] lpData, int lpcbData);

        [DllImport("coredll.dll", EntryPoint = "RegCloseKey")]
        public static extern int RegCloseKey(uint hKey);

        [DllImport("coredll.dll", EntryPoint = "RegDeleteKey")]
        public static extern int RegDeleteKey(uint hKey, string keyName);

        [DllImport("coredll.dll", EntryPoint = "RegDeleteValue")]
        public static extern int RegDeleteValue(uint hKey, string valueName);

        #endregion

        ///	<summary>			
        ///	Helper to allow	me to easily open a	key			
        ///	</summary>			
        ///	<param name="RootKey"></param>			
        ///	<param name="SubKey"></param>			
        ///	<returns></returns>			
        public static uint OpenKey(uint RootKey, string SubKey)
        {
            uint hKey = 0;
            int ret = 0;
            try
            {
                ret = RegOpenKeyEx(RootKey, SubKey, 0, 0, ref	hKey);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception opening key" + SubKey + ": " + ex.Message);
            }
            if (ret != 0)
            {
                throw new Exception("Failed to open key" + SubKey + " with error code: " + ret.ToString());
            }

            return hKey;
        }

        ///	<summary>			
        ///	Helper to allow	me to easily Create	a key			
        ///	</summary>			
        ///	<param name="RootKey"></param>			
        ///	<param name="SubKey"></param>			
        ///	<param name="KeyClass"></param>			
        ///	<param name="Disposition"></param>			
        ///	<returns></returns>			
        public static uint CreateKey(uint RootKey, string SubKey, string KeyClass, ref KeyDisposition Disposition)
        {
            uint hKey = 0;
            int disp = 0;
            int reserved = 0;
            int ret = 0;

            try
            {
                ret = RegCreateKeyEx(RootKey, SubKey, 0, KeyClass, 0, 0, ref reserved, ref hKey, ref disp);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception creating key" + SubKey + ": " + ex.Message);
            }

            if (ret != 0)
            {
                throw new Exception("Failed to create key" + SubKey + " with error code: " + ret.ToString());
            }
            else
            {
                Disposition = (KeyDisposition)disp;
            }

            return hKey;
        }
    }
}