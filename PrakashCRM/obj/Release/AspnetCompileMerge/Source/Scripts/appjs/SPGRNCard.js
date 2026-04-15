var apiUrl = $('#getServiceApiUrl').val() + 'SPGRN/';

$(document).ready(function () {

    InitializeGRNAttachmentUi();

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    ShowHideFields($('#lblDocumentType').html());
    $('#modalItemTracking .btn-close').click(function () {
        $('#modalItemTracking').css('display', 'none');
    });

    //input validate
    $("input[id^='txtQtyToReceive_']").on("change", function () {
        var lineNo = $(this).attr("id").split("_")[1]; // Extract LineNo from id
        var enteredQty = parseFloat($(this).val()) || 0;
        var actualQty = parseFloat($("#qty_" + lineNo).text()) || 0;
        var qcRemarks = $("#txtQCRemarks_" + lineNo).text();
        var rejectQC = parseFloat($("#txtRejectQC_" + lineNo).text()) || 0;

        if (enteredQty > actualQty) {
            ShowErrMsg("Entered quantity cannot be greater than available quantity (" + actualQty + ").");
            $(this).val(0);
        } else if (enteredQty < 0) {
            ShowErrMsg("Quantity cannot be negative.");
            $(this).val(0);
        }
    });
    $("input[id^='txtQtyToReceive_']").on("change", function () {
        var lineNo = $(this).attr("id").split("_")[1]; // Extract LineNo from id
        var enteredQty = parseFloat($(this).val()) || 0;
        var actualQty = parseFloat($("#qty_" + lineNo).text()) || 0;
        var qtyrecived = parseFloat($("#qtyrec_" + lineNo).text()) || 0;

        var remainingQty = actualQty - qtyrecived;
        if (enteredQty > remainingQty) {
            ShowErrMsg("Entered quantity cannot be greater than remaining quantity (" + remainingQty + ").");
            $(this).val(0);
            return;
        }
        $("#ShowItemTracking_" + lineNo).trigger("click");
    });

    $(document).on('click', '.btn-delete-itemtracking', function () {
        DeleteItemTrackingInGrid(this);
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
function getItemDescriptionByLineNo(lineNo) {
    var $qtyInput = $(`input[name='txtQtyToReceive_${lineNo}']`);
    if (!$qtyInput.length) return "Item";
    var desc = (($qtyInput.closest('tr').find('td').eq(1).text() || '').toString().trim());
    return desc || "Item";
}
function isTrackingQtyMatchedForLine(lineNo) {
    var qtyToReceive = parseFloat($(`input[name='txtQtyToReceive_${lineNo}']`).val()) || 0;
    var totalQty = parseFloat($('#txttotalQty').val()) || 0;
    var balanceQty = parseFloat($('#txtbalanceQty').val()) || 0;
    if (qtyToReceive <= 0 || totalQty <= 0) return false;
    if (Math.abs(balanceQty) > 0.0001) return false;
    return Math.abs(totalQty - qtyToReceive) <= 0.0001;
}
function showItemTrackingMismatchMessage(lineNo) {
    var itemDescription = getItemDescriptionByLineNo(lineNo);
    var msg = 'Total Qty. on Item Tracking Lines and Qty. to Receive in Product details for "' + itemDescription + '" does not match.';
    $('#lblItemTrackMsg').html('<span style="color: red;">' + msg + '</span>');
    //ShowErrMsg(msg);
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

function getItemTrackingSourceParams(documentType) {
    switch ((documentType || '').trim()) {
        case 'Purchase Order':
            return { sourceType: 39, sourceSubtype: '1' };
        case 'Sales Return':
            return { sourceType: 37, sourceSubtype: '5' };
        case 'Sales Order':
            return { sourceType: 37, sourceSubtype: '1' };
        case 'Transfer Order':
            return { sourceType: 5741, sourceSubtype: '1' };
        default:
            return { sourceType: '', sourceSubtype: '' };
    }
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
    if (!ValidateGRNData()) {
        return;
    }

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
        GRNLine.qcremarks = (qcremarks && qcremarks.value) ? qcremarks.value : "OK";
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


var deletedEntries = [];
function DeleteItemTrackingInGrid(button) {
    var $row = $(button).closest('tr');
    var rowKey = ResolveGRNAttachmentRowKey($row);
    var entryNo = $(button).closest('tr').find('td').eq(0).text().trim();

    var deletePayload = {
        entryno: entryNo,
        positive: true
    };

    $.ajax({
        type: "POST",
        url: '/SPGRN/DeleteGRNLineItemTracking',
        data: JSON.stringify(deletePayload),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data && data !== "") {
                // success handled if needed
            } else {
                ShowErrMsg("Error deleting Item Tracking line.");
            }
        },
        error: function (xhr) {
            ShowErrMsg("Error in DeleteGRNLineItemTracking API. " + xhr.responseText);
        }
    });

    // remove row from DOM
    $row.remove();
    delete grnAttachmentStore[rowKey];
    UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));

    // if no rows left, show "No Records Found"
    if ($('#tbItemTrackingLines .itemtrackingtr').length === 0) {
        $('#tbItemTrackingLines').append("<tr><td colspan=9>No Records Found</td></tr>");
    }
}

function ShowItemTracking(lineNo, itemno) {
    var documentType = $('#lblDocumentType').text().trim();
    var documentNo = $('#lblDocumentNo').text().trim();
    var sourceParams = getItemTrackingSourceParams(documentType);

    let qtyToReceive = parseFloat($(`input[name='txtQtyToReceive_${lineNo}']`).val()) || 0;

    if (qtyToReceive === 0) {
        ShowErrMsg("Quantity cannot be 0.");
        return false;
    }

    $('#modalItemTracking').data('lineno', lineNo);
    $('#txtbalanceQty').data('original', qtyToReceive);
    $('#txtbalanceQty').val(qtyToReceive.toFixed(2));
    $('#txttotalQty').val('0.00');

    if (documentType != "" && documentNo != "") {
        $.ajax({
            url: '/SPGRN/GetGRNLineItemTrackingForPopup?DocumentType=' + encodeURIComponent(documentType)
                + '&DocumentNo=' + encodeURIComponent(documentNo)
                + '&LineNo=' + encodeURIComponent(lineNo)
                + '&sourceType=' + encodeURIComponent(sourceParams.sourceType)
                + '&sourceSubtype=' + encodeURIComponent(sourceParams.sourceSubtype),
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                var rowData = "";
                $('#tbItemTrackingLines').empty();

                if (deletedEntries.length > 0) {
                    data = data.filter(item => !deletedEntries.includes(item.Entry_No.toString()));
                }

                if (documentType == "Transfer Order" || documentType == "Sales Return") {
                    if (data && data.length > 0) {
                        $.each(data, function (index, item) {
                            var attachmentKey = BuildGRNAttachmentRowKey(item.Entry_No, lineNo, item.Item_No);
                            rowData = `<tr class='itemtrackingtr' data-attachment-key='${attachmentKey}'><td style='display: none;'>${item.Entry_No}</td><td>${lineNo}</td><td>${item.Item_No}</td><td><input type='text' name='txtLotNo_${index + 1}' value='${item.Lot_No}' class='form-control' disabled /></td><td><input type='text' name='txtQtyToHandle_${index + 1}' value='${item.Qty_to_Handle_Base}' class='form-control' /></td><td><input type='text' name='txtExpDate_${index + 1}' value='${item.Expiration_Date}' class='form-control' disabled /></td><td>${item.Quantity}</td><td class='grn-attachment-cell'>${GetGRNAttachmentButtonHtml(attachmentKey, false)}</td><td></td>
                            </tr>`;
                            $('#tbItemTrackingLines').append(rowData);
                        });
                    } else {
                        rowData = "<tr><td colspan=9>No Records Found</td></tr>";
                        $('#tbItemTrackingLines').append(rowData);
                    }
                } else if (documentType == "Purchase Order") {
                    var draftAttachmentKey = BuildGRNAttachmentRowKey('0', lineNo, itemno, 'template');
                    rowData = `<tr class='itemtrackingtr' data-attachment-key='${draftAttachmentKey}'>
                        <td style='display: none;'>0</td><td>${lineNo}</td><td>${itemno}</td><td><input type='text' name='txtLotNo_0' value='' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_0' value='0' class='form-control' /></td><td><input type='text' name='txtExpDate_0' value='' class='form-control datepicker'/></td><td>0</td><td class='grn-attachment-cell'>${GetGRNAttachmentButtonHtml(draftAttachmentKey, true)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="AddItemTrackingInGrid();">Add</button></td>
                    </tr>`;
                    $('#tbItemTrackingLines').append(rowData);

                    if (data && data.length > 0) {
                        $.each(data, function (index, item) {
                            var purchaseAttachmentKey = BuildGRNAttachmentRowKey(item.Entry_No, lineNo, item.Item_No);
                            rowData = `<tr class='itemtrackingtr' data-attachment-key='${purchaseAttachmentKey}'><td style='display: none;'>${item.Entry_No}</td><td>${lineNo}</td><td>${item.Item_No}</td><td><input type='text' name='txtLotNo_${index + 1}' value='${item.Lot_No}' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_${index + 1}' value='${item.Qty_to_Handle_Base}' class='form-control' /></td><td><input type='text' name='txtExpDate_${index + 1}' value='${item.Expiration_Date}' class='form-control datepicker' /></td><td>${item.Quantity}</td><td class='grn-attachment-cell'>${GetGRNAttachmentButtonHtml(purchaseAttachmentKey, false)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="DeleteItemTrackingInGrid(this)">Delete</button></td>
                            </tr>`;
                            $('#tbItemTrackingLines').append(rowData);
                        });
                    }
                } else {
                    rowData = "<tr><td colspan=9>No Records Found</td></tr>";
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

                UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));
                if (!isTrackingQtyMatchedForLine(lineNo)) {
                    showItemTrackingMismatchMessage(lineNo);
                }

                $(document).off('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]').on('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]', function () {
                    UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));
                });

                UpdateGRNAttachmentButtons();
            },
            error: function () {
                ShowErrMsg("Error loading item tracking data. Please try again.");
            }
        });
    }
}


