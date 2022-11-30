using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;

public class AzureADConfiguration
{
    public bool SaveAzureADAdminSettings(string azureAdAdminUser, bool isAdEnabled)
    {
        var _iSetting = new Repositories<Setting>();
        var azureADAdmin = _iSetting.All().Where(s => s.Item.Equals("AzureAdAdmin") & s.Section.Equals("AzureAD")).FirstOrDefault();
        if (azureADAdmin == null)
        {
            azureADAdmin = new Setting();
            azureADAdmin.Section = "AzureAD";
            azureADAdmin.Item = "AzureAdAdmin";
            azureADAdmin.ItemValue = Conversions.ToString(Interaction.IIf(isAdEnabled, azureAdAdminUser.Trim(), ""));
            _iSetting.Add(azureADAdmin);
        }
        else
        {
            azureADAdmin.ItemValue = Conversions.ToString(Interaction.IIf(isAdEnabled, azureAdAdminUser.Trim(), ""));
            _iSetting.Update(azureADAdmin);
        }

        var azureADEnabled = _iSetting.All().Where(s => s.Item.Equals("EnabledAzureAD") & s.Section.Equals("AzureAD")).FirstOrDefault();
        if (azureADEnabled == null)
        {
            azureADEnabled = new Setting();
            azureADEnabled.Section = "AzureAD";
            azureADEnabled.Item = "EnabledAzureAD";
            azureADEnabled.ItemValue = Conversions.ToString(Interaction.IIf(isAdEnabled == true, "true", "false"));
            _iSetting.Add(azureADEnabled);
        }
        else
        {
            azureADEnabled.ItemValue = Conversions.ToString(Interaction.IIf(isAdEnabled == true, "true", "false"));
            _iSetting.Update(azureADEnabled);
        }
        return true;
    }
    public int SaveMappedUser(string azureAdAdminUser, string fullName)
    {
        int userId = 0;
        var _iSecureUser = new Repositories<SecureUser>();
        var secureUser = _iSecureUser.All().Where(x => (x.UserName.Trim().ToLower() ?? "") == (azureAdAdminUser.Trim().ToLower() ?? "")).FirstOrDefault();
        if (secureUser == null)
        {
            secureUser = new SecureUser();
            secureUser.PasswordHash = " ";
            secureUser.AccountType = AzureADSettings.secureUserType;  // Z' for Azure AD Users
            secureUser.PasswordUpdate = DateTime.Now;
            secureUser.MustChangePassword = false;
            secureUser.FullName = Conversions.ToString(Interaction.IIf(fullName is null, "", fullName));
            secureUser.Email = azureAdAdminUser;
            secureUser.AccountDisabled = false;
            secureUser.DisplayName = Conversions.ToString(Interaction.IIf(fullName is null, azureAdAdminUser, fullName));
            secureUser.UserName = azureAdAdminUser;

            _iSecureUser.Add(secureUser);
            userId = secureUser.UserID;
        }
        else
        {
            userId = secureUser.UserID;
        }
        return userId;
    }

