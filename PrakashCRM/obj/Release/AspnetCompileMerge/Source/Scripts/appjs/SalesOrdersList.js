/* start pagination filter code */
var filter = "";
var orderBy = 2;
var orderDir = "desc";
var allOpenFilter = "";
$(document).ready(function () {
    $('#btnAllOrders').removeClass('btn-outline-secondary').addClass('btn-outline-primary');
    $('#btnOpenOrders').removeClass('btn-outline-primary').addClass('btn-outline-secondary');
    allOpenFilter = "All";
    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);
    });
    
    $('#ddlField').change(function () {
        
        if ($('#ddlField').val() == "Document_Date") {
            $('#ddlOperator').css('display', 'none');
            $('#dvtxtSearch').css('display', 'none');
            $('#txtFromDate').css('display', 'block');
            $('#txtToDate').css('display', 'block');
        }
        else {
            $('#ddlOperator').css('display', 'block');
            $('#dvtxtSearch').css('display', 'block');
            $('#txtFromDate').css('display', 'none');
            $('#txtToDate').css('display', 'none');
        }
    });

    $('#btnSearch').click(function () {
        var flag = true;

        //
        if ($('#ddlField').val() == "Document_Date") {
            if ($('#txtFromDate').val() == "" || $('#txtToDate').val() == "") {
                flag = false;
            }
            else {
                filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
            }
        }
        else {
            if ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1" || $('#txtSearch').val() == "") {
                flag = false;
            }
            else {
                switch ($('#ddlOperator').val()) {
                    case 'Equal':
                        if ($('#ddlField').val() != "Amount" && $('#ddlField').val() != "Amount_Including_VAT") {
                            filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        }
                        else {
                            filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                        }
                        break;
                    case 'Not Equal':
                        if ($('#ddlField').val() != "Amount" && $('#ddlField').val() != "Amount_Including_VAT") {
                            filter = $('#ddlField').val() + ' ne ' + '\'' + $('#txtSearch').val() + '\'';
                        }
                        else {
                            filter = $('#ddlField').val() + ' ne ' + $('#txtSearch').val();
                        }
                        break;
                    case 'Starts With':
                        filter = "startswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                        break;
                    case 'Ends With':
                        filter = "endswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                        break;
                    case 'Contains':
                        filter = $('#ddlField').val() + ' eq ' + '\'@*' + $('#txtSearch').val() + '*\'';
                        break;
                    case 'Greater Than':
                        filter = $('#ddlField').val() + ' gt ' + $('#txtSearch').val();
                        break;
                    case 'Less Than':
                        filter = $('#ddlField').val() + ' lt ' + $('#txtSearch').val();
                        break;
                    default:
                        filter = "";
                        break;
                }
            }
        }

        if (flag == false) {

            var errMsg = "Please Fill All Filter Details";
            ShowErrMsg(errMsg);

        }
        else {
            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);
        }
        
    });

    $('#btnExport').click(function () {

        if ($('#ddlField').val() == "Document_Date") {
            filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
        }
        else {
            switch ($('#ddlOperator').val()) {
                case 'Equal':
                    if ($('#ddlField').val() != "Amount" && $('#ddlField').val() != "Amount_Including_VAT") {
                        filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                    }
                    else {
                        filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                    }
                    break;
                case 'Not Equal':
                    if ($('#ddlField').val() != "Amount" && $('#ddlField').val() != "Amount_Including_VAT") {
                        filter = $('#ddlField').val() + ' ne ' + '\'' + $('#txtSearch').val() + '\'';
                    }
                    else {
                        filter = $('#ddlField').val() + ' ne ' + $('#txtSearch').val();
                    }
                    break;
                case 'Starts With':
                    filter = "startswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                case 'Ends With':
                    filter = "endswith(" + $('#ddlField').val() + ",\'" + $('#txtSearch').val() + "\') eq true";
                    break;
                case 'Greater Than':
                    filter = $('#ddlField').val() + ' gt ' + $('#txtSearch').val();
                    break;
                case 'Less Than':
                    filter = $('#ddlField').val() + ' lt ' + $('#txtSearch').val();
                    break;
                default:
                    filter = "";
                    break;
            }
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);

    });

    $('#btnClearFilter').click(function () {

        $('#ddlField').val('-1');
        $('#ddlOperator').val('Contains');
        $('#txtSearch').val('');
        $('#txtFromDate').val('');
        $('#txtToDate').val('');
        $('#ddlOperator').css('display', 'block');
        $('#dvtxtSearch').css('display', 'block');
        $('#txtFromDate').css('display', 'none');
        $('#txtToDate').css('display', 'none');

        filter = "";
        allOpenFilter = "All";
        $('ul.pager li').remove();
        orderBy = 2;
        orderDir = "desc";
        $('#btnAllOrders').removeClass('btn-outline-secondary').addClass('btn-outline-primary');
        $('#btnOpenOrders').removeClass('btn-outline-primary').addClass('btn-outline-secondary');
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);

    });

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
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);
        }
    });

    $('#btnAllOrders').click(function () {
        $('ul.pager li').remove();
        allOpenFilter = "All";
        $('#btnAllOrders').removeClass('btn-outline-secondary').addClass('btn-outline-primary');
        $('#btnOpenOrders').removeClass('btn-outline-primary').addClass('btn-outline-secondary');
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);
    });

    $('#btnOpenOrders').click(function () {
        $('ul.pager li').remove();
        allOpenFilter = "Open";
        $('#btnOpenOrders').removeClass('btn-outline-secondary').addClass('btn-outline-primary');
        $('#btnAllOrders').removeClass('btn-outline-primary').addClass('btn-outline-secondary');
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter, allOpenFilter);
    });

});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter, allOpenFilter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesOrders/';

    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=SalesOrdersListDotNetAPI&filter=' + filter + '&allOpenFilter=' + allOpenFilter, function (data) {
        $('#hdnSPSOCount').val(data);
    });

    $.ajax(
        {
            url: '/SPSalesOrders/GetSalesOrdersListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top + '&allOpenFilter=' + allOpenFilter,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {

                    var rowData = "<tr><td></td><td>" + item.No + "</td><td>" + item.Document_Date + "</td><td align='right'>" + commaSeparateNumber(item.Amount.toFixed(2)) + "</td><td align='right'>" + commaSeparateNumber(item.Amount_Including_VAT.toFixed(2)) + "</td>";

                    if (item.Status == "Open") {
                        rowData += "<td><span class='badge bg-warning text-dark'>" + item.Status + "</span></td>";
                    }
                    else if (item.Status == "Released") {
                        rowData += "<td><span class='badge bg-success'>" + item.Status + "</span></td>";
                    }

                    rowData += "</tr>";

                    $('#tableBody').append(rowData);
                    // loop and do whatever with data

                });
                if (firsload == 1) {
                    pageMe();
                }
                dataTableFunction(orderBy, orderDir);

                if (data.length == 0) {
                    $('ul.pager li').remove();
                }

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function commaSeparateNumber(val) {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
    }
    return val;
}

