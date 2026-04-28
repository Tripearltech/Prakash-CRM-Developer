var custDetailState = {
    allRows: [],
    filteredRows: [],
    currentPage: 1,
    pageSize: 15,
    isSpDueAmt: false,
    totalCount: 0
};

$(document).ready(function () {
    initializeCustDetailReport();
});

function initializeCustDetailReport() {
    custDetailState.isSpDueAmt = getBooleanQueryParam('isSpDueAmt');
    setDetailAsOnDate();
    bindCustDetailEvents();
    updateCustDetailNavigation();
    loadCustomerDetailData();
}

function setDetailAsOnDate() {
    var today = new Date();
    var yyyy = today.getFullYear();
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var dd = String(today.getDate()).padStart(2, '0');
    var $dateField = $('#txtDetailAsOnDate');
    if ($dateField.length && !$dateField.val()) {
        $dateField.val(yyyy + '-' + mm + '-' + dd);
    }
}

function bindCustDetailEvents() {
    $('#btnDetailSearch').off('click.custDetail').on('click.custDetail', function () {
        hideProductSuggestions();
        applyCustomerDetailFilter();
    });

    $('#txtDetailProduct, #txtDetailBillNo, #txtDetailDocumentType').off('keypress.custDetail').on('keypress.custDetail', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            hideProductSuggestions();
            applyCustomerDetailFilter();
        }
    });

    $('#txtDetailProduct').off('input.custDetail').on('input.custDetail', function () {
        var val = ($(this).val() || '').trim().toLowerCase();
        if (!val) {
            hideProductSuggestions();
            return;
        }

        var uniqueProducts = getUniqueProductMatches(val);
        renderProductSuggestions(uniqueProducts);
    });

    $(document).off('click.prodSuggest', '#dlDetailProduct li').on('click.prodSuggest', '#dlDetailProduct li', function () {
        var val = $(this).data('value') || '';
        $('#txtDetailProduct').val(val);
        hideProductSuggestions();
        applyCustomerDetailFilter();
    });

    $(document).off('click.prodSuggestHide').on('click.prodSuggestHide', function (e) {
        if (!$(e.target).closest('#txtDetailProduct, #dlDetailProduct').length) {
            hideProductSuggestions();
        }
    });

    $('#btnDetailExport').off('click.custDetail').on('click.custDetail', function () {
        exportCustomerDetail();
    });

    $('#btnDetailRefresh').off('click.custDetail').on('click.custDetail', function () {
        resetCustomerDetailFilters();
    });

}

function loadCustomerDetailData() {
    var customerName = getQueryParam('customerName') || '';
    var spCode = ($('#hdnLoggedInSPCode').val() || $('[name="hdnLoggedInSPCode"]').val() || '').trim();

    if (customerName) {
        $('#lblDetailCustomerName').text(customerName);
    }

    toggleCustOverdueDetailLoader(true);

    var requestData = {};
    if (spCode) {
        requestData.spCode = spCode;
    }

    $.ajax({
        url: '/SPReports/GetCSOutstandingDuelist',
        type: 'GET',
        contentType: 'application/json',
        data: {
            spCode: requestData.spCode || ''
        },
        success: function (response) {
            if (response && response.error) {
                $('#customerDetailBody').html("<tr><td colspan='11' class='text-center'>" + escapeHtml(response.error) + "</td></tr>");
                toggleCustOverdueDetailLoader(false);
                return;
            }

            var rows = response || [];

            if (custDetailState.isSpDueAmt) {
                rows = $.grep(rows, function (item) {
                    return parseFloat(item.Overdue_Days || 0) > 0;
                });
            }

            if (customerName) {
                rows = $.grep(rows, function (item) {
                    return (item.Customer_Name || '').trim().toLowerCase() === customerName.trim().toLowerCase();
                });
            }

            rows = filterDetailRows(rows);

            custDetailState.allRows = rows;
            custDetailState.filteredRows = rows;
            updateProductSearchDatalist(rows);
            renderCustomerDetailRows(rows);
            toggleCustOverdueDetailLoader(false);
        },
        error: function () {
            $('#customerDetailBody').html("<tr><td colspan='8' class='text-center'>Error loading data</td></tr>");
            toggleCustOverdueDetailLoader(false);
        }
    });
}

function updateCustDetailNavigation() {
    var listUrl = '/SPReports/CustOverDueOutstandinglist?isSpDueAmt=' + (custDetailState.isSpDueAmt ? 'true' : 'false');
    $('#custOverdueListLink').attr('href', listUrl);
    $('#custOverdueBackLink').attr('href', listUrl);
}

function getBooleanQueryParam(name) {
    return (getQueryParam(name) || '').toLowerCase() === 'true';
}

