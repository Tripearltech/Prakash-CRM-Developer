/* start pagination filter code */
var filter = "";
var orderBy = 4;
var orderDir = "desc";
var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

$(document).ready(function () {

    filter += "TPTPL_Schedule_status eq '" + $('#ddlStatus').val() + "'";

    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#hfIsScheduleOrder').val("false");

    $('#ddlField').change(function () {
        
        if ($('#ddlField').val() == "Order_Date") {
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
            filter += "TPTPL_Schedule_status eq '" + $('#ddlStatus').val() + "'";
        }

        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);

    });

    $('#btnSearch').click(function () {
        var flag = true;

        filter = "";

        if ($('#ddlField').val() == "Order_Date") {
            if ($('#txtFromDate').val() == "" || $('#txtToDate').val() == "") {
                flag = false;
            }
            else {
                filter = $('#ddlField').val() + ' ge ' + $('#txtFromDate').val() + ' and ' + $('#ddlField').val() + ' le ' + $('#txtToDate').val();
            }
        }
        else {

            //if ($('#ddlField').val() == "---Select---" || $('#ddlOperator').val() == "---Select---" || $('#txtSearch').val() == "") {
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

                        if ($('#ddlField').val() == "Amount") {
                            filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                        }
                        else if ($('#ddlField').val() == "TPTPL_Short_Close") {

                            if ($('#txtSearch').val().toLowerCase() == "yes") {
                                filter = $('#ddlField').val() + ' eq true';
                            }
                            else if ($('#txtSearch').val().toLowerCase() == "no") {
                                filter = $('#ddlField').val() + ' eq false';
                            }

                        }
                        else {
                            filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        }

                        //if ($('#ddlField').val() != "Amount") {
                        //    filter = $('#ddlField').val() + ' eq ' + '\'' + $('#txtSearch').val() + '\'';
                        //}
                        //else {
                        //    filter = $('#ddlField').val() + ' eq ' + $('#txtSearch').val();
                        //}

                        break;
                    case 'Not Equal':

                        if ($('#ddlField').val() == "Amount") {
                            filter = $('#ddlField').val() + ' ne ' + $('#txtSearch').val();
                        }
                        else if ($('#ddlField').val() == "TPTPL_Short_Close") {

                            if ($('#txtSearch').val().toLowerCase() == "yes") {
                                filter = $('#ddlField').val() + ' ne true';
                            }
                            else if ($('#txtSearch').val().toLowerCase() == "no") {
                                filter = $('#ddlField').val() + ' ne false';
                            }

                        }
                        else {
                            filter = $('#ddlField').val() + ' ne ' + '\'' + $('#txtSearch').val() + '\'';
                        }

                        //if ($('#ddlField').val() != "Amount") {
                        //    filter = $('#ddlField').val() + ' ne ' + '\'' + $('#txtSearch').val() + '\'';
                        //}
                        //else {
                        //    filter = $('#ddlField').val() + ' ne ' + $('#txtSearch').val();
                        //}

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
            filter += " and TPTPL_Schedule_status eq '" + $('#ddlStatus').val() + "'";
        }
        else if ($('#ddlStatus').val() != "-1" && filter == "") {
            filter += "TPTPL_Schedule_status eq '" + $('#ddlStatus').val() + "'";
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

        ClearCustomFilter();

        $('#ddlStatus').val("Pending");
        filter = "TPTPL_Schedule_status eq '" + $('#ddlStatus').val() + "'";
        $('ul.pager li').remove();
        orderBy = 4;
        orderDir = "desc";
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex >= 3 && this.cellIndex <= 5) {
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

    $('#btnCloseModalSQ').click(function () {

        //if ($('#hfIsScheduleOrder').val() == "true") {

        //    if ($('#tblSchOrderProds').html().trim() != "") {

        //        $('#tblSchOrderProds tr').each(function () {

        //            var row = $(this)[0];
        //            const prodDetails = row.cells[0].innerHTML.split('_');

        //            $.post(apiUrl + 'UpdateScheduleQty?QuoteNo=' + $('#hfQuoteNo').val() + '&ProdLineNo=' + prodDetails[0] +
        //                '&ScheduleQty=' + parseFloat(prodDetails[2]), function (data) {

        //            });

        //        });

        //        $('#tblSchOrderProds').empty();
        //        $('#tblSchOrderDetails').css('display', 'none');
        //        $('.modal-footer').css('display', 'none');
        //        $('#hfIsScheduleOrder').val("false");
        //        $('#txtBalQty').prop('disabled', false);
        //        $('#txtBalQty').val("");
        //    }

        //}

        $('#modalSQ').css('display', 'none');
        $('#dvSQScheOrder').css('display', 'none');
        $('#dvSQLineItems').css('display', 'none');
        $('.modal-title').text('');
        $('.modal-footer').css('display', 'none');
        ResetSchOrdrDetails();
        $('#divImage').hide();
        location.reload(true);
    });

    $('#btnAddRow').click(function () {

        var errMsg = "";

        if ($('#ddlSQProds').val() == "-1" || $('#txtScheduleQty').val() == "") {

            $('#lblSchOrderErrMsg').css('display', 'block');
            $('#lblSchOrderErrMsg').text("Please fill Product, User and Schedule Qty Details");
            
        }
        else if (parseFloat($('#txtScheduleQty').val()) <= 0) {

            $('#lblSchOrderErrMsg').css('display', 'block');
            $('#lblSchOrderErrMsg').text("Please fill Schedule Qty > 0");
            
        }
        else if (parseFloat($('#txtScheduleQty').val()) > $('#txtBalQty').val()) {

            $('#lblSchOrderErrMsg').css('display', 'block');
            $('#lblSchOrderErrMsg').text("Please fill Schedule Qty Less Than OR Equal To Balance Qty");

        }
        else {
            
            const prodDetails = $('#ddlSQProds').val().split('_');
            var firstRowDropShipmentStatus = prodDetails[3];
            var prodDropShipmentStatus = "";
            if (prodDetails[3] == "true") {
                prodDropShipmentStatus = "Yes";
            }
            else if (prodDetails[3] == "false") {
                prodDropShipmentStatus = "No";
            }
            
            $('#lblSchOrderErrMsg').text("");
            $('#lblSchOrderErrMsg').css('display', 'none');
            
            var TROpts = "<tr><td hidden>" + $('#ddlSQProds').val() + "</td><td>" + $('#ddlSQProds option:selected').text() + "</td><td>" + $('#txtBalQty').val() + "</td><td>" + $('#txtScheduleQty').val() +
                "</td><td>" + $('#txtSchRemarks').val() + "</td><td>" + prodDropShipmentStatus + "</td>";

            if ($('#tblLotNoWiseQtyDetails').html() != "") {

                var InvQtyTable = "<td style='display:none'><table id=\"" + prodDetails[1] + "_InvDetails\" style='display:none'>";
                var QtyTROpts = "";
                $('#tblLotNoWiseQtyDetails tr').each(function () {

                    var row = $(this)[0];
                    var ItemNo = row.cells[2].innerHTML;
                    var LotNo = row.cells[3].innerHTML;
                    var ReqQty = parseInt(row.cells[4].innerHTML);
                    var LocCode = row.cells[5].innerHTML;;

                    QtyTROpts += "<tr><td></td><td></td><td>" + ItemNo + "</td><td>" + LotNo + "</td><td>" + ReqQty + "</td><td>" + LocCode + "</td></tr>";

                });

                InvQtyTable += QtyTROpts + "</table>";
                TROpts += InvQtyTable + "</td>";
                $('#tblLotNoWiseQtyDetails').empty();
            }
            else {

                TROpts += "<td style='display:none'></td>";

            }

            TROpts += "</tr>";

            $('#tblSchOrderDetails').css('display', 'block');
            if ($('#tblSchOrderProds').html().trim() == "") {

                $('#tblSchOrderProds').append(TROpts);
                $('.modal-footer').css('display', 'block');

                $.post(apiUrl + 'UpdateScheduleQty?QuoteNo=' + $('#hfQuoteNo').val() + '&ProdLineNo=' + prodDetails[0] +
                    '&ScheduleQty=' + parseFloat($('#txtScheduleQty').val()), function (data) {

                });

            }
            else {

                /*if ($('#tblSchOrderProds tr:first').find("TD").eq(6).html() == prodDropShipmentStatus) {*/

                    $('#lblSchOrderErrMsg').css('display', 'none');
                    $('#tblSchOrderProds').append(TROpts);
                    $('.modal-footer').css('display', 'block');

                    $.post(apiUrl + 'UpdateScheduleQty?QuoteNo=' + $('#hfQuoteNo').val() + '&ProdLineNo=' + prodDetails[0] +
                        '&ScheduleQty=' + parseFloat($('#txtScheduleQty').val()), function (data) {

                    });
                /*}
                else {

                    $('#lblSchOrderErrMsg').css('display', 'block');
                    $('#lblSchOrderErrMsg').text("Add All Products Either With Drop Shipment OR Without Drop Shipment");
                    
                }*/

            }
            
            $('#ddlSQProds').val('-1');
            $('#txtBalQty').val("");
            $('#txtScheduleQty').val("");
            $('#txtSchRemarks').val("");
            $('#ddlUsers').val('-1');

        }

    });

    $('#ddlSQProds').change(function () {

        const SQProdDetails = $('#ddlSQProds').val().split('_');
        $('#txtBalQty').val(parseFloat(SQProdDetails[2])).prop('disabled', true);
        $('#hfProdNo').val(SQProdDetails[1]);
        var prodDropShipment = "";
        if (SQProdDetails[3] == "true") {
            prodDropShipment = "Yes";
        }
        else {
            prodDropShipment = "No";
        }

        $('#lblDropShipment').html(prodDropShipment);
        $('#hfProdLocCode').val(SQProdDetails[4]);

    });

    //$('#btnShowInvQty').click(function () {

    //    alert("Inv Qty");
    //    $('#modalSQ').css('display', 'none');
    //    $('#dvSQScheOrder').css('display', 'none');
    //    $('.modal-title').text("Lot No. Wise Qty");
    //    $('#modalInvQty').css('display', 'block');

    //    //url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + $('#hfProdNo').val() + '&LocCode=' + $('#hfProdLocCode').val(),
        
    //    $.ajax(
    //        {
    //            url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + $('#hfProdNo').val() + '&LocCode=' + $('#hfProdLocCode').val(),
    //            type: 'GET',
    //            contentType: 'application/json',
    //            success: function (data) {

    //                $('#tblInvDetails').empty();

    //                var invDetailsTR = "";

    //                for (var i = 0; i < data.length; i++) {

    //                    invDetailsTR += "<tr><td hidden>" + data[i].ItemNo + "</td><td>" + data[i].ManufactureCode + "</td><td>" + data[i].LotNo + "</td><td>" + data[i].AvailableQty + "</td><td>" + data[i].RequestedQty + "</td><td><input id='" + data[i].LotNo + "_ReqQty' value='0' type='text' width='40%' /></td>" +
    //                        "<td>" + data[i].UnitCost + "</td></tr>";

    //                }

    //                $('#tblInvDetails').append(invDetailsTR);

    //            },
    //            error: function () {
    //                //alert("error");
    //            }
    //        }
    //    );

    //});

    $('#btnAddQty').click(function () {

        var errMsg = "";
        var Cnt = 0;
        var TotalQty = 0;
        $('#tblInvDetails tr').each(function () {

            var row = $(this)[0];
            if (parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val()) <= 0) {

                Cnt += 1;
                errMsg = "Please Fill Lot No. Wise Qty";

            }
        });

        if ($('#tblInvDetails tr').length > Cnt) {

            var prodNo = $('#hfSchProdNo').val();
            //var prodDetails = $('#ddlSQProds').val();
            //const prodDetails_ = prodDetails.split('_');

            var InvQtyTable = "<td style='display:none'><table id=\"" + prodNo + "_InvDetails\" style='display:none'>";
            var QtyTROpts = "";
            $('#tblInvDetails tr').each(function () {

                var row = $(this)[0];
                if (parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val()) > 0) {

                    //var ItemNo = row.cells[0].innerHTML;
                    //var LotNo = row.cells[2].innerHTML;
                    //var ReqQty = parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val());
                    //var LocCode = prodDetails_[4];
                    //var LocCode = $('#hfLocCode').val();

                    //var TROpts = "<tr><td></td><td></td><td>" + ItemNo + "</td><td>" + LotNo + "</td><td>" + ReqQty + "</td><td>" + LocCode + "</td></tr>";

                    //

                    
                    
                    //$('#tblLotNoWiseQtyDetails tr').each(function () {

                        //var row = $(this)[0];
                        var ItemNo = row.cells[0].innerHTML;
                        var LotNo = row.cells[2].innerHTML;
                        var ReqQty = parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val());
                        var LocCode = $('#hfLocCode').val();

                        QtyTROpts += "<tr><td></td><td></td><td>" + ItemNo + "</td><td>" + LotNo + "</td><td>" + ReqQty + "</td><td>" + LocCode + "</td></tr>";

                    //});

                    TotalQty += parseInt($("#" + row.cells[2].innerHTML + "_ReqQty").val());

                    //
                }

            });

            InvQtyTable += QtyTROpts + "</table></td>";
            //TROpts += InvQtyTable + "</td>";
            var TRSQProdNo = $('#hfTRSQProdNo').val();
            $("#" + TRSQProdNo).append(InvQtyTable);

            //
            //$('#tblLotNoWiseQtyDetails').append(TROpts);

            $('#lblInvQtyAddMsg').text(TotalQty + " Quantities Added");
            $('#lblInvQtyAddMsg').css('color', 'green');
            $('#lblInvQtyAddMsg').css('display', 'block');
            $("#" + prodNo + "_ScheduleQty").val(TotalQty);
            $("#" + prodNo + "_ScheduleQty").css('readonly', true);
            //$('#txtScheduleQty').val(TotalQty);
            //$('#txtScheduleQty').css('readonly', true);
        }
        else {

            $('#lblInvQtyAddMsg').text(errMsg);
            $('#lblInvQtyAddMsg').css('color', 'red');
            $('#lblInvQtyAddMsg').css('display', 'block');

        }

    });

    $('#btnCloseInvQty').click(function () {

        $('#lblInvQtyAddMsg').css('display', 'none');
        $('#modalInvQty').css('display', 'none');
        $('#dvSQScheOrder').css('display', 'block');
        $('#modalSQ').css('display', 'block');

    });

    $('#btnScheduleOrder').click(function () {

        /*var responseMsg = "Error : The sales Line is short Closed.";

        var responseMsg = "SDOM00163";

        if (responseMsg.includes("Error : ")) {

            const errDetails = responseMsg.split(':');
            $('#resMsg').text(errDetails[1].trim());
            $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-2.png');
            $("#resOrderNo").text("");
        }
        else {

            $("#resOrderNo").text("Order No : " + responseMsg);
            $('#resMsg').text("Order Scheduled Successfully");
            $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
        }

        $('#modalSchOrderMsg').css('display', 'block');*/

        if ($('#txtScheduleDate').val() == "" || $('#txtExternalDocNo').val() == "") {

            $('#lblErrMsgDateDocNo').text("Please Fill Details");
            $('#lblErrMsgDateDocNo').css('display', 'block');

        }
        else {

            //
            
            $('#lblErrMsgDateDocNo').text("");
            $('#lblErrMsgDateDocNo').css('display', 'none');
            $('#divImage').show();
            var scheduleorder = {};
            var invQtyDetails_ = new Array();
            var Err = "";
            var Cnt = 0;

            $('#tblProdsForSchOrdr tr').each(function () {

                var prodDetails = $(this).find("TD").eq(0).html();
                const prodDetails_ = prodDetails.split('_');
                var prodNo = prodDetails_[1];
                //
                var balanceQty = $(this).find("TD").eq(4).html();
                //

                if (parseFloat($("#" + prodNo + "_ScheduleQty").val()) <= 0) {

                    Err = "Error";
                    Cnt += 1;
                }

                if (parseFloat($("#" + prodNo + "_ScheduleQty").val()) > parseFloat(balanceQty)) {

                    Err = "SchQtyGreaterErr";
                    return false;
                }

            });

            var productDetails_ = new Array();

            if ($('#tblProdsForSchOrdr tr').length > Cnt)
            {

                if (Err == "SchQtyGreaterErr") {

                    $('#lblSchOrdrMsg').text("Schedule qty should be less than or equal to balance qty");
                    $('#lblSchOrdrMsg').css('color', 'red').css('display', 'block');

                }
                else {

                    $('#lblSchOrdrMsg').text("");
                    $('#lblSchOrdrMsg').css('display', 'none');
                    $('#btnSchOrderSpinner').show();
                    scheduleorder.QuoteNo = $('#hfQuoteNo').val();
                    scheduleorder.ScheduleDate = $('#txtScheduleDate').val();
                    scheduleorder.ExternalDocNo = $('#txtExternalDocNo').val();
                    if ($('#ddlUsers').val() == "-1") {
                        scheduleorder.AssignTo = "";
                    }
                    else {
                        const userDetails = $('#ddlUsers').val().split('_');
                        scheduleorder.AssignTo = userDetails[0];
                    }
                    
                    $('#tblProdsForSchOrdr tr').each(function () {

                        var productDetails = {};
                        var prodDetails = $(this).find("TD").eq(0).html();
                        const prodDetails_ = prodDetails.split('_');
                        var prodNo = prodDetails_[1];
                        var prodLineNo = parseInt(prodDetails_[0]);

                        if ($(this).prop('id').includes("TRSQProd_") && parseFloat($("#" + prodNo + "_ScheduleQty").val()) > 0) {

                            /*if (parseFloat($("#" + prodNo + "_ScheduleQty").val()) > 0) {*/

                            //UpdateScheduleQty($('#hfQuoteNo').val(), prodLineNo, parseFloat($("#" + prodNo + "_ScheduleQty").val()));
                            //$.post(apiUrl + 'UpdateScheduleQty?QuoteNo=' + $('#hfQuoteNo').val() + '&ProdLineNo=' + prodLineNo +
                            //    '&ScheduleQty=' + parseFloat($("#" + prodNo + "_ScheduleQty").val()), function (data) {

                            //    });
                            productDetails.QuoteNo = $('#hfQuoteNo').val();
                            productDetails.ProdLineNo = parseInt(prodDetails_[0]);
                            productDetails.ScheduleQty = parseFloat($("#" + prodNo + "_ScheduleQty").val());
                            productDetails_.push(productDetails);
                            /*}*/

                        }

                        if ($("#" + prodNo + "_InvDetails").html() != "") {

                            $("#" + prodNo + "_InvDetails tr").each(function () {

                                var invQtyDetails = {};

                                invQtyDetails.QuoteNo = $('#hfQuoteNo').val();
                                invQtyDetails.LineNo = prodLineNo;
                                invQtyDetails.ItemNo = $(this).find("TD").eq(2).html();
                                invQtyDetails.LotNo = $(this).find("TD").eq(3).html();
                                invQtyDetails.Qty = $(this).find("TD").eq(4).html();
                                invQtyDetails.LocationCode = $(this).find("TD").eq(5).html();

                                invQtyDetails_.push(invQtyDetails);
                            });

                            scheduleorder.InvQuantities = invQtyDetails_;

                        }

                    });

                    scheduleorder.SchQtyProds = productDetails_;

                    //$('#tblSchOrderProds tr').each(function () {

                    //    var prodDetails = $(this).find("TD").eq(0).html();
                    //    const prodDetails_ = prodDetails.split('_');
                    //    var prodNo = prodDetails_[1];
                    //    var prodLineNo = parseInt(prodDetails_[0]);

                    //    if ($("#" + prodNo + "_InvDetails").html() != "") {

                    //        $("#" + prodNo + "_InvDetails tr").each(function () {

                    //            var invQtyDetails = {};

                    //            invQtyDetails.QuoteNo = $('#hfQuoteNo').val();
                    //            invQtyDetails.LineNo = prodLineNo;
                    //            invQtyDetails.ItemNo = $(this).find("TD").eq(2).html();
                    //            invQtyDetails.LotNo = $(this).find("TD").eq(3).html();
                    //            invQtyDetails.Qty = $(this).find("TD").eq(4).html();
                    //            invQtyDetails.LocationCode = $(this).find("TD").eq(5).html();

                    //            invQtyDetails_.push(invQtyDetails);
                    //        });

                    //        scheduleorder.InvQuantities = invQtyDetails_;

                    //    }

                    //});

                    $.ajax({
                        type: "POST",
                        url: "/SPSalesQuotes/ScheduleOrder",
                        data: JSON.stringify(scheduleorder),
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {

                            $('#btnSchOrderSpinner').hide();
                            //$('#divImage').hide();
                            var responseMsg = data;

                            if (responseMsg.includes("Error_:")) {

                                const responseMsgDetails = responseMsg.split(':');
                                $('#btnSchOrderSpinner').hide();
                                $('#dvSQScheOrder').css('display', 'none');
                                $('#modalSQ').css('display', 'none');
                                $('#modalErrMsg').css('display', 'block');
                                $('#modalErrDetails').text(responseMsgDetails[1]);

                            }
                            else if (responseMsg.includes("Error : ")) {

                                //const errDetails = responseMsg.split(':');
                                //$('#resMsg').text(errDetails[1].trim());
                                //$('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-2.png');
                                //$("#resOrderNo").text("");

                                $('#lblSchOrdrMsg').text(errDetails[1].trim());
                                $('#lblSchOrdrMsg').css('color', 'red').css('display', 'block');
                            }
                            else if (responseMsg == null) {

                                $('#lblSchOrdrMsg').text("Error");
                                $('#lblSchOrdrMsg').css('color', 'red').css('display', 'block');

                            }
                            else {

                                //$("#resOrderNo").text("Order No : " + responseMsg);
                                //$('#resMsg').text("Order Scheduled Successfully");
                                //$('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');

                                $('#lblSchOrdrMsg').text("Order No : " + responseMsg + " - Order Scheduled Successfully");
                                $('#lblSchOrdrMsg').css('color', 'green').css('display', 'block');
                            }

                            //$('#dvSQScheOrder').css('display', 'none');
                            //$('#modalSQ').css('display', 'none');
                            //$('#modalSchOrderMsg').css('display', 'block');
                            $('.modal-title').text('Schedule The Order');
                            $('#lblErrMsgDateDocNo').text("");
                            $('#lblErrMsgDateDocNo').css('display', 'none');

                            ResetSchOrdrDetails();
                            BindSQProds($('#hfQuoteNo').val());

                        }

                    });

                }
                
            }
            else {

                $('#lblSchOrdrMsg').text("Please fill schedule qty in products for schedule order");
                $('#lblSchOrdrMsg').css('color', 'red').css('display', 'block');

            }

            //

            //$.post(apiUrl + 'ScheduleOrder?QuoteNo=' + $('#hfQuoteNo').val() + '&ScheduleDate=' + $('#txtScheduleDate').val() + '&ExternalDocNo=' +
            //    $('#txtExternalDocNo').val(), function (data) {

            //    var responseMsg = data;

            //    if (responseMsg.includes("Error : ")) {

            //        const errDetails = responseMsg.split(':');
            //        $('#resMsg').text(errDetails[1].trim());
            //        $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-2.png');
            //        $("#resOrderNo").text("");
            //    }
            //    else {

            //        $("#resOrderNo").text("Order No : " + responseMsg);
            //        $('#resMsg').text("Order Scheduled Successfully");
            //        $('#resIcon').attr('src', '../Layout/assets/images/appImages/Icon-1.png');
            //    }

            //    $('#dvSQScheOrder').css('display', 'none');
            //    $('#modalSQ').css('display', 'none');
            //    $('#modalSchOrderMsg').css('display', 'block');
            //    $('#lblErrMsgDateDocNo').text("");
            //    $('#lblErrMsgDateDocNo').css('display', 'none');
   
            //});

        }

    });

    $('#btnCloseSchOrderMsg').click(function () {

        $('#modalSchOrderMsg').css('display', 'none');
        location.reload(true);

    });

    $('#btnCloseModalErrMsg').click(function () {

        $('#modalErrDetails').text("");
        $('#modalErrMsg').css('display', 'none');

    });

    $('#btnCloseQtyDetails').click(function () {

        $('#dvSQProd').css('display', 'none');
        $('#dvOrderedQtyDetails').css('display', 'none');
        $('#dvInvoicedQtyDetails').css('display', 'none');
        $('#modalQtyDetails').css('display', 'none');
        $('#tblSQProduct').empty();
        $('#tblOrderedQtyDetails').empty();
        $('#tblInvoicedQtyDetails').empty();
        $('#tblInProcessQtyDetails').empty();

    });

    $('#btnCloseEmailSent').click(function () {
        $('#modalEmailSent').css('display', 'none');
    });

    $('#btnCloseModalShortclose').click(function () {

        $('#modalShortclose').css('display', 'none');
        $('#lblShortcloseErrMsg').css('display', 'none');
        $('#lblShortcloseErrMsg').text("");
        $('#hfShortcloseType').val("");

    });

    $('#ddlShortcloseReason').change(function () {

        if ($('#ddlShortcloseReason option:selected').text() == $('#hfSCRemarksSetupValue').val()) {

            $('#dvShortcloseRemarks').css('display', 'block');
            $('#hfShortcloseWithRemarks').val("true");

        }
        else {

            $('#txtShortcloseRemarks').val("");
            $('#dvShortcloseRemarks').css('display', 'none');
            $('#hfShortcloseWithRemarks').val("false");
        }

    });

    $('#btnShortclose').click(function () {

        if ($('#hfShortcloseWithRemarks').val() == "true" && $('#txtShortcloseRemarks').val() == "") {
            $('#lblShortcloseErrMsg').css('display', 'block');
            $('#lblShortcloseErrMsg').text("Please Fill Shortclose Remarks");
        }
        else {

            $('#lblShortcloseErrMsg').css('display', 'none');
            $('#lblShortcloseErrMsg').text("");
            $('#btnShortcloseSpinner').show();
            //SQNo = UrlVars["SQNo"];
            if ($('#hfShortcloseType').val() == "SalesQuote") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuote&SQNo=' + $('#hfSQNo').val() + '&SQProdLineNo=-1&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }
            else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                apiUrl += 'SalesQuoteShortclose?Type=SalesQuoteProd&SQNo=' + $('#hfSQNo').val() + '&SQProdLineNo=' + $('#hfSQProdLineNoForShortclose').val() + '&ShortcloseReason=' + $('#ddlShortcloseReason option:selected').text() + '&ShortcloseRemarks=' + $('#txtShortcloseRemarks').val();
            }

            $.post(apiUrl, function (data) {

                $('#btnShortcloseSpinner').hide();
                $('#modalShortclose').css('display', 'none');
                $('#lblShortcloseErrMsg').text("");
                $('#hfSQProdLineNoForShortclose').val("");

                if (data == "") {

                    $('#modalShortcloseMsg').css('display', 'block');

                    if ($('#hfShortcloseType').val() == "SalesQuote") {
                        $('#lblSQShortclose').text("Sales quote shortclose successfully");
                    }
                    else if ($('#hfShortcloseType').val() == "SalesQuoteProd") {
                        $('#lblSQShortclose').text("Sales quote product shortclose successfully");
                    }

                }
                else {

                    $('#modalErrMsg').css('display', 'block');
                    $('#modalErrDetails').text(data);

                }

            });

        }

    });

    $('#btnCloseShortcloseMsg').click(function () {

        $('#lblSQShortclose').text("");
        $('#modalShortcloseMsg').css('display', 'none');
        location.reload(true);

    });

});
var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    var SPCode = "";
    if ($('#hdnSPCodesOfReportingPersonUser').val() == "" || $('#hdnSPCodesOfReportingPersonUser').val() == null) {
        SPCode = $('#hdnLoggedInUserSPCode').val();
    }
    else {
        SPCode = $('#hdnLoggedInUserSPCode').val() + "," + $('#hdnSPCodesOfReportingPersonUser').val();
    }

    $.get(apiUrl + 'GetApiRecordsCount?Page=SQList&LoggedInUserNo=\'\'&UserRoleORReportingPerson=' + $('#hdnLoggedInUserRole').val() + '&SPCode=' + SPCode + '&apiEndPointName=SalesQuoteDotNetAPI&filter=' + filter, function (data) {
        $('#hdnSPSQCount').val(data);
    });

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesQuoteListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if ($.fn.dataTable.isDataTable('#dataList')) {
                    $('#dataList').DataTable().destroy();
                }
                $('#tableBody').empty();
                $.each(data, function (index, item) {

                    /*"<td><a href='#'><i class='bx bx-message'></i></a></td><td><a href='#'><i class='bx bx-mail-send'></i></a></td><td><a href='#'><i class='bx bx-printer'></i></a></td>"*/
                    var rowData = "<tr><td></td><td><a class='EditCls' onclick='EditSalesQuote(\"" + item.No + "\",\"" + item.TPTPL_Schedule_status + "\",\"" + item.PCPL_Status + "\",SQFor=\"Edit\",LoggedInUserRole=\"" + $('#hdnLoggedInUserRole').val() + "\")'><i class='bx bxs-edit'></i></a></td>" +
                        "<td><div class='dropdown ms-auto'><div class='cursor-pointer text-dark font-24 dropdown-toggle dropdown-toggle-nocaret' data-bs-toggle='dropdown'><i class='bx bx-dots-horizontal-rounded text-option'></i></div>" +
                        "<div class='dropdown-menu dropdown-menu-end'><a class='dropdown-item' href='javaScript:;'>Send SMS</a><a class='dropdown-item' onclick='SendEmail(\"" + item.No + "\",\"" + item.SellToEmail + "\")'>Send Email</a>" +
                        "<a class='dropdown-item' style='cursor:pointer' onclick='PrintQuote(\"" + item.No + "\")'>Print</a></div></div></td>" + "<td>" + item.No + "</td><td>" + item.Order_Date + "</td><td>" + item.Sell_to_Customer_Name + "</td>" +
                        "<td>";
                        
                    if (item.TPTPL_Schedule_status == "Pending") {

                        if (item.TPTPL_Short_Close) {
                            rowData += "<a title='Salesquote shortclosed, so can not schedule order'><span class='badge bg-danger'>" + item.TPTPL_Schedule_status + "</span></a>";
                        }
                        else {

                            if (item.PCPL_Status == "Approved") {
                                rowData += "<a class='ScheduleOrderCls' onclick='ScheduleOrder(\"" + item.No + "\",\"" + item.Location_Code + "\",\"" + item.PCPL_Status + "\")' title='Click here to schedule order'><span class='badge bg-danger'>" + item.TPTPL_Schedule_status + "</span></a>";
                            }
                            else {
                                rowData += "<a class='ScheduleOrderCls' onclick='ScheduleOrder(\"" + item.No + "\",\"" + item.Location_Code + "\",\"" + item.PCPL_Status + "\")' title='Approval is pending'><span class='badge bg-danger'>" + item.TPTPL_Schedule_status + "</span></a>";
                            }

                        }

                    }
                    else if (item.TPTPL_Schedule_status == "Partial") {

                        if (item.TPTPL_Short_Close) {
                            rowData += "<a title='Salesquote shortclose, so can not schedule order'><span class='badge bg-info text-dark'>" + item.TPTPL_Schedule_status + "</span></a>";
                        }
                        else {
                            rowData += "<a class='ScheduleOrderCls' onclick='ScheduleOrder(\"" + item.No + "\",\"" + item.Location_Code + "\")' title='Click here to schedule order'><span class='badge bg-info text-dark'>" + item.TPTPL_Schedule_status + "</span></a>";
                        }
                        
                    }
                    else if (item.TPTPL_Schedule_status == "Completed") {
                        rowData += "<span class='badge bg-success' title='Schedule completed'>" + item.TPTPL_Schedule_status + "</span>";
                    }

                    /*<td><a class='ScheduleOrderCls' onclick='ScheduleOrder(\"" + item.No + "\")'><span class='badge bg-primary'>Schedule</span></a></td>*/

                    rowData += "</td><td><a class='QtyCountCls' onclick='ShowSQProduct(\"" + item.No + "\")'>" + item.PCPL_Total_Qty + "</a></td>" +
                        "<td><a class='QtyCountCls' onclick='ShowOrderedQtyDetails(\"" + item.No + "\"," + item.TPTPLTotal_Ordered_Qty + ")'>" + item.TPTPLTotal_Ordered_Qty + "</a></td>" +
                        "<td><a class='QtyCountCls' onclick='ShowInvoicedQtyDetails(\"" + item.No + "\"," + item.TPTPLTotal_Invoiced_Qty + ")'>" + item.TPTPLTotal_Invoiced_Qty + "</a></td>";

                        var InProcessQty = item.TPTPLTotal_Ordered_Qty - item.TPTPLTotal_Invoiced_Qty;
                    rowData += "<td><a class='QtyCountCls' onclick='ShowInProcessQtyDetails(\"" + item.No + "\"," + InProcessQty + ")'>" + InProcessQty + "</a></td>";

                    if (item.PCPL_Status == "Approval pending from finance") {
                        rowData += "<td><span class='badge bg-primary'>Pending-Finance</span></td>";
                    }
                    else if (item.PCPL_Status == "Approval pending from HOD") {

                        rowData += "<td><span class='badge bg-primary'>Pending-HOD</span></td>";
                    }
                    else if (item.PCPL_Status == "Approved") {

                        rowData += "<td><span class='badge bg-success'>Approved</span></td>";
                    }
                    else if (item.PCPL_Status == "Rejected by finance") {

                        rowData += "<td><span class='badge bg-danger'>Rejected-Finance</span></td>";
                    }
                    else if (item.PCPL_Status == "Rejected by HOD") {

                        rowData += "<td><span class='badge bg-danger'>Rejected-HOD</span></td>";
                    }
   
                    if (item.TPTPL_Short_Close) {
                        rowData += "<td><span class='badge bg-primary'>Yes</span></td>";
                    }
                    else {
                        
                        if (item.TPTPL_Schedule_status == "Completed") {
                            rowData += "<td><span class='badge bg-secondary' title='schedule completed can\'t shortclose'>No</span></td>";
                        }
                        else {
                            rowData += "<td><a style='cursor:pointer' onclick='ShortcloseReason(\"" + item.No + "\",\"SalesQuote\",\"" + item.TPTPL_SC_Reason_Setup_Value + "\")'><span class='badge bg-secondary'>No</span></a></td>";
                        }
                    }

                    rowData += "<td>" + item.Payment_Terms_Code + "</td>";


                    
                    if (item.PCPL_Rejected_Reason != "") {
                        rowData += "<td>" + item.PCPL_Rejected_Reason + "</td>";
                    }
                    else if (item.PCPL_Rejected_Reason_HOD != "") {
                        rowData += "<td>" + item.PCPL_Rejected_Reason_HOD + "</td>";
                    }
                    else {
                        rowData += "<td></td>";
                    }

                    //if (item.PCPL_Rejected_Reason == null || item.PCPL_Rejected_Reason == "") {
                    //    rowData += "<td></td>";
                    //}
                    //else {
                    //    rowData += "<td>" + item.PCPL_Rejected_Reason + "</td>";
                    //}
    
                    rowData += "</tr>";

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

function commaSeparateNumber(val) {
    while (/(\d+)(\d{3})/.test(val.toString())) {
        val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
    }
    return val;
}

//function UpdateScheduleQty(QuoteNo, ProdLineNo, ScheduleQty) {

//    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

//    $.ajax({
//        type: "POST",
//        url: apiUrl + "UpdateScheduleQty?QuoteNo=" + QuoteNo + "&ProdLineNo=" + ProdLineNo +
//            "&ScheduleQty=" + ScheduleQty,
//        contentType: "application/json; charset=utf-8",
//        success: function (data) {

            

//        }

//    });

//}

function ScheduleOrder(QuoteNo, LocCode, SQStatus) {

    if (SQStatus == "Approval pending from finance" || SQStatus == "Approval pending from HOD") {

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text("Sales Quote Approval is Pending, Can\'t Schedule Order");

    }
    else if (SQStatus == "Rejected by finance" || SQStatus == "Rejected by HOD") {

        $('#modalErrMsg').css('display', 'block');
        $('#modalErrDetails').text("Sales Quote Rejected, Can\'t Schedule Order");

    }
    else {

        $('#modalErrDetails').text("");
        $('#modalErrMsg').css('display', 'none');

        $('#hfQuoteNo').val(QuoteNo);
        $('#hfIsScheduleOrder').val("true");
        $('#hfLocCode').val(LocCode);
        $('#modalSQ').css('display', 'block');
        $('#dvSQScheOrder').css('display', 'block');
        $('.modal-title').text('Schedule The Order');
        $('#lblSchOrderQuoteNo').text(QuoteNo);
        $('#tblSchOrderProds').empty();

        BindSQProds(QuoteNo);
        BindUsers();
        $('#lblBalQty').text("0.00");

    }

}

function BindSQProds(QuoteNo) {

    $.get(apiUrl + 'GetAllSQLinesOfSQ?QuoteNo=' + QuoteNo + '&SQLinesFor=ScheduleOrder', function (data) {

        if (data != null) {

            //$('#ddlSQProds').empty();

            //var opt = "<option value='-1'>---Select---</option>";
            var prodsTR = "";

            $('#tblProdsForSchOrdr').empty();

            for (i = 0; i < data.length; i++) {

                if (data[i].Outstanding_Quantity > 0) {

                    //opt += "<option value=\"" + data[i].Line_No + "_" + data[i].No + "_" + data[i].Outstanding_Quantity + "_" + data[i].Drop_Shipment + "_" + data[i].Location_Code + "\">" + data[i].Description + "</option>";
                    prodsTR += "<tr id=\"TRSQProd_" + data[i].No + "\"><td hidden>" + data[i].Line_No + "_" + data[i].No + "</td><td>" + data[i].Description + "</td>";

                    if (data[i].Drop_Shipment == true) {
                        prodsTR += "<td>Yes</td>";
                    }
                    else {
                        prodsTR += "<td>No</td>";
                    }

                    prodsTR += "<td><button type='button' id='btnShowInvQty' onclick='ShowInvQty(\"" + data[i].No + "\",\"" + $('#hfLocCode').val() + "\",\"TRSQProd_" + data[i].No + "\")' class='btn btn-primary bx bx-show-alt' title='Quantity From Inventory'></button></td><td>" + data[i].Outstanding_Quantity +
                        "</td><td><input type='text' id=\"" + data[i].No + "_ScheduleQty" + "\" value='0' class='form-control'></td><td><input type='text' id=\"" + data[i].No + "_Remarks" + "\" class='form-control'></td></tr>";

                }
                
            }

            $('#tblProdsForSchOrdr').append(prodsTR);
            /*$('#ddlSQProds').append(opt);*/

        }

    });

}

function BindUsers() {

    $.get(apiUrl + 'GetAllUsers', function (data) {

        if (data != null) {

            $('#ddlUsers').empty();

            var opt = "<option value='-1'>---Select---</option>";
            for (i = 0; i < data.length; i++) {
                opt += "<option value=\"" + data[i].No + "_" + data[i].Company_E_Mail + "\">" + data[i].First_Name + " " + data[i].Last_Name + "</option>";
            }
            $('#ddlUsers').append(opt);

        }
    });

}

function ShowSQProduct(SQNo) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesLineItems?DocumentNo=' + SQNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#lblQtyDetailsTitle').text("Sales Quote Products");
                $('#lblSQProdSQNo').text(SQNo);
                $('#tbSQProduct').empty();
                var rowData = "";

                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        rowData = "<tr><td>" + item.No + "</td><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.PCPL_Packing_Style_Code + "</td><td>" +
                            item.Unit_of_Measure_Code + "</td><td>" + item.PCPL_MRP + "</td><td>" + item.Unit_Price + "</td>";
                            
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
                    $('#tbSQProduct').append(rowData);
                }

                $('#modalQtyDetails').css('display', 'block');
                $('#dvSQProd').css('display', 'block');
                $('#dvOrderedQtyDetails').css('display', 'none');
                $('#dvInvoicedQtyDetails').css('display', 'none');
                $('#dvInProcessQtyDetails').css('display', 'none');
                
            },
            error: function () {
                alert("error");
            }
        }
    );
}

