using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPWarehousePurchase
    {
        public string SystemId { get; set; }
        public string No { get; set; }
        public string Buy_from_Vendor_No { get; set; }
        public string Buy_From_Vendor_Name { get; set; }
        public string Location_Code { get; set; }
        public string Shipment_Date { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public decimal PCPL_Fraight_Charges { get; set; }
        public string PCPL_LR_RR_No { get; set; }
        public string PCPL_LR_RR_Date { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_Mobile_No { get; set; }
        public string PCPL_Driver_License_No { get; set; }
        public string Vehicle_No { get; set; }
    }

    public class SPWarehousePurchaseLine
    {
        public string No { get; set; }
        public string Description { get; set; }
        public decimal Qty_to_Receive { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string PCPL_Packing_Style_Description { get; set; }
        public string Location_Code { get; set; }
    }

    public class SPWarehousePurchaseAccept
    {
        public string salesperson { get; set; }
        public string systemids { get; set; }
    }

    public class SPWarehousePurchaseAcceptResponse
    {
        public string value { get; set; }
    }

    public class SPWarehousePurchaseClosed
    {
        public string transporterCode { get; set; }
        public string systemids { get; set; }
        public string lrno { get; set; }
        public string lrdate { get; set; }
        public string drivername { get; set; }
        public string driverlicenseno { get; set; }
        public string drivermobileno { get; set; }
        public string vehicleno { get; set; }
        public bool applyloadingcharges { get; set; }
        public bool applyunloadingcharges { get; set; }
        public bool applyfreightcharges { get; set; }
        public decimal freightcharge { get; set; }
        public string remarks { get; set; }

    }
}
