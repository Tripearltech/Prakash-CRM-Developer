var apiUrl = $('#getServiceApiUrl').val() + 'SPGRN/';

$(document).ready(function () {

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    ShowHideFields($('#lblDocumentType').html());
    $('.btn-close').click(function () {
        $('#modalItemTracking').css('display', 'none');
    });
});
var delay = (function () {
    var timer = 0;
    return function (callback, ms) {
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
    };
})();

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

function ShowHideFields(documenttype) {

    if (documenttype == "Sales Return") {

        $('#divQcRemarks').css('display', 'none');
    }
    else if (documenttype == "Transfer Order") {

        $('#divCurrencyCode').css('display', 'none');
        $('#divVendorOrderNo').css('display', 'none');
        $('#divSalesperson').css('display', 'none');
        $('#divContactName').css('display', 'none');
        $('#divDocumentDate').css('display', 'none');
        $('#divVendorInvoiceNo').css('display', 'none');
        $('#divQcRemarks').css('display', 'none');
    }

}

function SaveGRN() {

    debugger;
    var apiUrl = '/SPGRN/SaveSPGRNCard';

    var GRNHeaderLine = {};
    let grnLines = [];

    var doctype = $('#lblDocumentType')[0].innerText;
    var docnumber = $('#lblDocumentNo')[0].innerText;
    GRNHeaderLine.documenttype = doctype;
    GRNHeaderLine.documentno = docnumber;
    GRNHeaderLine.orderno = $('#lblOrderDate')[0].innerText;
    GRNHeaderLine.vendorcustomername = $('#lblVendorCustomerName')[0].innerText;
    GRNHeaderLine.locationcode = $('#lblLocationCode')[0].innerText;
    GRNHeaderLine.vendorcustomername = $('#lblVendorCustomerName')[0].innerText;
    GRNHeaderLine.locationcode = $('#lblLocationCode')[0].innerText;
    GRNHeaderLine.currencycode = $('#lblCurrencyCode')[0].innerText;
    GRNHeaderLine.orderno = $('#lblOrderNo')[0].innerText;
    GRNHeaderLine.purchasercode = $('#lblPurchaserCode')[0].innerText;
    GRNHeaderLine.contactname = $('#lblContactName')[0].innerText;

    GRNHeaderLine.postingdate = $('#txtPostingDate').val();
    GRNHeaderLine.documentdate = $('#txtDocumentDate').val();
    GRNHeaderLine.referenceinvoiceno = $('#txtVendorInvoiceNo').val();
    GRNHeaderLine.qcremarks = $('#txtQCRemarks').val();
    GRNHeaderLine.lrno = $('#txtLRRRNo').val();
    GRNHeaderLine.lrdate = $('#txtLRRRDate').val();
    GRNHeaderLine.transportername = $('#txtTransporterName').val();
    GRNHeaderLine.vehicleno = $('#txtVehicleNo').val();
    GRNHeaderLine.transporterno = $('#txtTransporterNo').val();
    GRNHeaderLine.transportationamount = $('#txtTransportAmount').val();
    GRNHeaderLine.loadingcharges = $('#txtLoadingCharges').val();
    GRNHeaderLine.unloadingcharges = $('#txtUnLoadingCharges').val();

    const txtQtyToReceive = document.querySelectorAll('input[name^="txtQtyToReceive_"]');

    txtQtyToReceive.forEach(input => {
        var GRNLine = {};

        var lineNo = input.name.replace('txtQtyToReceive_', '');
        const qcremarks = document.querySelector(`select[name="txtQCRemarks_${lineNo}"]`);
        const rejectqty = document.querySelector(`input[name="txtRejectQC_${lineNo}"]`);
        const beno = document.querySelector(`input[name="txtBillOfEntryNo_${lineNo}"]`);

        const bedate = document.querySelector(`input[name="txtBillOfEntryDate_${lineNo}"]`);
        const remarks = document.querySelector(`input[name="txtRemarks_${lineNo}"]`);
        const concentrationratepercent = document.querySelector(`input[name="txtConcentrationRatePercent_${lineNo}"]`);
        // Save MakeMfgname/MgfCode
        const manufacturerValue = $('#txtManufacturer_' + lineNo).val();
        let mfgcode = "";
        let makemfgname = "";
        if (manufacturerValue && manufacturerValue.includes(" - ")) {
            const parts = manufacturerValue.split(" - ");
            makemfgname = parts[1].trim();
            mfgcode = parts[0].trim();
        } else {
            makemfgname = manufacturerValue;
            mfgcode = $('#hdnManufacturerNo_' + lineNo).val();
        }
        GRNLine.makemfgname = makemfgname;
        GRNLine.mfgcode = mfgcode;


        GRNLine.documenttype = doctype;
        GRNLine.documentno = docnumber;
        GRNLine.lineno = lineNo;
        GRNLine.qtytoreceive = input.value;
        GRNLine.qcremarks = qcremarks ? qcremarks.value : "";
        GRNLine.rejectqty = rejectqty ? rejectqty.value : "";
        GRNLine.beno = beno ? beno.value : "";

        GRNLine.bedate = bedate ? bedate.value : null;
        GRNLine.remarks = remarks ? remarks.value : "";
        GRNLine.concentratepercent = concentrationratepercent ? concentrationratepercent.value : "";


        grnLines.push(GRNLine);
    });

    GRNHeaderLine.grnCardLineRequest = grnLines;

    (async () => {
        const rawResponse = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(GRNHeaderLine)
        });
        const res = await rawResponse.ok;
        if (res) {
            var actionMsg = "GRN Save Successfully.";
            ShowActionMsg(actionMsg);
            window.setTimeout(function () {

                window.location.href = '/SPGRN/GRNList';

            }, 2000);
        }
        else {
            var actionMsg = "Error in GRN Save process. Try again.";
            ShowErrMsg(actionMsg);
        }
    })();
}

