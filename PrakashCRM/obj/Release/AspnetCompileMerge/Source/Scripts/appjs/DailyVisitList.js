/* start pagination filter code */
var filter = "";
var orderBy = 3;
var orderDir = "desc";
$(document).ready(function () {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlField').change(function () {

        if ($('#ddlField').val() == "Date" || $('#ddlField').val() == "Payment_Date") {
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

        filter = "";

        if ($('#ddlField').val() == "Date" || $('#ddlField').val() == "Payment_Date") {
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
                        filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        break;
                    case 'Not Equal':
                        filter = $('#ddlField').val() + ' ne \'' + $('#txtSearch').val() + '\'';
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
                    default:
                        filter = "";
                        break;
                }

            }

        }

        if (flag == false) {

            var msg = "Please Fill All Filter Details";
            ShowErrMsg(msg);

        }
        else {
            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }

    });

    $('#btnClearFilter').click(function () {

        ClearCustomFilter();

        $('#ddlField').val('-1');
        $('#ddlOperator').val('Contains');
        $('#txtSearch').val('');

        filter = "";
        $('ul.pager li').remove();
        orderBy = 3;
        orderDir = "desc";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#btnExport').click(function () {

        switch ($('#ddlOperator').val()) {
            case 'Equal':
                filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                break;
            case 'Not Equal':
                filter = $('#ddlField').val() + ' ne \'' + $('#txtSearch').val() + '\'';
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
            default:
                filter = "";
                break;
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);

    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex > 2) {
            orderBy = parseInt(this.cellIndex);
            orderDir = "asc";

            if (this.asc) {
                orderDir = "asc";
            }
            else {
                orderDir = "desc";
            }
            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });

    $('.btn-close').click(function () {

        $('#modalDetails').css('display', 'none');
        $('.modal-title').text('');
        $('#dvComplainDetails').css('display', 'none');
        $('#dvPaymentDetails').css('display', 'none');
        $('#dvExpanseDetails').css('display', 'none');
        $('#dvProductDetails').css('display', 'none');

    });
});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';

    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hfSPCode').val() + '&Page=DailyVisit&apiEndPointName=DailyVisitsDotNetAPI&filter=' + filter, function (data) {
        $('#hdnDVPCount').val(data);
    });

    $.ajax(
        {
            url: '/SPVisitEntry/GetDailyVisitsListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {

                    var rowData = "<tr><td></td><td><a href='/SPVisitEntry/DailyVisit?No=" + item.No + "'><i class='bx bxs-edit'></i></a></td>" +
                        "<td><a class='ViewProdCls' onclick='ShowDetails(\"" + item.No + "\",\"Product\")'><i class='bx bx-show'></i></a></td><td>" +
                        item.Date + "</td><td>" + item.Visit_Name + "</td><td>" + item.Visit_SubType_Name + "</td><td>" + item.Contact_Company_Name +
                        "</td><td>" + item.Contact_Person_Name + "</td><td>" + item.Event_Name + "</td><td>" + item.Topic_Name + "</td><td>" +
                        item.Mode_of_Visit + "</td><td>" + item.Feedback + "</td><td>" + item.Remarks + "</td><td>" + item.Market_Update + "</td><td>" +
                        item.Payment_Date + "</td><td>" + item.Payment_Amt + "</td><td>" + item.Payment_Remarks + "</td><td>" + item.Complain_Subject +
                        "</td><td>" + item.Com_Date + "</td><td>" + item.Root_Analysis + "</td><td>" + item.Root_Analysis_date + "</td><td>" +
                        item.Corrective_Action + "</td><td>" + item.Corrective_Action_Date + "</td><td>" + item.Preventive_Action + "</td><td>" +
                        item.Preventive_Date + "</td><td>" + item.Complain_Invoice + "</td><td></td><td>" + item.Complain_Assign_To + "</td>";

                    if (item.Is_PDC) {
                        rowData += "<td>Yes</td>";
                    }
                    else {
                        rowData += "<td>No</td>";
                    }
                    
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
        $('#dataList th:lt(3)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(2)').removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(3)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(2)').removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnDVPCount').val(); //32;// children.length;
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

        bindGridData(skip2, top2, 0, orderBy, orderDir, filter);
    }
};

