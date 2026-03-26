var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    BusinessTypeList();
});

function BusinessTypeList() {
    debugger
    $.ajax({
        url: '/SPReports/GetBusinessTypeSalesPerfomanceList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblBusinessTypesSalesPerformanceDetails").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                   
                    rowData += "<tr><td>" + item.Business_Type_Sales_Performance + "</td><td>" + item.Number_of_Customer + "</td><td>" + item.Sales_in_CR + "</td><td>"
                        + item.Percentage + "</td></tr>";
                    

                });
               
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblBusinessTypesSalesPerformanceDetails").append(rowData);
        }
    });
}