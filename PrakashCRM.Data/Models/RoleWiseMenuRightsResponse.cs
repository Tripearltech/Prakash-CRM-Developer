using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace PrakashCRM.Data.Models
{
    public class RoleWiseMenuRightsResponse
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("roles")]
        public List<RoleWiseRole> Roles { get; set; }
    }

    public class RoleWiseRole
    {
        [JsonProperty("roleId")]
        public string RoleId { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; }

        [JsonProperty("menus")]
        public List<RoleWiseMenu> Menus { get; set; }
    }

    public class RoleWiseMenu
    {
        [JsonProperty("menuId")]
        public string MenuId { get; set; }

        [JsonProperty("menuName")]
        public string MenuName { get; set; }

        [JsonProperty("isParent")]
        public bool IsParent { get; set; }

        [JsonProperty("children")]
        public List<RoleWiseMenuChild> Children { get; set; }
    }

    public class RoleWiseMenuChild
    {
        [JsonProperty("menuId")]
        public string MenuId { get; set; }

        [JsonProperty("menuName")]
        public string MenuName { get; set; }

        [JsonProperty("controller")]
        public string Controller { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("permissions")]
        public RoleWisePermission Permissions { get; set; }
    }

    public class RoleWisePermission
    {
        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("create")]
        public bool Create { get; set; }

        [JsonProperty("update")]
        public bool Update { get; set; }

        [JsonProperty("delete")]
        public bool Delete { get; set; }

        // Legacy / BC-style rights (some payloads use these instead of read/create/update/delete)
        [JsonProperty("Full_Rights")]
        public bool Full_Rights { get; set; }

        [JsonProperty("Add_Rights")]
        public bool Add_Rights { get; set; }

        [JsonProperty("Edit_Rights")]
        public bool Edit_Rights { get; set; }

        [JsonProperty("View_Rights")]
        public bool View_Rights { get; set; }

        [JsonProperty("Delete_Rights")]
        public bool Delete_Rights { get; set; }

        // Keep any extra fields without losing them (future-proof)
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }

        public void Normalize()
        {
            // If Full_Rights is present => all true
            if (Full_Rights)
            {
                Read = Create = Update = Delete = true;
                return;
            }

            // Map legacy flags into standard ones when standard ones are false
            if (!Read && View_Rights) Read = true;
            if (!Create && Add_Rights) Create = true;
            if (!Update && Edit_Rights) Update = true;
            if (!Delete && Delete_Rights) Delete = true;

            // Also look into extension data for alternate key spellings
            try
            {
                if (ExtensionData != null)
                {
                    bool hasFull = GetBool("full_rights") || GetBool("fullrights") || GetBool("full") || GetBool("fullRights");
                    if (hasFull)
                    {
                        Read = Create = Update = Delete = true;
                        return;
                    }

                    if (!Read && (GetBool("view_rights") || GetBool("read_rights") || GetBool("view") || GetBool("read"))) Read = true;
                    if (!Create && (GetBool("add_rights") || GetBool("create") || GetBool("add") || GetBool("insert_rights"))) Create = true;
                    if (!Update && (GetBool("edit_rights") || GetBool("update") || GetBool("edit") || GetBool("modify_rights"))) Update = true;
                    if (!Delete && (GetBool("delete_rights") || GetBool("delete") || GetBool("remove_rights"))) Delete = true;
                }
            }
            catch { }

            bool GetBool(string key)
            {
                if (ExtensionData == null || string.IsNullOrWhiteSpace(key)) return false;
                foreach (var kv in ExtensionData)
                {
                    if (kv.Key == null) continue;
                    if (!kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        if (kv.Value == null) return false;
                        if (kv.Value.Type == JTokenType.Boolean) return kv.Value.Value<bool>();
                        if (kv.Value.Type == JTokenType.Integer) return kv.Value.Value<int>() != 0;
                        if (kv.Value.Type == JTokenType.String)
                        {
                            var s = (kv.Value.Value<string>() ?? "").Trim();
                            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch { }
                }
                return false;
            }
        }
    }
}
