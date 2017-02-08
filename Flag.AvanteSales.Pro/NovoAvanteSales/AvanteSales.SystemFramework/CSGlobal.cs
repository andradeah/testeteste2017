using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Text;
using System.IO;
#if ANDROID
using Android.App;
using Android.Content;
using Android.Util;
using Android.Telephony;
#endif
using AvanteSales.SystemFramework;
using AvanteSales.BusinessRules;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

#if !ANDROID
using System.Data.SQLite;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.WindowsCE.Forms;
#endif


namespace AvanteSales
{
    public class CSGlobal
    {
        #region [ Estruturas ]

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public UInt16 Year;
            public UInt16 Month;
            public UInt16 DayOfWeek;
            public UInt16 Day;
            public UInt16 Hour;
            public UInt16 Minute;
            public UInt16 Second;
            public UInt16 MilliSecond;
        }

        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public int lpMinimumApplicationAddress;
            public int lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        #endregion

        #region [ Variaveis ]

        private static int m_TipoNomePDV = 2;
        private static string m_NumCompatibilidadeComOBanco;
        private static string m_COD_REVENDA = "XXXXXXXX";
        private static bool m_CalculaPrecoNestle = true;
        private static bool m_PedidoSugerido = false;
        private static bool m_PesquisarComoSugerido = false;
        private static bool m_AlterarFuncionalidadeCheckColetaEstoque = false;
        private static bool m_PedidoTroca = false;
        private static bool m_AcabouDeGerarPedidoSugerido = false;
        private static bool m_ExisteProdutoColetadoPerda = false;
        private static int m_QtdeItemCombo = 0;
        private static bool m_PedidoComCombo = false;
        private static bool m_StorageCard = false;
        private static bool m_UtilizarStorageCard = false;
        private static string m_Usuario;
        private static string m_Senha;
        //private static bool m_Vlr_Desconto = false;
#if ANDROID
        private static Context _context;
#endif


        #endregion

        #region [ Propriedades ]

        /// <summary>
        /// Guarda qual será o nome Mostrado. 1 para Nome Fantasia e 2 para Razao Social
        /// </summary>
        public static int TipoNomePDV
        {
            get
            {
                return m_TipoNomePDV;
            }
            set
            {
                m_TipoNomePDV = value;
            }
        }

        public static string Usuario
        {
            get
            {
                return m_Usuario;
            }
            set
            {
                m_Usuario = value;
            }
        }

        public static string Senha
        {
            get
            {
                return m_Senha;
            }
            set
            {
                m_Senha = value;
            }
        }

        public static string NumCompatibilidadeComOBanco
        {
            get { return m_NumCompatibilidadeComOBanco; }
            set { m_NumCompatibilidadeComOBanco = value; }
        }
#if ANDROID
        public static Android.Content.Context Context
        {
            get { return _context; }
            set { _context = value; }
        }
#endif

#if !ANDROID
        public static bool JaOcorreuOutOfMemoryException { get; set; }
        public static frmProdutoPedido formprodutopedido;
        public static frmProdutoPedido formRecalculoPedido;
        public static frmProcuraProduto formProcuraProduto;
        public static frmPedido formPedido;
        public static frmProdutos formProdutos;
        public static frmListaPedidos formListaPedidos;
        public static frmCliente formCliente;
        public static FrmBlqTabela formBlqTabela;
        public static frmMain formMain;
#endif
        public static string Fonte
        {
            get
            {
                return "Tahoma";
            }
        }

        public static float FonteSize
        {
            get
            {
                return (float)7.2;
            }
        }
#if !ANDROID
        public static FontStyle FonteStyle
        {
            get
            {
                return FontStyle.Regular;
            }
        }

#endif
        public static string COD_REVENDA
        {
            get
            {
                return m_COD_REVENDA;
            }
            set
            {
                m_COD_REVENDA = value;
            }
        }
        /// <summary>
        ///  Informa se o calculo do preço sera calculado apos informa os produtos.
        /// </summary>
#if !ANDROID
        public static bool CalculaPrecoNestle
        {
            get
            {
                return m_CalculaPrecoNestle;
            }
            set
            {
                m_CalculaPrecoNestle = value;
            }
        }


#endif
        public static bool PedidoSugerido
        {
            get
            {
                return m_PedidoSugerido;
            }
            set
            {
                m_PedidoSugerido = value;
            }
        }

        //public static bool Vlr_Desconto
        //{
        //    get
        //    {
        //        return m_Vlr_Desconto;
        //    }
        //    set
        //    {
        //        m_Vlr_Desconto = value;
        //    }

        //}

        public static bool PesquisarComoSugerido
        {
            get
            {
                return m_PesquisarComoSugerido;
            }
            set
            {
                m_PesquisarComoSugerido = value;
            }
        }

        public static bool AlterarFuncionalidadeCheckColetaEstoque
        {
            get
            {
                return m_AlterarFuncionalidadeCheckColetaEstoque;
            }
            set
            {
                m_AlterarFuncionalidadeCheckColetaEstoque = value;
            }
        }

        public static bool PedidoTroca
        {
            get
            {
                return m_PedidoTroca;
            }
            set
            {
                m_PedidoTroca = value;
            }
        }

        public static bool AcabouDeGerarPedidoSugerido
        {
            get
            {
                return m_AcabouDeGerarPedidoSugerido;
            }
            set
            {
                m_AcabouDeGerarPedidoSugerido = value;
            }
        }

        public static bool ExisteProdutoColetadoPerda
        {
            get
            {
                return m_ExisteProdutoColetadoPerda;
            }
            set
            {
                m_ExisteProdutoColetadoPerda = value;
            }
        }

        /// <summary>
        /// Informa a quantidade de item de um combo
        /// </summary>
        public static int QtdeItemCombo
        {
            get
            {
                return m_QtdeItemCombo;
            }
            set
            {
                m_QtdeItemCombo = value;
            }
        }
        /// <summary>
        /// Informa se o pedido tem combo
        /// </summary>
        public static bool PedidoComCombo
        {
            get
            {
                return m_PedidoComCombo;
            }
            set
            {
                m_PedidoComCombo = value;
            }
        }
        public static bool StorageCard
        {
            get
            {
                return m_StorageCard;
            }
            set
            {
                m_StorageCard = value;
            }
        }
        public static bool UtilizarStorageCard
        {
            get
            {
                return m_UtilizarStorageCard;
            }
            set
            {
                m_UtilizarStorageCard = value;
            }
        }
        #endregion


        //public static int ContadorDeTelas { get; set; }
        #region [ Constantes ]

        public class STATUS_ATUALIZACAO
        {
            public const string FALHA_CARGA_TOTAL = "FALHA_CARGA_TOTAL";
            public const string FALHA_CARGA_PARCIAL = "FALHA_CARGA_PARCIAL";
            public const string ATUALIZACAO_VERSAO = "ATUALIZACAO_VERSAO";
            public const string SUCESSO = "SUCESSO";
        }

        //SpecialFolderPath
        public enum SpecialFolderPath
        {
            CSIDL_FLAG_CREATE = (0x8000),
            //Version 5.0. Combine this CSIDL with any of the following CSIDLs to force the creation of the associated folder.
            CSIDL_ADMINTOOLS = (0x0030),
            //Version 5.0. The file system directory that is used to store administrative tools for an individual user. The Microsoft Management Console (MMC) will save customized consoles to this directory, and it will roam with the user.
            CSIDL_ALTSTARTUP = (0x001d),
            //The file system directory that corresponds to the user's nonlocalized Startup program group.
            CSIDL_APPDATA = (0x001a),
            //Version 4.71. The file system directory that serves as a common repository for application-specific data. A typical path is C:\Documents and Settings\username\Application Data. This CSIDL is supported by the redistributable Shfolder.dll for systems that do not have the Microsoft Internet Explorer 4.0 integrated Shell installed.
            CSIDL_BITBUCKET = (0x000a),
            //The virtual folder containing the objects in the user's Recycle Bin.

            CSIDL_CDBURN_AREA = (0x003b),
            //Version 6.0. The file system directory acting as a staging area for files waiting to be written to CD. A typical path is C:\Documents and Settings\username\Local Settings\Application Data\Microsoft\CD Burning.

            CSIDL_COMMON_ADMINTOOLS = (0x002f),
            //Version 5.0. The file system directory containing administrative tools for all users of the computer.

            CSIDL_COMMON_ALTSTARTUP = (0x001e),
            //The file system directory that corresponds to the nonlocalized Startup program group for all users. Valid only for Microsoft Windows NT systems.

            CSIDL_COMMON_APPDATA = (0x0023),
            //Version 5.0. The file system directory containing application data for all users. A typical path is C:\Documents and Settings\All Users\Application Data.

            CSIDL_COMMON_DESKTOPDIRECTORY = (0x0019),
            //The file system directory that contains files and folders that appear on the desktop for all users. A typical path is C:\Documents and Settings\All Users\Desktop. Valid only for Windows NT systems.

            CSIDL_COMMON_DOCUMENTS = (0x002e),
            //The file system directory that contains documents that are common to all users. A typical paths is C:\Documents and Settings\All Users\Documents. Valid for Windows NT systems and Microsoft Windows 95 and Windows 98 systems with Shfolder.dll installed.

            CSIDL_COMMON_FAVORITES = (0x001f),
            //The file system directory that serves as a common repository for favorite items common to all users. Valid only for Windows NT systems.

            CSIDL_COMMON_MUSIC = (0x0035),
            //Version 6.0. The file system directory that serves as a repository for music files common to all users. A typical path is C:\Documents and Settings\All Users\Documents\My Music.

            CSIDL_COMMON_PICTURES = (0x0036),
            //Version 6.0. The file system directory that serves as a repository for image files common to all users. A typical path is C:\Documents and Settings\All Users\Documents\My Pictures.

            CSIDL_COMMON_PROGRAMS = (0x0017),
            //The file system directory that contains the directories for the common program groups that appear on the Start menu for all users. A typical path is C:\Documents and Settings\All Users\Start Menu\Programs. Valid only for Windows NT systems.

            CSIDL_COMMON_STARTMENU = (0x0016),
            //The file system directory that contains the programs and folders that appear on the Start menu for all users. A typical path is C:\Documents and Settings\All Users\Start Menu. Valid only for Windows NT systems.