function exportGridData(skip, top, firsload, orderBy, orderDir, filter) {

    $.ajax(
        {
            url: '/SPVisitEntry/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.fileName != "") {
                    //use window.location.href for redirect to download action for download the file
                    window.location.href = "/SPVisitEntry/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function ClearCustomFilter() {

    $('#ddlField').val('-1');
    $('#ddlOperator').val('Contains');
    $('#txtSearch').val('');
    $('#txtFromDate').val('');
    $('#txtToDate').val('');
    $('#ddlOperator').css('display', 'block');
    $('#dvtxtSearch').css('display', 'block');
    $('#txtFromDate').css('display', 'none');
    $('#txtToDate').css('display', 'none');

}

function ShowDetails(dvpNo,detailsOf) {
    debugger;
    var apiUrl = $('#getServiceApiUrl').val() + 'SPVisitEntry/';
    var detailsUrl = "";

    if (detailsOf == "Complain") {
        detailsUrl = "/SPVisitEntry/GetComplainDetails?dvpNo=";
    }
    else if (detailsOf == "Payment") {
        detailsUrl = "/SPVisitEntry/GetPaymentDetails?dvpNo=";
    }
    else if (detailsOf == "Expanse") {
        detailsUrl = "/SPVisitEntry/GetExpanseDetails?dvpNo=";
    }
    else if (detailsOf == "Product") {
        detailsUrl = "/SPVisitEntry/GetDailyVisitProductDetails?dvpNo=";
    }

    $.ajax(
        {
            url: detailsUrl + dvpNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (detailsOf == "Complain") {

                    $('#tblComplainDet').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            rowData = "<tr><td>" + item.Com_Date + "</td><td>" + item.Complain_Subject + "</td><td>" + item.Complain_Product_Name + "</td><td>" + item.Complain_Assign_To + "</td>" +
                                "<td>" + item.Root_Analysis + "</td><td>" + item.Corrective_Action + "</td><td>" + item.Preventive_Action + "</td></tr>";
                            $('#tblComplainDet').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=7>No Records Found</td></tr>";
                        $('#tblComplainDet').append(rowData);
                    }

                    $('#modalDetails').css('display', 'block');
                    $('.modal-title').text('Complain Details');
                    $('#dvComplainDetails').css('display', 'block');
                    $('#dvPaymentDetails').css('display', 'none');
                    $('#dvExpanseDetails').css('display', 'none');
                    $('#dvProductDetails').css('display', 'none');

                }
                else if (detailsOf == "Payment") {

                    $('#tblPaymentDet').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            rowData = "<tr><td>" + item.Payment_Date + "</td><td>" + item.Payment_Amt + "</td><td>" + item.Payment_Remarks + "</td></tr>";
                            $('#tblPaymentDet').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=3>No Records Found</td></tr>";
                        $('#tblPaymentDet').append(rowData);
                    }

                    $('#modalDetails').css('display', 'block');
                    $('.modal-title').text('Payment Details');
                    $('#dvComplainDetails').css('display', 'none');
                    $('#dvPaymentDetails').css('display', 'block');
                    $('#dvExpanseDetails').css('display', 'none');
                    $('#dvProductDetails').css('display', 'none');
                }
                else if (detailsOf == "Expanse") {
                   
                    $('#tblExpanseDet').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            rowData = "<tr><td>" + item.Visit_Date + "</td><td>" + item.Start_Time + "</td><td>" + item.End_Time + "</td><td>" + item.Total_Time + "</td>" +
                                "<td>" + item.Start_km + "</td><td>" + item.End_km + "</td><td>" + item.Total_km + "</td></tr>";
                            $('#tblExpanseDet').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=7>No Records Found</td></tr>";
                        $('#tblExpanseDet').append(rowData);
                    }

                    $('#modalDetails').css('display', 'block');
                    $('.modal-title').text('Expanse Details');
                    $('#dvComplainDetails').css('display', 'none');
                    $('#dvPaymentDetails').css('display', 'none');
                    $('#dvExpanseDetails').css('display', 'block');
                    $('#dvProductDetails').css('display', 'none');

                }
                else if (detailsOf == "Product") {

                    $('#tblProductDet').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            rowData = "<tr><td>" + item.Product_Name + "</td><td>" + item.Quantity + "</td><td>" + item.Unit_of_Measure + "</td>" +
                                "<td>" + item.Competitor + "</td></tr>";
                            $('#tblProductDet').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=4>No Records Found</td></tr>";
                        $('#tblProductDet').append(rowData);
                    }

                    $('#modalDetails').css('display', 'block');
                    $('.modal-title').text('Product Details');
                    $('#dvComplainDetails').css('display', 'none');
                    $('#dvPaymentDetails').css('display', 'none');
                    $('#dvExpanseDetails').css('display', 'none');
                    $('#dvProductDetails').css('display', 'block');
                }

            },
            error: function () {
                alert("error");
            }
        }
    );
}