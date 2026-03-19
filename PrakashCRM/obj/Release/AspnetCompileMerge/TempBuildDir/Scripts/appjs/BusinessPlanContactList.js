//
var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';
var filter = "";
var orderBy = 1;
var orderDir = "asc";
$(document).ready(function () {

    BindFinancialYear();

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#btnSearch').click(function () {
        if ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1" || $('#txtSearch').val() == "") {

            var msg = "Please Fill All Filter Details";
            ShowErrMsg(msg);

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

            $('ul.pager li').remove();

            if ($('#ddlStatus').val() != "-1" && filter != "") {
                filter += " and Status eq '" + $('#ddlStatus').val() + "'";
            }
            else if ($('#ddlStatus').val() != "-1" && filter == "") {
                filter = "Status eq '" + $('#ddlStatus').val() + "'";
            }

            filter += " and Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });

    $('#btnClearFilter').click(function () {

        //$('#ddlField').val('-1');
        //$('#ddlOperator').val('Contains');
        //$('#txtSearch').val('');

        ClearCustomFilter();

        $('#ddlStatus').val('Not Filled');

        filter = "";
        $('ul.pager li').remove();
        orderBy = 1;
        orderDir = "asc";
        filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "' and Status eq '" + $('#ddlStatus').val() + "'";
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
            default:
                filter = "";
                break;
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);
    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex == 1) {
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
        $('#modalFeedbackLines').css('display', 'none');
    });

    $('#ddlFinancialYear').change(function () {

        ClearCustomFilter();
        filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";

        if ($('#ddlStatus').val() != "-1") {
            filter += " and Status eq '" + $('#ddlStatus').val() + "'";
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlStatus').change(function () {

        ClearCustomFilter();
        filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";

        if ($('#ddlStatus').val() != "-1") {
            filter += " and Status eq '" + $('#ddlStatus').val() + "'";
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    });

    $('#btnSendApproval').click(function () {

        $('#divImage').show();

        $.post(apiUrl + "SendBusinessPlanForApproval?SPCode=" + $('#hdnLoggedInUserSPCode').val() + "&PlanYear=" + $('#ddlFinancialYear').val(), function (data) {

            $('#divImage').hide();
            var resMsg = data;
            if (resMsg == "True") {

                //var actionMsg = "Business Plan Successfully Send For Approval";
                //ShowActionMsg(actionMsg);
                $('#modalApprovalMsg').css('display', 'block');

            }
            else if (resMsg.includes("Error:")) {
                const resMsgDetails = resMsg.split(':');

                $('#modalErrMsg').css('display', 'block');
                $('#modalErrDetails').text(resMsgDetails[1]);
            }

        });

    });

    $('#btnCloseApprovalMsg').click(function () {

        $('#modalApprovalMsg').css('display', 'none');
        location.reload(true);

    });


    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

    $.get(apiUrl + 'GetApiRecordsCount?page=BusinessPlanContactList&SPNo=' + $('#hdnLoggedInUserSPCode').val() + '&LoggedInUserNo=&apiEndPointName=Business_Plan_Customer_Wise&filter=' + filter, function (data) {
        $('#hdnContactCount').val(data);
    });

    //url: '/SPBusinessPlan/GetContactCompanyListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
    $.ajax(
        {
            url: '/SPBusinessPlan/GetBusinessPlanCustWiseListData?page=BusinessPlanContactList&SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();

                //var totalDemandQty = 0, totalTargetQty = 0;

                $.each(data, function (index, item) {
                    var rowData = "<tr><td></td><td>" + item.Customer_Name + "</td><td>" + item.Prev_Year_Demand_Qty.toFixed(3) + "</td><td>" + item.Prev_Year_Target_Qty.toFixed(3) +
                        "</td><td>" + item.Prev_Year_Achieved_Qty.toFixed(3) + "</td><td>" + item.Total_Demand_Qty.toFixed(3) + "</td><td>" + item.Targeted_Qty.toFixed(3) + "</td>";

                    //totalDemandQty += parseFloat(item.Total_Demand_Qty.toFixed(3));
                    //totalTargetQty += parseFloat(item.Targeted_Qty.toFixed(3));

                    if (item.Status == "Not Filled") {
                        rowData += "<td><span class='badge bg-warning text-dark'>Not Filled</span></td>";
                    }
                    else if (item.Status == "Filled") {
                        rowData += "<td><span class='badge bg-success'>Filled</span></td>";
                    }
                    else if (item.Status == "Submitted") {
                        rowData += "<td><span class='badge bg-primary'>Sent For Approval</span></td>";
                    }
                    else if (item.Status == "Approved") {

                        rowData += "<td><span class='badge bg-success'>Approved</span></td>";
                    }
                    else if (item.Status == "Rejected") {

                        rowData += "<td><span class='badge bg-danger'>Rejected</span></td>";
                    }

                    rowData += "<td><a class='AddCls' onclick='CreateBusinessPlan(\"" + item.Customer_No + "\",\"" + item.Customer_Name + "\",\"" + item.Plan_Year + "\")'>" +
                        "<img src='../Layout/assets/images/appImages/Plus-Icon.png' width='30' height='30' /></a></td>";
                        
                    
                    if (item.Status == "Rejected") {
                        rowData += "<td>" + item.Rejected_Reason + "</td>";
                    }
                    else {
                        rowData += "<td></td>";
                    }

                    rowData += "</tr>";

                    $('#tableBody').append(rowData);

                });

                //$('#lblTotalDemandQty').text(totalDemandQty.toFixed(3));
                //$('#lblTotalTargetQty').text(totalTargetQty.toFixed(3));

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


    $.ajax(
        {
            url: '/SPBusinessPlan/GetTotalDemandAndTargetQtyOfAllCust?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&filter=' + filter,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data != null) {

                    $('#lblTotalDemandQty').text(data.totalDemandQty.toFixed(3));
                    $('#lblTotalTargetQty').text(data.totalTargetQty.toFixed(3));

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
        $('#dataList tr:eq(1) th:lt(1)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th:gt(1)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th').slice(1, 2).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList tr:eq(1) th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList tr:eq(1) th:lt(1)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th:gt(1)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th').slice(1, 2).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList tr:eq(1) th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_asc").addClass("sorting_desc");
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

    var numItems = $('#hdnContactCount').val(); //32;
    var numPages = Math.ceil(numItems / perPage);

    pager.data("curr", 0);

    if (settings.showPrevNext) {
        $('<li><a href="#" class="prev_link">«</a></li>').appendTo(pager);
    }

    var curr = 0;
    var skip = 0, top = $('#ddlRecPerPage').val();

    while (numPages > curr && (settings.hidePageNumbers == false)) {
        $('<li id="pg' + (curr + 1) + '" class="pg"><a href="#" skip=' + skip + ' top=' + top + ' class="page_link">' + (curr + 1) + '</a></li>').appendTo(pager);
        skip = skip + parseInt($('#ddlRecPerPage').val());
        curr++;
    }

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

function BindFinancialYear() {

    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    let currentMonth = currentDate.getMonth();

    var yearOpts = "";
    yearOpts += "<option value='-1'>---Select---</option>";
    var prevFinancialYear = (currentYear - 1) + '-' + currentYear;
    var currFinancialYear = currentYear + '-' + (currentYear + 1);

    if (currentMonth <= 2) {
        /*yearOpts += "<option value='" + (currentYear - 1) + '-' + currentYear + "'>" + (currentYear - 1) + '-' + currentYear + "</option>";*/
        yearOpts += "<option value='" + prevFinancialYear + "'>" + prevFinancialYear + "</option>";
    }

    yearOpts += "<option value='" + currFinancialYear + "'>" + currFinancialYear + "</option>";

    $('#ddlFinancialYear').append(yearOpts);

    if (currentMonth <= 2) {
        $('#ddlFinancialYear').val(prevFinancialYear);
    }
    else {
        $('#ddlFinancialYear').val(currFinancialYear);
    }

    $('#lblPrevFinancialYear').text(prevFinancialYear);
    $('#lblFinancialYear').text(currFinancialYear);
    filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "' and Status eq '" + $('#ddlStatus').val() + "'";
    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
}

function ClearCustomFilter() {

    $('#ddlField').val('-1');
    $('#ddlOperator').val('Contains');
    $('#txtSearch').val('');
    
}

function ShowActionMsg(actionMsg) {

    Lobibox.notify('success', {
        pauseDelayOnHover: true,
        size: 'mini',
        rounded: true,
        icon: 'bx bx-check-circle',
        delayIndicator: false,
        continueDelayOnInactiveTab: false,
        position: 'top right',
        msg: actionMsg
    });

}

//
//var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

//$(document).ready(function () {

//    BindCustomers();

//});

//function BindCustomers() {

//    $.get(apiUrl + 'GetAllCompanyForDDL?SPCode=' + $('#hdnLoggedInUserSPCode').val(), function (data) {

//        if (data != null) {

//            var TROpts = "";
//            var i;
//            for (i = 0; i < data.length; i++) {
//                TROpts += "<tr><td hidden>" + data[i].No + "</td><td>" + data[i].Name + "</td><td>0.00</td><td><span class='badge bg-warning text-dark'>Not Fill</span></td>" +
//                    "<td><a href=''><i class='bx bx-plus-circle'></i></td>";
//            }

//            $('#ddlCustomer').append(opt);
//        }

//    });

//}