function UpdateItemTrackingTotals(originalBalance) {
    let total = 0;

    $('#lblItemTrackMsg').html('');

    $('#tbItemTrackingLines input[name^="txtQtyToHandle"]').each(function () {
        var inputName = (($(this).attr('name') || '') + '').trim();
        if (/_0$/.test(inputName)) {
            return;
        }
        let val = parseFloat($(this).val()) || 0;
        $(this).removeClass('is-invalid');
        total += val;
    });

    let balance = originalBalance - total;

    if (balance < 0) {
        $('#lblItemTrackMsg').html('<span style="color: red;">Balance quantity cannot be negative.</span>');
        balance = 0;
    }

    $('#txttotalQty').val(total.toFixed(2));
    $('#txtbalanceQty').val(balance.toFixed(2));
}


function SaveGRNItemTracking() {
    $('#lblItemTrackMsg').html('');
    var lineNo = $('#modalItemTracking').data('lineno');
    if (!lineNo) {
        lineNo = ($('#tbItemTrackingLines .itemtrackingtr').first().find('td').eq(1).text() || '').toString().trim();
    }

    let originalBalance = parseFloat($('#txtbalanceQty').data('original')) || 0;
    let total = parseFloat($('#txttotalQty').val()) || 0;

    if (total > originalBalance) {
        $('#lblItemTrackMsg').html('<span style="color: red;"></span>');
        return false;
    }
    if (!isTrackingQtyMatchedForLine(lineNo)) {
        showItemTrackingMismatchMessage(lineNo);
        return false;
    }

    var documenttype = $('#lblDocumentType').text().trim();
    var orderno = $('#lblDocumentNo').text().trim();
    var locationcode = $('#lblLocationCode').text().trim();

    var reservationEntryforGRN = [];
    var invalidQtyLineNo = "";

    $('#tbItemTrackingLines .itemtrackingtr').each(function () {
        var $tds = $(this).find('td');

        var entryno = $tds.eq(0).text().trim() || "0";
        var lineno = $tds.eq(1).text().trim();
        var itemno = $tds.eq(2).text().trim();
        var lotno = $tds.eq(3).find('input').val() || "";
        var $qtyInput = $tds.eq(4).find('input');
        var qtyInputName = (($qtyInput.attr('name') || '') + '').trim();
        var isZeroIndexRow = /_0$/.test(qtyInputName);
        var qtytohandle = parseFloat($tds.eq(4).find('input').val()) || 0;
        var expdate = $tds.eq(5).find('input').val() || "";
        var isTemplateRow = (($tds.eq(8).find('button').attr('onclick') || '').indexOf('AddItemTrackingInGrid') !== -1);

        if (!lineno) {
            return;
        }
        if (isZeroIndexRow) {
            return;
        }
        if (!isTemplateRow && qtytohandle <= 0) {
            invalidQtyLineNo = lineno;
            return false;
        }
        if (qtytohandle <= 0) {
            return;
        }
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
    if (invalidQtyLineNo !== "") {
        ShowErrMsg("0 and negative quantity are not allowed in tracking lines.");
        return false;
    }

    /* if (reservationEntryforGRN.length === 0) {
         $('#lblItemTrackMsg').html('<span style="color: red;">No valid item tracking lines to save.</span>');
         return false;
     }*/

    $.ajax({
        type: "POST",
        url: '/SPGRN/SaveGRNLineItemTracking',
        data: JSON.stringify(reservationEntryforGRN),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data) {
                $('#modalItemTracking').hide();
                ShowActionMsg("Item Tracking lines saved successfully.");
                $('#txttotalQty').val('0.00');
                $('#txtbalanceQty').val($('#txtbalanceQty').data('original').toFixed(2));
            } else {
                $('#lblItemTrackMsg').html('<span style="color: red;">Error saving Item Tracking data.</span>');
            }
        },
        error: function (xhr) {
            ShowErrMsg("Error in SaveGRNLineItemTracking API. " + xhr.responseText);
        }
    });
}

