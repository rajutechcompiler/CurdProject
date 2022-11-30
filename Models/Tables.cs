using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.Security;
using TabFusionRMS.Models;

namespace TabFusionRMS.WebCS
{

    public sealed class Tables
    {

        public static bool SaveNewFieldToTable(string sTableName, string sFieldName, Enums.DataTypeEnum FieldType, Databas pDatabaseEntity, int iViewsId, Passport passport)
        {
            string sSQL;

            var sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, null);
            if (pDatabaseEntity is not null)
            {
                sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDatabase, pDatabaseEntity);
            }

            sFieldName = Strings.Replace(sFieldName, "* ", "");
            sSQL = "ALTER TABLE [" + sTableName + "]";

            switch (FieldType)
            {
                case Enums.DataTypeEnum.rmDate:
                case Enums.DataTypeEnum.rmDBDate:
                case Enums.DataTypeEnum.rmDBTime:
                    {
                        sSQL = sSQL + " ADD [" + Strings.Trim(sFieldName) + "] DATETIME NULL";
                        break;
                    }
                case Enums.DataTypeEnum.rmBoolean:
                    {
                        sSQL = sSQL + " ADD [" + Strings.Trim(sFieldName) + "] BIT";
                        break;
                    }

                default:
                    {
                        sSQL = sSQL + " ADD [" + Strings.Trim(sFieldName) + "] VARCHAR(20) NULL";
                        break;
                    }
            }

            // Remove Existing view form sql
            ViewModel.SQLViewDelete(iViewsId, passport);

            try
            {
                return Conversions.ToInteger(DataServices.ProcessADOCommand(ref sSQL, sADOConn, false)) > -1;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static string GenerateKey(bool boolFlag, string pwdString = null, byte[] pwdByteArray = null)
        {
            string transformPwd;
            try
            {
                using (var myRijndael = new RijndaelManaged())
                {
                    var pdb = new Rfc2898DeriveBytes("RandomKey", Encoding.ASCII.GetBytes("SaltValueMustBeUnique"));
                    myRijndael.Key = pdb.GetBytes(32);
                    myRijndael.IV = pdb.GetBytes(16);
                    if (boolFlag)
                    {
                        transformPwd = EncryptString(pwdString, myRijndael.Key, myRijndael.IV);
                    }
                    else
                    {
                        transformPwd = DecryptString(pwdByteArray, myRijndael.Key, myRijndael.IV);
                    }
                    return transformPwd;
                }
            }
            catch (Exception ex)
            {
                return Conversions.ToString(false);
            }
        }

        public static string EncryptString(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText is null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
            if (Key is null || Key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }
            if (IV is null || IV.Length <= 0)
            {
                throw new ArgumentNullException("IV");
            }
            try
            {
                using (var rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;
                    var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                    var msEncrypt = new System.IO.MemoryStream();
                    // Using msEncrypt As New IO.MemoryStream()
                    var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                    // Using csEncrypt As New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Encoding.Default.GetString(msEncrypt.ToArray());
                    // End Using
                    // End Using
                }
            }
            catch (Exception ex)
            {
                return Conversions.ToString(false);
            }

        }

        public static string DecryptString(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText is null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (Key is null || Key.Length <= 0)
            {
                throw new ArgumentNullException("Key");
            }
            if (IV is null || IV.Length <= 0)
            {
                throw new ArgumentNullException("IV");
            }
            try
            {
                string plaintext = null;
                using (var rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;
                    var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                    var msDecrypt = new System.IO.MemoryStream(cipherText);
                    // Using msDecrypt As New IO.MemoryStream(cipherText)
                    var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                    // Using csDecrypt As New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)
                    using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                    // End Using
                    // End Using
                }
                return plaintext;
            }
            catch (Exception ex)
            {
                return Convert.ToString(false);
            }

        }
    }
}


// Public Function VerifyRetentionDispositionTypesForParentAndChildren(ByVal oCurrentTable As SRME.Tables) As String
// Dim oTable As SRME.Tables
// Dim sMessage As String = String.Empty

// Try
// If (oCurrentTable.RetentionFinalDisposition <> Tables.meFinalDisposition.fdNone) Then
// 'First check Parent tables
// For Each oRelationship As Relationships In oCurrentTable.ParentTables
// oTable = DirectCast(moApp.Tables.Item(oRelationship.UpperTableName), SRME.Tables)

