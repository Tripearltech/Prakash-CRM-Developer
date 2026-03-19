var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
var orderBy = 2;
var orderDir = "asc";
var filter = "";

$(document).ready(function () {
    $('#ddlRecPerPage').change(function () {
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
    });
    ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
});

function ComplainList(skip, top, firsload, orderBy, orderDir) {
    debugger
    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=DailyVisitsDotNetAPI&fdate=null&tdate=null&text=null', function (data) {
        $('#hdnSPSICount').val(data);
    });
    $.ajax({
        url: '/SPReports/GetComplaintList?orderBy=' + orderBy + '&orderDir=' + orderDir + '&skip=' + skip + '&top=' + top,
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            if ($.fn.dataTable.isDataTable('#dataList')) {
                $('#dataList').DataTable().destroy();
            }
            $("#tblComplaint").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                   
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
        ComplainList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
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

        ComplainList(skip2, top2, 0, orderBy, orderDir);
    }

};