function AddItemTrackingInGrid() {
    $('#tbItemTrackingLines tr:contains("No Records Found")').remove();
    var firstRow = $('#tbItemTrackingLines tr').first();
    // declaration the variable item tracking Document attcement
    var sourceAttachmentKey = ResolveGRNAttachmentRowKey(firstRow);
    var entryno = firstRow.find('td').eq(0).text().trim()
    let originalBalance = parseFloat($('#txtbalanceQty').data('original')) || 0;
    let totalQty = parseFloat($('#txttotalQty').val()) || 0;

    if (totalQty > originalBalance) {
        ShowActionMsg("Total quantity cannot exceed Balance Quantity. Cannot add more Qty.");
        return false;
    }
    var lineno = firstRow.find('td').eq(1).text().trim();
    var itemNo = firstRow.find('td').eq(2).text().trim();
    var lotNo = firstRow.find('input[name^="txtLotNo"]').val();
    var qty = parseFloat(firstRow.find('input[name^="txtQtyToHandle"]').val()) || 0;
    if (qty <= 0) {
        ShowErrMsg("0 and negative quantity are not allowed in tracking lines.");
        return false;
    }
    var expDate = firstRow.find('input[name^="txtExpDate"]').val();
    var newIndex = $('#tbItemTrackingLines tr').length;
    var newAttachmentbutton = BuildGRNAttachmentRowKey(entryno, lineno, itemNo, 'draft-' + NextGRNAttachmentId());

    // Document Attachement code
    if (sourceAttachmentKey && grnAttachmentStore[sourceAttachmentKey]) {
        grnAttachmentStore[newAttachmentbutton] = CloneGRNAttachmentFiles(grnAttachmentStore[sourceAttachmentKey]);
        delete grnAttachmentStore[sourceAttachmentKey];

        if (grnAttachmentState.rowKey === sourceAttachmentKey) {
            grnAttachmentState.rowKey = newAttachmentbutton;
            grnAttachmentState.files = CloneGRNAttachmentFiles(grnAttachmentStore[newAttachmentbutton]);
        }
    }

    var newRow = `<tr class='itemtrackingtr' data-attachment-key='${newAttachmentbutton}'><td style='display: none;'>${entryno}</td><td>${lineno}</td><td>${itemNo}</td><td><input type="text" name="txtLotNo_${newIndex}" value="${lotNo}" class="form-control"></td><td><input type="text" name="txtQtyToHandle_${newIndex}" value="${qty}" class="form-control"></td><td><input type="text" name="txtExpDate_${newIndex}" value="${expDate}" class="form-control datepicker"></td><td>${qty}</td><td class='grn-attachment-cell'>${GetGRNAttachmentButtonHtml(newAttachmentbutton, false)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="DeleteItemTrackingInGrid(this)">Delete</button></td>
    </tr>`;
    $('#tbItemTrackingLines').append(newRow);

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    firstRow.find('input[type="text"]').val('');
    UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));

    $(document).off('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]').on('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]', function () {
        UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));
    });

    UpdateGRNAttachmentButtons();
}

