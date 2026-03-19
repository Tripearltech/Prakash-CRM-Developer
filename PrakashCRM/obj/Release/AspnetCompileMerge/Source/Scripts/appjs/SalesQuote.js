var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
var InqNo = "", SQNo = "", ScheduleStatus = "", SQStatus = "", SQFor = "", LoggedInUser = "";

$(document).ready(function () {

    GetInterestRate();
    //alert($('#lblInterestRate').text());
    //alert("Interest Rate - " + $('#hfInterestRate').val());
    //$('#hfInterestRate').val(10);

    var UrlVars = getUrlVars();
    
    if (UrlVars["CompanyNo"] != undefined && UrlVars["CompanyName"] != undefined) {

        var companyName = UrlVars["CompanyName"];
        companyName = companyName.replaceAll("%20", " ");
        $('#hfCustomerName').val(companyName);
        $('#hfContactCompanyNo').val(UrlVars["CompanyNo"]);
    }

    //if ($('#hdnSalesQuoteAction').val() != "") {

    //    $('#divImage').hide();

    //    var SalesQuoteActionDetails = 'Sales Quote ' + $('#hdnSalesQuoteAction').val() + ' Successfully';
    //    var actionType = 'success';

    //    var actionMsg = SalesQuoteActionDetails;
    //    ShowActionMsg(actionMsg);

    //    $.get('NullSQSession', function (data) {

    //    });
    //}

    if ($('#hdnSalesQuoteActionErr').val() != "") {

        var SalesQuoteActionErr = $('#hdnSalesQuoteActionErr').val();

        $('#JustificationTitle').text("");
        $('#modalJustification').css('display', 'none');
        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text(SalesQuoteActionErr);

        $.get('NullSalesQuoteSession', function (data) {

        });

    }

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    //if ($('#hfIsSalesQuoteEdit').val() == "true") {

    //    GetSalesQuoteDetailsAndFill($('#hfSalesQuoteNo').val());

    //}

    //if ($('#hfIsSQHeaderAdded').val() == "true") {
    //    //GetInterestRate();
    //    BindVendors();
    //    productName_autocomplete();
    //    BindTransport();
    //    $('#dvSQLineDetails').css('display', 'block');
    //    BindSQLineDetails();
    //    //$('#txtCustomerName').change();
    //}
    //else {
    //    $('#dvSQLineDetails').css('display', 'none');
    //}

    var curDate = GetCurrentDate();

    $('#txtSalesQuoteDate').val(curDate);
    $('#txtSQValidUntillDate').val(curDate);

    /*$('#txtSalesQuoteDate').val("2025-01-05");*/

    //BindInquiries();
    //BindNoSeries();
    BindLocations();
    ////BindInquiryType();
    //BindPaymentMethod();
    BindPaymentTerms();
    BindIncoTerms();
    GetCustomerTemplateCode();

    //BindGSTPlaceOfSupply();
    companyName_autocomplete();
    $('#ddlPackingStyle').append("<option value='-1'>---Select---</option>");
    $('#ddlPackingStyle').val('-1');
    BindTransportMethod();
    BindVendors();

    if (UrlVars["InquiryNo"] != undefined) {
        
        InqNo = UrlVars["InquiryNo"];
        //var SQNo = GetSQNoFromInqNo(InqNo);

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        var SQNo = "", ScheduleStatus = "";

        $.get(apiUrl + 'GetSQNoFromInqNo?InqNo=' + InqNo, function (data) {

            if (data != "") {
                $('#txtInqNo').val(InqNo);

                const SQDetails = data.split('_');

                SQNo = SQDetails[0];
                ScheduleStatus = SQDetails[1];

                $('#dvInquiryLineDetails').css('display', 'block');
                BindInquiryLineDetails(InqNo);
                GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus);
            }
            else {
                $('#txtInqNo').val(InqNo);
                GetAndFillInquiryDetails(InqNo);
            }

        });

        //if (SQNo == "") {
        //    $('#txtInqNo').val(InqNo);
        //    GetAndFillInquiryDetails(InqNo);
        //}
        //else {
        //    GetSalesQuoteDetailsAndFill(SQNo);
        //}
        
    }

    if (UrlVars["SQNo"] != undefined && UrlVars["ScheduleStatus"] != undefined && UrlVars["SQStatus"] != undefined
        && UrlVars["SQFor"] != undefined && UrlVars["LoggedInUserRole"] != undefined && 
        UrlVars["LoggedInUserRole"] != "") {

        SQNo = UrlVars["SQNo"];
        ScheduleStatus = UrlVars["ScheduleStatus"];
        SQStatus = UrlVars["SQStatus"];
        SQFor = UrlVars["SQFor"];
        LoggedInUserRole = UrlVars["LoggedInUserRole"];
        GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole);

    }
    else if (UrlVars["SQNo"] != undefined && UrlVars["ScheduleStatus"] != undefined && UrlVars["SQStatus"] != undefined
        && UrlVars["SQFor"] != undefined && UrlVars["LoggedInUserRole"] == "") {

        SQNo = UrlVars["SQNo"];
        ScheduleStatus = UrlVars["ScheduleStatus"];
        SQStatus = UrlVars["SQStatus"];
        SQFor = UrlVars["SQFor"];
        LoggedInUserRole = $('#hdnLoggedInUserRole').val();
        GetSalesQuoteDetailsAndFill(SQNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole);

    }

    //$('#ddlInquiries').change(function () {

    //    GetAndFillInquiryDetails($('#ddlInquiries').val());

    //});
    
    //$('#ddlNoSeries').change(function () {

    //    $('#hfNoSeriesCode').val($('#ddlNoSeries').val());

    //    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    //    $.get(apiUrl + 'GetLocationCode?NoSeriesCode=' + $('#ddlNoSeries').val(), function (data) {
            
    //        $('#hfLocationCode').val(data);
            
    //    });

    //});

    //if ($('#hfSavedCustomerName').val() != "") {
    //    GetContactsOfCompany($('#txtCustomerName').val());
    //    GetCreditLimitAndCustDetails($('#txtCustomerName').val());
    //}

    $('#txtCustomerName').blur(function () {
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    });

    $('#txtProductName').blur(function () {
        GetProductDetails($('#txtProductName').val());
        CalculateFormula();
        $('#btnSaveProd').css('display', 'block');
        $('#btnSave').css('display', 'block');
    });

    $('#txtAdditionalQty').change(function () {

        AdditionalQtyChange();

    });

    $('#txtPurDiscount').change(function () {

        //CalculateFormula();

    });

    $('#ddlIncoTerms').change(function () {

        $('#ddlTransportMethod').prop('disabled', false);
        $('#txtLineDetailsIncoTerms').val($('#ddlIncoTerms option:selected').text());
        $('#txtLineDetailsIncoTerms').attr('readonly', true);
        $('#ddlTransportMethod').val("-1");
        $('#txtTransportCost').val("0");
        $('#txtInsurance').val("0");

        if ($('#ddlIncoTerms').val() == "CFR" || $('#ddlIncoTerms').val() == "CIF" || $('#ddlIncoTerms').val() == "DELIVERED") {
            $('#btnShowTPDet').css('display', 'block');
            $('#ddlTransportMethod').val("PAID");
            $('#ddlTransportMethod').prop('disabled', true);
        }
        else if ($('#ddlIncoTerms').val() == "EXF" || $('#ddlIncoTerms').val() == "EXW" || $('#ddlIncoTerms').val() == "FOB") {
            $('#btnShowTPDet').css('display', 'none');
            $('#ddlTransportMethod').val("TOPAY");
            $('#ddlTransportMethod').prop('disabled', true);
        }
        else {
            $('#ddlTransportMethod').prop('disabled', false);
        }

        //if ($('#ddlIncoTerms').val() == "EXF" || $('#ddlIncoTerms').val() == "EXW") {
        //    $('#btnShowTPDet').css('display', 'none');
        //    $('#ddlTransportMethod').val("TOPAY");
        //    $('#ddlTransportMethod').css('disabled', true);
        //    $('#txtInsurance').css('disabled', true);
        //}
        //else if ($('#ddlIncoTerms').val() == "CIF" || $('#ddlIncoTerms').val() == "DELIVERED") {
        //    $('#btnShowTPDet').css('display', 'block');
        //    $('#ddlTransportMethod').val("PAID");
        //    $('#ddlTransportMethod').css('disabled', false);
        //    $('#txtInsurance').css('disabled', false);
        //}
        //else if ($('#ddlIncoTerms').val() == "CFR" || $('#ddlIncoTerms').val() == "FOB") {
        //    $('#btnShowTPDet').css('display', 'block');
        //    $('#ddlTransportMethod').css('disabled', false);
        //    $('#txtInsurance').css('disabled', true);
        //}
        //else {
        //    $('#btnShowTPDet').css('display', 'block');
        //    $('#ddlTransportMethod').css('disabled', false);
        //    $('#txtInsurance').css('disabled', false);
        //}
        
        CalculateFormula();
        ////UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#txtSalesDiscount').change(function () {

        CalculateFormula();

    });

    //$('#chkIsSalesDisc').change(function () {

    //    var isChkSalesDiscChecked = $(this).is(":checked");

    //    if (isChkSalesDiscChecked == true) {
    //        $('#ddlSalesDiscPerUnitPercent').prop('disabled', false);
    //    }
    //    else {
    //        $('#ddlSalesDiscPerUnitPercent').val('-1');
    //        $('#ddlSalesDiscPerUnitPercent, #txtSalesDiscAmount').prop('disabled', true);
    //    }

    //});

    //$('#ddlSalesDiscPerUnitPercent').change(function () {

    //    if ($(this).val() != "-1") {

    //        if ($('#ddlSalesDiscPerUnitPercent').val() == "%") {

    //            $('#txtSalesDiscPercent,#txtSalesDiscAmount').prop('disabled', false);
    //            //$('#txtSalesDiscAmount').prop('disabled', false);
    //        }
    //        else {
    //            $('#txtSalesDiscPercent,#txtSalesDiscAmount').val("");
    //            $('#txtSalesDiscPercent').prop('disabled', true);
    //            $('#txtSalesDiscAmount').prop('disabled', false);
    //        }
            
    //    }
    //    else {
    //        $('#txtSalesDiscPercent,#txtSalesDiscAmount').prop('disabled', true);
    //        //$('#txtSalesDiscAmount').prop('disabled', true);
    //    }

    //});

    //$('#txtSalesDiscPercent').change(function () {

    //    if ($('#txtSalesDiscPercent').val() != "" || parseInt($('#txtSalesDiscPercent').val()) > 0) {

    //        $('#txtSalesDiscAmount').val(($('#txtSalesPrice').val() * $('#txtSalesDiscPercent').val()) / 100);
    //        $('#txtSalesDiscAmount').change();

    //    }
 
    //});

    //$('#txtSalesDiscAmount').change(function () {

    //    CalculateFormula();
    //    //UpdateValueForTotalCost();
    //    /*$('#txtAdditionalQty').change();
    //    //CalculateFormula();*/

    //});

    $('#ddlTransportMethod').change(function () {

        $('#txtTransportCost').val("0");

        if ($('#ddlTransportMethod').val() == "-1") {

            var msg = "Please Select Transport Method";
            ShowErrMsg(msg);

        }
        else {
            if ($("#ddlTransportMethod option:selected").text() == "ToPay") {
                $('#btnShowTPDet').css('display', 'none');
            }
            else {
                $('#btnShowTPDet').css('display', 'block');
            }
        }
        //CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#ddlCommissionPayable').change(function () {

        if ($('#ddlCommissionPayable').val() == "-1") {
            $('#txtCommissionAmt').val("0");
        }
        //CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#txtTransportCost').change(function () {

        CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#chkIsCommission').change(function () {

        var isChkCommissionChecked = $(this).is(":checked");

        if (isChkCommissionChecked == true) {
            $('#ddlCommissionPerUnitPercent').prop('disabled', false);
        }
        else {
            $('#ddlCommissionPerUnitPercent').val('-1');
            $('#txtCommissionPercent, #txtCommissionAmt').val("");
            $('#ddlCommissionPayable').val('-1');
            $('#ddlCommissionPerUnitPercent, #txtCommissionPercent, #txtCommissionAmt, #ddlCommissionPayable').prop('disabled', true);
            CalculateFormula();
        }

    });

    $('#ddlCommissionPerUnitPercent').change(function () {

        if ($(this).val() != "-1") {

            if ($('#ddlCommissionPerUnitPercent').val() == "%") {

                $('#txtCommissionPercent, #txtCommissionAmt').prop('disabled', false);
                //$('#txtCommissionAmt').prop('disabled', false);
            }
            else {
                $('#txtCommissionPercent, #txtCommissionAmt').val("");
                $('#txtCommissionPercent, #ddlCommissionPayable').prop('disabled', true);
                $('#txtCommissionAmt').prop('disabled', false);
                $('#txtCommissionAmt').prop('readonly', false);
                
            }

        }
        else {
            $('#txtCommissionPercent,#txtCommissionAmt').prop('disabled', true);
            //$('#txtCommissionAmt').prop('disabled', true);
        }

    });

    $('#txtCommissionPercent').change(function () {

        if ($('#txtCommissionPercent').val() != "" || parseInt($('#txtCommissionPercent').val()) > 0) {

            $('#txtCommissionAmt').val((($('#txtSalesPrice').val() * $('#txtCommissionPercent').val()) / 100).toFixed(2));
            $('#txtCommissionAmt').prop('readonly', true);
            $('#txtCommissionAmt').change();

        }
        
    });

    $('#txtCommissionAmt').change(function () {

        if ($('#txtCommissionAmt').val() != "" && parseFloat($('#txtCommissionAmt').val()) > 0) {

            $('#ddlCommissionPayable').prop('disabled', false);
        }

        //if (($('#ddlCommissionPayable').val() == "-1" && parseInt($('#txtCommissionAmt').val()) > 0)) {
        //    $('#txtCommissionAmt').val("0");
        //}
        CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#ddlPaymentTerms').change(function () {

        PaymentTermsChange();
        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        $('#txtLineDetailsPaymentTerms').attr('readonly', true);

    });

    $('#txtInsurance').change(function () {

        //CalculateFormula();
        //UpdateValueForTotalCost();
        $('#txtAdditionalQty').change();

    });

    $('#txtSalesPrice').blur(function () {

        CalculateFormula();
        //UpdateValueForTotalCost();
        /*$('#txtAdditionalQty').change();*/

    });

    $('#ddlTaxGroup').change(function () {

        //CalculateFormula();
        //UpdateValueForTotalCost();
        $('#txtAdditionalQty').change();

    });

    $('#ddlGSTPlaceOfSupply').change(function () {

        //CalculateFormula();
        //UpdateValueForTotalCost();
        $('#txtAdditionalQty').change();

    });

    $('#chkDropShipment').change(function () {

        var isChecked = $(this).is(":checked");

        if (isChecked == true) {
            BindItemVendors($('#hfProdNo').val());
            $('#dvVendors').css('display', 'block');
            $('#ddlItemVendors').css('display', 'block');
        }
        else {
            $('#ddlItemVendors').val('-1');
            $('#dvVendors').css('display', 'none');
        }

    });

    $('#ddlPackingStyle').change(function () {

        const packingStyleDetails = $('#ddlPackingStyle').val().split('_');
        $('#txtBasicPurchaseCost').val(parseFloat(packingStyleDetails[0]));
        $('#txtBasicPurchaseCost').prop('disabled', true);
        $('#hfPurchaseDays').val(parseInt(packingStyleDetails[2]));
        
    });

    $('#btnResetProdDetails').click(function () {

        ResetQuoteLineDetails();

    });
    

    $('#btnSaveProd').click(function () {

        var numbers = /^[0-9]+$/;
        var prodQty = $('#txtProdQty').val();
        var salesPrice = $('#txtSalesPrice').val();
        var transportCost = $('#txtTransportCost').val();
        var salesDiscount = $('#txtSalesDiscount').val();
        var commissionPercent = $('#txtCommissionPercent').val();

        if ($('#dvLiquidProdFields').css('display') == "none" && ($('#txtProductName').val() == "" || $('#txtProdQty').val() == "" || $('#ddlPackingStyle').val() == "-1" || $('#txtSalesPrice').val() == "" ||
            $('#txtDeliveryDate').val() == "")) {

            var errMsg = "Please fill product details";
            ShowErrMsg(errMsg);

        }
        else if ($('#dvLiquidProdFields').css('display') == "block" && ($('#txtProductName').val() == "" || $('#txtConcentratePercent').val() == "" || $('#txtNetWeight').val() == "" || $('#txtLiquidRate').val() == "" ||
            $('#txtProdQty').val() == "" || $('#ddlPackingStyle').val() == "-1" || $('#txtDeliveryDate').val() == "")) {

            var errMsg = "Please fill product details";
            ShowErrMsg(errMsg);

        }
        else if ($('#dvLiquidProdFields').css('display') == "block" && (parseFloat($('#txtConcentratePercent').val()) < 0 || parseInt($('#txtNetWeight').val()) < 0 || parseFloat($('#txtLiquidRate').val()) < 0)) {

            var errMsg = "Liquid product details should be greater than 0";
            ShowErrMsg(errMsg);
        }
        else if ($('#dvLiquidProdFields').css('display') == "block" && parseFloat($('#txtNetWeight').val()) > parseFloat($('#lblGrossWeight').text())) {
            var errMsg = "New Weight should be <= Gross Weight";
            ShowErrMsg(errMsg);
        }
        else if ($('#dvLiquidProdFields').css('display') == "none" && !prodQty.match(numbers)) {

            $('#lblNetWeightErrMsg').css('display', 'none');
            var errMsg = "Qty should be in numeric";
            ShowErrMsg(errMsg);
        }
        else if (prodQty <= 0) {

            var errMsg = "Please fill qty greater than 0";
            ShowErrMsg(errMsg);
        }
        else if ($('#dvLiquidProdFields').css('display') == "none" && !salesPrice.match(numbers)) {

            var errMsg = "Sales price should be in numeric";
            ShowErrMsg(errMsg);
        }
        else if ($('#dvLiquidProdFields').css('display') == "none" && salesPrice <= 0) {

            var errMsg = "Please fill sales price greater than 0";
            ShowErrMsg(errMsg);
        }
        else if (!transportCost.match(numbers)) {

            var errMsg = "Transport cost should be in numeric";
            ShowErrMsg(errMsg);
        }
        else if (!salesDiscount.match(numbers)) {

            var errMsg = "Sales discount should be in numeric";
            ShowErrMsg(errMsg);
        }
        else if (commissionPercent != "" && !commissionPercent.match(numbers)) {

            /*if (!commissionPercent.match(numbers)) {*/
            var errMsg = "Commission percent should be in numeric";
            ShowErrMsg(errMsg);
            /*}*/

        }
        else {

            var prodOpts = "";
            var prodOptsTR = "";

            $('#dataList').css('display', 'block');

            //

            if ($('#hfProdNoEdit').val() != "") {

                $("#ProdTR_" + $('#hfProdNoEdit').val()).html("");
                prodOptsTR = "";

            }
            else {
                prodOptsTR = "<tr id=\"ProdTR_" + $('#hfProdNo').val() + "\">";
            }


            //

            prodOpts = "<td></td><td><a class='SQLineCls' onclick='EditSQProd(\"ProdTR_" + $('#hfProdNo').val() + "\")'><i class='bx bxs-edit'></i></a>";

            if ($('#hfProdNoEdit').val() == "") {
                prodOpts += "&nbsp;<a class='SQLineCls' onclick='DeleteSQProd(\"ProdTR_" + $('#hfProdNo').val() + "\")'><i class='bx bxs-trash'></i></a>";
            }

            prodOpts += "</td><td>" + $('#hfProdNo').val() + "</td><td>" + $('#txtProductName').val() + "</td><td>" + $('#txtProdQty').val() + "</td><td>" +
                $('#txtUOM').val() + "</td>";

            var packingStyleDetails = $('#ddlPackingStyle').val().split('_');

            prodOpts += "<td>" + packingStyleDetails[1] + "</td><td>" + $('#txtBasicPurchaseCost').val() + "</td>";

            prodOpts += "<td>" + $('#txtSalesPrice').val() + "</td><td>" +
                $('#txtDeliveryDate').val() + "</td>";

            if ($('#hfSavedTotalCost').val() != "") {
                prodOpts += "<td id=\"" + $('#hfProdNo').val() + "_TotalCost\">" + parseFloat($('#hfSavedTotalCost').val()) + "</td>";
            }
            else {
                prodOpts += "<td id=\"" + $('#hfProdNo').val() + "_TotalCost\">" + parseFloat($('#txtTotalCost').val()) + "</td>";
            }

            if ($('#hfSavedMargin').val() != "") {
                prodOpts += "<td id=\"" + $('#hfProdNo').val() + "_Margin\">" + parseFloat($('#hfSavedMargin').val()) + "</td>";
            }
            else {
                prodOpts += "<td id=\"" + $('#hfProdNo').val() + "_Margin\">" + parseFloat($('#txtMargin').val()) + "</td>";
            }

            //if ($('#tblLotNoWiseQtyDetails').html() != "") {

            //    var InvQtyTable = "<td style='display:none'><table id=\"" + $('#hfProdNo').val() + "_InvDetails\" style='display:none'>";
            //    var QtyTROpts = "";
            //    $('#tblLotNoWiseQtyDetails tr').each(function () {

            //        var row = $(this)[0];
            //        var ItemNo = row.cells[2].innerHTML;
            //        var LotNo = row.cells[3].innerHTML;
            //        var ReqQty = parseInt(row.cells[4].innerHTML);
            //        var LocCode = row.cells[5].innerHTML;;

            //        QtyTROpts += "<tr><td></td><td></td><td>" + ItemNo + "</td><td>" + LotNo + "</td><td>" + ReqQty + "</td><td>" + LocCode + "</td></tr>";

            //    });

            //    InvQtyTable += QtyTROpts + "</table>";
            //    prodOpts += InvQtyTable + "</td>";
            //    $('#tblLotNoWiseQtyDetails').empty();
            //}
            //else {

            //    prodOpts += "<td style='display:none'></td>";

            //}

            const paymentTermsDetails = $('#ddlPaymentTerms').val().split('_');
            prodOpts += "<td>" + paymentTermsDetails[0] + "</td><td>" + $('#ddlIncoTerms').val() + "</td><td>" + $('#ddlTransportMethod').val() + "</td><td>" +
                $('#txtTransportCost').val() + "</td><td>" + $('#txtSalesDiscount').val() + "</td>";

            //if ($('#chkIsSalesDisc').is(":checked") == true) {

            //    if ($('#ddlSalesDiscPerUnitPercent').val() != "-1") {
            //        prodOpts += "<td>" + $('#ddlSalesDiscPerUnitPercent').val() + "</td><td>" + $('#txtSalesDiscPercent').val() + "</td><td>" + $('#txtSalesDiscAmount').val() + "</td>";
            //    }
            //    else {
            //        prodOpts += "<td></td><td></td><td></td>";
            //    }
            //}
            //else {
            //    prodOpts += "<td></td><td></td><td></td>";
            //}

            if ($('#chkIsCommission').is(":checked") == true) {

                if ($('#ddlCommissionPerUnitPercent').val() != "-1") {
                    prodOpts += "<td>" + $('#ddlCommissionPerUnitPercent').val() + "</td><td>" + $('#txtCommissionPercent').val() + "</td><td>" + $('#txtCommissionAmt').val() + "</td>";
                }
                else {
                    prodOpts += "<td></td><td></td><td></td>";
                }
            }
            else if ($('#hfIsLiquidProd').val() == "true") {
                prodOpts += "<td></td><td></td><td>" + $('#txtCommissionAmt').val() + "</td>";
            }
            else {
                prodOpts += "<td></td><td></td><td></td>";
            }

            prodOpts += "<td>" + $('#txtCreditDays').val() + "</td><td>" + $('#txtInterest').val() + "</td>";

            var dropShipmentOpt;
            if ($('#chkDropShipment').is(":checked") == true) {
                dropShipmentOpt = "Yes";
            }
            else {
                dropShipmentOpt = "No";
            }

            prodOpts += "<td><label id=\"" + $('#hfProdNo').val() + "_DropShipment\">" + dropShipmentOpt + "</lable></td>";

            if ($('#hfProdLineNo').val() != "") {

                prodOpts += "<td hidden><label id=\"" + $('#hfProdNo').val() + "_InqProdLineNo\">" + $('#hfProdLineNo').val() + "</label></td>";
                $('#hfProdLineNo').val("");
            }
            else {

                prodOpts += "<td hidden></td>";

            }

            prodOpts += "<td hidden>" + $('#ddlCommissionPayable').val() + "</td><td>" + $('#ddlCommissionPayable option:selected').text() + "</td>" +
                "<td hidden>" + $('#ddlItemVendors').val() + "</td><td>" + $('#ddlItemVendors option:selected').text() + "</td>";

            if ($('#hfSQProdLineNo').val() != "") {

                prodOpts += "<td hidden><label id=\"" + $('#hfProdNo').val() + "_SQLineNo\" style='display:none'>" + $('#hfSQProdLineNo').val() + "</label></td>";

            }
            else { 
                prodOpts += "<td hidden></td>";
            }
            
            prodOpts += "<td><label id=\"" + $('#hfProdNo').val() + "_MarginPercent\">" + $('#spnMarginPercent').text() + "</label></td>";

            //"<td hidden>" + $('#hfIsLiquidProd').val() + "</td>";
            if ($('#chkIsLiquidProd').prop('checked')) {
                prodOpts += "<td hidden>true</td>";
            }
            else {
                prodOpts += "<td hidden>false</td>";
            }
            
            if ($('#dvLiquidProdFields').css('display') == "block") {
                prodOpts += "<td hidden>" + $('#txtConcentratePercent').val() + "</td><td hidden>" + $('#txtNetWeight').val() + "</td><td hidden>" + $('#txtLiquidRate').val() + "</td>";
            }
            else {
                prodOpts += "<td></td><td></td><td></td>";
            }

            if ($('#hfProdNoEdit').val() != "") {

                $("#ProdTR_" + $('#hfProdNoEdit').val()).append(prodOpts);

            }
            else {
                prodOptsTR += prodOpts + "</tr>";
                $('#tblProducts').append(prodOptsTR);
            }

            //if (parseFloat($('#txtAvailableCreditLimit').val()) > 0) {
                $('#txtAvailableCreditLimit').prop('disabled', false);
                var availableCreditLimit = parseFloat($('#txtAvailableCreditLimit').val().replaceAll(",", "")) - (parseFloat($('#txtSalesPrice').val()) * parseFloat($('#txtProdQty').val()));
                $('#txtAvailableCreditLimit').val(commaSeparateNumber(availableCreditLimit.toFixed(2)));
                $('#txtAvailableCreditLimit').prop('disabled', true);
            //}

            dataTableFunction();
            ResetQuoteLineDetails();
            

        }
        
        /*
        if (($('#ddlCommissionPayable').val() != "-1" && $('#txtCommissionAmt').val() == "") || ($('#ddlCommissionPayable').val() != "-1" && $('#txtCommissionAmt').val() == "0")) {

            var msg = "Please Enter Commission Amount";
            ShowErrMsg(msg);

        }
        if (($('#ddlTransport').val() != "-1" && $('#txtTransportCost').val() == "") || ($('#ddlTransport').val() != "-1" && $('#txtTransportCost').val() == "0")) {

            var msg = "Please Enter Transport Cost";
            ShowErrMsg(msg);

        }
        if ($('#txtProdQty').val() == "0" || $('#txtProdQty').val() == "") {

            var msg = "Please Enter Valid Quantity";
            ShowErrMsg(msg);

        }

        if ($('#hfiteminvstatus').val() == "NotInInventory") {

            var UOM = $('#hfUOM').val();
            let salesQuoteDetails = {};

            salesQuoteDetails.Document_No = $('#hfQuoteNo').val();
            salesQuoteDetails.No = $('#hfProdNo').val();
            //salesQuoteDetails.Location_Code = $('#hfLocationCode').val();
            salesQuoteDetails.Quantity = $('#txtProdQty').val();
            salesQuoteDetails.Unit_of_Measure_Code = $('#hfUOM').val();
            //salesQuoteDetails.Unit_Price = $('#txtBasicPurchaseCost').val();
            salesQuoteDetails.PCPL_MRP = $('#txtProdMRP').val();
            salesQuoteDetails.PCPL_Basic_Price = $('#txtBasicPurchaseCost').val();
            salesQuoteDetails.Unit_Price = $('#txtSalesPrice').val();
            salesQuoteDetails.PCPL_Packing_Style_Code = $('#ddlPackingStyle').val();
            salesQuoteDetails.PCPL_Sales_Discount = $('#txtSalesDiscAmount').val();
            salesQuoteDetails.PCPL_Commission_Payable = $('#ddlCommissionPayable').val();
            salesQuoteDetails.PCPL_Commission_Amount = $('#txtCommissionAmt').val();
            salesQuoteDetails.PCPL_Transport_Method = $('#ddlTransport').val();
            salesQuoteDetails.PCPL_Transport_Amount = $('#txtTransportCost').val();
            salesQuoteDetails.PCPL_Margin = $('#txtMargin').val();
            salesQuoteDetails.PCPL_Credit_Days = 10;
            salesQuoteDetails.PCPL_Sales_Price = $('#txtSalesPrice').val();
            salesQuoteDetails.PCPL_Interest = $('#lblInterest').text();
            salesQuoteDetails.PCPL_Interest_Rate = $('#hfInterestRate').val();  //0.5;

            //salesQuoteDetails.Total_Amount_Excl_VAT = $('#txtTotalCost').val();

            var jsonSalesQuoteDetails = JSON.stringify(salesQuoteDetails);

            $.ajax({
                type: "POST",
                url: "/SPSalesQuotes/AddUpdateOnSaveProd",
                data: jsonSalesQuoteDetails,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {

                }
            });

            var actionMsg = "SQ Line Details Added Successfully";
            ShowActionMsg(actionMsg);

            ResetQuoteLineDetails();
            location.reload(true);
            //BindSQLineDetails();
            
           
        }*/

        //if ($('#hfiteminvstatus').val() == "NotInInventory") {

        //    var UOM = $('#hfUOM').val();

        //    let salesQuoteDetails = {};

        //    if ($('#ddlNoSeries').val() == "---Select---") {
        //        salesQuoteDetails.LocationName = "";
        //        salesQuoteDetails.Location_Code = "";
        //    }
        //    else {
        //        salesQuoteDetails.LocationName = $("#ddlNoSeries option:selected").text();
        //        salesQuoteDetails.Location_Code = $('#ddlNoSeries').val();
        //    }
        //    salesQuoteDetails.DailyVisitNo = $('#hfDailyVisitNo').val();
        //    salesQuoteDetails.InquiryNo = $('#lblInqNo').val();
        //    salesQuoteDetails.EntryNo = "";
        //    salesQuoteDetails.SupplierName = "";
        //    salesQuoteDetails.ManufactureName = "";
        //    salesQuoteDetails.ManufacturingExciseAmount = parseFloat(0);
        //    salesQuoteDetails.DealerName = "";
        //    salesQuoteDetails.DealerExciseAmount = parseFloat(0);
        //    salesQuoteDetails.AvailableQty = parseFloat(0);

        //    if ($('#txtProdQty').val() == "") {
        //        salesQuoteDetails.RequiredQty = parseFloat(0);
        //    }
        //    else {
        //        salesQuoteDetails.RequiredQty = $('#txtProdQty').val();
        //    }

        //    salesQuoteDetails.UOM = UOM;
        //    //salesQuoteDetails.Order_Date = "";
        //    salesQuoteDetails.DocumentNo = "";
        //    salesQuoteDetails.ManufacturerQty = parseFloat(0);
        //    salesQuoteDetails.TradeDiscount = parseFloat(0);
        //    salesQuoteDetails.QuantityDiscount = parseFloat(0);
        //    salesQuoteDetails.ConsigneeDiscount = parseFloat(0);

        //    var excise = 0;
        //    if ($('$txtExcise').val() == "") {
        //        excise = parseFloat(0);
        //    }
        //    else {
        //        excise = parseFloat($('$txtExcise').val());
        //    }

        //    var qty = 0;
        //    if ($('#txtProdQty').val() == "") {
        //        qty = parseFloat(0);
        //    }
        //    else {
        //        qty = $('#txtProdQty').val();
        //    }

        //    salesQuoteDetails.ExciseAmt = excise * qty;
        //    var BasicPrice = 0;
        //    if ($('#txtBasicPurchaseCost').val() == "") {
        //        BasicPrice = parseFloat(0);
        //    }
        //    else {
        //        BasicPrice = parseFloat($('#txtBasicPurchaseCost').val());
        //    }

        //    salesQuoteDetails.BasicPriceAmt = BasicPrice * qty;
        //    salesQuoteDetails.Prod_No = $('#hfProdNo').val();
        //    salesQuoteDetails.ProductName = $('#txtProductName').val();
        //    if ($('#ddlPackingStyle').val() == "---Select---") {
        //        salesQuoteDetails.PackagingStyle = "";
        //    }
        //    else {
        //        salesQuoteDetails.PackagingStyle = $('#ddlPackingStyle').val();
        //    }
        //    var SaleDisc = 0;
        //    if ($('#txtSalesDiscount').val() == "") {
        //        SaleDisc = parseFloat(0);
        //    }
        //    else {
        //        SaleDisc = parseFloat($('#txtSalesDiscount').val());
        //    }
        //    salesQuoteDetails.SalesDiscount = SaleDisc * qty;
        //    if ($('#ddlCommissionPayable').val() == "---Select---") {
        //        salesQuoteDetails.Commission = "";
        //    }
        //    else {
        //        salesQuoteDetails.Commission = $('#ddlCommissionPayable').val();
        //    }
        //    var CommissionAmt = 0;
        //    if ($('#txtCommission').val() == "") {
        //        CommissionAmt = parseFloat(0);
        //    }
        //    else {
        //        CommissionAmt = $('#txtCommission').val();
        //    }
        //    salesQuoteDetails.CommissionAmount = CommissionAmt * qty;
        //    if ($('#ddlIncoTerms').val() == "---Select---") {
        //        salesQuoteDetails.IncoTerms = "";
        //    }
        //    else {
        //        salesQuoteDetails.IncoTerms = $('#ddlIncoTerms').val();
        //    }
        //    if ($('#ddlSCVD').val() == "---Select---") {
        //        salesQuoteDetails.SVCD = "";
        //    }
        //    else {
        //        salesQuoteDetails.SVCD = $('#ddlSCVD').val();
        //    }
        //    if ($('#ddlTransport').val() == "---Select---") {
        //        salesQuoteDetails.Transport = "";
        //    }
        //    else {
        //        salesQuoteDetails.Transport = $('#ddlTransport').val();
        //    }
        //    var TranAmt = 0;
        //    if ($('#txtTransportCost').val() == "") {
        //        TranAmt = parseFloat(0);
        //    }
        //    else {
        //        TranAmt = parseFloat($('#txtTransportCost').val());
        //    }
        //    salesQuoteDetails.TransportAmount = TranAmt * qty;
        //    var CostPrice = 0;
        //    if ($('#txtTotalCost').val() == "") {
        //        CostPrice = parseFloat(0);
        //    }
        //    else {
        //        CostPrice = parseFloat($('#txtTotalCost').val());
        //    }
        //    salesQuoteDetails.TotalCost = CostPrice * qty;
        //    salesQuoteDetails.PaymentTerms = $('#ddlPaymentTerms').val();
        //    var mrgn = 0;
        //    if ($('#txtMargin').val() == "") {
        //        mrgn = parseFloat(0);
        //    }
        //    else {
        //        mrgn = parseFloat($('#txtMargin').val());
        //    }
        //    salesQuoteDetails.Margin = mrgn * qty;
        //    salesQuoteDetails.CreditDays = parseFloat($('#txtCreditDays').val()) * qty;
        //    var sPrice = 0;
        //    if ($('#txtSalesPrice').val() == "") {
        //        sPrice = parseFloat(0);
        //    }
        //    else {
        //        sPrice = parseFloat($('#txtSalesPrice').val());
        //    }
        //    salesQuoteDetails.SalesPrice = sPrice * qty;
        //    var Ins = 0;
        //    if ($('#txtInsurance').val() == "") {
        //        Ins = parseFloat(0);
        //    }
        //    else {
        //        Ins = $('#txtInsurance').val();
        //    }
        //    salesQuoteDetails.Insurance = Ins * qty;
        //    if ($('#ddlTaxGroup').val() == "---Select---") {
        //        salesQuoteDetails.TaxGroups = "";
        //    }
        //    else {
        //        salesQuoteDetails.TaxGroups = $('#ddlTaxGroup').val();
        //    }
        //    var Intrst = 0;
        //    if ($('#lblInterest').val() == "") {
        //        Intrst = parseFloat(0);
        //    }
        //    else {
        //        Intrst = $('#lblInterest').val();
        //    }
        //    salesQuoteDetails.Interest = Intrst * qty;
        //    if ($('#spnInterestRate').text() == "") {
        //        salesQuoteDetails.InterestRate = parseFloat(0);
        //    }
        //    else {
        //        salesQuoteDetails.InterestRate = parseFloat($('#spnInterestRate').text());
        //    }
        //    var TaxGroupAmt = 0;
        //    if ($('#lblTaxGroupAmount').val() == "") {
        //        TaxGroupAmt = parseFloat(0);
        //    }
        //    else {
        //        TaxGroupAmt = parseFloat($('#lblTaxGroupAmount').val());
        //    }
        //    salesQuoteDetails.TaxGroupAmount = TaxGroupAmt * qty;
        //    var totalSPrice = 0;
        //    if ($('#txtTotalSales').val() == "") {
        //        totalSPrice = parseFloat(0);
        //    }
        //    else {
        //        totalSPrice = parseFloat($('#txtTotalSales').val());
        //    }
        //    salesQuoteDetails.TotalSalesPrice = totalSPrice * qty;
        //    salesQuoteDetails.Type = "NotInInventory";
        //    if ($('#ddlExcise').val() == "---Select---") {
        //        salesQuoteDetails.ExciseType = "";
        //    }
        //    else {
        //        salesQuoteDetails.ExciseType = $('#ddlExcise').val();
        //    }
        //    var itemMrp = 0;
        //    if ($('#txtProdMRP').val() == "") {
        //        itemMrp = parseFloat(0);
        //    }
        //    else {
        //        itemMrp = parseFloat($('#txtProdMRP').val());
        //    }
        //    salesQuoteDetails.MRP = itemMrp * qty;
        //    salesQuoteDetails.InquiryType = $('#ddlInquiryType').val();
        //    salesQuoteDetails.Sell_to_Customer_No = $('#hfCustomerNo').val();
        //    if ($('#ddlContactName').val() == "---Select---") {
        //        salesQuoteDetails.Sell_to_Contact = "";
        //    }
        //    else {
        //        salesQuoteDetails.Sell_to_Contact = $('#ddlContactName').val();
        //    }
        //    salesQuoteDetails.Order_Date = $('#txtSalesQuoteDate').val();
        //    salesQuoteDetails.PaymentMethod = $('#ddlPaymentMethods').val();
        //    if ($("#ddlConsigneeAddress").val() == "---Select---") {
        //        salesQuoteDetails.ConsigneeAddress = "";
        //    }
        //    else {
        //        salesQuoteDetails.ConsigneeAddress = $("#ddlConsigneeAddress option:selected").text();
        //    }
        //    salesQuoteDetails.DeliveryDate = $('#txtDeliveryDate').val();
        //    salesQuoteDetails.AED = parseFloat($('#hfAEDAmt').val()) * qty;
        //    salesQuoteDetails.Remarks = "";
        //    salesQuoteDetails.ConsigneeCode = $('#hfCustomerNo').val();
        //    salesQuoteDetails.GSTPlaceOfSupply = $('#ddlGSTPlaceOfSupply').val();
        //    //salesQuoteDetails.IPAddress =
        //    //salesQuoteDetails.Browser =
        //    //salesQuoteDetails.WebURL =
        //    //salesQuoteDetails.ModifiedBy =
        //    //salesQuoteDetails.Msg =

        //    var jsonSalesQuoteDetails = JSON2.stringify(salesQuoteDetails);

        //    $.ajax({
        //        type: "POST",
        //        url: "/SPSalesQuotes/AddUpdateOnSaveProd",
        //        data: jsonSalesQuoteDetails,
        //        contentType: "application/json; charset=utf-8",
        //        dataType: "json",
        //        success: function (msg) {
        //            alert('In Ajax');
        //        }
        //    });
        //}
        //bindSalesQuoteDetailLineItems();
        //updateInquiryNotifcationStatus();

        //$.ajax(
        //    {
        //        url: '/SPSalesQuotes/GetSQDetailsBySQNo?DocumentNo=' + salesQuoteDetails.DocumentNo,
        //        type: 'GET',
        //        contentType: 'application/json',
        //        success: function (data) {

        //            if (data > 0) {

        //                Lobibox.notify('error', {
        //                    pauseDelayOnHover: true,
        //                    size: 'mini',
        //                    rounded: true,
        //                    delayIndicator: false,
        //                    icon: 'bx bx-x-circle',
        //                    continueDelayOnInactiveTab: false,
        //                    position: 'top right',
        //                    msg: 'Margin Should Be Greater Then Zero'
        //                });

        //                $('#modalJustification').css('display', 'block');
        //            }
        //        },
        //        error: function () {
        //            //alert("error");
        //        }
        //    }
        //);

        //if ($('#lblInqNo').val() != "") {

        //    //AutoFillNextLineItems($('#hfCurrentID').val(), $('#hfIDList').val());
        //    //resetproductForLocation();

        //}
        //else if ($('#hfDailyVisitNo').val() != "") {

        //    //AutoFillNextLineItemsDailyVisit($('#hfCurrentID').val(), $('#hfIDList').val());
        //    //resetproductForLocation();

        //}
        //else {

        //    //resetproduct();

        //}

    });

    $('#btnClear').click(function () {

        ResetQuoteLineDetails();

    });

    $('#ddlQtyPopupLocCode').change(function () {

        $.ajax(
            {
                url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + $('#hfProdNo').val() + '&LocCode=' + $('#ddlQtyPopupLocCode').val(),
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#tblInvDetails').empty();

                    var invDetailsTR = "";

                    for (var i = 0; i < data.length; i++) {

                        //invDetailsTR += "<tr><td hidden>" + data[i].ItemNo + "</td><td>" + data[i].ManufactureCode + "</td><td>" + data[i].LotNo + "</td><td>" + data[i].AvailableQty + "</td><td>" + data[i].RequestedQty + "</td><td><input id='" + data[i].LotNo + "_ReqQty' value='0' type='text' width='40%' /></td>" +
                        //    "<td>" + data[i].UnitCost + "</td></tr>";

                        invDetailsTR += "<tr><td hidden>" + data[i].ItemNo + "</td><td>" + data[i].ManufactureCode + "</td><td>" + data[i].LotNo + "</td><td>" + data[i].AvailableQty + "</td><td>" + data[i].RequestedQty + "</td>" +
                            "<td>" + data[i].UnitCost + "</td></tr>";

                    }

                    $('#tblInvDetails').append(invDetailsTR);

                },
                error: function () {
                    //alert("error");
                }
            }
        );

    });



    //$('#btnAddQty').click(function () {

    //    var errMsg = "";
    //    var Cnt = 0;
    //    var TotalQty = 0;
    //    $('#tblInvDetails tr').each(function () {

    //        var row = $(this)[0];
    //        if (parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val()) <= 0) {

    //            Cnt += 1;
    //            errMsg = "Please Fill Lot No. Wise Qty";

    //        }
    //    });

    //    if ($('#tblInvDetails tr').length > Cnt) {

    //        $('#tblInvDetails tr').each(function () {

    //            var row = $(this)[0];
    //            if (parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val()) > 0) {

    //                var ItemNo = row.cells[0].innerHTML;
    //                var LotNo = row.cells[2].innerHTML;
    //                var ReqQty = parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val());
    //                var LocCode = $('#ddlQtyPopupLocCode').val();

    //                var TROpts = "<tr><td></td><td></td><td>" + ItemNo + "</td><td>" + LotNo + "</td><td>" + ReqQty + "</td><td>" + LocCode + "</td></tr>";
    //                $('#tblLotNoWiseQtyDetails').append(TROpts);

    //                TotalQty += parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val());

    //            }

    //        });

    //        $('#lblInvQtyAddMsg').text("Quantities Added");
    //        $('#lblInvQtyAddMsg').css('color', 'green');
    //        $('#lblInvQtyAddMsg').css('display', 'block');
    //        $('#txtProdQty').val(TotalQty);
    //        $('#txtProdQty').css('readonly', true);
    //    }
    //    else {

    //        $('#lblInvQtyAddMsg').text(errMsg);
    //        $('#lblInvQtyAddMsg').css('color', 'red');
    //        $('#lblInvQtyAddMsg').css('display', 'block');

    //    }

    //});

    $('#btnAddIncoTerm').click(function () {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

        if ($('#txtIncoTermCode').val() == "" || $('#txtIncoTerm').val() == "") {

            $('#lblIncoTermAddMsg').css('display', 'block');
            $('#lblIncoTermAddMsg').text('Please Fill Inco Term Details');
            $('#lblIncoTermAddMsg').css('color', 'red');

        }
        else {

            $.post(
                apiUrl + 'AddNewIncoTerm?IncoTermCode=' + $('#txtIncoTermCode').val() + '&IncoTerm=' + $('#txtIncoTerm').val(),
                function (data) {

                    if (data) {

                        $('#lblIncoTermAddMsg').css('display', 'block');
                        $('#lblIncoTermAddMsg').text('Inco Term Added Successfully.');
                        $('#lblIncoTermAddMsg').css('color', 'green');
                        $('#txtIncoTermCode').val("");
                        $('#txtIncoTerm').val("");
                        BindIncoTerms();
                    }

                }
            );

        }

    });

    $('#btnCloseCostSheetMsg').click(function () {

        $('#dvCostSheetMsg').css('display', 'none');
        $('#dvUpdateCostSheetMsg').css('display', 'none');
        $('#modalCostSheetMsg').css('display', 'none');
        
        if ($('#hfCostSheetFlag').val() == "true") {
            showCostSheetDetails($('#hfItemNo').val(), $('#hfItemName').val());
        }
        
    });

    $('#btnCloseCostSheet').click(function () {

        $('#modalCostSheet').css('display', 'none');

    });

    $('#btnSaveCostSheet').click(function () {

        var flag = false;
        var errMsg = "";
        //var errMsg = CheckCostSheetDetails();
        //if (errMsg != "") {
        //    ShowErrMsg(errMsg);
        //}
        //else {

            $('#tblCostSheetDetails tr').each(function () {

                var row = $(this)[0];
                var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();
                if (RatePerUnit == "") {
                    errMsg = "Please Fill Cost Per Unit In All Charge Item";
                }

            });

            if (errMsg != "") {

                $('#lblCostSheetErrMsg').text(errMsg);
                $('#lblCostSheetErrMsg').css('display', 'block');

            }
            else {

                $('#divImage').show();
                $('#tblCostSheetDetails tr').each(function () {

                    var row = $(this)[0];

                    var CostSheetLineNo = parseInt(row.cells[0].innerHTML);
                    var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();

                    $.post(apiUrl + "UpdateCostSheet?SQNo=" + $('#lblCostSheetSQNo').text() + "&CostSheetLineNo=" + CostSheetLineNo +
                        "&RatePerUnit=" + parseFloat(RatePerUnit), function (data) {

                            if (data) {
                                flag = true;
                            }

                        });

                });

                /*if (flag) {*/

                $('#divImage').hide();
                $('#hfCostSheetFlag').val("false");
                $('#modalCostSheet').css('display', 'none');

                var prodNo = $('#hfItemNo').val();
                $("#" + prodNo + "_TotalUnitPrice").text($('#lblCostSheetTotalUnitPrice').text());
                $("#" + prodNo + "_Margin").text($('#lblCostSheetMargin').text());
                $('#modalCostSheetMsg').css('display', 'block');
                $('#dvUpdateCostSheetMsg').css('display', 'block');

                /*}*/
            }
            
        /*}*/
        
    });

    $('#btnCloseSQMsg').click(function () {

        $('#lblSQMsg').text("");
        $('#modalSQMsg').css('display', 'none');
        location.reload(true);

    });

    $('#chkIsShortclose').change(function () {

        if ($('#chkIsShortclose').prop('checked')) {
            $('#modalShortclose').css('display', 'block');
            $('#ShortcloseTitle').text("Shortclose");
            $('#hfShortcloseType').val("SalesQuote");
        }
        else {
            $('#modalShortclose').css('display', 'none');
        }
        
    });

    $('#ddlShortcloseReason').change(function () {

        if ($('#ddlShortcloseReason option:selected').text() == $('#hfSCRemarksSetupValue').val()) {

            $('#dvShortcloseRemarks').css('display', 'block');
            $('#hfShortcloseWithRemarks').val("true");

        }
        else {

            $('#txtShortcloseRemarks').val("");
            $('#dvShortcloseRemarks').css('display', 'none');
            $('#hfShortcloseWithRemarks').val("false");
        }

    });

    $('#btnShortclose').click(function () {

        apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        if ($('#hfShortcloseWithRemarks').val() == "true" && $('#txtShortcloseRemarks').val() == "") {
            $('#lblShortcloseErrMsg').css('display', 'block');
            $('#lblShortcloseErrMsg').text("Please Fill Shortclose Remarks");
        }
        else {

            $('#lblShortcloseErrMsg').css('display', 'none');
            $('#lblShortcloseErrMsg').text("");
            $('#btnShortcloseSpinner').show();
            SQNo = UrlVars["SQNo"];
            if ($('#hfShortcloseType').val() == "SalesQuote") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuote&SQNo=' + SQNo + '&SQProdLineNo=-1&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }
            else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuoteProd&SQNo=' + SQNo + '&SQProdLineNo=' + $('#hfSQProdLineNoForShortclose').val() + '&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }

            $.post(apiUrl, function (data) {

                $('#btnShortcloseSpinner').hide();
                $('#modalShortclose').css('display', 'none');
                $('#lblShortcloseErrMsg').text("");
                $('#hfSQProdLineNoForShortclose').val("");

                if (data == "") {

                    $('#modalShortcloseMsg').css('display', 'block');

                    if ($('#hfShortcloseType').val() == "SalesQuote") {
                        $('#lblSQShortclose').text("Sales quote shortclosed successfully");
                    }
                    else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                        $('#lblSQShortclose').text("Sales quote product shortclosed successfully");
                    }

                }
                else {

                    $('#modalErrMsg').css('display', 'block');
                    $('#modalErrDetails').text(data);

                }

            });

        }

    });

    $('#btnCloseModalShortclose').click(function () {

        $('#modalShortclose').css('display', 'none');
        $('#lblShortcloseErrMsg').css('display', 'none');
        $('#lblShortcloseErrMsg').text("");
        $('#hfShortcloseType').val("");

    });

    $('#btnCloseShortcloseMsg').click(function () {

        $('#lblSQShortclose').text("");
        $('#modalShortcloseMsg').css('display', 'none');
        location.reload(true);
        
    });

    $('#txtConcentratePercent').blur(function () {

        $('#txtProdQty').val(($('#txtConcentratePercent').val() * $('#txtNetWeight').val() / 100).toFixed(3));
        $('#txtProdQty').prop('readonly', true);

    });

    $('#txtNetWeight').blur(function () {

        $('#txtConcentratePercent').blur();
    });

    $('#txtLiquidRate').blur(function () {

        $('#txtSalesPrice').val(((parseFloat($('#txtNetWeight').val()) * parseFloat($('#txtLiquidRate').val())) / parseFloat($('#txtProdQty').val())).toFixed(2));

    });

    $('#chkShowAllProducts').change(function () {

        productName_autocomplete();

    });

    $('#btnAddNewContactPerson').click(function () {

        $('.modal-title').html("Add New Contact Person<br />In Selected Company");
        $('#hfAddNewDetails').val("ContactPerson");
        $('#dvAddNewCPerson').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        BindDepartment();

    });

    $('#btnAddNewBillTo').click(function () {

        $('.modal-title').html("Add New Bill-to Address");
        $('#hfAddNewDetails').val("BillToAddress");
        $('#dvAddNewShiptoAddress').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        $('#txtNewShiptoAddName').val($('#txtCustomerName').val());
        $('#txtNewShiptoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnAddNewJobTo').click(function () {

        $('.modal-title').html("Add New Delivery-to Address");
        $('#hfAddNewDetails').val("DeliveryToAddress");
        $('#dvAddNewJobtoAddress').css('display', 'block');
        $('#modalSQ').css('display', 'block');
        $('#txtNewJobtoAddName').val($('#txtCustomerName').val());
        $('#txtNewJobtoAddName').prop('readonly', true);
        BindPincodeMin2Char();
        BindArea();

    });

    $('#btnCloseModalSQ').click(function () {

        $('#lblMsg').text("");
        $('#hfAddNewDetails').val("");
        $('#dvAddNewCPerson').css('display', 'none');
        $('#dvAddNewShiptoAddress').css('display', 'none');
        $('#dvAddNewJobtoAddress').css('display', 'none');
        ResetCPersonDetails();
        ResetNewBillToAddressDetails();
        ResetNewDeliveryToAddressDetails();
        $('#modalSQ').css('display', 'none');

    });

    $('#btnConfirmAdd').click(function () {

        if ($('#hfAddNewDetails').val() == "ContactPerson") {

            var errMsg = CheckCPersonFieldValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var CPersonDetails = {};

                CPersonDetails.Name = $('#txtCPersonName').val();
                CPersonDetails.Company_No = $('#hfContactCompanyNo').val();
                CPersonDetails.Mobile_Phone_No = $('#txtCPersonMobile').val();
                CPersonDetails.E_Mail = $('#txtCPersonEmail').val();
                CPersonDetails.PCPL_Job_Responsibility = $('#txtJobResponsibility').val();
                CPersonDetails.PCPL_Department_Code = $('#ddlDepartment').val();
                CPersonDetails.Type = "Person";
                CPersonDetails.Salesperson_Code = $('#hdnLoggedInUserSPCode').val();
                CPersonDetails.PCPL_Allow_Login = $('#chkAllowLogin').prop('checked');
                CPersonDetails.chkEnableOTPOnLogin = $('#chkEnableOTPOnLogin').prop('checked');
                CPersonDetails.Is_Primary = $('#chkIsPrimary').prop('checked');

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewContactPerson', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(CPersonDetails)
                    });
                    const res = await rawResponse.ok;
                    if (res) {

                        $('#btnAddSpinner').css('display', 'none');
                        //$('#hfContactDetails').val($('#hfContactCompanyNo').val() + "_" + $('#hfPrimaryContactNo').val());
                        //BindContact($('#hfContactDetails').val());
                        GetContactsOfCompany($('#txtCustomerName').val());
                        $('#lblMsg').text("Contact Person Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetCPersonDetails();

                    }
                })();
            }

        }
        else if ($('#hfAddNewDetails').val() == "BillToAddress") {

            var errMsg = CheckNewBilltoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewBillToAddress = {};

                NewBillToAddress.Customer_No = $('#hfCustomerNo').val();
                NewBillToAddress.Code = $('#txtNewShiptoAddCode').val();
                NewBillToAddress.Address = $('#txtNewShiptoAddress').val();
                NewBillToAddress.Address_2 = $('#txtNewShiptoAddress2').val();
                NewBillToAddress.Post_Code = $('#txtNewShiptoAddPostCode').val();
                NewBillToAddress.PCPL_Area = $('#ddlNewShiptoAddArea').val();
                NewBillToAddress.State = $('#txtNewShiptoAddState').val();
                NewBillToAddress.GST_Registration_No = $('#txtNewShiptoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewBillToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewBillToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Bill-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewBillToAddressDetails();
                        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }
        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

            var errMsg = CheckNewDeliverytoAddressValues();

            if (errMsg != "") {
                $('#lblMsg').text(errMsg);
                $('#lblMsg').css('color', 'red').css('display', 'block');
            }
            else {

                $('#lblMsg').text("");
                $('#lblMsg').css('color', 'red').css('display', 'none');
                $('#btnAddSpinner').css('display', 'block');
                var NewDeliveryToAddress = {};

                NewDeliveryToAddress.Customer_No = $('#hfCustomerNo').val();
                NewDeliveryToAddress.Code = $('#txtNewJobtoAddCode').val();
                NewDeliveryToAddress.Address = $('#txtNewJobtoAddress').val();
                NewDeliveryToAddress.Address_2 = $('#txtNewJobtoAddress2').val();
                NewDeliveryToAddress.Post_Code = $('#txtNewJobtoAddPostCode').val();
                NewDeliveryToAddress.PCPL_Area = $('#ddlNewJobtoAddArea').val();
                NewDeliveryToAddress.State = $('#txtNewJobtoAddState').val();
                NewDeliveryToAddress.GST_Registration_No = $('#txtNewJobtoAddGSTNo').val();

                (async () => {
                    const rawResponse = await fetch('/SPSalesQuotes/AddNewDeliveryToAddress', {
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(NewDeliveryToAddress)
                    }).then(data => {
                        return data.text();
                    });

                    $('#btnAddSpinner').css('display', 'none');
                    if (rawResponse == "") {
                        $('#lblMsg').text("New Delivery-to Address Added Successfully");
                        $('#lblMsg').css('color', 'green').css('display', 'block');
                        ResetNewDeliveryToAddressDetails();
                        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
                    }
                    else {
                        $('#lblMsg').text(rawResponse);
                        $('#lblMsg').css('color', 'red').css('display', 'block');
                    }

                })();

            }

        }

    });

    $('#chkIsLiquidProd').change(function () {

        $('#txtSalesPrice').val("0");
        if ($('#chkIsLiquidProd').prop('checked')) {

            $('#dvLiquidProdFields').css('display', 'block');
            $('#txtSalesPrice').prop('disabled', true);
            $('#hfIsLiquidProd').val("true");
            //$('#txtNetWeight').val($('#hfNetWeight').val());
            //$('#lblGrossWeight').text($('#hfGrossWeight').val());
            //$('#chkIsCommission').prop('disabled', true);
            //$('#ddlCommissionPerUnitPercent').prop('disabled', true);
            
        }
        else {

            $('#dvLiquidProdFields').css('display', 'none');
            $('#txtSalesPrice').prop('disabled', false);
            $('#hfIsLiquidProd').val("false");
            $('#txtConcentratePercent').val("");
            $('#txtLiquidRate').val("");
            $('#txtProdQty').val("");
            $('#txtProdQty').prop('readonly', false);
            //$('#lblGrossWeight').text("");
            //$('#txtNetWeight').val("");
            $('#txtTransportCost').val("0");
            $('#txtSalesDiscount').val("0");
            $('#chkIsCommission').prop('checked', false);
            $('#chkIsCommission').change();
            $('#ddlCommissionPayable').val('-1');
            $('#txtCreditDays').val("0");
            $('#txtMargin').val("0.00");
            $('#txtInterest').val("0.00");
            $('#txtTotalCost').val("0.00");
            $('#txtDeliveryDate').val("");
            $('#chkDropShipment').prop('checked', false);
            $('#chkDropShipment').change();

        }

    });

    $('#btnCloseApproveRejectMsg').click(function () {

        $('#modalApproveRejectMsg').css('display', 'none');
        $('#lblApproveRejectMsg').text("");
        RedirectToSQApproval();

    });

    $('#btnReject').click(function () {

        $('#modalRejectRemarks').css('display', 'block');

    });

    $('#btnConfirmReject').click(function () {

        if ($('#txtRejectRemarks').val() == "") {
            $('#lblRemarksMsg').text("Please Fill Remarks");
        }
        else {
            ApproveRejectSQ("Reject", $('#txtRejectRemarks').val());
        }

    });

    $('#btnCloseModalRejectRemarks').click(function () {

        $('#modalRejectRemarks').css('display', 'none');

    });

});


