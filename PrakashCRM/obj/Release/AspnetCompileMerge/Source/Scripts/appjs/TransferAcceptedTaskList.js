/* start pagination filter code */

var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouseTransfer/';

var filter = "";
var orderBy = 5;
var orderDir = "asc";

$(document).ready(function () {

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#ddlField').change(function () {

        if ($('#ddlField').val() == "Shipment_Date") {
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

        //
        var flag = true;

        if ($('#ddlField').val() == "Shipment_Date") {
            if ($('#txtFromDate').val() == "" || $('#txtToDate').val() == "") {
                flag = false;
            }
            else {
                filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
            }
        }
        else {
            if ($('#ddlField').val() == "---Select---" || $('#ddlOperator').val() == "---Select---" || $('#txtSearch').val() == "") {
                flag = false;
            }
            else {
                switch ($('#ddlOperator').val()) {
                    case 'Equal':
                        if ($('#ddlField').val() != "Quantity") {
                            filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        }
                        else {
                            filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                        }
                        break;
                    case 'Not Equal':
                        if ($('#ddlField').val() != "Quantity") {
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
        }

        if (flag == false) {

            var errMsg = "Please Fill All Filter Details";
            ShowErrMsg(errMsg);

        }
        else {
            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }

    });

    $('#btnExport').click(function () {

        if ($('#ddlField').val() == "Shipment_Date") {
            filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
        }
        else {
            switch ($('#ddlOperator').val()) {
                case 'Equal':
                    if ($('#ddlField').val() != "Quantity") {
                        filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                    }
                    else {
                        filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                    }
                    break;
                case 'Not Equal':
                    if ($('#ddlField').val() != "Quantity") {
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

        rebindGrid();
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

    $('#chkAll').click(function () {

        $('#tableBody input[type=checkbox]').prop('checked', this.checked);

    });

    $('.btn-close').click(function () {
        $('#modalTransferLineList').css('display', 'none');
        $('#modalTransporter').css('display', 'none');
    });

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    $.get(apiUrl + 'GetAcceptedApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&apiEndPointName=TransferOrderDotNetAPI&filter=' + filter, function (data) {
        $('#hdnWSATaskCount').val(data);
    });

    $.ajax(
        {
            url: '/SPWarehouseTransfer/GetWarehouseAcceptedTaskListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();

                $.each(data, function (index, item) {
                    var rowData = "<tr><td></td><td><input type='checkbox' class='form-check-input' value='" + item.SystemId + "' /></td><td>" + item.No + "</td><td>" + item.Transfer_from_Code + "</td><td>" + item.Transfer_from_Name + "</td><td>" + item.Transfer_to_Code + "</td><td>" + item.Transfer_to_Name + "</td><td>" + item.Shipment_Date + "</td><td><a class='ViewInqProdCls' onclick='ShowTransferLines(\"" + item.No + "\")'><i class='bx bx-show'></i></a></td></tr>";// 
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

function ShowTransferLines(Document_No) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouseTransfer/';

    $.ajax(
        {
            url: '/SPWarehouseTransfer/GetAllwarehouseTransferLineForPopup?Document_No=' + Document_No,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tbTransferLines').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        var rowData = "<tr><td>" + item.Item_No + "</td><td>" + item.Description + "</td><td>" + item.Qty_to_Ship + "</td><td>" + item.PCPL_Packing_UOM + "</td><td>" + item.PCPL_Packing_Style_Description + "</td></tr>";
                        $('#tbTransferLines').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=8>No Records Found</td></tr>";
                    $('#tbTransferLines').append(rowData);
                }
                
                $('#modalTransferLineList').css('display', 'block');
                $('.modal-title').text('Transfer Line');
                $('#dvTransferLines').css('display', 'block');

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
        $('#dataList th:first-child').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(0)').removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:first-child').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(0)').removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnWSATaskCount').val(); //32;// children.length;
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

/* end pagination filter code */

function exportGridData(skip, top, firsload, orderBy, orderDir, filter) {
    $.ajax(
        {
            url: '/SPWarehouseTransfer/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                //alert(data);
                //debugger;
                if (data.fileName != "") {
                    //use window.location.href for redirect to download action for download the file
                    window.location.href = "/SPWarehouseTransfer/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
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

function MapTransporter() {

    var checkboxes = $('input[type=checkbox]');
    var checked = checkboxes.filter(':checked');

    if (checked.length <= 0) {

        var msg = "Please Select Atleast One task to accept.";
        ShowErrMsg(msg);
    }
    else {

        $('#modalTransporter').css('display', 'block');
        $('.modal-title').text('Transporter Details');
        $('#dvTransporter').css('display', 'block');

        BindTransporter();
    }
}

function rebindGrid() {
    $('#ddlField').val('---Select---');
    $('#ddlOperator').val('---Select---');
    $('#txtSearch').val('');
    $('#txtFromDate').val('');
    $('#txtToDate').val('');
    $('#ddlOperator').css('display', 'block');
    $('#dvtxtSearch').css('display', 'block');
    $('#txtFromDate').css('display', 'none');
    $('#txtToDate').css('display', 'none');

    filter = "";
    $('ul.pager li').remove();
    orderBy = 5;
    orderDir = "asc";
    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
}

$('#chkApplyFreightCharges').change(function () {
    debugger;
    if (this.checked) {
        document.getElementById('txtFraightCharges').removeAttribute("disabled");
    }
    else {
        document.getElementById('txtFraightCharges').setAttribute("disabled", "disabled");
    }
    $('#txtFraightCharges').val('0');
});

function BindTransporter() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    console.log('init_autocomplete');

    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouseTransfer/';
    $.get(apiUrl + 'GetAllTransporterForDDL', function (data) {
        if (data != null) {
            var str1 = "";
            debugger;
            var i;
            for (i = 0; i <= data.length - 1; i++) {
                str1 = str1 + '"' + data[i].No + '"' + ":" + '"' + data[i].Name + '"' + ","
            }
            str1 = str1.substring(0, str1.length - 1);
            str1 = "{" + str1 + "}";

            let objFromStr = JSON.parse(str1);

            var pincode = objFromStr;// { AA: "Abc", AO: "Abc", BB: "Bcd", CC: "Cde", DD: "Def", EE: "Efg", EH: "Ester", FF: "Fgh", GG: "Ghi", HH: "Hij", II: "Ijk", JJ: "Jkl", JO: "Jim", KK: "Klm", LL: "Lmn", LT: "Lina", MH: "Marty", OF: "Otis", RB: "Robin" };

            var pincodeArray = $.map(pincode, function (value, key) {
                return {
                    value: value,
                    data: key
                };
            });

            $('#txtTransporterNo').autocomplete({
                lookup: pincodeArray,
                onSelect: function (selecteditem) {

                    $('#hfTransporterNo').val((selecteditem.data));
                    //alert($('#hfTransporterNo').val());
                }
            });
        }
    });
}

function CloseTask() {
    debugger;
    var transporterCode, lrno, lrdate, drivername, driverlicenseno, drivermobileno, vehicleno, applyloadingcharges, applyunloadingcharges, applyfreightcharges, freightcharge, remarks;
    var systemIds = "";

    transporterCode = $('#hfTransporterNo')[0].value;
    lrno = $('#txtLRRRNo')[0].value;
    lrdate = $('#txtLRRRDate')[0].value;
    drivername = $('#txtDirverName')[0].value;
    driverlicenseno = $('#txtDirverLicenseNo')[0].value;
    drivermobileno = $('#txtDirverMobileNo')[0].value;
    vehicleno = $('#txtVehicleNo')[0].value;
    applyloadingcharges = $('#chkApplyLoadingCharges')[0].checked;
    applyunloadingcharges = $('#chkApplyUnloadingCharges')[0].checked;
    applyfreightcharges = $('#chkApplyFreightCharges')[0].checked;
    freightcharge = $('#txtFraightCharges')[0].value;
    remarks = $('#txtRemarks')[0].value;
    $('#tableBody input[type=checkbox]:checked').each(function () {

        systemIds += $(this).val() + "|";
    });
    systemIds = systemIds.slice(0, -1);

    if (systemIds.length > 0) {
        //return;
        $.post(
            apiUrl + 'ClosedTaskOfWarehouse?transporterCode=' + transporterCode + '&systemids=' + systemIds + '&lrno=' + lrno + '&lrdate=' + lrdate + '&drivername=' + drivername + '&driverlicenseno=' + driverlicenseno + '&drivermobileno=' + drivermobileno + '&vehicleno=' + vehicleno + '&applyloadingcharges=' + applyloadingcharges + '&applyunloadingcharges=' + applyunloadingcharges + '&applyfreightcharges=' + applyfreightcharges + '&freightcharge=' + freightcharge + '&remarks=' + remarks,

            function (data) {
                //alert(data);
                if (data) {

                    var actionMsg = "Task Closed Successfully.";
                    ShowActionMsg(actionMsg);
                    rebindGrid();
                }
            }
        );
    }
    else {
        var msg = "Please Select Atleast One task to accept.";
        ShowErrMsg(msg);

    }
}