            CSIDL_COMMON_STARTUP = (0x0018),
            //The file system directory that contains the programs that appear in the Startup folder for all users. A typical path is C:\Documents and Settings\All Users\Start Menu\Programs\Startup. Valid only for Windows NT systems.

            CSIDL_COMMON_TEMPLATES = (0x002d),
            //The file system directory that contains the templates that are available to all users. A typical path is C:\Documents and Settings\All Users\Templates. Valid only for Windows NT systems.

            CSIDL_COMMON_VIDEO = (0x0037),
            //Version 6.0. The file system directory that serves as a repository for video files common to all users. A typical path is C:\Documents and Settings\All Users\Documents\My Videos.

            CSIDL_COMPUTERSNEARME = (0x003d),
            //The folder representing other machines in your workgroup.

            CSIDL_CONNECTIONS = (0x0031),
            //The virtual folder representing Network Connections, containing network and dial-up connections. 

            CSIDL_CONTROLS = (0x0003),
            //The virtual folder containing icons for the Control Panel applications.

            CSIDL_COOKIES = (0x0021),
            //The file system directory that serves as a common repository for Internet cookies. A typical path is C:\Documents and Settings\username\Cookies.

            CSIDL_DESKTOP = (0x0000),
            //The virtual folder representing the Windows desktop, the root of the namespace.

            CSIDL_DESKTOPDIRECTORY = (0x0010),
            //The file system directory used to physically store file objects on the desktop = (not to be confused with the desktop folder itself),. A typical path is C:\Documents and Settings\username\Desktop.

            CSIDL_DRIVES = (0x0011),
            //The virtual folder representing My Computer, containing everything on the local computer: storage devices, printers, and Control Panel. The folder may also contain mapped network drives.

            CSIDL_FAVORITES = (0x0006),
            //The file system directory that serves as a common repository for the user's favorite items. A typical path is C:\Documents and Settings\username\Favorites.

            CSIDL_FLAG_DONT_VERIFY = (0x4000),
            //Combine with another CSIDL constant, except for CSIDL_FLAG_CREATE, to return an unverified folder pathÂ—with no attempt to create or initialize the folder. 

            CSIDL_FONTS = (0x0014),
            //A virtual folder containing fonts. A typical path is C:\Windows\Fonts.

            CSIDL_HISTORY = (0x0022),
            //The file system directory that serves as a common repository for Internet history items.

            CSIDL_INTERNET = (0x0001),
            //A viritual folder for Internet Explorer = (icon on desktop),.

            CSIDL_INTERNET_CACHE = (0x0020),
            //Version 4.72. The file system directory that serves as a common repository for temporary Internet files. A typical path is C:\Documents and Settings\username\Local Settings\Temporary Internet Files.

            CSIDL_LOCAL_APPDATA = (0x001c),
            //Version 5.0. The file system directory that serves as a data repository for local = (nonroaming), applications. A typical path is C:\Documents and Settings\username\Local Settings\Application Data.

            CSIDL_MYDOCUMENTS = (0x000c),
            //Version 6.0. The virtual folder representing the My Documents desktop item.

            CSIDL_MYMUSIC = (0x000d),
            //The file system directory that serves as a common repository for music files. A typical path is C:\Documents and Settings\User\My Documents\My Music.

            CSIDL_MYPICTURES = (0x0027),
            //Version 5.0. The file system directory that serves as a common repository for image files. A typical path is C:\Documents and Settings\username\My Documents\My Pictures.

            CSIDL_MYVIDEO = (0x000e),
            //Version 6.0. The file system directory that serves as a common repository for video files. A typical path is C:\Documents and Settings\username\My Documents\My Videos.

            CSIDL_NETHOOD = (0x0013),
            //A file system directory containing the link objects that may exist in the My Network Places virtual folder. It is not the same as CSIDL_NETWORK, which represents the network namespace root. A typical path is C:\Documents and Settings\username\NetHood.

            CSIDL_NETWORK = (0x0012),
            //A virtual folder representing Network Neighborhood, the root of the network namespace hierarchy.

            CSIDL_PERSONAL = (0x0005),
            //Version 6.0. The virtual folder representing the My Documents desktop item. This is equivalent to CSIDL_MYDOCUMENTS. 
            //Previous to Version 6.0. The file system directory used to physically store a user's common repository of documents. A typical path is C:\Documents and Settings\username\My Documents. This should be distinguished from the virtual My Documents folder in the namespace. To access that virtual folder, use SHGetFolderLocation, which returns the ITEMIDLIST for the virtual location, or refer to the technique described in Managing the File System.

            CSIDL_PRINTERS = (0x0004),
            //The virtual folder containing installed printers.

            CSIDL_PRINTHOOD = (0x001b),
            //The file system directory that contains the link objects that can exist in the Printers virtual folder. A typical path is C:\Documents and Settings\username\PrintHood.

            CSIDL_PROFILE = (0x0028),
            //Version 5.0. The user's profile folder. A typical path is C:\Documents and Settings\username. Applications should not create files or folders at this level; they should put their data under the locations referred to by CSIDL_APPDATA or CSIDL_LOCAL_APPDATA.

            CSIDL_PROGRAM_FILES = (0x0026),
            //Version 5.0. The Program Files folder. A typical path is C:\Program Files.

            CSIDL_PROGRAM_FILES_COMMON = (0x002b),
            //Version 5.0. A folder for components that are shared across applications. A typical path is C:\Program Files\Common. Valid only for Windows NT, Windows 2000, and Windows XP systems. Not valid for Windows Millennium Edition = (Windows Me),.

            CSIDL_PROGRAMS = (0x0002),
            //The file system directory that contains the user's program groups = (which are themselves file system directories),. A typical path is C:\Documents and Settings\username\Start Menu\Programs. 

            CSIDL_RECENT = (0x0008),
            //The file system directory that contains shortcuts to the user's most recently used documents. A typical path is C:\Documents and Settings\username\My Recent Documents. To create a shortcut in this folder, use SHAddToRecentDocs. In addition to creating the shortcut, this function updates the Shell's list of recent documents and adds the shortcut to the My Recent Documents submenu of the Start menu.

            CSIDL_SENDTO = (0x0009),
            //The file system directory that contains Send To menu items. A typical path is C:\Documents and Settings\username\SendTo.

            CSIDL_STARTMENU = (0x000b),
            //The file system directory containing Start menu items. A typical path is C:\Documents and Settings\username\Start Menu.

            CSIDL_STARTUP = (0x0007),
            //The file system directory that corresponds to the user's Startup program group. The system starts these programs whenever any user logs onto Windows NT or starts Windows 95. A typical path is C:\Documents and Settings\username\Start Menu\Programs\Startup.

            CSIDL_SYSTEM = (0x0025),
            //Version 5.0. The Windows System folder. A typical path is C:\Windows\System32.

            CSIDL_TEMPLATES = (0x0015),
            //The file system directory that serves as a common repository for document templates. A typical path is C:\Documents and Settings\username\Templates.

            CSIDL_WINDOWS = (0x0024),
            //Version 5.0. The Windows directory or SYSROOT. This corresponds to the %windir% or %SYSTEMROOT% environment variables. A typical path is C:\Windows.
        }

        const int PROCESSOR_X86_32BIT_CORE = 1;

        const int PROCESSOR_MIPS16_CORE = 1;
        const int PROCESSOR_MIPSII_CORE = 2;
        const int PROCESSOR_MIPSIV_CORE = 3;

        const int PROCESSOR_HITACHI_SH3_CORE = 1;
        const int PROCESSOR_HITACHI_SH4_CORE = 2;

        const int PROCESSOR_ARM_V4_CORE = 1;
        const int PROCESSOR_ARM_V4I_CORE = 2;
        const int PROCESSOR_ARM_V4T_CORE = 3;

        const int PROCESSOR_FEATURE_NOFP = 0;
        const int PROCESSOR_FEATURE_FP = 1;
        const int PROCESSOR_FEATURE_DSP = PROCESSOR_FEATURE_FP;