// If (oTable IsNot Nothing) Then
// If (((oTable.RetentionPeriodActive) Or (oTable.RetentionInactivityActive)) And (oTable.RetentionFinalDisposition <> Tables.meFinalDisposition.fdNone)) Then
// If (oTable.RetentionFinalDisposition <> oCurrentTable.RetentionFinalDisposition) Then sMessage = vbTab & vbTab & oTable.UserName & vbCrLf
// End If
// oTable = Nothing
// End If
// Next oRelationship

// 'Next check child tables
// For Each oRelationship As Relationships In oCurrentTable.ChildTables
// oTable = DirectCast(moApp.Tables.Item(oRelationship.LowerTableName), SRME.Tables)

// If (oTable IsNot Nothing) Then
// If (((oTable.RetentionPeriodActive) Or (oTable.RetentionInactivityActive)) And (oTable.RetentionFinalDisposition <> Tables.meFinalDisposition.fdNone)) Then
// If (oTable.RetentionFinalDisposition <> oCurrentTable.RetentionFinalDisposition) Then sMessage = vbTab & vbTab & oTable.UserName & vbCrLf
// End If
// oTable = Nothing
// End If
// Next oRelationship

// If (sMessage > "") Then
// sMessage = "WARNING:  The following related tables have a retention disposition " & vbCrLf & _
// "set differently than this table:" & vbCrLf & vbCrLf & _
// sMessage & vbCrLf & vbCrLf & _
// "This could give different results than expected." & vbCrLf & vbCrLf & _
// "Please correct the appropriate table if this is not what is intended."
// End If
// End If
// Catch ex As Exception
// sMessage = String.Empty
// End Try

// Return sMessage
// End Function

// Public Sub UpdateDestructionRecordsWithNewDispositionType(ByVal oTable As SRME.Tables, ByVal oldType As SRME.Tables.meFinalDisposition, ByVal newType As SRME.Tables.meFinalDisposition)
// Dim bUpdatedCertItem As Boolean
// Dim oSLDestructCertItems As SRME.SLDestructCertItems
// Dim oSLDestructCert As SRME.SLDestructionCerts
// Dim sSQL As String
// Dim rsDestructCerts As RMAG.RecordSet

// If ((oldType <> Tables.meFinalDisposition.fdNone) And (oldType <> newType)) Then
// moApp.SLDestructionCerts.Load()
// moApp.SLDestructCertItems.Load()

// For Each oSLDestructCert In moApp.SLDestructionCerts
// bUpdatedCertItem = False
// If ((oSLDestructCert.DispositionType = oldType) And (oSLDestructCert.DateDestroyed <= Date.MinValue)) Then
// For Each oSLDestructCertItems In moApp.SLDestructCertItems
// If ((oSLDestructCertItems.SLDestructionCertsId = oSLDestructCert.Id) And (StrComp(oSLDestructCertItems.TableName, oTable.TableName, vbTextCompare) = 0) And (Not oSLDestructCertItems.DispositionDate > Date.MinValue)) Then
// moApp.SLDestructCertItems.DeleteRecord(oSLDestructCertItems.Id)
// bUpdatedCertItem = True
// End If
// Next oSLDestructCertItems

// If (bUpdatedCertItem) Then
// sSQL = "SELECT * FROM [slDestructCertItems] WHERE ([SLDestructionCertsId] = " & oSLDestructCert.Id & ") AND (([DispositionDate] = 0) OR ([DispositionDate] IS NULL))"

// Try
// rsDestructCerts = moApp.Data.GetADORecordset(sSQL, "SLDestructCertItems")

// If (Not (rsDestructCerts Is Nothing)) Then
// If ((rsDestructCerts.BOF) And (rsDestructCerts.EOF)) Then
// sSQL = "SELECT * FROM [slDestructCertItems] WHERE ([SLDestructionCertsId] = " & oSLDestructCert.Id & ") AND ([DispositionDate] IS NOT NULL)"

// Try
// rsDestructCerts = moApp.Data.GetADORecordset(sSQL, "SLDestructCertItems")
// If ((rsDestructCerts.BOF) And (rsDestructCerts.EOF)) Then
// moApp.SLDestructionCerts.DeleteRecord(oSLDestructCert.Id)
// Else
// oSLDestructCert.DateDestroyed = Now
// moApp.SLDestructionCerts.Save(oSLDestructCert)
// End If
// Catch ex As Exception
// End Try
// End If
// rsDestructCerts.Close()
// rsDestructCerts = Nothing
// Else
// moApp.SLDestructionCerts.DeleteRecord(oSLDestructCert.Id)
// End If
// Catch ex As Exception
// End Try
// End If
// End If
// Next oSLDestructCert
// End If
// End Sub