function toggleCustOverdueDetailLoader(show) {
    var $loader = $('#custOverdueDetailLoader');
    if ($loader.length) {
        $loader.toggleClass('d-none', !show);
        return;
    }

    if (show) {
        if (typeof showPageDataLoader === 'function') showPageDataLoader();
    } else {
        if (typeof hidePageDataLoader === 'function') hidePageDataLoader();
    }
}

function getUniqueProductMatches(searchVal) {
    var seen = {};
    var result = [];

    $.each(custDetailState.allRows, function (_, item) {
        var pName = (item.Product_Name || '').trim();
        if (!pName) {
            return;
        }

        if (pName.toLowerCase().indexOf(searchVal) > -1 && !seen[pName]) {
            seen[pName] = true;
            result.push(pName);
        }
    });

    result.sort();
    return result;
}

function updateProductSearchDatalist(rows) {
    var seen = {};
    var count = 0;
    var html = '';

    $.each(rows, function (_, item) {
        var pName = (item.Product_Name || '').trim();
        if (!pName || seen[pName]) {
            return;
        }

        seen[pName] = true;
        count++;
        html += '<li data-value="' + escapeAttr(pName) + '" style="padding:8px 12px;cursor:pointer;" class="suggestion-item">'
            + escapeHtml(pName) + '</li>';

        if (count >= 100) {
            return false;
        }
    });

    $('#dlDetailProduct').html(html).hide();
}

function renderProductSuggestions(productNames) {
    var $ul = $('#dlDetailProduct');

    if (!productNames || productNames.length === 0) {
        $ul.hide();
        return;
    }

    var html = '';
    $.each(productNames.slice(0, 50), function (_, pName) {
        html += '<li data-value="' + escapeAttr(pName) + '" style="padding:8px 12px;cursor:pointer;" class="suggestion-item">'
            + escapeHtml(pName) + '</li>';
    });

    $ul.html(html).show();
}

function hideProductSuggestions() {
    $('#dlDetailProduct').hide().html('');
}

function applyCustomerDetailFilter() {
    custDetailState.filteredRows = filterDetailRows(custDetailState.allRows);
    renderCustomerDetailRows(custDetailState.filteredRows);
}

function resetCustomerDetailFilters() {
    $('#txtDetailProduct').val('');
    $('#txtDetailBillNo').val('');
    $('#txtDetailDocumentType').val('');
    $('#ddlDetailExportType').val('');
    hideProductSuggestions();

    custDetailState.filteredRows = custDetailState.allRows.slice();
    renderCustomerDetailRows(custDetailState.filteredRows);
}

function renderCustomerDetailRows(rows) {
    var $body = $('#customerDetailBody');
    $body.empty();

    if (!rows || rows.length === 0) {
        $body.html("<tr><td colspan='8' class='text-center'>No detail data found</td></tr>");
        return;
    }

    var html = '';
    $.each(rows, function (_, item) {
        html += buildDetailTableRow(item);
    });

    var totalAmt = 0;
    $.each(rows, function (_, item) {
        var amt = parseFloat(item.Remaining_Amt || item.Remain_Amount || 0);
        if (!isNaN(amt)) {
            totalAmt += amt;
        }
    });

    html += '<tr class="fw-bold table-secondary">'
        + '<td colspan="' + getDetailColumnCount() + '" class="text-start ps-2">Total</td>'
        + '<td class="text-end">' + formatNumber(totalAmt) + '</td>'
        + '</tr>';

    $body.html(html);
}

function filterDetailRows(rows) {
    var productFilter = ($('#txtDetailProduct').val() || '').trim().toLowerCase();
    var billNoFilter = ($('#txtDetailBillNo').val() || '').trim().toLowerCase();
    var docTypeFilter = ($('#txtDetailDocumentType').val() || '').trim().toLowerCase();

    return $.grep(rows || [], function (item) {
        var matchProduct = !productFilter || (item.Product_Name || '').toLowerCase().indexOf(productFilter) > -1;
        var matchBill = !billNoFilter || String(item.Bill_No || item.BillNo || '').toLowerCase().indexOf(billNoFilter) > -1;
        var matchDocType = !docTypeFilter || (item.Document_Type || item.DocumentType || '').toLowerCase().indexOf(docTypeFilter) > -1;
        return matchProduct && matchBill && matchDocType;
    });
}

function buildDetailTableRow(item) {
    var cols = getDetailColumns();
    var html = '<tr>';
    $.each(cols, function (_, col) {
        var val = getDetailCellValue(item, col.keys);
        if (col.isAmount) {
            html += '<td class="text-end">' + formatNumber(val) + '</td>';
        } else {
            html += '<td>' + escapeHtml(String(val == null ? '' : val)) + '</td>';
        }
    });
    return html + '</tr>';
}