function validateForm() {

    $('#lblMsg').html('');

    if ($('#txtPostingDate').val() == "") {
        $('#lblMsg').html('Select Posting Date');
        $('#txtPostingDate').focus();
        return false;
    }
    else if ($('#txtDocumentDate').val() == "") {
        $('#lblMsg').html('Select Document Date');
        $('#txtDocumentDate').focus();
        return false;
    }
    else if ($('#txtVendorInvoiceNo').val() == "") {
        $('#lblMsg').html('Enter Vendor Invoice No.');
        $('#txtVendorInvoiceNo').focus();
        return false;
    }
    else if ($('#txtQCRemarks').val() == "") {
        $('#lblMsg').html('Enter QC Remark.');
        $('#txtQCRemarks').focus();
        return false;
    }



    const txtQtyToReceive = document.querySelectorAll('input[name^="txtQtyToReceive_"]');
    var lineMsg = "";
    var QtyReceiveFlag = false;
    var RejectQtyFlag = fasle;
    txtQtyToReceive.forEach(input => {

        var lineNo = input.name.replace('txtQtyToReceive_', '');
        var qtytoreceive = input.value;
        const qcremarks = document.querySelector(`select[name="txtQCRemarks_${lineNo}"]`);
        const rejectqty = document.querySelector(`input[name="txtRejectQC_${lineNo}"]`);
        if (qtytoreceive == null || qtytoreceive == undefined || qtytoreceive == "") {
            QtyReceiveFlag = true;
        }

        if (qcremarks != null && rejectqty != null) {
            if (qcremarks.value == "Not Ok" && (rejectqty.value == null || rejectqty.value == "" || parseInt(rejectqty.value) == 0)) {
                RejectQtyFlag = true;
            }
        }
    });


    if (QtyReceiveFlag == true && RejectQtyFlag == true) {
        lineMsg = "Qty to receive required for all the line. if QC Remark selected as 'Not OK' then Rejecte Qty required.";
    }
    else if (QtyReceiveFlag == true && RejectQtyFlag == false) {
        lineMsg = "Qty to receive requried for all the line.";
    }
    else if (QtyReceiveFlag == false && RejectQtyFlag == true) {
        lineMsg = "if QC Remark selected as 'Not OK' then Rejecte Qty required.";
    }

    if (lineMsg != "") {
        $('#lblMsg').html(lineMsg);
        return false;
    }

    return true;
}

