/* start pagination filter code */

var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';

var filter = "";
var orderBy = 5;
var orderDir = "asc";

$(document).ready(function () {

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

    $('#btnTransporterRateCard').click(function () {

        var transporterno = $('#hfTransporterNo').val();
        if ($('#txtTransporterNo').val() == "") {
            transporterno = "";
        }

        $.ajax(
            {
                url: '/SPWarehouse/GetTransporterRate?FromPincode=' + $('#hfFromPincode').val() + '&ToPincode=' + $('#hfToPincode').val() + '&PackingUOMs=' + $('#hfPackingUOMs').val() + '&TransporterNo=' + transporterno,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    $('#tblRateLine').empty();
                    var rateLine = "";
                    $.each(data, function (index, item) {
                        rateLine = rateLine + "<tr><td><a href='' onclick='return SetFreight(this)'>Select</a></td><td style='display: none;'>" + item.Transporter_No + "</td><td style='display: none;'>" + item.Contact_No + "</td><td>" + item.Transporter_Name + "</td><td>" + item.From_Post_Code + "</td><td>" + item.To_Post_Code + "</td><td>" + item.From_Area + "</td><td>" + item.To_Area + "</td><td>" + item.UOM + "</td><td>" + item.Standard_Weight + "</td><td>" + item.Rate_for_Standard_Weight + "</td><td>" + item.Rate_above_Standard_Weight + "</td><td>" + item.Rate_Effective_Date + "</td><td>" + item.Vehicle_Type + "</td></tr>";
                    });

                    $('#tblRateLine').append(rateLine);

                    $('#modalTransporterRate').css('display', 'block');
                    $('.modal-title').text('Transporter Rate Card');

                },
                error: function () {
                    //alert("error");
                }
            }
        );
    });

    BindDrivers($('#hfTransporterNo').val(), "vendor");

});

var dtable;
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