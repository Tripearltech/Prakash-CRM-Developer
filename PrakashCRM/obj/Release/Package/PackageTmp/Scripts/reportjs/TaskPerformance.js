var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    //var fDate = "";
    //var tDate = "";

    TaskPerformance();
    //InquiryManagement(fDate, tDate);

});

function TaskPerformance() {
    debugger
    //var fromDate = fDate;
    //var toDate = tDate;

    $.ajax({
        url: '/SPReports/GetTaskPerformance',
        type: 'GET',
        contentType: 'application/json',
        data: {},
        success: function (data) {
            $('#tblSalesPerson').empty();
            var rowData = "";

            if (data.TaskPerformancesList && data.TaskPerformancesList.length > 0) {
                $.each(data.TaskPerformancesList, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr data-item="${itemJson}"><td class="SalesPerson"><a class="cursor-pointer">` + item.SalesPerson_Name + `</a></td></tr>`;
                });
            }

            $('#tblSalesPerson').append(rowData);
            $("#tblemployeeTaskPerformance").empty();
            var employeeRow = "";
            if (data.TaskPerformanceReportingList && data.TaskPerformanceReportingList.length > 0) {
                $.each(data.TaskPerformanceReportingList, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    employeeRow += `<tr data-item="${itemJson}"><td class="SalesPerson"><a class="cursor-pointer">` + item.SalesPerson_Name + `</a></td></tr>`;
                });
            }
            $("#tblemployeeTaskPerformance").append(employeeRow);

        },

    });

}

$(document).on('click', '.SalesPerson', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const salespersonName = item.SalesPerson_Name;
    $("#SalesPerson").val(salespersonName);
    PerfomanceTask(salespersonName, "", "");
});

function PerfomanceTask(salespersonName) {


    $.ajax({
        url: '/SPReports/GetSalesWiseTaskPerformance',
        type: 'GET',
        contentType: 'application/json',
        data: {
            SalesPersonName: salespersonName
        },
        success: function (data) {
            $("#tbleTaskPerformance").empty();
            $('#DetailsModal').modal('show');
            var rowData = "";
            var per = "";
            var pending = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    if (item.Percentage == "0") {
                        per = "";
                    }
                    else {
                        per = item.Percentage
                    }
                    if (item.Pending == "0") {
                        pending = "";
                    }
                    else {
                        pending = item.Pending
                    }

                    rowData += `<tr><td>` + item.Daily_Visit_in_Last_Monthwise + "</td><td>" + item.Targeted + "</td><td>" + item.Achievement + "</td><td>" + per + "</td><td>" + pending +"</td></tr>";

                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";

            }
            $("#tbleTaskPerformance").append(rowData);


        }

    });
}