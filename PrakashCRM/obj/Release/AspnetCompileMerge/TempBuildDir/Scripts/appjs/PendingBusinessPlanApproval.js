var filter = "";
var orderBy = 2;
var orderDir = "asc";
var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';
var PlanYear, SPCode, SPName;
$(document).ready(function () {

    //BindFinancialYear();

    var UrlVars = getUrlVars();

    if (UrlVars["PlanYear"] != undefined && UrlVars["SPCode"] != undefined && UrlVars["SPName"] != undefined) {

        PlanYear = UrlVars["PlanYear"];
        SPCode = UrlVars["SPCode"];
        SPName = UrlVars["SPName"];
        SPName = SPName.replace("%20", " ");

        const PlanYear_ = PlanYear.split('-');
        var prevFinancialYear = (parseInt(PlanYear_[0]) - 1) + "-" + PlanYear_[0];

        $('#lblPrevFinancialYear').text(prevFinancialYear);
        $('#lblFinancialYear').text(PlanYear);
        $('#lblFinancialYearGrid').text(PlanYear);
        //$('#lblFinancialYear').text(PlanYear);
        $('#lblSP').text(SPName);
        filter += "Plan_Year eq '" + PlanYear + "' and Status eq '" + $('#ddlStatus').val() + "'";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    }

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlStatus').change(function () {

        ClearCustomFilter();

        filter = "Plan_Year eq '" + PlanYear + "'";

        if ($('#ddlStatus').val() != "-1") {
            filter += " and Status eq '" + $('#ddlStatus').val() + "'";
        }

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

            if ($('#ddlStatus').val() != "-1" && filter != "") {
                filter += " and Status eq '" + $('#ddlStatus').val() + "'";
            }

            filter += " and Plan_Year eq '" + PlanYear + "'";

            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });

    $('#btnClearFilter').click(function () {

        ClearCustomFilter();

        $('#ddlStatus').val('Submitted');
        
        filter = "";

        if (UrlVars["PlanYear"] != undefined && UrlVars["SPCode"] != undefined) {

            PlanYear = UrlVars["PlanYear"];
            SPCode = UrlVars["SPCode"];
            $('#lblFinancialYear').text(PlanYear);
            filter = "Plan_Year eq '" + PlanYear + "' and Status eq '" + $('#ddlStatus').val() + "'";
            
        }

        $('ul.pager li').remove();
        orderBy = 2;
        orderDir = "asc";
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
        if (this.cellIndex == 2) {
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

    //$('#ddlFinancialYear').change(function () {

    //    filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
    //    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    //});

    $('#btnCloseApproveRejectMsg').click(function () {

        $('#modalApproveRejectMsg').css('display', 'none');
        $('#lblApproveRejectMsg').text("");
        location.reload(true);
        
    });

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrMsg').css('display', 'none');
        $('#modalErrDetails').text("");

    });

    $('#chkAll').click(function () {

        $('#tableBody input[type=checkbox]').prop('checked', this.checked);

    });

    $('#btnReject').click(function () {

        var flag = isCheckBoxesSelected();

        if (flag == false) {
            var msg = "Please Select Customer";
            ShowErrMsg(msg);
        }
        else {
            $('#modalRejectRemarks').css('display', 'block');
        }

    });

    $('#btnConfirmReject').click(function () {

        if ($('#txtRejectRemarks').val() == "") {
            $('#lblRemarksMsg').text("Please Fill Remarks");
        }
        else {
            ApproveRejectBusinessPlan("Reject", $('#txtRejectRemarks').val());
        }

    });

    $('#btnCloseModalRejectRemarks').click(function () {

        $('#modalRejectRemarks').css('display', 'none');

    });

    $('#btnCloseModalCustBusinessPlan').click(function () {

        $('#modalCustBusinessPlan').css('display', 'none');
    });

});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPBusinessPlan/';

    //$.get(apiUrl + 'GetApiRecordsCount?page=CustWiseForPendingApproval&SPNo=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=Business_Plan_Customer_Wise&filter=' + filter, function (data) {

    $.get(apiUrl + 'GetApiRecordsCount?page=CustWiseForPendingApproval&SPNo=' + SPCode + '&LoggedInUserNo=' + $('#hdnLoggedInUserNo').val() + '&apiEndPointName=Business_Plan_Customer_Wise&filter=' + filter, function (data) {
        $('#hdnContactCount').val(data);
    });

    //url: '/SPBusinessPlan/GetContactCompanyListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,

    $.ajax(
        {
            url: '/SPBusinessPlan/GetBusinessPlanCustWiseListData?page=CustWiseForPendingApproval&SPCode=' + SPCode + '&orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                var a = 0;
                $.each(data, function (index, item) {
                    var rowData = "<tr><td></td>";

                    if (item.Status == "Submitted") {
                        rowData += "<td><input type='checkbox' id=\"chk_" + item.Plan_Year + "_" + item.Customer_No + "_" + item.Salesperson_Purchaser + "\" class='form-check-input'></td>";
                    }
                    else {
                        rowData += "<td></td>";
                    }

                    rowData += "<td><a style='cursor:pointer' onclick='ShowCustBusinessPlan(\"" + item.Plan_Year + "\",\"" + item.Customer_No + "\",\"" + item.Customer_Name + "\")'>"
                        + item.Customer_Name + "</a></td><td>" + item.Prev_Year_Demand_Qty + "</td><td>" + item.Prev_Year_Target_Qty + "</td><td>" + item.Prev_Year_Achieved_Qty + 
                        "</td><td>" + item.Total_Demand_Qty + "</td><td>" + item.Targeted_Qty + "</td>";

                    if (item.Status == "Submitted") {

                        //rowData += "<td><button id='btnApproveRejectSpinner" + a + "' class='btn btn-primary' type='button' disabled style='display:none'><span class='spinner-border spinner-border-sm' role='status' aria-hidden='true'></span>" +
                        //    "<span class='visually-hidden'>Loading...</span></button>&nbsp;<button type='button' class='btn btn-outline-success px-5' onclick='ApproveRejectBusinessPlan(\"Approve\",\"" + item.Plan_Year + "\",\"" +
                        //    item.Customer_No + "\",\"btnApproveRejectSpinner" + a + "\",\"" + item.Salesperson_Purchaser + "\")'>" +
                        //    "<i class='bx bx-check mr-1'></i>Approve</button>&nbsp;<button type='button' class='btn btn-outline-danger px-5' onclick='ApproveRejectBusinessPlan(\"Reject\",\"" + item.Plan_Year + "\",\"" +
                        //    item.Customer_No + "\",\"btnApproveRejectSpinner" + a + "\",\"" + item.Salesperson_Purchaser + "\")'>" +
                        //    "<i class='bx bx-x mr-1'></i>Reject</button></td>";

                        rowData += "<td><span class='badge bg-primary'>Pending Approval</span></td>" +
                            "<td></td>";
                    }
                    else if (item.Status == "Approved") {

                        rowData += "<td><span class='badge bg-success'>Approved</span></td>" +
                            "<td></td>";
                    }
                    else if (item.Status == "Rejected") {

                        rowData += "<td><span class='badge bg-danger'>Rejected</span></td>" + 
                            "<td>" + item.Rejected_Reason + "</td>";
                    }

                    rowData += "</tr>";
                    //a += 1;

                    $('#tableBody').append(rowData);

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
        $('#dataList tr:eq(1) th:lt(2)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th:gt(2)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th').slice(2, 3).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList tr:eq(1) th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList tr:eq(1) th:lt(2)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th:gt(2)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList tr:eq(1) th').slice(2, 3).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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
    filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";
    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function ShowCustBusinessPlan(PlanYear, CustNo, CustName) {

    $('#modalCustBusinessPlan').css('display', 'block');
    $('#lblDetailsYear').text(PlanYear);
    $('#lblDetailsYearGrid').text(PlanYear);

    const PlanYear_ = PlanYear.split('-');
    $('#lblDetailsPrevYear').text((parseInt(PlanYear_[0]) - 1) + "-" + PlanYear_[0]);
    $('#lblDetailsCustName').text(CustName);

    $.get(apiUrl + 'GetCustomerBusinessPlan?SPCode=' + SPCode + '&CustomerNo=' + CustNo + '&PlanYear=' + PlanYear, function (data) {

        var TROpts = "";
        var i;
        $('#tblDetailsCustBusinessPlan').empty();

        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td hidden>" + data[i].Product_No + "</td><td>" + data[i].Product_Name + "</td><td>" + data[i].Pre_Year_Demand.toFixed(3) + "</td><td>" + data[i].Pre_Year_Target.toFixed(3) + "</td><td>" +
                    data[i].Last_year_Sale_Qty.toFixed(3) + "</td><td>" + data[i].Last_year_Sale_Amount.toFixed(2) + "</td>" +
                    "<td>" + data[i].Demand.toFixed(3) + "</td><td>" + data[i].Target.toFixed(3) + "</td><td>" + data[i].Average_Price.toFixed(2) + "</td><td>" +
                    data[i].PCPL_Target_Revenue.toFixed(2) + "</td></tr>";

            }

        }
        else {
            TROpts += "<tr><td colspan='10'>No Data Found</td></tr>";
        }

        $('#tblDetailsCustBusinessPlan').append(TROpts);

    });

}

function ApproveRejectBusinessPlan(Action, RejectRemarks) {

    var flag = isCheckBoxesSelected();

    if (flag == false) {
        var msg = "Please Select Customer";
        ShowErrMsg(msg);
    }
    else {

        $('#btnApproveRejectSpinner').show();
        if (Action == "Reject") {
            $('#btnRejectSpinner').show();
        }
        let BusinessPlanDetails = new Array();
        var a = 0;
        var errMsg = "";

        $('#tableBody input[type=checkbox]:checked').each(function () {

            BusinessPlanDetails[a] = $(this).prop("id");
            a += 1;

        });

        for (var a = 0; a < BusinessPlanDetails.length; a++) {

            const BusinessPlanDetails_ = BusinessPlanDetails[a].split('_');

            var financialYear = BusinessPlanDetails_[1];
            var custNo = BusinessPlanDetails_[2];
            var spCode = BusinessPlanDetails_[3];

            $.post(apiUrl + "BusinessPlanApproveReject?SPCode=" + spCode + "&LoggedInUserNo=" + $('#hdnLoggedInUserNo').val() + "&PlanYear=" + financialYear +
                "&CustomerNo=" + custNo + "&Action=" + Action + "&RejectReason=" + RejectRemarks, function (data) {

                    //$("#" + BtnSpinner).hide();
                    var resMsg = data;
                    if (resMsg == "True") {

                        //var actionMsg = "Business Plan Successfully Send For Approval";
                        //ShowActionMsg(actionMsg);
                        

                    }
                    else if (resMsg.includes("Error:")) {

                        $('#btnApproveRejectSpinner').hide();
                        errMsg = "Error";
                        const resMsgDetails = resMsg.split(':');

                        $('#modalErrMsg').css('display', 'block');
                        $('#modalErrDetails').text(resMsgDetails[1]);
                    }

            });

        }

        if (errMsg == "") {

            $('#btnApproveRejectSpinner').hide();
            
            $('#modalApproveRejectMsg').css('display', 'block');
            if (Action == "Approve") {
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
                $('#lblApproveRejectMsg').text("Business Plan Approved Successfully");
            }
            else if (Action == "Reject") {
                $('#btnRejectSpinner').hide();
                $('#modalRejectRemarks').css('display', 'none');
                $('#resIcon').attr('src', '../Layout/assets/images/appImages/CancelIcon.png');
                $('#lblApproveRejectMsg').text("Business Plan Rejected");
            }

        }
    }
}

function isCheckBoxesSelected() {

    var checkboxes = $('input[type=checkbox]');
    var checked = checkboxes.filter(':checked');
    var flag;

    if (checked.length <= 0) {
        flag = false;
    }
    else {
        flag = true;
    }

    return flag;
}

function ClearCustomFilter() {

    $('#ddlField').val('-1');
    $('#ddlOperator').val('Contains');
    $('#txtSearch').val('');

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