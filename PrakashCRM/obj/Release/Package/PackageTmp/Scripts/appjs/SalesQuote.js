var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
$(document).ready(function () {

    initSQPriceColumnIndexes();

    $('#btnReopenSQ').click(function () {
        // Button is kept as requested; reopen logic removed.
        ShowErrMsg('Reopen is disabled on this page.');
    });

    GetInterestRate();
    //BindCSOutstandingDuelist();
    var UrlVars = getUrlVars();

    if (UrlVars["CompanyNo"] != undefined && UrlVars["CompanyName"] != undefined) {

        var companyName = UrlVars["CompanyName"];
        companyName = companyName.replaceAll("%20", " ");
        $('#hfCustomerName').val(companyName);
        $('#hfContactCompanyNo').val(UrlVars["CompanyNo"]);
    }

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

    var curDate = GetCurrentDate();

    $('#txtSalesQuoteDate').val(curDate);
    $('#txtSQValidUntillDate').val(curDate);

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

    $('#txtCustomerName').change(function () {
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    });


    $('#txtProductName').change(function () {
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
        CalculateFormula();

    });

    $('#txtSalesDiscount').change(function () {

        CalculateFormula();

    });

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

    });

    $('#ddlCommissionPayable').change(function () {

        if ($('#ddlCommissionPayable').val() == "-1") {
            $('#txtCommissionAmt').val("0");
        }

    });

    $('#txtTransportCost').change(function () {

        CalculateFormula();


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

        CalculateFormula();

    });

    $('#ddlPaymentTerms').change(function () {

        PaymentTermsChange();
        $('#txtLineDetailsPaymentTerms').val($('#ddlPaymentTerms option:selected').text());
        $('#txtLineDetailsPaymentTerms').attr('readonly', true);

    });

    $('#txtInsurance').change(function () {

        $('#txtAdditionalQty').change();

    });

    $('#txtSalesPrice').on('input blur change', function () {

        // If Sales Price is being changed by the user, allow margin to be recalculated.
        try {
            SQ_FORM_LOCKS.Margin = false;
            SQ_FORM_LOCKS.MarginValue = null;
        } catch (e) { }

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
        $('#txtAdditionalQty').change();

    });

    $('#chkDropShipment').change(function () {

        var isChecked = $(this).is(":checked");
        if (isChecked == true) {
            if ($('#hfProdNo').val() != '') {

                BindItemVendors($('#hfProdNo').val());
                $('#dvVendors').css('display', 'block');
                $('#ddlItemVendors').css('display', 'block');
            } else {
                $('#chkDropShipment').prop('checked', false);
            }
        }
        else {
            $('#ddlItemVendors').val('');
            $('#dvVendors').css('display', 'none');
        }

    });

    $('#ddlPackingStyle').change(function () {
        const packingStyleDetails = $('#ddlPackingStyle').val().split('_');
        // Set Basic Purchase Cost to PCPL_MRP (use packing style Purchase Cost, not MRP Price)
        $('#txtBasicPurchaseCost').val(parseFloat(packingStyleDetails[0]).toFixed(2));
        $('#txtBasicPurchaseCost').prop('disabled', true);
        $('#txtMRPPrice').val(parseFloat(packingStyleDetails[3]).toFixed(4));
        $('#txtMRPPrice').prop('disabled', true);
        $('#hfPurchaseDays').val(parseInt(packingStyleDetails[2]));

        // Packing style drives Basic Purchase Cost; recalc totals/margin immediately.
        try { CalculateFormula(); } catch (e) { }
    });

    $('#btnResetProdDetails').click(function () {

        ResetQuoteLineDetails();

    });
    // Added the DataTable initialization
    var dtable;
    dtable = $('#dataList').DataTable({
        searching: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
        columnDefs: [{ className: 'dtr-control', targets: 0 }],
        initComplete: function (settings, json) {
            $('#dataList td.dataTables_empty').remove(); // row hata diya
        }
    });
    $('#btnSaveProd').click(function () {



        var isLiquidProd = $('#chkIsLiquidProd').prop('checked');

        var isCommission = $('#chkIsCommission').prop('checked');

        // Validations

        if ($('#txtProductName').val() === "" || $('#txtProductName').val() === null) {

            ShowErrMsg("Please select a Product Name before saving.");

            $('#txtProductName').focus();

            return;

        }

        if ($('#ddlPackingStyle').val() === "" || $('#ddlPackingStyle').val() === null || $('#ddlPackingStyle').val() === "-1") {

            ShowErrMsg("Please select a Packing Style before saving.");

            $('#ddlPackingStyle').focus();

            return;

        }

        if ($('#txtProdQty').val() === "" || $('#txtProdQty').val() === null || $('#txtProdQty').val() === "0") {

            ShowErrMsg("Please select a Qty before saving.");

            $('#txtProdQty').focus();

            return;

        }

        if ($('#txtBasicPurchaseCost').val() === "" || $('#txtBasicPurchaseCost').val() === null) {

            ShowErrMsg("Please select a Basic Purchase Cost before saving.");

            $('#txtBasicPurchaseCost').focus();

            return;

        } if ($('#txtMRPPrice').val() === "" || $('#txtMRPPrice').val() === null) {

            ShowErrMsg("Please select a MRP Price before saving.");

            $('#txtMRPPrice').focus();

            return;

        }
        // Enhanced validation for Inco Terms

        if ($('#ddlIncoTerms').val() === "" || $('#ddlIncoTerms').val() === null || $('#ddlIncoTerms').val() === "-1") {

            ShowErrMsg("Please select Inco Terms before saving.");

            $('#ddlIncoTerms').focus();

            console.log("Inco Terms validation failed: ", $('#ddlIncoTerms').val());

            return;

        }

        // Enhanced validation for Payment Terms

        if ($('#ddlPaymentTerms').val() === "" || $('#ddlPaymentTerms').val() === null || $('#ddlPaymentTerms').val() === "-1") {

            ShowErrMsg("Please select Payment Terms before saving.");

            $('#ddlPaymentTerms').focus();

            console.log("Payment Terms validation failed: ", $('#ddlPaymentTerms').val());

            return;

        }

        if ($('#txtSalesPrice').val() === "" || $('#txtSalesPrice').val() === null) {

            ShowErrMsg("Please select a Sales Price before saving.");

            $('#txtSalesPrice').focus();

            return;

        }

        if ($('#txtDeliveryDate').val() === "" || $('#txtDeliveryDate').val() === null) {

            ShowErrMsg("Please select a Delivery Date before saving.");

            $('#txtDeliveryDate').focus();

            return;

        }
        var prodOpts = $('#hfProdNo').val();

        var prodOptsTR = $('#hfProdNoEdit').val();

        var dropShipmentOpt = $('#chkDropShipment').is(':checked') ? 'Yes' : 'No';

        var inqProdLineNo = $('#hfProdLineNo').val() || '';
        var sqLineNo = 0;
        if (prodOptsTR) {
            try {
                var $existingRowForLine = $('#ProdTR_' + prodOptsTR);
                sqLineNo = parseInt((($existingRowForLine.attr('data-lineno') || '')).toString().trim()) || 0;
            } catch (e) { sqLineNo = 0; }
        }
        if (!sqLineNo || sqLineNo <= 0) {
            sqLineNo = parseInt($('#hfSQProdLineNo').val() || '0') || 0;
        }

        var editLineNoForActions = (sqLineNo && sqLineNo > 0) ? sqLineNo : (parseInt(inqProdLineNo || '0') || 0);

        var actionsHtml = `<a class='SQLineCls' onclick='EditSQProd(${editLineNoForActions},"ProdTR_${prodOpts}")'><i class='bx bxs-edit'></i></a>`;
        actionsHtml += `&nbsp;<a class='SQLineCls' onclick='DeleteSQProd(${editLineNoForActions},"ProdTR_${prodOpts}")'><i class='bx bxs-trash'></i></a>`;
        actionsHtml += `&nbsp;<span id='${prodOpts}_SQPriceBtns'></span>`;

        var commissionPerUnit = isCommission ? $('#ddlCommissionPerUnitPercent').val() : "";

        var commissionPercent = isCommission ? $('#txtCommissionPercent').val() : "";

        var commissionAmt = isCommission ? $('#txtCommissionAmt').val() : "";

        var commissionPayable = isCommission ? $('#ddlCommissionPayable').val() : "";

        var Concentrate = isLiquidProd ? $('#txtConcentratePercent').val() : "";

        var NetWeigth = isLiquidProd ? $('#txtNetWeight').val() : "";

        var LiquidRate = isLiquidProd ? $('#txtLiquidRate').val() : "";

        // Preserve price-approval fields when editing an existing row
        initSQPriceColumnIndexes();
        var existingNewPrice = '0';
        var existingNewMargin = '0';
        var existingPriceUpdated = 'false';
        if (prodOptsTR) {
            var $existingRow = $('#ProdTR_' + prodOptsTR);
            if ($existingRow.length && SQ_PRICE_COLS.NEW_PRICE !== null && SQ_PRICE_COLS.NEW_PRICE >= 0) {
                existingNewPrice = (sqGetRowCellText($existingRow, SQ_PRICE_COLS.NEW_PRICE) || '').trim() || '0';
            }
            if ($existingRow.length && SQ_PRICE_COLS.NEW_MARGIN !== null && SQ_PRICE_COLS.NEW_MARGIN >= 0) {
                existingNewMargin = (sqGetRowCellText($existingRow, SQ_PRICE_COLS.NEW_MARGIN) || '').trim() || '0';
            }
            if ($existingRow.length && SQ_PRICE_COLS.PRICE_UPDATED !== null && SQ_PRICE_COLS.PRICE_UPDATED >= 0) {
                existingPriceUpdated = (sqGetRowCellText($existingRow, SQ_PRICE_COLS.PRICE_UPDATED) || '').trim() || 'false';
            }
        }
        var sqLineNoLabel = `<label id="${prodOpts}_SQLineNo" style='display:none'>${sqLineNo || 0}</label>`;

        var prodOptsArray = [

            '', actionsHtml,
            prodOpts,
            $('#txtProductName').val(),
            $('#txtProdQty').val(),
            $('#txtUOM').val(),
            $('#ddlPackingStyle option:selected').text(),
            $('#txtBasicPurchaseCost').val(),
            $('#txtSalesPrice').val(),

            // New fields (defaults on create/edit)
            (prodOptsTR ? existingNewPrice : '0'), // New Price
            (prodOptsTR ? existingNewMargin : '0'), // New Margin
            (prodOptsTR ? existingPriceUpdated : 'false'), // Price Updated

            $('#txtDeliveryDate').val(),

            $('#txtTotalCost').val(),

            $('#txtMargin').val(),

            $('#ddlPaymentTerms').val(),

            $('#ddlIncoTerms').val(),

            $('#ddlTransportMethod').val(),

            $('#txtTransportCost').val(),

            $('#txtSalesDiscount').val(),

            commissionPerUnit,

            commissionPercent,

            commissionAmt,

            $('#txtCreditDays').val(),

            $('#txtInterest').val(),

            `<label id="${prodOpts}_DropShipment">${dropShipmentOpt}</label>`,

            "", // 26 → hidden
            "", // 27 → hidden
            `<label id="${prodOpts}_MarginPercent">${$('#spnMarginPercent').text()}</label>`,
            commissionPayable,
            `<label id="${prodOpts}_InqProdLineNo" style="display:none;">${inqProdLineNo}</label>`,
            // `<label id="${prodOpts}_SQLineNo" style="display:none;">${inqProdLineNo}</label>`,
            //`<label id="${prodOpts}_SQLineNo" style='display:none'>${prodOpts}</label>`,
            isLiquidProd,
            Concentrate,
            NetWeigth,
            LiquidRate,
            (dropShipmentOpt === 'Yes' ? ($('#ddlItemVendors').val() || "").split('_')[0] : ""),

            $('#txtMRPPrice').val(),
            sqLineNoLabel
        ];
        // Ensure column count matches header count

        var colCount = $('#dataList thead th').length;

        if (prodOptsArray.length < colCount) {

            while (prodOptsArray.length < colCount) prodOptsArray.push('');

        } else if (prodOptsArray.length > colCount) {

            prodOptsArray = prodOptsArray.slice(0, colCount);

        }

        if (prodOptsTR) {

            var rowIdSelector = '#ProdTR_' + prodOptsTR;

            var rowApi = dtable.row(rowIdSelector);

            if (rowApi.any()) {

                rowApi.data(prodOptsArray).draw(false);

                $(rowApi.node()).attr('id', 'ProdTR_' + prodOpts);
                $(rowApi.node()).attr('data-lineno', sqLineNo || 0);

                $(rowApi.node()).find("td").eq(0).addClass("dtr-control");

            } else {

                var newNode = dtable.row.add(prodOptsArray).draw(false).node();

                $(newNode).attr('id', 'ProdTR_' + prodOpts);
                $(newNode).attr('data-lineno', sqLineNo || 0);

                $(newNode).find("td").eq(0).addClass("dtr-control");

            }

            $('#hfProdNoEdit').val('');

        } else {

            var newNode = dtable.row.add(prodOptsArray).draw(false).node();

            $(newNode).attr('id', 'ProdTR_' + prodOpts);

            $(newNode).find("td").eq(0).addClass("dtr-control");

            $('#dataList').show();

        }

        // Store SQ line no (for edits) in hidden col 26 and original basic cost in hidden col 27
        var lineNo = $('#hfSQProdLineNo').val() || '';
        var $row = $('#ProdTR_' + prodOpts);
        if ($row.length) {
            $row.attr('data-lineno', lineNo);
            $row.find('TD').eq(26).html(`<label id="${prodOpts}_SQLineNo" style="display:none;">${lineNo}</label>`);
            $row.find('TD').eq(27).text('');
            applySQPriceRowState('ProdTR_' + prodOpts);
        }

        dtable.responsive.recalc();

        // Update available credit limit

        $('#txtAvailableCreditLimit').prop('disabled', false);

        var availableCreditLimit = parseFloat($('#txtAvailableCreditLimit').val().replaceAll(",", "")) -

            (parseFloat($('#txtSalesPrice').val()) * parseFloat($('#txtProdQty').val()));

        $('#txtAvailableCreditLimit').val(commaSeparateNumber(availableCreditLimit.toFixed(2)));

        $('#txtAvailableCreditLimit').prop('disabled', true);

        dataTableFunction();

        ResetQuoteLineDetails();

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


        }


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
        $('#txtProdQty').prop('readonly', false);

    });

    $('#txtNetWeight').blur(function () {

        $('#txtConcentratePercent').blur();
    });

    $('#txtLiquidRate').blur(function () {

        // Sales Price is derived for liquid products; trigger formula recalculation
        // because programmatic .val() does not fire input/change events by itself.
        $('#txtSalesPrice').val(((parseFloat($('#txtNetWeight').val()) * parseFloat($('#txtLiquidRate').val())) / parseFloat($('#txtProdQty').val())).toFixed(2));
        try {
            $('#txtSalesPrice').trigger('input');
        } catch (e) {
            try { CalculateFormula(); } catch (e2) { }
        }

    });

    $('#chkShowAllProducts').change(function () {

        productName_autocomplete();

    });

    $('#btnAddNewContactPerson').click(function () {

        let customer = $('#txtCustomerName').val();
        let title = "Add New Contact Person In Selected Company";
        if (customer != "") {
            title = title + " - " + "<b>" + customer + "</b>";
        }
        $('.modal-title').html(title);
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

            // Ensure Margin/TotalCost reflects the reset Sales Price
            try { $('#txtSalesPrice').trigger('input'); } catch (e) { try { CalculateFormula(); } catch (e2) { } }

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
            $('#txtMRPPrice').val("0.00");

            // Recompute after resetting liquid fields
            try { $('#txtSalesPrice').trigger('input'); } catch (e) { try { CalculateFormula(); } catch (e2) { } }
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

    // implement the Transpoter RateCard Method Function.
    $('#btnShowTPDet').on('click', function () {
        var packingUOMs = $('#hfPackingUnit').val();
        var fromToPincode = "";
        var jobToPincode = "";

        var DeliveryTopincode = $("#ddlDeliveryTo").val();
        var DropshipmentAdd = $("#hfDropshipment").val();
        if (DropshipmentAdd == "true") {
            fromToPincode = $("#hfFromPincode2").val();
        }
        else {
            fromToPincode = $("#hfFromPincode1").val()
        }
        if (DeliveryTopincode == '-1') {
            jobToPincode = $('#hfToPincode1').val();
        }
        else {
            jobToPincode = $('#hfToPincode2').val();
        }
        if (!packingUOMs || !fromToPincode || !jobToPincode) {
            $('#tbltansportermethod').empty()
                .append("<tr><td colspan='7' class='text-center text-danger fw-bold'>No Records Found</td></tr>");
            $('#dvTransportDet').show();
            return;
        }

        $.ajax({
            url: '/SPSalesQuotes/GetTransporterMethod',
            type: 'GET',
            contentType: 'application/json',
            data: {
                PackingUOMs: packingUOMs,
                FromToPincode: fromToPincode,
                JobToPincode: jobToPincode
            },
            success: function (data) {
                $('#tbltansportermethod').empty();

                var rows = "";
                if (!data || data.length === 0) {
                    rows = "<tr><td colspan='7' class='text-center'>No Records Found</td></tr>";
                } else {
                    $.each(data, function (i, item) {
                        rows += `
						<tr><td>${item.Rate_Effective_Date}</td><td>${item.UOM}</td><td>${item.Standard_Weight}</td><td>${item.Rate_for_Standard_Weight}</td><td>${item.Rate_above_Standard_Weight}</td><td>${item.Vehicle_Type}</td><td>${item.Transporter_Type}</td>
						</tr>`;
                    });
                }

                $('#tbltansportermethod').append(rows);
                $('#dvTransportDet').show();
            },
        });
    });

    $("#ddlLocations").on('change', function () {
        var selected = $("#ddlLocations option:selected").val();
        var parts = selected.split('_');
        var pin = parts.length > 0 ? parts[1].trim() : "";
        $("#hfFromPincode1").val(pin);
    });

    $("#hfDropshipment").val("false");
    $("#chkDropShipment").change(function () {
        $("#hfDropshipment").val($(this).is(":checked") ? "true" : "false");
    });
    $('#ddlItemVendors').on('change', function () {
        var selected = $("#ddlItemVendors option:selected").val();
        var pin;
        if ($("#ddlItemVendors").val() == '') {
            $("#ddlItemVendors").val('---Select---');
            pin = '';
        }
        else {
            var part = selected.split('_');
            pin = part[1].trim();

        }
        $("#hfFromPincode2").val(pin);

    });

    $('#ddlBillTo').on('change', function () {
        var selectedText = $("#ddlBillTo option:selected").text() || "";
        var parts = selectedText.split('-');
        var pincode = parts.length > 1 ? parts[1].trim() : "";
        $('#hfToPincode1').val(pincode);
    });
    $('#ddlDeliveryTo').on('change', function () {
        var selectedText = $("#ddlDeliveryTo option:selected").text() || "";
        var parts = selectedText.split('-');
        var pincode = parts.length > 1 ? parts[1].trim() : "";
        $('#hfToPincode2').val(pincode);
    });

    $('#ddlPackingStyle').on('change', function () {
        var selectedText = $("#ddlPackingStyle option:selected").text() || "";
        var selectedValue = $("#ddlPackingStyle option:selected").val();
        let packingType = "";
        if (selectedText.includes("KGS")) {
            let parts = selectedText.split("KGS");
            if (parts.length > 1) {
                packingType = parts[1].trim().split('_')[0];
            }
        } else {
            packingType = selectedText.trim().split('_')[0];
        }
        $('#hfPackingUnit').val(packingType);
    });

    $('#ddlBillTo').trigger('change');
    $('#ddlDeliveryTo').trigger('change');
    //$('#ddlPackingStyle').trigger('');

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
        GetContactsOfCompany(data.Contact_Company_Name);
        GetCreditLimitAndCustDetails(data.Contact_Company_Name);
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
                item.Quantity + ",\"" + item.PCPL_Packing_Style_Code + "\",\"" + item.Delivery_Date + "\")'><i class='bx bx-edit'></i></a></td>" +
                "<td hidden><label id='InqProdLineNo_" + item.Product_No + "'>" + item.Line_No + "</label></td></tr>";
        });

        $('#tblInqProdDetails').append(ProdTR);
    });
}