function BindDepartment() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllDepartmentForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {

                    $('#ddlDepartment').empty();
                    $('#ddlDepartment').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Department
                            }
                        ).html(data.Department).appendTo("#ddlDepartment");
                    });

                    $("#ddlDepartment").val('-1');

                    //if ($('#hfDepartmentCode').val() != "") {
                    //    $("#ddlDepartment").val($('#hfDepartmentCode').val());
                    //}
                }

            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function GetAndFillInquiryDetails(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetInquiryDetails?InqNo=' + InqNo, function (data) {

        $('#hfInqNo').val(data.Inquiry_No);
        $('#hfContactCompanyNo').val(data.Inquiry_Customer_Contact);
        $('#hfSavedContactPersonNo').val(data.PCPL_Contact_Person);
        $('#txtCustomerName').val(data.Contact_Company_Name);
        $('#txtCustomerName').blur();
        $('#hfPaymentTerms').val(data.Payment_Terms);
        $('#hfShiptoCode').val(data.Ship_to_Code);
        $('#hfJobtoCode').val(data.PCPL_Job_to_Code);
        $('#dvInquiryLineDetails').css('display', 'block');
        BindPaymentTerms();
        $('#ddlPaymentTerms').change();
        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        BindInquiryLineDetails(InqNo);

    });

}

