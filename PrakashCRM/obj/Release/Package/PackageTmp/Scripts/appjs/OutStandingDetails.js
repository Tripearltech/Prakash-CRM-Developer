var apiUrl = $('#getServiceApiUrl').val() + 'SPOutstandingPayment/';
var isGenerateInProgress = false;

$(document).ready(function () {
    if ($('#txtOutsFDate').val() === '') {
        $('#txtOutsFDate').val(getTodayDate());
    }

    updateCollectionUptoDateLabel($('#txtOutsFDate').val());

    $('#btnGenerate').on('click', function () {
        GenerateCustData();
    });

    $('#txtOutsFDate').on('change', function () {
        updateCollectionUptoDateLabel($(this).val());
    });

    setPageLoaderVisibility(true);
    BindOutstandingCustomerList(true);
});

function GenerateCustData() {
    var fromDate = $('#txtOutsFDate').val();

    if (fromDate === '') {
        if (typeof ShowErrMsg === 'function') {
            ShowErrMsg('Please select from date');
        }
        return;
    }

    isGenerateInProgress = true;
    setPageLoaderVisibility(true);
    updateCollectionUptoDateLabel(fromDate);

    $.post(apiUrl + 'GenerateCollData?FromDate=' + encodeURIComponent(fromDate))
        .done(function (data) {
            if (data === true || data === 'true') {
                BindOutstandingCustomerList(true);
            }
            else {
                isGenerateInProgress = false;
                setPageLoaderVisibility(false);
            }
        })
        .fail(function () {
            isGenerateInProgress = false;
            setPageLoaderVisibility(false);
        });
}

function BindOutstandingCustomerList(hideLoaderOnComplete) {
    var salespersonCode = $('#hdnLoggedInUserSPCode').val() || '';

    $.ajax({
        url: '/SPOutstandingPayment/GetCustomerCollectionOut?SPCode=' + encodeURIComponent(salespersonCode),
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {

            $("#tblOutstanding").empty();
            $("#tblrecivedData").empty();

            if (!data || data.length === 0) {
                $("#tblOutstanding").append(`<tr><td colspan="8" class="left-align">No Records Found</td></tr>`);
                return;
            }

            let html = "";
            let currentCustomer = "";
            let customerTotal = 0;
            let grandTotal = 0;
            let collectiondate = 0;
            let totalReceivedAmount = 0;

            $.each(data, function (i, item) {

                totalReceivedAmount += parseFloat(item.Received_Amount || 0);

                if (currentCustomer !== item.Customer_Name) {

                    if (currentCustomer !== "") {
                        html += `<tr class="total-row"><td></td><td class="left-align">Total ${currentCustomer}</td><td colspan="2"></td><td></td><td>${customerTotal.toFixed(2)}</td><td></td><td></td></tr>`;
                    }

                    customerTotal = 0;
                    currentCustomer = item.Customer_Name;

                    html += `<tr class="customer-row"><td><a class="clsPointer" onclick="toggleDetails(this)"><i class="bx bx-plus-circle"></i></a></td><td class="left-align">${currentCustomer}</td><td colspan="6"></td></tr>`;
                }

                html += `<tr class="toggle-detail" style="display:none;"><td></td><td></td><td>${item.Document_No}</td><td>${item.OverDays}</td><td>${item.Original_Amount}</td><td>${item.Remaining_Amount}</td><td>${item.Received_Amount}</td><td onmouseover="setCustomerTooltip(this)"onclick="openModal(); loadSixMonthData('${item.LastSixMonths_Customer_No}');">${item.ACD_Amt}</td></tr>`;

                customerTotal += parseFloat(item.Total_Customer_Amt || 0);
                grandTotal += parseFloat(item.Total_Period_Amt || 0);
                collectiondate += parseFloat(item.Total_Collection_Amt || 0);
            });

            html += `<tr class="total-row"><td></td><td class="left-align">Total ${currentCustomer}</td><td colspan="2"></td><td></td><td>${customerTotal.toFixed(2)}</td><td></td><td></td></tr>`;

            html += `<tr class="total-row"><td></td><td class="left-align">Total for the Period</td><td colspan="2"></td><td>${grandTotal.toFixed(2)}</td><td></td><td></td><td></td></tr>`;

            html += `<tr class="total-row"><td></td><td class="left-align">Collection upto Date</td><td colspan="2"></td><td>${collectiondate.toFixed(2)}</td><td></td><td></td><td></td></tr>`;

            let receivedRows = `<tr><td></td><td class="left-align bold">Collection up to <span id="txtInvTDate"></span><span class="blue">(To Date)</span></td><td></td><td></td><td></td><td>${totalReceivedAmount.toFixed(2)}</td><td></td><td></td></tr>`;
            $("#tblOutstanding").append(html);
            $("#tblrecivedData").append(receivedRows);
            updateCollectionUptoDateLabel($('#txtOutsFDate').val());
        },
        complete: function () {
            if (hideLoaderOnComplete === true || isGenerateInProgress) {
                isGenerateInProgress = false;
                setPageLoaderVisibility(false);
            }
        }
    });
}

