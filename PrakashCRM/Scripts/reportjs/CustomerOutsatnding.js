var customerOutstandingState = {
    pageNumber: 1,
    pageSize: parseInt($('#ddlRecPerPage').val(), 10) || 10,
    totalCount: 0,
    totalPages: 0,
    branches: [],
    salespersonCache: {}
};

$(document).ready(function () {
    bindCustomerOutstandingEvents();
    BindOutstandingDetails(1);
});

function bindCustomerOutstandingEvents() {
    $('#ddlRecPerPage').off('change.customerOutstanding').on('change.customerOutstanding', function () {
        customerOutstandingState.pageSize = parseInt($(this).val(), 10) || 10;
        customerOutstandingState.pageNumber = 1;
        customerOutstandingState.salespersonCache = {};
        BindOutstandingDetails(1);
    });

    $('#tblOutstanding').off('click.customerOutstanding', '.branch-toggle').on('click.customerOutstanding', '.branch-toggle', function (e) {
        e.preventDefault();

        var branchIndex = parseInt($(this).data('branchindex'), 10);
        var $icon = $(this).find('i');
        var $detailRow = $('#branch-detail-' + branchIndex);
        var branch = customerOutstandingState.branches[branchIndex];

        if (!branch) {
            return;
        }

        if ($detailRow.is(':visible')) {
            $detailRow.hide();
            $icon.removeClass('bx-minus-circle').addClass('bx-plus-circle');
            return;
        }

        $('.branch-detail-row').hide();
        $('.branch-toggle i').removeClass('bx-minus-circle').addClass('bx-plus-circle');

        $detailRow.show();
        $icon.removeClass('bx-plus-circle').addClass('bx-minus-circle');

        if (customerOutstandingState.salespersonCache[branch.Location_Code]) {
            renderSalespersonDetails(branchIndex, customerOutstandingState.salespersonCache[branch.Location_Code]);
            return;
        }

        loadSalespersonDetails(branchIndex, branch.Location_Code);
    });

    $('#myPager').off('click.customerOutstanding', '.page-link').on('click.customerOutstanding', '.page-link', function (e) {
        e.preventDefault();

        var targetPage = parseInt($(this).data('page'), 10);
        if (!targetPage || $(this).parent().hasClass('active') || $(this).parent().hasClass('disabled')) {
            return;
        }

        BindOutstandingDetails(targetPage);
    });
}

function BindOutstandingDetails(pageNumber) {
    customerOutstandingState.pageNumber = pageNumber || 1;
    customerOutstandingState.pageSize = parseInt($('#ddlRecPerPage').val(), 10) || 10;
    toggleCustomerOutstandingLoader(true);

    $.ajax({
        url: '/SPReports/GetCustomerOutStanding',
        type: 'GET',
        contentType: 'application/json',
        data: {
            pageNumber: customerOutstandingState.pageNumber,
            pageSize: customerOutstandingState.pageSize,
            orderby: 'Location_Code asc'
        },
        success: function (response) {
            var items = response && response.Items ? response.Items : [];

            customerOutstandingState.branches = items;
            customerOutstandingState.totalCount = response && response.TotalCount ? response.TotalCount : 0;
            customerOutstandingState.totalPages = response && response.TotalPages ? response.TotalPages : 0;

            renderOutstandingTable(items);
            renderOutstandingPager();
            toggleCustomerOutstandingLoader(false);
        },
        error: function () {
            $('#tblOutstanding').html("<tr><td colspan='11' style='text-align:center;'>Error loading records</td></tr>");
            $('#myPager').empty();
            toggleCustomerOutstandingLoader(false);
        }
    });
}

function toggleCustomerOutstandingLoader(showLoader) {
    $('#customerOutstandingLoader').toggleClass('d-none', !showLoader);
}

function renderOutstandingTable(branches) {
    var $tableBody = $('#tblOutstanding');
    $tableBody.empty();

    if (!branches || branches.length === 0) {
        $tableBody.append("<tr><td colspan='11' style='text-align:center;'>No Records Found</td></tr>");
        return;
    }

    $.each(branches, function (index, branch) {
        $tableBody.append(`
            <tr class="branch-row text-center">
                <td><a href="#" class="clsPointer branch-toggle" data-branchindex="${index}"><i class="bx bx-plus-circle"></i></a></td>
                <td>${branch.Location_Code || ''}</td>
                <td>${branch.CollectionuptoMTD || 0}</td>
                <td>${branch.CollRecdforthePeriod || 0}</td>
                <td>${branch.TotalCollectionRecdtilltoday || 0}</td>
                <td>${branch.Overdueuptopreviousmonthdue || 0}</td>
                <td>${branch._x0031_st10thdueofcurrentmonth || 0}</td>
                <td>${branch._x0031_1th20thdueofcurrentmonth || 0}</td>
                <td>${branch._x0032_1st30_31stdueofcurrentmonth || 0}</td>
                <td>${branch.Total_Due_in_Month || 0}</td>
                <td>${branch.AchivementinPercent || 0}</td>
            </tr>
            <tr id="branch-detail-${index}" class="branch-detail-row" style="display:none;">
                <td colspan="11" class="p-0"></td>
            </tr>
        `);
    });
}

