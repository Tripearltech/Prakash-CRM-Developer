var apiUrl = $('#getServiceApiUrl').val() + 'SPOutstandingPayment/';

$(document).ready(function () {
    
    //GenerateCustData();
    BindOutstandingCustomerList();
});

//function GenerateCustData() {
//    $('#divImage').show();
//    var fromDate = $('#txtOutsFDate').val();
//    if (fromDate !== "" && toDate !== "") {
//        $.post(apiUrl + 'GenerateCollData?FromDate=' + fromDate,
//            function (data) {
//                if (data) {
//                    BindOutstandingCustomerList();
//                }
//            }
//        );
//    }
//}

function BindOutstandingCustomerList() {
    $.ajax({
        url: '/SPOutstandingPayment/GetCustomerCollectionOut',
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

            let receivedRows = `<tr><td></td><td class="left-align bold">Collection up to <span id="txtInvTDate"></span><span class="blue">(To Date)</span></td><td></td><td></td><td></td><td>${totalReceivedAmount}</td><td></td><td></td></tr>`;
            $("#tblOutstanding").append(html);
            $("#tblrecivedData").append(receivedRows);
        }
    });
}

function loadSixMonthData(customerNo) {

    $("#acdBody").html(`<tr><td colspan='19'>Loading...</td></tr>`);

    $.ajax({
        url: '/SPOutstandingPayment/GetCustomerSexMonthData?customerNo=' + customerNo,
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