function ShowInvQty(ProdNo, LocCode, TRSQProdNo) {

    $('#hfTRSQProdNo').val(TRSQProdNo);
    $('#hfSchProdNo').val(ProdNo);
    $('#modalSQ').css('display', 'none');
    $('#dvSQScheOrder').css('display', 'none');
    $('#InvQtyDetailsTitle').text("Lot No. Wise Qty");
    $('#modalInvQty').css('display', 'block');
    

    //url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + $('#hfProdNo').val() + '&LocCode=' + $('#hfProdLocCode').val(),

    $.ajax(
        {
            url: '/SPSalesQuotes/GetInventoryDetails?ProdNo=' + ProdNo + '&LocCode=' + LocCode,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tblInvDetails').empty();

                var invDetailsTR = "";

                for (var i = 0; i < data.length; i++) {

                    invDetailsTR += "<tr><td hidden>" + data[i].ItemNo + "</td><td>" + data[i].ManufactureCode + "</td><td>" + data[i].LotNo + "</td><td>" + data[i].AvailableQty + "</td><td>" + data[i].RequestedQty + "</td><td><input id='" + data[i].LotNo + "_ReqQty' value='0' type='text' width='40%' /></td>" +
                        "<td>" + data[i].UnitCost + "</td></tr>";

                }

                $('#tblInvDetails').append(invDetailsTR);

            },
            error: function () {
                //alert("error");
            }
        }
    );

}