// Create Make/Mfg function 
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
function ValidateGRNData() {

    let headerQCrem = $('#txtQCRemarks').val();
    let docflag = $("#hfimportDoc").val();

    const txtQtyToReceive = document.querySelectorAll('input[name^="txtQtyToReceive_"]');

    for (let input of txtQtyToReceive) {

        var lineNo = input.name.replace('txtQtyToReceive_', '');

        const qcremarks = document.querySelector(`select[name="txtQCRemarks_${lineNo}"]`);
        const rejectqty = document.querySelector(`input[name="txtRejectQC_${lineNo}"]`);

        let qtyToReceive = parseFloat(input.value) || 0;
        let rejectQty = parseFloat(rejectqty ? rejectqty.value : 0) || 0;
        let qcRemarkVal = qcremarks ? qcremarks.value : "";

        if (docflag == "False") {

            // QC = OK
            if (qcRemarkVal === "OK") {

                if (rejectQty > 0) {
                    ShowErrMsg("Reject Qty should be 0 when QC Remark is OK.");
                    return false;
                }
            }

            // QC = NOT OK
            if (qcRemarkVal === "Not OK") {

                if (headerQCrem == null || headerQCrem.trim() === "") {
                    ShowErrMsg("QC Remarks in header must not be blank.");
                    return false;
                }
                if (rejectqty.value === "" || rejectQty === 0) {
                    ShowErrMsg("When QC Remarks are 'Not Ok', Reject Qty. should not be '0'");
                    return false;
                }

                if (rejectQty > qtyToReceive) {
                    ShowErrMsg("Reject Qty. should not be greater than Qty. to Receive.");
                    return false;
                }

            }
        }
        //T
        if (docflag == "True") {

            // QC = OK
            if (qcRemarkVal === "OK") {

                if (rejectQty > 0) {
                    ShowErrMsg("Reject Qty should be 0 when QC Remark is OK.");
                    return false;
                }
            }
            // QC = NOT OK
            if (qcRemarkVal === "Not OK") {

                if (headerQCrem == null || headerQCrem.trim() === "") {
                    ShowErrMsg("QC Remarks in header must not be blank.");
                    return false;
                }

                if (rejectQty > 0) {
                    ShowErrMsg("Reject Qty should be 0 when QC Remark is OK.");
                    return false;
                }

            }
        }

    }

    return true;
}
//Start Function
// Create the GRN Document Attchment Function 
var grnAttachmentStore = {};
var grnAttachmentState = {rowKey: '', rowContext: null, files: [], viewerFiles: [], viewerIndex: 0, viewerZoom: 1, viewerMode: 'single'};
var grnAttachmentIdSeed = 0;

function InitializeGRNAttachmentUi() {
    var $overlay = $('#grnAttachmentOverlay');
    if (!$overlay.length || $overlay.data('initialized')) {
        return;
    }

    $overlay.data('initialized', true);

    $('#grnAttachmentDropZone').on('click', function (e) {
        if ($(e.target).closest('button').length > 0) {
            return;
        }
        OpenGRNAttachmentFilePicker();
    });

    $('#grnAttachmentFileInput').on('change', function () {
        HandleGRNAttachmentFiles(this.files);
        this.value = '';
    });

    $('#grnAttachmentDropZone').on('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('dragover');
    });

    $('#grnAttachmentDropZone').on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
    });

    $('#grnAttachmentDropZone').on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('dragover');
        var files = e.originalEvent && e.originalEvent.dataTransfer ? e.originalEvent.dataTransfer.files : null;
        HandleGRNAttachmentFiles(files);
    });

    $overlay.on('click', function (e) {
        if (e.target === this) {
            CloseGRNAttachmentPanel();
        }
    });

    $('#grnAttachmentViewer').on('click', function (e) {
        if (e.target === this) {
            CloseGRNAttachmentViewer();
        }
    });

    $(document).on('keydown.grnAttachmentViewer', function (e) {
        if ($('#grnAttachmentViewer').css('display') === 'none') {
            return;
        }

        if (e.key === 'ArrowRight') {
            GRNViewerNext();
        }
        else if (e.key === 'ArrowLeft') {
            GRNViewerPrev();
        }
        else if (e.key === 'Escape') {
            CloseGRNAttachmentViewer();
        }
    });
}

function NextGRNAttachmentId() {
    grnAttachmentIdSeed += 1;
    return 'grn-att-' + new Date().getTime() + '-' + grnAttachmentIdSeed;
}
function CloneGRNAttachmentFiles(files) {
    return $.map(files || [], function (file) {
        return $.extend({}, file);
    });
}

function EncodeGRNAttachmentValue(value) {
    return $('<div/>').text(value || '').html();
}

function GetGRNAttachmentExtension(fileName) {
    if (!fileName || fileName.indexOf('.') === -1) {
        return '';
    }

    return fileName.split('.').pop().toLowerCase();
}

function IsGRNAttachmentImage(ext) {
    return ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg'].indexOf((ext || '').toLowerCase()) >= 0;
}

function FormatGRNAttachmentSize(size) {
    var fileSize = parseFloat(size) || 0;
    var units = ['B', 'KB', 'MB', 'GB'];
    var unitIndex = 0;

    while (fileSize >= 1024 && unitIndex < units.length - 1) {
        fileSize = fileSize / 1024;
        unitIndex += 1;
    }

    var formatted = unitIndex === 0 ? fileSize.toFixed(0) : fileSize.toFixed(fileSize >= 10 ? 1 : 2);
    return formatted + ' ' + units[unitIndex];
}

