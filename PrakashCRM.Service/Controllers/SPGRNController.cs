using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrakashCRM.Data.Models;
using PrakashCRM.Service.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/SPGRN")]
    public class SPGRNController : ApiController
    {

        [Route("GetGRNTaskAll")]
        public List<SPGRNList> GetGRNTaskAll(string orderby, string filter)
        {
            API ac = new API();
            List<SPGRNSales> grnSales = new List<SPGRNSales>();

            //if (filter == "" || filter == null)
            //    filter = "PCPL_Closed_Filter_FilterOnly eq false";
            //else
            //    filter += " and PCPL_Closed_Filter_FilterOnly eq false";
            //filter = "";
            orderby = "";

            var result = ac.GetData1<SPGRNSales>("GRNSalesOrders", filter, 0, 0, orderby);

            if (result.Result.Item1.value.Count > 0)
                grnSales = result.Result.Item1.value;

            for (int i = 0; i < grnSales.Count; i++)
            {
                string[] strDate = grnSales[i].Order_Date.Split('-');
                grnSales[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPGRNPurchase> grnPurchase = new List<SPGRNPurchase>();
            var result1 = ac.GetData1<SPGRNPurchase>("GRNPurchaseOrders", filter, 0, 0, orderby);

            if (result1.Result.Item1.value.Count > 0)
                grnPurchase = result1.Result.Item1.value;

            for (int i = 0; i < grnPurchase.Count; i++)
            {
                string[] strDate = grnPurchase[i].Order_Date.Split('-');
                grnPurchase[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPGRNTransfer> grnTransfer = new List<SPGRNTransfer>();
            var result2 = ac.GetData1<SPGRNTransfer>("GRNTransferOrders", filter, 0, 0, orderby);

            if (result2.Result.Item1.value.Count > 0)
                grnTransfer = result2.Result.Item1.value;

            for (int i = 0; i < grnTransfer.Count; i++)
            {
                string[] strDate = grnTransfer[i].Order_Date.Split('-');
                grnTransfer[i].Order_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
            }

            List<SPGRNList> allGRNData = MapToSPGRNList(grnSales, grnPurchase, grnTransfer);

            return allGRNData;
        }

        public List<SPGRNList> MapToSPGRNList(List<SPGRNSales> grnSales,
                                                  List<SPGRNPurchase> grnPurchase,
                                                  List<SPGRNTransfer> grnTransfer)
        {
            List<SPGRNList> combinedList = new List<SPGRNList>();

            // Map Sales
            foreach (var salesLine in grnSales)
            {
                combinedList.Add(new SPGRNList
                {
                    DocumentNo = salesLine.DocumentNo,
                    OrderDate = salesLine.Order_Date,
                    Name = salesLine.Name,
                    DocumentType = "Sales Return",
                    ProductName = salesLine.Description_Product_Name,
                    OutstandingQty = salesLine.Outstanding_Quantity,
                    PackingStyle = salesLine.Packing_Style,
                    PurchaserName = salesLine.SalesPerson_Purchaser_Name,
                    ContactName = salesLine.ContactName,
                    ContactPhoneNo = salesLine.Contact_Mobile_Phone_No_
                });
            }

            // Map Purchase
            foreach (var purchaseLine in grnPurchase)
            {
                combinedList.Add(new SPGRNList
                {
                    DocumentNo = purchaseLine.DocumentNo,
                    OrderDate = purchaseLine.Order_Date,
                    Name = purchaseLine.Name,
                    DocumentType = "Purchase Order",
                    ProductName = purchaseLine.Description_Product_Name,
                    OutstandingQty = purchaseLine.Outstanding_Quantity,
                    PackingStyle = purchaseLine.Packing_Style,
                    PurchaserName = purchaseLine.SalesPerson_Purchaser_Name,
                    ContactName = purchaseLine.ContactName,
                    ContactPhoneNo = purchaseLine.Contact_Mobile_Phone_No_,
                    MakeMfgCode = purchaseLine.PCPL_Make_Mfg_Code
                });
            }

            // Map Transfer
            foreach (var transferLine in grnTransfer)
            {
                combinedList.Add(new SPGRNList
                {
                    DocumentNo = transferLine.DocumentNo,
                    OrderDate = transferLine.Order_Date,
                    Name = transferLine.Name,
                    DocumentType = "Transfer Order",
                    ProductName = transferLine.Description_Product_Name,
                    OutstandingQty = transferLine.Outstanding_Quantity,
                    PackingStyle = transferLine.Packing_Style,
                    PurchaserName = "",
                    ContactName = "",
                    ContactPhoneNo = ""
                });
            }

            return combinedList;
        }

        [Route("GetGRNFromNo")]
        public SPGRNCard GetGRNFromNo(string No, string DocumentType)
        {
            API ac = new API();
            SPGRNCard spgrnCard = new SPGRNCard();
            var result = (dynamic)null;
            var resultline = (dynamic)null;

            if (DocumentType == "Purchase Order")
            {
                result = ac.GetData<SPGRNPurchaseCard>("PurchaseOrderCardDotNetAPI", "Document_Type eq 'Order' and No eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    var purchase = result.Result.Item1.value[0];
                    spgrnCard.DocumentType = DocumentType;
                    spgrnCard.DocumentNo = purchase.No;

                    if (purchase.Order_Date != "0001-01-01")
                    {
                        string[] porderdate = purchase.Order_Date.Split('-');
                        spgrnCard.OrderDate = porderdate[2] + '-' + porderdate[1] + '-' + porderdate[0];
                    }
                    else
                    {
                        spgrnCard.OrderDate = "";
                    }

                    spgrnCard.VendorCustomerName = purchase.Buy_from_Vendor_Name;
                    spgrnCard.LocationCode = purchase.Location_Code;
                    spgrnCard.CurrencyCode = purchase.Currency_Code;
                    spgrnCard.VendorOrderNo = purchase.Vendor_Order_No;
                    spgrnCard.PurchaserCode = purchase.Purchaser_Code;
                    spgrnCard.ContactName = purchase.Pay_to_Contact;

                    if (purchase.Posting_Date != "0001-01-01")
                    {
                        string[] ppostingdate = purchase.Posting_Date.Split('-');
                        spgrnCard.PostingDate = ppostingdate[2] + '-' + ppostingdate[1] + '-' + ppostingdate[0];
                    }
                    else
                    {
                        spgrnCard.PostingDate = "";
                    }

                    if (purchase.Document_Date != "0001-01-01")
                    {
                        string[] pdocumentdate = purchase.Document_Date.Split('-');
                        spgrnCard.DocumentDate = pdocumentdate[2] + '-' + pdocumentdate[1] + '-' + pdocumentdate[0];
                    }
                    else
                    {
                        spgrnCard.DocumentDate = "";
                    }

                    spgrnCard.VendorInvoiceNo = purchase.Vendor_Invoice_No;
                    spgrnCard.QCRemarks = purchase.QCRemarksText;
                    spgrnCard.LRRRNo = purchase.PCPL_LR_RR_No;

                    if (purchase.PCPL_LR_RR_Date != "0001-01-01")
                    {
                        string[] plrrrdate = purchase.PCPL_LR_RR_Date.Split('-');
                        spgrnCard.LRRRDate = plrrrdate[2] + '-' + plrrrdate[1] + '-' + plrrrdate[0];
                    }
                    else
                    {
                        spgrnCard.LRRRDate = "";
                    }

                    spgrnCard.TransporterName = purchase.PCPL_Transporter_Name;
                    spgrnCard.VehicleNo = purchase.Vehicle_No;
                    spgrnCard.TransporterNo = purchase.PCPL_Transporter_No;
                    spgrnCard.TransportAmount = purchase.PCPL_Transport_Amount;
                    spgrnCard.LoadingCharges = purchase.PCPL_Loading_Charges;
                    spgrnCard.UnLoadingCharges = purchase.PCPL_UnLoading_Charges;
                    spgrnCard.UnLoadingName = purchase.PCPL_UnLoading_Vendor_Name;
                    spgrnCard.MakeMfgCode = purchase.PCPL_Make_Mfg_Code;
                    spgrnCard.EXM_Import_Document = purchase.EXM_Import_Document;
                    List<SPGRNCardLine> spgrnCardLine = new List<SPGRNCardLine>();
                    resultline = ac.GetData<SPGRNPurchaseLine>("PurchaseLinesDotNetAPI", "Document_type eq 'Order' and Document_No eq '" + No + "'");

                    if (resultline.Result.Item1.value.Count > 0)
                    {
                        foreach (var purchaseline in resultline.Result.Item1.value)
                        {
                            spgrnCardLine.Add(new SPGRNCardLine
                            {
                                LineNo = purchaseline.Line_No,
                                Description = purchaseline.Description,
                                Quantity = purchaseline.Quantity,
                                UOMCode = purchaseline.Unit_of_Measure_Code,
                                QtyToReceive = purchaseline.Qty_to_Receive,
                                QuantityReceived = purchaseline.Quantity_Received,
                                QCRemarks = purchaseline.PCPL_QC_Remarks,
                                RejectQC = purchaseline.PCPL_Reject_QC,

                                BillOfEntryNo = purchaseline.PCPL_Bill_Of_Entry_No,
                                BillOfEntryDate = purchaseline.PCPL_Bill_Of_Entry_Date == "0001-01-01" ? "" : purchaseline.PCPL_Bill_Of_Entry_Date,
                                Remarks = purchaseline.PCPL_Remarks,
                                ConcentrationRatePercent = purchaseline.PCPL_Concentration_Rate_Percent,
                                ExpectedReceiptDate = purchaseline.Expected_Receipt_Date,
                                PackingStyleCode = purchaseline.PCPL_Packing_Style_Code,
                                PackingQty = purchaseline.PCPL_Packing_Qty,

                                PackingUOM = purchaseline.PCPL_Packing_UOM,
                                MfgName = purchaseline.PCPL_Mfg_Name,
                                HSNSACCode = purchaseline.HSN_SAC_Code,
                                MakeMfgCode = purchaseline.PCPL_Make_Mfg_Code,
                                NetWeight = purchaseline.Net_Weight,
                                BranchCode = purchaseline.Shortcut_Dimension_1_Code,
                                ProductCode = purchaseline.Shortcut_Dimension_2_Code,
                                FreightCharges = purchaseline.PCPL_Freight_Charges,
                                UnloadingCharges = purchaseline.PCPL_Unloading_Charges,
                                //UnloadingName = purchaseline.PCPL_UnLoading_Vendor_Name,
                                TrackingCode = purchaseline.Tracking_Code,
                                ItemNo = purchaseline.No
                            });

                        }
                    }
                    spgrnCard.grnCardLines = spgrnCardLine;
                }
            }
            else if (DocumentType == "Sales Return")
            {
                result = ac.GetData<SPGRNSalesReturnCard>("SalesReturnOrderDotNetAPI", "Document_Type eq 'Return Order' and No eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    var salesreturn = result.Result.Item1.value[0];
                    spgrnCard.DocumentType = DocumentType;
                    spgrnCard.DocumentNo = salesreturn.No;

                    if (salesreturn.Order_Date != "0001-01-01")
                    {
                        string[] sorderdate = salesreturn.Order_Date.Split('-');
                        spgrnCard.OrderDate = sorderdate[2] + '-' + sorderdate[1] + '-' + sorderdate[0];
                    }
                    else
                    {
                        spgrnCard.OrderDate = "";
                    }

                    spgrnCard.VendorCustomerName = salesreturn.Sell_to_Customer_Name;
                    spgrnCard.LocationCode = salesreturn.Location_Code;
                    spgrnCard.CurrencyCode = salesreturn.Currency_Code;
                    spgrnCard.VendorOrderNo = salesreturn.External_Document_No;
                    spgrnCard.PurchaserCode = salesreturn.Salesperson_Code;
                    spgrnCard.ContactName = salesreturn.Sell_to_Contact;

                    if (salesreturn.Posting_Date != "0001-01-01")
                    {
                        string[] spostingdate = salesreturn.Posting_Date.Split('-');
                        spgrnCard.PostingDate = spostingdate[2] + '-' + spostingdate[1] + '-' + spostingdate[0];
                    }
                    else
                    {
                        spgrnCard.PostingDate = "";
                    }

                    if (salesreturn.Document_Date != "0001-01-01")
                    {
                        string[] sdocumentdate = salesreturn.Document_Date.Split('-');
                        spgrnCard.DocumentDate = sdocumentdate[2] + '-' + sdocumentdate[1] + '-' + sdocumentdate[0];
                    }
                    else
                    {
                        spgrnCard.DocumentDate = "";
                    }

                    spgrnCard.VendorInvoiceNo = salesreturn.Your_Reference;
                    spgrnCard.QCRemarks = "";
                    spgrnCard.LRRRNo = salesreturn.LR_RR_No;

                    if (salesreturn.LR_RR_Date != "0001-01-01")
                    {
                        string[] slrrrdate = salesreturn.LR_RR_Date.Split('-');
                        spgrnCard.LRRRDate = slrrrdate[2] + '-' + slrrrdate[1] + '-' + slrrrdate[0];
                    }
                    else
                    {
                        spgrnCard.LRRRDate = "";
                    }

                    spgrnCard.TransporterName = salesreturn.PCPL_Transporter_Name;
                    spgrnCard.VehicleNo = salesreturn.Vehicle_No;
                    spgrnCard.TransporterNo = salesreturn.PCPL_Transporter_No;
                    spgrnCard.TransportAmount = salesreturn.PCPL_Transport_Amount;
                    spgrnCard.LoadingCharges = salesreturn.PCPL_Loading_Charges;
                    spgrnCard.LoadingName = salesreturn.PCPL_Loading_Vendor_Name;
                    spgrnCard.UnLoadingCharges = salesreturn.PCPL_UnLoading_Charges;
                    spgrnCard.UnLoadingName = salesreturn.PCPL_UnLoading_Vendor_Name;
                    spgrnCard.MakeMfgCode = "";

                    List<SPGRNCardLine> spgrnCardLine = new List<SPGRNCardLine>();
                    resultline = ac.GetData<SPGRNSalesReturnLine>("SalesReturnOrderSubformDotNetAPI", "Document_type eq 'Return Order' and Document_No eq '" + No + "' and Type eq 'Item'");

                    if (resultline.Result.Item1.value.Count > 0)
                    {
                        foreach (var salesreturnline in resultline.Result.Item1.value)
                        {
                            spgrnCardLine.Add(new SPGRNCardLine
                            {
                                LineNo = salesreturnline.Line_No,
                                Description = salesreturnline.Description,
                                Quantity = salesreturnline.Quantity,
                                UOMCode = salesreturnline.Unit_of_Measure_Code,
                                QtyToReceive = salesreturnline.Return_Qty_to_Receive,
                                QuantityReceived = salesreturnline.Return_Qty_Received,
                                QCRemarks = "",
                                RejectQC = "",

                                BillOfEntryNo = salesreturnline.PCPL_Bill_of_Entry_No,
                                BillOfEntryDate = salesreturnline.PCPL_Bill_of_Entry_Date == "0001-01-01" ? "" : salesreturnline.PCPL_Bill_of_Entry_Date,
                                Remarks = salesreturnline.PCPL_Remarks,
                                ConcentrationRatePercent = salesreturnline.PCPL_Concentration_Bool,
                                ExpectedReceiptDate = "",
                                PackingStyleCode = salesreturnline.PCPL_Packing_Style_Code,
                                PackingQty = salesreturnline.PCPL_Packing_Qty,

                                PackingUOM = salesreturnline.PCPL_Packing_UOM,
                                MfgName = "",
                                HSNSACCode = salesreturnline.HSN_SAC_Code,
                                MakeMfgCode = "",
                                NetWeight = salesreturnline.Net_Weight,
                                BranchCode = salesreturnline.Shortcut_Dimension_1_Code,
                                ProductCode = salesreturnline.Shortcut_Dimension_2_Code,
                                FreightCharges = salesreturnline.PCPL_Freight_Charges,
                                UnloadingCharges = salesreturnline.PCPL_Unloading_Charges,
                                //UnloadingName = salesreturnline.PCPL_UnLoading_Vendor_Name,
                                //loadingname = salesreturnline.PCPL_Loading_Vendor_Name,
                                TrackingCode = salesreturnline.Tracking_Code,
                                ItemNo = salesreturnline.No
                            });

                        }
                    }
                    spgrnCard.grnCardLines = spgrnCardLine;
                }
            }
            else if (DocumentType == "Transfer Order")
            {
                result = ac.GetData<SPGRNTransferCard>("TransferOrderDotNetAPI", "No eq '" + No + "'");

                if (result.Result.Item1.value.Count > 0)
                {
                    var transfer = result.Result.Item1.value[0];
                    spgrnCard.DocumentType = DocumentType;
                    spgrnCard.DocumentNo = transfer.No;

                    if (transfer.Receipt_Date != "0001-01-01")
                    {
                        string[] treceiptDate = transfer.Receipt_Date.Split('-');
                        spgrnCard.OrderDate = treceiptDate[2] + '-' + treceiptDate[1] + '-' + treceiptDate[0];
                    }
                    else
                    {
                        spgrnCard.OrderDate = "";
                    }

                    spgrnCard.VendorCustomerName = transfer.Transfer_from_Code;
                    spgrnCard.LocationCode = transfer.Transfer_to_Code;
                    spgrnCard.CurrencyCode = "";
                    spgrnCard.VendorOrderNo = "";
                    spgrnCard.PurchaserCode = "";
                    spgrnCard.ContactName = "";

                    if (transfer.Posting_Date != "0001-01-01")
                    {
                        string[] tpostingdate = transfer.Posting_Date.Split('-');
                        spgrnCard.PostingDate = tpostingdate[2] + '-' + tpostingdate[1] + '-' + tpostingdate[0];
                    }
                    else
                    {
                        spgrnCard.PostingDate = "";
                    }

                    spgrnCard.DocumentDate = "";
                    spgrnCard.VendorInvoiceNo = "";
                    spgrnCard.QCRemarks = "";
                    spgrnCard.LRRRNo = transfer.LR_RR_No;

                    if (transfer.LR_RR_Date != "0001-01-01")
                    {
                        string[] tlrrrdate = transfer.LR_RR_Date.Split('-');
                        spgrnCard.LRRRDate = tlrrrdate[2] + '-' + tlrrrdate[1] + '-' + tlrrrdate[0];
                    }
                    else
                    {
                        spgrnCard.LRRRDate = "";
                    }

                    spgrnCard.TransporterName = transfer.PCPL_Transporter_Name;
                    spgrnCard.VehicleNo = transfer.Vehicle_No;
                    spgrnCard.TransporterNo = transfer.PCPL_Transporter_No;
                    spgrnCard.TransportAmount = transfer.PCPL_Transport_Amount;
                    spgrnCard.LoadingCharges = transfer.PCPL_Loading_Charges;
                    spgrnCard.UnLoadingCharges = transfer.PCPL_UnLoading_Charges;
                    spgrnCard.UnLoadingName = transfer.PCPL_UnLoading_Vendor_Name;
                    spgrnCard.LoadingName = transfer.PCPL_Loading_Vendor_Name;
                    spgrnCard.MakeMfgCode = "";

                    List<SPGRNCardLine> spgrnCardLine = new List<SPGRNCardLine>();
                    resultline = ac.GetData<SPGRNTransferLine>("TransferLinesDotNetAPI", "Document_No eq '" + No + "'");

                    if (resultline.Result.Item1.value.Count > 0)
                    {
                        foreach (var transferline in resultline.Result.Item1.value)
                        {
                            spgrnCardLine.Add(new SPGRNCardLine
                            {
                                LineNo = transferline.Line_No,
                                Description = transferline.Description,
                                Quantity = transferline.Quantity,
                                UOMCode = transferline.Unit_of_Measure_Code,
                                QtyToReceive = transferline.Qty_to_Receive,
                                QuantityReceived = transferline.Quantity_Received,
                                QCRemarks = "",
                                RejectQC = "",

                                BillOfEntryNo = "",
                                BillOfEntryDate = "",
                                Remarks = "",
                                ConcentrationRatePercent = "",
                                ExpectedReceiptDate = transferline.Receipt_Date == "0001-01-01" ? "" : transferline.Receipt_Date,
                                PackingStyleCode = transferline.PCPL_Packing_Style_Code,
                                PackingQty = transferline.PCPL_Packing_Qty,

                                PackingUOM = transferline.PCPL_Packing_UOM,
                                MfgName = "",
                                HSNSACCode = transferline.HSN_SAC_Code,
                                NetWeight = "",
                                MakeMfgCode = "",
                                BranchCode = transferline.Shortcut_Dimension_1_Code,
                                ProductCode = transferline.Shortcut_Dimension_2_Code,
                                FreightCharges = transferline.PCPL_Freight_Charges,
                                UnloadingCharges = transferline.PCPL_Unloading_Charges,
                                //loadingName = transferline.PCPL_Loading_Vendor_Name,
                                //UnloadingName = transferline.PCPL_UnLoading_Vendor_Name,
                                TrackingCode = transferline.Tracking_Code,
                                ItemNo = transferline.No
                            });

                        }
                    }
                    spgrnCard.grnCardLines = spgrnCardLine;
                }
            }

            return spgrnCard;
        }

        [Route("SaveSPGRNCard")]
        public bool SaveSPGRNCard(SPGRNCardRequest grnCardRequest)
        {
            bool headerflag = false;
            bool lineflag = false;

            var resGRNSave = new SPGRNSaveResponse();
            //lrdate = dd_MM_yyyytoyyyy_MM_dd(lrdate);
            SPGRNHeaderRequest sPGRNHeaderRequest = new SPGRNHeaderRequest
            {
                documenttype = grnCardRequest.documenttype,
                documentno = grnCardRequest.documentno,
                postingdate = dd_MM_yyyytoyyyy_MM_dd(grnCardRequest.postingdate),
                documentdate = dd_MM_yyyytoyyyy_MM_dd(grnCardRequest.documentdate),
                referenceinvoiceno = grnCardRequest.referenceinvoiceno,
                qcremarks = grnCardRequest.qcremarks,
                lrno = grnCardRequest.lrno,
                lrdate = dd_MM_yyyytoyyyy_MM_dd(grnCardRequest.lrdate),
                transportername = grnCardRequest.transportername,
                transporterno = grnCardRequest.transporterno,
                vehicleno = grnCardRequest.vehicleno,
                transportationamount = grnCardRequest.transportationamount,
                loadingcharges = grnCardRequest.loadingcharges,
                unloadingcharges = grnCardRequest.unloadingcharges
            };
            List<SPGRNCardLineRequest> sPGRNCardLineRequests = grnCardRequest.grnCardLineRequest;

            var result = (dynamic)null;
            result = PostGRNCard("APIMngt_GRNPostHeader", sPGRNHeaderRequest, resGRNSave);

            if (result.Result.Item1 != null)
            {
                headerflag = Convert.ToBoolean(result.Result.Item1.value);
                if (headerflag)
                {
                    foreach (SPGRNCardLineRequest lineRequest in sPGRNCardLineRequests)
                    {
                        var resultLine = (dynamic)null;
                        lineRequest.bedate = dd_MM_yyyytoyyyy_MM_dd(lineRequest.bedate);
                        resultLine = PostGRNLine("APIMngt_GRNPostLines", lineRequest, resGRNSave);

                        if (resultLine.Result.Item1 != null)
                        {
                            lineflag = Convert.ToBoolean(resultLine.Result.Item1.value);
                            if (lineflag)
                            {

                            }
                            else
                            {

                            }
                        }
                    }
                }
            }

            return headerflag;
        }

        public string dd_MM_yyyytoyyyy_MM_dd(string dd_MM_yyyyDate)
        {
            if (dd_MM_yyyyDate != "" && dd_MM_yyyyDate != null)
            {
                DateTime yyyy_MM_ddDate;
                bool success = DateTime.TryParseExact(dd_MM_yyyyDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out yyyy_MM_ddDate);

                if (success)
                {
                    Console.WriteLine("TryParseExact date: " + yyyy_MM_ddDate);
                }
                else
                {
                    yyyy_MM_ddDate = DateTime.ParseExact(dd_MM_yyyyDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    Console.WriteLine("ParseExact date: " + yyyy_MM_ddDate);
                }

                return yyyy_MM_ddDate.ToString("yyyy-MM-dd");
            }
            else
            {
                return dd_MM_yyyyDate;
            }
        }

        public async Task<(SPGRNSaveResponse, errorDetails)> PostGRNCard<SPGRNHeaderRequest>(string apiendpoint, SPGRNHeaderRequest requestModel, SPGRNSaveResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPGRNSaveResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(SPGRNSaveResponse, errorDetails)> PostGRNLine<SPGRNCardLineRequest>(string apiendpoint, SPGRNCardLineRequest requestModel, SPGRNSaveResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPGRNSaveResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        [Route("GetGRNLineItemTrackingForPopup")]
        public List<SPGRNLineItemTracking> GetGRNLineItemTrackingForPopup(string DocumentType, string DocumentNo, string lineNo, int? sourceType = null, string sourceSubtype = null)
        {
            API ac = new API();
            dynamic result = null;
            string filter = "";
            List<SPGRNLineItemTracking> grnLineItemTracking = new List<SPGRNLineItemTracking>();

            int resolvedSourceType;
            string resolvedSourceSubtype;

            if (sourceType.HasValue && !string.IsNullOrWhiteSpace(sourceSubtype))
            {
                resolvedSourceType = sourceType.Value;
                resolvedSourceSubtype = sourceSubtype;
            }
            else if (!TryResolveItemTrackingSource(DocumentType, out resolvedSourceType, out resolvedSourceSubtype))
            {
                return grnLineItemTracking;
            }

            if (DocumentType == "Transfer Order")
            {
                filter = "Source_Type eq " + resolvedSourceType + " and Source_Subtype eq '" + resolvedSourceSubtype + "' and Source_ID eq '" + DocumentNo + "' and Transferred_from_Entry_No eq 0 and Source_Ref_No eq " + lineNo;
            }
            else
            {
                filter = "Source_Type eq " + resolvedSourceType + " and Source_Subtype eq '" + resolvedSourceSubtype + "' and Source_ID eq '" + DocumentNo + "' and Source_Ref_No eq " + lineNo;
            }

            result = ac.GetData<SPGRNLineItemTracking>("ItemTrackingLinesDotNetAPI", filter);

            if (result?.Result?.Item1?.value != null && result.Result.Item1.value.Count > 0)
                grnLineItemTracking = result.Result.Item1.value;


            for (int i = 0; i < grnLineItemTracking.Count; i++)
            {
                if (grnLineItemTracking[i].Expiration_Date != "0001-01-01")
                {
                    string[] strDate = grnLineItemTracking[i].Expiration_Date.Split('-');
                    grnLineItemTracking[i].Expiration_Date = strDate[2] + '-' + strDate[1] + '-' + strDate[0];
                }
                else
                {
                    grnLineItemTracking[i].Expiration_Date = "";
                }
            }

            return grnLineItemTracking;
        }

        private bool TryResolveItemTrackingSource(string documentType, out int sourceType, out string sourceSubtype)
        {
            sourceType = 0;
            sourceSubtype = "";

            switch (documentType)
            {
                case "Purchase Order":
                    sourceType = 39;
                    sourceSubtype = "1";
                    return true;
                case "Sales Return":
                    sourceType = 37;
                    sourceSubtype = "5";
                    return true;
                case "Sales Order":
                    sourceType = 37;
                    sourceSubtype = "1";
                    return true;
                case "Transfer Order":
                    sourceType = 5741;
                    sourceSubtype = "1";
                    return true;
                default:
                    return false;
            }
        }

        [Route("SaveGRNLineItemTracking")]
        public bool SaveGRNLineItemTracking(List<ReservationEntryForGRN> reservationEntryForGRNs)
        {
            bool headerflag = false;
            if (reservationEntryForGRNs == null || reservationEntryForGRNs.Count == 0)
            {
                return false;
            }

            foreach (var item in reservationEntryForGRNs)
            {
                if (item.ExpirationDate != null && item.ExpirationDate != "")
                {
                    item.ExpirationDate = dd_MM_yyyytoyyyy_MM_dd(item.ExpirationDate);
                }
            }

            var resGRNSave = new SPGRNSaveResponse();

            var json = JsonConvert.SerializeObject(reservationEntryForGRNs);
            var textWithSingleQuotes = json.Replace("\"", "'");

            ReservationEntryForGRNRequest reservationEntryForGRNRequest = new ReservationEntryForGRNRequest();
            reservationEntryForGRNRequest.text = textWithSingleQuotes;

            var result = (dynamic)null;
            result = PostGRNLineItemTracking("APIMngt_InsertUpdateReservationEntryforGRN", reservationEntryForGRNRequest, resGRNSave);

            if (result.Result.Item1 != null)
            {
                headerflag = Convert.ToBoolean(result.Result.Item1.value);

            }

            return headerflag;
        }

        public async Task<(SPGRNSaveResponse, errorDetails)> PostGRNLineItemTracking<ReservationEntryForGRNRequest>(string apiendpoint, ReservationEntryForGRNRequest requestModel, SPGRNSaveResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPGRNSaveResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }

        [Route("GetMakeMfgCodeAndName")]
        public List<SPGRNVendors> GetMakeMfgCodeAndName(string prefix)
        {
            API ac = new API();
            List<SPGRNVendors> makemfgcode = new List<SPGRNVendors>();
            var result = ac.GetData<SPGRNVendors>("VendorDotNetAPI", "startswith(Name,'" + prefix + "')");

            if (result != null && result.Result.Item1.value.Count > 0)
                makemfgcode = result.Result.Item1.value;
            return makemfgcode;


        }

        [HttpPost]
        [Route("DeleteGRNLineItemTracking")]
        public string DeleteGRNLineItemTracking([FromBody] DeleteReservationEntryForGRNRequest request)
        {
            string msg = "";

            var resDeleteitemline = new SPGRNSaveResponse();
            dynamic result = null;
            var requestDeleteitemline = new
            {
                request.entryno,
                request.positive
            };
            result = PostCodeUnit("APIMngt_DeleteReservationEntry", requestDeleteitemline, resDeleteitemline);
            if (result.Result.Item1 != null)
            {
                msg = result.Result.Item1.value;
            }

            return msg;
        }


        public async Task<(SPGRNSaveResponse, errorDetails)> PostCodeUnit<DeleteReservationEntryForGRNRequest>(string apiendpoint, DeleteReservationEntryForGRNRequest requestModel, SPGRNSaveResponse responseModel)
        {
            string _codeUnitBaseUrl = System.Configuration.ConfigurationManager.AppSettings["CodeUnitBaseURL"];
            string _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            string _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            string _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];

            API ac = new API();
            var accessToken = await ac.GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            //string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {

            }
            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<SPGRNSaveResponse>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }
            }
            return (responseModel, errordetail);
        }
    }
}