function GetAndFillSQDetails(SQNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetSQDetailsForSQNo?SQNo=' + SQNo, function (data) {

        $('#hfInqNo').val(data.Inquiry_No);
        $('#txtCustomerName').val(data.Contact_Company_Name);
        $('#txtCustomerName').blur();
        $('#hfPaymentTerms').val(data.Payment_Terms);
        $('#hfShiptoCode').val(data.Ship_to_Code);
        $('#hfJobtoCode').val(data.PCPL_Job_to_Code);
        $('#dvInquiryLineDetails').css('display', 'block');
        BindInquiryLineDetails(InqNo);

    });

}


function BindSQLineDetails(SalesQuoteNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllSQLinesOfSQ?QuoteNo=' + SalesQuoteNo + '&SQLinesFor=SalesQuote',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#dataList').css('display', 'block');
                $('#tblProducts').empty();
                var SQLineTR = "";
                $.each(data, function (index, item) {

                    prodOpts = "<tr><td></td><td hidden>" + item.Line_No + "</td><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" +
                        item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.PCPL_Basic_Price +
                        "</td><td>" + item.Delivery_Date + "</td><td>" + $('#ddlTransportMethod option:selected').text() + "</td><td>" + item.PCPL_Sales_Price + "</td><td>" +
                        $('#txtLineDetailsPaymentTerms').val() + "</td><td>" + $('#txtLineDetailsIncoTerms').val() + "</td><td id='CostSheetOpt'><a class='CostSheetCls' onclick='showCostSheetDetails(\"" + $('#hfSalesQuoteNo').val() + "\"," + item.Line_No + ")'><span class='badge bg-primary'>Cost Sheet</span></a></td></tr>";

                    $('#tblProducts').append(prodOpts);
                    
                });

                dataTableFunction();

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function dTableFunction() {

    dataTableFunction();

}

