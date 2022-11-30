 
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic

public partial class OfficeApp
{

    private static bool mbAccessInstalled;
    private static bool mbExcelInstalled;
    private static bool mbOutlookInstalled;
    private static bool mbPowerPointInstalled;
    private static bool mbWordInstalled;
    private static double mdMinimumVersion;
    // Private mhWnd As Long

    public static double MinimumVersion
    {
        get
        {
            return mdMinimumVersion;
        }
        set
        {
            mdMinimumVersion = value;
        }
    }

    public static bool AccessInstalled
    {
        get
        {
            return mbAccessInstalled;
        }
        set
        {
            mbAccessInstalled = value;
        }
    }

    public static bool ExcelInstalled
    {
        get
        {
            return mbExcelInstalled;
        }
        set
        {
            mbExcelInstalled = value;
        }
    }

    public static bool OutlookInstalled
    {
        get
        {
            return mbOutlookInstalled;
        }
        set
        {
            mbOutlookInstalled = value;
        }
    }

    public static bool PowerPointInstalled
    {
        get
        {
            return mbPowerPointInstalled;
        }
        set
        {
            mbPowerPointInstalled = value;
        }
    }


    public static bool WordInstalled
    {
        get
        {
            return mbWordInstalled;
        }
        set
        {
            mbWordInstalled = value;
        }
    }

    // Public Property Let Handle(ByVal lWnd As Long)
    // mhWnd = lWnd
    // End Property

    // Public Property Let MinimumVersion(ByVal dNewValue As Double)
    // mdMinimumVersion = dNewValue
    // End Property

    // Public Property Get AccessInstalled() As Boolean
    // AccessInstalled = mbAccessInstalled
    // End Property

    // Public Property Get ExcelInstalled() As Boolean
    // ExcelInstalled = mbExcelInstalled
    // End Property

    // Public Property Get OutlookInstalled() As Boolean
    // OutlookInstalled = mbOutlookInstalled
    // End Property

    // Public Property Get PowerPointInstalled() As Boolean
    // PowerPointInstalled = mbPowerPointInstalled
    // End Property

    // Public Property Get WordInstalled() As Boolean
    // WordInstalled = mbWordInstalled
    // End Property

    // Public Shared Sub DemoRegistryKeys()
    // Dim regVersion As RegistryKey
    // Dim keyValue As String = "Software\Microsoft\Office\12.0\"
    // regVersion = Registry.CurrentUser.OpenSubKey(keyValue, False)
    // If (Not regVersion Is Nothing) Then
    // lRC = RegEnumKeyEx(hKey, lRegIndex, ByVal sValue, lValueLen, 0&, ByVal sData, lDataLen, tTime)
    // Dim intVersion = regVersion.GetValue("Excel")
    // 'For Each Str As String In intVersion
    // '    Dim val = intVersion(Str)
    // 'Next
    // 'GetValue("WinXPLanguagePatch", 0)
    // regVersion.Close()
    // End If
    // End Sub
    // Registry constants
    public const long ERROR_SUCCESS = 0L;
    public const int HKEY_CLASSES_ROOT = int.MinValue + 0x00000000;
    public const int HKEY_LOCAL_MACHINE = int.MinValue + 0x00000002;
    public const int HKEY_CURRENT_USER = int.MinValue + 0x00000001;
    public const int STANDARD_RIGHTS_ALL = 0x1F0000;
    public const int KEY_QUERY_VALUE = 0x1;
    public const int KEY_SET_VALUE = 0x2;
    public const int KEY_CREATE_SUB_KEY = 0x4;
    public const int KEY_ENUMERATE_SUB_KEYS = 0x8;
    public const int KEY_NOTIFY = 0x10;
    public const int KEY_CREATE_LINK = 0x20;
    public const int SYNCHRONIZE = 0x100000;
    public const int KEY_ALL_ACCESS = (STANDARD_RIGHTS_ALL | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY | KEY_CREATE_LINK) & ~SYNCHRONIZE;
    public const int BLOCK_SIZE = 30;
    public const int REG_CREATED_NEW_KEY = 0x1;
    public const int REG_SZ = 1;
    public const int REG_DWORD = 4;

    public const string OFFICEKEY = @"Software\Microsoft\Office\";


    [DllImport("advapi32.dll", EntryPoint = "RegOpenKeyA")]
    public static extern int RegOpenKeyEx(int hKey, string lpSubKey, ref IntPtr phkResult);




    [DllImport("advapi32.dll", EntryPoint = "RegEnumKeyExA")]
    public static extern int RegEnumKeyEx(IntPtr hKey, int dwIndex, string lpValueName, ref int lpcValueName, IntPtr lpReserved, IntPtr lpType, string lpData, IntPtr lpcbData);









    [DllImport("advapi32.dll")]
    public static extern int RegCloseKey(UIntPtr hKey);


    // Public Declare Function RegOpenKeyEx Lib "advapi32.dll" Alias "RegOpenKeyExA" (ByVal hKey As Long, ByVal lpSubKey As String, ByVal ulOptions As Long, ByVal samDesired As Long, phkResult As Long) As Long
    // Public Declare Function RegEnumKeyEx Lib "advapi32.dll" Alias "RegEnumKeyExA" (ByVal hKey As Integer, ByVal dwIndex As Integer, ByVal lpName As String, ByRef lpcbName As Integer, ByVal lpReserved As Integer, ByVal lpReserved As Integer, ByVal lpClass As String, ByRef lpcbClass As Integer, ByRef lpftLastWriteTime As System.Runtime.InteropServices.ComTypes.FILETIME) As Integer
    // Public Declare Function RegCloseKey Lib "advapi32.dll" (ByVal hKey As Long) As Long