function setPageLoaderVisibility(isVisible) {
    $('#divImage').hide();
    $('#btnGenerate').prop('disabled', isVisible === true);

    if (typeof showPageDataLoader === 'function' && typeof hidePageDataLoader === 'function') {
        if (isVisible === true) {
            showPageDataLoader();
        }
        else {
            hidePageDataLoader();
        }
    }
}

function updateCollectionUptoDateLabel(fromDate) {
    var previousDate = getPreviousDate(fromDate);
    $('#txtInvTDate').text(previousDate === '' ? '' : previousDate + ' ');
}

function getPreviousDate(fromDate) {
    if (fromDate == null || fromDate === '') {
        return '';
    }

    var dateParts = fromDate.split('-');

    if (dateParts.length !== 3) {
        return '';
    }

    var selectedDate = new Date(parseInt(dateParts[0], 10), parseInt(dateParts[1], 10) - 1, parseInt(dateParts[2], 10));
    selectedDate.setDate(selectedDate.getDate() - 1);

    var day = selectedDate.getDate().toString().padStart(2, '0');
    var month = (selectedDate.getMonth() + 1).toString().padStart(2, '0');
    var year = selectedDate.getFullYear();

    return day + '-' + month + '-' + year;
}

function getTodayDate() {
    var today = new Date();
    var day = today.getDate().toString().padStart(2, '0');
    var month = (today.getMonth() + 1).toString().padStart(2, '0');
    var year = today.getFullYear();

    return year + '-' + month + '-' + day;
}

function loadSixMonthData(customerNo) {
    var salespersonCode = $('#hdnLoggedInUserSPCode').val() || '';

    $("#acdBody").html(`<tr><td colspan='19'>Loading...</td></tr>`);

    $.ajax({
        url: '/SPOutstandingPayment/GetCustomerSexMonthData?customerNo=' + encodeURIComponent(customerNo) + '&SPCode=' + encodeURIComponent(salespersonCode),
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#acdBody").empty();

            var TROpts = "";

            $.each(data, function (index, item) {
                TROpts += "<tr><td>" + item.LastSixMonths_Document_No + "</td><td>" + item.LastSixMonths_Posting_Date + "</td><td>" + item.LastSixMonths_Original_Amt + "</td><td>" + item.LastSixMonths_DueDate + "</td><td>" + item.LastSixMonths_Received_Date + "</td><td>" + item.LastSixMonths_Received_Amt + "</td><td>" + item.LastSixMonths_ACD_Amt + "</td><td>" + item.LastSixMonths_ADD_Amt + "</td><td>" + item.LastSixMonths_No_of_Days + "</td></tr>";
            });

            $('#acdBody').append(TROpts);

        },
    });
}
