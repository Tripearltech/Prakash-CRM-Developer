var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    var fDate = "";
    var tDate = "";

    DailyVisitMonthWise(fDate, tDate);

});

function DailyVisitMonthWise(fDate, tDate) {
    debugger
    var fromDate = fDate;
    var toDate = tDate;

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetDailyVisitMonthWise',
        type: 'GET',
        contentType: 'application/json',
        data: {
            FromDate: fromDate,
            ToDate: toDate
        },
        success: function (data) {
            $('#tblDailyVisitMonthWise').empty();
            var rowData = "";

            if (data.DailyVisitMonthWises && data.DailyVisitMonthWises.length > 0) {
                $.each(data.DailyVisitMonthWises, function (index, item) {

                    rowData += "<tr><td>" + item.SalesPerson_Name + "</td><td>" + item.Month_Year + "</td><td>" + item.Total_Time_HH_MM + "</td><td>" + item.No_of_Visit_Personal + "</td><td>" + item.Total_Kilometers + "</td></tr>";
                });
            }
            //else {
            //    rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            //}
            $('#tblDailyVisitMonthWise').append(rowData);
            $('#tblEmpDailyVisitMonthWise').empty();
            var employeeRow = "";
            if (data.EmployeeDailyVisitMonthWise && data.EmployeeDailyVisitMonthWise.length > 0) {
                $.each(data.EmployeeDailyVisitMonthWise, function (index, item) {
                    employeeRow += "<tr><td>" + item.SalesPerson_Name + "</td><td>" + item.Month_Year + "</td><td>" + item.Total_Time_HH_MM + "</td><td>" + item.No_of_Visit_Personal + "</td><td>" + item.Total_Kilometers + "</td></tr>";
                });
            }
            $('#tblEmpDailyVisitMonthWise').append(employeeRow);

        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }

    });

}

$("#btnClearFilter").on('click', function () {

    ClearDispatchFilter();
});

function ClearDispatchFilter() {
    var fDate = "";
    var tDate = "";

    $("#FromDate").val('');
    $("#ToDate").val('');
    /* $("#TxtSearch").val('');*/
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");

    DailyVisitMonthWise(fDate, tDate);
}

$('#SearchBtn').on('click', function () {

    var fDate = $("#FromDate").val();

    var tDate = $("#ToDate").val();

    if (fDate == "" || tDate == "") {


        if (fDate == "" && tDate == "") {
            $("#Fdatevalidate").text("From Date is Required");
            $("#Tdatevalidate").text("To Date is Required");

        }
        else if (fDate == "" && tDate != "") {

            $("#Fdatevalidate").text("From Date is Required");
            $("#Tdatevalidate").text("");


        }

        else if (fDate != "" && tDate == "") {

            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("To Date is Required");

        }
        else {
            $("#Fdatevalidate").text("");

            $("#Tdatevalidate").text("");
            DailyVisitMonthWise(fDate, tDate);

        }



    }

    else if (fDate != "" && tDate != "") {

        if (fDate != null) {

            $("#Fdatevalidate").text("");

        } if (tDate != null) {

            $("#Tdatevalidate").text("");

        }
        DailyVisitMonthWise(fDate, tDate);

    }
    //DailyVisitMonthWise(fDate, tDate);

});