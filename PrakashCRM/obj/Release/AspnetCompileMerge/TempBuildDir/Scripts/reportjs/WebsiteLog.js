var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    var fDate = "";
    var tDate = "";
    
    WebsiteLog(fDate, tDate);

});

function WebsiteLog(fDate, tDate) {
    debugger
    var fromDate = fDate;
    var toDate = tDate;

    $.ajax({
        url: '/SPReports/GetWebsiteLog',
        type: 'GET',
        contentType: 'application/json',
        data: {
            FromDate: fromDate,
            ToDate: toDate
        },
        success: function (data) {
            $('#tblWebsiteLog').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += "<tr><td>" + item.First_Name + "</td><td>" + item.Email + "</td><td>" + item.Phone_No + "</td><td>" + item.Last_Modified_At + "</td></tr>";
                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $('#tblWebsiteLog').append(rowData);
        },

    });

}

$("#btnClearFilter").on('click', function () {

    ClearWebsiteLogFilter();
});

function ClearWebsiteLogFilter() {
    var fDate = "";
    var tDate = "";
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");

    WebsiteLog(fDate, tDate);
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

    WebsiteLog(fDate, tDate);

});