function loadSalespersonDetails(branchIndex, branchCode) {
    var $detailCell = $('#branch-detail-' + branchIndex + ' td');
    $detailCell.html("<div class='p-3 text-center'>Loading...</div>");

    $.ajax({
        url: '/SPReports/GetSalespersonData',
        type: 'GET',
        contentType: 'application/json',
        data: { branchCode: branchCode },
        success: function (salespersons) {
            customerOutstandingState.salespersonCache[branchCode] = salespersons || [];
            renderSalespersonDetails(branchIndex, salespersons || []);
        },
        error: function () {
            $detailCell.html("<div class='p-3 text-danger'>Error loading Salesperson Data</div>");
        }
    });
}

function renderSalespersonDetails(branchIndex, salespersons) {
    var $detailCell = $('#branch-detail-' + branchIndex + ' td');

    if (!salespersons || salespersons.length === 0) {
        $detailCell.html("<div class='p-3 text-center'>No Salesperson Data</div>");
        return;
    }

    var rows = '';
    $.each(salespersons, function (_, item) {
        rows += `
            <tr class="text-center">
                <td onclick="SalesPersonClick('${escapeJsValue(item.Salesperson_Code)}')" style="cursor:pointer; color:blue;">${item.Salesperson_Code || ''}</td>
                <td>${item.CollectionuptoMTD || 0}</td>
                <td>${item.CollRecdforthePeriod || 0}</td>
                <td>${item.TotalCollectionRecdtilltoday || 0}</td>
                <td>${item.Overdueuptopreviousmonthdue || 0}</td>
                <td>${item._x0031_st10thdueofcurrentmonth || 0}</td>
                <td>${item._x0031_1th20thdueofcurrentmonth || 0}</td>
                <td>${item._x0032_1st30_31stdueofcurrentmonth || 0}</td>
                <td>${item.Total_Due_in_Month || 0}</td>
                <td>${item.AchivementinPercent || 0}</td>
            </tr>`;
    });

    $detailCell.html(`
        <div class="table-responsive p-2 bg-light">
            <table class="table table-striped mb-0">
                <thead>
                    <tr class="text-center">
                        <th scope="col">Salesperson Code</th>
                        <th scope="col">Col. Upto MTD-1 Date</th>
                        <th scope="col">Date on Title - TD</th>
                        <th scope="col">Total Collection</th>
                        <th scope="col">Overdue CM-1</th>
                        <th scope="col">1st-10th Due Cur. Month</th>
                        <th scope="col">11th-20th Due Cur. Month</th>
                        <th scope="col">21st-30th Due Cur. Month</th>
                        <th scope="col">Total Due Cur. Month</th>
                        <th scope="col">Achievement in %</th>
                    </tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
        </div>
    `);
}

function renderOutstandingPager() {
    var $pager = $('#myPager');
    var totalPages = customerOutstandingState.totalPages || Math.ceil(customerOutstandingState.totalCount / customerOutstandingState.pageSize);
    var currentPage = customerOutstandingState.pageNumber;

    $pager.empty();

    if (!totalPages || totalPages <= 1) {
        return;
    }

    $pager.append(buildPagerItem('«', currentPage - 1, currentPage === 1, false));

    $.each(getVisiblePages(currentPage, totalPages), function (_, pageToken) {
        if (pageToken === 'ellipsis') {
            $pager.append("<li class='page-item disabled'><span class='page-link'>...</span></li>");
            return;
        }

        $pager.append(buildPagerItem(pageToken, pageToken, false, pageToken === currentPage));
    });

    $pager.append(buildPagerItem('»', currentPage + 1, currentPage === totalPages, false));
}

function buildPagerItem(label, pageNumber, isDisabled, isActive) {
    var disabledClass = isDisabled ? ' disabled' : '';
    var activeClass = isActive ? ' active' : '';
    return `<li class="page-item${disabledClass}${activeClass}"><a href="#" class="page-link" data-page="${pageNumber}">${label}</a></li>`;
}