function dataTableFunction() {

    dtable = $('#dataList').DataTable({
        retrieve: true,
        filter: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
    });

}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function BindInquiryLineDetails(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetInquiryProdDetails?InqNo=' + InqNo, function (data) {

        var ProdTR = "";
        $.each(data, function (index, item) {
         
            ProdTR += "<tr><td hidden>" + item.Line_No + "</td><td hidden>" + item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" +
                item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.Unit_of_Measure + "</td><td>" + item.Delivery_Date + "</td><td>" + item.PCPL_Payment_Terms + "</td>" +
                "<td><a class='InqProdCls' onclick='FillInqProdDetails(\"" + item.Line_No + "\",\"" + item.Product_No + "\",\"" + item.Product_Name + "\"," +
                item.Quantity + ",\"" + item.PCPL_Packing_Style_Code + "\",\"" + item.Delivery_Date + "\")'><i class='bx bx-edit'></i></a></td></tr>";
                
        });
        
        $('#tblInqProdDetails').append(ProdTR);
    });

}

function FillInqProdDetails(ProdLineNo, ProductNo, ProductName, Quantity, PackingStyleCode, DeliveryDate) {

    ResetQuoteLineDetails();
    $('#hfProdLineNo').val(ProdLineNo);
    $('#hfProdNo').val(ProductNo);
    $('#hfInqProdPackingStyle').val(PackingStyleCode);
    $('#txtProductName').val(ProductName);
    $('#txtProductName').blur();
    $('#txtProdQty').val(Quantity);
    //alert($('#ddlPackingStyle').val());
    //$('#ddlPackingStyle').val(PackingStyleCode);
    //$('#ddlPackingStyle').change();
    $('#txtDeliveryDate').val(DeliveryDate);
    $('#ddlPaymentTerms').change();
}

//function BindNoSeries() {

//    $.ajax(
//        {
//            url: '/SPSalesQuotes/GetNoSeriesForDDL',
//            type: 'GET',
//            contentType: 'application/json',
//            success: function (data) {

//                $('#ddlNoSeries option').remove();
//                var locationsOpts = "<option value='-1'>---Select---</option>";
//                $.each(data, function (index, item) {
//                    locationsOpts += "<option value='" + item.Series_Code + "'>" + item.Series_Description + "</option>";
//                });

//                $('#ddlNoSeries').append(locationsOpts);

//                $('#ddlNoSeries').val('-1');

//                if ($('#hfSavedNoSeriesCode').val() != "" && $('#hfSavedNoSeriesCode').val() != null) {
//                    $('#ddlNoSeries').val($('#hfSavedNoSeriesCode').val());
//                }

//            },
//            error: function () {
//                //alert("error");
//            }
//        }
//    );

//}

function BindLocations() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetLocationsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlLocations option').remove();
                var locationsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    locationsOpts += "<option value='" + item.Code + "'>" + item.Name + "</option>";
                });

                $('#ddlLocations').append(locationsOpts);

                $('#ddlLocations').val('-1');

                if ($('#hfSavedLocationCode').val() != "" && $('#hfSavedLocationCode').val() != null) {
                    $('#ddlLocations').val($('#hfSavedLocationCode').val());
                }

                //if ($('#hfSavedNoSeriesCode').val() != "" && $('#hfSavedNoSeriesCode').val() != null) {
                //    $('#ddlNoSeries').val($('#hfSavedNoSeriesCode').val());
                //}

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function GetCustomerTemplateCode() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetCustomerTemplateCode', function (data) {

        if (data != "") {
            $('#hfCustomerTemplateCode').val(data);
        }

    });

}

function BindInquiryType() {

        $('#ddlInquiryType option').remove();
        var inquirytypeOpts = "<option value='-1'>---Select---</option>";
        inquirytypeOpts += "<option value='Local'>Local</option>"; 
        inquirytypeOpts += "<option value='Export'>Export</option>";
        inquirytypeOpts += "<option value='Sample'>Sample</option>";

        $('#ddlInquiryType').append(inquirytypeOpts);
        $('#ddlInquiryType').val('Local');
        $('#ddlInquiryType').attr('disabled', true);
}

function GetContactsOfCompany(companyName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetAllContactsOfCompany?companyName=' + companyName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    var primaryContactNo = "";
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));
                    $.each(data, function (i, data) {
                        if (data.Is_Primary) {
                            primaryContactNo = data.No;
                        }
                        $('<option>',
                            {
                                value: data.No,
                                text: data.Name
                            }
                        ).html(data.Name).appendTo("#ddlContactName");
                    });

                    $("#ddlContactName").val(primaryContactNo);
                    $('#btnSaveProd').css('display', 'block');
                    $('#btnSave').css('display', 'block');

                    if ($('#hfSavedContactPersonNo').val() != "") {

                        $("#ddlContactName").val($('#hfSavedContactPersonNo').val());

                    }
                    //if ($('#hfAreaCode').val() != "") {
                    //    $("#ddlArea").val($('#hfAreaCode').val());
                    //}
                }
                else {
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));

                    $('#btnSaveProd').css('display', 'none');
                    $('#btnSave').css('display', 'none');
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function companyName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {
        if (data != null) {
            var str1 = "";

            var i;
            for (i = 0; i < data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var company = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var companiesArray = $.map(company, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtCustomerName').autocomplete({
                lookup: companiesArray,
                onSelect: function (selecteditem) {
                    $('#hfContactCompanyNo').val(selecteditem.data);
                    $('#btnAddNewContactPerson').prop('disabled', false);
                },
            });
        }
    });

    $('#ddlContactName').empty();
    $('#ddlContactName').append("<option value='-1'>---Select---</option>");
    $('#ddlContactName').val('-1');

    if ($('#hfSavedCustomerName').val() != "" && $('#hfSavedCustomerName').val() != null) {
        $('#txtCustomerName').val($('#hfSavedCustomerName').val());
        $('#txtCustomerName').blur();
    }
    else if ($('#hfCustomerName').val() != "" && $('#hfCustomerName').val() != null) {
        $('#txtCustomerName').val($('#hfCustomerName').val());
        //$('#txtCustomerName').change();
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    }

};

function BindPincodeMin2Char() {
    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    $('#txtNewShiptoAddPostCode, #txtNewJobtoAddPostCode').autocomplete({
        serviceUrl: '/SPSalesQuotes/GetPincodeForDDL',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },
        onSelect: function (suggestion) {
            jQuery("#hfPostCode").val(suggestion.value);

            var citydis = suggestion.data.split(",");
            var statecountry = suggestion.id.split(",");

            if ($('#hfAddNewDetails').val() == "BillToAddress") {

                jQuery("#txtNewShiptoAddCity").val(citydis[0]);
                $("#txtNewShiptoAddCity").prop('readonly', true);
                jQuery("#txtNewShiptoAddDistrict").val(citydis[1]);
                $("#txtNewShiptoAddDistrict").prop('readonly', true);

                jQuery("#txtNewShiptoAddCountryCode").val(statecountry[1]);
                $("#txtNewShiptoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewShiptoAddState").val(statecountry[0]);
                $("#txtNewShiptoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewShiptoAddPostCode').val());

            }

            if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                jQuery("#txtNewJobtoAddCity").val(citydis[0]);
                $("#txtNewJobtoAddCity").prop('readonly', true);
                jQuery("#txtNewJobtoAddDistrict").val(citydis[1]);
                $("#txtNewJobtoAddDistrict").prop('readonly', true);

                jQuery("#txtNewJobtoAddCountryCode").val(statecountry[1]);
                $("#txtNewJobtoAddCountryCode").prop('readonly', true);
                jQuery("#txtNewJobtoAddState").val(statecountry[0]);
                $("#txtNewJobtoAddState").prop('readonly', true);
                GetDetailsByCode($('#txtNewJobtoAddPostCode').val());
            }

            return false;
        },
        select: function (event, ui) {
        },
        transformResult: function (response) {
            return {
                suggestions: jQuery.map(jQuery.parseJSON(response), function (item) {
                    return {
                        value: item.Code,
                        data: item.City + ',' + item.District_Code,
                        id: item.PCPL_State_Code + ',' + item.Country_Region_Code
                    };
                })
            };
        },
    });

}


function BindArea() {

    $.get(apiUrl + 'GetAllAreasForDDL', function (data) {

        if (data.length > 0) {

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').empty();
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').empty();
            }

            var AreaOpts = "<option value='-1'>---Select---</option>";

            for (var a = 0; a < data.length; a++) {

                AreaOpts += "<option value='" + data[a].Code + "'>" + data[a].Text + "</option>";

            }

            if ($('#hfAddNewDetails').val() == "BillToAddress") {
                $('#ddlNewShiptoAddArea').append(AreaOpts);
            }
            else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                $('#ddlNewJobtoAddArea').append(AreaOpts);
            }

        }

    });

}

function GetCreditLimitAndCustDetails(companyName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetCreditLimitAndCustDetails?companyName=' + companyName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#txtTotalCreditLimit').val(data.CreditLimit).prop('disabled',true);
                    $('#txtAvailableCreditLimit').val(data.AvailableCredit).prop('disabled', true);
                    $('#txtOutstandingDue').val(data.OutstandingDue).prop('disabled', true);
                    $('#hfUsedCreditLimit').val(data.UsedCreditLimit);
                    $('#hdnCustBalanceLCY').val(data.AccountBalance);

                    if (SQFor == "ApproveReject") {
                        $('#tdOverdue').text($('#txtOutstandingDue').val());
                        $('#tdTotalExpo').text($('#hdnCustBalanceLCY').val());
                    }

                    $('#ddlConsigneeAddress option').remove();
                    var consigneeAddOpts = "<option value='-1'>---Select---</option>";

                    if ((data.Address != "" && data.Address != null) || (data.City != "" && data.City != null) ||
                        (data.Post_Code != "" && data.Post_Code != null)) {

                        var consigneeAddText = "", consigneeAddValue = "";


                        if (data.CustNo != null) {
                            $('#hfCustomerNo').val(data.CustNo);
                            consigneeAddText = data.CustName + "," + data.Address + "," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            //
                            consigneeAddValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            //

                        }
                        else {
                            consigneeAddText = data.CompanyName + "," + data.Address + "," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            //
                            consigneeAddValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            //
                        }

                        consigneeAddOpts += "<option value=\"" + consigneeAddValue + "\">" + consigneeAddText + "</option>";

                        $('#ddlConsigneeAddress').append(consigneeAddOpts);
                        $('#ddlConsigneeAddress').val(consigneeAddValue);

                        $('#hfConsigneeAdd').val(consigneeAddValue);
                    }
                    else {
                        $('#ddlConsigneeAddress').empty();
                        $('#ddlConsigneeAddress').append(consigneeAddOpts);
                        $('#ddlConsigneeAddress').prop('disabled', true);
                    }

                    if (data.CustNo != null) {

                        $('#hfCustPANNo').val(data.PANNo);

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";

                        if (data.ShiptoAddress != null) {
                            for (var i = 0; i < data.ShiptoAddress.length; i++) {
                                billtoAddOpts = billtoAddOpts + "<option value=\"" + data.ShiptoAddress[i].Code + "\">" + data.ShiptoAddress[i].Address + "</option>";
                            }
                        }
                        
                        $('#ddlBillTo').append(billtoAddOpts);
                        $('#btnAddNewBillTo').prop('disabled', false);

                        if ($('#hfShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfShiptoCode').val());
                        }
                        else if ($('#hfSavedShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfSavedShiptoCode').val());
                        }
                        else {
                            $('#ddlBillTo').val('-1');
                        }

                        $('#ddlBillTo').attr('disabled', false);

                        $('#ddlDeliveryTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        if (data.JobtoAddress != null) {
                            for (var i = 0; i < data.JobtoAddress.length; i++) {
                                shiptoAdd = shiptoAdd + "<option value=\"" + data.JobtoAddress[i].Code + "\">" + data.JobtoAddress[i].Address + "</option>";
                            }
                        }
                        
                        $('#ddlDeliveryTo').append(shiptoAdd);
                        $('#btnAddNewJobTo').prop('disabled', false);

                        if ($('#hfJobtoCode').val() != "") {
                            $('#ddlDeliveryTo').val($('#hfJobtoCode').val());
                        }
                        else if ($('#hfSavedJobtoCode').val() != "") {
                            $('#ddlDeliveryTo').val($('#hfSavedJobtoCode').val());
                        }
                        else {
                            $('#ddlDeliveryTo').val('-1');
                        }

                        $('#ddlDeliveryTo').attr('disabled', false);

                        $('#hfCustomerNo').val(data.CustNo);
                    }
                    else {

                        $('#ddlBillTo option').remove();
                        var billtoAddOpts = "<option value='-1'>---Select---</option>";

                        $('#ddlBillTo').append(billtoAddOpts);
                        $('#ddlBillTo').val('-1');
                        $('#ddlBillTo').attr('disabled', true);

                        $('#ddlDeliveryTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        $('#ddlDeliveryTo').append(shiptoAdd);
                        $('#ddlDeliveryTo').val('-1');
                        $('#ddlDeliveryTo').attr('disabled', true);
                    }
                    
                    
                }

            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );

}

function productName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    if ($('#chkShowAllProducts').prop('checked')) {
        apiUrl += 'GetAllProductsForShowAllProd';
    }
    else {
        apiUrl += 'GetAllProducts?CCompanyNo=' + $('#hfContactCompanyNo').val();
    }

    $.get(apiUrl, function (data) {
        
        if (data != null) {
            let str1 = "";

            var i;
            for (i = 0; i < data.length; i++) {

                if ($('#chkShowAllProducts').prop('checked')) {
                    str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Description.trim() + '"' + ",";
                }
                else {
                    str1 = str1 + '"' + data[i].Item_No + '"' + ":" + '"' + data[i].Item_Name.trim() + '"' + ",";
                }

            }

            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";
            
            let objFromStr = JSON.parse(str1);
            
            var products = objFromStr; // { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            //var item = {"AB001":"abc", "AB002":"bcd"};

            var productsArray = $.map(products, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });
            
            $('#txtProductName').autocomplete({
                lookup: productsArray,
                onSelect: function (selecteditem) {
                    $('#hfProdNo').val(selecteditem.data);
                },
            });
        }
    });
};