function GetCurrentGRNAttachmentDocumentContext() {
    return {
        documentType: ($('#lblDocumentType').text() || '').trim(),
        documentNo: ($('#lblDocumentNo').text() || '').trim()
    };
}

function BuildGRNAttachmentRowKey(entryNo, lineNo, itemNo, fallbackKey) {
    var context = GetCurrentGRNAttachmentDocumentContext();
    var cleanEntryNo = (entryNo || '').toString().trim();
    if (cleanEntryNo !== '' && cleanEntryNo !== '0') {
        return ['entry', context.documentType, context.documentNo, lineNo, cleanEntryNo].join('|');
    }

    return ['draft', context.documentType, context.documentNo, lineNo, itemNo, fallbackKey || NextGRNAttachmentId()].join('|');
}

function IsGRNTemplateTrackingRow($row) {
    var $actionButton = $row.find('td').eq(8).find('button');
    return ($actionButton.attr('onclick') || '').indexOf('AddItemTrackingInGrid') !== -1;
}

function ResolveGRNAttachmentRowKey($row) {
    var existingKey = ($row.attr('data-attachment-key') || '').trim();
    if (existingKey !== '') {
        return existingKey;
    }

    var $cells = $row.find('td');
    var entryNo = ($cells.eq(0).text() || '').trim();
    var lineNo = ($cells.eq(1).text() || '').trim();
    var itemNo = ($cells.eq(2).text() || '').trim();
    var fallbackKey = IsGRNTemplateTrackingRow($row) ? 'template' : NextGRNAttachmentId();
    var rowKey = BuildGRNAttachmentRowKey(entryNo, lineNo, itemNo, fallbackKey);

    $row.attr('data-attachment-key', rowKey);
    return rowKey;
}

function GetGRNAttachmentRowContext($row) {
    var $cells = $row.find('td');
    var lotNo = $.trim($cells.eq(3).find('input').val() || $cells.eq(3).text() || '');

    return {
        entryNo: ($cells.eq(0).text() || '').trim(), lineNo: ($cells.eq(1).text() || '').trim(), itemNo: ($cells.eq(2).text() || '').trim(), lotNo: lotNo, rowKey: ResolveGRNAttachmentRowKey($row)
    };
}

function GetGRNAttachmentCount(rowKey) {
    return (grnAttachmentStore[rowKey] || []).length;
}

function GetGRNAttachmentButtonHtml(rowKey, disabled) {

    var count = GetGRNAttachmentCount(rowKey);
    var iconColor = disabled ? '#141517' : (count > 0 ? '#0d6efd' : '#6c757d');
    var countMarkup = count > 0 ? '<span class="badge bg-primary ms-1">' + count + '</span>' : '';

        // this enable the variable attement button has been disable 
    var disabledClass = disabled ? 'disabled' : '';
    var disabledAttr = disabled ? 'disabled' : '';

    return '<button type="button" ' + "" + ' class="btn btn-link p-0 d-inline-flex align-items-center ' + "" + '" ' + ' title="Line attachments" onclick="OpenGRNAttachmentPanel(this)">' + GetGRNAttachmentPaperclipSvg(iconColor) + countMarkup + '</button>';
}
// SVG Icon
function GetGRNAttachmentPaperclipSvg(color) {
    return '<svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="none" ' + 'stroke="' + color + '" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" ' + 'class="me-1">' + '<path d="M21.44 11.05l-8.49 8.49a5.5 5.5 0 01-7.78-7.78l8.49-8.49a3.5 3.5 0 114.95 4.95l-8.49 8.49a1.5 1.5 0 11-2.12-2.12l7.78-7.78"/>' + '</svg>';
}

function UpdateGRNAttachmentButtons() {
    $('#tbItemTrackingLines .itemtrackingtr').each(function () {
        var $row = $(this);
        var $attachmentCell = $row.find('td').eq(7);
        if (!$attachmentCell.length) {
            return;
        }

        var rowKey = ResolveGRNAttachmentRowKey($row);
        $attachmentCell.html(GetGRNAttachmentButtonHtml(rowKey, IsGRNTemplateTrackingRow($row)));
    });
}

function OpenGRNAttachmentPanel(button) {
    var $row = $(button).closest('tr');
    if (!$row.length || IsGRNTemplateTrackingRow($row)) {
        return;
    }

    var context = GetGRNAttachmentRowContext($row);
    // Add the validation for GRN Item tracking Document Attachment
    if (!context.lotNo) {
        ShowErrMsg("Please enter Lot No. before Click attachment button.");
        $row.find('td').eq(3).find('input[name^="txtLotNo"]').focus();
        return;
    }

    grnAttachmentState.rowKey = context.rowKey;
    grnAttachmentState.rowContext = context;
    grnAttachmentState.files = CloneGRNAttachmentFiles(grnAttachmentStore[context.rowKey]);

    $('#grnAttachmentError').text('');
    $('body').addClass('grn-attachment-open');
    $('#grnAttachmentOverlay').css('display', 'flex');
    RenderGRNAttachmentFileList();
}

function CloseGRNAttachmentPanel() {
    $('#grnAttachmentOverlay').hide();
    $('#grnAttachmentError').text('');
    SetGRNAttachmentSaveState(false); // call Save Function
    grnAttachmentState.rowKey = '';
    grnAttachmentState.rowContext = null;
    grnAttachmentState.files = [];
    if ($('#grnAttachmentViewer').css('display') === 'none') {
        $('body').removeClass('grn-attachment-open');
    }
}