function ShowOrderedQtyDetails(SQNo, QtyCount) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $('#lblQtyDetailsTitle').text("Ordered Qty Details");
    $('#lblSQProdSQNo').text(SQNo);

    if (QtyCount > 0) {

        $.ajax(
            {
                url: '/SPSalesQuotes/GetOrderedQtyDetails?SQNo=' + SQNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#tblOrderedQtyDetails').empty();
                    var rowData = "";

                    if (data != null && data != "") {

                        const OrderDate = data.OrderDate.split('-');
                        var OrderDate_ = OrderDate[2] + "-" + OrderDate[1] + "-" + OrderDate[0];

                        const ScheduledDate = data.ScheduledDate.split('-');
                        var ScheduledDate_ = ScheduledDate[2] + "-" + ScheduledDate[1] + "-" + ScheduledDate[0];

                        rowData = "<tr><td>" + data.orderNo + "</td><td>" + OrderDate_ + "</td><td>" + data.Quantity + "</td><td>" + data.ScheduleQty + "</td>" +
                            "<td>" + ScheduledDate_ + "</td><td>" + data.PCPL_Remarks + "</td><td>" + data.TPTPL_Assign_to + "</td></tr>";

                        $('#tblOrderedQtyDetails').append(rowData);
                    }
                    else {
                        rowData = "<tr><td colspan=7>No Records Found</td></tr>";
                        $('#tblOrderedQtyDetails').append(rowData);
                    }

                },
                error: function () {
                    alert("error");
                }
            }
        );

    }
    else {

        var rowData = "<tr><td colspan=7>No Records Found</td></tr>";
        $('#tblOrderedQtyDetails').append(rowData);

    }

    $('#modalQtyDetails').css('display', 'block');
    $('#dvSQProd').css('display', 'none');
    $('#dvOrderedQtyDetails').css('display', 'block');
    $('#dvInvoicedQtyDetails').css('display', 'none');
    $('#dvInProcessQtyDetails').css('display', 'none');
}

