var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    //var fDate = "";
    //var tDate = "";

    SalesPerformance();
    //InquiryManagement(fDate, tDate);

});

function SalesPerformance() {
    debugger

    $.ajax({
        url: '/SPReports/GetSalesPerformance',
        type: 'GET',
        contentType: 'application/json',
        data: {},
        success: function (data) {
            $('#tblSalesPerformanceBranch').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr data-item="${itemJson}"><td class="branchname"><a class="cursor-pointer">` + item.Branch_Name + `</a></td><td>` + item.Annual_target + `</td><td>` + item.June_Target + `</td><td>` + item.June_Sales + `</td><td>` + item.June_Percent + `</td><td>` + item.Up_to_June_Sales + `</td><td>` + item.Annual_Percent +`</td></tr>`;
                });
            }

            $('#tblSalesPerformanceBranch').append(rowData);
        },

    });

}

$(document).on('click', '.branchname', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const BranchName = item.Branch_Name;
    $("#SalesPerson").val(BranchName);
    BranchWiseProduct(BranchName);
});

function BranchWiseProduct(BranchName) {
   

    $.ajax({
        url: '/SPReports/GetBranchWiseProduct',
        type: 'GET',
        contentType: 'application/json',
        data: {
            BranchName: BranchName
        },
        success: function (data) {
            $("#tbleSalesPerformanceProduct").empty();
            $('#DetailsModal').modal('show');
            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    rowData += `<tr><td>` + item.Product_Name + `</td><td>` + item.Annual_target + `</td><td>` + item.June_Target + `</td><td>` + item.June_Sales + `</td><td>` + item.June_Percent + `</td><td>` + item.Up_to_June_Sales + `</td><td>` + item.Annual_Percent + `</td></tr>`;

                });
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";

            }
            $("#tbleSalesPerformanceProduct").append(rowData);


        }

    });
}