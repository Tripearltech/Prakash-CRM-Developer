/* start pagination filter code */
var filter = "";
var orderBy = 3;
var orderDir = "asc";
var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

$(document).ready(function () {

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#hfIsScheduleOrder').val("false");

    $('#ddlField').change(function () {

        if ($('#ddlField').val() == "Posting_Date" || $('#ddlField').val() == "Due_Date") {
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

        if ($('#ddlField').val() == "Posting_Date" || $('#ddlField').val() == "Due_Date") {
            if ($('#txtFromDate').val() == "" || $('#txtToDate').val() == "") {
                flag = false;
            }
            else {
                filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
            }
        }
        else {
            //if ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1" || $('#txtSearch').val() == "") {
            //    flag = false;
            //}

            if ($('#ddlField').val() != "-1" && ($('#ddlOperator').val() == "-1" || $('#txtSearch').val() == "")) {
                flag = false;
            }
            else if ($('#ddlOperator').val() != "-1" && ($('#ddlField').val() == "-1" || $('#txtSearch').val() == "")) {
                flag = false;
            }
            else if ($('#txtSearch').val() != "" && ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1")) {
                flag = false;
            }
            else {
                switch ($('#ddlOperator').val()) {
                    case 'Equal':
                        if ($('#ddlField').val() != "Amount") {
                            filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        }
                        else {
                            filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                        }
                        break;
                    case 'Not Equal':
                        if ($('#ddlField').val() != "Amount") {
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

            var msg = "Please Fill All Filter Details";
            ShowErrMsg(msg);

        }
        else {
            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }

    });

    $('#btnExport').click(function () {


        if ($('#ddlField').val() == "Posting_Date" || $('#ddlField').val() == "Due_Date") {
            filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
        }
        else {

            switch ($('#ddlOperator').val()) {
                case 'Equal':
                    if ($('#ddlField').val() != "Amount") {
                        filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                    }
                    else {
                        filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                    }
                    break;
                case 'Not Equal':
                    if ($('#ddlField').val() != "Amount") {
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
        $('ul.pager li').remove();
        orderBy = 3;
        orderDir = "asc";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex >= 3 && this.cellIndex <= 6) {
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

    $('#btnCloseModalSQProds').click(function () {

        $('#modalSQProds').css('display', 'none');
    });

    $('#btnCloseApproveRejectMsg').click(function () {

        $('#modalApproveRejectMsg').css('display', 'none');
        $('#lblApproveRejectMsg').text("");
        location.reload(true);

    });

    $('#chkAll').click(function () {

        $('#tableBody input[type=checkbox]').prop('checked', this.checked);

    });

    $('#btnReject').click(function () {

        var checkboxes = $('input[type=checkbox]');
        var checked = checkboxes.filter(':checked');
        var SQNos_ = "";

        if (checked.length <= 0) {

            var msg = "Please Select Sales Quote";
            ShowErrMsg(msg);

        }
        else if (checked.length > 1) {

            var msg = "Please Select Single Sales Quote For Reject";
            ShowErrMsg(msg);

        }
        else if (checked.length == 1) {

            $('#modalRejectRemarks').css('display', 'block');

        }

    });

    $('#btnConfirmReject').click(function () {

        if ($('#txtRejectRemarks').val() == "") {
            $('#lblRemarksMsg').text("Please Fill Remarks");
        }
        else {
            ApproveRejectSQ("Reject", $('#txtRejectRemarks').val());
        }

    });

    $('#btnCloseModalRejectRemarks').click(function () {

        $('#modalRejectRemarks').css('display', 'none');

    });

});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    if ($('#hdnLoggedInUserRole').val() == "Finance") {
        UserRoleORReportingPerson = "Finance";
    }
    else {
        UserRoleORReportingPerson = "ReportingPerson";
    }

    $.get(apiUrl + 'GetApiRecordsCount?Page=SQListForApproveReject&LoggedInUserNo=' + $('#hdnLoggedInUserNo').val() + '&UserRoleORReportingPerson=' + UserRoleORReportingPerson + '&SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=SalesQuoteDotNetAPI&filter=' + filter, function (data) {
        $('#hdnSPSQCount').val(data);
    });

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSQListDataForApproveReject?UserRoleORReportingPerson=' + UserRoleORReportingPerson + '&orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                var a = 0;
                $.each(data, function (index, item) {

                    if (item.PCPL_Status != "0") {

                        /*"<td><a href='#'><i class='bx bx-message'></i></a></td><td><a href='#'><i class='bx bx-mail-send'></i></a></td><td><a href='#'><i class='bx bx-printer'></i></a></td>"*/
                        var rowData = "<tr><td></td>";

                        if (item.PCPL_Status == "Approval pending from HOD" || item.PCPL_Status == "Approval pending from finance") {
                            rowData += "<td><input type='checkbox' id=\"chk_" + item.No + "\" class='form-check-input'></td>";
                        }
                        else {
                            rowData += "<td></td>";
                        }
                        
                        rowData += "<td><a style='cursor:pointer' onclick='ShowSQProduct(\"" + item.No + "\",\"" + item.PCPL_Status + "\")'><i class='bx bx-show'></i></a></td><td><a onclick='RedirectToSQCard(\"" + item.No + "\",\"" + item.TPTPL_Schedule_status + "\",\"" + item.PCPL_Status + "\",SQFor=\"ApproveReject\",LoggedInUserRole=\"" + $('#hdnLoggedInUserRole').val() + "\")' href='#'>" + item.No + "</a></td><td>" +
                            item.Order_Date + "</td><td>" + item.Sell_to_Customer_Name + "</td><td>" + item.Payment_Terms_Code + "</td><td hidden>" + item.PCPL_ApprovalFor + "</td>";

                        if (item.PCPL_Status == "Approval pending from HOD" || item.PCPL_Status == "Approval pending from finance") {

                            //rowData += "<td><button id='btnApproveRejectSpinner" + a + "' class='btn btn-primary' type='button' disabled style='display:none'><span class='spinner-border spinner-border-sm' role='status' aria-hidden='true'></span>" +
                            //    "<span class='visually-hidden'>Loading...</span></button>&nbsp;<button type='button' class='btn btn-outline-success px-5' onclick='ApproveRejectSQ(\"Approve\",\"" + item.No + "\",\"btnApproveRejectSpinner" + a +
                            //    "\")'><i class='bx bx-check mr-1'></i>Approve</button>&nbsp;<button type='button' class='btn btn-outline-danger px-5' onclick='ApproveRejectSQ(\"Reject\",\"" + item.No + "\",\"btnApproveRejectSpinner" + a + "\")'><i class='bx bx-x mr-1'></i>Reject</button></td>";

                            rowData += "<td><span class='badge bg-primary'>Pending For Approval</span></td>";
                        }
                        //else if (item.PCPL_Status == "Approved") {

                        //    rowData += "<td><span class='badge bg-success'>Approved</span></td>";
                        //}
                        //else if (item.PCPL_Status == "Rejected by HOD" || item.PCPL_Status == "Rejected by finance") {

                        //    rowData += "<td><span class='badge bg-danger'>Rejected</span></td>";
                        //}

                        rowData += "<td>" + item.Salesperson_Code + "</td><td hidden>" + item.PCPL_SalesPerson_Email + "</td>";
                      
                        rowData += "</tr>";
                        
                        $('#tableBody').append(rowData);

                    }

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
        $('#dataList th:gt(6)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th').slice(3, 7).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(3)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(6)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th').slice(3, 7).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnSPSQCount').val(); //32;
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

function ShowSQProduct(SQNo, ApprovalStatus) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $('#modalSQProds').css('display', 'block');

    if (ApprovalStatus == "Approval pending from finance") {

        $('#dvAprDetailsFinanceUser').css('display', 'block');
        $('#dvAprDetailsRPerson').css('display', 'none');

    }
    else if (ApprovalStatus == "Approval pending from HOD") {

        $('#dvAprDetailsFinanceUser').css('display', 'none');
        $('#dvAprDetailsRPerson').css('display', 'block');

        $.ajax(
            {
                url: '/SPSalesQuotes/GetSalesLineItems?DocumentNo=' + SQNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#lblSQProdSQNo').text(SQNo);
                    $('#tblSQProduct').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            rowData = "<tr><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" +
                                item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.Unit_Price + "</td><td></td><td>" + item.Delivery_Date + "</td>" +
                                "<td>" + item.PCPL_Total_Cost + "</td><td>" + item.PCPL_Margin + "</td>";

                            if (item.Drop_Shipment == true) {
                                rowData += "<td>Yes</td>";
                            }
                            else {
                                rowData += "<td>No</td>";
                            }

                            /*rowData += "<td>" + item.PCPL_Vendor_Name + "</td></tr>";*/

                            rowData += "</tr>";

                            $('#tblSQProduct').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=9>No Records Found</td></tr>";
                        $('#tblSQProduct').append(rowData);
                    }

                },
                error: function () {
                    alert("error");
                }
            }
        );
    }
    
}

function ApproveRejectSQ(Action, RejectRemarks) {

    var checkboxes = $('input[type=checkbox]');
    var checked = checkboxes.filter(':checked');
    var SQNosAndApprovalFor_ = "";

    if (checked.length <= 0) {

        var msg = "Please Select Sales Quote";
        ShowErrMsg(msg);

    }
    else {

        if (RejectRemarks == null || RejectRemarks == "") {
            $('#btnApproveSpinner').show();
        }
        else {
            $('#btnRejectSpinner').show();
        }

        //$('#btnApproveSpinner').show();
        let SQNos = new Array();
        var a = 0;
        var str = "";

        $('#tableBody input[type=checkbox]:checked').each(function () {
            var row = $(this).closest("tr")[0];
            SQNos[a] = row.cells[3].innerHTML + ":" + row.cells[7].innerHTML;
            str += row.cells[3].innerHTML + ":" + row.cells[7].innerHTML + ":" + row.cells[10].innerHTML + ",";
            a += 1;
        });

        SQNosAndApprovalFor_ = str;

        //$.post(
        //    apiUrl + 'SendFeedbackFormLink?contactCompanyNo=' + ContactNos[a] + '&ToCCEmail=' + FeedbackEmailToCC[a] + '&contactName=' + ContactNames[a] + '&contactMobileNo=' + FeedbackMobiles[a] +
        //    '&contactAddress=' + ContactAddresses[a] + '&custVendorPortalUrl=' + custVendorPortalUrl + '&SPNo=' + $('#hfSPNo').val(),
        //    function (data) {
        //        if (data) {
        //            flag = true;
        //        }
        //        else {
        //            flag = false;
        //        }
        //    }
        //);
        var UserRoleORReportingPerson = "";

        if ($('#hdnLoggedInUserRole').val() == "Finance") {
            UserRoleORReportingPerson = "Finance";
        }
        else {
            UserRoleORReportingPerson = "ReportingPerson";
        }
        
        $.post(apiUrl + "SQApproveReject?SQNosAndApprovalFor=" + SQNosAndApprovalFor_ + "&LoggedInUserNo=" + $('#hdnLoggedInUserNo').val() + "&Action=" + Action + "&UserRoleORReportingPerson=" + UserRoleORReportingPerson + "&RejectRemarks=" + RejectRemarks + "&LoggedInUserEmail=" + $('#hdnLoggedInUserEmail').val(), function (data) {

            //$("#" + BtnSpinner).hide();
            var resMsg = data;

            if (RejectRemarks == null || RejectRemarks == "") {
                $('#btnApproveSpinner').hide();
            }
            else {
                $('#btnRejectSpinner').hide();
            }
            
            if (resMsg == "True") {

                //var actionMsg = "Business Plan Successfully Send For Approval";
                //ShowActionMsg(actionMsg);
                $('#modalApproveRejectMsg').css('display', 'block');
                if (Action == "Approve") {
                    $('#lblApproveRejectMsg').text("Sales Quote Approved Successfully");
                }
                else if (Action == "Reject") {
                    $('#modalRejectRemarks').css('display', 'none');
                    $('#lblApproveRejectMsg').text("Sales Quote Rejected Successfully");
                }

            }
            else if (resMsg.includes("Error:")) {
                const resMsgDetails = resMsg.split(':');

                $('#modalErrMsg').css('display', 'block');
                $('#modalErrDetails').text(resMsgDetails[1]);
            }

        });
        
    }

    //var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    //$("#" + BtnSpinner).show();

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