function ShowInvoicedQtyDetails(SQNo, QtyCount) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $('#lblQtyDetailsTitle').text("Invoiced Qty Details");
    $('#lblSQProdSQNo').text(SQNo);

    if (QtyCount > 0)
    {

        $.ajax(
            {
                url: '/SPSalesQuotes/GetInvoicedQtyDetails?SQNo=' + SQNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#tblInvoicedQtyDetails').empty();
                    var rowData = "";

                    if (data != null && data != "") {

                        $.each(data, function (index, item) {

                            rowData = "<tr><td>" + item.InvoiceNo + "</td><td>" + item.Posting_Date + "</td><td>" + item.Quantity + "</td><td>" + item.Vehicle_No_ + "</td>" +
                                "<td>" + item.PCPL_Dirver_Mobile_No_ + "</td><td>" + item.LR_RR_No_ + "</td></tr>";

                            $('#tblInvoicedQtyDetails').append(rowData);

                        });

                    }
                    else {
                        rowData = "<tr><td colspan=6>No Records Found</td></tr>";
                        $('#tblInvoicedQtyDetails').append(rowData);
                    }

                },
                error: function () {
                    alert("error");
                }
            }
        );

    }
    else {

        var rowData = "<tr><td colspan=6>No Records Found</td></tr>";
        $('#tblInvoicedQtyDetails').append(rowData);

    }
    $('#modalQtyDetails').css('display', 'block');
    $('#dvSQProd').css('display', 'none');
    $('#dvOrderedQtyDetails').css('display', 'none');
    $('#dvInvoicedQtyDetails').css('display', 'block');
    $('#dvInProcessQtyDetails').css('display', 'none');

}