function FillInqProdDetails(ProdLineNo, ProductNo, ProductName, Quantity, PackingStyleCode, DeliveryDate) {
    /* ResetQuoteLineDetails();*/
    $('#hfProdLineNo').val(ProdLineNo); // Store InqProdLineNo
    GetProductDetails(ProductName)
    $('#hfProdNo').val(ProductNo);
    $('#hfInqProdPackingStyle').val(PackingStyleCode);
    $('#txtProductName').val(ProductName);
    $('#txtProductName').blur();
    $('#txtProdQty').val(Quantity);
    $('#txtDeliveryDate').val(DeliveryDate);
    $('#ddlPaymentTerms').change();
}

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
                    //locationsOpts += "<option value='" + item.Code + "'>" + item.Name + "</option>";
                    locationsOpts += "<option value='" + item.Code + "_" + item.Post_Code + "' >" + item.Name + "</option>";

                });

                $('#ddlLocations').append(locationsOpts);

                $('#ddlLocations').val('-1');

                if ($('#hfSavedLocationCode').val() != "" && $('#hfSavedLocationCode').val() != null) {
                    $('#ddlLocations').val($('#hfSavedLocationCode').val());
                }

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
            cache: false,
            success: function (data) {

                $('#ddlContactName').empty();
                $('#ddlContactName').append('<option value="-1">---Select---</option>');

                if (data.length > 0) {

                    var primaryContactNo = "";
                    $.each(data, function (i, data) {
                        if (data.Is_Primary) {
                            primaryContactNo = data.No;
                        }
                        $('<option>', {
                            value: data.No,
                            text: data.Name
                        }).appendTo("#ddlContactName");

                    });

                    $("#ddlContactName").val(primaryContactNo);
                    $('#btnSaveProd').css('display', 'block');
                    $('#btnSave').css('display', 'block');

                    if ($('#hfSavedContactPersonNo').val() != "") {

                        $("#ddlContactName").val($('#hfSavedContactPersonNo').val());

                    }
                }
                else {
                    $('#ddlContactName').empty();
                    $('#ddlContactName').append($('<option value="-1">---Select---</option>'));

                    $('#btnSaveProd').css('display', 'none');
                    $('#btnSave').css('display', 'none');
                }
            }
        }
    );

}