    public bool SaveUserGroups(int userId, int fusionGroupId)
    {
        var _iSecureUserGroup = new Repositories<SecureUserGroup>();
        var secureUserGroups = new List<SecureUserGroup>();
        try
        {
            // Add default group for sync user
            var secureUserDefaultGroup = _iSecureUserGroup.Where(s => s.GroupID == AzureADSettings.userDefaultGroupId && s.UserID == userId).FirstOrDefault();
            if (secureUserDefaultGroup == null)
            {
                var pSecureUserGroup = new SecureUserGroup();
                pSecureUserGroup.UserID = userId;
                pSecureUserGroup.GroupID = AzureADSettings.userDefaultGroupId;
                secureUserGroups.Add(pSecureUserGroup);
                // _iSecureUserGroup.Add(pSecureUserGroup)
            }
            var secureUserGroup = _iSecureUserGroup.Where(s => s.GroupID == fusionGroupId && s.UserID == userId).FirstOrDefault();
            if (secureUserGroup == null)
            {
                var pSecureUserGroup = new SecureUserGroup();
                pSecureUserGroup.UserID = userId;
                pSecureUserGroup.GroupID = fusionGroupId;
                secureUserGroups.Add(pSecureUserGroup);
                // _iSecureUserGroup.Add(pSecureUserGroup)
            }

            if (secureUserGroups.Count > 0)
            {
                _iSecureUserGroup.AddList(secureUserGroups);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool SaveMappedGroups(int fusionRMSGroupId, string adGroupName)
    {
        var _iSecureGroup = new Repositories<SecureGroup>();
        var azureGroupMapping = _iSecureGroup.Where(s => s.GroupID == fusionRMSGroupId).FirstOrDefault();
        if (!(azureGroupMapping == null))
        {
            azureGroupMapping.ActiveDirectoryGroup = adGroupName.Trim();
            _iSecureGroup.Update(azureGroupMapping);
        }
        else
        {
            return false;
        }
        return true;
    }

    public bool DeleteMappedGroups(int fusionRMSGroupId)
    {
        var _iSecureGroup = new Repositories<SecureGroup>();
        var azureGroupMapping = _iSecureGroup.Where(s => s.GroupID == fusionRMSGroupId).FirstOrDefault();
        if (!(azureGroupMapping == null))
        {
            azureGroupMapping.ActiveDirectoryGroup = string.Empty;
            _iSecureGroup.Update(azureGroupMapping);
            return true;
        }
        return false;
    }

    public List<GroupsMappingModel> GetAzureADGroupMappings()
    {
        var _iSecureGroup = new Repositories<SecureGroup>();
        var secureGroups = _iSecureGroup.All().Where(x => !string.IsNullOrEmpty(x.ActiveDirectoryGroup.Trim())).ToList();
        var groupMappingModel = new List<GroupsMappingModel>();
        if (secureGroups.Count > 0)
        {
            groupMappingModel = secureGroups.Select(x => new GroupsMappingModel()
            {
                GroupID = x.GroupID,
                GroupName = (String.IsNullOrEmpty(x.ActiveDirectoryGroup) ? "": x.ActiveDirectoryGroup) + " => " + x.GroupName
            }).ToList();
        }
        return groupMappingModel;
    }

    public bool SyncUsers(string token)
    {
        try
        {
            var _iSecureGroup = new Repositories<SecureGroup>();
            var secureGroupsADMapped = _iSecureGroup.All().Where(x => !string.IsNullOrEmpty(x.ActiveDirectoryGroup.Trim())).ToList();

            // Get Azure Groups Mapped from the Database (Distinct)
            var azureADGroupsFromDB = secureGroupsADMapped.Select(x => x.ActiveDirectoryGroup).Distinct();
            // Get Azure Groups for get IDs
            var azureADGroups = GetGroups(token);
            var azureADNewWithIds = new List<AzureADGroupsModel>();
            // Filtered group which is into the database
            foreach (string adGroupName in azureADGroupsFromDB)
            {
                var azureAdGroup = azureADGroups.Where(x => (x.DisplayName.ToLower() ?? "") == (adGroupName.Trim().ToLower() ?? "")).FirstOrDefault();
                if (!(adGroupName == null))
                {
                    azureADNewWithIds.Add(azureAdGroup);
                }
            }
            if (azureADNewWithIds.Count <= 0)
            {
                return false;
            }
            // Loop on new object for required to sync
            foreach (AzureADGroupsModel azureADGroup in azureADNewWithIds)
            {
                // Graph API call to get the members from the groups
                var adGroupMembers = new List<AzureADGroupUsersModel>();
                string url = string.Format(AzureADSettings.urlGroupMembers, azureADGroup.GroupId);
                GetGroupMembers(token, url, ref adGroupMembers);
                if (adGroupMembers.Count > 0)
                {
                    // Find if Azure AD groups mapped with multiple Fusion Groups
                    var allFusionGroupIdsForThisGroup = secureGroupsADMapped.Where(x => (x.ActiveDirectoryGroup.Trim().ToLower() ?? "") == (azureADGroup.DisplayName.ToLower() ?? "")).Select(x => x.GroupID).Distinct();

                    // Loop through all the mebers received from the Azure Graph API call
                    foreach (AzureADGroupUsersModel user in adGroupMembers)
                    {

                        // Save User which received from the Azure Group Members
                        int userId = this.SaveMappedUser(user.Email, user.DisplayName);
                        if (userId > 0)
                        {
                            foreach (int fusionGroupId in allFusionGroupIdsForThisGroup)
                                // For this user mapped with multiple groups in Fusion
                                SaveUserGroups(userId, fusionGroupId);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    #region Azure Graph API Calls

    private string GetHttpContentWithToken(string url, string token)
    {
        try
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("Authorization", "Bearer " + token);
                    request.Headers.Add("Accept", "application/json;odata.metadata=minimal");
                    using (var response = client.SendAsync(request).Result)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string groupJson = response.Content.ReadAsStringAsync().Result;
                            // Dim jobjectResult = Newtonsoft.Json.Linq.JObject.Parse(groupJson)
                            // Dim results As Object = Newtonsoft.Json.JsonConvert.DeserializeObject(Of Object)(groupJson)
                            return groupJson;
                        }
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message.ToString();
        }
    }

    public List<AzureADGroupsModel> GetGroups(string token)
    {
        string groupJson = this.GetHttpContentWithToken(AzureADSettings.urlListGroups, token);
        var groups = new List<AzureADGroupsModel>();
        if (!string.IsNullOrEmpty(groupJson))
        {
            var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(groupJson);
            foreach (JObject items in (IEnumerable)jsonResult["value"])
            {
                if (items is not null)
                {
                    var group = new AzureADGroupsModel();
                    group.GroupId = items["id"].Value<string>();
                    group.Id = Conversions.ToString(items["displayName"].Value<string>());
                    group.DisplayName = Conversions.ToString(items["displayName"].Value<string>());
                    groups.Add(group);
                }
            }
        }
        return groups;
    }

    public void GetGroupMembers(string token, string url, ref List<AzureADGroupUsersModel> groupUsers) // As List(Of AzureADGroupUsersModel)
    {
        string groupUserJson = GetHttpContentWithToken(url, token);
        // Dim groupUsers As New List(Of AzureADGroupUsersModel)
        if (!string.IsNullOrEmpty(groupUserJson))
        {
            var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(groupUserJson);
            foreach (JObject items in (IEnumerable)jsonResult["value"])
            {
                if (items is not null)
                {
                    var groupUser = new AzureADGroupUsersModel();
                    groupUser.Id = items["id"].Value<string>();
                    groupUser.DisplayName = items["displayName"].Value<string>();
                    groupUser.Email = items["userPrincipalName"].Value<string>();
                    groupUsers.Add(groupUser);
                }
            }
            if (jsonResult.ContainsKey("@odata.nextLink"))
            {
                var nextPageLink = jsonResult["@odata.nextLink"];
                if (!string.IsNullOrEmpty(Conversions.ToString(nextPageLink)))
                {
                    GetGroupMembers(token, Conversions.ToString(nextPageLink), ref groupUsers);
                }
            }
        }
        // Return groupUsers
    }

    #endregion

}