        const int PROCESSOR_QUERY_INSTRUCTION = 0; //PROCESSOR_INSTRUCTION_CODE(0,0,0);
        const int PROCESSOR_X86_32BIT_INSTRUCTION = 0x00010001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_INTEL, PROCESSOR_X86_32BIT_CORE, PROCESSOR_FEATURE_FP);
        const int PROCESSOR_MIPS_MIPS16_INSTRUCTION = 0x01010000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_MIPS,  PROCESSOR_MIPS16_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_MIPS_MIPSII_INSTRUCTION = 0x01020000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_MIPS,  PROCESSOR_MIPSII_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_MIPS_MIPSIIFP_INSTRUCTION = 0x01020001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_MIPS,  PROCESSOR_MIPSII_CORE, PROCESSOR_FEATURE_FP);
        const int PROCESSOR_MIPS_MIPSIV_INSTRUCTION = 0x01030000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_MIPS,  PROCESSOR_MIPSIV_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_MIPS_MIPSIVFP_INSTRUCTION = 0x01030001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_MIPS,  PROCESSOR_MIPSIV_CORE, PROCESSOR_FEATURE_FP);
        const int PROCESSOR_HITACHI_SH3_INSTRUCTION = 0x04010000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_SHX,   PROCESSOR_HITACHI_SH3_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_HITACHI_SH3DSP_INSTRUCTION = 0x04010001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_SHX,   PROCESSOR_HITACHI_SH3_CORE, PROCESSOR_FEATURE_DSP);
        const int PROCESSOR_HITACHI_SH4_INSTRUCTION = 0x04020001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_SHX,   PROCESSOR_HITACHI_SH4_CORE, PROCESSOR_FEATURE_FP);

        const int PROCESSOR_ARM_V4_INSTRUCTION = 0x05010000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_ARM_V4FP_INSTRUCTION = 0x05010001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4_CORE, PROCESSOR_FEATURE_FP);
        const int PROCESSOR_ARM_V4I_INSTRUCTION = 0x05020000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4I_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_ARM_V4IFP_INSTRUCTION = 0x05020001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4I_CORE, PROCESSOR_FEATURE_FP);
        const int PROCESSOR_ARM_V4T_INSTRUCTION = 0x05030000; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4T_CORE, PROCESSOR_FEATURE_NOFP);
        const int PROCESSOR_ARM_V4TFP_INSTRUCTION = 0x05030001; //PROCESSOR_INSTRUCTION_CODE(PROCESSOR_ARCHITECTURE_ARM,   PROCESSOR_ARM_V4T_CORE, PROCESSOR_FEATURE_FP);

        public const int SPI_GETWORKAREA = 48;
        public const uint SPI_GETPLATFORMTYPE = 257;
        public const int SM_CYCAPTION = 4;
        public const int SM_CYMENU = 15;

        #region PROCESSOR_ARCHITECTURE
        public const int PROCESSOR_INTEL_386 = 386;
        public const int PROCESSOR_INTEL_486 = 486;
        public const int PROCESSOR_INTEL_PENTIUM = 586;
        public const int PROCESSOR_INTEL_PENTIUMII = 686;
        public const int PROCESSOR_MIPS_R4000 = 4000;    // incl R4101 & R3910 for Windows CE
        public const int PROCESSOR_ALPHA_21064 = 21064;
        public const int PROCESSOR_PPC_403 = 403;
        public const int PROCESSOR_PPC_601 = 601;
        public const int PROCESSOR_PPC_603 = 603;
        public const int PROCESSOR_PPC_604 = 604;
        public const int PROCESSOR_PPC_620 = 620;
        public const int PROCESSOR_HITACHI_SH3 = 10003;   // Windows CE
        public const int PROCESSOR_HITACHI_SH3E = 10004;   // Windows CE
        public const int PROCESSOR_HITACHI_SH4 = 10005;   // Windows CE
        public const int PROCESSOR_MOTOROLA_821 = 821;     // Windows CE
        public const int PROCESSOR_SHx_SH3 = 103;     // Windows CE
        public const int PROCESSOR_SHx_SH4 = 104;     // Windows CE
        public const int PROCESSOR_STRONGARM = 2577;    // Windows CE - 0xA11
        public const int PROCESSOR_ARM720 = 1824;    // Windows CE - 0x720
        public const int PROCESSOR_ARM820 = 2080;    // Windows CE - 0x820
        public const int PROCESSOR_ARM920 = 2336;    // Windows CE - 0x920
        public const int PROCESSOR_ARM_7TDMI = 70001;   // Windows CE

        public const int PROCESSOR_ARCHITECTURE_INTEL = 0;
        public const int PROCESSOR_ARCHITECTURE_MIPS = 1;
        public const int PROCESSOR_ARCHITECTURE_ALPHA = 2;
        public const int PROCESSOR_ARCHITECTURE_PPC = 3;
        public const int PROCESSOR_ARCHITECTURE_SHX = 4;
        public const int PROCESSOR_ARCHITECTURE_ARM = 5;
        public const int PROCESSOR_ARCHITECTURE_IA64 = 6;
        public const int PROCESSOR_ARCHITECTURE_ALPHA64 = 7;
        public const int PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        #endregion

        #endregion

        #region [ Metodos ]

        #region [ API's ]

        public class SHELLEXECUTEEX
        {
            public UInt32 cbSize;
            public UInt32 fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            public IntPtr lpFile;
            public IntPtr lpParameters;
            public IntPtr lpDirectory;
            public int nShow;
            public IntPtr hInstApp;

            // Optional members 
            public IntPtr lpIDList;
            public IntPtr lpClass;
            public IntPtr hkeyClass;
            public UInt32 dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
#if !ANDROID
        [DllImport("Coredll.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll")]
        public extern static int ShellExecuteEx(SHELLEXECUTEEX ex);

        [DllImport("coredll")]
        public extern static IntPtr LocalAlloc(int flags, int size);

        [DllImport("coredll.dll", EntryPoint = "SetLocalTime", SetLastError = true)]
        private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll", EntryPoint = "ConvertDefaultLocale", SetLastError = true)]
        private static extern bool SetUserDefaultLCID(Int32 LCID);

        [DllImport("coredll")]
        public extern static void LocalFree(IntPtr ptr);

        [DllImport("coredll.dll")]
        private static extern IntPtr FindWindow(
            string lpClassName, // class name 
            string lpWindowName // window name 
            );

        [DllImport("coredll.dll")]
        private static extern int SendMessageW(
            string lpClassName, // class name 
            string lpWindowName // window name 
            );

        private static IntPtr GetWindowHandle(Form f)
        {
            return (IntPtr)FindWindow("", f.Text);
        }

        [DllImport("coredll")]
        public static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            ref RECT pvParam,
            uint fWinIni);
        [DllImport("coredll")]
        public static extern bool SystemParametersInfo(
            uint uiAction,
            uint uiParam,
            StringBuilder pvParam,
            uint fWinIni);

        [DllImport("coredll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("Coredll")]
        public static extern bool KernelIoControl(UInt32 dwIoControlCode, IntPtr lpInBuf, UInt32 nInBufSize, byte[] buf, UInt32 nOutBufSize, [In, Out] uint lpBytesReturned);

        const int IOCTL_PROCESSOR_INFORMATION = 0x01010064;

        [DllImport("Coredll")]
        public static extern void GetSystemInfo(ref SYSTEM_INFO SystemInfo);

        [DllImport("Coredll")]
        public static extern bool QueryInstructionSet(
            uint dwInstructionSet,
            out CpuInstructionSet CurrentInstructionSet
            );

        [DllImport("coredll.dll")]
        static extern int SHGetSpecialFolderPath(IntPtr hwndOwner, StringBuilder lpszPath, int nFolder, int fCreate);
#endif
        #endregion
#if !ANDROID

        /// <summary>
        /// 
        /// 		/// </summary>
        /// <param name="formToDraw">form que será pintado</param>
        /// <param name="formCaption">String que será desenhado no top do form</param>
        public static void DrawFormStyle(Form formToDraw)
        {
            //ContadorDeTelas++;
            try
            {
                if (DeveCriarLayoutDaTela)
                {
                    CriarLayoutTelaPadrao(formToDraw);
                }

                AdicionarEstiloNoForm(formToDraw);
                RedimensionarIcones(formToDraw);
            }
            catch (OutOfMemoryException)
            {
                //MessageBox.Show("Deu erro na tela: " + ContadorDeTelas);
                if (!JaOcorreuOutOfMemoryException)
                    MessageBox.Show("O Sistema está com pouca memória, algumas imagens de podem não ser exibidas ou serem reduzidas.");
                JaOcorreuOutOfMemoryException = true;
                //FlushMemory();
            }
            // Adiciona o title
            //Image imgTitle = null;
            //Image imgLeft = null;
            //Image imgBotton = null;
            //if (widthFactor >= 2 && widthFactor < 3)
            //{
            //    imgTitle = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.top1_2.bmp"));
            //    imgLeft = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.line_2.bmp"));
            //    imgBotton = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.botton_2.bmp"));
            //}
            //else                                                   
            //{
            //    imgTitle = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.top1.bmp"));
            //    imgLeft = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.line.bmp"));
            //    imgBotton = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("AvanteSales.Imagens.botton.bmp"));

            //}                                                          
        }

        public static int GetPixelFactor(int current, float factor)
        {
            return Convert.ToInt32(Math.Round(current * factor));
        }


        private static Color azul = Color.FromArgb(82, 93, 173);
        private static Color amarelo = Color.FromArgb(255, 190, 82);

        private static float? _heightFactor;
        private static float? HeightFactor
        {
            get
            {
                return _heightFactor;
            }
            set
            {
                _heightFactor = (float)value / 268;
            }
        }

        private static float? _widthFactor;
        private static float? WidthFactor
        {
            get
            {
                return _widthFactor;
            }
            set
            {
                _widthFactor = (float)value / 240;
            }
        }

        private static int FormWidth;
        private static int FormHeight;

        private static bool DeveCriarLayoutDaTela = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formToDraw">form que será pintado</param>
        /// <param name="formCaption">String que será desenhado no top do form</param>
        public static void DrawFormStyle(Form formToDraw, string formCaption)
        {
            DrawFormStyle(formToDraw);

            if (!string.IsNullOrEmpty(formCaption))
            {
                // Busca o tamanho em pixels do desenho da string
                AdicionarCaptionNoForm(formToDraw, formCaption);
            }
        }

        private static void AdicionarCaptionNoForm(Form formToDraw, string formCaption)
        {
            DrawString(formToDraw, formCaption);

        }

        private static void AdicionarEstiloNoForm(Form formToDraw)
        {
            try
            {
                if (DeveCriarLayoutDaTela)
                {
                    CriarBarraLateral();
                    CriarBarraSuperior();
                }
                AdicionarBarraLateral(formToDraw);
                AdicionarBarraSuperior(formToDraw);
                formToDraw.PerformAutoScale();
            }
            catch (ObjectDisposedException)
            {
                //CriarBarraLateral();
                //CriarBarraSuperior();
                //AdicionarBarraLateral(formToDraw);
                //AdicionarBarraSuperior(formToDraw);
                //formToDraw.PerformAutoScale();
            }
        }

        private static void AdicionarBarraSuperior(Form formToDraw)
        {
            formToDraw.Controls.Add(BarraSuperior);
            BarraSuperior.Show();
        }

        private static void AdicionarBarraLateral(Form formToDraw)
        {
            formToDraw.Controls.Add(BarraLateral);
            BarraLateral.Show();
        }

        private static void RedimensionarIcones(Form formToDraw)
        {
            var imagelist = formToDraw.Controls.OfType<ToolBar>();

            var largura = GetPixelFactor(16, WidthFactor.Value);
            var altura = GetPixelFactor(16, HeightFactor.Value);
            var proporcao = Math.Min(largura, altura);

            foreach (var item in imagelist)
            {
                item.ImageList.ImageSize = new Size(proporcao, proporcao);
            }
        }

        //private class CaixaDeImagem : PictureBox
        //{
        //    protected override void Dispose(bool disposing)
        //    {                
        //        //desabilita o dispose da caixa de Imagem
        //        //base.Dispose(disposing);
        //    }
        //}

        private static PictureBox _barraLateral;
        private static PictureBox BarraLateral
        {
            get { return _barraLateral; }
            set { _barraLateral = value; }
        }

        private static PictureBox _barraSuperior;
        private static PictureBox BarraSuperior
        {
            get { return _barraSuperior; }
            set { _barraSuperior = value; }
        }

        private static void CriarLayoutTelaPadrao(Form formToDraw)
        {
            HeightFactor = FormHeight = formToDraw.Height;
            WidthFactor = FormWidth = formToDraw.Width;

            CriarBarraLateral();
            CriarBarraSuperior();

            //DeveCriarLayoutDaTela = false;
        }

        private static void CriarBarraSuperior()
        {

            var rectangle = new Rectangle(0, 0, FormWidth, GetPixelFactor(15, HeightFactor.Value));
            var bitmap = new Bitmap(rectangle.Width, rectangle.Height);
            var graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(new SolidBrush(azul), rectangle);
            var pontos = new Point[] { 
                    new Point(0, 0), 
                    new Point(0, GetPixelFactor(15, HeightFactor.Value)), 
                    new Point(GetPixelFactor(30, HeightFactor.Value), GetPixelFactor(15, HeightFactor.Value)), 
                    new Point(GetPixelFactor(15, HeightFactor.Value), 0)
                };
            graphics.FillPolygon(new SolidBrush(amarelo), pontos);
            BarraSuperior = new PictureBox() { Image = bitmap, Size = bitmap.Size, Location = rectangle.Location };
            BarraSuperior.Disposed += new EventHandler(BarraSuperior_Disposed);
        }

        static void BarraSuperior_Disposed(object sender, EventArgs e)
        {
            //MessageBox.Show("BarraSuperior_Disposed");
            DeveCriarLayoutDaTela = true;
        }

        private static void CriarBarraLateral()
        {
            var rectangle = new Rectangle(0, 0, 10, FormHeight);
            var bitmap = new Bitmap(rectangle.Width, rectangle.Height);
            var graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(new SolidBrush(amarelo), rectangle);

            BarraLateral = new PictureBox() { Image = bitmap, Size = bitmap.Size, Location = rectangle.Location };
            BarraLateral.Disposed += new EventHandler(BarraLateral_Disposed);
        }

        static void BarraLateral_Disposed(object sender, EventArgs e)
        {
            DeveCriarLayoutDaTela = true;
        }

        public static void DrawString(Form formToDraw, string formCaption)
        {
            var heightFactor = (float)formToDraw.Height / 268;
            var widthFactor = (float)formToDraw.Width / 240;
            Label lblCaption = null;

            foreach (Control c in formToDraw.Controls)
            {
                if (c.Name == "lblCaption")
                {
                    lblCaption = ((Label)c);
                    break;
                }
            }

            if (lblCaption == null)
            {
                lblCaption = new Label() { Name = "lblCaption", Font = new Font("Tahoma", 8, FontStyle.Bold), ForeColor = Color.White, BackColor = azul };
            }

            // Busca o tamanho em pixels do desenho da string
            SizeF s = Graphics.FromImage(BarraSuperior.Image).MeasureString(formCaption, new Font("Tahoma", 8, FontStyle.Bold));

            lblCaption.Location = new Point(GetPixelFactor(40, WidthFactor.Value), GetPixelFactor(2, HeightFactor.Value));
            lblCaption.Text = formCaption;
            lblCaption.Width = GetPixelFactor((int)s.Width + 1, WidthFactor.Value);
            formToDraw.Controls.Add(lblCaption);
            lblCaption.BringToFront();
            formToDraw.PerformAutoScale();
        }

        /// <summary>
        /// Mostra um form fazendo todas as tarefas para a correta exibicao no CF
        /// </summary>
        /// <param name="formToShow">Windows.Form a exibir</param>
        public static void ShowForm(Form formToShow, Form formShowing)
        {
            string originalText = formShowing.Text;
            formShowing.Text = "";

            formToShow.ShowDialog();

            formShowing.Text = originalText;
            formShowing.BringToFront();
            //formToShow.PerformAutoScale();
            //formShowing.PerformAutoScale();

        }

        public static void DrawControlBorder(Control c)
        {
            Image img = new Bitmap(c.Width + 2, c.Height + 2);
            Graphics gfxBack = Graphics.FromImage(img);

            gfxBack.FillRectangle(new SolidBrush(Color.Black), 0, 0, img.Width, img.Height);

            PictureBox picBorder = new PictureBox();
            picBorder.Size = new Size(c.Width + 2, c.Height + 2);
            picBorder.Location = new Point(c.Bounds.X - 1, c.Bounds.Y - 1);
            picBorder.Visible = true;

            c.Parent.Controls.Add(picBorder);
            picBorder.Image = img;
            picBorder.BringToFront();

            c.BringToFront();
            gfxBack.Dispose();

        }
#endif
        public CSGlobal()
        {
        }

        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static bool RespostaValida(CSPesquisasMercado.CSPesquisaMercado.CSMarcas.CSMarca.CSRespostas.CSResposta resposta)
        {
            bool valido = true;

            if (resposta.VAL_RESPOSTA == null)
                valido = false;
            else
            {
                if (resposta.PERGUNTA.TIP_RESP_PERGUNTA_MERC == 1)
                    return (resposta.VAL_RESPOSTA == "Sim" || resposta.VAL_RESPOSTA == "Não");
                //else if (resposta.PERGUNTA.TIP_RESP_PERGUNTA_MERC == 5)
                //    return string.IsNullOrEmpty(resposta.VAL_RESPOSTA;
                else if (resposta.PERGUNTA.TIP_RESP_PERGUNTA_MERC == 2 ||
                        resposta.PERGUNTA.TIP_RESP_PERGUNTA_MERC == 3)
                    return (Convert.ToDecimal(resposta.VAL_RESPOSTA) >= resposta.PERGUNTA.VAL_FAIXAINI_MERC &&
                            Convert.ToDecimal(resposta.VAL_RESPOSTA) <= resposta.PERGUNTA.VAL_FAIXAFIM_MERC);
                else if (resposta.PERGUNTA.TIP_RESP_PERGUNTA_MERC == 4)
                    return ((string.IsNullOrEmpty(resposta.VAL_RESPOSTA)) || (resposta.VAL_RESPOSTA.Length >= 13 && resposta.VAL_RESPOSTA.Length <= 14));
            }

            return valido;
        }

        public static decimal StrToDecimal(string text)
        {
            text = text.Trim();

            if (text.Length == 0 || text == CSGlobal.DecimalSeparator.ToString())
                return 0;
            else
                return Convert.ToDecimal(text);
        }

        public static int StrToInt(string text)
        {
            text = text.Trim();

            if (text.Length == 0 || text == CSGlobal.DecimalSeparator.ToString())
                return 0;
            else
                return int.Parse(text);
        }

        private static bool m_ValidarTopCategoria;

        public static bool ValidarTopCategoria
        {
            get
            {
                return m_ValidarTopCategoria;
            }
            set
            {
                m_ValidarTopCategoria = value;
            }
        }

        public static void SetSelection(EditText txt)
        {
            if (!string.IsNullOrEmpty(txt.Text))
                txt.SetSelection(txt.Text.Length);
        }

        public static void Focus(Context ctx, View view)
        {
            view.RequestFocus();

            InputMethodManager imm = (InputMethodManager)ctx.GetSystemService(Context.InputMethodService);
            imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        public static void EsconderTeclado(Activity activity)
        {
            InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Activity.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken, 0);
        }

        public static decimal Round(decimal numero, int casas)
        {
            int sinal = (numero >= 0) ? 1 : -1;
            numero = Math.Abs(numero);

            int inteira = (int)numero;
            decimal fracionaria = numero - inteira;

            casas = (int)Math.Pow(10, casas);
            decimal parcial = (fracionaria * casas);

            if ((parcial - ((int)parcial)) >= (decimal)0.5)
                parcial = (int)parcial + 1;
            else
                parcial = (int)parcial;

            fracionaria = parcial / casas;

            return (inteira + fracionaria) * sinal;
        }

        public static string ArquivoImagem(CSProdutos.CSProduto produto)
        {
            var diretorioAvante = CSGlobal.GetCurrentDirectory();
            int indexDiretorioAvante = diretorioAvante.ToLower().IndexOf("avante");
            var diretorioImagens = Path.Combine(diretorioAvante.Substring(0, indexDiretorioAvante), "ImagensProdutosAvante");

            string fileIconeProduto = Path.Combine(diretorioImagens, string.Format("{0}.png", produto.COD_FABRICA_PRODUTO));

            if (!File.Exists(fileIconeProduto))
            {
                fileIconeProduto = Path.Combine(diretorioImagens, string.Format("{0}.png", produto.DESCRICAO_APELIDO_PRODUTO));

                if (!File.Exists(fileIconeProduto))
                {
                    fileIconeProduto = Path.Combine(diretorioImagens, string.Format("{0}.jpg", produto.COD_FABRICA_PRODUTO));

                    if (!File.Exists(fileIconeProduto))
                    {
                        fileIconeProduto = Path.Combine(diretorioImagens, string.Format("{0}.jpg", produto.DESCRICAO_APELIDO_PRODUTO));

                        if (!File.Exists(fileIconeProduto))
                            return null;
                    }
                }
            }

            return fileIconeProduto;
        }

        /// <summary>
        /// Muda a data do OS para a data informada
        /// </summary>
        /// <param name="d">Data que será a data do sistema</param>
        public static void MudaData(DateTime d)
        {
#if !ANDROID
            try
            {
                SYSTEMTIME t = new SYSTEMTIME();

                t.Day = (UInt16)d.Day;
                t.Month = (UInt16)d.Month;
                t.Year = (UInt16)d.Year;
                t.Hour = (UInt16)d.Hour;
                t.Minute = (UInt16)d.Minute;
                t.Second = (UInt16)0;
                t.MilliSecond = (UInt16)0;
                t.DayOfWeek = (UInt16)d.DayOfWeek;

                SetSystemTime(ref t);
                SetLocalTime(ref t);

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw ex;
            }
#endif
        }

        /// <summary>
        /// Bloqueia o acesso ao relógio do sistema
        /// </summary>
        public static void DisableClock()
        {
            try
            {
                // [ Abre o registro do relógio ]
                using (CSRegistryKey registro = CSRegistry.LocalMachine.OpenSubKey("Software\\Microsoft\\Clock", true))
                {
                    byte[] data = new byte[1];
                    data[0] = 0x00;

                    // [ Define o valor: Enabled 0x11, Disabled 0x00 ]
                    registro.SetValue("AppState", data);

                    registro.Close();
                    registro.Dispose();
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.Message);
            }
        }

        private static string GetSpecialFolderPath(SpecialFolderPath folderCSIDL)
        {

            StringBuilder resultPath = new StringBuilder(255);
#if !ANDROID
            SHGetSpecialFolderPath((IntPtr)0, resultPath, (int)folderCSIDL, 0);
#endif
            return resultPath.ToString();
        }

        public static void ShowMessage(string message)
        {
#if !ANDROID
            MessageBox.Show(message);
#endif

        }

        // [ apaga os atalhos duplicados do POZ ]
        public static void DeletaAtalhosPOZ()
        {
            string path = null;

            try
            {
                path = GetSpecialFolderPath(SpecialFolderPath.CSIDL_PROGRAMS);

                string[] files = Directory.GetFiles(path, "Avante Sales (*");

                foreach (string file in files)
                {
                    File.Delete(file);
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.Message);
            }
        }
        public static void Finalizar()
        {
#if !ANDROID
            if (CSGlobal.formprodutopedido != null)
                CSGlobal.formprodutopedido.Dispose();

            if (CSGlobal.formProcuraProduto != null)
                CSGlobal.formProcuraProduto.Dispose();

            if (CSGlobal.formPedido != null)
                CSGlobal.formPedido.Dispose();

            if (CSGlobal.formProdutos != null)
                CSGlobal.formProdutos.Dispose();

            if (CSGlobal.formListaPedidos != null)
                CSGlobal.formListaPedidos.Dispose();

            if (CSGlobal.formCliente != null)
                CSGlobal.formCliente.Dispose();
#endif

        }

        public static char DecimalSeparator
        {
            get
            {
                string ch = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                return ch.ToCharArray()[0];
            }
        }

        public static string DecimalStringFormat
        {
            get
            {
                return "#,##0.00";
            }
        }

        public static bool BloquearSaidaCliente { get; set; }
        public static int COD_VENDEDOR_DADOS { get; set; }

        public static bool IsNumeric(string numero)
        {
            bool resultado = true;

            for (int i = 0; i < numero.Length; i++)
            {
                if (!Char.IsNumber(numero, i))
                {
                    resultado = false;
                    break;
                }
            }
            return resultado;
        }
        public static long Asc(string strValor)
        {
            string strRetorno = "";
            string strValorCorreto = "";
            char strCaracter;
            //Tamanho maximo permitido parao o campo desaplcodite portanto todos os produtos
            //devem ter o mesmo tamanho se nao tiver completa com 9 para garatir ordenacao
            if (strValor.IndexOf(" ") >= 0)
                strValorCorreto = strValor.Replace(" ", "9"); //substitui espacos em branco
            else
                strValorCorreto = strValor;

            if (strValorCorreto.Length < 6)
            {
                strValorCorreto = strValorCorreto + new string('9', 6 - strValorCorreto.Length);
            }
            for (int i = 0; i < strValorCorreto.Length; i++)
            {
                strCaracter = System.Convert.ToChar(strValorCorreto.Substring(i, 1));
                if (!char.IsNumber(strCaracter))
                    strRetorno = strRetorno + System.Convert.ToString((System.Convert.ToInt32(strCaracter)));
                else
                    strRetorno = strRetorno + System.Convert.ToString((System.Convert.ToInt32(strCaracter)));
            }

            return System.Convert.ToInt64(strRetorno);
        }

        public static DateTime? DataUltimaCargaParcial()
        {
            string query = "SELECT DATA_ULTIMA_CARGA_PARCIAL FROM INFORMACOES_SINCRONIZACAO";

            var result = CSDataAccess.Instance.ExecuteScalar(query);

            if (result == System.DBNull.Value)
                return null;

            return Convert.ToDateTime(result);
        }

        public static bool VerificarCargaParcial()
        {
            if (CSEmpresa.ColunaExiste("EMPRESA", "IND_UTILIZA_CARGA_PARCIAL"))
            {
                string query = "SELECT IND_UTILIZA_CARGA_PARCIAL FROM EMPRESA";

                return CSDataAccess.Instance.ExecuteScalar(query).ToString().ToUpper() == "S" ? true : false;
            }

            return false;
        }

        public static void CriaTabelasAuxiliares()
        {
            try
            {
                if (CSDataAccess.Instance.TableExists("PDV") &&
                    !CSEmpresa.ColunaExiste("PDV", "IND_FOTO_DUVIDOSA"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV ADD IND_FOTO_DUVIDOSA VARCHAR(1)");
                }

                if (CSDataAccess.Instance.TableExists("PDV") &&
                    !CSEmpresa.ColunaExiste("PDV", "DSC_NOME_FOTO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV ADD DSC_NOME_FOTO VARCHAR(50)");
                }

                if (CSDataAccess.Instance.TableExists("PDV") &&
                    !CSEmpresa.ColunaExiste("PDV", "BOL_FOTO_VALIDADA"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV ADD BOL_FOTO_VALIDADA BIT");
                }

                if (CSDataAccess.Instance.TableExists("PDV") &&
                    !CSEmpresa.ColunaExiste("PDV", "NUM_LATITUDE_FOTO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV ADD NUM_LATITUDE_FOTO VARCHAR(200)");
                }

                if (CSDataAccess.Instance.TableExists("PDV") &&
                    !CSEmpresa.ColunaExiste("PDV", "NUM_LONGITUDE_FOTO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV ADD NUM_LONGITUDE_FOTO VARCHAR(200)");
                }

                if (CSDataAccess.Instance.TableExists("HISTORICO_MOTIVO") &&
                    !CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_LATITUDE_LOCALIZACAO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE HISTORICO_MOTIVO ADD NUM_LATITUDE_LOCALIZACAO VARCHAR(20)");
                }

                if (CSDataAccess.Instance.TableExists("HISTORICO_MOTIVO") &&
                    !CSEmpresa.ColunaExiste("HISTORICO_MOTIVO", "NUM_LONGITUDE_LOCALIZACAO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE HISTORICO_MOTIVO ADD NUM_LONGITUDE_LOCALIZACAO VARCHAR(20)");
                }

                if (CSDataAccess.Instance.TableExists("PEDIDO") &&
                  !CSEmpresa.ColunaExiste("PEDIDO", "NUM_LATITUDE_LOCALIZACAO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PEDIDO ADD NUM_LATITUDE_LOCALIZACAO VARCHAR(20)");
                }

                if (CSDataAccess.Instance.TableExists("PEDIDO") &&
                    !CSEmpresa.ColunaExiste("PEDIDO", "NUM_LONGITUDE_LOCALIZACAO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PEDIDO ADD NUM_LONGITUDE_LOCALIZACAO VARCHAR(20)");
                }

                if (CSDataAccess.Instance.TableExists("TELEFONE_PDV") &&
                    !CSEmpresa.ColunaExiste("TELEFONE_PDV", "IND_ALTERADO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE TELEFONE_PDV ADD IND_ALTERADO BIT");
                    CSDataAccess.Instance.ExecuteNonQuery("UPDATE TELEFONE_PDV SET IND_ALTERADO = 0");
                }

                if (CSDataAccess.Instance.TableExists("PDV_EMAIL") &&
                    !CSEmpresa.ColunaExiste("PDV_EMAIL", "IND_ALTERADO"))
                {
                    CSDataAccess.Instance.ExecuteNonQuery("ALTER TABLE PDV_EMAIL ADD IND_ALTERADO BIT");
                    CSDataAccess.Instance.ExecuteNonQuery("UPDATE PDV_EMAIL SET IND_ALTERADO = 0");
                }

                if (!CSDataAccess.Instance.TableExists("TMPITENS"))
                {
                    CSDataAccess.Instance.CreateTable("TMPITENS", "CREATE TABLE TMPITENS(COD_PRODUTO INT) ");
                }

                if (!CSDataAccess.Instance.TableExists("TMPITEMPEDIDO"))
                {
                    CSDataAccess.Instance.CreateTable(
                        "CREATE TABLE TMPITEMPEDIDO " +
                        " (VLR_UNITARIO NUMERIC(9,2), VLR_TOTAL NUMERIC(9,2), VLR_DESCONTO NUMERIC(9,2) " +
                        " ,PRC_DESCONTO NUMERIC(6,2), VLR_ADICIONAL_FINANCEIRO NUMERIC(9,2) " +
                        " ,PRC_ADICIONAL_FINANCEIRO NUMERIC(6,2), QTD_PEDIDA NUMERIC(16,2) " +
                        " ,STATE INT, CODPRODUTO INT, VLR_DESCONTO_UNITARIO NUMERIC(9,2) " +
                        " ,VLR_ADICIONAL_UNITARIO NUMERIC(9,2), COD_TABELA_PRECO INT " +
                        " ,VLR_INDENIZACAO NUMERIC(11,2), VLR_VERBA_EXTRA NUMERIC(11,2) " +
                        " ,VLR_VERBA_NORMAL NUMERIC(11,2), COD_ITEM_COMBO INT " +
                        " ,QTD_INDENIZACAO NUMERIC(16,2), VLR_UNITARIO_INDENIZACAO NUMERIC(11,2), IND_UTILIZA_QTD_SUGERIDA BIT NOT NULL, VLR_TOTAL_IMPOSTO_BROKER DECIMAL(9,2) NULL ) ");
                }
                else
                {
                    CSDataAccess.Instance.ClearTable("TMPITEMPEDIDO");
                }

                if (!CSDataAccess.Instance.TableExists("TMPITEMINDENIZACAO"))
                {
                    CSDataAccess.Instance.CreateTable(
                        "CREATE TABLE TMPITEMINDENIZACAO " +
                        "(COD_INDENIZACAO INT NOT NULL, " +
                        "COD_PRODUTO INT NOT NULL, " +
                        "MOTIVO_INDENIZACAO INT NOT NULL, " +
                        "QTD_INDENIZACAO INT NOT NULL, " +
                        "VOLUME_INDENIZACAO DECIMAL NOT NULL, " +
                        "PCT_TAXA_INDENIZACAO DECIMAL NOT NULL, " +
                        "VLR_INDENIZACAO DECIMAL NOT NULL, " +
                        "PESO DECIMAL NOT NULL, " +
                        "VLR_UNITARIO_INDENIZACAO DECIMAL NOT NULL)");
                }
                else
                {
                    CSDataAccess.Instance.ClearTable("TMPITEMINDENIZACAO");
                }

                if (!CSDataAccess.Instance.TableExists("TMP_PEDIDO_EXCLUIDO"))
                {
                    CSDataAccess.Instance.CreateTable("TMP_PEDIDO_EXCLUIDO",
                        "CREATE TABLE TMP_PEDIDO_EXCLUIDO " +
                        " (COD_OPERACAO INT, COD_PEDIDO INT, COD_EMPREGADO INT, COD_PDV INT " +
                        " ,DAT_PEDIDO DATETIME, VLR_TOTAL_PEDIDO NUMERIC(16,2), COD_CONDICAO_PAGAMENTO INT " +
                        " ,IND_HISTORICO BIT, COD_PEDIDO_FLEXX INT, IND_PRECO_ANTERIOR BIT, DAT_ALTERACAO DATETIME " +
                        " ,BOL_ATUALIZADO_FLEXX BIT,COD_PEDIDO_POCKET INT, DATA_ENTREGA DATETIME, COD_PDV_SOLDTO INT " +
                        " ,NUM_DOC_INDENIZACAO NVARCHAR(10), COD_TIPO_MOT_INDENIZACAO INT, COD_MOT_INDENIZACAO INT " +
                        " ,IND_VLR_DESCONTO_ATUSALDO BIT, IND_VLR_INDENIZACAO_ATUSALDO BIT, MENSAGEM_PEDIDO NVARCHAR(200), RECADO_PEDIDO NVARCHAR(200), IND_FOB BIT, COD_POLITICA_CALCULO_PRECO INT ) ");
                }

                if (!CSDataAccess.Instance.TableExists("TMPVARIAVEIS_BNG"))
                {
                    CSDataAccess.Instance.CreateTable("TMPVARIAVEIS_BNG",
                            "CREATE TABLE TMPVARIAVEIS_BNG(NR_NOTEBOOK  VARCHAR(4) " +
                            " , CD_BASE_CLIENTE VARCHAR(9) " +
                            " , CD_LOJA_CLIENTE VARCHAR(4) " +
                            " , CD_CLIENTE VARCHAR(10) " +
                            " , CD_UNIDADE_FEDERACAO VARCHAR(3) " +
                            " , CD_SEGMENTO_CLIENTE VARCHAR(2) " +
                            " , CD_SETOR_INDUSTRIAL VARCHAR(4) " +
                            " , CD_ORG_VENDAS VARCHAR(4) " +
                            " , CD_CAN_DISTRIBUICAO VARCHAR(2) " +
                            " , CD_SET_ATIVIDADES VARCHAR(2) " +
                            " , CD_REGIONAL_VENDA VARCHAR(10) " +
                            " , CD_FILIAL VARCHAR(4) " +
                            " , CD_GERENCIA_COMERCIAL VARCHAR(10) " +
                            " , CD_TIPO_FRETE VARCHAR(3) " +
                            " , PRODUTO_BUNGE INT " +
                            " , CD_NEGOCIO VARCHAR(9) " +
                            " , CD_CLASSIFICACAO_FISCAL VARCHAR(16) " +
                            " , CD_IMPOSTO VARCHAR(2) " +
                            " , CD_MICRORREGIAO VARCHAR(2) " +
                            " )");
                }
                else
                {
                    CSDataAccess.Instance.ClearTable("TMPVARIAVEIS_BNG");
                }

                if (!CSDataAccess.Instance.TableExists("INDENIZACAO"))
                {
                    CSDataAccess.Instance.CreateTable("INDENIZACAO",
                    "CREATE TABLE INDENIZACAO( " +
                    "COD_INDENIZACAO INT PRIMARY KEY NOT NULL UNIQUE, " +
                    "COD_INDENIZACAO_POCKET INT NULL, " +
                    "COD_PDV INT NOT NULL, " +
                    "COD_GRUPO_COMERCIALIZACAO INT NOT NULL, " +
                    "DAT_CADASTRO DATE NOT NULL, " +
                    "NUM_NOTA_DEVOLUCAO INT NOT NULL, " +
                    "SERIE_NOTA CHAR(3) NOT NULL, " +
                    "DAT_NOTA_DEVOLUCAO DATE, " +
                    "COD_VENDEDOR INT NOT NULL, " +
                    "VOLUME_INDENIZACAO DECIMAL NOT NULL, " +
                    "NOME_RESPONSAVEL VARCHAR(40) NOT NULL, " +
                    "STATUS CHAR(1) NOT NULL, " +
                    "VLR_TOTAL DECIMAL NOT NULL, " +
                    "CONDICAO_PAGAMENTO INT NOT NULL, " +
                    "PESO_BRUTO DECIMAL NOT NULL, " +
                    "IND_DESCARREGADO BIT)");
                }

                if (!CSDataAccess.Instance.TableExists("ITEM_INDENIZACAO"))
                {
                    CSDataAccess.Instance.CreateTable("ITEM_INDENIZACAO",
                    "CREATE TABLE ITEM_INDENIZACAO ( " +
                    "COD_INDENIZACAO INT NOT NULL, " +
                    "COD_PRODUTO INT NOT NULL, " +
                    "MOTIVO_INDENIZACAO INT NOT NULL, " +
                    "QTD_INDENIZACAO INT NOT NULL, " +
                    "VOLUME_INDENIZACAO DECIMAL NOT NULL, " +
                    "PCT_TAXA_INDENIZACAO DECIMAL NOT NULL, " +
                    "VLR_INDENIZACAO DECIMAL NOT NULL, " +
                    "PESO DECIMAL NOT NULL, " +
                    "VLR_UNITARIO_INDENIZACAO DECIMAL NOT NULL)");
                }

                if (!CSDataAccess.Instance.TableExists("VERSAO"))
                {
                    CSDataAccess.Instance.CreateTable("VERSAO", "CREATE TABLE VERSAO (DSC_VERSAO_PDA VARCHAR(10) NULL)");
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                throw new Exception("Erro na criação das tabelas temporárias", ex);
            }
        }

        public enum RelatoriosPDV
        {
            UltimosPedidos,
            DocumentosReceber,
            HistoricoIndenizacao,
            SimulacaoPreco
        }

        /// <summary>
        /// Muda a cultura do Sistema para portugues-brasil
        /// </summary>
        public static void ChangeCultureInfo(System.Globalization.CultureInfo Culture)
        {

#if !ANDROID
            if (!SetUserDefaultLCID((Int32)Culture.LCID))
            {
                throw new Exception("Erro ao mudar a linguagem do sistema para portugues-brasil.");
            }
#endif

        }


        /// <summary> 
        /// same common params as the VBScript DateDiff: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/script56/html/vsfctdatediff.asp 
        /// /*Sample Code: 
        /// * System.DateTime dt1 = new System.DateTime(1974,12,16); 
        /// * System.DateTime dt2 = new System.DateTime(1973,12,16); 
        /// * Page.Response.Write(Convert.ToString(DateDiff("t", dt1, dt2))); 
        /// * */ 
        /// </summary> 
        /// <param name="howtocompare"></param> 
        /// <param name="startDate"></param> 
        /// <param name="endDate"></param> 
        /// <returns></returns> 
        private double DateDiff(string howtocompare, System.DateTime startDate, System.DateTime endDate)
        {
            double diff = 0;
            try
            {
                System.TimeSpan TS = new System.TimeSpan(startDate.Ticks - endDate.Ticks);
                #region converstion options
                switch (howtocompare.ToLower())
                {
                    case "m":
                        diff = Convert.ToDouble(TS.TotalMinutes);
                        break;
                    case "s":
                        diff = Convert.ToDouble(TS.TotalSeconds);
                        break;
                    case "t":
                        diff = Convert.ToDouble(TS.Ticks);
                        break;
                    case "mm":
                        diff = Convert.ToDouble(TS.TotalMilliseconds);
                        break;
                    case "yyyy":
                        diff = Convert.ToDouble(TS.TotalDays / 365);
                        break;
                    case "q":
                        diff = Convert.ToDouble((TS.TotalDays / 365) / 4);
                        break;
                    default:
                        //d 
                        diff = Convert.ToDouble(TS.TotalDays);
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
                diff = -1;
            }
            return diff;
        }


        public static bool IsPocketPC()
        {
            Version verPPC = new Version(3, 0);
            Version verCurrent = Environment.OSVersion.Version;
            if (Environment.OSVersion.Platform == PlatformID.WinCE && verCurrent.Major == verPPC.Major)
            {
                return true;
            }
            return false;
        }
#if !ANDROID
        public static void CenterForm(Form frm)
        {
            Rectangle r = GetVisibleDesktop();
            frm.Location = new Point((r.Width - frm.Width) / 2, (r.Height - frm.Height) / 2);
        }



        public static CPU_ARCH GetCPUArchitecture()
        {
            SYSTEM_INFO si = new SYSTEM_INFO();
            GetSystemInfo(ref si);
            switch (si.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_ARM:
                    return CPU_ARCH.ARM;
                case PROCESSOR_ARCHITECTURE_SHX:
                    return CPU_ARCH.SH3;
                case PROCESSOR_ARCHITECTURE_INTEL:
                    return CPU_ARCH.X86;
                case PROCESSOR_ARCHITECTURE_MIPS:
                    return CPU_ARCH.MIPS;
                default:
                    return CPU_ARCH.Unknown;
            }
        }

        public static string GetPlatformType()
        {
            StringBuilder sb = new StringBuilder(255);
            SystemParametersInfo(SPI_GETPLATFORMTYPE, (uint)sb.Capacity, sb, 0);
            string platType = sb.ToString();
            if (platType == "PocketPC")
                return "PPC";
            else if (platType == "Windows CE")
                return "WCE";
            else
                return "Unknown";
        }

        public static string GetInstructionSet()
        {
            CpuInstructionSet iset = CpuInstructionSet.X86;
            try
            {
                QueryInstructionSet(0, out iset);
            }
            catch (MissingMethodException)
            {
                System.Windows.Forms.MessageBox.Show("MissingMethodException");
                // We are running an older version of the OS
                // so QueryInstructionSet is not available
                SYSTEM_INFO si = new SYSTEM_INFO();
                GetSystemInfo(ref si);
                switch (si.wProcessorArchitecture)
                {
                    case PROCESSOR_ARCHITECTURE_ARM:
                        return CpuInstructionSet.ARMV4.ToString();
                    case PROCESSOR_ARCHITECTURE_SHX:
                        return CpuInstructionSet.SH3.ToString();
                    case PROCESSOR_ARCHITECTURE_INTEL:
                        return CpuInstructionSet.X86.ToString();
                    case PROCESSOR_ARCHITECTURE_MIPS:
                        return CpuInstructionSet.MIPSV4.ToString();
                    default:
                        return "Unknown";
                }
            }

            return iset.ToString();
        }
        public static Rectangle GetVisibleDesktop()
        {
            RECT r = new RECT();
            SystemParametersInfo(SPI_GETWORKAREA, 0, ref r, 0U);
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

#endif

        public static void GravaLogSync(string TIPO_OPERACAO, string DSC_STATUS)
        {
#if !ANDROID
            // Verifica se o codigo informado é valido
            WebService.AvanteSales ws = new WebService.AvanteSales();
            ws.Url = ws.Url.Replace("localhost", CSConfiguracao.GetConfig("internetURL"));
            ws.GravaLogSync(int.Parse(CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA)), TIPO_OPERACAO, DSC_STATUS, CSGlobal.COD_REVENDA);
#endif
        }


        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum CPU_ARCH
        {
            ARM,
            X86,
            SH3,
            MIPS,
            Unknown
        }

        class PROCESSOR_INFO
        {
            byte[] data;

            public PROCESSOR_INFO()
            {
                data = new byte[574];
                wVersion = 1;
            }

            public byte[] DataBuffer
            {
                get
                {
                    return data;
                }
            }

            public ushort wVersion
            {
                get
                {
                    return (ushort)BitConverter.ToInt16(data, 0);
                }
                set
                {
                    BitConverter.GetBytes(value).CopyTo(data, 0);
                }
            }


            public string szProcessorCore
            {
                get
                {
                    byte[] ret = new byte[80];
                    Buffer.BlockCopy(data, 2, ret, 0, 80);
                    return Encoding.Unicode.GetString(ret, 0, ret.Length).TrimEnd('\0');
                }
                set
                {
                    Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, data, 2, 80);
                }
            }
            public ushort wCoreRevision
            {
                get
                {
                    return (ushort)BitConverter.ToInt16(data, 82);
                }
                set
                {
                    BitConverter.GetBytes(value).CopyTo(data, 82);
                }
            }

            public string szProcessorName
            {
                get
                {
                    byte[] ret = new byte[80];
                    Buffer.BlockCopy(data, 84, ret, 0, 80);
                    return Encoding.Unicode.GetString(ret, 0, ret.Length).TrimEnd('\0');
                }
                set
                {
                    Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, data, 84, 80);
                }
            }

            public ushort wProcessorRevision
            {
                get
                {
                    return (ushort)BitConverter.ToInt16(data, 164);
                }
                set
                {
                    BitConverter.GetBytes(value).CopyTo(data, 164);
                }
            }

            public string szCatalogNumber/*[100]*/
            {
                get
                {
                    byte[] ret = new byte[200];
                    Buffer.BlockCopy(data, 166, ret, 0, 200);
                    return Encoding.Unicode.GetString(ret, 0, ret.Length).TrimEnd('\0');
                }
                set
                {
                    Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, data, 166, 200);
                }
            }

            public string szVendor /*[100];*/
            {
                get
                {
                    byte[] ret = new byte[200];
                    Buffer.BlockCopy(data, 366, ret, 0, 200);
                    return Encoding.Unicode.GetString(ret, 0, ret.Length).TrimEnd('\0');
                }
                set
                {
                    Buffer.BlockCopy(Encoding.Unicode.GetBytes(value), 0, data, 366, 200);
                }
            }

            public uint dwInstructionSet
            {
                get
                {
                    return (uint)BitConverter.ToInt32(data, 566);
                }
                set
                {
                    BitConverter.GetBytes(value).CopyTo(data, 566);
                }
            }

            public uint dwClockSpeed
            {
                get
                {
                    return (uint)BitConverter.ToInt32(data, 570);
                }
                set
                {
                    BitConverter.GetBytes(value).CopyTo(data, 570);
                }
            }
        }

        public int PROCESSOR_INSTRUCTION_CODE(int arch, int core, int feature)
        {
            return ((arch) << 24 | (core) << 16 | (feature));
        }

        // Helper procedure
        public static string GetCurrentDirectory()
        {
#if ANDROID
            if (Context.GetExternalFilesDir(null) != null)
                return Context.GetExternalFilesDir(null).AbsolutePath;
            else
                return Context.GetDir("base", FileCreationMode.WorldWriteable).AbsolutePath;
#else
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
#endif
        }
        // Diretorio do banco de dados.
        public static string GetCurrentDirectoryDB()
        {
            try
            {
                string sPathBD = "";


                //if (m_UtilizarStorageCard)
                //{
                sPathBD = GetCurrentDirectoryDBCartao();
                if (sPathBD.ToString().Length > 0)
                {
                    sPathBD = Path.Combine(sPathBD, "AvanteSales");
                    if (!Directory.Exists(sPathBD))
                        Directory.CreateDirectory(sPathBD);
                }
                //}

                if (sPathBD.ToString().Length == 0)
                    sPathBD = GetCurrentDirectory();

                return sPathBD;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // Retorna diretorio do cartão de memoria
        public static string GetCurrentDirectoryDBCartao()
        {
#if ANDROID
            if (Android.OS.Environment.MediaMounted == "MEDIA_MOUNTED")
            {
                //return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
            else
            {
                return string.Empty;
            }
#else
            string sPathDBCartao = "";

            foreach (string sDir in Directory.GetDirectories(@"\"))
            {
                DirectoryInfo dInfo = new DirectoryInfo(sDir);

                if (dInfo.Attributes == (FileAttributes.Directory | FileAttributes.Temporary))
                {
                    // iPAQ File Store
                    if (dInfo.FullName.IndexOf("iPAQ File Store") > 0 ||
                        dInfo.FullName.Contains("Bluetooth"))
                        continue;

                    sPathDBCartao = dInfo.FullName;
                    m_StorageCard = true;
                    break;
                }
            }
            return sPathDBCartao;
#endif

        }
        public static string GetCurrentVersion()
        {
            string versao =
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString() + "." +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();

            return versao;
        }
#if ANDROID
        public static bool ValidaCompatibilidadeDoBancoSQLiteComAVersaoDoAvante()
        {

            if (string.IsNullOrEmpty(CSEmpresa.Current.VERSAO_AVANTE_SALES_COMPATIBILIDADE))
            {
                return true;
            }

            NumCompatibilidadeComOBanco = ""; //Context.PackageManager.GetApplicationInfo(Context.PackageName, 128).MetaData.Get("versaoCompatibilidadeComBanco").ToString();
            //TODO mudar isso aqui pra algo q vá funcionar

            var versoes1 = CSEmpresa.Current.VERSAO_AVANTE_SALES_COMPATIBILIDADE.Split('.');
            var versoes2 = NumCompatibilidadeComOBanco.Split('.');


            for (int i = 0; i < NumCompatibilidadeComOBanco.Count(c => c == '.'); i++)
            {
                switch (int.Parse(versoes1[i]).CompareTo(int.Parse(versoes2[i])))
                {
                    case -1:
                        ShowMessage("A versão do banco de dados é inferior à versão suportada pela atual versão do Avante Sales. Favor efetuar a descarga e fazer uma nova carga.");
                        return false;

                    case 1:
                        ShowMessage("A versão do banco de dados é superior à versão suportada pela atual versão do Avante Sales. Favor efetuar a descarga e atualizar a versão na Google Play");
                        return false;
                    default:
                        break;
                }
            }
            return true;

        }
#endif
        public static void VerificaAtualizacaoSistema()
        {
            // [ Verifica se houve atualização do sistema ]
            if (CSConfiguracao.GetConfig("VERSAO_AVANTE") != GetCurrentVersion())
            {
                // [ invalida dados de todas as empresas ]
                foreach (string empresa in CSConfiguracao.GetEmpresas())
                    CSConfiguracao.SetConfig("ATUALIZADO_" + empresa.Substring(0, 8), CSGlobal.STATUS_ATUALIZACAO.ATUALIZACAO_VERSAO);

                CSConfiguracao.SetConfig("VERSAO_AVANTE", GetCurrentVersion());
            }

            // [ Verifica se esta utilizando Cartão de Memoria para banco de dados ]
            if (CSConfiguracao.GetConfig("StoredCard").Trim() != "")
                CSGlobal.UtilizarStorageCard = bool.Parse(CSConfiguracao.GetConfig("StoredCard"));



        }
        public enum CpuInstructionSet
        {
            X86 = PROCESSOR_X86_32BIT_INSTRUCTION,
            SH3 = PROCESSOR_HITACHI_SH3_INSTRUCTION,
            SH4 = PROCESSOR_HITACHI_SH4_INSTRUCTION,
            MIPSV4 = PROCESSOR_MIPS_MIPSIV_INSTRUCTION,
            MIPSV4_FP = PROCESSOR_MIPS_MIPSIVFP_INSTRUCTION,
            MIPSVII = PROCESSOR_MIPS_MIPSII_INSTRUCTION,
            MIPSVII_FP = PROCESSOR_MIPS_MIPSIIFP_INSTRUCTION,
            MIPS16 = PROCESSOR_MIPS_MIPS16_INSTRUCTION,
            ARMV4 = PROCESSOR_ARM_V4_INSTRUCTION,
            ARMV4T = PROCESSOR_ARM_V4T_INSTRUCTION,
        }

        #endregion

        #region [ FlexX GPS ]

        // [ Limpa arquivos já processados do FlexX GPS ]
        public static void LimpaArquivosFlexxGPS(string diretorioOrigem, string arquivosOrigem)
        {
            try
            {
                if (System.IO.Directory.Exists(diretorioOrigem) && arquivosOrigem.Trim() != "")
                {
                    DirectoryInfo diretorio = new DirectoryInfo(diretorioOrigem);

                    // Carrega todos os arquivos texto do diretorio origem
                    FileInfo[] arquivos = diretorio.GetFiles(arquivosOrigem);

                    foreach (FileInfo arquivo in arquivos)
                    {
                        // Excluir os arquivo exitem no diretorio origem.
                        File.Delete(Path.Combine(diretorioOrigem, arquivo.Name.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }
        }

        public static string GetLongitudeFlexxGPS()
        {
            string longitude = string.Empty;

            try
            {
                if (File.Exists("/sdcard/FLAGPS_BD/POSATUAL.TXT"))
                {
                    FileStream arquivo = new FileStream("/sdcard/FLAGPS_BD/POSATUAL.TXT", FileMode.OpenOrCreate);

                    StreamReader sr = new StreamReader(arquivo);
                    longitude = sr.ReadLine();
                    sr.Close();
                    arquivo.Dispose();

                    if (longitude.Trim() != "")
                        longitude = (string)longitude.Split(';').GetValue(3);
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }

            return longitude;
        }

        public static string GetDataUltimaLocalizacaoGPS()
        {
            string dataCompleta = string.Empty;

            try
            {
                if (File.Exists("/sdcard/FLAGPS_BD/POSATUAL.TXT"))
                {
                    FileStream arquivo = new FileStream("/sdcard/FLAGPS_BD/POSATUAL.TXT", FileMode.OpenOrCreate);

                    StreamReader sr = new StreamReader(arquivo);
                    var arquivoTxt = sr.ReadLine();
                    string horaNaoFormatada = string.Empty;
                    string dataNaoFormatada = string.Empty;
                    string horaFormatada = string.Empty;
                    string dataFormatada = string.Empty;
                    sr.Close();

                    if (arquivoTxt.Trim() != "")
                    {
                        horaNaoFormatada = (string)arquivoTxt.Split(';').GetValue(1);

                        string horas = horaNaoFormatada.Substring(0, 2);
                        string minutos = horaNaoFormatada.Substring(2, 2);

                        horaFormatada = string.Format("{0}:{1}", horas, minutos);

                        dataNaoFormatada = (string)arquivoTxt.Split(';').GetValue(0);

                        string ano = dataNaoFormatada.Substring(0, 4);
                        string mes = dataNaoFormatada.Substring(4, 2);
                        string dia = dataNaoFormatada.Substring(6, 2);

                        dataFormatada = string.Format("{0}/{1}/{2}", dia, mes, ano);

                        dataCompleta = string.Format("{0} {1}", dataFormatada, horaFormatada);
                    }
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }

            return dataCompleta;
        }

        public static string GetLatitudeFlexxGPS()
        {
            string latitude = string.Empty;

            try
            {
                if (File.Exists("/sdcard/FLAGPS_BD/POSATUAL.TXT"))
                {
                    FileStream arquivo = new FileStream("/sdcard/FLAGPS_BD/POSATUAL.TXT", FileMode.OpenOrCreate);

                    StreamReader sr = new StreamReader(arquivo);
                    latitude = sr.ReadLine();
                    sr.Close();

                    if (latitude.Trim() != "")
                        latitude = (string)latitude.Split(';').GetValue(2);
                }
            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }

            return latitude;
        }

        // [ Retorna localização para o FlexX GPS ]
        public static string GetLocalizacaoFlexXGPS()
        {
            string localizacao = " | ";

            try
            {

#if ANDROID
                if (File.Exists("/sdcard/FLAGPS_BD/POSATUAL.TXT"))
                {
                    FileStream arquivo = new FileStream("/sdcard/FLAGPS_BD/POSATUAL.TXT", FileMode.OpenOrCreate);
#else
                if (File.Exists(@"\FLAGPS_BD\POSATUAL.TXT"))
                {
                    FileStream arquivo = new FileStream(@"\FLAGPS_BD\POSATUAL.TXT", FileMode.OpenOrCreate);
#endif
                    StreamReader sr = new StreamReader(arquivo);
                    localizacao = sr.ReadLine();
                    sr.Close();
                    arquivo.Dispose();

                    if (localizacao.Trim() != "")
                        localizacao = (string)localizacao.Split(';').GetValue(2) + "|" + (string)localizacao.Split(';').GetValue(3);
                    else
                        localizacao = " | ";
                }

            }
            catch (Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }

            return localizacao;

        }

        // [ Cria diretorios padrões do FlexX GPS ]
        public static void criaDiretoriosFlexxGPS()
        {
#if ANDROID
            // Verificar se existe os diretorios do Flexx GPS            
            if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD"))
            {
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIAR");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIADOS");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIAR_GPS");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIADOS_GPS");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/AENVIAR");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/ENVIADOS");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/RECEBIDOS");
                System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/LIDOS");
            }
            else
            {
                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/ENVIAR"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIAR");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/ENVIADOS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIADOS");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/ENVIAR_GPS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIAR_GPS");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/ENVIADOS_GPS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/ENVIADOS_GPS");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/MENSAGEM"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/MENSAGEM/AENVIAR"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/AENVIAR");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/MENSAGEM/ENVIADOS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/ENVIADOS");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/MENSAGEM/RECEBIDOS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/RECEBIDOS");

                if (!System.IO.Directory.Exists("/sdcard/FLAGPS_BD/MENSAGEM/LIDOS"))
                    System.IO.Directory.CreateDirectory("/sdcard/FLAGPS_BD/MENSAGEM/LIDOS");
            }
#else
            // Verificar se existe os diretorios do Flexx GPS
            if (!System.IO.Directory.Exists(@"\FLAGPS_BD"))
            {
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIAR");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIADOS");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIAR_GPS");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIADOS_GPS");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\AENVIAR");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\ENVIADOS");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\RECEBIDOS");
                System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\LIDOS");
            }
            else
            {
                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\ENVIAR"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIAR\");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\ENVIADOS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIADOS\");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\ENVIAR_GPS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIAR_GPS");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\ENVIADOS_GPS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\ENVIADOS_GPS");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\MENSAGEM"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\MENSAGEM\AENVIAR"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\AENVIAR\");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\MENSAGEM\ENVIADOS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\ENVIADOS");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\MENSAGEM\RECEBIDOS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\RECEBIDOS");

                if (!System.IO.Directory.Exists(@"\FLAGPS_BD\MENSAGEM\LIDOS"))
                    System.IO.Directory.CreateDirectory(@"\FLAGPS_BD\MENSAGEM\LIDOS");
            }
#endif
        }


        // [ Gerar o arquivo de CONFIG.TXT para o FlexX GPS ]
#if ANDROID
        public static void GetCriaArquivoConfigFlexXGPS(Context context)
#else
        public static void GetCriaArquivoConfigFlexXGPS()
#endif
        {
            try
            {
#if ANDROID
                if (File.Exists("/sdcard/FLAGPS_BD/CONFIG.TXT"))
                    File.Delete("/sdcard/FLAGPS_BD/CONFIG.TXT");
                FileStream arquivo = new FileStream("/sdcard/FLAGPS_BD/CONFIG.TXT", FileMode.OpenOrCreate);
                StreamWriter sr = new StreamWriter(arquivo);
                sr.WriteLine("IMEI=" + GetDeviceId(context));
#else
                if (File.Exists(@"\FLAGPS_BD\CONFIG.TXT"))
                    File.Delete(@"\FLAGPS_BD\CONFIG.TXT");

                FileStream arquivo = new FileStream(@"\FLAGPS_BD\CONFIG.TXT", FileMode.OpenOrCreate);
                StreamWriter sr = new StreamWriter(arquivo);
#endif
                sr.WriteLine("ID_VEND=" + CSEmpresa.Current.CODIGO_REVENDA.ToString().Trim() + CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA));
                sr.WriteLine("ID_EMP=" + CSEmpresa.Current.CODIGO_REVENDA.ToString());
                sr.WriteLine("TEMPO_LEITURA=" + CSEmpresa.Current.TEMPO_LEITURA.ToString());
                sr.WriteLine("TEMPO_LEITURA_DIST=20");
                sr.WriteLine("TEMPO_TRANS=" + CSEmpresa.Current.TEMPO_TRANSFERENCIA.ToString());
                sr.WriteLine("STR_CONECT=" + CSEmpresa.Current.CONEXAO_FLEXX_GPS.Trim().ToLower());
                sr.WriteLine("SISTEMA_FORCA=AVANTE");
#if ANDROID
                sr.WriteLine("SISTEMA_FORCA_PATH=");
#else
                sr.WriteLine("SISTEMA_FORCA_PATH=" + GetCurrentDirectory().Trim() + @"/Avante Sales.exe");
#endif

                sr.WriteLine("HOT_HOST_PATH=");

                sr.Close();
            }
            catch (System.Exception ex)
            {
                CSGlobal.ShowMessage(ex.ToString());
            }

        }

#if ANDROID
        public static string GetDeviceId(Context context)
        {
            TelephonyManager telephonyMgr = context.GetSystemService(Context.TelephonyService) as TelephonyManager;

            string deviceId = telephonyMgr.DeviceId == null ? "UNAVAILABLE" : telephonyMgr.DeviceId;

            return deviceId;
        }
#endif

        // [ Verifica se tem mensagem recebida ]
        public static Boolean GetRecebeuMensagem()
        {
            Boolean abreMensagem = false;
#if ANDROID
            string diretorioOrigem = "/sdcard/FLAGPS_BD/MENSAGEM/RECEBIDOS";
#else
            string diretorioOrigem = @"\FLAGPS_BD\MENSAGEM\RECEBIDOS";
#endif

            if (System.IO.Directory.Exists(diretorioOrigem))
            {
                DirectoryInfo diretorio = new DirectoryInfo(diretorioOrigem);

                // Carrega todos os arquivos texto do diretorio origem
                FileInfo[] arquivos = diretorio.GetFiles("*.txt");

                abreMensagem = arquivos.GetLength(0) != 0;

            }

            return abreMensagem;
        }

        #endregion

        public static string DiretorioImagem()
        {
            return "/sdcard/FLAGPS_BD/IMAGEM";
        }

        public static string DiretorioLog()
        {
            return Path.Combine(GetCurrentDirectoryDB(), "Log");
        }

        public static void criaDiretorioLogErro()
        {
            string diretorioLog = DiretorioLog();

            if (!Directory.Exists(diretorioLog))
            {
                Directory.CreateDirectory(diretorioLog);
            }
        }
    }
}