async function SaveGRNAttachmentPanel() {
    if (!grnAttachmentState.rowKey) {
        CloseGRNAttachmentPanel();
        return;
    }

    var currentContext = RefreshGRNAttachmentRowContext(grnAttachmentState.rowKey);
    if (!currentContext || !currentContext.itemNo) {
        $('#grnAttachmentError').text('Unable to resolve the selected row for attachment upload.');
        return;
    }

    if (!currentContext.lotNo) {
        $('#grnAttachmentError').text('Lot No is required before saving attachments.');
        return;
    }

    try {
        SetGRNAttachmentSaveState(true);
        $('#grnAttachmentError').text('');

        var pendingFiles = $.grep(grnAttachmentState.files, function (file) {
            return !file.isUploaded && file.rawFile;
        });

        if (pendingFiles.length > 0) {
            var uploadedFiles = await UploadGRNAttachmentFiles(pendingFiles, currentContext);

            $.each(pendingFiles, function (index, pendingFile) {
                var uploadedFile = uploadedFiles[index] || {};
                pendingFile.isUploaded = true;
                pendingFile.rawFile = null;
                pendingFile.base64 = uploadedFile.Base64 || '';
                pendingFile.contentType = uploadedFile.ContentType || pendingFile.contentType || '';
                pendingFile.fileExtension = uploadedFile.FileExtension || pendingFile.fileExtension || '';
                pendingFile.serverFileName = uploadedFile.FileName || '';
                pendingFile.lotNo = uploadedFile.LotNo || currentContext.lotNo;
                pendingFile.itemNo = uploadedFile.ItemNo || currentContext.itemNo;
            });
        }

        if (grnAttachmentState.files.length > 0) {
            grnAttachmentStore[grnAttachmentState.rowKey] = CloneGRNAttachmentFiles(grnAttachmentState.files);
        }
        else {
            delete grnAttachmentStore[grnAttachmentState.rowKey];
        }

        UpdateGRNAttachmentButtons();
        ShowActionMsg(pendingFiles.length > 0 ? pendingFiles.length + ' attachment(s) uploaded successfully.' : 'Attachment list updated successfully.');
        CloseGRNAttachmentPanel();
    }
    catch (error) {
        $('#grnAttachmentError').text(error && error.message ? error.message : 'Unable to save attachments.');
    }
    finally {
        SetGRNAttachmentSaveState(false);
    }
}

function OpenGRNAttachmentFilePicker() {
    $('#grnAttachmentFileInput').trigger('click');
}

function HandleGRNAttachmentFiles(fileList) {
    if (!fileList || !fileList.length) {
        return;
    }

    var mappedFiles = $.map(fileList, function (file) {
        return {
            id: NextGRNAttachmentId(),
            name: file.name,
            size: file.size || 0,
            ext: GetGRNAttachmentExtension(file.name),
            url: window.URL ? window.URL.createObjectURL(file) : '',
            uploadedAt: new Date().toLocaleString('en-IN'),
            rawFile: file,
            isUploaded: false,
            base64: '',
            contentType: file.type || '',
            fileExtension: GetGRNAttachmentExtension(file.name) ? '.' + GetGRNAttachmentExtension(file.name) : '',
            serverFileName: '',
            lotNo: '',
            itemNo: ''
        };
    });

    grnAttachmentState.files = grnAttachmentState.files.concat(mappedFiles);
    $('#grnAttachmentError').text('');
    RenderGRNAttachmentFileList();
}

function RemoveGRNAttachmentFile(fileId) {
    grnAttachmentState.files = $.grep(grnAttachmentState.files, function (file) {
        return file.id !== fileId;
    });

    RenderGRNAttachmentFileList();
}

function RenderGRNAttachmentFileList() {
    var context = grnAttachmentState.rowContext;
    var fileCount = grnAttachmentState.files.length;
    var metaParts = [];

    if (context) {
        metaParts.push('Line ' + (context.lineNo || ''));
        if (context.itemNo) {
            metaParts.push(context.itemNo);
        }
        if (context.lotNo) {
            metaParts.push('Lot ' + context.lotNo);
        }
    }
    metaParts.push(fileCount + ' file' + (fileCount === 1 ? '' : 's'));

    $('#grnAttachmentMeta').text(metaParts.join(' • '));
    $('#grnAttachmentSaveCount').text(fileCount);

    var $list = $('#grnAttachmentFileList');
    $list.empty();

    if (fileCount === 0) {
        $list.append('<div class="grn-attachment-empty">No attachments yet. Upload files above.</div>');
        return;
    }

    $.each(grnAttachmentState.files, function (index, file) {
        var previewMarkup = IsGRNAttachmentImage(file.ext)
            ? '<img src="' + EncodeGRNAttachmentValue(file.url) + '" alt="' + EncodeGRNAttachmentValue(file.name) + '" style="width:44px;height:44px;object-fit:cover;border-radius:10px;cursor:pointer;" onclick="OpenGRNAttachmentViewer(' + index + ')">'
            : '<div class="grn-attachment-file-badge" style="cursor:pointer;" onclick="OpenGRNAttachmentViewer(' + index + ')">' + EncodeGRNAttachmentValue((file.ext || 'file').toUpperCase()) + '</div>';

        var uploadStatus = file.isUploaded ? 'Uploaded' : 'Pending upload';
        var rowHtml = '<div class="grn-attachment-file-row">' + '<div class="grn-attachment-file-main">' + previewMarkup + '<div style="min-width:0;">' + '<span class="grn-attachment-file-name" title="' + EncodeGRNAttachmentValue(file.name) + '">' + EncodeGRNAttachmentValue(file.name) + '</span>' + '<span class="grn-attachment-file-meta">' + EncodeGRNAttachmentValue(FormatGRNAttachmentSize(file.size)) + ' • ' + EncodeGRNAttachmentValue(file.uploadedAt) + ' • ' + EncodeGRNAttachmentValue(uploadStatus) + '</span>' + '</div>' + '</div>' + '<div class="d-flex align-items-center gap-2">' + '<button type="button" class="btn btn-sm btn-outline-secondary" onclick="OpenGRNAttachmentViewer(' + index + ')">View</button>' + '<button type="button" class="btn btn-sm btn-outline-danger" onclick="RemoveGRNAttachmentFile(\'' + EncodeGRNAttachmentValue(file.id) + '\')">Remove</button>' + '</div>' + '</div>';

        $list.append(rowHtml);
    });
}

