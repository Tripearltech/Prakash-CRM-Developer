var custOverdueState = {
    allCustomers: [],
    filteredCustomers: [],
    currentPage: 1,
    pageSize: 15,
    isSpDueAmt: false,
    customerSearch: ''
};

$(document).ready(function () {
    initializeCustOverdueReport();
});

function initializeCustOverdueReport() {
    custOverdueState.isSpDueAmt = getBooleanQueryParam('isSpDueAmt');
    setAsOnDate();
    bindCustOverdueEvents();
    loadCustomerSummary();
}

function setAsOnDate() {
    var today = new Date();
    var yyyy = today.getFullYear();
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var dd = String(today.getDate()).padStart(2, '0');
    $('#txtAsOnDate').val(yyyy + '-' + mm + '-' + dd);
}

function bindCustOverdueEvents() {
    $('#btnCustomerSearch').off('click.custOverdue').on('click.custOverdue', function () {
        hideCustomerSuggestions();
        applyCustomerFilter();
    });

    $('#txtCustomerSearch').off('keypress.custOverdue').on('keypress.custOverdue', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            hideCustomerSuggestions();
            applyCustomerFilter();
        }
    });

    $('#txtCustomerSearch').off('input.custOverdue').on('input.custOverdue', function () {
        var val = ($(this).val() || '').trim().toLowerCase();
        if (!val) {
            hideCustomerSuggestions();
            return;
        }

        var matches = $.grep(custOverdueState.allCustomers, function (item) {
            return (item.Customer_Name || '').toLowerCase().indexOf(val) > -1;
        });

        renderCustomerSuggestions(matches);
    });

    $(document).off('click.custSuggest', '#dlCustomerSearch li').on('click.custSuggest', '#dlCustomerSearch li', function () {
        var val = ($(this).data('value') || '').toString();
        $('#txtCustomerSearch').val(val);
        hideCustomerSuggestions();
        custOverdueState.customerSearch = val;
        loadCustomerSummary(1);
    });

    $(document).off('click.custSuggestHide').on('click.custSuggestHide', function (e) {
        if (!$(e.target).closest('#txtCustomerSearch, #dlCustomerSearch').length) {
            hideCustomerSuggestions();
        }
    });

    $('#btnCustomerExport').off('click.custOverdue').on('click.custOverdue', function () {
        exportCustomerSummary();
    });

    $('#btnCustomerRefresh').off('click.custOverdue').on('click.custOverdue', function () {
        resetCustomerFilters();
    });

}

function loadCustomerSummary() {
    var spCode = ($('#hdnLoggedInSPCode').val() || $('[name="hdnLoggedInSPCode"]').val() || '').trim();
    var loggedInSalesCode = spCode;

    if (!loggedInSalesCode) {
        $('#tblCustomerSummary').html("<tr><td colspan='2' class='text-center'>Salesperson session not found. Please login again.</td></tr>");
        updateCustomerSummaryTotal([]);
        return;
    }

    toggleCustOverdueLoader(true);

    $.ajax({
        url: '/SPReports/GetCSOutstandingDuelist',
        type: 'GET',
        contentType: 'application/json',
        data: {
            spCode: loggedInSalesCode,
            isSpDueAmt: custOverdueState.isSpDueAmt
        },
        success: function (response) {
            var rows = response || [];

            if (custOverdueState.isSpDueAmt) {
                rows = $.grep(rows, function (item) {
                    return parseFloat(item.Overdue_Days || 0) > 0;
                });
            }

            custOverdueState.allCustomers = buildCustomerSummary(rows);
            custOverdueState.filteredCustomers = filterCustomerSummary(custOverdueState.allCustomers, custOverdueState.customerSearch);

            renderCustomerSummaryTable(custOverdueState.filteredCustomers);
            toggleCustOverdueLoader(false);
        },
        error: function () {
            $('#tblCustomerSummary').html("<tr><td colspan='2' class='text-center'>Error loading customer data</td></tr>");
            updateCustomerSummaryTotal([]);
            toggleCustOverdueLoader(false);
        }
    });
}