function ShowItemTracking(lineNo, itemno) {
    debugger;
    var documentType, documentNo;
    documentType = $('#lblDocumentType')[0].innerText;
    documentNo = $('#lblDocumentNo')[0].innerText;

    if (documentType != "" && documentNo != "") {
        $.ajax(
            {
                url: '/SPGRN/GetGRNLineItemTrackingForPopup?DocumentType=' + documentType + '&DocumentNo=' + documentNo + '&LineNo=' + lineNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    var rowData = "";
                    $('#tbItemTrackingLines').empty();
                    if (documentType == "Transfer Order" || documentType == "Sales Return") {
                        if (data != null && data != "" && data.length > 0) {
                            $.each(data, function (index, item) {
                                rowData = "<tr class='itemtrackingtr'><td style='display: none;'>" + item.Entry_No + "</td><td>" + lineNo + "</td><td>" + item.Item_No + "</td><td><input type='text' name='txtLotNo_" + (index + 1) + "' value='" + item.Lot_No + "' class='form-control' disabled /></td><td><input type='text' name='txtQtyToHandle_" + (index + 1) + "' value='" + item.Qty_to_Handle_Base + "' class='form-control' /></td><td><input type='text' name='txtExpDate_" + (index + 1) + "' value='" + item.Expiration_Date + "' class='form-control' disabled /></td><td>" + item.Quantity + "</td><td></td></tr>";
                                $('#tbItemTrackingLines').append(rowData);
                            });
                        }
                        else {
                            rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                            $('#tbItemTrackingLines').append(rowData);
                        }
                    }
                    else if (documentType == "Purchase Order") {

                        rowData = "<tr><td style='display: none;'>0</td><td>" + lineNo + "</td><td>" + itemno + "</td><td><input type='text' name='txtLotNo_0' value='' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_0' value='0' class='form-control' /></td><td><input type='text' name='txtExpDate_0' value='' class='form-control datepicker'/></td><td>0</td><td><button type='button' class='btn btn-primary btn-sm radius-30 px-4' onclick='AddItemTrackingInGrid();'>Add</button></td></tr>";
                        $('#tbItemTrackingLines').append(rowData);

                        if (data != null && data != "" && data.length > 0) {
                            $.each(data, function (index, item) {
                                rowData = "<tr class='itemtrackingtr'><td style='display: none;'>" + item.Entry_No + "</td><td>" + lineNo + "</td><td>" + item.Item_No + "</td><td><input type='text' name='txtLotNo_" + (index + 1) + "' value='" + item.Lot_No + "' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_" + (index + 1) + "' value='" + item.Qty_to_Handle_Base + "' class='form-control' /></td><td><input type='text' name='txtExpDate_" + (index + 1) + "' value='" + item.Expiration_Date + "' class='form-control datepicker' /></td><td>" + item.Quantity + "</td><td><button type='button' class='btn btn-primary btn-sm radius-30 px-4' onclick='DeleteItemTrackingInGrid();'>Delete</button></td></tr>";
                                $('#tbItemTrackingLines').append(rowData);
                            });
                        }
                        else {
                            rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                            $('#tbItemTrackingLines').append(rowData);
                        }
                    }
                    else {
                        rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                        $('#tbItemTrackingLines').append(rowData);
                    }

                    $('.datepicker').pickadate({
                        selectMonths: true,
                        selectYears: true,
                        format: 'dd-mm-yyyy'
                    });

                    $('#modalItemTracking').css('display', 'block');
                    $('.modal-title').text('Item Tracking Lines');
                    $('#dvItemTracking').css('display', 'block');
                },
                error: function () {
                    alert("error");
                }
            }
        );
    }
}