function FindGRNAttachmentRowByKey(rowKey) {
    return $('#tbItemTrackingLines .itemtrackingtr').filter(function () {
        return ($(this).attr('data-attachment-key') || '').trim() === rowKey;
    }).first();
}

function RefreshGRNAttachmentRowContext(rowKey) {
    var $row = FindGRNAttachmentRowByKey(rowKey);
    if (!$row.length) {
        return grnAttachmentState.rowContext;
    }

    var context = GetGRNAttachmentRowContext($row);
    grnAttachmentState.rowContext = context;
    return context;
}

function SetGRNAttachmentSaveState(isSaving) {
    var $saveButton = $('.grn-attachment-save-btn');
    $saveButton.prop('disabled', isSaving);
    $saveButton.css('opacity', isSaving ? '0.75' : '1');
}

async function UploadGRNAttachmentFiles(files, context) {
    var uploadUrl = '/SPGRN/UploadGRNDocumentAttachment';
    var uploadedFiles = [];

    if (typeof window.FormData === 'undefined') {
        throw new Error('This browser does not support file uploads. Please use a modern browser.');
    }

    for (var index = 0; index < files.length; index += 1) {
        var file = files[index];
        var formData = new FormData();

        formData.append('files', file.rawFile, file.name);
        formData.append('lotNo', context.lotNo || '');
        formData.append('itemNo', context.itemNo || '');
        formData.append('FileName', file.name || '');

        var responseBody = await UploadGRNAttachmentRequest(uploadUrl, formData);
        var currentUploadedFiles = $.isArray(responseBody) ? responseBody : [responseBody];

        if (!currentUploadedFiles.length || !currentUploadedFiles[0]) {
            throw new Error('Attachment upload response was empty.');
        }

        uploadedFiles.push(currentUploadedFiles[0]);
    }

    return uploadedFiles;
}

function UploadGRNAttachmentRequest(uploadUrl, formData) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            url: uploadUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (data) {
                resolve(data);
            },
            error: function (xhr) {
                if (xhr && xhr.status === 401) {
                    reject(new Error('Session expired. Please login again.'));
                    return;
                }

                var responseText = xhr && xhr.responseText ? xhr.responseText : '';
                if (responseText.indexOf('<!DOCTYPE html') >= 0 || responseText.indexOf('<html') >= 0) {
                    reject(new Error('Upload request was redirected. Please refresh the page and login again.'));
                    return;
                }

                var responseJson = xhr && xhr.responseJSON ? xhr.responseJSON : null;
                if (!responseJson && responseText) {
                    try {
                        responseJson = JSON.parse(responseText);
                    }
                    catch (e) {
                        responseJson = null;
                    }
                }

                var errorMessage = responseJson && (responseJson.Message || responseJson.message || responseJson.error || responseJson.details)
                    ? (responseJson.Message || responseJson.message || responseJson.error || responseJson.details)
                    : (responseText || 'Attachment upload failed.');

                reject(new Error(errorMessage));
            }
        });
    });
}

function OpenGRNAttachmentViewer(index) {
    if (!grnAttachmentState.files.length) {
        return;
    }

    grnAttachmentState.viewerFiles = CloneGRNAttachmentFiles(grnAttachmentState.files);
    grnAttachmentState.viewerIndex = Math.max(0, Math.min(index || 0, grnAttachmentState.viewerFiles.length - 1));
    grnAttachmentState.viewerZoom = 1;
    grnAttachmentState.viewerMode = 'single';

    $('#grnAttachmentViewer').css('display', 'flex');
    $('body').addClass('grn-attachment-open');
    RenderGRNAttachmentViewer();
}

function CloseGRNAttachmentViewer() {
    $('#grnAttachmentViewer').hide();
    grnAttachmentState.viewerFiles = [];
    grnAttachmentState.viewerIndex = 0;
    grnAttachmentState.viewerZoom = 1;
    grnAttachmentState.viewerMode = 'single';

    if ($('#grnAttachmentOverlay').css('display') === 'none') {
        $('body').removeClass('grn-attachment-open');
    }
}

function GRNViewerPrev() {
    if (!grnAttachmentState.viewerFiles.length) {
        return;
    }

    grnAttachmentState.viewerZoom = 1;
    grnAttachmentState.viewerIndex = (grnAttachmentState.viewerIndex - 1 + grnAttachmentState.viewerFiles.length) % grnAttachmentState.viewerFiles.length;
    RenderGRNAttachmentViewer();
}

function GRNViewerNext() {
    if (!grnAttachmentState.viewerFiles.length) {
        return;
    }

    grnAttachmentState.viewerZoom = 1;
    grnAttachmentState.viewerIndex = (grnAttachmentState.viewerIndex + 1) % grnAttachmentState.viewerFiles.length;
    RenderGRNAttachmentViewer();
}

function GRNViewerZoomIn() {
    UpdateGRNAttachmentViewerZoom(0.25);
}

function GRNViewerZoomOut() {
    UpdateGRNAttachmentViewerZoom(-0.25);
}

function GRNViewerResetZoom() {
    grnAttachmentState.viewerZoom = 1;
    ApplyGRNAttachmentViewerZoom();
}

function ToggleGRNViewerMode() {
    grnAttachmentState.viewerMode = grnAttachmentState.viewerMode === 'single' ? 'grid' : 'single';
    RenderGRNAttachmentViewer();
}