function ShowInProcessQtyDetails(SQNo, QtyCount) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $('#lblQtyDetailsTitle').text("In Process Qty Details");
    $('#lblSQProdSQNo').text(SQNo);

    if (QtyCount > 0)
    {

        $.ajax(
            {
                url: '/SPSalesQuotes/GetInProcessQtyDetails?SQNo=' + SQNo,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#tblInProcessQtyDetails').empty();
                    var rowData = "";

                    if (data != null && data != "") {

                        $.each(data, function (index, item) {

                            rowData = "<tr><td>" + item.No + "</td><td>" + item.PCPL_Transporter_Name + "</td><td>" + item.LR_RR_No + "</td><td>" + item.Vehicle_No + "</td><td>" + item.PCPL_Driver_Mobile_No + "</td>" +
                                "<td>" + item.PCPL_Remarks + "</td></tr>";

                            $('#tblInProcessQtyDetails').append(rowData);

                        });

                    }
                    else {
                        rowData = "<tr><td colspan=5>No Records Found</td></tr>";
                        $('#tblInProcessQtyDetails').append(rowData);
                    }

                },
                error: function () {
                    alert("error");
                }
            }
        );
    }
    else {

        var rowData = "<tr><td colspan=5>No Records Found</td></tr>";
        $('#tblInProcessQtyDetails').append(rowData);

    }
    $('#modalQtyDetails').css('display', 'block');
    $('#dvSQProd').css('display', 'none');
    $('#dvOrderedQtyDetails').css('display', 'none');
    $('#dvInvoicedQtyDetails').css('display', 'none');
    $('#dvInProcessQtyDetails').css('display', 'block');

}

