var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
var orderBy = 2;
var orderDir = "asc";
var filter = "";
var allCustomers = [];
var selectedCustomer = "";
var currentFromDate = "";
var currentToDate = "";

$(document).ready(function () {
    // Load customer data on page load
    LoadCustomerData();

    $('#ddlRecPerPage').change(function () {
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
    });

    // Show full customer list on focus
    $('#TxtSearch').on('focus', function () {
        var dropdown = $('#customerDropdown');
        if (allCustomers.length > 0 && $('#TxtSearch').val() === '') {
            dropdown.empty();
            allCustomers.forEach(function (customer) {
                dropdown.append('<div class="dropdown-item" style="padding: 8px 12px; cursor: pointer; border-bottom: 1px solid #eee; background: #f9f9f9;">' + customer + '</div>');
            });
            dropdown.show();
        }
    });

    // Autocomplete for customer search
    $('#TxtSearch').on('input', function () {
        var searchValue = $(this).val().toLowerCase();
        var dropdown = $('#customerDropdown');

        if (searchValue.length > 0) {
            var filtered = allCustomers.filter(function (customer) {
                return customer.toLowerCase().includes(searchValue);
            });

            if (filtered.length > 0) {
                dropdown.empty();
                filtered.forEach(function (customer) {
                    dropdown.append('<div class="dropdown-item" style="padding: 10px 12px; cursor: pointer; border-bottom: 1px solid #ddd; background: #f9f9f9;">' + customer + '</div>');
                });
                dropdown.show();
            } else {
                dropdown.empty();
                dropdown.hide();
            }
        } else {
            // Show full list again when cleared
            if (allCustomers.length > 0) {
                dropdown.empty();
                allCustomers.forEach(function (customer) {
                    dropdown.append('<div class="dropdown-item" style="padding: 10px 12px; cursor: pointer; border-bottom: 1px solid #ddd; background: #f9f9f9;">' + customer + '</div>');
                });
                dropdown.show();
            } else {
                dropdown.empty();
                dropdown.hide();
            }
            selectedCustomer = "";
        }
    });

    // Handle dropdown item selection
    $(document).on('click', '#customerDropdown .dropdown-item', function () {
        selectedCustomer = $(this).text();
        $('#TxtSearch').val(selectedCustomer);
        $('#customerDropdown').hide();
    });

    // Hide dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#TxtSearch, #customerDropdown').length) {
            $('#customerDropdown').hide();
        }
    });

    ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
});

// Search button handler
$('#SearchBtn').on('click', function () {
    var fromdate = $("#FromDate").val();
    var todate = $("#ToDate").val();
    var search = $("#TxtSearch").val().trim();

    if (fromdate != "" && todate != "") {
        currentFromDate = fromdate;
        currentToDate = todate;
        selectedCustomer = search;
        $("#Fdatevalidate").text("");
        $("#Tdatevalidate").text("");
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
    } else if (fromdate != "" || todate != "") {
        if (fromdate != "" && todate == "") {
            $("#Tdatevalidate").text('please select to date');
        } else if (fromdate == "" && todate != "") {
            $("#Fdatevalidate").text('Please select first date');
        }
    } else if (search != "") {
        currentFromDate = "";
        currentToDate = "";
        selectedCustomer = search;
        $("#Fdatevalidate").text('');
        $("#Tdatevalidate").text('');
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, "", "", search);
    } else {
        $("#Fdatevalidate").text('Please select first date');
        $("#Tdatevalidate").text('please select to date');
    }
});

// Clear/Refresh button handler
$('#btnClearFilter').on('click', function () {
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#TxtSearch").val('');
    $("#Fdatevalidate").text('');
    $("#Tdatevalidate").text('');
    selectedCustomer = "";
    currentFromDate = "";
    currentToDate = "";
    ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, "", "", "");
});