function UpdateGRNAttachmentViewerZoom(delta) {
    grnAttachmentState.viewerZoom = Math.max(0.25, Math.min(4, grnAttachmentState.viewerZoom + delta));
    ApplyGRNAttachmentViewerZoom();
}

function ApplyGRNAttachmentViewerZoom() {
    $('#grnViewerImage').css('transform', 'scale(' + grnAttachmentState.viewerZoom + ')');
    $('#grnViewerZoomLabel').text(Math.round(grnAttachmentState.viewerZoom * 100) + '%');
}

function RenderGRNAttachmentViewer() {
    var files = grnAttachmentState.viewerFiles;
    var total = files.length;
    var file = total > 0 ? files[grnAttachmentState.viewerIndex] : null;
    var isSingleMode = grnAttachmentState.viewerMode === 'single';
    var hasImagePreview = file && IsGRNAttachmentImage(file.ext);

    $('#grnViewerCounter').text(total > 0 ? (grnAttachmentState.viewerIndex + 1) + ' / ' + total : '');
    $('#grnViewerFileName').text(file ? file.name : '');
    $('#grnViewerDownloadLink').attr('href', file ? file.url : '#').attr('download', file ? file.name : '');
    $('#grnViewerModeButton').text(isSingleMode ? 'Grid' : 'Single');

    $('#grnViewerSingle').toggle(isSingleMode);
    $('#grnViewerGrid').css('display', isSingleMode ? 'none' : 'grid');
    $('#grnViewerThumbStrip').toggle(isSingleMode && total > 1);
    $('.grn-viewer-nav').toggle(total > 1 && isSingleMode);
    $('#grnViewerZoomLabel').toggle(isSingleMode && hasImagePreview);
    $('#grnViewerZoomLabel').prev('button').toggle(isSingleMode && hasImagePreview);
    $('#grnViewerZoomLabel').next('button').toggle(isSingleMode && hasImagePreview);
    $('#grnViewerModeButton').prev('button').toggle(isSingleMode && hasImagePreview);

    if (isSingleMode && file) {
        if (hasImagePreview) {
            $('#grnViewerImage').attr('src', file.url).show();
            $('#grnViewerNoPreview').hide();
            ApplyGRNAttachmentViewerZoom();
        }
        else {
            $('#grnViewerImage').hide().attr('src', '');
            $('#grnViewerNoPreview').show();
            $('#grnViewerZoomLabel').text('100%');
        }

        RenderGRNAttachmentViewerThumbStrip();
    }
    else {
        $('#grnViewerImage').hide().attr('src', '');
        $('#grnViewerNoPreview').hide();
    }

    if (!isSingleMode) {
        RenderGRNAttachmentViewerGrid();
    }
}

function RenderGRNAttachmentViewerThumbStrip() {
    var $strip = $('#grnViewerThumbStrip');
    $strip.empty();

    $.each(grnAttachmentState.viewerFiles, function (index, file) {
        var thumbHtml = IsGRNAttachmentImage(file.ext)
            ? '<img src="' + EncodeGRNAttachmentValue(file.url) + '" alt="' + EncodeGRNAttachmentValue(file.name) + '">'
            : EncodeGRNAttachmentValue((file.ext || 'FILE').toUpperCase());
        var $thumb = $('<div class="grn-viewer-thumb"></div>');
        $thumb.toggleClass('active', index === grnAttachmentState.viewerIndex);
        $thumb.html(thumbHtml);
        $thumb.on('click', function () {
            grnAttachmentState.viewerIndex = index;
            grnAttachmentState.viewerZoom = 1;
            RenderGRNAttachmentViewer();
        });
        $strip.append($thumb);
    });
}

function RenderGRNAttachmentViewerGrid() {
    var $grid = $('#grnViewerGrid');
    $grid.empty();

    $.each(grnAttachmentState.viewerFiles, function (index, file) {
        var thumbHtml = IsGRNAttachmentImage(file.ext)
            ? '<img src="' + EncodeGRNAttachmentValue(file.url) + '" alt="' + EncodeGRNAttachmentValue(file.name) + '">'
            : '<div class="grn-attachment-file-badge" style="width:72px;height:72px;font-size:14px;">' + EncodeGRNAttachmentValue((file.ext || 'FILE').toUpperCase()) + '</div>';
        var cardHtml = '<div class="grn-viewer-grid-thumb">' + thumbHtml + '</div>'
            + '<div class="grn-viewer-grid-name" title="' + EncodeGRNAttachmentValue(file.name) + '">' + EncodeGRNAttachmentValue(file.name) + '</div>';
        var $card = $('<div class="grn-viewer-grid-card"></div>');
        $card.toggleClass('active', index === grnAttachmentState.viewerIndex);
        $card.html(cardHtml);
        $card.on('click', function () {
            grnAttachmentState.viewerIndex = index;
            grnAttachmentState.viewerMode = 'single';
            grnAttachmentState.viewerZoom = 1;
            RenderGRNAttachmentViewer();
        });
        $grid.append($card);
    });
}

function GetGRNAttachmentPaperclipSvg(color) {
    return '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">'
        + '<path d="M8.75 12.25L14.9 6.1C16.27 4.73 18.5 4.73 19.87 6.1C21.24 7.47 21.24 9.7 19.87 11.07L11.07 19.87C8.91 22.03 5.42 22.03 3.26 19.87C1.1 17.71 1.1 14.22 3.26 12.06L11.72 3.6" stroke="' + color + '" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>'
        + '<path d="M7.54 16.4L15.96 7.98" stroke="' + color + '" stroke-width="2.2" stroke-linecap="round" opacity="0.9"/>'
        + '</svg>';
}
// End Function