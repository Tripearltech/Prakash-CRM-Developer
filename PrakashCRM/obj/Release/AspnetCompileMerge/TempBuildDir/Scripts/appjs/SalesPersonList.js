
/* start pagination filter code */
var filter = "";
var orderBy = 3;
var orderDir = "asc";
$(document).ready(function () {

    filter = "Status eq '" + $('#ddlStatus').val() + "'";

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

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

            filter += " and Status eq '" + $('#ddlStatus').val() + "'";

            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });

    $('#btnClearFilter').click(function () {

        $('#ddlField').val('-1');
        $('#ddlOperator').val('Contains');
        $('#txtSearch').val('');

        $('#ddlStatus').val("Active");
        filter = "Status eq '" + $('#ddlStatus').val() + "'";

        $('ul.pager li').remove();
        orderBy = 3;
        orderDir = "asc";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlStatus').change(function () {

        ClearCustomFilter();

        filter = "Status eq '" + $('#ddlStatus').val() + "'";

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

    $('#txtNewPassword').blur(function () {
        if ($('#txtNewPassword').val() != "") {
            var str = $('#txtNewPassword').val();
            if (str.match(/[a-z]/g) && str.match(/[A-Z]/g) && str.match(/[0-9]/g) && str.match(/[^a-zA-Z\d]/g) && isValid(str) && str.length >= 8) {
                document.getElementById('lblMsg').innerText = "";
            }
            else {
                document.getElementById('lblMsg').innerText = "Password does not match the policy guidelines.";
                $('#txtNewPassword').val('');
                $('#txtNewPassword').focus();
            }
        }
    });

    $('#txtConfirmPassword').blur(function () {
        if ($('#txtConfirmPassword').val() != "") {
            if ($('#txtNewPassword').val() != $('#txtConfirmPassword').val()) {

                document.getElementById('lblMsg').innerText = "New Password and Confirm Password didn\'t match.";
                $('#txtConfirmPassword').val('');
                $('#txtConfirmPassword').focus();
            }
            else {
                document.getElementById('lblMsg').innerText = "";
            }
        }
    });

    $('#btnUpdatePassword').click(function () {
        var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';
        if ($('#txtNewPassword').val() != "" && $('#txtConfirmPassword').val() != "" && $('#txtNewPassword').val() == $('#txtConfirmPassword').val()) {
            $('#btnUpdatePassSpinner').show();
            $.post(
                apiUrl + 'UpdatePassword?email=' + $('#hfSPEmail').val() + '&userNo=' + $('#hfSPNo').val() + '&newPassword=' + $('#txtConfirmPassword').val(),
                function (data) {
                    if (data) {
                        document.getElementById('lblMsg').innerText = "Password updated successfully.";
                        $('#txtNewPassword').val('');
                        $('#txtConfirmPassword').val('');
                        $('#btnUpdatePassword').prop('disabled', true);
                        $('#btnUpdatePassSpinner').hide();
                    }
                }
            );
        }
        else {
            var msg = "Please Enter New Password and Confirm New Password and both must be a same.";
            document.getElementById('lblMsg').innerText = msg;
            $('#txtConfirmPassword').focus();
        }
    });

    $('.btn-close').click(function () {
        $('#modalPassword').css('display', 'none');
    });
});

function isValid(str) {
    return /^(?=.*[!@$_])[a-zA-Z0-9!@$_]+$/.test(str);
}
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';

    $.get(apiUrl + 'GetApiRecordsCount?apiEndPointName=EmployeesDotNetAPI&filter=' + filter, function (data) {
        $('#hdnSPCount').val(data);
    });

    $.ajax(
        {
            url: '/SalesPerson/GetSalesPersonListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {
                    var rowData = "<tr><td></td><td><a href='/SalesPerson/SalesPersonCard?No=" + item.No + "'><i class='bx bxs-edit'></i></a></td><td align='center'><a class='ViewChangePassCls' onclick='ChangePassword(\"" + item.No + "\",\"" + item.Company_E_Mail + "\")'><i class='bx bx-key'></i></a></td>" +
                        "<td>" + item.No + "</td><td>" + item.PCPL_Employee_Code + "</td><td>" + item.First_Name + "</td><td>" + item.Last_Name + "</td><td>" + item.Company_E_Mail + "</td><td>" + item.Job_Title + "</td><td>" + item.Address + "</td>";

                    if (item.PCPL_Enable_OTP_On_Login) {
                        rowData += "<td><span class='badge bg-primary'>Yes</span></td>";
                    }
                    else {
                        rowData += "<td><span class='badge bg-secondary'>No</span></td>";
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
        $('#dataList th:gt(9)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th').slice(3, 10).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(3)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(9)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th').slice(3, 10).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnSPCount').val(); //32;// children.length;
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
            url: '/SalesPerson/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.fileName != "") {
                    //use window.location.href for redirect to download action for download the file
                    window.location.href = "/SalesPerson/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function ChangePassword(No, Email) {

    if (No != null && No != "") {

        $('#txtNewPassword').val('');
        $('#txtConfirmPassword').val('');
        document.getElementById('lblMsg').innerText = "";
        $('#btnUpdatePassword').prop('disabled', false);

        $('.modal-title').text('Update Password');
        $('#modalPassword').css('display', 'block');

        $('#hfSPNo').val(No);
        $('#hfSPEmail').val(Email);

    }

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
/* end pagination filter code */