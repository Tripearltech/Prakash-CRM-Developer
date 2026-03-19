using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PrakashCRM.Data.Models
{
    public class SPWarehouse
    {
        public string SystemId { get; set; }
        public string No { get; set; }
        public string Sell_to_Customer_No { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Location_Code { get; set; }
        public string Shipment_Date { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public decimal PCPL_Fraight_Charges { get; set; }
        public string LR_RR_No { get; set; }
        public string LR_RR_Date { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_Mobile_No { get; set; }
        public string PCPL_Driver_License_No { get; set; }
        public string Vehicle_No { get; set; }

    }

    public class SPWarehouseSalesLine
    {
        public string No { get; set; }
        public string Description { get; set; }
        public decimal Qty_to_Ship { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string PCPL_Packing_Style_Description { get; set; }
        public string Location_Code { get; set; }
        public string Quantity_Shipped { get; set; }
    }

    //New
    public class SPWarehouseSalesHeaderLine
    {
        public string No_SalesHeader { get; set; }
        public string DocumentType_SalesHeader { get; set; }
        public string ShiptoName { get; set; }
        public string Quantity_Shipped { get; set; }
        public string PCPLVendorAddress { get; set; }
        public string PCPLVendorAddress2 { get; set; }
        public string PCPLVendorCountryRegionCode { get; set; }
        public string PCPLVendorCity { get; set; }
        public string PCPLVendorPostCode { get; set; }
        public string ShiptoAddress { get; set; }
        public string ShiptoAddress2 { get; set; }
        public string ShiptoCity { get; set; }
        public string ShiptoPostCode { get; set; }
        public string ShiptoCountryRegionCode { get; set; }
        public string ShipmentDate_SalesHeader { get; set; }
        public string PCPLAcceptedBy_SalesHeader { get; set; }
        public string PCPLAcceptedbyName_SalesHeader { get; set; }
        public string Job_to_Code { get; set; }
        public string Job_To_Address { get; set; }
        public string Job_To_Address_2 { get; set; }
        public string Job_To_City { get; set; }
        public string Job_To_Post_Code { get; set; }
        public string Job_To_Country_Region_Code { get; set; }

        public string Name_Location { get; set; }
        public string Address_Location { get; set; }
        public string Address2_Location { get; set; }
        public string City_Location { get; set; }
        public string PostCode_Location { get; set; }
        public string CountryRegionCode_Location { get; set; }

        public string No_SalesLine { get; set; }
        public string Description_SalesLine { get; set; }
        public decimal Quantity_SalesLine { get; set; }
        public string UnitofMeasure_SalesLine { get; set; }
        public string PCPLPackingStyleDescription_SalesLine { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string Transport_Quantity_Line { get; set; }
        public string PCPL_Item_Tracking_Code { get; set; }
        public string Tracking_Code { get; set; }
        public string TPTPL_Drop_Shipment { get; set; }
        public string PCPLVendorName_SalesLine { get; set; }
        public string Line_No_ { get; set; }

        public string IncoTerms_SalesHeader { get; set; }
        public string Location_Code { get; set; }

        public string SystemId { get; set; }
        public string PCPL_Transporter_No_ { get; set; }
        public string PCPL_Transporter_Contact_No_ { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string LR_RR_No_ { get; set; }
        public string LR_RR_Date { get; set; }
        public string VehiclenNo { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_License_No_ { get; set; }
        public string PCPL_Driver_Mobile_No_ { get; set; }
        public string PCPL_Remarks { get; set; }
        public string PCPL_Packing_Qty_ { get; set; }
        public string LocationCode { get; set; }
        public string PCPL_Area_Location { get; set; }
        public string FromArea { get; set; }
        public string PCPL_Ship_to_Area { get; set; }
        public string Job_To_Area { get; set; }
        public decimal GrossWeightperunit { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string PCPL_UnLoading_Vendor { get; set; }
        public string PCPL_Loading_Vendor { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
        public string Sell_to_City { get; set; }
        public string TransfertoCity { get; set; }
        public string TransferfromCity { get; set; }
    }

    public class SPWarehousePurchaseHeaderLine
    {
        public string No_Purchase_Header { get; set; }
        public string ShiptoName { get; set; }
        public string ShiptoAddress { get; set; }
        public string ShiptoAddress2 { get; set; }
        public string ShiptoCity { get; set; }
        public string ShiptoPostCode { get; set; }
        public string ShiptoCountryRegionCode { get; set; }
        public string ShipmentDate_Purchase_Header { get; set; } //Missiing in API
        public string PCPLAcceptedBy_Purchase_Header { get; set; }
        public string PCPLAcceptedbyName_Purchase_Header { get; set; }
        public bool TPTPLDropShipment { get; set; }

        public string Name_Location { get; set; }
        public string Address_Location { get; set; }
        public string Address2_Location { get; set; }
        public string CityLocation { get; set; }
        public string PostCodeLocation { get; set; }
        public string CountryRegionCode_Location { get; set; }

        public string No_Purchase_Line { get; set; }
        public string Description_Purchase_Line { get; set; }
        public decimal Quantity_Purchase_Line { get; set; }
        public string UnitofMeasure_Purchase_Line { get; set; }
        public string PCPLPackingStyleDescription_Purchase_Line { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string Transport_Quantity_Line { get; set; }
        public string Tracking_Code { get; set; }
        public string Line_No_ { get; set; }

        public string IncoTerms_Purchase_Header { get; set; }
        public string shiptocode { get; set; }

        public string SystemId { get; set; }
        public string PCPL_Transporter_No_ { get; set; }
        public string PCPL_Transporter_Contact_No_ { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string PCPL_LR_RR_No_ { get; set; }
        public string LR_RR_Date { get; set; }
        public string VehiclenNo { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_License_No_ { get; set; }
        public string PCPL_Driver_Mobile_No_ { get; set; }
        public string PCPL_Remarks { get; set; }
        public string PCPL_Packing_Qty_ { get; set; }
        public string LocationCode { get; set; }
        public string Location_Code { get; set; }
        public string PCPLBuyfromArea { get; set; }
        public object PCPL_Ship_to_Area { get; set; }
        public decimal GrossWeightperunit { get; set; }
        public string PCPL_UnLoading_Vendor { get; set; }
        public string PCPL_Loading_Vendor { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
        public string City_Location { get; set; }
        public string Sell_to_City { get; set; }
        public string TransfertoCity { get; set; }
        public string TransferfromCity { get; set; }

    }

    public class SPWarehouseTransferHeaderLine
    {
        public string No { get; set; }
        public string TransfertoName { get; set; }
        public string TransfertoAddress { get; set; }
        public string TransfertoAddress2 { get; set; }
        public string PCPLFromArea { get; set; }
        public string TransfertoPostCode { get; set; }
        public string TransfertoCounty { get; set; }

        public string Shipment_Date { get; set; }

        //public string PCPLAcceptedBy_SalesHeader { get; set; } //Missing in API
        public string PCPLAcceptedBy { get; set; }

        //public string PCPLAcceptedbyName_SalesHeader { get; set; }//Missing in API
        public string PCPLAcceptedName { get; set; }

        public string TransferfromName { get; set; }
        public string TransferfromAddress { get; set; }
        public string TransferfromAddress2 { get; set; }
        public string FromArea { get; set; }
        public string TransferfromPostCode { get; set; }
        public string TransferfromCounty { get; set; }

        public string Item_No_ { get; set; }
        public string Description_SalesLine { get; set; }
        public decimal Quantity_SalesLine { get; set; }
        public string UnitofMeasure_SalesLine { get; set; }
        public string PCPLPackingStyleDescription_SalesLine { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string Transport_Quantity_Line { get; set; }
        public string Tracking_Code { get; set; }
        public string Line_No_ { get; set; }
        public string TransferfromCode { get; set; }
        public string TransfertoCode { get; set; }

        public string SystemId { get; set; }
        public string PCPL_Transporter_No_ { get; set; }
        public string PCPL_Transporter_Contact_No_ { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string LR_RR_No_ { get; set; }
        public string LR_RR_Date { get; set; }
        public string VehiclenNo { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_License_No_ { get; set; }
        public string PCPL_Driver_Mobile_No_ { get; set; }
        public string PCPL_Remarks { get; set; }
        public string PCPL_Packing_Qty_ { get; set; }
        public string LocationCode { get; set; }
        public string Location_Code { get; set; }
        /* public string FromArea { get; set; }*/
        public string PCPLToArea { get; set; }

        public string TransferfromCity { get; set; }
        public string TransfertoCity { get; set; }
        public decimal GrossWeightperunit { get; set; }
        public string PCPL_UnLoading_Vendor { get; set; }
        public string PCPL_Loading_Vendor { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
        public string City_Location { get; set; }
        public string Sell_to_City { get; set; }
        public string ShiptoCity { get; set; }
    }

    public class SPWarehouseList
    {
        public string DocumentNo { get; set; }
        public string ShipmentDate { get; set; }
        public string CustomerVendorNo { get; set; }
        public string CustomerVendorName { get; set; }
        public string DocumentType { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string IsAnyLineWithDropShipment { get; set; }
        public string ItemNo { get; set; }
        public string ItemDescription { get; set; }
        public decimal Qty { get; set; }
        public string PCPLVendorCity { get; set; }
        public string UOM { get; set; }
        public string PackingUOM { get; set; }
        public string PackingStyle { get; set; }
        public string TransportQty { get; set; }
        public string PackingQty { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string DropShipment { get; set; }
        public string DropShipmentVendor { get; set; }
        public string LineNo { get; set; }
        public string IncoTerm { get; set; }
        public string AcceptedBy { get; set; }
        public string SystemId { get; set; }
        public string FormLocation { get; set; }
        public string FromArea { get; set; }
        public string ToArea { get; set; }
        public string City_Location { get; set; }
        public string Sell_to_City { get; set; }
        public string TransfertoCity { get; set; }
        public string TransferfromCity { get; set; }
        public string ShiptoCity { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public string TopurCity { get; set; }
        public string ToTransCity { get; set; }
        public string ToRetrunCity { get; set; }
        public string FrompurCity { get; set; }
        public string fromTransCity { get; set; }
        public string fromVenFromCity { get; set; }
    }

    public class SPWarehouseSalesAccept
    {
        public string salesperson { get; set; }
        public string systemids { get; set; }
    }

    public class SPWarehouseSalesAcceptResponse
    {
        public string value { get; set; }
    }

    public class SPWarehouseClose
    {
        public string transporterCode { get; set; }
       // public string transportercontactno { get; set; }
        public string systemids { get; set; }
        public string lrno { get; set; }
        public string lrdate { get; set; }
        public string drivername { get; set; }
        public string driverlicenseno { get; set; }
        public string drivermobileno { get; set; }
        public string vehicleno { get; set; }
        public decimal loadingcharges { get; set; }
        public decimal unloadingcharges { get; set; }
        public decimal transporteramount { get; set; }
        public string remarks { get; set; }
        public bool isclosed { get; set; }
        public string loadingvendor { get; set; }
        public string unloadingvendor { get; set; }


    }

    //New Purvisha
    public class SPWarehouseCardSales
    {
        //public string SystemId { get; set; }
        public string No { get; set; }
        public string Quote_No { get; set; }
        public string Shipment_Date { get; set; }
        public string Ship_to_Customer { get; set; }
        public string Ship_to_Name { get; set; }
        public string Ship_to_Contact { get; set; }

        //public string Ship_to_Customer { get; set; }

        public string Location_Code { get; set; } //From Address

        public string Ship_to_Address { get; set; } //To Address

        public string Ship_to_Address_2 { get; set; } //To Address

        public string Ship_to_City { get; set; } //To Address

        public string Ship_to_County { get; set; } //To Address
        public string Ship_to_Post_Code { get; set; } //To Address
        //public string Ship_to_Country_Region_Code { get; set; } //To Address

        public string Inco_Term { get; set; }

        public string PCPL_Accepted_by_Name { get; set; }
        public string CustomerVendorName { get; set; }

    }

    public class SPWarehouseCard
    {
        public string DocumentNo { get; set; }
        public string ShipmentDate { get; set; }
        public string CustomerVendorNo { get; set; }
        public string CustomerVendorName { get; set; }
        public string DocumentType { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string FromPincode { get; set; }
        public string ToPincode { get; set; }
        public string TPTPLDropShipment { get; set; }
        public string DropShipmentVendor { get; set; }
        public string IncoTerm { get; set; }
        public string AcceptedBy { get; set; }
        public List<WarehouseCardLine> warehouseCardLines { get; set; } = new List<WarehouseCardLine>();
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }

        public string SystemId { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public string PCPL_Transporter_Contact_No_ { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string LR_RR_No_ { get; set; }
        public string LR_RR_Date { get; set; }
        public string VehiclenNo { get; set; }
        public string PCPL_Driver_Name { get; set; }
        public string PCPL_Driver_License_No_ { get; set; }
        public string PCPL_Driver_Mobile_No_ { get; set; }
        public string PCPL_Remarks { get; set; }
        public string ToArea { get; set; }
        public string FromArea { get; set; }
        public dynamic PackingQty { get; set; }
        public string PCPL_UnLoading_Vendor { get; set; }
        public string PCPL_Loading_Vendor { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
    }

    public class WarehouseCardLine
    {
        public string ItemNo { get; set; }
        public string ItemDescription { get; set; }
        public decimal Qty { get; set; }
        public string UOM { get; set; }
        public string PackingUOM { get; set; }
        public string PackingStyle { get; set; }
        public string TransportQty { get; set; }
        public string PackingQty { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public decimal GrossWeightperunit { get; set; }
        public string LineNo { get; set; }
        public string Shipqty { get; set; }
        public string PCPL_Item_Tracking_Code { get; set; }
    }

    public class TransporterRateCard
    {
        public string Transporter_No { get; set; }

        public string From_Post_Code { get; set; }

        public string To_Post_Code { get; set; }

        public string From_Area { get; set; }

        public string To_Area { get; set; }

        public string UOM { get; set; }

        public decimal Standard_Weight { get; set; }

        public decimal Rate_for_Standard_Weight { get; set; }

        public decimal Rate_above_Standard_Weight { get; set; }

        public string Vehicle_Type { get; set; }

        public string Rate_Effective_Date { get; set; }

        public string Transporter_Type { get; set; }

        public string Transporter_Name { get; set; }
        public string Contact_No { get; set; }

    }

    public class WarehouseCardDrivers
    {
        public string No { get; set; }
        public string Name { get; set; }
        public string Job_Title { get; set; }
        public string Registration_Number { get; set; }
        public string Mobile_Phone_No { get; set; }
        public string Type { get; set; }

    }

    public class WarehouseLoadingUnloading
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string PCPL_Loading_Charges_Per_Unit { get; set; }
        public string PCPL_Unloading_Chgs_Per_Unit { get; set; }

    }

    public class SPDriver
    {
        public string No { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public string Company_No { get; set; }

        public string Mobile_Phone_No { get; set; }
        public string Registration_Number { get; set; }
        public string Job_Title { get; set; }
    }

    public class WarehouseVendor
    {
        public string Vendor_No { get; set; }
        public string Vendor_Name { get; set; }
        public string PCPL_Product_Incharge_Code { get; set; }
    }
    public class UnloadingAndLoading
    {
        public string No { get; set; }
        public string Name { get; set; }
        public bool Transporter { get; set; }

    }
    public class PostWarehouseCardDrivers
    {
        public string Company_No { get; set; }
        public string Name { get; set; }
        public string Job_Title { get; set; }
        public string Registration_Number { get; set; }
        public string Mobile_Phone_No { get; set; }
        public string Type { get; set; }

    }

}