function companyName_autocomplete() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete company name');

    if ($("#txtCustomerName").data("autocomplete")) {
        $('#txtCustomerName').autocomplete('dispose');
    }

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {

        if (data != null && data.length > 0) {

            let company = {};
            for (let i = 0; i < data.length; i++) {
                company[data[i].No] = data[i].Name;
            }

            var companiesArray = $.map(company, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            var cuscurrentValue = '';
            $('#txtCustomerName').autocomplete({
                lookup: companiesArray,
                onSelect: function (selecteditem) {
                    if (selecteditem.value != cuscurrentValue) {
                        cuscurrentValue = selecteditem.value
                        $('#hfContactCompanyNo').val(selecteditem.data);
                        $('#btnAddNewContactPerson').prop('disabled', false);
                        GetContactsOfCompany(selecteditem.value);
                        $('#txtCustomerName').trigger('change');
                    }
                },
            });
        }
    });

    $('#ddlContactName').empty();
    $('#ddlContactName').append("<option value='-1'>---Select---</option>");
    $('#ddlContactName').val('-1');
    if ($('#hfSavedCustomerName').val()) {

        $('#txtCustomerName').val($('#hfSavedCustomerName').val());
        $('#txtCustomerName').blur();
    }
    else if ($('#hfCustomerName').val()) {

        $('#txtCustomerName').val($('#hfCustomerName').val());
        GetContactsOfCompany($('#txtCustomerName').val());
        GetCreditLimitAndCustDetails($('#txtCustomerName').val());
        productName_autocomplete();
    }
}


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
            // 👉 IsActive check — disable / enable dropdown
            if (suggestion.isActive === false) {
                $("#ddlNewShiptoAddArea").prop("disabled", true);
            } else {
                $("#ddlNewShiptoAddArea").prop("disabled", false);
            }
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

                    $('#txtTotalCreditLimit').val(data.CreditLimit).prop('disabled', true);
                    $('#txtAvailableCreditLimit').val(data.AvailableCredit).prop('disabled', true);
                    $('#txtOutstandingDue').val(data.OutstandingDue).prop('disabled', true);
                    $('#hfUsedCreditLimit').val(data.UsedCreditLimit);
                    $('#hdnCustBalanceLCY').val(data.AccountBalance);
                    $('#tdClassCustomer').text(data.PcplClass); // Set the text content of tdClassCustomer
                    $('#tdClassCustomer').val(data.PcplClass);
                    $('#tdAvgDelayDays').text(data.AverageDelayDays); // Set the text content of td AverageDelayDays
                    $('#tdAvgDelayDays').val(data.AverageDelayDays);

                    if (SQFor == "ApproveReject") {
                        $('#tdOverdue').text($('#txtOutstandingDue').val());
                        $('#tdTotalExpo').text($('#hdnCustBalanceLCY').val());
                    }

                    $('#ddlConsigneeAddress option').remove();
                    var consigneeAddOpts = "<option value='-1'>---Select---</option>";
                    var FromPincode = "";
                    var JobToPincode = "";
                    if ((data.Address != "" && data.Address != null) || (data.City != "" && data.City != null) ||
                        (data.Post_Code != "" && data.Post_Code != null)) {

                        var consigneeAddText = "", consigneeAddValue = "";


                        if (data.CustNo != null) {
                            $('#hfCustomerNo').val(data.CustNo);

                            let custText = data.CustName + "," + data.Address + " ," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            consigneeAddText = custText.split(',').slice(1).join(',').trim();

                            let custValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            consigneeAddValue = custValue.split('_').slice(1).join('_').trim();

                        }
                        else {
                            let custText = data.CompanyName + "," + data.Address + "," + data.Address_2 + " " + data.City + "-" + data.Post_Code;
                            consigneeAddText = custText.split(',').slice(1).join(',').trim();

                            let custValue = data.CustName + "_" + data.Address + "_" + data.Address_2 + "_" + data.City + "_" + data.Post_Code;
                            consigneeAddValue = custValue.split('_').slice(1).join('_').trim();
                        }

                        consigneeAddOpts += "<option value=\"" + consigneeAddValue + "\">" + consigneeAddText + "</option>";
                        //DelliveryToPincode = data.Post_Code;

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
                        var billtoCode = "";
                        var billtoAdd = "";


                        if (data.ShiptoAddress != null) {
                            for (var i = 0; i < data.ShiptoAddress.length; i++) {
                                //billtoAdd = data.ShiptoAddress[i].Address;
                                billtoAddOpts += "<option value=\"" + data.ShiptoAddress[i].Code + "\">" + data.ShiptoAddress[i].Address + "</option>";
                                FromPincode = data.ShiptoAddress[i].Address;
                            }
                            billtoCode = data.ShiptoAddress[0].Code;
                            $('#ddlBillTo').append(billtoAddOpts);
                            $('#ddlBillTo').val(billtoCode);
                        }
                        else {
                            $('#ddlBillTo').append("<option value='-1'>---Select---</option>");
                        }
                        let shiptopincode = FromPincode.split(',');
                        let pincode = shiptopincode[3]?.split('-')[1]?.trim() || '';
                        $("#hfFromPincode").val(pincode);

                        $('#btnAddNewBillTo').prop('disabled', false);

                        if ($('#hfShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfShiptoCode').val());
                        }
                        else if ($('#hfSavedShiptoCode').val() != "") {
                            $('#ddlBillTo').val($('#hfSavedShiptoCode').val());
                        }
                        //else {
                        //    $('#ddlBillTo').append("<option value='-1'>---Select---</option>");
                        //}

                        //$('#ddlBillTo').attr('disabled', true);

                        $('#ddlDeliveryTo option').remove();
                        var shiptoAdd = "<option value='-1'>---Select---</option>";

                        if (data.JobtoAddress != null) {
                            for (var i = 0; i < data.JobtoAddress.length; i++) {
                                shiptoAdd = shiptoAdd + "<option value=\"" + data.JobtoAddress[i].Code + "\">" + data.JobtoAddress[i].Address + "</option>";
                                JobToPincode = data.JobtoAddress[i].Address;
                            }
                        }

                        $('#ddlDeliveryTo').append(shiptoAdd);
                        $('#btnAddNewJobTo').prop('disabled', false);
                        //let jobToPincode = JobToPincode.split('-')[1]?.trim() || '';

                        //$("#hfToPincode").val(jobToPincode);

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
                        //$('#ddlBillTo').attr('disabled', true);

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

            let products = {}; // Create empty object

            for (let i = 0; i < data.length; i++) {
                if ($('#chkShowAllProducts').prop('checked')) {
                    products[data[i].No] = data[i].Description.trim();
                } else {
                    products[data[i].Item_No] = data[i].Item_Name.trim();
                }
            }

            // Convert to array for autocomplete
            var productsArray = $.map(products, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            var currentValue = '';
            $('#txtProductName').autocomplete({
                lookup: productsArray,
                onSelect: function (selecteditem) {
                    if (selecteditem.value != currentValue) {
                        currentValue = selecteditem.value
                        $('#hfProdNo').val(selecteditem.data);
                        $('#txtProductName').trigger('change');
                    }
                }
            });
        }
    });
}

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
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function BindItemVendors(ProdNo) {

    $.ajax({
        url: '/SPSalesQuotes/GetItemVendorsForDDL?ProdNo=' + ProdNo,
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {

            $('#ddlItemVendors').empty();
            var itemVendorsOpts = "<option value=''>---Select---</option>";

            var savedVendor = $('#hfSavedItemVendorNo').val();
            var selectedVal = "";

            $.each(data, function (index, item) {

                var optionValue = item.Vendor_No + "_" + item.PCPL_Vendor_Post_Code;

                itemVendorsOpts += "<option value='" + optionValue + "'>" + item.Vendor_Name + "</option>";

                // hidden vendor code match
                if (savedVendor == item.Vendor_No) {
                    selectedVal = optionValue;
                }
            });

            $('#ddlItemVendors').append(itemVendorsOpts);

            if (selectedVal != "") {
                $('#ddlItemVendors').val(selectedVal);
            }

        },
        error: function () {
        }
    });
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

function GetSalesQuoteDetailsAndFill(SalesQuoteNo, ScheduleStatus, SQStatus, SQFor, LoggedInUserRole) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    //$.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + $('#hfSalesQuoteNo').val(), function (data) {
    $.get(apiUrl + "GetSalesQuoteFromNo?SQNo=" + SalesQuoteNo, function (data) {

        if (data.No != "" || data.No != null) {

            // Keep latest status available globally (used by Price Update action).
            window.SQStatus = data.Status || window.SQStatus;
            window.ScheduleStatus = ScheduleStatus || window.ScheduleStatus;

            $('#hfSQEdit').val("true");
            $('#hfSalesQuoteNo').val(SalesQuoteNo);
            //$('#ddlNoSeries').prop('disabled', true);
            $('#hfInqNo').val(data.InquiryNo);
            $('#txtInqNo').val(data.InquiryNo);
            $('#hfSavedLocationCode').val(data.LocationCode + '_' + data.PCPL_Location_Post_Code);
            BindLocations();
            //$('#ddlLocations').val(data.LocationCode);
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
            $('#txtAvailableCreditLimit').val(data.AvailableCreditLimit);
            $('#txtTotalCreditLimit').val(data.TotalCreditLimit);
            $('#txtOutstandingDue').val(data.OutstandingDue);
            BindPaymentTerms();
            //$('#ddlPaymentTerms').val(data.PaymentTermsCode);
            $('#hfSavedIncoTerms').val(data.ShipmentMethodCode);
            $('#ddlIncoTerms').val(data.ShipmentMethodCode);
            companyName_autocomplete();
            GetContactsOfCompany(data.ContactCompanyName);
            GetCreditLimitAndCustDetails(data.ContactCompanyName);
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
            }

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
                $('#tdReopenSQ').show();
            }
            else {
                $('#btnSaveProd').prop('disabled', false);
                $('#btnSave').prop('disabled', false);
                $('#tdReopenSQ').hide();
            }

            $('#dataList').css('display', 'block');
            var itemLineNo = "";
            var dtable = $('#dataList').DataTable({
                retrieve: true,
                searching: false,
                paging: false,
                info: false,
                responsive: true,
                ordering: false,
                columnDefs: [

                    { className: 'dtr-control', targets: 0 }
                ]
            });

            $.each(data.ProductsRes, function (index, item) {
                var actionsHtml = "";
                if (item.TPTPL_Short_Closed) {
                    actionsHtml = "<span class='badge bg-secondary'>Shortclosed</span>";
                } else {
                    actionsHtml = `<a class='SQLineCls' onclick='EditSQProd(${item.Line_No},"ProdTR_${item.No}")'><i class='bx bxs-edit'></i></a>`;
                    if (!(ScheduleStatus === "Completed" || data.ShortcloseStatus === true)) {
                        actionsHtml += `&nbsp;<a class='SQLineCls' title='Click to shortclose' onclick='ShortcloseSQProd("${item.Line_No}")'><i class='bx bx-message-rounded-x'></i></a>`;
                    }
                    actionsHtml += `&nbsp;<span id='${item.No}_SQPriceBtns'></span>`;
                }
                initSQPriceColumnIndexes();

                var newPrice = parseFloatSafe(item.New_Price);
                var newMargin = parseFloatSafe(item.New_Margin);
                var priceUpdated = parseBool(item.Price_Updated);

                // Pending approval: show New Price instead of Basic Purchase Cost (display only)
                var showNewPriceAsCost = (newPrice !== 0) && !priceUpdated &&
                    (data.Status == "Approval pending from finance" || data.Status == "Approval pending from HOD");
                var displayedBasicCost = showNewPriceAsCost ? newPrice : parseFloatSafe(item.PCPL_MRP);

                var rowArray = [
                    "", // dtr-control
                    actionsHtml,
                    item.No,
                    item.Description,
                    item.Quantity,
                    item.Unit_of_Measure_Code,
                    item.PCPL_Packing_Style_Code,
                    item.PCPL_MRP,
                    item.Unit_Price,
                    newPrice,
                    newMargin,
                    priceUpdated ? 'true' : 'false',
                    item.Delivery_Date,
                    item.PCPL_Total_Cost,
                    item.PCPL_Margin,
                    data.PaymentTermsCode,
                    data.ShipmentMethodCode,
                    item.PCPL_Transport_Method,
                    item.PCPL_Transport_Cost,
                    item.PCPL_Sales_Discount,
                    item.PCPL_Commission_Type,
                    item.PCPL_Commission,
                    item.PCPL_Commission_Amount,
                    item.PCPL_Credit_Days,
                    item.PCPL_Interest,
                    `<label id="${item.No}_DropShipment">${item.Drop_Shipment}</label>`,
                    "",
                    "",
                    `<label id="${item.No}_MarginPercent">${item.PCPL_Margin_Percent} %</label>`,

                    item.PCPL_Commission_Payable,
                    //item.PCPL_Commission_Payable_Name,
                    "",
                    item.PCPL_Liquid,
                    item.PCPL_Concentration_Rate_Percent,
                    item.Net_Weight,
                    item.PCPL_Liquid_Rate,
                    item.PCPL_Vendor_No,
                    item.PCPL_Packing_MRP_Price,
                    `<label id="${item.No}_SQLineNo" style='display:none'>${item.Line_No}</label>`,
                ];

                var colCount = $('#dataList thead th').length;
                while (rowArray.length < colCount) rowArray.push('');
                if (rowArray.length > colCount) rowArray = rowArray.slice(0, colCount);

                var newNode = dtable.row.add(rowArray).draw(false).node();
                $(newNode).attr('id', 'ProdTR_' + item.No);
                $(newNode).attr('data-lineno', item.Line_No);
                $(newNode).find("td").eq(0).addClass("dtr-control");

                applySQPriceRowState('ProdTR_' + item.No);
            });

            dtable.responsive.recalc();
            itemLineNo = data.QuoteNo + "," + itemLineNo;
            $('#hfSalesQuoteResDetails').val(itemLineNo);

            if (SQFor == "ApproveReject") {

                if (LoggedInUserRole != "Admin" && LoggedInUserRole != "Salesperson") {
                    $('#dvSQApproveRejectBtn').css('display', 'block');

                    if (LoggedInUserRole == "Finance") {
                        $('#dvSQAprJustificationDetails').css('display', 'block');
                        $('#dvAprDetailsFinanceUser').css('display', 'block');
                        $('#lblOutstandingValue').css('display', 'block');
                        $('#dvcustomerOutStanding').css('display', 'block');
                        $('#lblJustificationTitle').text("Last 10 Sales Quote Justification Details");

                        $('#AprDetailsCustName').text(data.ContactCompanyName);
                        $('#AprDetailsApprovalFor').text(data.ApprovalFor);
                        $('#AprDetailsJustificationReason').text(data.WorkDescription);
                        $('#tdDetailsStatus').text(data.Status);
                        $('#tdDetailsLocationCode').text(data.LocationCode);
                        GetAndFillCompanyIndustry(data.ContactCompanyNo);
                    }
                    else {
                        $('#dvSQAprJustificationDetails').css('display', 'none');
                        $('#dvAprDetailsFinanceUser').css('display', 'none');
                        $('#lblOutstandingValue').css('display', 'none');
                        $('#dvcustomerOutStanding').css('display', 'none');
                        $('#lblJustificationTitle').text("Last 3 Sales Quote Justification Details");
                    }

                    $('#dvSQJustificationDetails').css('display', 'block');
                    // open this From for OnClick Section.

                    $(document).on("click", "#lblOutstandingValue", function () {
                        $(this).toggleClass("active");
                        BindCSOutstandingDuelist();
                    });

                    //$(document).on("click", "#lblJustificationTitle", function () {
                    //    BindJustificationDetails(LoggedInUserRole, data.ContactCompanyNo);
                    //});
                    $(document).on("click", "#lblJustificationTitle", function () {
                        BindJustificationDetails(LoggedInUserRole, data.ContactCompanyNo);
                        $(this).toggleClass("active");
                    });

                }

            }
            else {

                $('#dvSQApproveRejectBtn').css('display', 'none');
                $('#dvSQAprJustificationDetails').css('display', 'none');
                $('#dvAprDetailsFinanceUser').css('display', 'none');
                $('#lblOutstandingValue').css('display', 'none');
                $('#dvcustomerOutStanding').css('display', 'none');
                $('#dvSQJustificationDetails').css('display', 'none');
                $('#lblJustificationTitle').text("");

            }

        }
    });
    var LoggedInUserRole = $('#hdnLoggedInUserRole').val();
    if (LoggedInUserRole == "Finance" || LoggedInUserRole == "HOD") {

        $('#dvSQLineDetails').hide();
        $('#btnSaveProd').css('display', 'none');
        $('#btnSave').hide();
        $('#btnFinancehide').hide();
        $('#btnFinancehide1').hide();

    }
    else {

        $('#dvSQLineDetails').show();
        $('#btnSaveProd').css('display', 'block');
        $('#btnSave').show();
        $('#btnFinancehide').css('display', 'block');
        $('#btnFinancehide1').show();

    }

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
                                inqProdPackingStyle = data.ProductPackingStyle[i].PCPL_Purchase_Cost + "_" + data.ProductPackingStyle[i].Packing_Style_Code + "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days + "_" + data.ProductPackingStyle[i].PCPL_MRP_Price;
                            }
                        }
                        $('#hfPackingUnit').val(data.ProductPackingStyle[i].Packing_Unit);

                        packingstyleOpts +=
                            "<option value='" + data.ProductPackingStyle[i].PCPL_Purchase_Cost + "_" + data.ProductPackingStyle[i].Packing_Style_Code + "_" + data.ProductPackingStyle[i].PCPL_Purchase_Days + "_" + data.ProductPackingStyle[i].PCPL_MRP_Price + "'>" + data.ProductPackingStyle[i].Packing_Style_Description +
                            "</option>";
                    }

                    $('#ddlPackingStyle').append(packingstyleOpts);

                    if ($('#hfInqProdPackingStyle').val() != "" && $('#hfInqProdPackingStyle').val() != null) {
                        $('#ddlPackingStyle').val(inqProdPackingStyle);
                        $('#ddlPackingStyle').change();
                    }
                    else {
                        $('#ddlPackingStyle').val('-1');
                    }
                    if (data.PCPL_liquid === true) {

                        // enable only — DO NOT auto-check
                        $('#chkIsLiquidProd')
                            .prop('disabled', false)
                            .prop('checked', false);

                        // hide liquid fields (user will open manually)
                        $('#dvLiquidProdFields').hide();

                        $('#hfNetWeight').val(data.Net_Weight);
                        $('#hfGrossWeight').val(data.Gross_Weight);
                        $('#lblGrossWeight').text(data.Gross_Weight);
                        $('#txtNetWeight').val(data.Net_Weight);
                    }
                    else {

                        // unchecked + disabled
                        $('#chkIsLiquidProd')
                            .prop('checked', false)
                            .prop('disabled', true);

                        $('#dvLiquidProdFields').hide();

                        $('#hfNetWeight').val(data.Net_Weight);
                        $('#hfGrossWeight').val(data.Gross_Weight);
                        $('#lblGrossWeight').text(data.Gross_Weight);
                        $('#txtNetWeight').val(data.Net_Weight);
                    }




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
                    }

                    // If SQLinePriceAction triggered edit, apply overrides AFTER all packing style changes.
                    applySQEditOverridesIfAny();

                    if ($('#hfiteminvstatus').val() == "NotInInventory") {

                    }

                    // Apply one-shot overrides after packing style changes are done.
                    applySQEditOverridesIfAny();

                }
            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function EditSQProd(Line_No, ProdTR) {

    ResetQuoteLineDetails();
    initSQPriceColumnIndexes();

    var prodNo = ($("#" + ProdTR).find("TD").eq(2).text() || '').toString().trim();
    $('#hfProdNo').val(prodNo);
    $('#hfProdNoEdit').val(prodNo);
    var product = $("#" + ProdTR).find("TD").eq(6).html();
    var packingStyle = ($("#" + ProdTR).find("TD").eq(3).html());
    GetProductDetails(packingStyle);
    $('#ddlPackingStyle').val(product);
    $('#hfSavedPackingStyle').val(product);
    $('#txtProductName').val($("#" + ProdTR).find("TD").eq(3).html());

    try {
        var ln = parseInt(((($("#" + ProdTR).attr('data-lineno') || '')).toString().trim())) || 0;
        if (!ln || ln <= 0) {
            ln = parseInt(((($("#" + prodNo + "_SQLineNo").text() || '')).toString().trim())) || 0;
        }
        $('#hfSQProdLineNo').val(ln || '');
    } catch (e) {
        $('#hfSQProdLineNo').val($("#" + prodNo + "_SQLineNo").text());
    }
    $('#txtProdQty').val($("#" + ProdTR).find("TD").eq(4).html());
    $('#txtProdMRP').val($("#" + ProdTR).find("TD").eq(7).html());
    $('#ddlTransportMethod option:selected').text($("#" + ProdTR).find("TD").eq(17).html());
    $('#txtSalesPrice').val($("#" + ProdTR).find("TD").eq(8).html());
    $('#txtTransportCost').val($("#" + ProdTR).find("TD").eq(18).html());
    $('#txtSalesDiscount').val($("#" + ProdTR).find("TD").eq(19).html());
    $('#txtDeliveryDate').val($("#" + ProdTR).find("TD").eq(12).html());
    var isLiquidProd = $("#" + ProdTR).find("TD").eq(31).html();
    var isChkCommissionChecked = $("#" + ProdTR).find("TD").eq(20).html();
    var dropShipHtml = $("#" + ProdTR).find("TD").eq(25).text().trim();
    var VendorDrop = $("#" + ProdTR).find("TD").eq(35).html();
    if (dropShipHtml == 'Yes' || dropShipHtml == 'true') {
        $("#chkDropShipment").prop('checked', true);
        $('#chkDropShipment').change();
        $("#dvVendors").show();
        $('#hfSavedItemVendorNo').val(VendorDrop);
    } else {
        $("#chkDropShipment").prop('checked', false);
    }
    if (isChkCommissionChecked == "" || isChkCommissionChecked == " ") {
        $('#chkIsCommission').prop('checked', false);
        $('#chkIsCommission').change();
        $('#txtSalesPrice').prop('readonly', false);
        $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(22).html());
        $('#txtCommissionAmt').prop('disabled', false);
        $('#ddlCommissionPayable').prop('disabled', false);
    } else {
        $('#txtSalesPrice').prop('readonly', false);
        $('#chkIsCommission').prop('checked', true);
        $('#chkIsCommission').change();
        $('#ddlCommissionPerUnitPercent').val($("#" + ProdTR).find("TD").eq(20).html());
        $('#txtCommissionPercent').val($("#" + ProdTR).find("TD").eq(21).html());
        $('#txtCommissionAmt').val($("#" + ProdTR).find("TD").eq(22).html());

    }
    if (isLiquidProd == "true") {
        $('#dvLiquidProdFields').css('display', 'block');
        $('#chkIsLiquidProd').prop('checked', true);
        $('#hfIsLiquidProd').val($("#" + ProdTR).find("TD").eq(31).html());
        $('#txtConcentratePercent').val($("#" + ProdTR).find("TD").eq(32).html());
        $('#txtNetWeight').val($("#" + ProdTR).find("TD").eq(33).html());
        $('#txtLiquidRate').val($("#" + ProdTR).find("TD").eq(34).html());
    }
    else {
        $('#chkIsLiquidProd').prop('checked', false);
    }
    var commissionPayable = $("#" + ProdTR).find("TD").eq(29).html();
    if (commissionPayable != "") {
        $('#ddlCommissionPayable').val(commissionPayable);
    }
    else {
        $('#ddlCommissionPayable').val('-1');
    }
    $('#txtCreditDays').val($("#" + ProdTR).find("TD").eq(23).html());
    $('#txtMargin').val($("#" + ProdTR).find("TD").eq(14).html());
    $('#spnMarginPercent').val($("#" + ProdTR).find("TD").eq(33).html());
    $('#txtInterest').val($("#" + ProdTR).find("TD").eq(24).html());
    // Set InqProdLineNo from DataTable
    var inqProdLineNo = $("#" + prodNo + "_InqProdLineNo").text();
    $('#hfProdLineNo').val(inqProdLineNo || Line_No);
    $('#txtTotalCost').val($("#" + ProdTR).find("TD").eq(13).html());
    if ($("#" + prodNo + "_InqProdLineNo").val() != "") {
        $('#hfProdLineNo').val($("#" + prodNo + "_InqProdLineNo").val());
    }
    $('#txtMRPPrice').val($("#" + ProdTR).find("TD").eq(36).html());

    // If this Edit was opened from Price Update action, force the required values now.
    // This runs after ResetQuoteLineDetails() and after the default field population.
    try {
        if (SQ_PENDING_PRICEUPDATE && (SQ_PENDING_PRICEUPDATE.prodNo || '') === (prodNo || '').toString().trim()) {
            var pendingLineNo = parseInt(SQ_PENDING_PRICEUPDATE.lineNo || 0);
            var currentLineNo = parseInt($('#hfSQProdLineNo').val() || Line_No || 0);
            if (!pendingLineNo || pendingLineNo === currentLineNo) {
                // Per requirements, New Price maps to Sales Price (not Basic Purchase Cost).
                $('#txtSalesPrice').val(parseFloat(SQ_PENDING_PRICEUPDATE.newPrice || 0).toFixed(2));

                // If commission amount is percentage-based, refresh it based on the new Sales Price
                // before calculating totals/margin.
                try { sqRecalcCommissionAmountFromSalesPriceIfPercentMode(); } catch (e) { }

                if (SQ_PENDING_PRICEUPDATE.lockMargin === true) {
                    $('#txtMargin').val(parseFloat(SQ_PENDING_PRICEUPDATE.newMargin || 0).toFixed(2));
                    $('#txtMargin').prop('disabled', true);
                    SQ_FORM_LOCKS.Margin = true;
                    SQ_FORM_LOCKS.MarginValue = parseFloat(SQ_PENDING_PRICEUPDATE.newMargin || 0).toFixed(2);
                } else {
                    SQ_FORM_LOCKS.Margin = false;
                    SQ_FORM_LOCKS.MarginValue = null;
                }

                // Always recalc totals/margin after Price Update applies Sales Price.
                try { CalculateFormula(); } catch (e) { }

                // Push the form-computed Margin back to the grid row so UI matches formula logic.
                try {
                    initSQPriceColumnIndexes();
                    var marginIdx = (SQ_PRICE_COLS.MARGIN !== null && SQ_PRICE_COLS.MARGIN >= 0) ? SQ_PRICE_COLS.MARGIN : 14;
                    var $row = $("#" + ProdTR);
                    if ($row && $row.length) {
                        var m = parseFloatSafe($('#txtMargin').val());
                        sqSetRowCellText($row, marginIdx, m.toFixed(2));
                    }
                } catch (e) { }

                SQ_PENDING_PRICEUPDATE = null;
            }
        }
    } catch (e) {
        // no-op
    }

    // Normal Edit open should also apply the same formula logic
    // (TotalCost/Margin/Margin%) after async packing style + basic cost fields are populated.
    try {
        (function (expectedProdNo) {
            var attempts = 12;
            var tick = function () {
                attempts--;

                // If user switched rows quickly, abort.
                var cur = ($('#hfProdNo').val() || '').toString().trim();
                if (cur !== (expectedProdNo || '').toString().trim()) return;

                var packingVal = ($('#ddlPackingStyle').val() || '').toString();
                var packingOk = (packingVal !== '' && packingVal !== '-1');
                var basicOk = (($('#txtBasicPurchaseCost').val() || '').toString().trim() !== '');

                if (packingOk && basicOk) {
                    try { CalculateFormula(); } catch (e) { }
                    return;
                }

                if (attempts > 0) return setTimeout(tick, 150);

                // Best-effort final compute.
                try { CalculateFormula(); } catch (e) { }
            };

            setTimeout(tick, 0);
        })(prodNo);
    } catch (e) {
        // no-op
    }
}
function DeleteSQProd(LineNo, ProdTR) {
    var $row = $("#" + ProdTR);

    try {
        if ($.fn.dataTable && $.fn.dataTable.isDataTable && $.fn.dataTable.isDataTable('#dataList')) {
            var dt = $('#dataList').DataTable();
            var rowApi = dt.row($row.length ? $row : ('#' + ProdTR));

            if (rowApi && rowApi.any && rowApi.any()) {
                rowApi.remove().draw(false);
                try { dt.responsive.recalc(); } catch (e) { }
            } else {
                $row.remove();
            }
        } else {
            $row.remove();
        }
    } catch (e) {
        $row.remove();
    }

    var deletedProdNo = (ProdTR || '').toString().replace(/^ProdTR_/, '').trim();
    var currentProdNo = ($('#hfProdNo').val() || '').toString().trim();
    var currentEditProdNo = ($('#hfProdNoEdit').val() || '').toString().trim();

    // Clear stale edit state so add flow does not accidentally target a deleted row.
    if (currentEditProdNo === deletedProdNo || currentProdNo === deletedProdNo) {
        ResetQuoteLineDetails();
    }

    $('#hfProdNoEdit').val('');
    $('#hfSQProdLineNo').val('');

    if (currentProdNo === deletedProdNo) {
        $('#hfProdNo').val('');
    }
}