function LoadCustomerData() {
    // Try to load from API, but fallback to empty array if fails
    $.ajax({
        url: '/SPReports/GetCustomerReport?prefix=&salesPerson=' + $('#hdnLoggedInUserSPCode').val(),
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            if (data && data.length > 0) {
                allCustomers = data.map(function (item) {
                    return item.Customer_Name || item.CustomerName || '';
                }).filter(function (name) {
                    return name.length > 0;
                });
                allCustomers = [...new Set(allCustomers)]; // Remove duplicates
            }
        },
        error: function () {
            // If API fails, customer list will be populated from report data
            console.log('LoadCustomerData API failed, will load from report data');
        }
    });
}

function ExtractCustomersFromReport(data) {
    // Extract unique customer names from report data
    if (data && data.length > 0) {
        var customers = data.map(function (item) {
            return item.Contact_Company_Name || item.Customer_Name || item.CustomerName || '';
        }).filter(function (name) {
            return name.length > 0;
        });

        // Merge with existing customers and remove duplicates
        allCustomers = [...new Set([...allCustomers, ...customers])];
    }
}

function ComplainList(skip, top, firsload, orderBy, orderDir, fromdate, todate, customerName) {
    debugger
    if (typeof showPageDataLoader === 'function') {
        showPageDataLoader();
    }

    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=DailyVisitsDotNetAPI&fdate=' + (fromdate || 'null') + '&tdate=' + (todate || 'null') + '&text=' + (customerName || 'null'), function (data) {
        $('#hdnSPSICount').val(data);
    });
    $.ajax({
        url: '/SPReports/GetComplaintList?orderBy=' + orderBy + '&orderDir=' + orderDir + '&skip=' + skip + '&top=' + top + '&fromdate=' + (fromdate || '') + '&todate=' + (todate || '') + '&search=' + (customerName || ''),
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            if ($.fn.dataTable.isDataTable('#dataList')) {
                $('#dataList').DataTable().destroy();
            }
            $("#tblComplaint").empty();

            // Extract customers from report data
            ExtractCustomersFromReport(data);

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {
                    // Filter by customer name if selected
                    if (customerName && customerName !== "" && item.Contact_Company_Name && !item.Contact_Company_Name.toLowerCase().includes(customerName.toLowerCase())) {
                        return; // Skip this record
                    }

                    rowData += "<tr><td>" + item.Complain_Invoice + "</td><td>" + item.No + "</td><td>" + item.Contact_Company_Name + "</td><td>" + item.Complain_Subject + "</td><td>" +
                        item.Com_Date + "</td><td>" + item.Root_Analysis + "</td><td>" + item.Root_Analysis_date + "</td><td>"
                        + item.Corrective_Action + "</td><td>" + item.Corrective_Action_Date + "</td><td>" + item.Preventive_Action + "</td><td>" + item.Preventive_Date + "</td><td>" + item.Status + "</td></tr>";


                });

            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblComplaint").append(rowData);
            if (firsload == 1) {
                pageMe();
            }
            //dataTableFunction(orderBy, orderDir);

            if (data.length == 0) {
                $('ul.pager li').remove();
            }

        },
        complete: function () {
            if (typeof hidePageDataLoader === 'function') {
                hidePageDataLoader();
            }
        }
    });
}
$('#dataList th').click(function () {
    var table = $(this).parents('table').eq(0)

    this.asc = !this.asc;
    if (this.cellIndex != 0) {
        orderBy = parseInt(this.cellIndex);
        orderDir = "asc";

        if (this.asc) {
            orderDir = "asc";
        }
        else {
            orderDir = "desc";
        }
        $('ul.pager li').remove();
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
    }
});