function BindPaymentTerms() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetPaymentTermsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlPaymentTerms option').remove();
                var paymentTermsCodeForInq = "";
                var paymenttermsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {

                    if ($('#hfPaymentTerms').val() != "" && $('#hfPaymentTerms').val() != null) {

                        if ($('#hfPaymentTerms').val() == item.Code) {
                            paymentTermsCodeForInq = item.Code + "_" + item.Due_Date_Calculation;
                        }
                        
                    }
                    paymenttermsOpts += "<option value=\"" + item.Code + "_" + item.Due_Date_Calculation + "\">" + item.Description + "</option>";
                });

                $('#ddlPaymentTerms').append(paymenttermsOpts);

                $('#ddlPaymentTerms').val('-1');

                if ($('#hfPaymentTerms').val() != "" && $('#hfPaymentTerms').val() != null) {

                    $('#ddlPaymentTerms').val(paymentTermsCodeForInq);
                }

                //if ($('#hfSavedPaymentTerms').val() != "" || $('#hfSavedPaymentTerms').val() != null) {
                //    $('#ddlPaymentTerms').val($('#hfSavedPaymentTerms').val());
                //}
                
                //if ($('#hfSavedPaymentTermsCode').val() != "" && $('#hfPaymentTerms').val() != null) {
                //    $('#ddlPaymentTerms').val($('#hfSavedPaymentTermsCode').val());
                //}
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindItemVendors(ProdNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetItemVendorsForDDL?ProdNo=' + ProdNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.length > 0) {
                    $('#ddlItemVendors option').remove();
                    var itemVendorsOpts = "<option value='-1'>---Select---</option>";
                    $.each(data, function (index, item) {
                        itemVendorsOpts += "<option value='" + item.Vendor_No + "'>" + item.Vendor_Name + "</option>";
                    });

                    $('#ddlItemVendors').append(itemVendorsOpts);
                    $('#ddlItemVendors').val('-1');
                    if ($('#hfSavedItemVendorNo').val() != "" && $('#hfSavedItemVendorNo').val() != null) {
                        $('#ddlItemVendors').val($('#hfSavedItemVendorNo').val());
                    }
                }
                else {
                    $('#ddlItemVendors').append("<option value='-1'>---Select---</option>");
                    $('#ddlItemVendors').val('-1');
                }
                
                
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindIncoTerms() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetIncoTermsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlIncoTerms option').remove();
                var incotermsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    incotermsOpts += "<option value='" + item.Code + "'>" + item.Description + "</option>";
                });

                $('#ddlIncoTerms').append(incotermsOpts);

                $('#ddlIncoTerms').val('-1');

                if ($('#hfSavedIncoTerms').val() != "" && $('#hfSavedIncoTerms').val() != null) {
                    $('#ddlIncoTerms').val($('#hfSavedIncoTerms').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindTransportMethod() {

    //$('#ddlTransport option').remove();
    //var transportOpts = "<option value='-1'>---Select---</option>";
    //transportOpts += "<option value='ToPay'>ToPay</option>";
    //transportOpts += "<option value='Paid'>Paid</option>";
    
    //$('#ddlTransport').append(transportOpts);

    $.ajax(
        {
            url: '/SPSalesQuotes/GetTransportMethodsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlTransportMethod option').remove();
                var transportMethods = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    transportMethods += "<option value='" + item.Code + "'>" + item.Description + "</option>";
                });

                $('#ddlTransportMethod').append(transportMethods);

                $('#ddlTransportMethod').val('-1');

                if ($('#hfSavedTransportMethod').val() != "" && $('#hfSavedTransportMethod').val() != null) {
                    $('#ddlTransportMethod').val($('#hfSavedTransportMethod').val());
                }

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindGSTPlaceOfSupply() {
    $('#ddlGSTPlaceOfSupply option').remove();
    var gstplaceofsupplyOpts = "<option value='-1'>---Select---</option>";
    gstplaceofsupplyOpts += "<option value='1'>Bill-to Address</option>";
    gstplaceofsupplyOpts += "<option value='2'>Ship-to Address</option>";
    gstplaceofsupplyOpts += "<option value='3'>Location Address</option>";

    $('#ddlGSTPlaceOfSupply').append(gstplaceofsupplyOpts);
    $('#ddlGSTPlaceOfSupply').val('1');
    $('#ddlGSTPlaceOfSupply').attr('disabled', true);
}

//function BindPaymentMethod() {

//    $.ajax(
//        {
//            url: '/SPSalesQuotes/GetPaymentMethodsForDDL',
//            type: 'GET',
//            contentType: 'application/json',
//            success: function (data) {

//                $('#ddlPaymentMethods option').remove();
//                var paymentmethodOpts = "<option value='-1'>---Select---</option>";
//                $.each(data, function (index, item) {
//                    paymentmethodOpts += "<option value='" + item.Code + "'>" + item.Description + "</option>";
//                });

//                $('#ddlPaymentMethods').append(paymentmethodOpts);

//                if ($('#hfSavedPaymentMethod').val() != "" && $('#hfSavedPaymentMethod').val() != null) {
//                    $('#ddlPaymentMethods').val($('#hfSavedPaymentMethod').val());
//                }
//            },
//            error: function () {
//                //alert("error");
//            }
//        }
//    );

//}

//function BindInquiries() {

//    $.ajax(
//        {
//            url: '/SPSalesQuotes/GetInquiriesForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(),
//            type: 'GET',
//            contentType: 'application/json',
//            success: function (data) {

//                $('#ddlInquiries option').remove();
//                var inquiriesOpts = "<option value='-1'>---Select---</option>";
//                $.each(data, function (index, item) {
//                    inquiriesOpts += "<option value=\"" + item.Inquiry_No + "\">" + item.Inquiry_No + "</option>";
//                });

//                $('#ddlInquiries').append(inquiriesOpts);

//                if ($('#hfInqNo').val() != "") {
//                    $('#ddlInquiries').val($('#hfInqNo').val());
//                }

//                //if ($('#hfSavedInquiryNo').val() != "") {
//                //    $('#ddlInquiries').val($('#hfSavedInquiryNo').val());
//                //}
//            },
//            error: function () {
//                //alert("error");
//            }
//        }
//    );

//}

function GetSalesQuoteDetailsAndFill(SalesQuoteNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    //$.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + $('#hfSalesQuoteNo').val(), function (data) {
    $.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + SalesQuoteNo, function (data) {

        if (data.No != "" || data.No != null) {

            $('#hfSQEdit').val("true");
            $('#hfSalesQuoteNo').val(SalesQuoteNo);
            //$('#ddlNoSeries').prop('disabled', true);
            $('#hfInqNo').val(data.InquiryNo);
            $('#txtInqNo').val(data.InquiryNo);
            $('#hfSavedLocationCode').val(data.LocationCode);
            $('#txtSalesQuoteDate').val(data.OrderDate);
            $('#txtSQValidUntillDate').val(data.ValidUntillDate);
            $('#hfContactCompanyNo').val(data.ContactCompanyNo);
            $('#hfSavedCustomerName').val(data.ContactCompanyName);
            $('#hfSavedContactPersonNo').val(data.ContactPersonNo);
            $('#hfSavedShiptoCode').val(data.ShiptoCode);
            $('#hfSavedJobtoCode').val(data.JobtoCode);
            $('#hfSavedPaymentMethod').val(data.PaymentMethodCode);
            //$('#hfSavedTransportMethod').val(data.TransportMethodCode);
            $('#hfPaymentTerms').val(data.PaymentTermsCode);
            $('#hdnSalespersonEmail').val(data.SalespersonEmail);
            $('#hdnApprovalFor').val(data.ApprovalFor);
            BindPaymentTerms();
            //$('#ddlPaymentTerms').val(data.PaymentTermsCode);
            $('#hfSavedIncoTerms').val(data.ShipmentMethodCode);
            $('#ddlIncoTerms').val(data.ShipmentMethodCode);
            companyName_autocomplete();
            //$('#CostSheetOpt').css('display', 'block');
            $('#txtCustomerName').prop('readonly', true);
            $('#txtSalesQuoteDate').prop('readonly', true);
            $('#hfSCRemarksSetupValue').val(data.SCRemarksSetupValue);
            if ($('#hfInqNo').val() == "-1") {
                $('#ddlInquiries').prop('disabled', true);
            }

            if (ScheduleStatus == "Completed") {
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
                //$('#dvIsShortclose').css('display', 'none');
            }
            //else {
            //    $('#dvIsShortclose').css('display', 'block');
            //}

            if (data.ShortcloseStatus) {
                //$('#chkIsShortclose').prop('checked', true);
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
            }
            else {
                //$('#chkIsShortclose').prop('checked', false);
                $('#btnSaveProd').prop('disabled', false);
                $('#btnSave').prop('disabled', false);
            }

            if (data.Status == "Approval pending from finance" || data.Status == "Approval pending from HOD"
                || data.Status == "Approved") {
                $('#btnSaveProd').prop('disabled', true);
                $('#btnSave').prop('disabled', true);
            }
            else {
                $('#btnSaveProd').prop('disabled', false);
                $('#btnSave').prop('disabled', false);
            }

            $('#dataList').css('display', 'block');
            var itemLineNo = "";

            $.each(data.ProductsRes, function (index, item) {

                var prodOpts = "", prodOptsTR = "";
                itemLineNo += item.No + "_" + item.Line_No + ",";

                //prodOpts = "<tr id=\"ProdTR_" + item.No + "\"><td></td><td><a class='SQLineCls' onclick='EditSQProd(\"ProdTR_" + item.No + "\")'><i class='bx bxs-edit'></i></a></td><td>" + item.No + "</td><td>" +
                //    item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.Unit_Cost_LCY +
                //    "</td><td id=\"" + item.No + "_TotalUnitPrice\"></td><td id=\"" + item.No + "_Margin\"></td><td>" + item.Unit_Price + "</td><td>" + item.Delivery_Date + "</td><td id=\"" + item.No +
                //    "_CostSheet\"><a class='CostSheetCls' onclick='showCostSheetDetails(\"" + item.No + "\",\"" + item.Description + "\")'>" + "<span class='badge bg-primary'>Cost Sheet</span></a></td>";

                prodOpts = "<tr id=\"ProdTR_" + item.No + "\"><td></td>";

                if (item.TPTPL_Short_Closed) {
                    prodOpts += "<td><span class='badge bg-secondary'>Shortclosed</span></td>";
                }
                else {

                    if (ScheduleStatus == "Completed" || data.ShortcloseStatus == true) {
                        prodOpts += "<td><a class='SQLineCls' onclick='EditSQProd(\"ProdTR_" + item.No + "\")'><i class='bx bxs-edit'></i></a></td>";
                    }
                    else {
                        prodOpts += "<td><a class='SQLineCls' onclick='EditSQProd(\"ProdTR_" + item.No + "\")'><i class='bx bxs-edit'></i></a>&nbsp;" +
                            "<a class='SQLineCls' title='Click to shortclose' onclick='ShortcloseSQProd(\"" + item.Line_No + "\")'><i class='bx bx-message-rounded-x'></i></a></td>";
                    }

                }
                
                prodOpts += "<td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.Unit_Price +
                    "</td><td>" + item.Delivery_Date + "</td><td>" + item.PCPL_Total_Cost + "</td><td>" + item.PCPL_Margin + "</td><td>" + data.PaymentTermsCode + "</td><td>" +
                    data.ShipmentMethodCode + "</td><td>" + item.PCPL_Transport_Method + "</td><td>" + item.PCPL_Transport_Cost + "</td><td>" + item.PCPL_Sales_Discount + "</td><td>" + 
                    item.PCPL_Commission_Type + "</td><td>" + item.PCPL_Commission + "</td><td>" + item.PCPL_Commission_Amount + "</td><td>" + item.PCPL_Credit_Days + "</td><td>" +
                    item.PCPL_Interest + "</td>";

                var dropShipmentOpt;
                if (item.Drop_Shipment == true) {
                    dropShipmentOpt = "Yes";
                }
                else {
                    dropShipmentOpt = "No";
                }

                //

                prodOpts += "<td><label id=\"" + item.No + "_DropShipment\">" + dropShipmentOpt + "</lable></td>";

                //if ($('#hfProdLineNo').val() != "") {

                //    prodOpts += "<td hidden><label id=\"" + $('#hfProdNo').val() + "_InqProdLineNo\">" + $('#hfProdLineNo').val() + "</label></td>";
                //    $('#hfProdLineNo').val("");
                //}
                //else {

                    prodOpts += "<td hidden></td>";

                //}

                prodOpts += "<td hidden>" + item.PCPL_Commission_Payable + "</td><td>" + item.PCPL_Commission_Payable_Name + "</td>" +
                    "<td hidden>" + item.PCPL_Vendor_No + "</td><td>" + item.PCPL_Vendor_Name + "</td>" + 
                    "<td hidden><label id=\"" + item.No + "_SQLineNo\" style='display:none'>" + item.Line_No + "</label></td>" +
                    "<td><label id=\"" + item.No + "_MarginPercent\">" + item.PCPL_Margin_Percent + " %</label></td><td>" + item.PCPL_Liquid + "</td><td>" +
                    item.PCPL_Concentration_Rate_Percent + "</td><td>" + item.Net_Weight + "</td><td>" + item.PCPL_Liquid_Rate + "</td>";

                if ($('#hfProdNoEdit').val() != "") {

                    $("#ProdTR_" + $('#hfProdNoEdit').val()).append(prodOpts);

                }
                else {
                    prodOptsTR += prodOpts + "</tr>";
                    $('#tblProducts').append(prodOptsTR);
                }

                //
                //prodOpts += "<td><label id=\"" + item.No + "_DropShipment\">" + dropShipmentOpt + "</lable></td><td><label style='display:none' id=\"" + item.No + "_VendorNo\">" +
                //    item.PCPL_Vendor_No + "</label><label id=\"" + item.No + "_VendorName\">" + item.PCPL_Vendor_Name + "</label></td>";

                //if (item.PCPL_Inquiry_No != "-1")
                //    prodOpts += "<td hidden><label id=\"" + item.No + "_InqProdLineNo\">" + item.Line_No + "</label></td>";
                //else
                //    prodOpts += "<td hidden></td>";

                //prodOpts += "<td>" + data.PaymentTermsCode + "</td><td>" + data.ShipmentMethodCode + "<label id=\"" + item.No + "_SQLineNo\" style='display:none'>" + item.Line_No + "</label></td></tr>";

                //$('#tblProducts').append(prodOpts);
                dataTableFunction();

            });

            itemLineNo = data.QuoteNo + "," + itemLineNo;
            $('#hfSalesQuoteResDetails').val(itemLineNo);

            if (SQFor == "ApproveReject") {

                if (LoggedInUserRole != "Admin" && LoggedInUserRole != "Salesperson")
                {
                    $('#dvSQApproveRejectBtn').css('display', 'block');

                    if (LoggedInUserRole == "Finance") {
                        $('#dvSQAprJustificationDetails').css('display', 'block');
                        $('#dvAprDetailsFinanceUser').css('display', 'block');
                        $('#lblJustificationTitle').text("Last 10 Sales Quote Justification Details");

                        $('#AprDetailsCustName').text(data.ContactCompanyName);
                        $('#AprDetailsApprovalFor').text(data.ApprovalFor);
                        $('#AprDetailsJustificationReason').text(data.WorkDescription);
                        $('#tdDetailsStatus').text(data.Status);
                        $('#tdDetailsLocationCode').text(data.LocationCode);
                        //$('#tdOverdue').text($('#txtOutstandingDue').val());
                        //$('#tdTotalExpo').text($('#hdnCustBalanceLCY').val());
                        GetAndFillCompanyIndustry(data.ContactCompanyNo);
                    }
                    else {
                        $('#dvSQAprJustificationDetails').css('display', 'none');
                        $('#dvAprDetailsFinanceUser').css('display', 'none');
                        $('#lblJustificationTitle').text("Last 3 Sales Quote Justification Details");
                    }

                    $('#dvSQJustificationDetails').css('display', 'block');
                    BindJustificationDetails(LoggedInUserRole, data.ContactCompanyNo);
                }

            }
            else {

                $('#dvSQApproveRejectBtn').css('display', 'none');
                $('#dvSQAprJustificationDetails').css('display', 'none');
                $('#dvAprDetailsFinanceUser').css('display', 'none');
                $('#dvSQJustificationDetails').css('display', 'none');
                $('#lblJustificationTitle').text("");

            }

        }

    });

}

function GetProductDetails(productName) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetProductDetails?productName=' + productName,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#hfProdNo').val(data.No);

                    $('#ddlPackingStyle option').remove();
                    var packingstyleOpts = "<option value='-1'>---Select---</option>";
                    var inqProdPackingStyle = "";

                    for (var i = 0; i < data.ProductPackingStyle.length; i++) {

                        if ($('#hfInqProdPackingStyle').val() != "" && $('#hfInqProdPackingStyle').val() != null) {
                            if ($('#hfInqProdPackingStyle').val() == data.ProductPackingStyle[i].Packing_Style_Code) {
                                inqProdPackingStyle = data.ProductPackingStyle[i].PCPL_MRP_Price + "_" + data.ProductPackingStyle[i].Packing_Style_Code +
                                    "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days;        
                            }
                        }

                        packingstyleOpts += "<option value='" + data.ProductPackingStyle[i].PCPL_MRP_Price + "_" + data.ProductPackingStyle[i].Packing_Style_Code +
                            "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days + "'>" +
                            data.ProductPackingStyle[i].Packing_Style_Description + "</option>";

                    }

                    $('#ddlPackingStyle').append(packingstyleOpts);

                    if ($('#hfInqProdPackingStyle').val() != "" && $('#hfInqProdPackingStyle').val() != null) {
                        $('#ddlPackingStyle').val(inqProdPackingStyle);
                        $('#ddlPackingStyle').change();
                    }
                    else {
                        $('#ddlPackingStyle').val('-1');
                    }

                    $('#hfNetWeight').val(data.Net_Weight);
                    $('#hfGrossWeight').val(data.Gross_Weight);
                    $('#lblGrossWeight').text(data.Gross_Weight);
                    $('#txtNetWeight').val(data.Net_Weight);

                    //if (data.Is_Liquid) {
                    //    $('#dvLiquidProdFields').css('display', 'block');
                    //}

                    /*if (data.PCPL_liquid) {
                        $('#dvLiquidProdFields').css('display', 'block');
                        $('#txtSalesPrice').prop('disabled', true);
                        $('#hfIsLiquidProd').val("true");
                        $('#lblGrossWeight').text(data.Gross_Weight);
                        $('#txtNetWeight').val(data.Net_Weight);
                        $('#chkIsCommission').prop('disabled', true);
                        $('#ddlCommissionPerUnitPercent').prop('disabled', true);
                        $('#txtCommissionPercent').prop('disabled', true);
                        $('#txtCommissionAmt').prop('disabled', false);
                        $('#txtCommissionAmt').prop('readonly', false);
                    }
                    else {
                        $('#dvLiquidProdFields').css('display', 'none');
                        $('#txtSalesPrice').prop('disabled', false);
                        $('#hfIsLiquidProd').val("false");
                        $('#lblGrossWeight').text("");
                        $('#txtNetWeight').val("");
                        $('#chkIsCommission').prop('disabled', false);
                        $('#txtCommissionAmt').prop('disabled', true);
                    }*/

                    //$('#txtProdMRP').val(data.PCPL_MRP_Price);
                    //$('#txtProdMRP').prop('readonly', true);
                    //$('#txtBasicPurchaseCost').val(data.PCPL_MRP_Price);
                    //$('#txtProdUnitCost').val(data.Unit_Cost);
                    //$('#txtProdUnitCost').prop('readonly', true);

                    //if ($('#hfUnitPriceEdit').val() != "") {
                    //    $('#txtUnitPrice').val(parseInt($('#hfUnitPriceEdit').val()));
                    //}
                    //else {
                    //    $('#txtUnitPrice').val(data.Unit_Price);
                    //}
                    
                    $('#hfUOM').val(data.Base_Unit_of_Measure);
                    $('#txtUOM').val(data.Base_Unit_of_Measure);

                    if ($('#hfSavedPackingStyle').val() != "" && $('#hfSavedPackingStyle').val() != null) {

                        $('#ddlPackingStyle option').each(function () {
                            var optionValue = $(this).val();
                            
                            if (optionValue.includes($('#hfSavedPackingStyle').val())) {
                                $('#ddlPackingStyle').val(optionValue);
                                $('#ddlPackingStyle').change();
                            }
                        });

                        //$('#ddlPackingStyle').val($('#hfSavedPackingStyle').val());
                    }

                    if ($('#hfiteminvstatus').val() == "NotInInventory") {
                        //remove
                        //if ($('#ddlExcise').val() == "Inclusive") {
                        //    $('#ddlSCVD').val("YES");
                        //}
                    }

                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function EditSQProd(ProdTR) {

    //ResetQuoteLineDetails();
    //var prodNo = $("#" + ProdTR).find("TD").eq(2).html();
    //$('#hfProdNo').val(prodNo);
    //$('#hfProdNoEdit').val(prodNo);
    //$('#txtProductName').val($("#" + ProdTR).find("TD").eq(3).html());
    //$('#txtProductName').change();
    //$('#txtProdQty').val($("#" + ProdTR).find("TD").eq(4).html());
    //$('#hfSavedPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    ////$('#ddlPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    //$('#txtProdMRP').val($("#" + ProdTR).find("TD").eq(7).html());
    //$('#txtProdUnitCost').val($("#" + ProdTR).find("TD").eq(8).html());
    //var totalUnitPrice = $("#" + ProdTR).find("TD").eq(9).html();
    //var margin = $("#" + ProdTR).find("TD").eq(10).html();
    //if (totalUnitPrice != "") {
    //    $('#hfSavedTotalUnitPrice').val(totalUnitPrice);
    //}
    //else {
    //    $('#hfSavedTotalUnitPrice').val("");
    //}

    //if (margin != "") {
    //    $('#hfSavedMargin').val(margin);
    //}
    //else {
    //    $('#hfSavedMargin').val("");
    //}

    //$('#hfUnitPriceEdit').val($("#" + ProdTR).find("TD").eq(11).html());
    //$('#txtUnitPrice').val(parseInt($("#" + ProdTR).find("TD").eq(11).html()));
    //$('#txtDeliveryDate').val($("#" + ProdTR).find("TD").eq(12).html());

    //var dropshipment = $("#" + prodNo + "_DropShipment").text();

    //if (dropshipment == "Yes") {
    //    $('#chkDropShipment').prop('checked', true);
    //}
    //else {
    //    $('#chkDropShipment').prop('checked', false);
    //}
    //$('#ddlItemVendors').val($("#" + prodNo + "_VendorNo").text());
    //$('#chkDropShipment').change();

    //if ($("#" + prodNo + "_InqProdLineNo").val() != "") {
    //    $('#hfProdLineNo').val($("#" + prodNo + "_InqProdLineNo").val());
    //}

    //$('#txtLineDetailsPaymentTerms').val($("#" + ProdTR).find("TD").eq(17).html());
    //$('#txtLineDetailsIncoTerms').val($("#" + ProdTR).find("TD").eq(18).html());

    ResetQuoteLineDetails();
    var prodNo = $("#" + ProdTR).find("TD").eq(2).html();
    $('#hfProdNo').val(prodNo);
    $('#hfProdNoEdit').val(prodNo);
    $('#hfSavedPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    $('#txtProductName').val($("#" + ProdTR).find("TD").eq(3).html());
    $('#txtProductName').blur();

    //alert($('#ddlPackingStyle > option').length);
    //$('#ddlPackingStyle option').each(function () {
    //    var optionValue = $(this).val();
    //    alert(optionValue);
    //    if (optionValue.includes($("#" + ProdTR).find("TD").eq(6).html())) {
    //        $('#ddlPackingStyle').val(optionValue);
    //        $('#ddlPackingStyle').change();
    //    }
    //});

    $('#hfSQProdLineNo').val($("#" + prodNo + "_SQLineNo").text());
    //$('#ddlPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    $('#txtProdQty').val($("#" + ProdTR).find("TD").eq(4).html());
    //$('#hfSavedPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    //$('#ddlPackingStyle').val($("#" + ProdTR).find("TD").eq(6).html());
    $('#txtProdMRP').val($("#" + ProdTR).find("TD").eq(7).html());
    $('#ddlTransportMethod option:selected').text($("#" + ProdTR).find("TD").eq(14).html());
    $('#txtSalesPrice').val($("#" + ProdTR).find("TD").eq(8).html());
    $('#txtTransportCost').val($("#" + ProdTR).find("TD").eq(15).html());
    $('#txtSalesDiscount').val($("#" + ProdTR).find("TD").eq(16).html());
    $('#txtDeliveryDate').val($("#" + ProdTR).find("TD").eq(9).html());

    var isLiquidProd = $("#" + ProdTR).find("TD").eq(30).html();
    if ($("#" + ProdTR).find("TD").eq(17).html() != "") {

        if (isLiquidProd == "true") {
            $('#chkIsCommission').prop('checked', false);
            $('#txtSalesPrice').prop('readonly', true);
            $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(19).html());
            $('#txtCommissionAmt').prop('disabled', false);
            $('#ddlCommissionPayable').prop('disabled', false);
        }
        else {
            $('#txtSalesPrice').prop('readonly', false);
            $('#chkIsCommission').prop('checked', true);
            $('#chkIsCommission').change();
            $('#ddlCommissionPerUnitPercent').val($("#" + ProdTR).find("TD").eq(17).html());
            $('#txtCommissionPercent').val($("#" + ProdTR).find("TD").eq(18).html());
            $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(19).html());
        }

        var commissionPayable = $("#" + ProdTR).find("TD").eq(24).html();
        if (commissionPayable != "") {
            $('#ddlCommissionPayable').val(commissionPayable);
        }
        else {
            $('#ddlCommissionPayable').val('-1');
        }
        
    }

    $('#txtCreditDays').val($("#" + ProdTR).find("TD").eq(20).html());
    $('#txtMargin').val($("#" + ProdTR).find("TD").eq(11).html());
    $('#spnMarginPercent').text($("#" + prodNo + "_MarginPercent").text());
    $('#txtInterest').val($("#" + ProdTR).find("TD").eq(21).html());

    if ($("#" + prodNo + "_DropShipment").text() == "Yes") {
        $('#chkDropShipment').prop('checked', true);
        $('#hfSavedItemVendorNo').val($("#" + ProdTR).find("TD").eq(26).html());
        $('#chkDropShipment').change();
        //$('#ddlItemVendors').val($("#" + ProdTR).find("TD").eq(26).html());
    }
    else {
        $('#chkDropShipment').prop('checked', false);
    }
    
    $('#txtTotalCost').val($("#" + ProdTR).find("TD").eq(10).html());
    if ($("#" + prodNo + "_InqProdLineNo").val() != "") {
        $('#hfProdLineNo').val($("#" + prodNo + "_InqProdLineNo").val());
    }

    
    if (isLiquidProd == "true") {
        $('#dvLiquidProdFields').css('display', 'block');
        $('#chkIsLiquidProd').prop('checked', true);

        $('#hfIsLiquidProd').val($("#" + ProdTR).find("TD").eq(30).html());
        $('#txtConcentratePercent').val($("#" + ProdTR).find("TD").eq(31).html());
        $('#txtNetWeight').val($("#" + ProdTR).find("TD").eq(32).html());
        $('#txtLiquidRate').val($("#" + ProdTR).find("TD").eq(33).html());
    }
    else {
        $('#chkIsLiquidProd').prop('checked', false);
    }
    //$('#txtProdUnitCost').val($("#" + ProdTR).find("TD").eq(8).html());
    //var totalUnitPrice = $("#" + ProdTR).find("TD").eq(9).html();
    //var margin = $("#" + ProdTR).find("TD").eq(10).html();
    //if (totalUnitPrice != "") {
    //    $('#hfSavedTotalUnitPrice').val(totalUnitPrice);
    //}
    //else {
    //    $('#hfSavedTotalUnitPrice').val("");
    //}

    //if (margin != "") {
    //    $('#hfSavedMargin').val(margin);
    //}
    //else {
    //    $('#hfSavedMargin').val("");
    //}

    //$('#hfUnitPriceEdit').val($("#" + ProdTR).find("TD").eq(11).html());
    //$('#txtUnitPrice').val(parseInt($("#" + ProdTR).find("TD").eq(11).html()));
    

    //var dropshipment = $("#" + prodNo + "_DropShipment").text();

    //if (dropshipment == "Yes") {
    //    $('#chkDropShipment').prop('checked', true);
    //}
    //else {
    //    $('#chkDropShipment').prop('checked', false);
    //}
    //$('#ddlItemVendors').val($("#" + prodNo + "_VendorNo").text());
    //$('#chkDropShipment').change();

    //if ($("#" + prodNo + "_InqProdLineNo").val() != "") {
    //    $('#hfProdLineNo').val($("#" + prodNo + "_InqProdLineNo").val());
    //}

}

