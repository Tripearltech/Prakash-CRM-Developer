var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    BindOutstandingDetails();
});

const ProductGroupsCode = {};
function BindOutstandingDetails() {
    $.ajax({
        url: '/SPReports/GetCustomerOutStanding',
        type: 'GET',
        contentType: 'application/json',
        success: function (branches) {
            const $tbl = $('#tblOutstanding');
            $tbl.empty();

            if (!branches || branches.length === 0) {
                $tbl.append("<tr><td colspan='7' style='text-align:center;'>No Records Found</td></tr>");
                return;
            }

            branches.forEach((b, bi) => {
                const branchId = `InvBranch${bi}`;
                const branchRow = $(`
                    <tr data-branchindex="${bi}" class="branch-row" style='text-align:center;'>
                        <td><a href="#" class="clsPointer branch-toggle" data-target="${branchId}SPRecs"><i class="bx bx-plus-circle"></i></a></td>
                        <td>${b.Location_Code}</td><td>${b.CollectionuptoMTD}</td><td>${b.CollRecdforthePeriod}</td><td>${b.TotalCollectionRecdtilltoday}</td><td>${b.Overdueuptopreviousmonthdue}</td><td>${b._x0031_st10thdueofcurrentmonth}</td><td>${b._x0031_1th20thdueofcurrentmonth}</td><td>${b._x0032_1st30_31stdueofcurrentmonth}</td><td>${b.Total_Due_in_Month}</td><td>${b.AchivementinPercent}</td>
                    </tr>
                    <tr id="${branchId}SPRecs" style="display:none;" data-type="product-groups"><td colspan="7"></td></tr>
                `);
                $tbl.append(branchRow);
            });

            $('#divImage').hide();

            $tbl.on('click', '.branch-toggle', function (e) {
                e.preventDefault();
                const $a = $(this);
                const $icon = $a.find('i');
                const $branchRow = $a.closest('tr');
                const bi = $branchRow.data('branchindex');
                const locCode = branches[bi].Location_Code;
                const targetId = $a.data('target');
                const $subRow = $(`#${targetId}`);
                const $nextRows = $branchRow.nextUntil('tr.branch-row');

                if ($subRow.is(':visible')) {
                    $nextRows.hide();
                    $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
                } else {
                    $subRow.show();
                    $subRow.empty();
                    $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');

                    const $productRows = $subRow.nextUntil('tr.branch-row');
                    if ($productRows.length === 0) {
                        SalespersonData($branchRow, $subRow, locCode, bi);
                    } else {
                        $productRows.show();
                    }
                }
            });

            function SalespersonData($branchRow, $subRow, locCode, bi) {
                $.ajax({
                    url: `/SPReports/GetSalespersonData?branchCode=${locCode}`,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (sps) {
                        ProductGroupsCode[bi] = sps;

                        if (!sps || sps.length === 0) {
                            const noGroupRow = `<tr data-type="product-group-empty"><td colspan="11" style="padding-right:40px;">No Salesperson Data</td></tr>`;
                            $branchRow.after(noGroupRow);
                            return;
                        }

                        let rowsHtml = '';
                        sps.forEach((sp, spi) => {
                            const spId = `InvBranch${bi}_ProGroup${spi}SPRecs`;
                            rowsHtml += `
                            <tr class="product-group-row" data-spi="${spi}" style="text-align:center;">
                            <td>
                            <a href="#" class="clsPointer sp-toggle"
                                data-target="${spId}" data-branch="${bi}" data-sp="${spi}"><i class=""></i></a></td><td onclick="SalesPersonClick('${sp.Salesperson_Code}')" 
                              style="cursor:pointer; color:blue;">
                              ${sp.Salesperson_Code}</td><td>${sp.CollectionuptoMTD}</td><td>${sp.CollRecdforthePeriod}</td><td>${sp.TotalCollectionRecdtilltoday}</td><td>${sp.Overdueuptopreviousmonthdue}</td><td>${sp._x0031_st10thdueofcurrentmonth}</td><td>${sp._x0031_1th20thdueofcurrentmonth}</td><td>${sp._x0032_1st30_31stdueofcurrentmonth}</td><td>${sp.Total_Due_in_Month}</td><td>${sp.AchivementinPercent}</td></tr>
                            <tr id="${spId}" class="item-row" style="display:none;"><td colspan="11"></td>
                            </tr>`;
                        });
                        $branchRow.after(rowsHtml);
                    },
                    error: function () {
                        $subRow.html(`<td colspan="7" style="color:red;">Error loading Salesperson Data</td>`);
                    }
                });
            }
        },
        error: function (err) {
            alert("Error fetching branch data: " + err.responseText);
        }
    });
}

function SalesPersonClick(code) {
    $("#itemDetailsModal").modal("show");
    $("#itemDetailsModalLabel").text("Customer Details");
    $("#itemDetailsModal thead").html(`<tr><th></th><th scope="col">Customer Name</th><th scope="col">Class</th><th scope="col">Amount (Rs)</th><th scope="col">ACD</th><th scope="col">ADD</th></tr>`);
    $("#tblCustomerData").empty();
    $.ajax({
        url: '/SPReports/GetCustomerDataBySalesperson?spCode=' + code,
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {

            if (!data || data.length === 0) {
                $("#tblCustomerData").html("<tr><td colspan='7'>No Customer Data Found</td></tr>");
                return;
            }

            let rows = "";
            $.each(data, function (i, item) {
                rows += `
                    <tr>
                        <td></td>
                        <td onclick="OpenCustomerDetail('${item.Customer_Name}')" style="cursor:pointer; color:blue;">${item.Customer_Name}</td><td>${item.Class}</td><td>${item.Remaining_Amt}</td><td>${item.ACD_Amt}</td><td>${item.ADD_Amt}</td>
                    </tr>`;
            });

            $("#tblCustomerData").html(rows);
        },
        error: function () {
            $("#tblCustomerData").html("<tr><td colspan='7'>Error loading data</td></tr>");
        }
    });
}

function OpenCustomerDetail(customerCode) {
    $("#customerDetailModal").modal("show");

    $("#customerDetailthead").html(`
        <tr><th></th><th scope="col">Customer Name</th><th scope="col">Class</th><th scope="col">Amount (Rs)</th><th scope="col">ACD</th><th scope="col">ADD</th></tr>
    `);

    $("#customerDetailthead").text("Customer Invoice Details")
    $.ajax({
        url: '/SPReports/GetCustomerwiseInvoice?customerCode=' + customerCode,
        type: 'GET',
        success: function (list) {

            $("#customerDetailBody").empty();

            if (!list || list.length === 0) {
                $("#customerDetailBody").html("<tr><td colspan='12'>No Record Found</td></tr>");
                return;
            }

            let html = "";
            list.forEach(item => {
                html += `
                    <tr><td>${item.Document_Type}</td><td>${item.PO_Number}</td><td>${item.Bill_No}</td><td>${item.Bill_Date}</td><td>${item.Product_Dimension}</td><td>${item.TERMS}</td><td>${item.Due_Date}</td><td>${item.Invoice_Amt}</td><td>${item.Remaining_Amt}</td><td>${item.Total_Days}</td><td>${item.Overdue_Days}</td>
                    </tr>`;
            });

            $("#customerDetailBody").html(html);
        },
        error: function () {
            $("#customerDetailBody").html("<tr><td colspan='12'>Error fetching data</td></tr>");
        }
    });
}