var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    IndustryWiseList();
});

function IndustryWiseList() {
    debugger
    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetIndustryWiseSalesPerfomanceList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblIndustryWiseSalesPerformanceDetails").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                   
                    rowData += "<tr><td>" + item.Industrial_Type + "</td><td>" + item.Number_of_Customer + "</td><td>" + item.Sales_in_CR + "</td><td>"
                        + item.Percentage + "</td></tr>";
                    

                });
              }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblIndustryWiseSalesPerformanceDetails").append(rowData);
        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }
    });
}