function DeleteSQProd(ProdTR) {

    $("#" + ProdTR).remove();

}

function CalculateFormula() {

    //if ($('#ddlNoSeries').val() == "-1") {

    //    var msg = "Please select Location No Series";
    //    ShowErrMsg(msg);

    //}
    if($('#txtProductName').val() == "") {
        //var msg = "Please enter product name";
        //ShowErrMsg(msg);
    }
    else {
        var CostPrice = 0, Interest = 0, BasicPurchaseCost = 0, SalesPrice = 0;
        $('#txtTotalCost').val("0");
        if ($('#txtSalesDiscount').val() == "") {
            $('#txtSalesDiscount').val("0");
        }
        if ($('#txtCommissionAmt').val() == "") {
            $('#txtCommissionAmt').val("0");
        }
        if ($('#txtTransportCost').val() == "") {
            $('#txtTransportCost').val("0");
        }

        if ($('#txtProdQty').val() == "") {
            $('#txtProdQty').val("0");
        }
        if ($('#txtBasicPurchaseCost').val() == "") {
            BasicPurchaseCost = parseFloat("0");
        }
        else {
            BasicPurchaseCost = parseFloat($('#txtBasicPurchaseCost').val());
        }
        
        if ($('#txtSalesPrice').val() == "") {
            $('#txtSalesPrice').val("0");
            SalesPrice = parseFloat("0");
        }
        else {
            SalesPrice = parseFloat($('#txtSalesPrice').val());
        }
        
        if ($('#hfiteminvstatus').val() == "InInventory") {
            if ($('#ddlTransportMethod').val() == "TOPAY") {
                CostPrice = parseFloat($('#txtBasicPurchaseCost').val()) + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtInterest').val()); // + parseFloat($('#txtInsurance').val());
            }
            else if ($('#ddlTransportMethod').val() == "PAID") {
                CostPrice = parseFloat($('#txtBasicPurchaseCost').val()) + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtTransportCost').val()) + parseFloat($('#txtInterest').val()); // + parseFloat($('#txtInsurance').val());
            }
            
            $('#txtTotalCost').val(CostPrice.toFixed(2));
            $('#txtTotalCost').prop('disabled', true);
            
        }
        else {
            //$('#hfAEDAmt').val();
            //$('#lblAEDAmt').text("( AED : " + $('#hfAEDAmt').val() + " )");

            //Temp Interest Start
            //var TempInterestRate = 0.50;
            var TempCreditDays = 10;

            //var Interest = (BasicPurchaseCost * $('#hfInterestRate').val()) * (parseFloat(TempCreditDays) / 365);
            //$('#lblInterest').text(parseFloat(Interest));
            //Temp Interest End

            var creditDays = GetCreditDaysForNotInInventory();
            $('#txtCreditDays').val(creditDays);
            $('#txtCreditDays').prop('disabled', true);
            var InterestRate = parseFloat($('#hfInterestRate').val()) / 100;
            Interest = (BasicPurchaseCost * InterestRate) * (parseInt(creditDays) / 365);
            //Interest = (BasicPurchaseCost * $('#hfInterestRate').val()) * (parseInt(creditDays) / 365);
            //Interest = (BasicPurchaseCost * (GetInterestRate() / 100)) * (parseFloat(GetCreditDaysForNotInInventory()) / 365);
            $('#txtInterest').val(parseFloat(Interest).toFixed(2));
            $('#txtInterest').prop('disabled', true);
            $('#spnInterestRate').text("");
            $('#spnInterestRate').text($('#hfInterestRate').val());
            if ($('#ddlTransportMethod').val() == "TOPAY") {

                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val())  + parseFloat($('#txtInsurance').val());
            
            }
            else if ($('#ddlTransportMethod').val() == "PAID"){
            
                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtTransportCost').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val()) + parseFloat($('#txtInsurance').val());
            
            }

            $('#txtTotalCost').val(CostPrice.toFixed(2));
            $('#txtTotalCost').prop('disabled', true);
        }

        $('#txtMargin').val((parseFloat($('#txtSalesPrice').val()) - parseFloat($('#txtTotalCost').val())).toFixed(2));
        $('#txtMargin').prop('disabled', true);
        if (SalesPrice > 0) {
            if ($('#txtTotalCost').val().length > 0 && parseFloat($('#txtTotalCost').val()) > 0) {
                $('#spnMarginPercent').text(((parseFloat($('#txtMargin').val()) * 100) / parseFloat($('#txtTotalCost').val())).toFixed(2) + " %");
            }
            else {
                $('#spnMarginPercent').text("0 %");
            }
        }
        else {
            $('#spnMarginPercent').text("0 %");
        }

    }
}

function ShortcloseSQProd(SQProdLineNo) {
    $('#modalShortclose').css('display', 'block');
    $('#ShortcloseTitle').text("Shortclose Sales Quote Product");
    $('#hfShortcloseType').val("SalesQuoteProd");
    $('#hfSQProdLineNoForShortclose').val(SQProdLineNo);
    BindShortcloseReason();
}


/*function UpdateValueForTotalCost() {

    if ($('#hfiteminvstatus').val() == "InInventory") {

    var Commission = 0, CostPrice = 0, TotalCost = 0, SalesDiscount = 0, AED = 0, TotalAED = 0;
    var Transport = 0, CreditNoOfDays = 0, Interest = 0, InterestAmt = 0, TotalRequiredQty = 0, TotalSalesPrice = 0;
    var Margin = 0, SalesPrice = 0, IntRate = 0, Insurance = 0, BasicPrice = 0, Excise = 0, TaxGroupAmount = 0;
    var CreditDays = 0, CrDays = 0;

    $.ajax(
        {
            url: '/SPSalesQuotes/GetDetailsOfInInventoryLineItems',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $.each(data, function (index, item) {
                    BasicPrice = parseFloat(item.BasicPrice) * parseFloat(item.RequiredQty);
                    Excise = parseFloat(item.Excise) * parseFloat(item.RequiredQty);
                    //SalesDiscount = parseFloat($('#txtSalesDiscAmount').val()) * parseFloat(item.RequiredQty);
                    //Commission = parseFloat($('#txtCommissionAmt').val()) * parseFloat(item.RequiredQty);
                    AED = parseFloat(GetAEDForInInventory(item.EntryNo)) * parseFloat(item.RequiredQty);
                    TotalAED = TotalAED + AED;
                    //Transport = parseFloat($('#txtTransportCost').val()) * parseFloat(item.RequiredQty);
                    CreditNoOfDays = parseFloat(GetCreditDaysForINInventory(item.EntryNo));
                    CrDays = parseInt(parseFloat(CreditNoOfDays) * parseFloat(item.RequiredQty));
                    CreditDays = CreditDays + parseInt(parseFloat(CreditNoOfDays) * parseFloat(item.RequiredQty));
                    IntRate = parseFloat($('#spnInterestRate').text());
                    Interest = parseFloat(item.BasicPrice) * (IntRate / 100) * (parseFloat(parseInt(CreditNoOfDays) / 365)) * parseFloat(item.RequiredQty);
                    InterestAmt = InterestAmt + Interest;
                    //Insurance = parseFloat($('#txtInsurance').val()) * parseFloat(item.RequiredQty);
                    TotalRequiredQty = TotalRequiredQty + parseFloat(item.RequiredQty);
                    CostPrice = (BasicPrice + SalesDiscount + Commission + Interest + Insurance + Transport);
                    if ($('#ddlExcise').val() == "Exclusive") {
                        TotalCost = parseFloat(CostPrice) - parseFloat(Excise);
                    }
                    else if ($('#ddlExcise').val() == "Inclusive") {
                        if ($('#ddlSCVD').val() == "YES") {
                            TotalCost = parseFloat(CostPrice) - parseFloat(AED);
                        }
                        else if ($('#ddlSCVD').val() == "NO") {
                            TotalCost = parseFloat(CostPrice);
                        }
                        else {
                            TotalCost = parseFloat(CostPrice);
                        }
                    }
                    else {
                        TotalCost = parseFloat(CostPrice);
                    }

                    //Margin = parseFloat($('#txtMargin').val()) * parseFloat(item.RequiredQty);
                    SalesPrice = parseFloat($('#txtSalesPrice').val()) * parseFloat(item.RequiredQty);
                    //TaxGroupAmount = parseFloat($('#lblTaxGroupAmount').text()) * parseFloat(item.RequiredQty);
                    //TotalSalesPrice = parseFloat($('#txtTotalSales').val()) * parseFloat(item.RequiredQty);

                });

                let salesQuote = {};

                salesQuote.BasicPrice = BasicPrice;
                salesQuote.Excise = Excise;
                salesQuote.SalesDiscount = SalesDiscount;
                salesQuote.Commission = Commission;
                salesQuote.AED = AED;
                salesQuote.TotalAED = TotalAED;
                salesQuote.Transport = Transport;
                salesQuote.CreditNoOfDays = CreditNoOfDays;
                salesQuote.CrDays = CrDays;
                salesQuote.CreditDays = CreditDays;
                salesQuote.IntRate = IntRate;
                salesQuote.Interest = Interest;
                salesQuote.InterestAmt = InterestAmt;
                salesQuote.Insurance = Insurance;
                salesQuote.TotalRequiredQty = TotalRequiredQty;
                salesQuote.CostPrice = CostPrice;
                salesQuote.TotalCost = TotalCost;
                salesQuote.Margin = Margin;
                salesQuote.SalesPrice = SalesPrice;
                salesQuote.TaxGroupAmount = TaxGroupAmount;
                salesQuote.TotalSalesPrice = TotalSalesPrice

                var jsonSalesQuote = JSON2.stringify(salesQuote);

                $.ajax({
                    type: "POST",
                    url: "/SPSalesQuotes///UpdateValueForTotalCost",
                    data: jsonSalesQuote,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (msg) {
                        alert('In Ajax');
                    }
                });

                $('#txtCreditDays').val(parseInt(CreditDays / parseFloat(TotalRequiredQty)));
                $('#lblInterest').val(parseFloat(InterestAmt) * parseFloat(TotalRequiredQty));
                $('#hfAEDAmt').val(parseFloat(TotalAED) * parseFloat(TotalRequiredQty));
                bindSalesQuoteDetailLineItems();

            },
            error: function () {
                //alert("error");
            }
        }
    );

    }
}*/

