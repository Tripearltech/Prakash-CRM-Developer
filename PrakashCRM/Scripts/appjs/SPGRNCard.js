var apiUrl = $('#getServiceApiUrl').val() + 'SPGRN/';
var GRN_ATTACHMENT_TABLE_ID = 6505;
var grnAttachmentState = {
    rows: {},
    currentRowKey: '',
    viewerFiles: [],
    viewerIndex: 0,
    viewerZoom: 1,
    viewerMode: 'single'
};
var grnAttachmentSequence = 0;

$(document).ready(function () {

    EnsureGRNAttachmentLayers();

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    ShowHideFields($('#lblDocumentType').html());
    $('.btn-close').click(function () {
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

    $(document).on('click', '.grn-attachment-button', function (event) {
        event.preventDefault();
        OpenGRNAttachmentPanel(this);
    });

    $('#grnAttachmentFileInput').on('change', function (event) {
        HandleGRNAttachmentFiles(event.target.files);
        $(this).val('');
    });

    $('#grnAttachmentDropZone').on('dragover', function (event) {
        event.preventDefault();
        $(this).addClass('dragover');
    });

    $('#grnAttachmentDropZone').on('dragleave', function () {
        $(this).removeClass('dragover');
    });

    $('#grnAttachmentDropZone').on('drop', function (event) {
        event.preventDefault();
        $(this).removeClass('dragover');
        HandleGRNAttachmentFiles(event.originalEvent.dataTransfer.files);
    });

    $('#grnAttachmentOverlay').on('click', function (event) {
        if (event.target.id === 'grnAttachmentOverlay') {
            CloseGRNAttachmentPanel();
        }
    });

    $('#grnAttachmentViewer').on('click', function (event) {
        if (event.target.id === 'grnAttachmentViewer') {
            CloseGRNAttachmentViewer();
        }
    });

    $(document).on('keydown', function (event) {
        if ($('#grnAttachmentViewer').is(':visible')) {
            if (event.key === 'ArrowLeft') {
                GRNViewerPrev();
            } else if (event.key === 'ArrowRight') {
                GRNViewerNext();
            } else if (event.key === 'Escape') {
                CloseGRNAttachmentViewer();
            }
        } else if ($('#grnAttachmentOverlay').is(':visible') && event.key === 'Escape') {
            CloseGRNAttachmentPanel();
        }
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
    RemoveGRNAttachmentsForRow($row.data('row-key'));
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
    $('#modalItemTracking').data('itemno', itemno);
    $('#modalItemTracking').data('documenttype', documentType);
    $('#modalItemTracking').data('documentno', documentNo);
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
                            var rowKey = CreateTrackingRowKey(lineNo, item.Item_No, item.Entry_No, index + 1);
                                rowData = `<tr class='itemtrackingtr' data-row-key='${rowKey}' data-line-no='${lineNo}' data-item-no='${item.Item_No || itemno}'><td style='display: none;'>${item.Entry_No}</td><td>${lineNo}</td><td>${item.Item_No}</td><td><input type='text' name='txtLotNo_${index + 1}' value='${item.Lot_No}' class='form-control' disabled /></td><td><input type='text' name='txtQtyToHandle_${index + 1}' value='${item.Qty_to_Handle_Base}' class='form-control' /></td><td><input type='text' name='txtExpDate_${index + 1}' value='${item.Expiration_Date}' class='form-control' disabled /></td><td>${item.Quantity}</td><td class='text-center'>${GetAttachmentButtonHtml(rowKey)}</td><td></td>
                            </tr>`;
                            $('#tbItemTrackingLines').append(rowData);
                        });
                    } else {
                        rowData = "<tr><td colspan=9>No Records Found</td></tr>";
                        $('#tbItemTrackingLines').append(rowData);
                    }
                } else if (documentType == "Purchase Order") {
                    var templateRowKey = CreateTrackingRowKey(lineNo, itemno, 0, 0);
                        rowData = `<tr class='itemtrackingtr' data-row-key='${templateRowKey}' data-line-no='${lineNo}' data-item-no='${itemno}'>
                        <td style='display: none;'>0</td><td>${lineNo}</td><td>${itemno}</td><td><input type='text' name='txtLotNo_0' value='' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_0' value='0' class='form-control' /></td><td><input type='text' name='txtExpDate_0' value='' class='form-control datepicker'/></td><td>0</td><td class='text-center'>${GetAttachmentButtonHtml(templateRowKey)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="AddItemTrackingInGrid();">Add</button></td>
                    </tr>`;
                    $('#tbItemTrackingLines').append(rowData);

                    if (data && data.length > 0) {
                        $.each(data, function (index, item) {
                            var existingRowKey = CreateTrackingRowKey(lineNo, item.Item_No, item.Entry_No, index + 1);
                                rowData = `<tr class='itemtrackingtr' data-row-key='${existingRowKey}' data-line-no='${lineNo}' data-item-no='${item.Item_No || itemno}'><td style='display: none;'>${item.Entry_No}</td><td>${lineNo}</td><td>${item.Item_No}</td><td><input type='text' name='txtLotNo_${index + 1}' value='${item.Lot_No}' class='form-control'/></td><td><input type='text' name='txtQtyToHandle_${index + 1}' value='${item.Qty_to_Handle_Base}' class='form-control' /></td><td><input type='text' name='txtExpDate_${index + 1}' value='${item.Expiration_Date}' class='form-control datepicker' /></td><td>${item.Quantity}</td><td class='text-center'>${GetAttachmentButtonHtml(existingRowKey)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4" onclick="DeleteItemTrackingInGrid(this)">Delete</button></td>
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
    var attachmentUploadBatch = BuildGRNAttachmentUploadBatch(documenttype, orderno);

    if (!attachmentUploadBatch.isValid) {
        $('#lblItemTrackMsg').html('<span style="color: red;">' + attachmentUploadBatch.message + '</span>');
        return false;
    }

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
                UploadGRNAttachmentBatch(attachmentUploadBatch, function () {
                    $('#modalItemTracking').hide();
                    ShowActionMsg("Item Tracking lines saved successfully.");
                    $('#txttotalQty').val('0.00');
                    $('#txtbalanceQty').val($('#txtbalanceQty').data('original').toFixed(2));
                }, function (message) {
                    ShowErrMsg(message || "Item Tracking saved, but attachment upload failed.");
                });
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
    var templateRowKey = firstRow.data('row-key');
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
    var newRowKey = CreateTrackingRowKey(lineno, itemNo, entryno, newIndex);
    var newRow = `<tr class='itemtrackingtr' data-row-key='${newRowKey}' data-line-no='${lineno}' data-item-no='${itemNo}'><td style='display: none;'>${entryno}</td><td>${lineno}</td><td>${itemNo}</td><td><input type="text" name="txtLotNo_${newIndex}" value="${lotNo}" class="form-control"></td><td><input type="text" name="txtQtyToHandle_${newIndex}" value="${qty}" class="form-control"></td><td><input type="text" name="txtExpDate_${newIndex}" value="${expDate}" class="form-control datepicker"></td><td>${qty}</td><td class='text-center'>${GetAttachmentButtonHtml(newRowKey)}</td><td><button type="button" class="btn btn-primary btn-sm radius-30 px-4 btn-delete-itemtracking" onclick="DeleteItemTrackingInGrid(this);">Delete</button></td>
    </tr>`;
    $('#tbItemTrackingLines').append(newRow);

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });

    if (templateRowKey && grnAttachmentState.rows[templateRowKey]) {
        var refreshedTemplateRowKey = CreateTrackingRowKey(lineno, itemNo, 0, 0);
        grnAttachmentState.rows[newRowKey] = grnAttachmentState.rows[templateRowKey];
        grnAttachmentState.rows[newRowKey].rowInfo = $.extend({}, grnAttachmentState.rows[newRowKey].rowInfo, {
            lineNo: lineno,
            itemNo: itemNo,
            lotNo: lotNo
        });
        delete grnAttachmentState.rows[templateRowKey];
        firstRow.attr('data-row-key', refreshedTemplateRowKey).data('row-key', refreshedTemplateRowKey);
        firstRow.attr('data-line-no', lineno);
        firstRow.attr('data-item-no', itemNo);
        firstRow.find('.grn-attachment-button').attr('data-row-key', refreshedTemplateRowKey);
        RefreshGRNAttachmentButton(newRowKey);
    }

    firstRow.find('input[type="text"]').val('');
    EnsureGRNAttachmentRowState(firstRow.data('row-key'), {
        lineNo: lineno,
        itemNo: itemNo,
        lotNo: ''
    });
    RefreshGRNAttachmentButton(firstRow.data('row-key'));
    UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));

    $(document).off('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]').on('input', '#tbItemTrackingLines input[name^="txtQtyToHandle"]', function () {
        UpdateItemTrackingTotals($('#txtbalanceQty').data('original'));
    });
}

function CreateTrackingRowKey(lineNo, itemNo, entryNo, rowIndex) {
    grnAttachmentSequence += 1;
    return ['grn', lineNo || '0', itemNo || 'item', entryNo || '0', rowIndex || '0', grnAttachmentSequence].join('-');
}

function EnsureGRNAttachmentRowState(rowKey, rowInfo) {
    if (!rowKey) {
        return null;
    }

    if (!grnAttachmentState.rows[rowKey]) {
        grnAttachmentState.rows[rowKey] = {
            files: [],
            serverLoaded: false,
            rowInfo: {}
        };
    }

    if (rowInfo) {
        grnAttachmentState.rows[rowKey].rowInfo = $.extend({}, grnAttachmentState.rows[rowKey].rowInfo, rowInfo);
    }

    return grnAttachmentState.rows[rowKey];
}

function RemoveGRNAttachmentsForRow(rowKey) {
    if (!rowKey) {
        return;
    }

    delete grnAttachmentState.rows[rowKey];
}

function GetAttachmentButtonHtml(rowKey) {
    var count = GetGRNAttachmentCount(rowKey);
    var countHtml = count > 0 ? `<span class="grn-attachment-count">${count}</span>` : '';
    return `<button type="button" class="btn btn-outline-secondary btn-sm grn-attachment-button" data-row-key="${rowKey}" onclick="OpenGRNAttachmentPanel(this);"><i class="bx bx-paperclip"></i>${countHtml}</button>`;
}

function GetGRNAttachmentCount(rowKey) {
    var rowState = grnAttachmentState.rows[rowKey];
    return rowState && rowState.files ? rowState.files.length : 0;
}

function RefreshGRNAttachmentButton(rowKey) {
    if (!rowKey) {
        return;
    }

    var $button = $('.grn-attachment-button[data-row-key="' + rowKey + '"]');
    if (!$button.length) {
        return;
    }

    var count = GetGRNAttachmentCount(rowKey);
    $button.find('.grn-attachment-count').remove();
    if (count > 0) {
        $button.append(`<span class="grn-attachment-count">${count}</span>`);
    }
}

function GetGRNRowInfo($row) {
    var rowKey = $row.data('row-key');
    var modalLineNo = ($('#modalItemTracking').data('lineno') || '').toString().trim();
    var modalItemNo = ($('#modalItemTracking').data('itemno') || '').toString().trim();
    var rowLineNo = (($row.data('line-no') || '').toString().trim()) || (($row.find('td').eq(1).text() || '').toString().trim()) || modalLineNo;
    var rowItemNo = (($row.data('item-no') || '').toString().trim()) || (($row.find('td').eq(2).text() || '').toString().trim()) || modalItemNo;
    return {
        rowKey: rowKey,
        documentType: ($('#modalItemTracking').data('documenttype') || '').toString(),
        documentNo: ($('#modalItemTracking').data('documentno') || '').toString(),
        lineNo: rowLineNo,
        itemNo: rowItemNo,
        lotNo: (($row.find('td').eq(3).find('input').val() || '').toString().trim())
    };
}

function ResolveGRNAttachmentButton(buttonOrRow) {
    var $source = $(buttonOrRow || []);
    if (!$source.length) {
        return $();
    }

    if ($source.hasClass('grn-attachment-button')) {
        return $source.first();
    }

    if ($source.is('tr')) {
        return $source.find('.grn-attachment-button').first();
    }

    return $source.closest('tr').find('.grn-attachment-button').first();
}

function OpenGRNAttachmentFilePicker() {
    $('#grnAttachmentFileInput').trigger('click');
}

function EnsureGRNAttachmentLayers() {
    var $overlay = $('#grnAttachmentOverlay');
    var $viewer = $('#grnAttachmentViewer');

    if ($overlay.length && !$overlay.parent().is('body')) {
        $overlay.appendTo('body');
    }

    if ($viewer.length && !$viewer.parent().is('body')) {
        $viewer.appendTo('body');
    }
}

function OpenGRNAttachmentPanel(button) {
    EnsureGRNAttachmentLayers();

    var $button = ResolveGRNAttachmentButton(button);
    var $row = $button.closest('tr');

    if (!$row.length) {
        return;
    }

    var rowInfo = GetGRNRowInfo($row);

    if (!rowInfo.rowKey) {
        rowInfo.rowKey = CreateTrackingRowKey(rowInfo.lineNo, rowInfo.itemNo, $row.find('td').eq(0).text(), $row.index());
        $row.attr('data-row-key', rowInfo.rowKey);
        if ($button.length) {
            $button.attr('data-row-key', rowInfo.rowKey);
        }
    }

    var rowState = EnsureGRNAttachmentRowState(rowInfo.rowKey, rowInfo);

    if (!rowState) {
        return;
    }

    grnAttachmentState.currentRowKey = rowInfo.rowKey;
    $('body').addClass('grn-attachment-open');
    $('#grnAttachmentMeta').text('Line No: ' + (rowInfo.lineNo || '-') + ' | Item No: ' + (rowInfo.itemNo || '-') + ' | Lot No: ' + (rowInfo.lotNo || '-'));
    $('#grnAttachmentError').text('');
    RenderGRNAttachmentFileList();
    $('#grnAttachmentOverlay').css('display', 'flex');

    if (rowInfo.lotNo && rowInfo.itemNo) {
        LoadGRNExistingAttachments(rowInfo.rowKey, rowInfo, true);
    }
}

function OpenItemTrackingAttachmentModalFromRow(buttonOrRow) {
    var $button = ResolveGRNAttachmentButton(buttonOrRow);
    if (!$button.length && typeof buttonOrRow === 'string') {
        $button = $('.grn-attachment-button[data-row-key="' + buttonOrRow + '"]').first();
    }

    if (!$button.length) {
        return;
    }

    OpenGRNAttachmentPanel($button[0]);
}

function CloseGRNAttachmentPanel() {
    $('#grnAttachmentOverlay').hide();
    $('#grnAttachmentError').text('');
    grnAttachmentState.currentRowKey = '';
    $('body').removeClass('grn-attachment-open');
}

function SaveGRNAttachmentPanel() {
    var rowKey = grnAttachmentState.currentRowKey;
    var rowState = EnsureGRNAttachmentRowState(rowKey);

    if (!rowState) {
        return;
    }

    var pendingFiles = $.grep(rowState.files || [], function (file) {
        return !file.isSaved;
    });

    if (!pendingFiles.length) {
        RefreshGRNAttachmentButton(rowKey);
        CloseGRNAttachmentPanel();
        ShowActionMsg('Koi naya document save karne ke liye pending nahi hai.');
        return;
    }
    $('#grnAttachmentError').text('');

    EnsureGRNAttachmentFilesReadyForUpload(pendingFiles).then(function () {
        var batch = BuildGRNAttachmentUploadBatchForRow(rowKey);
        if (!batch.isValid) {
            $('#grnAttachmentError').text(batch.message || 'Attachment save ke liye required details missing hain.');
            return;
        }

        UploadGRNAttachmentBatch(batch, function () {
            RefreshGRNAttachmentButton(rowKey);
            CloseGRNAttachmentPanel();
            ShowActionMsg('Document save ho gaya.');
        }, function (message) {
            $('#grnAttachmentError').text(message || 'Document save nahi ho paya.');
        });
    }).catch(function () {
        $('#grnAttachmentError').text('Document ko Base64 me convert nahi kar paye.');
    });
}

function HandleGRNAttachmentFiles(fileList) {
    var rowKey = grnAttachmentState.currentRowKey;
    var rowState = EnsureGRNAttachmentRowState(rowKey);

    if (!rowState || !fileList || !fileList.length) {
        return;
    }

    var files = Array.prototype.slice.call(fileList);
    var readers = $.map(files, function (file) {
        return FileToBase64(file).then(function (base64Text) {
            rowState.files.push({
                id: CreateGRNAttachmentFileId(),
                fileName: file.name,
                fileExtension: GetGRNFileExtension(file.name),
                fileType: file.type || 'application/octet-stream',
                base64Text: NormalizeGRNAttachmentBase64(base64Text),
                sourceFile: file,
                size: file.size || 0,
                isSaved: false
            });
        });
    });

    Promise.all(readers).then(function () {
        RenderGRNAttachmentFileList();
        RefreshGRNAttachmentButton(rowKey);
    }).catch(function () {
        $('#grnAttachmentError').text('File read karne mein error aaya.');
    });
}

function RenderGRNAttachmentFileList() {
    var rowState = EnsureGRNAttachmentRowState(grnAttachmentState.currentRowKey);
    var $list = $('#grnAttachmentFileList');
    $list.empty();

    if (!rowState || !rowState.files || rowState.files.length === 0) {
        $list.html('<div class="grn-attachment-empty">No attachment added for this row.</div>');
        return;
    }

    $.each(rowState.files, function (index, file) {
        var removable = `<button type="button" class="btn btn-sm btn-outline-danger" onclick="RemoveGRNAttachmentFile('${file.id}');">Remove</button>`;
        var badgeText = (file.fileExtension || 'file').substring(0, 4).toUpperCase();
        var savedLabel = file.isSaved ? 'Uploaded' : 'Pending upload';
        $list.append(`<div class="grn-attachment-file-row"><div class="grn-attachment-file-main"><span class="grn-attachment-file-badge">${badgeText}</span><div><span class="grn-attachment-file-name">${EscapeHtml(file.fileName)}</span><span class="grn-attachment-file-meta">${savedLabel} | ${FormatGRNFileSize(file.size)}</span></div></div><div><button type="button" class="btn btn-sm btn-outline-primary" onclick="OpenGRNAttachmentViewer('${grnAttachmentState.currentRowKey}', ${index});">View</button> ${removable}</div></div>`);
    });
}

function RemoveGRNAttachmentFile(fileId) {
    var rowKey = grnAttachmentState.currentRowKey;
    var rowState = EnsureGRNAttachmentRowState(rowKey);

    if (!rowState || !rowState.files) {
        return;
    }

    var targetFile = null;
    $.each(rowState.files, function (_, file) {
        if (file.id === fileId) {
            targetFile = file;
            return false;
        }
    });

    if (!targetFile) {
        return;
    }

    if (targetFile.isSaved && targetFile.systemId) {
        $.ajax({
            type: 'POST',
            url: '/SPGRN/DeleteGRNItemTrackingAttachment',
            data: JSON.stringify({ SystemId: targetFile.systemId }),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (!data) {
                    $('#grnAttachmentError').text('Uploaded attachment delete nahi ho paya.');
                    return;
                }

                RemoveGRNAttachmentFileFromState(rowKey, fileId);
                ShowActionMsg('Attachment deleted successfully.');
            },
            error: function (xhr) {
                $('#grnAttachmentError').text('Attachment delete API error. ' + (xhr.responseText || ''));
            }
        });
        return;
    }

    RemoveGRNAttachmentFileFromState(rowKey, fileId);
}

function RemoveGRNAttachmentFileFromState(rowKey, fileId) {
    var rowState = EnsureGRNAttachmentRowState(rowKey);
    if (!rowState || !rowState.files) {
        return;
    }

    rowState.files = $.grep(rowState.files, function (file) {
        return file.id !== fileId;
    });

    RenderGRNAttachmentFileList();
    RefreshGRNAttachmentButton(rowKey);
}

function LoadGRNExistingAttachments(rowKey, rowInfo, forceReload, onLoaded, onError) {
    $.ajax({
        type: 'POST',
        url: '/SPGRN/GetGRNItemTrackingAttachments',
        data: JSON.stringify({
            DocumentType: rowInfo.documentType,
            LineNo: rowInfo.lineNo,
            LotNo: rowInfo.lotNo,
            ItemNo: rowInfo.itemNo
        }),
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            var rowState = EnsureGRNAttachmentRowState(rowKey, rowInfo);
            var existingFilesByKey = {};

            if (forceReload) {
                rowState.files = $.grep(rowState.files || [], function (file) {
                    return !file.isSaved;
                });
            }

            $.each(rowState.files, function (_, file) {
                var fileKey = (file.fileName || 'Attachment') + '|' + NormalizeGRNAttachmentBase64(file.base64Text || '');
                existingFilesByKey[fileKey] = file;
            });

            $.each(data || [], function (_, attachment) {
                var fileName = attachment.File_Name || 'Attachment';
                var base64Text = NormalizeGRNAttachmentBase64(attachment.Base64Text || '');
                var uniqueKey = fileName + '|' + base64Text;
                if (existingFilesByKey[uniqueKey]) {
                    existingFilesByKey[uniqueKey].systemId = attachment.SystemId || existingFilesByKey[uniqueKey].systemId || '';
                    existingFilesByKey[uniqueKey].isSaved = true;
                    return;
                }

                rowState.files.push({
                    id: CreateGRNAttachmentFileId(),
                    systemId: attachment.SystemId || '',
                    fileName: fileName,
                    fileExtension: attachment.File_Extension || GetGRNFileExtension(fileName),
                    fileType: attachment.File_Type || 'application/octet-stream',
                    base64Text: base64Text,
                    size: GetApproximateBase64Size(base64Text),
                    isSaved: true
                });
            });

            rowState.serverLoaded = true;
            RefreshGRNAttachmentButton(rowKey);

            if (grnAttachmentState.currentRowKey === rowKey) {
                RenderGRNAttachmentFileList();
            }

            if (typeof onLoaded === 'function') {
                onLoaded(data || [], rowState);
            }
        },
        error: function () {
            $('#grnAttachmentError').text('Existing attachments load nahi ho paye.');
            if (typeof onError === 'function') {
                onError('Existing attachments load nahi ho paye.');
            }
        }
    });
}

function BuildGRNAttachmentUploadBatch(documentType, documentNo) {
    var batchItems = [];
    var invalidMessage = '';

    $('#tbItemTrackingLines .itemtrackingtr').each(function () {
        var $row = $(this);
        var rowInfo = GetGRNRowInfo($row);
        var rowState = EnsureGRNAttachmentRowState(rowInfo.rowKey, rowInfo);

        if (!rowState || !rowState.files || rowState.files.length === 0) {
            return;
        }

        var pendingFiles = $.grep(rowState.files, function (file) {
            return !file.isSaved;
        });

        if (!pendingFiles.length) {
            return;
        }

        if (!rowInfo.itemNo || !rowInfo.lotNo) {
            invalidMessage = 'Attachment upload ke liye Item No aur Lot No required hai.';
            return false;
        }

        $.each(pendingFiles, function (_, file) {
                var normalizedBase64Text = NormalizeGRNAttachmentBase64(file.base64Text);
            batchItems.push({
                rowKey: rowInfo.rowKey,
                fileId: file.id,
                payload: {
                    Table_ID: GRN_ATTACHMENT_TABLE_ID,
                    Document_Type: documentType,
                    Line_No: rowInfo.lineNo,
                    No: rowInfo.lotNo,
                    Item_No: rowInfo.itemNo,
                    File_Extension: file.fileExtension,
                    File_Name: file.fileName,
                    File_Type: file.fileType,
                        Base64Text: normalizedBase64Text
                }
            });
        });
    });

    return {
        isValid: invalidMessage === '',
        message: invalidMessage,
        items: batchItems,
        request: {
            DocumentNo: documentNo,
            ItemNo: batchItems.length === 1 ? (batchItems[0].payload.Item_No || '') : '',
            Attachments: $.map(batchItems, function (item) {
                return item.payload;
            })
        }
    };
}

function UploadGRNAttachmentBatch(batch, onSuccess, onError) {
    if (!batch || !batch.items || batch.items.length === 0) {
        if (typeof onSuccess === 'function') {
            onSuccess();
        }
        return;
    }

    $.ajax({
        type: 'POST',
        url: '/SPGRN/UploadGRNItemTrackingAttachments',
        data: JSON.stringify(batch.request),
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            if (!data) {
                if (typeof onError === 'function') {
                    onError('Item Tracking save ho gaya, lekin attachment upload fail ho gaya.');
                }
                return;
            }

            VerifyGRNAttachmentBatchSaved(batch).then(function () {
                if (typeof onSuccess === 'function') {
                    onSuccess();
                }
            }).catch(function (error) {
                if (typeof onError === 'function') {
                    onError((error && error.message) || 'Document save verify nahi ho paya.');
                }
            });
        },
        error: function (xhr) {
            if (typeof onError === 'function') {
                onError('Attachment upload API error. ' + (xhr.responseText || ''));
            }
        }
    });
}

function VerifyGRNAttachmentBatchSaved(batch) {
    var rowItems = {};

    $.each(batch.items || [], function (_, item) {
        if (!rowItems[item.rowKey]) {
            rowItems[item.rowKey] = [];
        }
        rowItems[item.rowKey].push(item);
    });

    var verificationTasks = $.map(Object.keys(rowItems), function (rowKey) {
        var rowState = grnAttachmentState.rows[rowKey];
        if (!rowState || !rowState.rowInfo || !rowState.rowInfo.lotNo || !rowState.rowInfo.itemNo) {
            return Promise.reject(new Error('Attachment row verification details missing hain.'));
        }

        return new Promise(function (resolve, reject) {
            LoadGRNExistingAttachments(rowKey, rowState.rowInfo, true, function (_, refreshedRowState) {
                var missingFiles = $.grep(rowItems[rowKey], function (item) {
                    var matchedFile = null;

                    $.each((refreshedRowState && refreshedRowState.files) || [], function (_, file) {
                        if (file.id === item.fileId) {
                            matchedFile = file;
                            return false;
                        }
                    });

                    return !matchedFile || !matchedFile.isSaved;
                });

                if (missingFiles.length) {
                    reject(new Error('Document save response mila, lekin document list me record nahi mila.'));
                    return;
                }

                RefreshGRNAttachmentButton(rowKey);
                resolve();
            }, function (message) {
                reject(new Error(message || 'Attachment verification load fail ho gaya.'));
            });
        });
    });

    if (!verificationTasks.length) {
        return Promise.resolve();
    }

    return Promise.all(verificationTasks).then(function () {
        return true;
    });
}

function OpenGRNAttachmentViewer(rowKey, startIndex) {
    EnsureGRNAttachmentLayers();

    var rowState = EnsureGRNAttachmentRowState(rowKey);
    if (!rowState || !rowState.files || rowState.files.length === 0) {
        return;
    }

    grnAttachmentState.viewerFiles = rowState.files.slice();
    grnAttachmentState.viewerIndex = startIndex || 0;
    grnAttachmentState.viewerZoom = 1;
    grnAttachmentState.viewerMode = 'single';
    $('body').addClass('grn-attachment-open');
    $('#grnAttachmentViewer').css('display', 'flex');
    RenderGRNAttachmentViewer();
}

function CloseGRNAttachmentViewer() {
    $('#grnAttachmentViewer').hide();
    if (!$('#grnAttachmentOverlay').is(':visible')) {
        $('body').removeClass('grn-attachment-open');
    }
}

function GRNViewerPrev() {
    if (!grnAttachmentState.viewerFiles.length) {
        return;
    }

    grnAttachmentState.viewerIndex = (grnAttachmentState.viewerIndex - 1 + grnAttachmentState.viewerFiles.length) % grnAttachmentState.viewerFiles.length;
    RenderGRNAttachmentViewer();
}

function GRNViewerNext() {
    if (!grnAttachmentState.viewerFiles.length) {
        return;
    }

    grnAttachmentState.viewerIndex = (grnAttachmentState.viewerIndex + 1) % grnAttachmentState.viewerFiles.length;
    RenderGRNAttachmentViewer();
}

function GRNViewerZoomIn() {
    grnAttachmentState.viewerZoom = Math.min(grnAttachmentState.viewerZoom + 0.2, 3);
    RenderGRNAttachmentViewer();
}

function GRNViewerZoomOut() {
    grnAttachmentState.viewerZoom = Math.max(grnAttachmentState.viewerZoom - 0.2, 0.4);
    RenderGRNAttachmentViewer();
}

function GRNViewerResetZoom() {
    grnAttachmentState.viewerZoom = 1;
    RenderGRNAttachmentViewer();
}

function ToggleGRNViewerMode() {
    grnAttachmentState.viewerMode = grnAttachmentState.viewerMode === 'single' ? 'grid' : 'single';
    RenderGRNAttachmentViewer();
}

function RenderGRNAttachmentViewer() {
    var files = grnAttachmentState.viewerFiles;
    if (!files.length) {
        return;
    }

    var currentFile = files[grnAttachmentState.viewerIndex];
    var isImage = IsGRNPreviewableImage(currentFile);
    var fileUrl = BuildGRNAttachmentDataUrl(currentFile);

    $('#grnViewerCounter').text((grnAttachmentState.viewerIndex + 1) + ' / ' + files.length);
    $('#grnViewerFileName').text(currentFile.fileName || 'Attachment');
    $('#grnViewerZoomLabel').text(Math.round(grnAttachmentState.viewerZoom * 100) + '%');
    $('#grnViewerModeButton').text(grnAttachmentState.viewerMode === 'single' ? 'Grid' : 'Single');

    if (grnAttachmentState.viewerMode === 'grid') {
        $('#grnViewerSingle').hide();
        $('#grnViewerGrid').css('display', 'grid');
        RenderGRNAttachmentViewerGrid();
    } else {
        $('#grnViewerGrid').hide().empty();
        $('#grnViewerSingle').css('display', 'flex');
        if (isImage) {
            $('#grnViewerImage').attr('src', fileUrl).css({ display: 'block', transform: 'scale(' + grnAttachmentState.viewerZoom + ')' });
            $('#grnViewerNoPreview').hide();
        } else {
            $('#grnViewerImage').hide().attr('src', '');
            $('#grnViewerNoPreview').show();
            $('#grnViewerDownloadLink').attr({ href: fileUrl, download: currentFile.fileName || 'Attachment' });
        }
    }

    RenderGRNAttachmentThumbStrip();
}

function RenderGRNAttachmentViewerGrid() {
    var $grid = $('#grnViewerGrid');
    $grid.empty();

    $.each(grnAttachmentState.viewerFiles, function (index, file) {
        var preview = IsGRNPreviewableImage(file)
            ? `<img src="${BuildGRNAttachmentDataUrl(file)}" alt="${EscapeHtml(file.fileName)}" />`
            : `<span>${EscapeHtml((file.fileExtension || 'file').toUpperCase())}</span>`;
        var activeClass = index === grnAttachmentState.viewerIndex ? ' active' : '';
        $grid.append(`<div class="grn-viewer-grid-card${activeClass}" onclick="SelectGRNAttachmentViewerIndex(${index});"><div class="grn-viewer-grid-thumb">${preview}</div><div class="grn-viewer-grid-name">${EscapeHtml(file.fileName)}</div></div>`);
    });
}

function RenderGRNAttachmentThumbStrip() {
    var $strip = $('#grnViewerThumbStrip');
    $strip.empty();

    $.each(grnAttachmentState.viewerFiles, function (index, file) {
        var content = IsGRNPreviewableImage(file)
            ? `<img src="${BuildGRNAttachmentDataUrl(file)}" alt="${EscapeHtml(file.fileName)}" />`
            : `<span>${EscapeHtml((file.fileExtension || 'file').toUpperCase())}</span>`;
        var activeClass = index === grnAttachmentState.viewerIndex ? ' active' : '';
        $strip.append(`<div class="grn-viewer-thumb${activeClass}" onclick="SelectGRNAttachmentViewerIndex(${index});">${content}</div>`);
    });
}

function SelectGRNAttachmentViewerIndex(index) {
    grnAttachmentState.viewerIndex = index;
    RenderGRNAttachmentViewer();
}

function FileToBase64(file) {
    return new Promise(function (resolve, reject) {
        var reader = new FileReader();
        reader.onload = function () {
            var result = (reader.result || '').toString();
            var parts = result.split(',');
            resolve(parts.length > 1 ? parts[1] : result);
        };
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

function NormalizeGRNAttachmentBase64(base64Text) {
    var normalizedText = (base64Text || '').toString().trim();
    if (!normalizedText) {
        return '';
    }

    var commaIndex = normalizedText.indexOf(',');
    if (commaIndex >= 0 && normalizedText.substring(0, commaIndex).indexOf('base64') >= 0) {
        normalizedText = normalizedText.substring(commaIndex + 1);
    }

    return normalizedText.replace(/\s+/g, '');
}

function EnsureGRNAttachmentFilesReadyForUpload(files) {
    var conversions = $.map(files || [], function (file) {
        if (!file) {
            return null;
        }

        file.base64Text = NormalizeGRNAttachmentBase64(file.base64Text);
        if (file.base64Text) {
            return null;
        }

        if (!file.sourceFile) {
            return Promise.reject(new Error('Source file missing for Base64 conversion.'));
        }

        return FileToBase64(file.sourceFile).then(function (base64Text) {
            file.base64Text = NormalizeGRNAttachmentBase64(base64Text);
            return file.base64Text;
        });
    });

    if (!conversions.length) {
        return Promise.resolve();
    }

    return Promise.all(conversions).then(function () {
        var missingBase64Files = $.grep(files || [], function (file) {
            return !NormalizeGRNAttachmentBase64(file && file.base64Text);
        });

        if (missingBase64Files.length) {
            return Promise.reject(new Error('Base64Text generation failed.'));
        }
    });
}

function BuildGRNAttachmentDataUrl(file) {
    return 'data:' + (file.fileType || 'application/octet-stream') + ';base64,' + (file.base64Text || '');
}

function IsGRNPreviewableImage(file) {
    var extension = (file.fileExtension || '').toLowerCase();
    return ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'].indexOf(extension) !== -1;
}

function CreateGRNAttachmentFileId() {
    grnAttachmentSequence += 1;
    return 'grn-file-' + grnAttachmentSequence;
}

function GetGRNFileExtension(fileName) {
    var parts = (fileName || '').split('.');
    return parts.length > 1 ? parts.pop().toLowerCase() : '';
}

function FormatGRNFileSize(size) {
    if (!size || size < 1024) {
        return (size || 0) + ' B';
    }
    if (size < 1024 * 1024) {
        return (size / 1024).toFixed(1) + ' KB';
    }
    return (size / (1024 * 1024)).toFixed(1) + ' MB';
}

function GetApproximateBase64Size(base64Text) {
    if (!base64Text) {
        return 0;
    }
    return Math.floor((base64Text.length * 3) / 4);
}

function EscapeHtml(value) {
    return $('<div/>').text(value || '').html();
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

function BuildGRNAttachmentUploadBatchForRow(rowKey) {
    var rowState = EnsureGRNAttachmentRowState(rowKey);
    var rowInfo = rowState ? rowState.rowInfo : null;
    var pendingFiles = rowState && rowState.files ? $.grep(rowState.files, function (file) {
        return !file.isSaved;
    }) : [];

    if (!rowState || !rowInfo) {
        return {
            isValid: false,
            message: 'Attachment row information available nahi hai.',
            items: [],
            request: { DocumentNo: '', ItemNo: '', Attachments: [] }
        };
    }

    if (!pendingFiles.length) {
        return {
            isValid: true,
            message: '',
            items: [],
            request: {
                DocumentNo: rowInfo.documentNo || '',
                ItemNo: rowInfo.itemNo || '',
                Attachments: []
            }
        };
    }

    if (!rowInfo.documentNo || !rowInfo.documentType || !rowInfo.lineNo || !rowInfo.itemNo || !rowInfo.lotNo) {
        return {
            isValid: false,
            message: 'Attachment save ke liye Document No, Line No, Item No aur Lot No required hai.',
            items: [],
            request: { DocumentNo: rowInfo.documentNo || '', ItemNo: rowInfo.itemNo || '', Attachments: [] }
        };
    }

    var batchItems = $.map(pendingFiles, function (file) {
            var normalizedBase64Text = NormalizeGRNAttachmentBase64(file.base64Text);
        return {
            rowKey: rowKey,
            fileId: file.id,
            payload: {
                Table_ID: GRN_ATTACHMENT_TABLE_ID,
                Document_Type: rowInfo.documentType,
                Line_No: rowInfo.lineNo,
                No: rowInfo.lotNo,
                Item_No: rowInfo.itemNo,
                File_Extension: file.fileExtension,
                File_Name: file.fileName,
                File_Type: file.fileType,
                    Base64Text: normalizedBase64Text
            }
        };
    });

        if ($.grep(batchItems, function (item) {
            return !item.payload.Base64Text;
        }).length) {
            return {
                isValid: false,
                message: 'Attachment Base64Text generate nahi ho paya.',
                items: [],
                request: { DocumentNo: rowInfo.documentNo || '', ItemNo: rowInfo.itemNo || '', Attachments: [] }
            };
        }

    return {
        isValid: true,
        message: '',
        items: batchItems,
        request: {
            DocumentNo: rowInfo.documentNo,
            ItemNo: rowInfo.itemNo || '',
            Attachments: $.map(batchItems, function (item) {
                return item.payload;
            })
        }
    };
}