function dataTableFunction(orderBy, orderDir) {
    dtable = $('#dataList').DataTable({
        retrieve: true,
        filter: false,
        paging: false,
        info: false,
        responsive: true,
        ordering: false,
    });

    if (orderDir == "asc") {
        $('#dataList th:first-child').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(0)').removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:first-child').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(0)').removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_asc").addClass("sorting_desc");
    }
}

function pageMe() {

    if (filter != "" || filter != null)
        $('ul.pager li').remove();

    var opts = {
        pagerSelector: '#myPager',
        showPrevNext: true,
        hidePageNumbers: false,
        perPage: $('#ddlRecPerPage').val()
    };
    var $this = $('#tableBody'),
        defaults = {
            perPage: 7,
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

    var numItems = $('#hdnSPSOCount').val(); //32;// children.length;
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
        goTo(clickedPage, skip1, top1, orderBy, orderDir);
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
        goTo(goToPage, skip1, top1, orderBy, orderDir);
    }

    function next() {
        goToPage = parseInt(pager.data("curr")) + 1;
        var skip1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("skip");
        var top1 = $('#pg' + (goToPage + 1) + ' .page_link').attr("top");
        goTo(goToPage, skip1, top1, orderBy, orderDir);
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
            if (page < curr - 2) nextpg.after($("<li class='ell'>").html("<span>...</span>"));
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

        bindGridData(skip2, top2, 0, orderBy, orderDir, filter, allOpenFilter);
    }
};
function exportGridData(skip, top, firsload, orderBy, orderDir, filter) {
    $.ajax(
        {
            url: '/SPSalesOrders/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top + '&allOpenFilter=' + allOpenFilter,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //alert(data);
                //debugger;
                if (data.fileName != "") {
                    //use window.location.href for redirect to download action for download the file
                    window.location.href = "/SPSalesOrders/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function ShowErrMsg(errMsg) {

    Lobibox.notify('error', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        delayIndicator: false,
        icon: 'bx bx-x-circle',
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: errMsg
    });

}