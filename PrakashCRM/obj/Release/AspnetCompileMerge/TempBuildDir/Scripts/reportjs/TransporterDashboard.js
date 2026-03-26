var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
var orderBy = 2;
var orderDir = "asc";
var filter = "";
$(document).ready(function () {
    $("#ddlRecPerPage").change(function () {
        TransporterdashboardList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
    });
    TransporterdashboardList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
});

function TransporterdashboardList(skip, top, firsload, orderBy, orderDir) {
    $.get(apiUrl + 'GetApiRecordsCounts?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=Transporterdashboard', function (data) {
        $('#hdnSPSICount').val(data);
    });
    $.ajax({
        url: '/SPReports/GetTransporterDashboardList?orderBy=' + orderBy + '&orderDir=' + orderDir + '&skip=' + skip + '&top=' + top,
        type: 'GET',
        contentType: 'application/json',
        data: {},
        success: function (data) {
            if ($.fn.dataTable.isDataTable('#dataList')) {
                $('#dataList').DataTable().destroy();
            }

            $("#tblTransporterdashboard").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                    //const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    //<td>" + item.Reject_Date + "</td><td>" + item.Suggestion + `</td><td class="Approved"><a href="#" class="btn btn-outline-success px-2">Approve</a></td><td class="Rejected" ><a href="#"  class="btn btn-outline-danger px-2 Rejected">Rejected</a></td>
                    //data - item="${itemJson}"
                    rowData += `<tr><td>` + item.Posting_Date + `</td><td>` + item.Document_No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" +
                        item.Location + "</td><td>" + item.Destination_Location + "</td><td>" + item.Transporter_Name + "</td><td>"
                        + item.Vehicle_No + "</td><td>" + item.LR_RR_No + "</td><td>" + item.Freight_Amount + "</td><td>" + item.Loading_Amount + "</td><td>" + item.Remarks + "</td></tr>"
                });

            }
            else {
                rowData = "<tr><td colspan='15' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblTransporterdashboard").append(rowData);
            if (firsload == 1) {
                pageMe();
            }
            if (data.length == 0) {
                $('ul.pager li').remove();
            }
        }
    });
}
$(document).on("click", '.Approved', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const no = JSON.parse(itemJson);
    var Status = "Approve";
    const dailyvisitNo = no.No;
    const rowno = no.Entry_Type;
    $.ajax({
        url: '/SPReports/GetComplaintReportDailyVisitPlanApproved',
        type: 'GET',
        contentType: 'application/json',
        data: { DailyVisitoNo: dailyvisitNo, Status: Status, RowNo: rowno },
        success: function (data) {
             
        }
    });

});

$(document).on("click", '.Rejected', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const no = JSON.parse(itemJson);
    var Status = "Rejected";
    const dailyvisitNo = no.No;
    const rowno = no.Entry_Type;
    $.ajax({
        url: '/SPReports/GetComplaintReportDailyVisitPlanApproved',
        type: 'GET',
        contentType: 'application/json',
        data: { DailyVisitoNo: dailyvisitNo, Status: Status, RowNo: rowno },
        success: function (data) {
           
        }
    });

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
        TransporterdashboardList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir);
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
    var $this = $('#tblTransporterdashboard'),
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

        TransporterdashboardList(skip2, top2, 0, orderBy, orderDir);
    }

};