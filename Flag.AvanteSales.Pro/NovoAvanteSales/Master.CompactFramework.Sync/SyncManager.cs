using System;
using System.Data;
#if !ANDROID
using System.Drawing;
#else
using Android.Graphics;
#endif

using System.IO;
using System.Collections;
using System.Reflection;

namespace Master.CompactFramework.Sync
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public interface ISyncProvider
    {
        // Events
        event SyncManager.StatusChangedEventHandler CheckPointCompleted;
        event SyncManager.StatusChangedEventHandler CheckPointStarted;
        event SyncManager.StatusChangedEventHandler StatusChanged;


        event EventHandler DownloadCompleted;

        // Methods
        bool VerificaVersaoDLL(ref string mensagem, string versaoCompativel, string versaoAplicativoAvanteSales);
        void CargaImagem(string diretorioImagens,string codRevenda, long espacoDisponivelAndroid, string[] imagensExistentes);
        void Carga(params string[] dataParams);
        void CargaParcial(params string[] dataParams);
        void Descarga(DataSet dadosDescarga, params string[] dataParams);
        void AtualizaCabs();

        // Properties
        string DataBaseFilePath
        {
            get;
            set;
        }
        string ProviderName
        {
            get;
        }
        string ServerAddress
        {
            get;
            set;
        }
    }

    public class SyncManager
    {
        // Methods
        public SyncManager()
        {
        }

        public ISyncProvider CreateProvider(ProviderInfo providerInfo)
        {
            ISyncProvider provider = null;

            try
            {
                Assembly assembly = Assembly.LoadFrom(providerInfo.AssemblyPath);
                provider = (ISyncProvider)assembly.CreateInstance(providerInfo.ClassName);

            }
            catch (Exception exception)
            {
                throw exception;
            }

            return provider;
        }

        public ISyncProvider CreateProvider(string providerName)
        {
            ProviderInfo[] infoArray = SyncManager.EnumProviders();

            foreach (ProviderInfo info in infoArray)
            {
                if (info.ClassName == providerName)
                {
                    return this.CreateProvider(info);
                }
            }
            return null;
        }


        public static ProviderInfo[] EnumProviders()
        {
            ArrayList list1 = null;
            ArrayList list2 = null;
            ProviderInfo[] mappedProviders = null;

            try
            {
                list1 = new ArrayList();
                list2 = new ArrayList();
#if ANDROID
                string text1 = AvanteSales.CSGlobal.GetCurrentDirectory();
                mappedProviders = new ProviderInfo[2];
                mappedProviders[0] = new ProviderInfo() { ClassName = "SQLLiteDirectProvider", AssemblyPath = "AvanteSales.SyncManager.SQLLiteProvider" };
                mappedProviders[1] = new ProviderInfo() { ClassName = "XMLProvider", AssemblyPath = "AvanteSales.SyncManager.XmlProvider" };
                return mappedProviders;
#else
                string text1 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);

#endif
                string[] textArray1 = Directory.GetFiles(text1, "*Provider.dll");

                for (int num2 = 0; num2 < textArray1.Length; num2++)
                {
                    string text2 = textArray1[num2];

                    try
                    {
                        Assembly assembly1 = Assembly.LoadFrom(text2);
                        Module[] moduleArray1 = assembly1.GetModules();

                        for (int num3 = 0; num3 < moduleArray1.Length; num3++)
                        {
                            Module module1 = moduleArray1[num3];
                            Type[] typeArray1 = module1.GetTypes();

                            for (int num4 = 0; num4 < typeArray1.Length; num4++)
                            {
                                Type type1 = typeArray1[num4];
                                Type[] typeArray2 = type1.GetInterfaces();

                                for (int num5 = 0; num5 < typeArray2.Length; num5++)
                                {
                                    Type type2 = typeArray2[num5];

                                    if (type2.FullName == "Master.CompactFramework.Sync.ISyncProvider")
                                    {
                                        list1.Add(text2);
                                        list2.Add(type1.FullName);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                mappedProviders = new SyncManager.ProviderInfo[list1.Count];

                for (int num1 = 0; num1 < list1.Count; num1++)
                {
                    mappedProviders[num1] = new SyncManager.ProviderInfo();
                    mappedProviders[num1].ClassName = (string)list2[num1];
                    mappedProviders[num1].AssemblyPath = (string)list1[num1];
                }

                return mappedProviders;

            }
            catch (Exception exception1)
            {
                throw new ApplicationException("Erro ao enumerar os providers.", exception1);
            }
        }

        // Nested Types
        public class ProviderInfo
#if ANDROID
            : Java.Lang.Object
#endif  
        {
            // Fields
            public string AssemblyPath;
            public string ClassName;

            // Methods
            public ProviderInfo()
            {
            }

            public override string ToString()
            {
                char[] chArray1 = new char[1] { '.' };
                string[] textArray1 = this.ClassName.Split(chArray1);
                return textArray1[textArray1.Length - 1];
            }
        }
#if !ANDROID
        public delegate void StatusChangedEventHandler(string statusMessage, int maxProgress, int currentProgress, Bitmap icon);
#else
        public delegate void StatusChangedEventHandler(string statusMessage, int maxProgress, int currentProgress, Bitmap icon);
#endif
    }
}