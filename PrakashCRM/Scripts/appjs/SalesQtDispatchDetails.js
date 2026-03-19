var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

$(document).ready(function () {
    var fDate = "";
    var tDate = "";
    var txtSearch = "";
    DispatchDetails(fDate, tDate, txtSearch);

});

function DispatchDetails(fDate, tDate, txtSearch) {
    debugger
    var fromDate = fDate;
    var toDate = tDate;
    var Search = txtSearch;
    $.ajax({
        url: '/SPSalesQuotes/GetDispatchDetails',
        type: 'GET',
        contentType: 'application/json',
        data: {
            FromDate: fromDate,
            ToDate: toDate,
            Search: Search
        },
        success: function (data) {
            $('#tblDispatchDetails').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += "<tr><td>" + item.No + "</td><td>" + item.Posting_Date + "</td><td>" + item.Customer_No + "</td><td>" + item.Customer_Name + "</td><td>" + item.Vehicle_No + "</td><td>" + item.LRNo
                        + "</td><td>" + item.Quote_No + `</td><td class="open-modal" data-item="${itemJson}"><a class='order_no cursor-pointer'>` + item.Order_No + "</a></td></tr>";
                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblDispatchDetails').append(rowData);
        },

    });

}

$("#btnClearFilter").on('click', function () {

    ClearDispatchFilter();
});

function ClearDispatchFilter() {
    var fDate = "";
    var tDate = "";
    var txtSearch = "";
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#TxtSearch").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");

    DispatchDetails(fDate, tDate, txtSearch);
}

$('#SearchBtn').on('click', function () {

    var fDate = $("#FromDate").val();

    var tDate = $("#ToDate").val();

    let txtSearch = $("#TxtSearch").val();

    if (fDate == "" || tDate == "" || txtSearch == "") {

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

    else if (fDate != "" || tDate != "" || txtSearch != "") {

        $("#Fdatevalidate").text("");

        $("#Tdatevalidate").text("");

    }

    DispatchDetails(fDate, tDate, txtSearch);

});



$(document).on("click", ".open-modal", function () {
    const itemJson = $(this).attr("data-item");
    const item = JSON.parse(itemJson);
    openModal(item);
});
function openModal(item) {
    var rowData = "";
    $('#DetailsModal').modal('show');
    $("#tbleOrderDetails").empty();
    rowData += "<tr><td>" + item.Item_No + "</td><td>" + item.Invoice_Qty + "</td><td>" + item.Total_Qty + "</td><td>" + item.Price + "</td></tr>";
    $("#tbleOrderDetails").append(rowData);
}