var interestRate = 0;

function GetInterestRate() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetInterestRate',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {
                    $('#lblInterestRate').text(data.PCPL_Interest_Rate_Percent);
                    $('#hfInterestRate').val(data.PCPL_Interest_Rate_Percent);
                    $('#spnInterestRate').text(data.PCPL_Interest_Rate_Percent);
                }
                
            },
            error: function () {
                //alert("error");
            }
         }
    );

}

function AdditionalQtyChange() {

    if ($('#hfiteminvstatus').val() == "InInventory") {

        if ($('#txtAdditionalQty').val() > 0) {
            $('#txtAdditionalQty').attr("readonly", true);
        }

        let salesQuoteDetails = {};

        salesQuoteDetails.Qty = $('#txtProdQty').val();
        salesQuoteDetails.AdditionalQty = $('#txtAdditionalQty').val();

        if ($('#txtAdditionalQty').val() > 0) {
            salesQuoteDetails.MRPAmt = ($('#txtProdMRP').val() * $('#txtAdditionalQty').val());
            salesQuoteDetails.BasicPriceAmt = ($('#txtBasicPurchaseCost').val() * $('#txtAdditionalQty').val());
            salesQuoteDetails.ExciseAmt = ($('#txtExcise').val() * $('#txtAdditionalQty').val());
            salesQuoteDetails.QuantityDiscount = ($('#txtAdditionalQty').val() * $('#hfQuantityDiscount').val());
            salesQuoteDetails.TradeDiscount = ($('#txtAdditionalQty').val() * $('#hfTradeDiscount').val());
            salesQuoteDetails.ConsigneeDiscount = ($('#txtAdditionalQty').val() * $('#hfConsigneeDiscount').val());
            salesQuoteDetails.AdditionalSalesDiscount = ($('#txtAdditionalQty').val() * $('#txtSalesDiscount').val());
            salesQuoteDetails.AdditionalCommission = ($('#txtAdditionalQty').val() * $('#txtCommissionAmt').val());
            salesQuoteDetails.AdditionalTransportCost = ($('#txtAdditionalQty').val() * $('#txtTransportCost').val());

            $('#hfAdditionalCreditDays').val($('#txtAdditionalQty').val() * $('#hfCreditDays').val());
            $('#hfAdditionalAED').val($('#txtAdditionalQty').val() * $('#hfAEDAmt').val());
            salesQuoteDetails.InterestAdditionalAmt = ($('#txtAdditionalQty').val() * $('#lblInterest').val());
            salesQuoteDetails.AdditionalInsurance = ($('#txtAdditionalQty').val() * $('#txtInsurance').val());
            salesQuoteDetails.AdditionalTotalCost = (salesQuoteDetails.BasicPriceAmt + salesQuoteDetails.AdditionalSalesDiscount + salesQuoteDetails.AdditionalCommission + salesQuoteDetails.InterestAdditionalAmt + salesQuoteDetails.AdditionalInsurance + salesQuoteDetails.AdditionalTransportCost);

            if ($('#ddlExcise').val() == "Exclusive") {
                salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost - salesQuoteDetails.ExciseAmt;
            }
            else if ($('#ddlExcise').val() == "Inclusive") {

                if ($('#ddlSCVD').val() == "YES") {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost - $('#hfAdditionalAED').val();
                }
                else if ($('#ddlSCVD').val() == "NO") {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
                }
                else {
                    salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
                }
            }
            else {
                salesQuoteDetails.AddTotalCost = salesQuoteDetails.AdditionalTotalCost;
            }

            salesQuoteDetails.AdditionalMargin = ($('#txtAdditionalQty').val() * $('#txtMargin').val());
            salesQuoteDetails.AdditionalSalesPrice = ($('#txtAdditionalQty').val() * $('#txtSalesPrice').val());
            salesQuoteDetails.AdditionalTaxGroupAmount = ($('#txtAdditionalQty').val() * $('#lblTaxGroupAmount').val());
            salesQuoteDetails.AdditionalTotalSalesPrice = ($('#txtAdditionalQty').val() * $('#txtTotalSales').val());

            if ($('#ddlNoSeries').val() == "-1") {
                salesQuoteDetails.LocationName = "";
                salesQuoteDetails.Location_Code = "";
            }
            else {
                salesQuoteDetails.LocationName = $("#ddlNoSeries option:selected").text();
                salesQuoteDetails.Location_Code = $('#ddlNoSeries').val();
            }

            salesQuoteDetails.EntryNo = "";
            salesQuoteDetails.SupplierName = "";
            salesQuoteDetails.ManufactureName = "";
            salesQuoteDetails.ManufacturingExciseAmount = 0;
            salesQuoteDetails.DealerName = "";
            salesQuoteDetails.DealerExciseAmount = 0;
            salesQuoteDetails.AvailableQty = 0;
            salesQuoteDetails.RequiredQty = $('#txtAdditionalQty').val();
            salesQuoteDetails.UOM = $('#hfUOM').val();
            salesQuoteDetails.Order_Date = "";
            salesQuoteDetails.DocumentNo = "";
            salesQuoteDetails.ManufacturerQty = 0;
            salesQuoteDetails.Prod_No = $('#hfProdNo').val();
            salesQuoteDetails.ProductName = $('#txtProductName').val();

            if ($('#ddlPackingStyle').val() == "-1") {
                salesQuoteDetails.PackagingStyle = "";
            }
            else {
                salesQuoteDetails.PackagingStyle = $('#ddlPackingStyle').val();
            }

            salesQuoteDetails.SalesDiscount = salesQuoteDetails.AdditionalSalesDiscount;

            if ($('#ddlCommissionPayable').val() == "-1") {
                salesQuoteDetails.Commission = "";
            }
            else {
                salesQuoteDetails.Commission = $('#ddlCommissionPayable').val();
            }
            
            salesQuoteDetails.CommissionAmount = salesQuoteDetails.AdditionalCommission;

            if ($('#ddlIncoTerms').val() == "-1") {
                salesQuoteDetails.IncoTerms = "";
            }
            else {
                salesQuoteDetails.IncoTerms = $('#ddlIncoTerms').val();
            }

            if ($('#ddlSCVD').val() == "-1") {
                salesQuoteDetails.SVCD = "";
            }
            else {
                salesQuoteDetails.SVCD = $('#ddlSCVD').val();
            }

            if ($('#ddlTransportMethod').val() == "-1") {
                salesQuoteDetails.Transport = "";
            }
            else {
                salesQuoteDetails.Transport = $('#ddlTransportMethod').val();
            }

            salesQuoteDetails.TransportAmount = salesQuoteDetails.AdditionalTransportCost;
            salesQuoteDetails.TotalCost = salesQuoteDetails.AddTotalCost;
            salesQuoteDetails.PaymentTerms = $('#ddlPaymentTerms').val();
            salesQuoteDetails.Margin = $('#txtMargin').val();
            salesQuoteDetails.CreditDays = $('#hfAdditionalCreditDays').val();
            salesQuoteDetails.SalesPrice = salesQuoteDetails.AdditionalSalesPrice;
            salesQuoteDetails.Insurance = salesQuoteDetails.AdditionalInsurance;

            if ($('#ddlTaxGroup').val() == "-1") {
                salesQuoteDetails.TaxGroups = "";
            }
            else {
                salesQuoteDetails.TaxGroups = $('#ddlTaxGroup').val();
            }
            
            salesQuoteDetails.Interest = salesQuoteDetails.InterestAdditionalAmt;

            if ($('#spnInterestRate').text() == "") {
                salesQuoteDetails.InterestRate = "0"
            }
            else {
                salesQuoteDetails.InterestRate = $('#spnInterestRate').text();
            }
            
            salesQuoteDetails.TaxGroupAmount = salesQuoteDetails.AdditionalTaxGroupAmount;
            salesQuoteDetails.TotalSalesPrice = salesQuoteDetails.AdditionalTotalSalesPrice;
            salesQuoteDetails.Type = "InInventoryAdditionalQty";

            if ($('#ddlExcise').val() == "-1") {
                salesQuoteDetails.ExciseType = "";
            }
            else {
                salesQuoteDetails.ExciseType = $('#ddlExcise').val();
            }
            
            salesQuoteDetails.MRP = salesQuoteDetails.MRPAmt;

            salesQuoteDetails.InquiryType = $('#ddlInquiryType').val();
            //salesQuoteDetails.CustomerName = $('#txtCustomerName').val();
            salesQuoteDetails.Sell_to_Customer_No = $('#hfCustomerNo').val();

            if ($('#ddlContactName').val() == "-1") {
                salesQuoteDetails.Sell_to_Contact = "";
            }
            else {
                salesQuoteDetails.Sell_to_Contact = $('#ddlContactName').val();
            }
            
            salesQuoteDetails.SalesQuoteDate = $('#txtSalesQuoteDate').val();
            //salesQuoteDetails.PaymentMethod = $('#ddlPaymentMethods').val();
            salesQuoteDetails.ConsigneeAddress = $("#ddlConsigneeAddress option:selected").text();
            salesQuoteDetails.DeliveryDate = $('#txtDeliveryDate').val();
            salesQuoteDetails.Remarks = "";
            //salesQuoteDetails.IPAddress =
            //salesQuoteDetails.Browser =
            //salesQuoteDetails.WebURL =
            //salesQuoteDetails.ModifiedBy =
            //salesQuoteDetails.Msg =
            //salesQuoteDetails.ConsigneeCode =


            var jsonSalesQuoteDetails = JSON2.stringify(salesQuoteDetails);

            $.ajax({
                type: "POST",
                url: "/SPSalesQuotes/AddUpdateOnAddQtyChange",
                data: jsonSalesQuoteDetails,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    alert('In Ajax');
                }
            });

        }
    }
}

function PaymentTermsChange() {

    if ($('#ddlPaymentTerms').val() != "-1") {

        if ($('#hfiteminvstatus').val() == "InInventory") {

            var IntRate = 0, Interest = 0, InterestAmt = 0, TotalRequiredQty = 0;
            var CreditNoOfDays = 0;
            var CreditDays = 0;

            IntRate = parseFloat($('#hfInterestRate').val());

            $.ajax(
                {
                    url: '/SPSalesQuotes/GetDetailsOfInInventoryLineItems',
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (data) {

                        $.each(data, function (index, item) {

                            CreditNoOfDays = parseFloat(GetCreditDaysForINInventory(item.EntryNo));
                            CreditDays = CreditDays + parseInt(parseInt(CreditNoOfDays) * parseFloat(item.RequiredQty));
                            Interest = parseFloat(item.BasicPrice) * (IntRate / 100) * (parseFloat(parseInt(CreditNoOfDays)) / 365);
                            InterestAmt = InterestAmt + parseFloat(Interest);
                            TotalRequiredQty = TotalRequiredQty + parseFloat(item.RequiredQty);
                        });

                        $('#txtCreditDays').val(parseInt(CreditDays / parseFloat(TotalRequiredQty)));
                        $('#txtInterest').val(parseFloat(InterestAmt) / parseFloat(TotalRequiredQty));
                        $('#spnInterestRate').text(IntRate);

                    },
                    error: function () {
                        //alert("error");
                    }
                }
            );
        }
        //$('#txtAdditionalQty').change();
        if ($('#ddlPaymentTerms').val() != null) {
        const PaymentTermsDetails = $('#ddlPaymentTerms').val().split('_');
        if (PaymentTermsDetails[1] != "") {
            var PaymentTermsDays = PaymentTermsDetails[1].substring(0, PaymentTermsDetails[1].length - 1);
            $('#hfPaymentTermsDays').val(parseInt(PaymentTermsDays));
            }
        }
        CalculateFormula();
        ////UpdateValueForTotalCost();

    }
    else {
        $('#hfInterestAmt').val("0");
        $('#txtCreditDays').val("0");
        $('#txtInterest').val("0");
    }
}

function GetCurrentDate() {

    const date = new Date();

    let day = date.getDate();
    if (day <= 9) {
        day = "0" + day;
    }

    let month = date.getMonth() + 1;
    if (month <= 9) {
        month = "0" + month;
    }

    let year = date.getFullYear();

    let curDate = year + '-' + month + '-' + day;
    return curDate;

}
function commaSeparateNumber(val) {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
    }
    return val;
}

function updateInquiryNotifcationStatus() {

    if ($('#lblInqNo').text() != "") {

        $.post(
            apiUrl + 'updateInquiryNotifcationStatus?InqNo=' + $('#lblInqNo').text(),
            function (data) {

            }
        );
    }
}

function GetSQNoFromInqNo(InqNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    var SQNo = "";

    $.get(apiUrl + 'GetSQNoFromInqNo?InqNo=' + InqNo, function (data) {

        if (data != "") {
            SQNo = data;
        }

    });

    return SQNo;
}

function CheckFieldValues() {

    var errMsg = "";

    //if ($('#hfSQEdit').val() == "true" && ($('#txtSalesQuoteDate').val() == "" || $('#txtCustomerName').val() == "" || $('#ddlContactName').val() == "-1" ||
    //    $('#ddlPaymentTerms').val() == "-1" || $('#ddlIncoTerms').val() == "-1" || $('#txtSQValidUntillDate').val() == "")) {
    //    errMsg = "Please Fill Details";
    //}
    //if ($('#hfSQEdit').val() == "false" && ($('#ddlLocations').val() == "-1" || $('#txtSalesQuoteDate').val() == "" || $('#txtCustomerName').val() == "" || $('#ddlContactName').val() == "-1" ||
    //    $('#ddlPaymentTerms').val() == "-1" || $('#ddlIncoTerms').val() == "-1" || $('#txtSQValidUntillDate').val() == "")) {
    //        errMsg = "Please Fill Details";
    //}

    if ($('#ddlLocations').val() == "-1" || $('#txtSalesQuoteDate').val() == "" || $('#txtCustomerName').val() == "" || $('#ddlContactName').val() == "-1" ||
        $('#ddlPaymentTerms').val() == "-1" || $('#ddlIncoTerms').val() == "-1" || $('#txtSQValidUntillDate').val() == "") {
            errMsg = "Please Fill Details";
    }
    else if ($('#txtSalesQuoteDate').val() > $('#txtSQValidUntillDate').val()) {
        errMsg = "Sales quote valid untill date should be date after sales quote date";
    }
    else if ($('#tblProducts').text().trim() == "") {

        errMsg = "Please Add Product Details";
    }

    return errMsg;
}

function CheckCPersonFieldValues() {

    var errMsg = "";

    if ($('#txtCPersonName').val() == "" || $('#txtCPersonMobile').val() == "" || $('#txtCPersonEmail').val() == "" || $('#ddlDepartment').val() == "-1" ||
        $('#txtJobResponsibility').val() == "") {
        errMsg = "Please Fill Details";
    }

    return errMsg;
}

