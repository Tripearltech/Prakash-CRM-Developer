var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';

$(document).ready(function () {
    //var fDate = "";
    //var tDate = "";

    StockManagement();
    //InquiryManagement(fDate, tDate);

});

function StockManagement() {
    debugger

    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetStockManagement',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $('#tblStockManagement').empty();
            var rowData = "";

            if (data.length > 0) {
                $.each(data, function (index, item) {
                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr data-item="${itemJson}"><td class="branchName"><a class="cursor-pointer">` + item.Branch + `</a></td><td>` + item.Inventory + `</td><td>` + item.Qty_on_Purch_Order + "</td><td>" + item.Qty_on_Sales_Order + "</td><td>" + item.Closing_Stock + "</td></tr>";
                });
            }
            $('#tblStockManagement').append(rowData);

        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }

    });

}

$(document).on('click', '.branchName', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const item = JSON.parse(itemJson);
    const branchName = item.Branch;
    $("#SalesPerson").val(branchName);
    BranchWiseProducts(branchName);
});
const ProductGroupsCode = {};
function BranchWiseProducts(branchName) {
    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.ajax({
        url: '/SPReports/GetBranchWiseProducts',
        type: 'GET',
        contentType: 'application/json',
        data: { BranchName: branchName },
        success: function (data) {
            $("#tbleStockProductManagement").empty();
            $("#ftableBody").empty();
            $('#DetailsModal').modal('show');
            var rowData = "";
            var rowData1 = "";

            let groupData = {};
            let totals = {};

            if (data.BranchProductWise.length > 0) {
                let productSet = new Set();

                $.each(data.BranchProductWise, function (index, item) {
                    if (item.Description) {
                        productSet.add(item.Description.trim());
                    }
                });

                // Convert to array
                let productList = Array.from(productSet);
                productList.sort((a, b) => a.localeCompare(b));
                // Bind dropdown
                let dropdown = $('#ddlProductSearch'); // 👈 apna dropdown id
                dropdown.empty();

                dropdown.append('<option value="">--Select Product--</option>');

                productList.forEach(function (prod) {
                    dropdown.append(`<option value="${prod}">${prod}</option>`);
                });
                $.each(data.BranchProductWise, function (index, item) {
                    let location = item.Location_Code;
                    if (!groupData[location]) {
                        groupData[location] = [];
                    }
                    if (item.Location_Total === "true") {
                        totals[location] = item;
                        //totals[item].push(item);
                    }
                    else {
                        groupData[location].push(item);
                    }

                });

                $.each(groupData, function (locations, records) {
                    $.each(records, function (i, item) {
                        var productId = `Product${i}SPRecs`;

                        rowData += `<tr data-branchindex="${i}" class="branch-row">
                       <td><a href="#" class="branch-toggle" data-target="${productId}"><i class="bx bx-plus-circle"></i></a></td>
                       <td>${item.Description}</td><td>${item.Location_Code}</td><td>${item.Inventory}</td>
                       <td>${item.Qty_on_Purch_Order}</td><td>${item.Qty_on_Sales_Order}</td>
                       <td>${item.Closing_Stock}</td>
                       <td></td>
                       <td></td>
                       <td></td>
                       <td></td>
                       </tr>`;

                    });
                    if (totals[locations]) {
                        let total = totals[locations];
                        rowData += `<tr class="branch-row">
               <td></td>
               <td style="font-weight: bold;">Location Total</td>
               <td>${total.Location_Code}</td><td>${total.Inventory}</td>
               <td>${total.Qty_on_Purch_Order}</td><td>${total.Qty_on_Sales_Order}</td>
               <td>${total.Closing_Stock}</td>
               <td></td>
               <td></td>
               <td></td>
               <td></td>
               </tr>`;

                    }
                });

                $("#tbleStockProductManagement").append(rowData);

            }
            if (data.BranchWiseTotalList.length > 0) {
                $.each(data.BranchWiseTotalList, function (index, item) {

                    rowData1 += `<tr>
             <td></td>
             <td style="font-weight: bold;">Total</td>
             <td></td><td>${item.Inventory}</td>
             <td>${item.Qty_on_Purch_Order}</td><td>${item.Qty_on_Sales_Order}</td>
             <td>${item.Closing_Stock}</td>
           <td>${item.Packing_Unit}</td><td>${item.Packing_MRP_Price}</td>
           <td>${item.Expected_Shipment_Qty}</td><td>${item.Expected_Receipt_Qty}</td>
         </tr>`;
                });
                $("#ftableBody").append(rowData1);
            }

            $("#tbleStockProductManagement").off('click.branch-toggle').on('click.branch-toggle', '.branch-toggle', function (e) {
                e.preventDefault();

                const $a = $(this);
                const $icon = $a.find('i');
                const $branchRow = $a.closest('tr.branch-row');
                const bi = $branchRow.data('branchindex');
                const branchName = data.BranchProductWise[bi].Location_Code;
                const product = data.BranchProductWise[bi].Description;
                const targetId = $a.data('target');

                const $subRows = $branchRow.nextUntil('tr.branch-row');

                if ($subRows.filter(':visible').length > 0) {


                    $branchRow.find('td:nth-child(4), td:nth-child(5), td:nth-child(6), td:nth-child(7)')
                        .css('font-weight', 'normal');

                    $subRows.slideUp(300);
                    $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
                } else {


                    $branchRow.find('td:nth-child(4), td:nth-child(5), td:nth-child(6), td:nth-child(7)')
                        .css('font-weight', 'bold');

                    if ($subRows.find('tr[data-packing="true"]').length === 0) {
                        $subRows.remove();
                        loadProductPackingStyle($branchRow, branchName, product, bi, targetId);
                    } else {
                        $subRows.slideDown(300);
                    }
                    $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');
                }
            });
        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }
    });


}

function loadProductPackingStyle($branchRow, branchName, product, bi, targetId) {
    $.ajax({
        url: `/SPReports/GetProductPackingStyle?BranchName=${encodeURIComponent(branchName)}&Product=${encodeURIComponent(product)}`,
        type: 'GET',
        success: function (packingstyle) {
            let rowsHtml = "";

            if (!packingstyle || packingstyle.length === 0) {
                rowsHtml = `<tr data-packing="true" style="padding-left:20px;"><td colspan="11">No Packing Styles Found</td></tr>`;
            } else {
                packingstyle.forEach((ps, psi) => {
                    const packingId = `${targetId}_Pack${psi}`;
                    rowsHtml += `
                        <tr data-packing="true" data-parent="${targetId}">
                            <td></td>
                            <td></td>
                            <td></td>
                           
                            <td>${ps.Inventory}</td><td>${ps.Qty_on_Purch_Order}</td>
                            <td>${ps.Qty_on_Sales_Order}</td><td>${ps.Closing_Stock}</td>
                            <td>${ps.Packing_Unit}</td><td>${ps.Packing_MRP_Price}</td>
                            <td>${ps.Expected_Shipment_Qty}</td><td>${ps.Expected_Receipt_Qty}</td>
                        </tr>`;
                });
            }

            $branchRow.after(rowsHtml);
        },
        error: function () {
            $branchRow.after('<tr data-packing="true"><td colspan="11" style="padding-left:20px;color:red;">Error loading packing styles</td></tr>');
        }
    });
}

$('#btnSearchProduct').click(function () {

    let val = $('#ddlProductSearch').val().toLowerCase();

    $('#tbleStockProductManagement tr').each(function () {
        let product = $(this).find('td:nth-child(2)').text().toLowerCase();

        $(this).toggle(product.includes(val));
    });

});

$('#btnClearProduct').click(function () {

    $('#ddlProductSearch').val('');

    $('#tbleStockProductManagement tr').show();

});