function CalculateFormula() {
    if ($('#txtProductName').val() == "") {
    }
    else {
        var CostPrice = 0, Interest = 0, BasicPurchaseCost = 0, SalesPrice = 0;
        $('#txtTotalCost').val("0");

        // If basic purchase cost isn't populated yet (common when edit loads async),
        // try to derive it from the selected packing style option value.
        try {
            var psVal = ($('#ddlPackingStyle').val() || '').toString();
            if (($('#txtBasicPurchaseCost').val() || '').toString().trim() === '' && psVal && psVal !== '-1' && psVal.indexOf('_') > 0) {
                var parts = psVal.split('_');
                if (parts.length >= 1) {
                    var derivedBasic = parseFloatSafe(parts[0]);
                    if (derivedBasic > 0) {
                        $('#txtBasicPurchaseCost').val(derivedBasic.toFixed(2));
                        $('#txtBasicPurchaseCost').prop('disabled', true);
                    }
                }
                if (parts.length >= 4) {
                    var derivedMrp = parseFloatSafe(parts[3]);
                    if (derivedMrp > 0) {
                        $('#txtMRPPrice').val(derivedMrp.toFixed(4));
                        $('#txtMRPPrice').prop('disabled', true);
                    }
                }
                if (parts.length >= 3) {
                    var pd = parseInt(parts[2] || '0');
                    if (!isNaN(pd) && pd > 0) $('#hfPurchaseDays').val(pd);
                }
            }
        } catch (e) { }

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
            var TempCreditDays = 10;

            var creditDays = GetCreditDaysForNotInInventory();
            $('#txtCreditDays').val(creditDays);
            $('#txtCreditDays').prop('disabled', true);
            var InterestRate = parseFloat($('#hfInterestRate').val()) / 100;
            Interest = (BasicPurchaseCost * InterestRate) * (parseInt(creditDays) / 365);
            $('#txtInterest').val(parseFloat(Interest).toFixed(2));
            $('#txtInterest').prop('disabled', true);
            $('#spnInterestRate').text("");
            $('#spnInterestRate').text($('#hfInterestRate').val());
            if ($('#ddlTransportMethod').val() == "TOPAY") {

                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val())  + parseFloat($('#txtInsurance').val());

            }
            else if ($('#ddlTransportMethod').val() == "PAID") {

                CostPrice = BasicPurchaseCost + parseFloat($('#txtSalesDiscount').val()) + parseFloat($('#txtCommissionAmt').val()) + parseFloat($('#txtTransportCost').val()) + parseFloat(Interest); //  - parseFloat($('#txtPurDiscount').val()) + parseFloat($('#txtInsurance').val());

            }

            $('#txtTotalCost').val(CostPrice.toFixed(2));
            $('#txtTotalCost').prop('disabled', true);
        }

        if (SQ_FORM_LOCKS.Margin === true && SQ_FORM_LOCKS.MarginValue !== null && SQ_FORM_LOCKS.MarginValue !== undefined && SQ_FORM_LOCKS.MarginValue !== '') {
            $('#txtMargin').val(parseFloat(SQ_FORM_LOCKS.MarginValue).toFixed(2));
            $('#txtMargin').prop('disabled', true);
        }
        else {
            $('#txtMargin').val((parseFloat($('#txtSalesPrice').val()) - parseFloat($('#txtTotalCost').val())).toFixed(2));
            $('#txtMargin').prop('disabled', true);
        }
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
            salesQuoteDetails.ConsigneeAddress = $("#ddlConsigneeAddress option:selected").text();
            salesQuoteDetails.DeliveryDate = $('#txtDeliveryDate').val();
            salesQuoteDetails.Remarks = "";


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