function toggleCustOverdueLoader(show) {
    var $loader = $('#custOverdueLoader');
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

function renderCustomerSuggestions(list) {
    var $ul = $('#dlCustomerSearch');

    if (!list || list.length === 0) {
        $ul.hide();
        return;
    }

    var html = '';
    $.each(list.slice(0, 50), function (_, item) {
        var name = item.Customer_Name || '';
        html += '<li data-value="' + escapeAttr(name) + '" style="padding:8px 12px;cursor:pointer;" class="suggestion-item">'
            + escapeHtml(name) + '</li>';
    });

    $ul.html(html).show();
}

function buildCustomerSummary(items) {
    var grouped = {};

    $.each(items, function (_, item) {
        var name = (item.Customer_Name || '').trim();
        if (!name) {
            return;
        }

        var amt = parseFloat(item.Remaining_Amt || item.Remain_Amount || 0);
        if (isNaN(amt)) {
            amt = 0;
        }

        if (!grouped[name]) {
            grouped[name] = 0;
        }

        grouped[name] += amt;
    });

    var result = [];
    $.each(grouped, function (name, amount) {
        result.push({ Customer_Name: name, Remain_Amount: amount });
    });

    result.sort(function (a, b) {
        return a.Customer_Name.localeCompare(b.Customer_Name);
    });

    return result;
}

function filterCustomerSummary(customers, searchText) {
    var normalizedSearch = (searchText || '').trim().toLowerCase();

    if (!normalizedSearch) {
        return customers.slice();
    }

    return $.grep(customers, function (item) {
        return (item.Customer_Name || '').toLowerCase().indexOf(normalizedSearch) > -1;
    });
}

function hideCustomerSuggestions() {
    $('#dlCustomerSearch').hide().html('');
}

function applyCustomerFilter() {
    custOverdueState.customerSearch = ($('#txtCustomerSearch').val() || '').trim();
    custOverdueState.filteredCustomers = filterCustomerSummary(custOverdueState.allCustomers, custOverdueState.customerSearch);
    renderCustomerSummaryTable(custOverdueState.filteredCustomers);
}

function resetCustomerFilters() {
    $('#txtCustomerSearch').val('');
    $('#ddlCustomerExportType').val('');

    setAsOnDate();
    hideCustomerSuggestions();

    custOverdueState.customerSearch = '';

    custOverdueState.filteredCustomers = custOverdueState.allCustomers.slice();
    renderCustomerSummaryTable(custOverdueState.filteredCustomers);
}

function renderCustomerSummaryTable(customers) {
    var $body = $('#tblCustomerSummary');
    $body.empty();

    updateCustomerSummaryTotal(customers || []);

    if (!customers || customers.length === 0) {
        $body.html("<tr><td colspan='2' class='text-center'>No customer data found</td></tr>");
        return;
    }

    var html = '';
    $.each(customers, function (_, item) {
        html += "<tr>" +
            "<td><a href='#' class='cust-link' onclick=\"openCustomerDetailReport('" + escapeJsValue(item.Customer_Name) + "');return false;\">" + escapeHtml(item.Customer_Name) + "</a></td>" +
            "<td class='text-end'>" + formatNumber(item.Remain_Amount) + "</td>" +
            "</tr>";
    });

    $body.html(html);
}

function updateCustomerSummaryTotal(customers) {
    var total = 0;

    $.each(customers, function (_, item) {
        var amount = parseFloat(item.Remain_Amount || item.Remaining_Amt || 0);
        if (!isNaN(amount)) {
            total += amount;
        }
    });

    $('#lblRemainAmountTotal').text(formatNumber(total));
}

function openCustomerDetailReport(customerName) {
    var asOnDate = $('#txtAsOnDate').val() || '';
    var detailUrl = '/SPReports/CustOverDueOutstandingDetail?customerName=' + encodeURIComponent(customerName || '')
        + '&asOnDate=' + encodeURIComponent(asOnDate)
        + '&isSpDueAmt=' + (custOverdueState.isSpDueAmt ? 'true' : 'false');
    window.location.href = detailUrl;
}

function getBooleanQueryParam(name) {
    return (getQueryParam(name) || '').toLowerCase() === 'true';
}

function getQueryParam(name) {
    var params = new URLSearchParams(window.location.search || '');
    return params.get(name) || '';
}

function exportCustomerSummary() {
    if (!custOverdueState.filteredCustomers || custOverdueState.filteredCustomers.length === 0) {
        alert('No data to export');
        return;
    }

    var exportType = ($('#ddlCustomerExportType').val() || '').trim();
    if (!exportType) {
        alert('Please select export type');
        return;
    }

    if (exportType === 'Excel') {
        exportCustomerSummaryExcel();
    } else if (exportType === 'PDF') {
        exportCustomerSummaryPdf();
    } else {
        alert('Invalid export type selected');
    }
}

function exportCustomerSummaryExcel() {
    var tableHtml = "<table border='1'><tr><th>Customer Name</th><th>Remain Amount (Rs)</th></tr>";

    $.each(custOverdueState.filteredCustomers, function (_, item) {
        tableHtml += '<tr><td>' + escapeHtml(item.Customer_Name || '') + '</td>'
            + '<td style="text-align:right;">' + formatNumber(item.Remain_Amount) + '</td></tr>';
    });

    var total = 0;
    $.each(custOverdueState.filteredCustomers, function (_, item) {
        var amount = parseFloat(item.Remain_Amount || 0);
        if (!isNaN(amount)) {
            total += amount;
        }
    });

    tableHtml += '<tr><td style="font-weight:bold;">Total</td><td style="text-align:right;font-weight:bold;">' + formatNumber(total) + '</td></tr>';
    tableHtml += '</table>';

    var html = '<html><head><meta charset="utf-8" /></head><body>' + tableHtml + '</body></html>';
    var blob = new Blob(['\ufeff' + html], { type: 'application/vnd.ms-excel;charset=utf-8;' });
    var link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'CustomerOverdueSummary.xls';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
}

function exportCustomerSummaryPdf() {
    var rowsHtml = '';
    var total = 0;

    $.each(custOverdueState.filteredCustomers, function (_, item) {
        var amount = parseFloat(item.Remain_Amount || 0);
        if (!isNaN(amount)) {
            total += amount;
        }

        rowsHtml += '<tr>'
            + '<td style="padding:6px 8px;border:1px solid #ddd;">' + escapeHtml(item.Customer_Name || '') + '</td>'
            + '<td style="padding:6px 8px;border:1px solid #ddd;text-align:right;">' + formatNumber(item.Remain_Amount) + '</td>'
            + '</tr>';
    });

    rowsHtml += '<tr>'
        + '<td style="padding:6px 8px;border:1px solid #ddd;font-weight:bold;">Total</td>'
        + '<td style="padding:6px 8px;border:1px solid #ddd;text-align:right;font-weight:bold;">' + formatNumber(total) + '</td>'
        + '</tr>';

    var printWindow = window.open('', '_blank');
    if (!printWindow) {
        alert('Please allow popups to export PDF');
        return;
    }

    printWindow.document.write('<html><head><title>Customer Overdue Summary</title></head><body>');
    printWindow.document.write('<h3>Customer Overdue Summary</h3>');
    printWindow.document.write('<table style="border-collapse:collapse;width:100%;font-family:Arial,sans-serif;">');
    printWindow.document.write('<thead><tr><th style="padding:6px 8px;border:1px solid #ddd;text-align:left;">Customer Name</th><th style="padding:6px 8px;border:1px solid #ddd;text-align:right;">Remain Amount (Rs)</th></tr></thead>');
    printWindow.document.write('<tbody>' + rowsHtml + '</tbody>');
    printWindow.document.write('</table></body></html>');
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
    printWindow.close();
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

function escapeJsValue(value) {
    if (!value) {
        return '';
    }

    return String(value).replace(/\\/g, '\\\\').replace(/'/g, "\\'");
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
