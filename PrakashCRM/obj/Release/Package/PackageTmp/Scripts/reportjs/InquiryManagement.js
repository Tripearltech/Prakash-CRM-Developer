var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    //var fDate = "";
    //var tDate = "";

    InquiryManagement();
    //InquiryManagement(fDate, tDate);

});

function InquiryManagement() {
    debugger
    //var fromDate = fDate;
    //var toDate = tDate;

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetInquiryManagement',
        type: 'GET',
        contentType: 'application/json',
        data: {
            //FromDate: fromDate,
            //ToDate: toDate
        },
        success: function (data) {
            $('#tblInquiryManagement').empty();
            var rowData = "";

            if (data.InquiryManagements && data.InquiryManagements.length > 0) {
                $.each(data.InquiryManagements, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr data-item="${itemJson}"><td>` + item.Sales_Person_Name + `</td><td class="salesInquiry"><a class="cursor-pointer">` + item.Total_Inquiry + `</a></td><td class="salesQuotes"><a class="cursor-pointer">` + item.Total_Sales_Quote + "</a></td><td>" + item.Total_Send_SMS + "</td><td>" + item.Total_Send_Email + `</td><td class="confirmqoute"><a class="cursor-pointer">` + item.Total_Confirm_Quote + "</a></td><td>" + item.Inquiry_Conversion_Ratio + "</td><td>" + item.Quote_Conversion_Ratio + "</td><td>" + item.Quote_to_SMS + "</td><td>" + item.Quote_to_Email + "</td></tr>";
                });
            }

            $('#tblInquiryManagement').append(rowData);
            $("#tblemployeeInquiryManagement").empty();
            var employeeRow = "";
            if (data.EmployeeWiseInquiryManagements && data.EmployeeWiseInquiryManagements.length > 0) {
                $.each(data.EmployeeWiseInquiryManagements, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    employeeRow += `<tr data-item="${itemJson}"><td>` + item.Sales_Person_Name + `</td><td class="salesInquiry"><a class="cursor-pointer">` + item.Total_Inquiry + `</a></td><td class="salesQuotes"><a class="cursor-pointer">` + item.Total_Sales_Quote + "</a></td><td>" + item.Total_Send_SMS + "</td><td>" + item.Total_Send_Email + `</td><td class="confirmqoute"><a class="cursor-pointer">` + item.Total_Confirm_Quote + "</a></td><td>" + item.Inquiry_Conversion_Ratio + "</td><td>" + item.Quote_Conversion_Ratio + "</td><td>" + item.Quote_to_SMS + "</td><td>" + item.Quote_to_Email + "</td></tr>";
                });
            }
            $("#tblemployeeInquiryManagement").append(employeeRow);

        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }

    });

}

$(document).on('click', '.salesQuotes', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const salespersonName = item.Sales_Person_Name;
    $("#SalesPerson").val(salespersonName);
    SalesPersonQuotes(salespersonName, "", "");
});

function SalesPersonQuotes(salespersonName, fromDate, toDate) {

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }


    $.ajax({
        url: '/SPReports/GetSalesPesonQuotes',
        type: 'GET',
        contentType: 'application/json',
        data: {
            SalesPerson: salespersonName,
            FromDate: fromDate,
            ToDate: toDate
        },
        success: function (data) {
            $("#tbleSalesPesonQuotes").empty();
            $('#DetailsModal').modal('show');
            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += `<tr><td>` + item.No + "</td><td>" + item.Document_Date + "</td><td>" + item.Sell_to_Customer_Name + "</td><td>" + item.Sell_to_Contact + "</td><td>" + item.Due_Date + "</td><td>" + item.Amount + "</td></tr>";

                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";

            }
            $("#tbleSalesPesonQuotes").append(rowData);


        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }

    });
}


$(document).on('click', '.salesInquiry', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const salespersonName = item.Sales_Person_Name;
    $("#SalesPerson").val(salespersonName);

    SalesPersonInquiry(salespersonName, "", "");
});
function SalesPersonInquiry(salespersonName, fromDate, toDate) {
    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetSalesPesonInquiry',
        type: 'GET',
        contentType: 'application/json',
        data: {
            SalesPerson: salespersonName,
            FromDate: fromDate,
            ToDate: toDate
        },
        success: function (data) {
            $("#tbleSalesPesonInquiry").empty();
            $("#SalesPesonModal").modal('show');
            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.No + "</td><td>" + item.Document_Date + "</td><td>" + item.Sell_to_Customer_Name + "</td><td>" + item.Sell_to_Contact + "</td><td>" + item.Payment_Terms_Code + "</td><td>" + item.PCPL_Inquiry_Remarks + "</td><td>" + item.Ship_to_Code + "</td><td>" + item.Ship_to_Address + "</td><td>" + item.Ship_to_Address_2 + "</td><td>" + item.Ship_to_City + "</td><td>" + item.Ship_to_Country_Region_Code + "</td><td>"
                });
            }
            else {

                rowData = "<tr><td colspan='15' style='text-align:left;'>No Records Found</td></tr>";

            }
            $("#tbleSalesPesonInquiry").append(rowData);

        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }
    });
}

$(document).on('click', '.confirmqoute', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const salesPerson = item.Sales_Person_Name;
    $("#SalesPerson").val(salesPerson);
    ConfirmSalesQuotes(salesPerson, "", "");
});
function ConfirmSalesQuotes(salespersonName, fromDate, toDate) {

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }


    $.ajax({
        url: '/SPReports/GetSalesPesonConfirmQuotes',
        type: 'GET',
        contentType: 'application/json',
        data: {
            SalesPerson: salespersonName,
            FromDate: fromDate,
            ToDate: toDate
        },
        success: function (data) {
            $("#tbleSalesPesonConfirmQuotes").empty();
            $('#confirmModal').modal('show');
            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += `<tr><td>` + item.No + "</td><td>" + item.Document_Date + "</td><td>" + item.Sell_to_Customer_Name + "</td><td>" + item.Sell_to_Contact + "</td><td>" + item.Due_Date + "</td><td>" + item.Amount + "</td></tr>";

                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";

            }
            $("#tbleSalesPesonConfirmQuotes").append(rowData);


        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }

    });
}

$("#btnClear").on('click', function () {

    ClearSalesPesonConfirmQuotes();
});

$("#btnClearFilter").on('click', function () {

    ClearInquiryManagementFilter();
});

function ClearInquiryManagementFilter() {
    var fDate = "";
    var tDate = "";
    var SalesPerson = $("#SalesPerson").val();
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");

    SalesPersonQuotes(SalesPerson, fDate, tDate);
}

function ClearSalesPesonConfirmQuotes() {
    var fDate = "";
    var tDate = "";
    var SalesPerson = $("#SalesPerson").val();
    $("#FDate").val('');
    $("#TDate").val('');
    $("#Fdate").text("");
    $("#Tdate").text("");

    ConfirmSalesQuotes(SalesPerson, fDate, tDate);
}
$("#btnClearFilter1").on('click', function () {

    ClearInquiryManagementFilter1();
});

function ClearInquiryManagementFilter1() {
    var fDate = "";
    var tDate = "";
    var SalesPerson = $("#SalesPerson").val();
    $("#FromDate1").val('');
    $("#ToDate1").val('');
    $("#Fdatevalidate1").text("");
    $("#Tdatevalidate1").text("");

    SalesPersonInquiry(SalesPerson, fDate, tDate);
}

$('#SearchBtn').on('click', function () {

    var SalesPerson = $("#SalesPerson").val();
    var fDate = $("#FromDate").val();

    var tDate = $("#ToDate").val();

    if (fDate == "" || tDate == "") {

        if (fDate == "" || tDate == "") {

            if (fDate == "" && tDate != "") {

                $("#Fdatevalidate").text("From Date is Required");

            }

            else {

                $("#Fdatevalidate").text("");

            }

            if (fDate != "" && tDate == "") {

                $("#Tdatevalidate").text("To Date is Required");

            }

            else {

                $("#Tdatevalidate").text("");

            }

        }

        else if (fDate != "" && tDate != "") {

            if (fDate != null) {

                $("#Fdatevalidate").text("");

            } if (tDate != null) {

                $("#Tdatevalidate").text("");

            }

        }

    }

    else if (fDate != "" || tDate != "") {

        $("#Fdatevalidate").text("");

        $("#Tdatevalidate").text("");

    }

    SalesPersonQuotes(SalesPerson, fDate, tDate);

});$('#Search').on('click', function () {

    var SalesPerson = $("#SalesPerson").val();
    var fDate = $("#FDate").val();

    var tDate = $("#TDate").val();

    if (fDate == "" || tDate == "") {

        if (fDate == "" || tDate == "") {

            if (fDate == "" && tDate != "") {

                $("#Fdate").text("From Date is Required");

            }

            else {

                $("#Fdate").text("");

            }

            if (fDate != "" && tDate == "") {

                $("#Tdate").text("To Date is Required");

            }

            else {

                $("#Tdate").text("");

            }

        }

        else if (fDate != "" && tDate != "") {

            if (fDate != null) {

                $("#Fdate").text("");

            } if (tDate != null) {

                $("#Tdate").text("");

            }

        }

    }

    else if (fDate != "" || tDate != "") {

        $("#Fdate").text("");

        $("#Tdate").text("");

    }

    ConfirmSalesQuotes(SalesPerson, fDate, tDate);

});
$('#SearchBtn1').on('click', function () {

    var SalesPerson = $("#SalesPerson").val();
    var fDate = $("#FromDate1").val();

    var tDate = $("#ToDate1").val();

    if (fDate == "" || tDate == "") {

        if (fDate == "" || tDate == "") {

            if (fDate == "" && tDate != "") {

                $("#Fdatevalidate1").text("From Date is Required");

            }

            else {

                $("#Fdatevalidate1").text("");

            }

            if (fDate != "" && tDate == "") {

                $("#Tdatevalidate1").text("To Date is Required");

            }

            else {

                $("#Tdatevalidate1").text("");

            }

        }

        else if (fDate != "" && tDate != "") {

            if (fDate != null) {

                $("#Fdatevalidate1").text("");

            } if (tDate != null) {

                $("#Tdatevalidate1").text("");

            }

        }

    }

    else if (fDate != "" || tDate != "") {

        $("#Fdatevalidate1").text("");

        $("#Tdatevalidate1").text("");

    }

    SalesPersonInquiry(SalesPerson, fDate, tDate);

});