function ResetSchOrdrDetails() {

    $('#txtScheduleDate').val("");
    $('#txtExternalDocNo').val("");
    $('#ddlUsers').val('-1');
    $('#tblProdsForSchOrdr').empty();

}

function showLineItems(itemNo) {

    $('#modalSQ').css('display', 'block');
    $('#dvSQLineItems').css('display', 'block');
    $('.modal-title').text(itemNo + ' - Sales Quote Line Items');
    

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesLineItems?DocumentNo=' + itemNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#tbodyLItems').empty();
                $.each(data, function (index, item) {
                    var rowData = "<tr><td>" + item.Description + "</td><td>" + item.Quantity + "</td><td>" + item.Line_Amount + "</td></tr>";
                    $('#tbodyLItems').append(rowData);
                    
                });

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
        $('#dataList th:gt(5)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th').slice(3,6).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(3)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(5)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th').slice(3,6).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

function exportGridData(skip, top, firsload, orderBy, orderDir, filter) {
    $.ajax(
        {
            url: '/SPSalesQuotes/ExportListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                
                if (data.fileName != "") {
                    
                    window.location.href = "/SPSalesQuotes/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );
}

function ShortcloseReason(SQNo, ShortcloseType, SCRemarksSetupValue) {

    $('#modalShortclose').css('display', 'block');
    $('#ShortcloseTitle').text("Shortclose");
    $('#hfShortcloseType').val(ShortcloseType);
    $('#hfSQNo').val(SQNo);
    $('#hfSCRemarksSetupValue').val(SCRemarksSetupValue);
    BindShortcloseReason();
    
}

function SendEmail(SQNo, CustEmail) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
    $('#divImage').show();
    $.post(apiUrl + 'SalesQuoteSendEmail?custEmail=' + CustEmail + '&SPEmail=' + $('#hdnSPEmail').val() + '&SQNo=' + SQNo, function (data) {

        if (data) {
            $('#divImage').hide();
            $('#modalEmailSent').css('display', 'block');
                
        }

    });

}

//function PrintPreviewSQ(SQNo) {

//    var apiUrl = $('#getServiceApiUrl').val() + 'SPSalesQuotes/';
//    $.post('/SPSalesQuotes/PrintPreviewSQ?SQNo=' + SQNo, function (data) {


//    });

//}

function PrintQuote(SQNo) {

    //var quoteNo = 'POD000002';

    $('#divImage').show();

    $.ajax({
        type: "POST",
        url: "/SPSalesQuotes/PrintQuote?QuoteNo=" + SQNo,
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (data) {
            //debugger;
            //alert("Success: " + data);
            // Get the base URL (e.g., https://www.example.com/)
            let baseUrl = window.location.origin;

            // File name or relative path (e.g., "Files/MyDocument.pdf")
            let filePath = "SalesQuotePrint/" + data;

            // Build the full URL
            let fullUrl = baseUrl + "/" + filePath;

            $('#divImage').hide();
            // Open in a new tab or window
            window.open(fullUrl, '_blank');
        },
        error: function (xhr, status, error) {
            debugger;

            let message = "An unexpected error occurred.";

            // Try to get server-side message (if any)
            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            } else if (xhr.responseText) {
                message = xhr.responseText;
            } else if (error) {
                message = error;
            }

            alert("Error: " + message);
        }
    });
}

function BindShortcloseReason() {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetShortcloseReasons',
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                var opts = "<option value='-1'>---Select---</option>";

                $.each(data, function (index, item) {
                    opts += "<option value='" + item.Entry_No + "'>" + item.Short_Close_Reason + "</option>";
                });

                $('#ddlShortcloseReason').append(opts);

            },
            error: function () {
                alert("error");
            }
        }
    );

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