function getVisiblePages(currentPage, totalPages) {
    var pages = [];
    var startPage;
    var endPage;
    var page;

    if (totalPages <= 5) {
        for (page = 1; page <= totalPages; page++) {
            pages.push(page);
        }

        return pages;
    }

    pages.push(1);

    if (currentPage <= 3) {
        startPage = 2;
        endPage = 4;
    } else if (currentPage >= totalPages - 2) {
        startPage = totalPages - 3;
        endPage = totalPages - 1;
    } else {
        startPage = currentPage - 1;
        endPage = currentPage + 1;
    }

    if (startPage > 2) {
        pages.push('ellipsis');
    }

    for (page = startPage; page <= endPage; page++) {
        if (page > 1 && page < totalPages) {
            pages.push(page);
        }
    }

    if (endPage < totalPages - 1) {
        pages.push('ellipsis');
    }

    pages.push(totalPages);
    return pages;
}

function escapeJsValue(value) {
    if (!value) {
        return '';
    }

    return String(value).replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}

function SalesPersonClick(code) {
    $('#itemDetailsModal').modal('show');
    $('#itemDetailsModalLabel').text('Customer Details');
    $('#itemDetailsModal thead').html('<tr><th></th><th scope="col">Customer Name</th><th scope="col">Class</th><th scope="col">Amount (Rs)</th><th scope="col">ACD</th><th scope="col">ADD</th></tr>');
    $('#tblCustomerData').empty();

    $.ajax({
        url: '/SPReports/GetCustomerDataBySalesperson',
        type: 'GET',
        contentType: 'application/json',
        data: { spCode: code },
        success: function (data) {
            if (!data || data.length === 0) {
                $('#tblCustomerData').html("<tr><td colspan='6'>No Customer Data Found</td></tr>");
                return;
            }

            var rows = '';
            $.each(data, function (_, item) {
                rows += `
                    <tr>
                        <td></td>
                        <td onclick="OpenCustomerDetail('${escapeJsValue(item.Customer_Name)}')" style="cursor:pointer; color:blue;">${item.Customer_Name || ''}</td>
                        <td>${item.Class || ''}</td>
                        <td>${item.Remaining_Amt || 0}</td>
                        <td>${item.ACD_Amt || 0}</td>
                        <td>${item.ADD_Amt || 0}</td>
                    </tr>`;
            });

            $('#tblCustomerData').html(rows);
        },
        error: function () {
            $('#tblCustomerData').html("<tr><td colspan='6'>Error loading data</td></tr>");
        }
    });
}

function OpenCustomerDetail(customerCode) {
    $('#customerDetailModal').modal('show');
    $('#customerDetailModalLabel').text('Customer Invoice Details');
    $('#customerDetailThead').html('<tr><th scope="col">Document Type</th><th scope="col">PO Number</th><th scope="col">Bill No</th><th scope="col">Bill Date</th><th scope="col">Product Dimension</th><th scope="col">Terms</th><th scope="col">Due Date</th><th scope="col">Invoice Amt</th><th scope="col">Remaining Amt</th><th scope="col">Total Days</th><th scope="col">Overdue Days</th></tr>');

    $.ajax({
        url: '/SPReports/GetCustomerwiseInvoice',
        type: 'GET',
        data: { customerCode: customerCode },
        success: function (list) {
            $('#customerDetailBody').empty();

            if (!list || list.length === 0) {
                $('#customerDetailBody').html("<tr><td colspan='11'>No Record Found</td></tr>");
                return;
            }

            var html = '';
            $.each(list, function (_, item) {
                html += `
                    <tr>
                        <td>${item.Document_Type || ''}</td>
                        <td>${item.PO_Number || ''}</td>
                        <td>${item.Bill_No || ''}</td>
                        <td>${item.Bill_Date || ''}</td>
                        <td>${item.Product_Dimension || ''}</td>
                        <td>${item.TERMS || ''}</td>
                        <td>${item.Due_Date || ''}</td>
                        <td>${item.Invoice_Amt || 0}</td>
                        <td>${item.Remaining_Amt || 0}</td>
                        <td>${item.Total_Days || 0}</td>
                        <td>${item.Overdue_Days || 0}</td>
                    </tr>`;
            });

            $('#customerDetailBody').html(html);
        },
        error: function () {
            $('#customerDetailBody').html("<tr><td colspan='11'>Error fetching data</td></tr>");
        }
    });
}