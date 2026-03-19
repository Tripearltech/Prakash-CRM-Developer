/* start pagination filter code */
var filter = "";
var orderBy = 6;
var orderDir = "desc";

$(document).ready(function () {

    BindUserSendNotif();

    filter += "Inquiry_Status eq '" + $('#ddlStatus').val() + "'";

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlField').change(function () {

        if ($('#ddlField').val() == "Inquiry_Date" || $('#ddlField').val() == "Delivery_Date") {
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

    $('#ddlStatus').change(function () {

        ClearCustomFilter();
        filter = "";

        if ($('#ddlStatus').val() != "-1") {
            filter += "Inquiry_Status eq '" + $('#ddlStatus').val() + "'";
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    });

    $('#btnSearch').click(function () {
        var flag = true;

        filter = "";

        if ($('#ddlField').val() == "Inquiry_Date" || $('#ddlField').val() == "Delivery_Date") {
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
            else
            {
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

        if ($('#ddlStatus').val() != "-1" && filter != "") {
            filter += " and Inquiry_Status eq '" + $('#ddlStatus').val() + "'";
        }
        else if ($('#ddlStatus').val() != "-1" && filter == "") {
            filter += "Inquiry_Status eq '" + $('#ddlStatus').val() + "'";
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

        exportGridData(0, 0, 0, orderBy, orderDir, filter);

    });

    $('#btnClearFilter').click(function () {

        //$('#ddlField').val('-1');
        //$('#ddlOperator').val('Contains');
        //$('#txtSearch').val('');
        //$('#txtFromDate').val('');
        //$('#txtToDate').val('');
        //$('#ddlOperator').css('display', 'block');
        //$('#dvtxtSearch').css('display', 'block');
        //$('#txtFromDate').css('display', 'none');
        //$('#txtToDate').css('display', 'none');

        ClearCustomFilter();

        $('#ddlStatus').val("Pending");
        filter = "Inquiry_Status eq '" + $('#ddlStatus').val() + "'";
        $('ul.pager li').remove();
        orderBy = 6;
        orderDir = "desc";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex >= 5 && this.cellIndex <= 10) {
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


        //$('#modalIP').css('display', 'none');
        //$('#dvInquiryProducts').css('display', 'none');
        //$('.modal-title').text('');
        //$('.modal-footer').css('display', 'none');

        //

        $('#modalInquiryList').css('display', 'none');
        $('.modal-title').text('');
        $('#dvInquiryProducts').css('display', 'none');
        $('#dvSendNotification').css('display', 'none');
        $('.modal-footer').css('display', 'none');

        //

    });

    $('#chkAll').click(function () {

        $('#tableBody input[type=checkbox]').prop('checked', this.checked);

    });

    $('#btnSendNotification').click(function () {

        var checkboxes = $('input[type=checkbox]');
        var checked = checkboxes.filter(':checked');

        if (checked.length <= 0) {

            var msg = "Please Select Inquiry";
            ShowErrMsg(msg);

        }
        else {

            $('#modalInquiryList').css('display', 'block');
            $('.modal-title').text('Send Notification');
            $('#dvInquiryProducts').css('display', 'none');
            $('#dvSendNotification').css('display', 'block');
            $('.modal-footer').css('display', 'block');

        }

    });

    $('#btnConfirmSend').click(function () {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';
        var InquiryNos = "";

        $('#btnSendNotificationSpinner').show();
        $('#tableBody input[type=checkbox]:checked').each(function () {

            var row = $(this).closest("tr")[0];
            InquiryNos += row.cells[5].innerHTML + ",";
            
        });

        $.post(apiUrl + 'SendNotification?userEmail=' + $('#ddlUserSendNotif').val() + '&SPEmail=' + $('#hdnSPEmail').val() + '&InqNos=' + InquiryNos
            + '&NotifMsg=' + $('#txtNotifMsg').val(), function (data) {

            if (data) {

                $('#btnSendNotificationSpinner').hide();
                //CreateTask($('#txtNotifMsg').val());
                $('#dvSendNotification').css('display', 'none');
                $('#modalInquiryList').css('display', 'none');
                $('#modalSent').css('display', 'block');
                $('#lblSentTitle').text("Notification");
                $('#lblSentMsg').text("Notification Sent Successfully");
            }

        });

    });

    $('#btnCloseSent').click(function () {

        $('#modalSent').css('display', 'none');
        $('#lblSentTitle').text("");
        $('#lblSentMsg').text("");
        $('#tableBody input[type=checkbox]').prop('checked', false);
        location.reload(true);

    });

    $('#btnAssign').click(function () {

        var checkboxes = $('input[type=checkbox]');
        var checked = checkboxes.filter(':checked');

        if (checked.length <= 0) {

            var msg = "Please Select Inquiry";
            ShowErrMsg(msg);

        }
        else {
            BindSalesperson();
            $('#modalAssign').css('display', 'block');
        }

    });

    $('#btnConfirmAssign').click(function () {

        var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

        if ($('#ddlSalesperson').val() == "-1") {
            $('#lblAssignSPMsg').css('display', 'block');
        }
        else {

            var InquiryNos = "";
            $('#btnAssignSpinner').show();

            $('#tableBody input[type=checkbox]:checked').each(function () {

                var row = $(this).closest("tr")[0];
                InquiryNos += row.cells[5].innerHTML + ",";

            });

            $.post(apiUrl + "AssignToSalesperson?SPDetails=" + $('#ddlSalesperson').val() + "&InqNos='" + InquiryNos + "'&LoggedInSPName=" + $('#hdnSPName').val() + "&AssignToMsg=" + $('#txtAssignToMsg').val(), function (data) {

                if (data) {

                    $('#btnAssignSpinner').hide();
                    $('#modalAssign').css('display', 'none');
                    $('#modalSent').css('display', 'block');
                    $('#lblSentTitle').text("Assign To Other Salesperson");
                    $('#lblSentMsg').text("Assigned Successfully");

                }

            });

        }

    });

    $('#btnCloseAssign').click(function () {

        $('#modalAssign').css('display', 'none');

    });

});
var dtable;

function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    var SPCode = "";
    if ($('#hdnSPCodesOfReportingPersonUser').val() == "" || $('#hdnSPCodesOfReportingPersonUser').val() == null) {
        SPCode = $('#hdnLoggedInUserSPCode').val();
    }
    else {
        SPCode = $('#hdnLoggedInUserSPCode').val() + "," + $('#hdnSPCodesOfReportingPersonUser').val();
    }

    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + SPCode + '&apiEndPointName=InquiryDotNetAPI&filter=' + filter, function (data) {
        $('#hdnSPIQCount').val(data);
    });

    $.ajax(
        {
            url: '/SPInquiry/GetInquiryListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {
                    /*var rowData = "<tr><td></td><td><input type='checkbox' /></td><td><a href='/SPInquiry/Inquiry?Inquiry_No='" + item.Inquiry_No + "><i class='bx bxs-edit'></i></a></td><td>" + item.Inquiry_No + "</td><td>" + item.Daily_Visit_Number + "</td><td>" + item.Inquiry_Date + "</td><td>" + item.Delivery_Date + "</td><td>" + item.Inq_Customer_Name + "</td><td>" + item.Salesperson_Code + "</td><td>" + item.Assign_To + "</td><td>" + item.Inquiry_Status + "</td><td><a class='ViewInqProdCls' onclick='ShowInquiryProduct(\"" + item.Inquiry_No + "\")'><i class='bx bx-show'></i></a></td></tr>";*/
                    /*var rowData = "<tr><td></td><td><input type='checkbox' class='form-check-input' /></td><td><a href='/SPInquiry/Inquiry?Inquiry_No='" + item.Inquiry_No + "'><i class='bx bxs-edit'></i></a>&nbsp;&nbsp;<a href='#'><i class='bx bxs-trash'></i></a></td><td>" + item.Inquiry_No + "</td><td>" + item.Inquiry_Date + "</td><td>" + item.Contact_Company_Name + "</td><td>" + item.Delivery_Date + "</td><td>" + item.Payment_Terms + "</td><td>" + item.Inquiry_Status + "</td><td>" + item.Salesperson_Code + "</td><td><a><i class='bx bx-file'></i></a></td><td><a class='ViewInqProdCls' onclick='ShowInquiryProduct(\"" + item.Inquiry_No + "\")'><i class='bx bx-show'></i></a></td><td>" + item.Consignee_Address + item.Consignee_Address_2 + item.Consignee_City + "</td><td>" + item.Remarks + "</td></tr>";*/
                    /*var rowData = "<tr><td></td><td><input type='checkbox' class='form-check-input' /></td><td><a href='/SPInquiry/Inquiry?Inquiry_No='" + item.Inquiry_No + "'><i class='bx bxs-edit'></i></a>&nbsp;&nbsp;<a href='#'><i class='bx bxs-trash'></i></a></td><td>" + item.Inquiry_No + "</td><td>" + item.Inquiry_Date + "</td><td>" + item.Contact_Company_Name + "</td><td>" + item.Payment_Terms + "</td><td>" + item.Inquiry_Status + "</td><td>" + item.Salesperson_Code + "</td><td><a><i class='bx bx-file'></i></a></td><td><a class='ViewInqProdCls' onclick='ShowInquiryProduct(\"" + item.Inquiry_No + "\")'><i class='bx bx-show'></i></a></td><td>" + item.Sell_to_Address + "," + item.Sell_to_Address_2 + "," + item.Sell_to_City + "-" + item.Sell_to_Post_Code + "</td><td>" + item.Remarks + "</td></tr>";*/
                    var rowData = "<tr><td></td><td><input type='checkbox' class='form-check-input' /></td>";

                    if (item.Inquiry_Status == "Completed") {
                        rowData += "<td><a onclick=''><i class='bx bx-lock-alt'></i>&nbsp;<i class='bx bxs-edit'></i></a></td>" +
                            "<td><a onclick=''><i class='bx bx-file'></i></a></td>";
                    }
                    else {
                        rowData += "<td><a class='EditCls' onclick='EditInquiry(\"" + item.Inquiry_No + "\")'><i class='bx bxs-edit'></i></a></td>" + 
                            "<td><a class='ConvertToQuoteCls' onclick='ConvertToQuote(\"" + item.Inquiry_No + "\")'><i class='bx bx-file'></i></a></td>";
                    }
 
                    rowData += "<td><a class='ViewInqProdCls' onclick='ShowInquiryProduct(\"" + item.Inquiry_No + "\")'><i class='bx bx-show'></i></a></td>" +
                        "<td>" + item.Inquiry_No + "</td><td>" + item.Inquiry_Date + "</td><td>" + item.Contact_Company_Name + "</td><td>" + item.Payment_Terms + "</td>";

                    if (item.Inquiry_Status == "Pending") {
                        rowData += "<td><span class='badge bg-danger'>" + item.Inquiry_Status + "</span></td>";
                    }
                    else if (item.Inquiry_Status == "Partial") {
                        rowData += "<td><span class='badge bg-info text-dark'>" + item.Inquiry_Status + "</span></td>";
                    }
                    else if (item.Inquiry_Status == "Completed") {
                        rowData += "<td><span class='badge bg-success'>" + item.Inquiry_Status + "</span></td>";
                    }

                    rowData += "<td>" + item.Salesperson_Code + "</td><td>" + item.Sell_to_Address + "," + item.Sell_to_Address_2 + "," + item.Sell_to_City + "-" + item.Sell_to_Post_Code + "</td>" + 
                        "<td>" + item.Inquiry_Status_Remarks + "</td><td>" + item.Ship_to_Name + "</td><td>" + item.Ship_to_Address + "," + item.Ship_to_Address_2 + "," + item.Ship_to_City + "-" + item.Ship_to_Post_Code +
                        "</td><td>" + item.PCPL_Job_to_Name + "</td><td>" + item.PCPL_Job_to_Address + "," + item.PCPL_Job_to_Address_2 + "," + item.PCPL_Job_to_City + "-" + item.PCPL_Job_to_Post_Code + "</td></tr>";
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
        $('#dataList th:lt(5)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(10)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th').slice(5, 11).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(5)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(10)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th').slice(5, 11).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnSPIQCount').val(); //32;// children.length;
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
            url: '/SPInquiry/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //alert(data);
                //debugger;
                if (data.fileName != "") {
                    //use window.location.href for redirect to download action for download the file
                    window.location.href = "/SPInquiry/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function ShowInquiryProduct(inquiryno) {
    
    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.ajax(
        {
            url: '/SPInquiry/GetAllInquiryProductForPopup?Document_No=' + inquiryno,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tbInqProduct').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        rowData = "<tr><td>" + item.Product_No + "</td><td>" + item.Product_Name + "</td><td>" + item.Quantity + "</td><td>" +
                            item.PCPL_Packing_Style_Code + "</td><td>" + item.Unit_of_Measure + "</td></tr>";
                        $('#tbInqProduct').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=6>No Records Found</td></tr>";
                    $('#tbInqProduct').append(rowData);
                }


                // loop and do whatever with data

                //

                $('#modalInquiryList').css('display', 'block');
                $('.modal-title').text('Inquiry Product');
                $('#dvInquiryProducts').css('display', 'block');
                $('#lblInqProdInqNo').text(inquiryno);
                $('#dvSendNotification').css('display', 'none');
                $('.modal-footer').css('display', 'none');

                //

                //$('.modal-title').text('Inquiry Product');
                //$('#modalIP').css('display', 'block');
                //$('#dvInquiryProducts').css('display', 'block');
            },
            error: function () {
                alert("error");
            }
        }
    );
}



function ConvertToQuote(InquiryNo) {

    window.location.href = "/SPSalesQuotes/SalesQuote?InquiryNo=" + InquiryNo;

}

function BindUserSendNotif() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.ajax(
        {
            url: '/SPInquiry/GetAllUserForSendNotif',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var rowData = "<option value='-1'>---Select---</option>";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        rowData += "<option value='" + item.Company_E_Mail + "'>" + item.First_Name + " " + item.Last_Name + "</option>";
                        
                    });
                }

                $('#ddlUserSendNotif').append(rowData);

            },
            error: function () {
                alert("error");
            }
        }
    );

}

function BindSalesperson() {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

    $.get(apiUrl + 'GetAllSalespersonForDDL', function (data) {

        if (data != null) {

            var opt = "<option value='-1'>---Select---</option>";
            for (i = 0; i < data.length - 1; i++) {
                opt += "<option value=\"" + data[i].Salespers_Purch_Code + "_" + data[i].Company_E_Mail + "\">" + data[i].First_Name + " " + data[i].Last_Name + "</option>";
            }

            $('#ddlSalesperson').append(opt);

        }

    });

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

//function CreateTask(TaskTitle) {

//    var apiUrl = $('#getServiceApiUrl').val() + 'SPInquiry/';

//    $.post(apiUrl + 'CreateNotifTask?TaskTitle=' + TaskTitle, function (data) {

//        if (data) {

//            $('#dvSendNotification').css('display', 'none');
//            $('#modalInquiryList').css('display', 'none');
//            $('#modalSentNotifMsg').css('display', 'block');
//        }

//    });

//}

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