function SaveGRNItemTracking() {

    $('#lblItemTrackMsg').html('');

    var documenttype = $('#lblDocumentType')[0].innerText;
    var orderno = $('#lblDocumentNo')[0].innerText;
    var locationcode = $('#lblLocationCode')[0].innerText;

    var reservationEntryforGRN = [];

    $('#tbItemTrackingLines .itemtrackingtr').each(function () {
        var $tds = $(this).find('td');

        var entryno = $tds.eq(0).text().trim();
        var lineno = $tds.eq(1).text().trim();
        var itemno = $tds.eq(2).text().trim();
        var lotno = $tds.eq(3).find('input').val();
        var qtytohandle = $tds.eq(4).find('input').val();
        var expdate = $tds.eq(5).find('input').val();

        var reservationEntryforGRNObject = {};

        reservationEntryforGRNObject.DocumentType = documenttype;
        reservationEntryforGRNObject.OrderNo = orderno;
        reservationEntryforGRNObject.LocationCode = locationcode;

        reservationEntryforGRNObject.LineNo = lineno;
        reservationEntryforGRNObject.ItemNo = itemno;
        reservationEntryforGRNObject.LotNo = lotno;
        reservationEntryforGRNObject.Qty = qtytohandle;
        reservationEntryforGRNObject.ExpirationDate = expdate;
        reservationEntryforGRNObject.EntryNo = entryno;

        reservationEntryforGRN.push(reservationEntryforGRNObject);
    });

    var apiUrl = '/SPGRN/SaveGRNLineItemTracking';

    $.ajax({
        type: "POST",
        url: apiUrl,
        data: JSON.stringify(reservationEntryforGRN),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data)
                $('#modalItemTracking').css('display', 'none');
            else
                $('#lblItemTrackMsg').html('Error In Saving Data...');
        }
    });
}

function AddItemTrackingInGrid() {
    $('#tbItemTrackingLines tr:contains("No Records Found")').remove();
    var firstRow = $('#tbItemTrackingLines tr').first();
    var entryno = firstRow.find('td').eq(0).text().trim()
    var lineno = firstRow.find('td').eq(1).text().trim();
    var itemNo = firstRow.find('td').eq(2).text().trim();
    var lotNo = firstRow.find('input[name^="txtLotNo"]').val();
    var qty = firstRow.find('input[name^="txtQtyToHandle"]').val();
    var expDate = firstRow.find('input[name^="txtExpDate"]').val();
    var newIndex = $('#tbItemTrackingLines tr').length;
    var newRow = `<tr class='itemtrackingtr'>
      <td style='display: none;'>${entryno}</td>
      <td>${lineno}</td>
      <td>${itemNo}</td>
      <td><input type="text" name="txtLotNo_${newIndex}" value="${lotNo}" class="form-control"></td>
      <td><input type="text" name="txtQtyToHandle_${newIndex}" value="${qty}" class="form-control"></td>
      <td><input type="text" name="txtExpDate_${newIndex}" value="${expDate}" class="form-control datepicker"></td>
      <td>${qty}</td>
      <td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="DeleteItemTrackingInGrid();">Delete</button></td>
    </tr>`;
    $('#tbItemTrackingLines').append(newRow);

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    firstRow.find('input[type="text"]').val('');
}
function DeleteItemTrackingInGrid() {
    alert("Development under process..");
}
// Create Make/Mgf function 
function ManufacturerAutocompleteAPI(LineDetailsLineNo) {
    if (typeof ($.fn.autocomplete) === 'undefined') return;

    const $input = $("#" + LineDetailsLineNo);
    const lineNo = $input.data("lineno");
    const $hiddenInput = $("#hdnManufacturerNo_" + lineNo);
    const $loader = $("#loader_" + lineNo);
    const $spinner = $("#spinnerId_" + lineNo);

    if ($input.data("autocomplete-initialized")) {
        if ($input.val().length >= 2) {
            $input.autocomplete("search");
        }
        return;
    }

    $input.data("autocomplete-initialized", true);
    $input.autocomplete({
        serviceUrl: '/SPGRN/GetMakeMfgCodeAndName',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },

        onSearchStart: function () {
            $spinner.addClass("input-group");
            $loader.show();
        },
        transformResult: function (response) {
            try {
                $spinner.removeClass("input-group");
                $loader.hide();
                const parsed = $.parseJSON(response);
                return {
                    suggestions: $.map(parsed, function (item) {
                        return {
                            value: item.Name,
                            data: item.No
                        };
                    })
                };
            } catch (e) {
                console.error("Invalid autocomplete response", e);
                return { suggestions: [] };
            }
        },

        onSelect: function (suggestion) {
            $input.val(suggestion.value);
            $hiddenInput.val(suggestion.data);
            return false;
        },

        onShow: function () {
            setTimeout(() => {
                $input.focus();
            }, 10);
        }
    });

    $input.on('input', function () {
        $hiddenInput.val('');
    });
    if ($input.val().length >= 2) {
        $input.autocomplete("search");
    }
}
