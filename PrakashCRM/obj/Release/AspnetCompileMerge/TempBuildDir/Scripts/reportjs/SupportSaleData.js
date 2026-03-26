var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    var fDate = "";
    var tDate = "";
    SupportSaleDataList(fDate, tDate);
});

function SupportSaleDataList(fDate, tDate) {
    debugger
    $.ajax({
        url: '/SPReports/GetSupportSaleDataList',
        data: { FDate: fDate, TDate: tDate},
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblSupportSaleData").empty();

            var rowData = "";
            var ManagedBy = "";
            if (data.SupportSaleDatas && data.SupportSaleDatas.length > 0) {
                $.each(data.SupportSaleDatas, function (index, item) {
                    if (item.Primary == 'true' && item.Support == 'false') {
                        ManagedBy = 'Primary';
                    }
                    else if (item.Primary == 'false' && item.Support == 'true') {
                        ManagedBy = 'Support';
                    }
                    if (item.Primary == 'false' && item.Support == 'false') {
                        ManagedBy = '';
                    }


                    rowData += "<tr><td>" + item.Date + "</td><td>" + item.Customer_Name + "</td><td>" + item.Contact_Name + "</td><td>" + item.Item_Description + "</td><td>" + item.Primary_Salesperson_Name + "</td><td>" + item.Secondary_Salesperson_Name + "</td><td>" + ManagedBy + "</td><td>" + item.Total_Quantity + "</td></tr>";


                });

            }
            $("#tblSupportSaleData").append(rowData);

            $("#tblSupportRepotingSaleData").empty();

            var Rowdata = "";
            if (data.SupportReportingSaleDatas && data.SupportReportingSaleDatas.length > 0) {
                $.each(data.SupportReportingSaleDatas, function (index, item) {
                    if (item.Primary == 'true' && item.Support == 'false') {
                        ManagedBy = 'Primary';
                    }
                    else if (item.Primary == 'false' && item.Support == 'true') {
                        ManagedBy = 'Support';
                    }
                    if (item.Primary == 'false' && item.Support == 'false') {
                        ManagedBy = '';
                    }


                    Rowdata += "<tr><td>" + item.Date + "</td><td>" + item.Customer_Name + "</td><td>" + item.Contact_Name + "</td><td>" + item.Item_Description + "</td><td>" + item.Primary_Salesperson_Name + "</td><td>" + item.Secondary_Salesperson_Name + "</td><td>" + ManagedBy + "</td><td>" + item.Total_Quantity + "</td></tr>";


                });

            }
            $("#tblSupportRepotingSaleData").append(Rowdata);

        }
    });
}
$('#SearchBtn').on('click', function () {
    var fDate = $("#FromDate").val();
    var tDate = $("#ToDate").val();
   
    if (fDate == "" || tDate == "" ) {

        if (fDate == "" && tDate != "") {

            $("#Fdatevalidate").text("From Date is Required");
            $("#Tdatevalidate").text("");

        }

        else if (fDate != "" && tDate == "") {

            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("To Date is Required");

        }
        else if (fDate == "" && tDate == "") {

            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("");
            SupportSaleDataList(fDate, tDate);

        }
       
    }
    else if (fDate != "" && tDate != "") {

        if (fDate != null) {

            $("#Fdatevalidate").text("");

        } if (tDate != null) {

            $("#Tdatevalidate").text("");

        }

    }
    if (fDate != "" && tDate != "") {

        $("#Fdatevalidate").text("");

        $("#Tdatevalidate").text("");

        SupportSaleDataList(fDate, tDate);
    }
});

$("#btnClearFilter").on('click', function () {
    FeedBackListClear();
});
function FeedBackListClear() {

    $("#TxtSearch").val('');
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");
    SupportSaleDataList("", "", "");
}
//function BindCustomerDropdown_autocomplete(textSearch) {
//    var Search = textSearch != null ? textSearch.trim() : "";
//    if (typeof ($.fn.autocomplete) === 'undefined') return;

//    var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/GetCustomerDropdown?Search' + Search;
//    $.get(apiUrl, function (data) {
//        if (data != null) {
//            var customerArray = [];
//            for()
//        }
//    }
