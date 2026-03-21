using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class SPGRNList
    {
        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string OrderDate { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public string OutstandingQty { get; set; }
        public string PackingStyle { get; set; }
        public string PurchaserName { get; set; }
        public string ContactName { get; set; }
        public string ContactPhoneNo { get; set; }
        public object MakeMfgCode { get; set; }
    }

    public class SPGRNSales
    {
        public string DocumentNo { get; set; }
        public string Order_Date { get; set; }
        public string Name { get; set; }
        public string Description_Product_Name { get; set; }
        public string Outstanding_Quantity { get; set; }
        public string Packing_Style { get; set; }
        public string SalesPerson_Purchaser_Name { get; set; }
        public string ContactName { get; set; }
        public string Contact_Mobile_Phone_No_ { get; set; }
    }

    public class SPGRNPurchase
    {
        public string DocumentNo { get; set; }
        public string Order_Date { get; set; }
        public string Name { get; set; }
        public string Description_Product_Name { get; set; }
        public string Outstanding_Quantity { get; set; }
        public string Packing_Style { get; set; }
        public string SalesPerson_Purchaser_Name { get; set; }
        public string ContactName { get; set; }
        public string Contact_Mobile_Phone_No_ { get; set; }
        public object PCPL_Make_Mfg_Code { get; set; }
    }

    public class SPGRNTransfer
    {
        public string DocumentNo { get; set; }
        public string Order_Date { get; set; }
        public string Name { get; set; }
        public string Description_Product_Name { get; set; }
        public string Outstanding_Quantity { get; set; }
        public string Packing_Style { get; set; }
        //Missing in API
        //public string SalesPerson_Purchaser_Name { get; set; }
        //public string ContactName { get; set; }
        //public string Contact_Mobile_Phone_No_ { get; set; }
    }

    public class SPGRNCard
    {
        public dynamic UnLoadingName;

        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string OrderDate { get; set; }
        public string VendorCustomerName { get; set; }
        public string LocationCode { get; set; }
        public string CurrencyCode { get; set; }
        public string VendorOrderNo { get; set; }
        public string PurchaserCode { get; set; }
        public string ContactName { get; set; } // Contact Person Name
        public string PostingDate { get; set; }
        public string DocumentDate { get; set; }
        public string VendorInvoiceNo { get; set; }
        public string QCRemarks { get; set; }
        public string LRRRNo { get; set; }
        public string LRRRDate { get; set; }
        public string TransporterName { get; set; }
        public string VehicleNo { get; set; }
        public string TransporterNo { get; set; }
        public string TransportAmount { get; set; }
        public string LoadingCharges { get; set; }
        public string UnLoadingCharges { get; set; }
        public string MakeMfgCode { get; set; }
        public bool EXM_Import_Document { get; set; }
        public List<SPGRNCardLine> grnCardLines { get; set; }
        public dynamic LoadingName { get; set; }
    }

    public class SPGRNCardLine
    {
        public string loadingName { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string UOMCode { get; set; }
        public string QtyToReceive { get; set; }
        public string QuantityReceived { get; set; }
        public string QCRemarks { get; set; }
        public string RejectQC { get; set; }
        public string BillOfEntryNo { get; set; }
        public string BillOfEntryDate { get; set; }
        public string Remarks { get; set; }
        public string ConcentrationRatePercent { get; set; }
        public string ExpectedReceiptDate { get; set; }
        public string PackingStyleCode { get; set; }
        public string PackingQty { get; set; }
        public string PackingUOM { get; set; }
        public string MfgName { get; set; }
        public string HSNSACCode { get; set; }
        public string NetWeight { get; set; }
        public string BranchCode { get; set; } //Shortcut_Dimension_1_Code
        public string ProductCode { get; set; } //Shortcut_Dimension_2_Code
        public string FreightCharges { get; set; }
        public string UnloadingCharges { get; set; }
        public int LineNo { get; set; }
        public string TrackingCode { get; set; }
        public string ItemNo { get; set; }
        public string MakeMfgCode { get; set; }
        public string UnloadingName { get; set; }
    }

    public class SPGRNPurchaseCard
    {
        //type = Purchase
        public string No { get; set; }
        public string Order_Date { get; set; }
        public string Buy_from_Vendor_Name { get; set; }
        public string Location_Code { get; set; }
        public string Currency_Code { get; set; }
        public string Vendor_Order_No { get; set; } // Purchase = Vendor Order No. / Sales = External DocumentNo
        public string Purchaser_Code { get; set; } //Purchaser/ Salesperson Name
        public string Pay_to_Contact { get; set; } // Contact Person Name
        public string Posting_Date { get; set; }
        public string Document_Date { get; set; }
        public string Vendor_Invoice_No { get; set; }// Purchase = Vendor Invoice No. / Sales = Your Reference
        public string QCRemarksText { get; set; }
        public string PCPL_LR_RR_No { get; set; }
        public string PCPL_LR_RR_Date { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string Vehicle_No { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string PCPL_Make_Mfg_Code { get; set; }
        public bool EXM_Import_Document { get; set; }

    }

    public class SPGRNPurchaseLine
    {
        public int Line_No { get; set; }
        public string Document_No { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string Unit_of_Measure_Code { get; set; }
        public string Qty_to_Receive { get; set; }
        public string Quantity_Received { get; set; }
        public string PCPL_QC_Remarks { get; set; }
        public string PCPL_Reject_QC { get; set; }
        public string PCPL_Bill_Of_Entry_No { get; set; }
        public string PCPL_Bill_Of_Entry_Date { get; set; }
        public string PCPL_Remarks { get; set; }
        public string PCPL_Concentration_Rate_Percent { get; set; }
        public string Expected_Receipt_Date { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string PCPL_Packing_Qty { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string PCPL_Mfg_Name { get; set; }
        public string HSN_SAC_Code { get; set; }
        public string PCPL_Make_Mfg_Code { get; set; }
        public string Net_Weight { get; set; }
        public string Shortcut_Dimension_1_Code { get; set; } //Branch Code
        public string Shortcut_Dimension_2_Code { get; set; } //Product Code
        public string PCPL_Freight_Charges { get; set; }
        public string PCPL_Unloading_Charges { get; set; }
        public string Tracking_Code { get; set; }
        public string No { get; set; }
    }

    public class SPGRNSalesReturnCard
    {
        public string Document_Type { get; set; }
        public string No { get; set; }
        public string Order_Date { get; set; }
        public string Sell_to_Customer_Name { get; set; }
        public string Location_Code { get; set; }
        public string Currency_Code { get; set; }
        public string External_Document_No { get; set; } // Purchase = Vendor Order No. / Sales = External DocumentNo
        public string Salesperson_Code { get; set; }  // Purchaser/ Salesperson Name
        public string Sell_to_Contact { get; set; }  // Contact Person Name
        public string Posting_Date { get; set; }
        public string Document_Date { get; set; }
        public string Your_Reference { get; set; } // Purchase = Vendor Invoice No. / Sales = Your Reference
        public string LR_RR_No { get; set; }
        public string LR_RR_Date { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string Vehicle_No { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
    }

    public class SPGRNSalesReturnLine
    {
        public int Line_No { get; set; }
        public string Document_No { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string Unit_of_Measure_Code { get; set; }
        public string Return_Qty_to_Receive { get; set; }
        public string Return_Qty_Received { get; set; }
        public string PCPL_Bill_of_Entry_No { get; set; }
        public string PCPL_Bill_of_Entry_Date { get; set; }
        public string PCPL_Remarks { get; set; }
        public string PCPL_Concentration_Bool { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string PCPL_Packing_Qty { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string HSN_SAC_Code { get; set; }
        public string Net_Weight { get; set; }
        public string Shortcut_Dimension_1_Code { get; set; }// Branch Code
        public string Shortcut_Dimension_2_Code { get; set; }// Product Code
        public string PCPL_Freight_Charges { get; set; }//not in API
        public string PCPL_Unloading_Charges { get; set; }//not in API
        public string Tracking_Code { get; set; }
        public string No { get; set; }
    }

    public class SPGRNTransferCard
    {
        public string No { get; set; }
        public string Receipt_Date { get; set; }
        public string Transfer_to_Code { get; set; }
        public string Transfer_from_Code { get; set; }
        public string Posting_Date { get; set; }
        public string LR_RR_No { get; set; }
        public string LR_RR_Date { get; set; }
        public string PCPL_Transporter_No { get; set; }
        public string PCPL_Transporter_Name { get; set; }
        public string Vehicle_No { get; set; }
        public string PCPL_Transport_Amount { get; set; }
        public string PCPL_Loading_Charges { get; set; }
        public string PCPL_UnLoading_Charges { get; set; }
        public string PCPL_Loading_Vendor_Name { get; set; }
        public string PCPL_UnLoading_Vendor_Name { get; set; }
    }

    public class SPGRNTransferLine
    {
        public int Line_No { get; set; }
        public string Document_No { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string Unit_of_Measure_Code { get; set; }
        public string Qty_to_Receive { get; set; }
        public string Quantity_Received { get; set; }
        public string Receipt_Date { get; set; }
        public string PCPL_Packing_Style_Code { get; set; }
        public string PCPL_Packing_Qty { get; set; }
        public string PCPL_Packing_UOM { get; set; }
        public string HSN_SAC_Code { get; set; }
        public string Shortcut_Dimension_1_Code { get; set; } // Branch Code
        public string Shortcut_Dimension_2_Code { get; set; } // Product Code
        public string PCPL_Freight_Charges { get; set; } //not in API
        public string PCPL_Unloading_Charges { get; set; } //not in API
        public string Tracking_Code { get; set; }
        public string No { get; set; }

    }

    public class SPGRNCardRequest
    {
        public string documenttype { get; set; }
        public string documentno { get; set; }
        public string postingdate { get; set; }
        public string documentdate { get; set; }
        public string referenceinvoiceno { get; set; }
        public string qcremarks { get; set; }
        public string lrno { get; set; }
        public string lrdate { get; set; }
        public string transportername { get; set; }
        public string transporterno { get; set; }
        public string vehicleno { get; set; }
        public decimal transportationamount { get; set; }
        public decimal loadingcharges { get; set; }
        public decimal unloadingcharges { get; set; }
        public List<SPGRNCardLineRequest> grnCardLineRequest { get; set; }

    }
    public class SPGRNCardLineRequest
    {
        public string documenttype { get; set; }
        public string documentno { get; set; }
        public int lineno { get; set; }
        public double qtytoreceive { get; set; }
        public string qcremarks { get; set; }
        public decimal rejectqty { get; set; }
        public string beno { get; set; }
        public string bedate { get; set; }
        public string remarks { get; set; }
        public decimal concentratepercent { get; set; }
        public string makemfgname { get; set; }
        public string mfgcode { get; set; }
    }

    public class SPGRNSaveResponse
    {
        public string value { get; set; }
    }

    public class SPGRNHeaderRequest
    {
        public string documenttype { get; set; }
        public string documentno { get; set; }
        public string postingdate { get; set; }
        public string documentdate { get; set; }
        public string referenceinvoiceno { get; set; }
        public string qcremarks { get; set; }
        public string lrno { get; set; }
        public string lrdate { get; set; }
        public string transportername { get; set; }
        public string transporterno { get; set; }
        public string vehicleno { get; set; }
        public decimal transportationamount { get; set; }
        public decimal loadingcharges { get; set; }
        public decimal unloadingcharges { get; set; }

    }

    public class SPGRNLineItemTracking
    {
        public int Entry_No { get; set; }
        public bool Positive { get; set; }
        public int Source_Type { get; set; }
        public string Source_ID { get; set; }
        public int Source_Ref_No { get; set; }
        public string Source_Subtype { get; set; }
        public string Expiration_Date { get; set; }
        public string Item_No { get; set; }
        public string Lot_No { get; set; }
        public string Location_Code { get; set; }
        public int Quantity { get; set; }
        public int Qty_to_Handle_Base { get; set; }
        public int Transferred_from_Entry_No { get; set; }
    }
    public class ReservationEntryForGRN
    {
        public string EntryNo { get; set; }
        public string DocumentType { get; set; }
        public string OrderNo { get; set; }
        public string LocationCode { get; set; }
        public string LineNo { get; set; }
        public string ItemNo { get; set; }
        public string LotNo { get; set; }
        public string Qty { get; set; }
        public string ExpirationDate { get; set; }

    }

    public class ReservationEntryForGRNRequest
    {
        public string text { get; set; }
    }

    public class GRNItemTrackingAttachment
    {
        public string AttachmentId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string LineNo { get; set; }
        public string RowKey { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string UploadedAt { get; set; }
        public string StoredFileName { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class GRNDocumentAttachmentPayload
    {
        public int Table_ID { get; set; }
        public string No { get; set; }
        public string Item_No { get; set; }
        public string File_Extension { get; set; }
        public string File_Name { get; set; }
        public string File_Type { get; set; }
        public string Base64Text { get; set; }
    }

    public class UploadGRNItemTrackingAttachmentsRequest
    {
        public string DocumentNo { get; set; }
        public string ItemNo { get; set; }
        public List<GRNDocumentAttachmentPayload> Attachments { get; set; } = new List<GRNDocumentAttachmentPayload>();
    }

    public class GetGRNItemTrackingAttachmentsRequest
    {
        public string LotNo { get; set; }
        public string ItemNo { get; set; }
    }

    public class FinalizeGRNItemTrackingAttachmentMapping
    {
        public string OldRowKey { get; set; }
        public string NewRowKey { get; set; }
    }

    public class FinalizeGRNItemTrackingAttachmentsRequest
    {
        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string LineNo { get; set; }
        public List<FinalizeGRNItemTrackingAttachmentMapping> Mappings { get; set; } = new List<FinalizeGRNItemTrackingAttachmentMapping>();
    }

    public class SPGRNVendors
    {
        public string Name { get; set; }
        public string No { get; set; }
    }
    public class DeleteReservationEntryForGRNRequest
    {
        public string entryno { get; set; }
        public bool positive { get; set; }
    }

}