function ResetQuoteLineDetails() {

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
    $('#ddlIncoTerms').change();
    $('#txtUOM').val("");
    $('#txtTransportCost').val("");
    $("#hfProdNo").val('');
    $('#txtDeliveryDate').val("");
    $('#txtSalesDiscount').val("");
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
    $('#txtCreditDays').val("0");
    $('#txtCreditDays').prop('disabled', false);
    $('#txtSalesPrice').val("");
    $('#chkDropShipment').prop('checked', false);
    $('#dvVendors').css('display', 'none');
    $('#ddlItemVendors').empty();
    $('#ddlItemVendors').append("<option value=''>---Select---</option>");
    $('#ddlItemVendors').val('');
    $('#txtMRPPrice').val("");
    $('#txtMRPPrice').prop('disabled', true);


    $('#hfProdNoEdit').val("");
    $('#hfUnitPriceEdit').val("");
    $('#hfSavedPackingStyle').val("");
    $('#hfSavedTotalUnitPrice').val("");
    $('#hfSavedMargin').val("");

    // Clear any Price Update locks when resetting the form.
    SQ_FORM_LOCKS.Margin = false;
    SQ_FORM_LOCKS.MarginValue = null;

}



function AddReqQty() {

    $('#txtProdQty').val($('#txtReqQty').val());
    $('#txtBasicPurchaseCost').val($('#txtMrpFromILE').val());
    $('#txtMRPPrice').val($('#txtMrpFromILE').val());
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

    // Prevent double-submit (works for both Finance and HOD flows)
    $('#btnApprove').prop('disabled', true);
    $('#btnReject').prop('disabled', true);
    $('#btnConfirmReject').prop('disabled', true);

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

            // Keep buttons disabled after success
            $('#btnApprove').prop('disabled', true);
            $('#btnReject').prop('disabled', true);
            $('#btnConfirmReject').prop('disabled', true);

        }
        else if (resMsg.includes("Error:")) {
            const resMsgDetails = resMsg.split(':');

            $('#modalErrMsg').css('display', 'block');
            $('#modalErrDetails').text(resMsgDetails[1]);

            // Allow retry on error
            $('#btnApprove').prop('disabled', false);
            $('#btnReject').prop('disabled', false);
            $('#btnConfirmReject').prop('disabled', false);
        }
        else {
            // Unknown response; allow retry
            $('#btnApprove').prop('disabled', false);
            $('#btnReject').prop('disabled', false);
            $('#btnConfirmReject').prop('disabled', false);
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

function BindCSOutstandingDuelist() {
    var customerName = $("#hfSavedCustomerName").val();
    //var productName = $("#hfProdNo").val();

    $.ajax({
        url: '/SPSalesQuotes/GetCSOutstandingDuelist',
        type: 'GET',
        data: {
            CustomerName: customerName,
            // ProductName: productName
        },
        success: function (data) {
            $("#tblCSOutstandingDuelist").empty();
            var TROpts = "";

            $.each(data, function (index, item) {
                TROpts += "<tr>" + "<td>" + item.Document_Type + "</td>" + "<td>" + item.Bill_No + "</td>" + "<td>" + item.Bill_Date + "</td>" + "<td>" + item.Product_Name + "</td>" + "<td>" + item.Terms + "</td>" + "<td>" + item.Due_Date + "</td>" + "<td>" + item.Invoice_Amount + "</td>" + "<td>" + item.Remain_Amount + "</td>" + "<td>" + item.Total_Days + "</td>" + "<td>" + item.Overdue_Days + "</td>" + "</tr>";
            });

            $('#tblCSOutstandingDuelist').append(TROpts);

        },
        error: function () {
            alert("error");
        }
    }
    );

}

// item Price change related global variables and functions
var InqNo = "", SQNo = "", ScheduleStatus = "", SQStatus = "", SQFor = "", LoggedInUser = "";

var SQ_PRICE_COLS = {
    BASIC_PURCHASE_COST: null,
    SALES_PRICE: null,
    TOTAL_COST: null,
    NEW_PRICE: null,
    NEW_MARGIN: null,
    PRICE_UPDATED: null,
    MRP_PRICE: null,
    MARGIN: null
};
var SQ_EDIT_OVERRIDES = {
    SalesPrice: null,
    Margin: null
};
var SQ_FORM_LOCKS = {
    Margin: false,
    MarginValue: null
};

// Pending form-apply values for Price Update action (applied inside EditSQProd).
var SQ_PENDING_PRICEUPDATE = null;


function applySQEditOverridesIfAny() {
    if (SQ_EDIT_OVERRIDES.SalesPrice !== null) {
        $('#txtSalesPrice').val(SQ_EDIT_OVERRIDES.SalesPrice);
    }
    if (SQ_EDIT_OVERRIDES.Margin !== null) {
        $('#txtMargin').val(SQ_EDIT_OVERRIDES.Margin);
        $('#txtMargin').prop('disabled', true);

        // Prevent CalculateFormula() from overwriting the user-mandated Margin value
        // after packing-style/other dependent recalculations.
        SQ_FORM_LOCKS.Margin = true;
        SQ_FORM_LOCKS.MarginValue = SQ_EDIT_OVERRIDES.Margin;
    }
    if (SQ_EDIT_OVERRIDES.SalesPrice !== null || SQ_EDIT_OVERRIDES.Margin !== null) {
        SQ_EDIT_OVERRIDES.SalesPrice = null;
        SQ_EDIT_OVERRIDES.Margin = null;
    }
}

function initSQPriceColumnIndexes() {
    // Resolve by header text so column order can change safely.
    var $ths = $('#dataList thead th');
    var findIdx = function (label) {
        var idx = -1;
        $ths.each(function (i) {
            if ($(this).text().trim().toLowerCase() === label) {
                idx = i;
                return false;
            }
        });
        return idx;
    };
    SQ_PRICE_COLS.BASIC_PURCHASE_COST = findIdx('basic purchase cost');
    SQ_PRICE_COLS.SALES_PRICE = findIdx('sales price');
    SQ_PRICE_COLS.TOTAL_COST = findIdx('total cost');
    SQ_PRICE_COLS.NEW_PRICE = findIdx('new price');
    SQ_PRICE_COLS.NEW_MARGIN = findIdx('new margin');
    SQ_PRICE_COLS.PRICE_UPDATED = findIdx('price updated');
    SQ_PRICE_COLS.MRP_PRICE = findIdx('mrp price');
    SQ_PRICE_COLS.MARGIN = findIdx('margin');
}

function sqTryComputeMarginFromRow($tr, salesPrice) {
    try {
        if (!$tr || $tr.length === 0) return 0;
        initSQPriceColumnIndexes();

        var totalCostIdx = (SQ_PRICE_COLS.TOTAL_COST !== null && SQ_PRICE_COLS.TOTAL_COST >= 0) ? SQ_PRICE_COLS.TOTAL_COST : 13;
        var totalCost = parseFloatSafe($tr.find('TD').eq(totalCostIdx).text());
        if (totalCost === 0) return 0;
        return parseFloatSafe(salesPrice) - totalCost;
    } catch (e) {
        return 0;
    }
}

function sqSetRowCellText($tr, colIdx, text) {
    if (!$tr || $tr.length === 0) return;
    if (colIdx === null || colIdx === undefined || colIdx < 0) return;

    // Prefer DataTables API so Responsive child rows also reflect the change.
    try {
        if ($.fn.dataTable && $.fn.dataTable.isDataTable && $.fn.dataTable.isDataTable('#dataList')) {
            var dt = $('#dataList').DataTable();
            var rowApi = dt.row($tr);
            if (rowApi && rowApi.any && rowApi.any()) {
                var rowData = rowApi.data();
                if (rowData && rowData.length > colIdx) {
                    rowData[colIdx] = text;
                    rowApi.data(rowData).draw(false);
                    try { dt.responsive.recalc(); } catch (e2) { }
                    return;
                }
            }
        }
    } catch (e) {
        // fallback to raw DOM
    }

    $tr.find('TD').eq(colIdx).text(text);
}

function sqGetRowCellText($tr, colIdx) {
    if (!$tr || $tr.length === 0) return '';
    if (colIdx === null || colIdx === undefined || colIdx < 0) return '';

    // Prefer DataTables API so Responsive child rows don't break reads.
    try {
        if ($.fn.dataTable && $.fn.dataTable.isDataTable && $.fn.dataTable.isDataTable('#dataList')) {
            var dt = $('#dataList').DataTable();
            var rowApi = dt.row($tr);
            if (rowApi && rowApi.any && rowApi.any()) {
                var rowData = rowApi.data();
                if (rowData && rowData.length > colIdx) {
                    return (rowData[colIdx] === null || rowData[colIdx] === undefined) ? '' : rowData[colIdx].toString();
                }
            }
        }
    } catch (e) {
        // fallback to raw DOM
    }

    return ($tr.find('TD').eq(colIdx).text() || '');
}

function parseBool(val) {
    if (val === true || val === false) return val;
    var s = ((val === null || val === undefined) ? "" : val).toString().trim().toLowerCase();
    return (s === "true" || s === "yes" || s === "1");
}

function parseFloatSafe(val) {
    if (val === null || val === undefined) return 0;
    var s = val.toString().replace(/,/g, '').trim();
    var n = parseFloat(s);
    return isNaN(n) ? 0 : n;
}

function sqRecalcCommissionAmountFromSalesPriceIfPercentMode() {
    try {
        // Commission amount depends on Sales Price when commission is enabled and mode is %.
        // Keep this logic scoped and conservative.
        if (!$('#chkIsCommission').is(':checked')) return;
        if (($('#ddlCommissionPerUnitPercent').val() || '').toString().trim() !== '%') return;

        var commissionPercent = parseFloatSafe($('#txtCommissionPercent').val());
        if (commissionPercent <= 0) return;

        var salesPrice = parseFloatSafe($('#txtSalesPrice').val());
        var commissionAmt = ((salesPrice * commissionPercent) / 100);
        $('#txtCommissionAmt').val(commissionAmt.toFixed(2));
        $('#txtCommissionAmt').prop('readonly', true);
    } catch (e) {
        // no-op
    }
}

function isSQStatusApprovedOrRejected() {
    var s = ((window.SQStatus === null || window.SQStatus === undefined) ? '' : window.SQStatus).toString().trim().toLowerCase();
    return (s.indexOf('approved') >= 0) || (s.indexOf('rejected') >= 0);
}

function enableSaveButtonsForPriceUpdateIfNeeded() {
    if (!isSQStatusApprovedOrRejected()) return;
    $('#btnSaveProd').prop('disabled', false);
    $('#btnSave').prop('disabled', false);
}

function forceApplyPriceUpdateValuesToForm(prodNo, newPrice, newMargin, lockMargin) {
    if (lockMargin === undefined || lockMargin === null) lockMargin = false;
    var attempts = 20;
    var tryApply = function () {
        attempts--;

        var curProdNo = ($('#hfProdNo').val() || '').toString().trim();
        if (curProdNo !== (prodNo || '').toString().trim()) {
            if (attempts > 0) return setTimeout(tryApply, 150);
            return;
        }

        // Per requirements, New Price maps to Sales Price (not Basic Purchase Cost).
        $('#txtSalesPrice').val(parseFloat(newPrice || 0).toFixed(2));
        if (lockMargin) {
            $('#txtMargin').val(parseFloat(newMargin || 0).toFixed(2));
            $('#txtMargin').prop('disabled', true);
            SQ_FORM_LOCKS.Margin = true;
            SQ_FORM_LOCKS.MarginValue = parseFloat(newMargin || 0).toFixed(2);
        } else {
            SQ_FORM_LOCKS.Margin = false;
            SQ_FORM_LOCKS.MarginValue = null;
        }

        try { CalculateFormula(); } catch (e) { }
    };

    setTimeout(tryApply, 0);
}

function buildSQPriceActionsHtml(prodNo, lineNo, enabled) {
    var roleBlocked = isSQPriceActionRoleBlocked();
    var canClick = !!enabled && !roleBlocked;

    var dis = canClick ? "" : "disabled";
    var title1 = "";
    if (!canClick) {
        if (roleBlocked) title1 = " title='Disabled for Finance/HOD role'";
        else title1 = " title='New Price must be non-zero'";
    }

    return `<button type='button' class='btn btn-primary' ${dis}${title1} onclick='SQLinePriceAction("Update","${prodNo}",${lineNo},"ProdTR_${prodNo}")'>Update Price</button>`
        + `&nbsp;<button type='button'class='btn btn-secondary' ${dis}${title1} onclick='SQLinePriceAction("NoUpdate","${prodNo}",${lineNo},"ProdTR_${prodNo}")'>Not Update Price</button>`;
}

function isSQPriceActionRoleBlocked() {
    try {
        var role = (typeof LoggedInUserRole !== 'undefined' && LoggedInUserRole !== null)
            ? LoggedInUserRole
            : ($('#hdnLoggedInUserRole').val() || '');
        role = (role || '').toString().trim().toLowerCase();
        return role === 'finance' || role === 'hod';
    } catch (e) {
        return false;
    }
}

function applySQPriceRowState(prodTR) {
    if (!prodTR) return;
    initSQPriceColumnIndexes();

    var $tr = $("#" + prodTR);
    if ($tr.length === 0) return;

    var newPrice = parseFloatSafe($tr.find("TD").eq(SQ_PRICE_COLS.NEW_PRICE).text());
    var priceUpdated = parseBool($tr.find("TD").eq(SQ_PRICE_COLS.PRICE_UPDATED).text());

    $tr.removeClass('sq-price-pending sq-price-updated');
    if (priceUpdated) {
        $tr.addClass('sq-price-updated');
    } else if (newPrice !== 0) {
        $tr.addClass('sq-price-pending');
    }

    // Enable actions only when New Price != 0
    // NOTE: Do not rely on fixed TD indexes for product no; DataTables Responsive can reorder/hide cells.
    var prodNo = (prodTR || '').toString();
    if (prodNo.indexOf('ProdTR_') === 0) prodNo = prodNo.substring('ProdTR_'.length);
    prodNo = (prodNo || '').toString().trim();
    if (!prodNo) {
        prodNo = ($tr.find("TD").eq(2).text() || '').toString().trim();
    }

    var lineNo = parseInt($tr.attr('data-lineno') || '0');
    var enabled = (newPrice !== 0);

    // Buttons are rendered in the existing left-side Actions column (before Product No)
    var $host = $tr.find("span[id$='_SQPriceBtns']").first();
    if ($host.length === 0 && prodNo) {
        $host = $("#" + prodNo + "_SQPriceBtns");
    }
    if ($host.length === 0) {
        var $actionsTd = $tr.find("TD").eq(1);
        if ($actionsTd.length && prodNo) {
            $actionsTd.append(`&nbsp;<span id='${prodNo}_SQPriceBtns'></span>`);
            $host = $("#" + prodNo + "_SQPriceBtns");
        }
    }
    if ($host.length && prodNo) {
        // Ensure host has the expected id for future lookups.
        if (($host.attr('id') || '') !== (prodNo + "_SQPriceBtns")) {
            $host.attr('id', prodNo + "_SQPriceBtns");
        }
        $host.html(buildSQPriceActionsHtml(prodNo, lineNo, enabled));
    }
}

function setSQEditable(isEditable) {
    $('#btnSaveProd').prop('disabled', !isEditable);
    $('#btnSave').prop('disabled', !isEditable);
}

function callReopenSalesQuote(quoteNo, onDone) {
    // Reopen is intentionally disabled on Sales Quote page.
    // Button remains, but we do not call backend reopen endpoint.
    ShowErrMsg('Reopen is disabled on this page.');
    if (typeof onDone === 'function') onDone(false);
}

function SQLinePriceAction(action, prodNo, lineNo, prodTR) {
    initSQPriceColumnIndexes();
    var quoteNo = $('#hfSalesQuoteNo').val();
    if (!quoteNo) {
        ShowErrMsg('Sales Quote No not found.');
        return;
    }

    // Requirement: For Finance/HOD role, price update actions are always disabled.
    if (isSQPriceActionRoleBlocked()) {
        ShowErrMsg('Update Price action is disabled for Finance/HOD role.');
        return;
    }

    // If quote is not editable (approval pending), do not allow Price Update actions.
    // Reopen is disabled per requirements.
    if ($('#btnSave').prop('disabled') === true && !isSQStatusApprovedOrRejected()) {
        ShowErrMsg('Sales Quote is not editable. Please reject first, then update.');
        return;
    }

    var $tr = $("#" + prodTR);
    if ($tr.length === 0) {
        ShowErrMsg('Quote line not found.');
        return;
    }

    var newPrice = parseFloatSafe($tr.find('TD').eq(SQ_PRICE_COLS.NEW_PRICE).text());
    var newMargin = parseFloatSafe($tr.find('TD').eq(SQ_PRICE_COLS.NEW_MARGIN).text());

    if (newPrice === 0) {
        ShowErrMsg('New Price must be non-zero.');
        return;
    }

    // No Price Update: UX requirement is to immediately clear New fields and remove red highlight.
    if (action === 'NoUpdate') {
        // No change in Basic Purchase Cost / Margin
        sqSetRowCellText($tr, SQ_PRICE_COLS.NEW_PRICE, '0');
        sqSetRowCellText($tr, SQ_PRICE_COLS.NEW_MARGIN, '0');
        sqSetRowCellText($tr, SQ_PRICE_COLS.PRICE_UPDATED, 'false');
        applySQPriceRowState(prodTR);

        // If user is currently editing this same product, release any margin lock.
        if (($('#hfProdNo').val() || '').toString().trim() === (prodNo || '').toString().trim()) {
            SQ_FORM_LOCKS.Margin = false;
            SQ_FORM_LOCKS.MarginValue = null;
            $('#txtMargin').prop('disabled', false);
        }

        // Clear any pending update for this product.
        if (SQ_PENDING_PRICEUPDATE && (SQ_PENDING_PRICEUPDATE.prodNo || '') === (prodNo || '')) {
            SQ_PENDING_PRICEUPDATE = null;
        }
        return;
    }

    var applyUpdateClientSide = function () {
        enableSaveButtonsForPriceUpdateIfNeeded();

        // Margin will be recalculated via CalculateFormula() after opening Edit.
        var computedMargin = sqTryComputeMarginFromRow($tr, newPrice);

        // Update Sales Price + Margin from New values
        var salesPriceIdx = (SQ_PRICE_COLS.SALES_PRICE !== null && SQ_PRICE_COLS.SALES_PRICE >= 0) ? SQ_PRICE_COLS.SALES_PRICE : 8;
        var marginIdx = (SQ_PRICE_COLS.MARGIN !== null && SQ_PRICE_COLS.MARGIN >= 0) ? SQ_PRICE_COLS.MARGIN : 14;
        sqSetRowCellText($tr, salesPriceIdx, newPrice.toFixed(2));
        sqSetRowCellText($tr, marginIdx, computedMargin.toFixed(2));

        // Clear New fields + mark updated
        sqSetRowCellText($tr, SQ_PRICE_COLS.NEW_PRICE, '0');
        sqSetRowCellText($tr, SQ_PRICE_COLS.NEW_MARGIN, '0');
        sqSetRowCellText($tr, SQ_PRICE_COLS.PRICE_UPDATED, 'true');

        applySQPriceRowState(prodTR);

        // Open line in edit mode and auto-save changes.
        SQ_EDIT_OVERRIDES.SalesPrice = newPrice.toFixed(2);
        SQ_EDIT_OVERRIDES.Margin = null;

        SQ_FORM_LOCKS.Margin = false;
        SQ_FORM_LOCKS.MarginValue = null;

        // Ensure EditSQProd applies values after it finishes populating the form.
        SQ_PENDING_PRICEUPDATE = {
            prodNo: (prodNo || '').toString().trim(),
            lineNo: parseInt(lineNo || 0),
            newPrice: newPrice,
            newMargin: null,
            lockMargin: false
        };

        EditSQProd(lineNo, prodTR);

        // Ensure the edit form fields are updated even if auto-save readiness fails.
        forceApplyPriceUpdateValuesToForm(prodNo, newPrice, 0, false);

        var attempts = 12;
        var tryAutoSave = function () {
            attempts--;
            if ($('#hfProdNo').val() !== prodNo) {
                if (attempts > 0) return setTimeout(tryAutoSave, 200);
                return;
            }

            // Ensure key fields are present before auto-saving.
            var ready =
                ($('#txtProductName').val() || '').trim() !== '' &&
                ($('#ddlPackingStyle').val() || '') !== '' && ($('#ddlPackingStyle').val() || '') !== '-1' &&
                (parseFloat($('#txtProdQty').val() || '0') > 0) &&
                ($('#txtSalesPrice').val() || '').trim() !== '' &&
                ($('#txtDeliveryDate').val() || '').trim() !== '' &&
                ($('#ddlIncoTerms').val() || '') !== '' && ($('#ddlIncoTerms').val() || '') !== '-1' &&
                ($('#ddlPaymentTerms').val() || '') !== '' && ($('#ddlPaymentTerms').val() || '') !== '-1';

            if (!ready) {
                if (attempts > 0) return setTimeout(tryAutoSave, 200);
                return;
            }

            $('#txtSalesPrice').val(newPrice.toFixed(2));
            SQ_FORM_LOCKS.Margin = false;
            SQ_FORM_LOCKS.MarginValue = null;
            try { sqRecalcCommissionAmountFromSalesPriceIfPercentMode(); } catch (e) { }
            try { CalculateFormula(); } catch (e) { }

            // Auto-save only when editable.
            if ($('#btnSave').prop('disabled') === true || $('#btnSaveProd').prop('disabled') === true) return;
            $('#btnSaveProd').click();
        };

        setTimeout(tryAutoSave, 250);
    };
    applyUpdateClientSide();
}