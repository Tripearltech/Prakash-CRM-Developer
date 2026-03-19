var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
$(document).ready(function () {
    $('#btnGenerate').on('click', function () {
        GenerateInvData();
    });
});

function GenerateInvData() {
    $('#divImage').show();
    var fromDate = $('#txtInvFDate').val();
    var toDate = $('#txtInvTDate').val();

    if (fromDate !== "" && toDate !== "") {
        $.post(apiUrl + 'GenerateInvData?FromDate=' + fromDate + '&ToDate=' + toDate,
            function (data) {
                if (data) {
                    BindInvBranchWiseTotals();
                }
            }
        );
    }
}

const ProductGroupsCode =  {};
function BindInvBranchWiseTotals() {
    $.ajax({
        url: '/SPReports/GetBranchWiseTotal',
        type: 'GET',
        contentType: 'application/json',
        success: function (branches) {
            const $tbl = $('#tblInventroyDetails');
            $tbl.empty();

            if (!branches || branches.length === 0) {
                $tbl.append("<tr><td colspan='7' style='text-align:center;'>No Records Found</td></tr>");
                return;
            }

            branches.forEach((b, bi) => {
                const branchId = `InvBranch${bi}`;
                const branchRow = $(`
                    <tr data-branchindex="${bi}" class="branch-row">
                        <td><a href="#" class="clsPointer branch-toggle" data-target="${branchId}SPRecs"><i class="bx bx-plus-circle"></i></a></td>
                        <td>${b.LocationCode}</td><td>${b.Opening_LocationCode}</td><td>${b.Inward_LocationCode}</td><td>${b.Outward_LocationCode}</td><td>${b.Reserved_LocationCode}</td><td>${b.CLStock_LocationCode}</td>
                    </tr>
                    <tr id="${branchId}SPRecs" style="display:none;" data-type="product-groups"><td colspan="7"></td></tr>
                `);
                $tbl.append(branchRow);
            });

            $('#divImage').hide();
            // Branch Toggle
            $tbl.on('click', '.branch-toggle', function (e) {
                e.preventDefault();
                const $a = $(this);
                const $icon = $a.find('i');
                const $branchRow = $a.closest('tr');
                const bi = $branchRow.data('branchindex');
                const locCode = branches[bi].LocationCode;
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
                        loadProductGroups($branchRow, $subRow, locCode, bi);
                    } else {
                        $productRows.show();
                    }
                }
            });

            function loadProductGroups($branchRow, $subRow, locCode, bi) {
                $.ajax({
                    url: `/SPReports/GetInv_ProductGroupsWise?branchCode=${locCode}`,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (pgs) {
                        ProductGroupsCode[bi] = pgs;

                        if (!pgs || pgs.length === 0) {
                            const noGroupRow = `<tr data-type="product-group-empty"><td colspan="7" style="padding-left:20px;">No Product Groups</td></tr>`;
                            $branchRow.after(noGroupRow);
                            return;
                        }

                        let rowsHtml = '';
                        pgs.forEach((pg, pgi) => {
                            const pgId = `InvBranch${bi}_ProGroup${pgi}SPRecs`;
                            rowsHtml += `
                                <tr class="product-group-row" data-pgi="${pgi}">
                                    <td style="padding-left:20px;">
                                        <a href="#" class="clsPointer pg-toggle" data-target="${pgId}" data-branch="${bi}" data-pg="${pgi}">
                                            <i class="bx bx-plus-circle"></i>
                                        </a>
                                    </td>
                                    <td>${pg.Code}</td><td>${pg.Opening}</td><td>${pg.Inward}</td><td>${pg.Outward}</td><td>${pg.Reserved}</td><td>${pg.CLStock}</td>
                                </tr>
                                <tr id="${pgId}" class="item-row" style="display:none;"><td colspan="7"></td></tr>
                            `;
                        });
                        $branchRow.after(rowsHtml);
                    },
                    error: function () {
                        $subRow.html(`<td colspan="7" style="color:red;">Error loading product groups</td>`);
                    }
                });
            }

            // Product Group toggle handler
            $tbl.on('click', '.pg-toggle', function (e) {
                e.preventDefault();
                const $a = $(this);
                const $icon = $a.find('i');
                const pgId = $a.data('target');
                const branchIndex = $a.data('branch');
                const pgIndex = $a.data('pg');

                const $row = $(`#${pgId}`);
                const $itemRows = $row.nextUntil('tr.product-group-row, tr.branch-row');

                if ($row.is(':visible')) {
                    $itemRows.hide();
                    $row.hide();
                    $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
                } else {
                    $row.show();
                    $row.empty();
                    $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');

                    if ($itemRows.length === 0 || $itemRows.length === 1) {
                        loadItems(branchIndex, pgIndex, $row);
                    } else {
                        $itemRows.show();
                    }
                }
            });


            // Quantity click handler for pop-up
            $tbl.on('click', '.quantity-link', function (e) {
                e.preventDefault();
                const branchIndex = $(this).data('branch');
                const pgIndex = $(this).data('pg');
                const itemName = $(this).data('item');
                const quantityType = $(this).data('type');
                loadItemDetails(branchIndex, pgIndex, itemName, quantityType);
            });

            function loadItems(branchIndex, pgIndex, $itemRow) {
                const branchCode = branches[branchIndex].LocationCode;
                const pgCode = ProductGroupsCode[branchIndex][pgIndex].Code;

                $.ajax({
                    url: `/SPReports/GetInv_ItemWise?branchCode=${branchCode}&pgCode=${pgCode}`,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (items) {
                        if (!items || items.length === 0) {
                            const noItemsRow = `<tr class="item-row-empty"><td colspan="7" style="padding-left:40px;">No Item Records Found</td></tr>`;
                            $(`#InvBranch${branchIndex}_ProGroup${pgIndex}SPRecs`).after(noItemsRow);
                            return;
                        }
                        let itemRows = '';
                        items.forEach((it) => {
                            itemRows += `
                            <tr class="item-row-detail">
                                <td style='padding-left:40px;'>-</td>
                                <td>${it.ItemName}</td>
                                <td>${it.Opening_Item}</td>
                                <td><a href="#" class="quantity-link" data-branch="${branchIndex}" data-pg="${pgIndex}" data-item="${it.ItemName}" data-type="Inward">${it.Inward_Item}</a></td>
                                <td><a href="#" class="quantity-link" data-branch="${branchIndex}" data-pg="${pgIndex}" data-item="${it.ItemName}" data-type="Outward">${it.Outward_Item}</a></td>
                                <td><a href="#" class="quantity-link" data-branch="${branchIndex}" data-pg="${pgIndex}" data-item="${it.ItemName}" data-type="Reserved">${it.Reserved_Item}</a></td>
                                <td><a href="#" class="quantity-link" data-branch="${branchIndex}" data-pg="${pgIndex}" data-item="${it.ItemName}" data-type="CLStock">${it.CLStock_Item}</a></td>
                            </tr>`;
                        });
                        $(`#InvBranch${branchIndex}_ProGroup${pgIndex}SPRecs`).after(itemRows);
                    },
                    error: function () {
                        $itemRow.html("<td colspan='7' style='color:red;'>Error loading item-wise data</td>");
                    }
                });
            }

            function loadItemDetails(branchIndex, pgIndex, itemName, quantityType) {

                const branchCode = branches[branchIndex].LocationCode;
                const pgCode = ProductGroupsCode[branchIndex][pgIndex].Code;
                const iditemName = itemName; 

                const fromDate = $('#txtInvFDate').val(); // New
                const toDate = $('#txtInvTDate').val(); 

                let entryType = "";
                let documentType = "";

                if (quantityType === "Inward") {
                    entryType = "Purchase";
                    documentType = "Purchase Receipt";
                }
                else if (quantityType === "Outward") {
                    entryType = "Sale";
                    documentType = "Sales Shipment";
                }
                else if (quantityType === "Reserved") {
                    entryType = "Sale";
                    documentType = ""; 
                 }
                else if (quantityType === "CLStock") {
                    entryType = "Purchase";
                    documentType = "Purchase Receipt";
                }

                const url_ = `/SPReports/GetInv_Inward?` + `Entry_Type=${encodeURIComponent(entryType)}` + `&Document_Type=${encodeURIComponent(documentType)}` + `&branchCode=${encodeURIComponent(branchCode)}` + `&pgCode=${encodeURIComponent(pgCode)}` + `&itemName=${encodeURIComponent(iditemName)}` + `&FromDate=${encodeURIComponent(fromDate)}` + `&ToDate=${encodeURIComponent(toDate)}`;
               $.ajax({
                    url: url_,
                    type: 'GET',
                    contentType: 'application/json',
                    success: function (data) {
                        const $table = $('#tblInwordAttributeClick').closest('table');
                        const $thead = $table.find('thead');
                        const $tbody = $('#tblInwordAttributeClick');
                        $tbody.empty();

                        // Header creation
                        let headerHtml = '';
                        let modalTitle = '';

                        if (quantityType === "Inward") {
                            modalTitle = "Inward Details";
                            headerHtml = `<tr><th></th><th>Sr. No.</th><th>Name of the Supplier</th><th>Make</th><th>GRN QTY</th><th>Batch No.</th><th>Remarks</th></tr>`;
                        }
                        else if (quantityType === "Outward") {
                            modalTitle = "Outward Details";
                            headerHtml = `<tr><th></th><th>Sr. No.</th><th>Name of the Customer</th><th>Inv No.</th><th>Inv Date</th><th>QTY</th><th>Batch No.</th></tr>`;
                        }
                        else if (quantityType === "Reserved") {
                            modalTitle = "Reserved Details";
                            headerHtml = `<tr><th></th><th>Sr. No.</th><th>Name of the Customer</th><th>Sales Person</th><th>Sales Order Date</th><th>QTY</th><th>Make</th><th>Remarks</th></tr>`;
                        }
                        else if (quantityType === "CLStock") {
                            modalTitle = "Closing Stock Details";
                            headerHtml = `<tr><th></th><th>Sr. No.</th><th>Name of the Supplier</th><th>Make</th><th>Rec QTY</th><th>CI Stock</th><th>Reserved</th><th>Avaible</th><th>GRN No.</th><th>Batch No.</th><th>Remark</th><th>NO. Of Days</th><th>Cost Per Unit</th></tr>`;
                        }

                        $thead.html(headerHtml);
                        $('#itemDetailsModalLabel').text(modalTitle);

                        if (!data || data.length === 0) {
                            $tbody.append("<tr><td colspan='7' style='text-align:center;'>No Details Found</td></tr>");
                        } else {
                            data.forEach((d, index) => {
                                const availableQty = Math.abs((d.Remaining_Quantity || 0) - (d.Reserved_Quantity || 0));
                                let rowHtml = `<tr><td></td><td>${index + 1}</td>`;

                                if (quantityType === "Inward") {
                                    rowHtml += `<td>${d.PCPL_Vendor_Name}</td><td>${d.PCPL_Mfg_Name}</td><td>${d.Quantity}</td><td>${d.Lot_No}</td><td>${d.PCPL_Remarks}</td>`;
                                }
                                else if (quantityType === "Outward") {
                                    rowHtml += `<td>${d.Source_Description}</td><td>${d.Document_No}</td><td>${d.Posting_Date}</td><td>${d.Quantity}</td><td>${d.Lot_No}</td>`;
                                }
                                else if (quantityType === "Reserved") {
                                    rowHtml += `<td>${d.PCPL_Vendor_Name}</td><td>${d.PCPL_Salesperson_Code}</td><td>${d.Posting_Date}</td><td>${d.Reserved_Quantity}</td><td>${d.PCPL_Mfg_Name}</td><td>${d.PCPL_Remarks}</td>`;
                                }
                                else if (quantityType === "CLStock") {
                                    rowHtml += `<td>${d.Source_Description}</td><td>${d.PCPL_Mfg_Name}</td><td>${d.Document_Type}</td><td>${d.Remaining_Quantity}</td><td>${d.Reserved_Quantity}</td><td>${availableQty}</td><td>${d.Document_No}</td><td>${d.Lot_No}</td><td>${d.PCPL_Remarks}</td><td>${d.No_of_days}</td><td>${d.Cost_Amount_Actual}</td>`;
                                }

                                rowHtml += '</tr>';
                                $tbody.append(rowHtml);
                            });
                        }

                        $('#itemDetailsModal').modal('show');
                    },
                    error: function () {
                        const $tbody = $('#tblInwordAttributeClick');
                        $tbody.empty().append("<tr><td colspan='7' style='color:red;'>Error loading item details</td></tr>");
                        $('#itemDetailsModal').modal('show');
                    }
                });
            }
        },
        error: function (err) {
            alert("Error fetching branch data: " + err.responseText);
        }
    });
}