function pageMe() {

    if (filter != "" || filter != null)
        $('ul.pager li').remove();

    var opts = {
        pagerSelector: '#myPager',
        showPrevNext: true,
        hidePageNumbers: false,
        perPage: $('#ddlRecPerPage').val()
    };
    var $this = $('#tblComplaint'),
        defaults = {
            perPage: 11,
            showPrevNext: false,
            hidePageNumbers: false
        },
        settings = $.extend(defaults, opts);

    var listElement = $this;
    var perPage = settings.perPage;
    var children = listElement.children();
    var pager = $('.pager');

    if (typeof settings.childSelector != "undefined") {
        children = listElement.find(settings.childSelector);
    }

    if (typeof settings.pagerSelector != "undefined") {
        pager = $(settings.pagerSelector);
    }

    var numItems = $('#hdnSPSICount').val(); //32;// children.length;
    var numPages = Math.ceil(numItems / perPage);

    pager.data("curr", 0);

    if (settings.showPrevNext) {
        $('<li><a href="#" class="prev_link">«</a></li>').appendTo(pager);
    }

    var curr = 0;
    var skip = 0, top = $('#ddlRecPerPage').val();
    // Added class and id in li start
    while (numPages > curr && (settings.hidePageNumbers == false)) {
        $('<li id="pg' + (curr + 1) + '" class="pg"><a href="#" skip=' + skip + ' top=' + top + ' class="page_link">' + (curr + 1) + '</a></li>').appendTo(pager);
        skip = skip + parseInt($('#ddlRecPerPage').val());
        curr++;
    }
    // Added class and id in li end

    if (settings.showPrevNext) {
        $('<li><a href="#" class="next_link">»</a></li>').appendTo(pager);
    }

    pager.find('.page_link:first').addClass('active');
    pager.find('.prev_link').hide();
    if (numPages <= 1) {
        pager.find('.next_link').hide();
    }
    pager.children().eq(1).addClass("active");

    children.hide();
    children.slice(0, perPage).show();
    if (numPages > 3) {
        $('.pg').hide();
        $('#pg1,#pg2,#pg3').show();
        $("#pg3").after($("<li class='ell'>").html("<span>...</span>"));
    }

    pager.find('li .page_link').click(function () {
        var clickedPage = $(this).html().valueOf() - 1;
        var skip1 = $(this).attr("skip");
        var top1 = $(this).attr("top");
        goTo(clickedPage, skip1, top1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
        return false;
    });
    pager.find('li .prev_link').click(function () {
        previous();
        return false;
    });
    pager.find('li .next_link').click(function () {
        next();
        return false;
    });

    function previous() {
        var goToPage = parseInt(pager.data("curr")) - 1;
        var skip1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("skip");
        var top1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("top");
        goTo(goToPage, skip1, top1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
    }

    function next() {
        goToPage = parseInt(pager.data("curr")) + 1;
        var skip1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("skip");
        var top1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("top");
        goTo(goToPage, skip1, top1, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
    }

    function goTo(page, skip2, top2) {
        var startAt = page * perPage,
            endOn = startAt + perPage;

        // Added few lines from here start

        $('.pg').hide();
        $(".ell").remove();
        var prevpg = $("#pg" + page).show();
        var currpg = $("#pg" + (page + 1)).show();
        var currpg1 = $("#pg" + (page + 1)).find("a");
        var nextpg = $("#pg" + (page + 2)).show();
        if (prevpg.length == 0) nextpg = $("#pg" + (page + 3)).show();
        if (prevpg.length == 1 && nextpg.length == 0) {
            prevpg = $("#pg" + (page - 1)).show();
        }
        $("#pg1").show()
        if (curr > 3) {
            if (page > 1) prevpg.before($("<li class='ell'>").html("<span>...</span>"));
            if (page < curr + 2) nextpg.after($("<li class='ell'>").html("<span>...</span>"));
        }

        if (page <= numPages - 3) {
            $("#pg" + numPages.toString()).show();
        }

        $('.page_link').removeClass("active");

        currpg1.addClass("active");
        // Added few lines till here end

        children.css('display', 'none').slice(startAt, endOn).show();

        if (page >= 1) {
            pager.find('.prev_link').show();
        } else {
            pager.find('.prev_link').hide();
        }

        if (page < (numPages - 1)) {
            pager.find('.next_link').show();
        } else {
            pager.find('.next_link').hide();
        }

        pager.data("curr", page);

        ComplainList(skip2, top2, 0, orderBy, orderDir, currentFromDate, currentToDate, selectedCustomer);
    }

};