function getDetailColumns() {
    return [
        { keys: ['Document_Type', 'DocumentType'] },
        { keys: ['PO_No', 'PONo', 'PO_No'] },
        { keys: ['Bill_No', 'BillNo'] },
        { keys: ['Bill_Date', 'BillDate'], isDate: true },
        { keys: ['Product_Name', 'Product_Name', 'ProductName'] },
        { keys: ['TERMS', 'Terms'] },
        { keys: ['Due_Date', 'DueDate'], isDate: true },
        { keys: ['Invoice_Amt', 'Invoice_Amount'], isAmount: true },
        { keys: ['Remaining_Amt', 'Remain_Amount', 'Amount', 'Amt'], isAmount: true },
        { keys: ['Total_Days', 'TotalDays'], isNumber: true },
        { keys: ['Overdue_Days', 'OverdueDays'], isNumber: true }
    ];
}

function getDetailCellValue(item, keys) {
    if (!item || !keys || !keys.length) {
        return '';
    }

    for (var i = 0; i < keys.length; i++) {
        var value = item[keys[i]];
        if (value !== undefined && value !== null && value !== '') {
            return value;
        }
    }

    return '';
}

function getDetailColumnCount() {
    var thCount = $('#customerDetailThead tr th').length;
    return thCount > 1 ? thCount - 1 : 7;
}

function exportCustomerDetail() {
    if (!custDetailState.filteredRows || custDetailState.filteredRows.length === 0) {
        alert('No data to export');
        return;
    }

    var exportType = ($('#ddlDetailExportType').val() || '').trim();
    if (!exportType) {
        alert('Please select export type');
        return;
    }

    if (exportType === 'Excel') {
        exportCustomerDetailExcel();
    } else if (exportType === 'PDF') {
        exportCustomerDetailPdf();
    } else {
        alert('Invalid export type');
    }
}

function exportCustomerDetailExcel() {
    if (!custDetailState.filteredRows.length) {
        return;
    }

    var cols = getDetailColumns();
    var header = $.map(cols, function (c) { return '<th>' + escapeHtml(c.label) + '</th>'; }).join('');
    var rows = '';

    $.each(custDetailState.filteredRows, function (_, item) {
        var cells = $.map(cols, function (c) {
            var val = getDetailCellValue(item, c.keys);
            return c.isAmount
                ? '<td style="text-align:right;">' + formatNumber(val) + '</td>'
                : '<td>' + escapeHtml(String(val == null ? '' : val)) + '</td>';
        });
        rows += '<tr>' + cells.join('') + '</tr>';
    });

    var html = '<html><head><meta charset="utf-8"/></head><body>'
        + "<table border='1'><thead><tr>" + header + "</tr></thead><tbody>" + rows + "</tbody></table>"
        + '</body></html>';

    var blob = new Blob(['\ufeff' + html], { type: 'application/vnd.ms-excel;charset=utf-8;' });
    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);

    var custName = getQueryParam('customerName') || 'Customer';
    link.download = custName.replace(/[^a-z0-9]/gi, '_') + '_OverdueDetail.xls';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}

function exportCustomerDetailPdf() {
    if (!custDetailState.filteredRows.length) {
        return;
    }

    var cols = getDetailColumns();
    var headerCells = $.map(cols, function (c) {
        return '<th style="padding:6px 8px;border:1px solid #ddd;text-align:' + (c.isAmount ? 'right' : 'left') + ';">'
            + escapeHtml(c.label) + '</th>';
    }).join('');

    var rows = '';
    $.each(custDetailState.filteredRows, function (_, item) {
        var cells = $.map(cols, function (c) {
            var val = getDetailCellValue(item, c.keys);
            return '<td style="padding:6px 8px;border:1px solid #ddd;text-align:' + (c.isAmount ? 'right' : 'left') + ';">'
                + (c.isAmount ? formatNumber(val) : escapeHtml(String(val == null ? '' : val))) + '</td>';
        });
        rows += '<tr>' + cells.join('') + '</tr>';
    });

    var custName = getQueryParam('customerName') || '';

    var printWindow = window.open('', '_blank');
    if (!printWindow) {
        alert('Please allow popups to export PDF');
        return;
    }

    printWindow.document.write('<html><head><title>Customer Overdue Detail</title></head><body>');
    if (custName) {
        printWindow.document.write('<h3>Customer: ' + escapeHtml(custName) + ' - Overdue Detail</h3>');
    }
    printWindow.document.write('<table style="border-collapse:collapse;width:100%;font-family:Arial,sans-serif;">');
    printWindow.document.write('<thead><tr>' + headerCells + '</tr></thead>');
    printWindow.document.write('<tbody>' + rows + '</tbody>');
    printWindow.document.write('</table></body></html>');
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
}



function getQueryParam(name) {
    var url = window.location.href;
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)', 'i');
    var results = regex.exec(url);
    if (!results || !results[2]) {
        return '';
    }

    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

function formatNumber(value) {
    var number = parseFloat(value || 0);
    if (isNaN(number)) {
        number = 0;
    }

    return number.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeAttr(value) {
    return String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function escapeHtml(value) {
    if (!value) {
        return '';
    }

    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
