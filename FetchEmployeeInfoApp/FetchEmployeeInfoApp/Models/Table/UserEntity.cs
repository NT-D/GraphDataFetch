using Microsoft.Graph;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchEmployeeInfoApp.Models.Table
{
    public class UserEntity : TableEntity
    {
        public string Id { get; set; }
        public string City { get; set; }
        public string Displayname { get; set; }
        public string Officelocation { get; set; }
        public string Country { get; set; }
        public string Department { get; set; }
        public string Userprincipalname { get; set; }
        public string PreferredLanguage { get; set; }

        public UserEntity(User user)
        {
            PartitionKey = user.Id;
            RowKey = string.IsNullOrEmpty(user.PreferredLanguage) ? "en" : user.PreferredLanguage;

            Id = user.Id;
            City = user.City;
            Displayname = user.DisplayName;
            Officelocation = user.OfficeLocation;
            Country = user.Country;
            Department = user.Department;
            Userprincipalname = user.UserPrincipalName;
            PreferredLanguage = user.PreferredLanguage;
        }
    }
}