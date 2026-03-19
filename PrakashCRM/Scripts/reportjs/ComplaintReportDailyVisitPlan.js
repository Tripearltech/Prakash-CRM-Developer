var apiUrl = $('#getServiceApiUrl').val() + 'SPReports/';
var orderBy = 2;
var orderDir = "asc";
var filter = "";
$(document).ready(function () {
    var fromdate = "";
    var todate = "";
    var search = "";
        ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
    $('#ddlRecPerPage').change(function () {
        ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
    });
});

function ComplaintReportDailyVisitPlanList(skip, top, firsload, orderBy, orderDir, fromdate, todate, search) {

    // Records count (pagination)
    $.get(
        apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() +
        '&apiEndPointName=DailyVisitsDotNetAPI&fdate=' + fromdate +
        "&tdate=" + todate +
        "&text=" + search,
        function (data) {
            $('#hdnSPSICount').val(data);
        }
    );

    $.ajax({
        url: '/SPReports/GetComplaintReportDailyVisitPlanList',
        type: 'GET',
        contentType: 'application/json',
        data: {
            Fromdate: fromdate,
            Todate: todate,
            Search: search,
            orderBy: orderBy,
            orderDir: orderDir,
            skip: skip,
            top: top
        },
        success: function (data) {

            $("#tblComplaintReportDailyVisitPlan").empty();
            var rowData = "";

            if (data && data.length > 0) {

                $.each(data, function (index, item) {

                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');

                    // ✅ Status check
                    var isApproved = item.Status === "Approve";
                    var isRejected = item.Status === "Rejected";

                    rowData += `
                        <tr data-item="${itemJson}"><td>${item.Complain_Invoice}</td><td>${item.No}</td><td>${item.Contact_Company_Name}</td><td>${item.Complain_Subject}</td><td>${item.Com_Date}</td><td>${item.Root_Analysis}</td><td>${item.Root_Analysis_date}</td><td>${item.Corrective_Action}</td><td>${item.Corrective_Action_Date}</td><td>${item.Preventive_Action}</td><td>${item.Preventive_Date}</td><td>${item.Appro_Date}</td><td>${item.Reject_Date}</td><td>${item.Suggestion}</td><td>    ${(!isApproved && !isRejected)? `<a href="#" class="btn btn-success px-2 Approve">Approve</a>`: ``}</td>
                    <td>${(!isApproved && !isRejected)? `<a href="#" class="btn btn-danger px-2 Rejected">Reject</a>`: ``}</td>
                        </tr>`;
                });

            } else {
                rowData = "<tr><td colspan='15'>No Records Found</td></tr>";
                $('ul.pager li').remove();
            }

            $("#tblComplaintReportDailyVisitPlan").append(rowData);

            if (firsload == 1) {
                pageMe();
            }
        }
    });
}


$(document).on("click", '.Approve', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const no = JSON.parse(itemJson);
    var Status = "Approve";
    const dailyvisitNo = no.No;
    const rowno = no.Entry_Type;
    var $btn = $(this);
    var $row = $btn.closest("tr");
    $.ajax({
        url: '/SPReports/GetComplaintReportDailyVisitPlanApproved',
        type: 'GET',
        contentType: 'application/json',
        data: { DailyVisitoNo: dailyvisitNo, Status: Status, RowNo: rowno },
        success: function (data) {
            $row.find('.Approve,.Rejected').remove();
        }
    });

});

$(document).on("click", '.Rejected', function () {
    const itemJson = $(this).closest("tr").attr("data-item");
    const no = JSON.parse(itemJson);
    var Status = "Rejected";
    const dailyvisitNo = no.No;
    const rowno = no.Entry_Type;
    var $btn = $(this);
    var $row = $btn.closest("tr");
    $.ajax({
        url: '/SPReports/GetComplaintReportDailyVisitPlanApproved',
        type: 'GET',
        contentType: 'application/json',
        data: { DailyVisitoNo: dailyvisitNo, Status: Status, RowNo: rowno },
        success: function (data) {
            $row.find('.Rejected,.Approve').remove();
        }
    });

});


//$('#dataList th').click(function () {
//    var table = $(this).parents('table').eq(0)

//    this.desc = !this.desc;
//    if (this.cellIndex != 0) {
//        orderBy = parseInt(this.cellIndex);
//        orderDir = "desc";

//        if (this.desc) {
//            orderDir = "desc";
//        }
//        else {
//            orderDir = "desc";
//        }
//        $('ul.pager li').remove();
//        ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir,null,null,null);
//    }
//});
$("#btnClearFilter").on('click', function () {

    ClearDispatchFilter();
});

function ClearDispatchFilter() {
    var fDate = "";
    var tDate = "";
    var txtSearch = "";
    $("#FromDate").val('');
    $("#ToDate").val('');
    $("#TxtSearch").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");

    ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, null, null, null);
}
$("#SearchBtn").on('click', function () {
    var fromdate = $("#FromDate").val();
    var todate = $("#ToDate").val();
    var search = $("#TxtSearch").val().trim();

    if (fromdate != "" || todate != "" || search != "") {
        if (fromdate != "" || todate != "") {
            if (fromdate != "" && todate == "") {

                $("#Tdatevalidate").text('please select to date');

            } else {
                $("#Tdatevalidate").text('');

            }
            if (fromdate == "" && todate != "") {
                $("#Fdatevalidate").text('Please select first date');

            }
            else {

                $("#Fdatevalidate").text('');
            }
        }


    }
    else if (fromdate != "" || todate != "" || search != "") {
        $("#Fdatevalidate").text('');
        $("#Tdatevalidate").text('');
    }
    if (fromdate != "" || todate != "" || search == "") {
        if (fromdate != "" && todate != "" && search == "") {

            if (fromdate != null) {

                $("#Fdatevalidate").text("");

            } if (todate != null) {

                $("#Tdatevalidate").text("");

            }
            ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
        }
        else if (fromdate == "" && todate == "" && search == "") {
            $("#Fdatevalidate").text("Please select first date");
            $("#Tdatevalidate").text("please select to date");
            //ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);

        }


    }
    if (fromdate == "" && todate == "" && search != "") {
        $("#Fdatevalidate").text('');
        $("#Tdatevalidate").text('');
        ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
    }
    if (fromdate != "" && todate != "" && search != "") {
        $("#Fdatevalidate").text('');
        $("#Tdatevalidate").text('');
        ComplaintReportDailyVisitPlanList(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, fromdate, todate, search);
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
    var $this = $('#tblComplaintReportDailyVisitPlan'),
        defaults = {
            perPage: 15,
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

        ComplaintReportDailyVisitPlanList(skip2, top2, 0, orderBy, orderDir,null,null,null);
    }

};