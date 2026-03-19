var filter = "";
var orderBy = 2;
var orderDir = "asc";
$(document).ready(function () {

    BindFromCode();

    filter = "IsActive eq " + JSON.parse($('#ddlStatus').val());

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlFromCode').change(function () {

        if ($('#ddlFromCode').val() == "All") {
            filter = "IsActive eq " + JSON.parse($('#ddlStatus').val());
        }
        else {
            filter = "From_Code eq \'" + $('#ddlFromCode').val() + "\' and IsActive eq " + JSON.parse($('#ddlStatus').val());
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlStatus').change(function () {

        if ($('#ddlFromCode').val() == "All") {
            filter = "IsActive eq " + JSON.parse($('#ddlStatus').val());
        }
        else {
            filter = "From_Code eq \'" + $('#ddlFromCode').val() + "\' and IsActive eq " + JSON.parse($('#ddlStatus').val());
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    });

    $('#btnExport').click(function () {

        if ($('#ddlFromCode').val() == "All") {
            filter = "";
        }
        else {
            filter = "From_Code eq \'" + $('#ddlFromCode').val() + "\'";
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);
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
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });


});

function BindFromCode() {

    $.ajax(
        {
            url: '/SPNotification/GetAllSPNoCodeForDDL',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.length > 0) {

                    var FromCodeOpt = "<option value='All'>All</option>";

                    $.each(data, function (i, data) {
                        FromCodeOpt += "<option value=\"" + data.PCPL_Employee_Code + "\">" + data.PCPL_Employee_Code + "</option>";
                    });

                    $('#ddlFromCode').append(FromCodeOpt);


                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );
}


var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPNotification/';

    $.get(apiUrl + 'GetApiRecordsCount?filter=' + filter + '&apiEndPointName=NotificationsListDotNetAPI', function (data) {
        $('#hdnNotifSetupCount').val(data);
    });

    $.ajax(
        {
            url: '/SPNotification/GetNotificationSetupListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {
                    var rowData = "<tr><td></td><td><a href='/SPNotification/Notification?Type=" + item.Type + "&Employee_No=" + item.Employee_No + "'><i class='bx bxs-edit'></i></a></td><td>" + item.Type + "</td><td>" + item.From_Code + "</td><td>" + item.To_E_mail + "</td><td>" + item.CC_E_mail + "</td><td>" + item.BCC_E_mail + "</td>";

                    rowData = rowData + "</tr>";

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
        $('#dataList th:lt(2)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(1)').removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(2)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(1)').removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnNotifSetupCount').val(); //32;
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

function exportGridData(skip, top, firsload, orderBy, orderDir, filter) {
    $.ajax(
        {
            url: '/SPNotification/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.fileName != "") {
                    
                    window.location.href = "/SPNotification/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                //alert(data1);
            }
        }
    );
}