function CheckNewBilltoAddressValues() {

    var errMsg = "";

    if ($('#txtNewShiptoAddCode').val() == "" || $('#txtNewShiptoAddress').val() == "" || $('#txtNewShiptoAddPostCode').val() == "" ||
        $('#ddlNewShiptoAddArea').val() == "-1" || $('#txtNewShiptoAddState').val() == "" || $('#txtNewShiptoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewShiptoAddGSTNo').val().length > 0 && $('#txtNewShiptoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewShiptoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function CheckNewDeliverytoAddressValues() {

    var errMsg = "";

    if ($('#txtNewJobtoAddCode').val() == "" || $('#txtNewJobtoAddress').val() == "" || $('#txtNewJobtoAddPostCode').val() == "" ||
        $('#ddlNewJobtoAddArea').val() == "-1" || $('#txtNewJobtoAddState').val() == "" || $('#txtNewJobtoAddGSTNo').val() == "") {

        errMsg = "Please Fill Details";
    }
    else if ($('#txtNewJobtoAddGSTNo').val().length > 0 && $('#txtNewJobtoAddGSTNo').val().length != 15) {
        errMsg = "GST No. should be in 15 character";
    }
    else if (!$('#txtNewJobtoAddGSTNo').val().includes($('#hfCustPANNo').val())) {
        errMsg = "GST No should contains PAN No";
    }

    return errMsg;
}

function ResetQuoteLineDetails()
{

    if ($('#tblProducts tr').length > 0) {
        $('#ddlIncoTerms').prop('disabled', true);
        $('#ddlPaymentTerms').prop('disabled', true);
    }

    $('#txtProductName').val("");
    $('#ddlPackingStyle').empty();
    $('#ddlPackingStyle').append("<option value='-1'>---Select---</option>");
    $('#ddlPackingStyle').val("-1");
    $('#chkIsLiquidProd').prop('checked', false);
    $('#txtConcentratePercent').val("");
    $('#txtNetWeight').val("");
    $('#txtLiquidRate').val("");
    $('#lblGrossWeight').text("");
    $('#dvLiquidProdFields').css('display', 'none');
    $('#hfIsLiquidProd').val("false");
    $('#txtProdQty').val("");
    $('#txtBasicPurchaseCost').val("");
    $('#txtBasicPurchaseCost').prop('disabled', false);
    //$('#ddlIncoTerms').val("-1");
    $('#ddlIncoTerms').change();
    //$('#ddlPaymentTerms').val("-1");
    $('#txtUOM').val("");
    //$('#txtProdUnitCost').val("");
    //$('#txtUnitPrice').val("");
    //$('#ddlTransportMethod').val("-1");
    //$('#ddlTransportMethod').prop('disabled', false);
    $('#txtTransportCost').val("");
    
    //$('#txtPurDiscount').val("");
    //$('#txtSalesDiscPercentage').val("");
    //$('#txtSalesDiscPercentage').prop('disabled', true);
    //$('#txtSalesDiscAmount').val("");
    //$('#ddlSalesDiscPerUnitPercent').val('-1');
    //$('#chkIsSalesDisc').prop('checked', false);
    //$('#chkIsSalesDisc').change();
    
    
    
    $('#txtDeliveryDate').val("");
    //$('#txtCommissionPer').val("");
    //$('#txtCommissionPer').prop('disabled', true);
    //$('#txtCommissionAmt').val("");
    //$('#ddlCommissionPerUnitPercent').val('-1');
    //$('#chkIsCommission').prop('checked', false);
    //$('#chkIsCommission').change();

    //$('#txtSalesDiscPercent').val("");
    $('#txtSalesDiscount').val("");
    //$('#ddlSalesDiscPerUnitPercent').val("-1");
    //$('#chkIsSalesDisc').prop('checked', false);

    $('#ddlCommissionPerUnitPercent').val("-1");
    $('#txtCommissionPercent').val("");
    $('#txtCommissionAmt').val("");
    $('#chkIsCommission').prop('checked', false);
    $('#ddlCommissionPerUnitPercent, #txtCommissionPercent, #txtCommissionAmt').prop('disabled', true);
    $('#ddlCommissionPayable').val("-1");
    $('#ddlCommissionPayable').prop('disabled', true);
    $('#txtMargin').val("");
    $('#txtMargin').prop('disabled', false);
    $('#spnMarginPercent').text("");

    $('#txtInterest').val("");
    $('#spnInterestRate').val("0");
    $('#txtInterest').prop('disabled', false);

    $('#txtTotalCost').val("");
    //$('#lblInterest').text("");
    //$('#spnInterestRate').val("0");
    $('#txtCreditDays').val("0");
    $('#txtCreditDays').prop('disabled', false);
    $('#txtSalesPrice').val("");
    $('#chkDropShipment').prop('checked', false);
    $('#ddlItemVendors').empty();
    $('#ddlItemVendors').append("<option value='-1'>---Select---</option>");
    $('#ddlItemVendors').val('-1');
    //$('#chkDropShipment').change();

    $('#hfProdNoEdit').val("");
    $('#hfUnitPriceEdit').val("");
    $('#hfSavedPackingStyle').val("");
    $('#hfSavedTotalUnitPrice').val("");
    $('#hfSavedMargin').val("");
}

function AddReqQty() {

    $('#txtProdQty').val($('#txtReqQty').val());
    $('#txtBasicPurchaseCost').val($('#txtMrpFromILE').val());
    //alert($('#txtReqQty').val());
    $('#btnClose,.btn-close').click();

}

function BindVendors() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetVendorsForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#ddlCommissionPayable option').remove();
                var vendorsOpts = "<option value='-1'>---Select---</option>";
                $.each(data, function (index, item) {
                    vendorsOpts += "<option value='" + item.No + "'>" + item.Name + "</option>";
                });

                $('#ddlCommissionPayable').append(vendorsOpts);
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function GenerateCostSheet(ItemNo, ItemName) {

    if ($('#hfSalesQuoteResDetails').val() == "") {

        var errMsg = "Fill cost sheet details after create sales quote";
        ShowErrMsg(errMsg);
    }
    else {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
        const SQDetails = $('#hfSalesQuoteResDetails').val().split(',');
        var ItemLineNo;

        var SalesQuoteNo = SQDetails[0];

        for (var a = 0; a < SQDetails.length; a++) {

            if (SQDetails[a].includes(ItemNo)) {

                const SQLineDetails = SQDetails[a].split('_');
                ItemLineNo = parseInt(SQLineDetails[1]);
            }

        }
        
        $('#divImage').show();
        $.get(apiUrl + "GenerateCostSheet?SQNo=" + SalesQuoteNo + "&ItemLineNo=" + ItemLineNo, function (data) {

            var responseMsg = data;
            $('#divImage').hide();
            $('#modalCostSheetMsg').css('display', 'block');
            $('#dvCostSheetMsg').css('display', 'block');
            if (responseMsg.includes("Error : ")) {

                const errDetails = responseMsg.split(':');
                $('#resMsg').text(errDetails[1].trim());
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-2.png');
                $('#hfCostSheetFlag').val("false");
            }
            else {

                $('#hfItemNo').val(ItemNo);
                $('#hfItemName').val(ItemName);
                $('#resMsg').text(responseMsg);
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
                $('#hfCostSheetFlag').val("true");
            }

        });
    
    }

}

function showCostSheetDetails(ItemNo, ItemName) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    if ($('#hfSalesQuoteResDetails').val() == "") {

        var errMsg = "Fill cost sheet details after create sales quote";
        ShowErrMsg(errMsg);
    }
    else {

        const SQDetails = $('#hfSalesQuoteResDetails').val().split(',');
        var ItemLineNo;

        var SalesQuoteNo = SQDetails[0];

        for (var a = 0; a < SQDetails.length; a++) {

            if (SQDetails[a].includes(ItemNo)) {

                const SQLineDetails = SQDetails[a].split('_');
                ItemLineNo = parseInt(SQLineDetails[1]);
            }

        }

        var salesprice, totalunitprice, margin;
        
        $.get(apiUrl + "GetCostSheetDetails?SQNo=" + SalesQuoteNo + "&ItemLineNo=" + ItemLineNo, function (data) {

            var detailsOpt = "";
            $('#tblCostSheetDetails').empty();
            if (data.length == 0) {
                GenerateCostSheet(ItemNo, ItemName);
            }
            else {

                $('#modalCostSheet').css('display', 'block');
                $('#lblCostSheetSQNo').text(SalesQuoteNo);
                $('#lblCostSheetItemLineNo').text(ItemLineNo);
                $('#lblCostSheetItemName').text(ItemName);

                $.each(data, function (index, item) {

                    detailsOpt += "<tr><td hidden>" + item.TPTPL_Line_No + "</td><td>" + item.TPTPL_Item_Charge + "</td><td>" + item.TPTPL_Description + "</td><td id=\"" + item.TPTPL_Line_No + "_Qty\">" + item.TPTPL_Quantity + "</td>" +
                        "<td><input id=\"" + item.TPTPL_Line_No + "_CostUnitPrice\" onchange='calculateCostSheetDetails(\"" + item.TPTPL_Line_No + "_Qty\",\"" + item.TPTPL_Line_No + "_CostUnitPrice\",\"" + item.TPTPL_Line_No + "_Amount\",\"" + item.TPTPL_Line_No + "_Revenue\")' type='text' value=\"" + item.TPTPL_Rate_per_Unit + "\" class='form-control' /></td>" +
                        "<td id=\"" + item.TPTPL_Line_No + "_Amount\">" + item.TPTPL_Amount + "</td><td hidden id=\"" + item.TPTPL_Line_No + "_Revenue\">" + item.TPTPL_Revenue + "</td><td hidden><input type='checkbox' id=\"" + item.TPTPL_Line_No + "_PostToGL\" /></td>" +
                        "<td hidden><input type='checkbox' id=\"" + item.TPTPL_Line_No + "_Revenue\" /></td></tr >";

                    salesprice = item.SalesPrice;
                    totalunitprice = item.TotalUnitPrice;
                    margin = item.Margin;

                });

                $('#tblCostSheetDetails').append(detailsOpt);
                $('#lblCostSheetSalesPrice').text(salesprice);
                $('#lblCostSheetTotalUnitPrice').text(totalunitprice);
                $('#lblCostSheetMargin').text(margin);

            }

        });

    }

}

function calculateCostSheetDetails(Qty, UnitPrice, Amount, ItemRevenue) {

    var TotalUnitPrice = parseFloat($('#lblCostSheetTotalUnitPrice').text());
    if ($("#" + ItemRevenue).text() == true || $("#" + ItemRevenue).text() == "true") {

        TotalUnitPrice -= parseFloat($("#" + UnitPrice).val());
    }
    else if ($("#" + ItemRevenue).text() == false || $("#" + ItemRevenue).text() == "false") {

        TotalUnitPrice += parseFloat($("#" + UnitPrice).val());
    }

    $("#" + Amount).text($("#" + Qty).text() * $("#" + UnitPrice).val());
    $('#lblCostSheetTotalUnitPrice').text(parseFloat(TotalUnitPrice));
    $('#lblCostSheetMargin').text(parseFloat($('#lblCostSheetSalesPrice').text()) - parseFloat($('#lblCostSheetTotalUnitPrice').text()));
}

function CheckCostSheetDetails() {

    var errMsg = "";
    $('#tblCostSheetDetails tr').each(function () {

        var row = $(this)[0];
        var RatePerUnit = $("#" + row.cells[0].innerHTML + "_CostUnitPrice").val();
        if (RatePerUnit == "") {
            errMsg = "Please Fill Cost Per Unit In All Charge Item";
        }

    });
    return errMsg;
}

function GetCreditDaysForNotInInventory() {

    if ($('#ddlPackingStyle').val() != "-1") {
        const packingStyleDetails = $('#ddlPackingStyle').val().split('_');
        $('#hfPurchaseDays').val(parseInt(packingStyleDetails[2]));
    }
    
    return ($('#hfPaymentTermsDays').val() - $('#hfPurchaseDays').val());

}

function GetDetailsByCode(pincode) {

    pincode = jQuery("#hfPostCode").val();

    if (pincode != "") {

        $.ajax(
            {
                url: '/SPInquiry/GetAreasByPincodeForDDL?Pincode=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    if (data.length > 0) {

                        if ($('#hfAddNewDetails').val() == "BillToAddress") {

                            $('#ddlNewShiptoAddArea').empty();
                            $('#ddlNewShiptoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewShiptoAddArea");
                            });

                        }

                        if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

                            $('#ddlNewJobtoAddArea').empty();
                            $('#ddlNewJobtoAddArea').append($('<option value="-1">---Select---</option>'));

                            $.each(data, function (i, data) {
                                $('<option>',
                                    {
                                        value: data.Code,
                                        text: data.Text
                                    }
                                ).html(data.Text).appendTo("#ddlNewJobtoAddArea");
                            });

                        }

                    }
                    else {
                        $('#ddlNewShiptoAddArea, #ddlNewJobtoAddArea').empty();
                        BindArea();
                    }
                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );

    }
}

/*function GetDetailsByCode(pincode) {

    if ($('#hfAddNewDetails').val() == "BillToAddress") {

        $("#ddlNewShiptoAddCity").prop("disabled", false);
        $("#ddlNewShiptoAddCity").val("-1");

        $("#ddlNewShiptoAddDistrict").prop("disabled", false);
        $("#ddlNewShiptoAddDistrict").val("");

        $("#ddlNewShiptoAddState").prop("disabled", false);
        $("#ddlNewShiptoAddState").val("-1");

        $("#ddlNewShiptoAddCountryCode").prop("disabled", false);
        $("#ddlNewShiptoAddCountryCode").val("-1");

    }
    else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

        $("#ddlNewJobtoAddCity").prop("disabled", false);
        $("#ddlNewJobtoAddCity").val("-1");

        $("#ddlNewJobtoAddDistrict").prop("disabled", false);
        $("#ddlNewJobtoAddDistrict").val("");

        $("#ddlNewJobtoAddState").prop("disabled", false);
        $("#ddlNewJobtoAddState").val("-1");

        $("#ddlNewJobtoAddCountryCode").prop("disabled", false);
        $("#ddlNewJobtoAddCountryCode").val("-1");

    }

    if (pincode != "") {
        $.ajax(
            {
                url: '/SPSalesQuotes/GetDetailsByCode?Code=' + pincode,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    if (data.length > 0) {

                        $.ajax(
                            {
                                url: '/SPSalesQuotes/GetAreasByPincodeForDDL?Pincode=' + pincode,
                                type: 'GET',
                                contentType: 'application/json',
                                success: function (data) {

                                    if (data.length > 0) {

                                        if ($('#hfAddNewDetails').val() == "BillToAddress") {

                                            $('#ddlNewShiptoAddArea').empty();
                                            $('#ddlNewShiptoAddArea').append($('<option value="-1">---Select---</option>'));
                                            $.each(data, function (i, data) {
                                                $('<option>',
                                                    {
                                                        value: data.Code,
                                                        text: data.Text
                                                    }
                                                ).html(data.Text).appendTo("#ddlNewShiptoAddArea");
                                            });

                                        }
                                        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

                                            $('#ddlNewJobtoAddArea').empty();
                                            $('#ddlNewJobtoAddArea').append($('<option value="-1">---Select---</option>'));
                                            $.each(data, function (i, data) {
                                                $('<option>',
                                                    {
                                                        value: data.Code,
                                                        text: data.Text
                                                    }
                                                ).html(data.Text).appendTo("#ddlNewJobtoAddArea");
                                            });

                                        }

                                        //if ($('#hfAreaCode').val() != "") {
                                        //    $("#ddlNewShiptoAddArea").val($('#hfAreaCode').val());
                                        //}
                                    }
                                    else {

                                        if ($('#hfAddNewDetails').val() == "BillToAddress") {
                                            $('#ddlNewShiptoAddArea').empty();
                                        }
                                        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                                            $('#ddlNewJobtoAddArea').empty();
                                        }

                                        BindArea();
                                    }
                                },
                                error: function (data1) {
                                    alert(data1);
                                }
                            }
                        );

                        if ($('#hfAddNewDetails').val() == "BillToAddress") {

                            $("#ddlNewShiptoAddCity").val(data[0].Code);
                            $("#ddlNewShiptoAddCity").prop("disabled", true);
                            //$("#ddlCity").prop("disabled", true);

                            $("#ddlNewShiptoAddDistrict").val(data[0].District_Code);
                            $("#ddlNewShiptoAddDistrict").prop("disabled", true);
                            //$("#ddlDistrict").prop("disabled", true);

                            $("#ddlNewShiptoAddState").val(data[0].PCPL_State_Code);
                            $("#ddlNewShiptoAddState").prop("disabled", true);
                            //$("#ddlState").prop("disabled", true);

                            $("#ddlNewShiptoAddCountryCode").val(data[0].Country_Region_Code);
                            $("#ddlNewShiptoAddCountryCode").prop("disabled", true);
                            //$("#ddlCountry").prop("disabled", true);

                        }
                        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {

                            $("#ddlNewJobtoAddCity").val(data[0].Code);
                            $("#ddlNewJobtoAddCity").prop("disabled", true);
                            //$("#ddlCity").prop("disabled", true);

                            $("#ddlNewJobtoAddDistrict").val(data[0].District_Code);
                            $("#ddlNewJobtoAddDistrict").prop("disabled", true);
                            //$("#ddlDistrict").prop("disabled", true);

                            $("#ddlNewJobtoAddState").val(data[0].PCPL_State_Code);
                            $("#ddlNewJobtoAddState").prop("disabled", true);
                            //$("#ddlState").prop("disabled", true);

                            $("#ddlNewJobtoAddCountryCode").val(data[0].Country_Region_Code);
                            $("#ddlNewJobtoAddCountryCode").prop("disabled", true);
                            //$("#ddlCountry").prop("disabled", true);

                        }

                    }
                    else {

                        if ($('#hfAddNewDetails').val() == "BillToAddress") {
                            $('#ddlNewShiptoAddArea').empty();
                        }
                        else if ($('#hfAddNewDetails').val() == "DeliveryToAddress") {
                            $('#ddlNewJobtoAddArea').empty();
                        }

                        BindArea();
                    }

                },
                error: function (data1) {
                    alert(data1);
                }
            }
        );
    }
}*/

function ResetCPersonDetails() {

    $('#txtCPersonName').val("");
    $('#txtCPersonMobile').val("");
    $('#txtCPersonEmail').val("");
    $('#txtJobResponsibility').val("");
    BindDepartment();
    $('#chkAllowLogin').prop('checked', false);
    $('#chkEnableOTPOnLogin').prop('checked', false);

}

function ResetNewBillToAddressDetails() {

    $('#txtNewShiptoAddCode').val("");
    $('#txtNewShiptoAddress').val("");
    $('#txtNewShiptoAddress2').val("");
    $('#txtNewShiptoAddPostCode').val("");
    $('#txtNewShiptoAddGSTNo').val("");
    $('#txtNewShiptoAddCountryCode').val("");
    $('#txtNewShiptoAddState').val("");
    $('#txtNewShiptoAddDistrict').val("");
    $('#txtNewShiptoAddCity').val("");
    $('#ddlNewShiptoAddArea').val("-1");

}

function ResetNewDeliveryToAddressDetails() {

    $('#txtNewJobtoAddCode').val("");
    $('#txtNewJobtoAddress').val("");
    $('#txtNewJobtoAddress2').val("");
    $('#txtNewJobtoAddPostCode').val("");
    $('#txtNewJobtoAddGSTNo').val("");
    $('#txtNewJobtoAddCountryCode').val("");
    $('#txtNewJobtoAddState').val("");
    $('#txtNewJobtoAddDistrict').val("");
    $('#txtNewJobtoAddCity').val("");
    $('#ddlNewJobtoAddArea').val("-1");

}

function BindShortcloseReason() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetShortcloseReasons',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var opts = "<option value='-1'>---Select---</option>";

                $.each(data, function (index, item) {
                    opts += "<option value='" + item.Entry_No + "'>" + item.Short_Close_Reason + "</option>";
                });

                $('#ddlShortcloseReason').append(opts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function BindJustificationDetails(LoggedInUserRole, ContactCompanyNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesQuoteJustificationDetails?LoggedInUserRole=' + LoggedInUserRole + '&CCompanyNo=' + ContactCompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var TROpts = "";

                $.each(data, function (index, item) {
                    TROpts += "<tr><td>" + item.PCPL_Target_Date + "</td><td>" + item.No + "</td><td>" + item.WorkDescription + "</td></tr>";
                });

                $('#tblSQJustificationDetails').append(TROpts);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function GetAndFillCompanyIndustry(ContactCompanyNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetCompanyIndustry?CCompanyNo=' + ContactCompanyNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {
                    $('#tdTraderMfg').text(data);
                }

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function ApproveRejectSQ(Action, RejectRemarks) {

    if (RejectRemarks == null || RejectRemarks == "") {
        $('#btnApproveSpinner').show();
    }
    else {
        $('#btnRejectSpinner').show();
    }

    let SQNos = new Array();
    var a = 0;
    var str = $('#hfSalesQuoteNo').val() + ":" + $('#hdnApprovalFor').val() + ":" + $('#hdnSalespersonEmail').val() + ",";
    SQNosAndApprovalFor_ = str;

    var UserRoleORReportingPerson = "";

    if ($('#hdnLoggedInUserRole').val() == "Finance") {
        UserRoleORReportingPerson = "Finance";
    }
    else {
        UserRoleORReportingPerson = "ReportingPerson";
    }

    $.post(apiUrl + "SQApproveReject?SQNosAndApprovalFor=" + SQNosAndApprovalFor_ + "&LoggedInUserNo=" + $('#hdnLoggedInUserNo').val() + "&Action=" + Action + "&UserRoleORReportingPerson=" + UserRoleORReportingPerson + "&RejectRemarks=" + RejectRemarks + "&LoggedInUserEmail=" + $('#hdnLoggedInUserEmail').val(), function (data) {

        var resMsg = data;

        if (RejectRemarks == null || RejectRemarks == "") {
            $('#btnApproveSpinner').hide();
        }
        else {
            $('#btnRejectSpinner').hide();
        }

        if (resMsg == "True") {

            $('#modalApproveRejectMsg').css('display', 'block');
            if (Action == "Approve") {
                $('#lblApproveRejectMsg').text("Sales Quote Approved Successfully");
            }
            else if (Action == "Reject") {
                $('#modalRejectRemarks').css('display', 'none');
                $('#lblApproveRejectMsg').text("Sales Quote Rejected Successfully");
            }

        }
        else if (resMsg.includes("Error:")) {
            const resMsgDetails = resMsg.split(':');

            $('#modalErrMsg').css('display', 'block');
            $('#modalErrDetails').text(resMsgDetails[1]);
        }

    });

}

function ShowActionMsg(actionMsg) {

    Lobibox.notify('success', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        icon: 'bx bx-check-circle',
        delayIndicator: false,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: actionMsg
    });
}

function ShowErrMsg(errMsg) {

    Lobibox.notify('error', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        delayIndicator: false,
        icon: 'bx bx-x-circle',
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: errMsg
    });

}