    public static string EnumRegistryKeys(string sKey, long lLevel)
    {
        string EnumRegistryKeysRet = default;
        var hKey = default(long);
        long lIndex;
        long lRC;
        long lRegIndex;
        string sData;
        string sResult;
        string sValue;
        string sText = "";
        string sSubText;
        Collection cSubKeys;
        const long Buffer = 2000L;
        long lDataLen;
        long lValueLen;
        try
        {
            if (lLevel > 2L | mbAccessInstalled & mbWordInstalled & mbExcelInstalled & mbOutlookInstalled)
            {
                EnumRegistryKeysRet = "";
                return EnumRegistryKeysRet;
            }
            int localRegOpenKeyEx() { IntPtr argphkResult = (IntPtr)hKey; var ret = OfficeApp.RegOpenKeyEx(HKEY_CURRENT_USER, sKey, ref argphkResult); hKey = (long)argphkResult; return ret; }

            if (localRegOpenKeyEx() != ERROR_SUCCESS)
            {
                EnumRegistryKeysRet = "";
                return EnumRegistryKeysRet;
            }

            // If (RegOpenKeyEx(HKEY_CURRENT_USER, sKey, 0&, KEY_ALL_ACCESS, hKey) <> ERROR_SUCCESS) Then
            // Exit Function
            // End If
            lRegIndex = 0L;
            cSubKeys = new Collection();
            do
            {
                sValue = Strings.Space((int)Buffer);
                lValueLen = Buffer;
                sData = Strings.Space((int)Buffer);
                lDataLen = Buffer;
                // lRC = RegEnumKeyEx(hKey, lRegIndex, ByVal sValue, lValueLen, 0&, ByVal sData, lDataLen, tTime)
                int arglpcValueName = (int)lValueLen;
                lRC = OfficeApp.RegEnumKeyEx((IntPtr)hKey, (int)lRegIndex, sValue, ref arglpcValueName, (IntPtr)0, (IntPtr)0L,  sData, (IntPtr)lDataLen);
                lValueLen = arglpcValueName;
                lRegIndex = lRegIndex + 1L;

                if (lRC == ERROR_SUCCESS)
                {
                    sResult = Strings.Left(sValue, (int)lValueLen);
                    if (mdMinimumVersion <= 0d | lLevel > 1L)
                    {
                        cSubKeys.Add(sResult);
                    }
                    else if (lLevel == 1L & Information.IsNumeric(sResult))
                    {
                        if (Conversions.ToDouble(sResult) >= mdMinimumVersion)
                            cSubKeys.Add(sResult);
                    }

                    if (lLevel == 1L & !Information.IsNumeric(sResult))
                    {
                        break;
                    }
                }
            }
            while (lRC == ERROR_SUCCESS);

            RegCloseKey((UIntPtr)hKey);

            if (lLevel == 1L)
            {
                var loopTo = (long)cSubKeys.Count;
                for (lIndex = 1L; lIndex <= loopTo; lIndex++)
                {
                    if (Information.IsNumeric(cSubKeys[lIndex]))
                    {
                        sSubText = EnumRegistryKeys(Conversions.ToString(Operators.ConcatenateObject(GetPathWithSlash(sKey), cSubKeys[lIndex])), lLevel + 1L);
                        sText = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sText, cSubKeys[lIndex]), sSubText));
                    }
                }
            }
            else
            {
                var loopTo1 = (long)cSubKeys.Count;
                for (lIndex = 1L; lIndex <= loopTo1; lIndex++)
                {
                    if (Strings.StrComp(Conversions.ToString(cSubKeys[lIndex]), "access", Constants.vbTextCompare) == 0L)
                        mbAccessInstalled = true;
                    if (Strings.StrComp(Conversions.ToString(cSubKeys[lIndex]), "word", Constants.vbTextCompare) == 0L)
                        mbWordInstalled = true;
                    if (Strings.StrComp(Conversions.ToString(cSubKeys[lIndex]), "excel", Constants.vbTextCompare) == 0L)
                        mbExcelInstalled = true;
                    if (Strings.StrComp(Conversions.ToString(cSubKeys[lIndex]), "outlook", Constants.vbTextCompare) == 0L)
                        mbOutlookInstalled = true;

                    if (Strings.StrComp(Conversions.ToString(cSubKeys[lIndex]), "powerpoint", Constants.vbTextCompare) == 0L)
                    {
                        // Until we can figure out what's up with PowerPoint DO NOT uncomment. RVW 08/13/2007
                        mbPowerPointInstalled = false;
                    }

                    sText = Conversions.ToString(Operators.ConcatenateObject(sText, cSubKeys[lIndex]));
                }
            }

            cSubKeys = null;
            EnumRegistryKeysRet = sText;
            return EnumRegistryKeysRet;
        }
        catch (Exception ex)
        {
            throw ex.InnerException;
        }
    }

    public static string GetPathWithSlash(string sPath)
    {
        string GetPathWithSlashRet = default;
        sPath = Strings.Trim(sPath);
        GetPathWithSlashRet = sPath;
        if (Strings.InStrRev(sPath, @"\") != Strings.Len(sPath) & Strings.Len(sPath) > 0)
            GetPathWithSlashRet = sPath + @"\";
        return GetPathWithSlashRet;
    }
}