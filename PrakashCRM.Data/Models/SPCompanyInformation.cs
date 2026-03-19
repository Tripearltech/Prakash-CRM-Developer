namespace PrakashCRM.Data.Models
{
    // Maps to Business Central OData endpoint: CompanyInformationDotNetAPI
    // Only the fields needed for email footer are included.
    public class SPCompanyInformation
    {
        public string Name { get; set; }
        public string Bank_Name { get; set; }
        public string Bank_Account_No { get; set; }
        public string IFSC { get; set; }
    }
}
