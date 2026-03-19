/* start pagination filter code */

var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';

var filter = "";
var orderBy = 5;
var orderDir = "asc";

$(document).ready(function () {

    $('.datepicker').pickadate({
        selectMonths: true,
        selectYears: true,
        format: 'dd-mm-yyyy'
    });
    $('#txtGSTorARNNo').prop('readonly', true);

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

    $('#btnClose,.btn-close').click(function () {
        $('#modalTransporterRate').css('display', 'none');
        $('#tblRateLine').empty();
    });

    BindTransporter();
    //getGstnumber();

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

    if ($('#hfTransporterNo').val() == "") {
        SetLoaingUnloading();
    }
    else {
        $('#hfDriverflag').val(true);
    }

    $('#txtTransporterNo').change(function (event) {
        //do stuff with the "event" object as the object that called the method
        //alert(event);
        if (this.value == "") {
            $('#hfTransporterNo').val("");
            BindDrivers($('#hfTransporterNo').val(), "vendor");
            setTransporterGSTNumber($('#hfTransporterNo').val());
        }
    });

    BindDrivers($('#hfTransporterNo').val(), "vendor");
    setTransporterGSTNumber($('#hfTransporterNo').val());

});
var dtable;

function SetFreight(link) {

    var standardqty = parseFloat($(link).closest('tr').find('td').eq(9).html());
    var standardrate = parseFloat($(link).closest('tr').find('td').eq(10).html());
    var aboverate = parseFloat($(link).closest('tr').find('td').eq(11).html());
    var companyno = $(link).closest('tr').find('td').eq(2).html();
    var vendorno = $(link).closest('tr').find('td').eq(1).html();
    var vendorname = $(link).closest('tr').find('td').eq(3).html();

    var totalqty = $('#hfTotalQty').val();
    var qtydiff = totalqty - standardqty;
    var freight = 0;
    //if (qtydiff <= 0) {
    //    freight = standardrate;
    //}
    //else {
    //    var diffrate = qtydiff * aboverate;
    //    freight = standardrate + diffrate;
    //}

    if (totalqty <= standardqty) {
        freight = standardrate;
    }
    else {
        freight = totalqty * aboverate;
    }

    $('#txtFraightCharges').val(freight);
    $('#hfVendorCompanyNo').val(companyno);
    $('#hfTransporterNo').val(vendorno);
    $('#txtTransporterNo').val(vendorname);

    BindDrivers(companyno, "company");
    setTransporterGSTNumber(vendorno);
    $('#modalTransporterRate').css('display', 'none');
    return false;
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

function BindTransporter() {

    if (typeof ($.fn.autocomplete) === 'undefined') { return; }
    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';
    $.get(apiUrl + 'GetAllTransporterForDDL', function (data) {
        if (data != null) {
            var str1 = "";
            //debugger;
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
            debugger;
            $('#txtTransporterNo').autocomplete({
                lookup: pincodeArray,
                onSelect: function (selecteditem) {
                    if ($('#hfTransporterNo').val() != selecteditem.data) {
                        $('#hfTransporterNo').val((selecteditem.data));
                        //alert($('#hfTransporterNo').val());
                        BindDrivers($('#hfTransporterNo').val(), "vendor");
                        setTransporterGSTNumber($('#hfTransporterNo').val());
                    }
                },

            });
        }
    });
}

function setTransporterGSTNumber(vendorno) {
    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';
    $.get(apiUrl + 'GetTransporterDetailByVendorNo?vendorno=' + vendorno, function (data) {
        if (data != null && data.length > 0) {
            if (data[0].GST_Registration_No && data[0].GST_Registration_No.length > 0) {
                $('#txtGSTorARNNo').val(data[0].GST_Registration_No);
            } else {
                $('#txtGSTorARNNo').val(data[0].ARN_No);
            }
        }
    });
}
function CloseTask() {
    //debugger;
    var doctype, transporterCode, lrno, lrdate, drivername, driverlicenseno, drivermobileno, vehicleno, applyloadingcharges, applyunloadingcharges, applyfreightcharges, freightcharge, remarks;
    var systemIds = "";

    doctype = $('#lnlDocumentType')[0].innerText;
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
            apiUrl + 'ClosedTaskOfWarehouse?doctype=' + doctype + '&transporterCode=' + transporterCode + '&systemids=' + systemIds + '&lrno=' + lrno + '&lrdate=' + lrdate + '&drivername=' + drivername + '&driverlicenseno=' + driverlicenseno + '&drivermobileno=' + drivermobileno + '&vehicleno=' + vehicleno + '&applyloadingcharges=' + applyloadingcharges + '&applyunloadingcharges=' + applyunloadingcharges + '&applyfreightcharges=' + applyfreightcharges + '&freightcharge=' + freightcharge + '&remarks=' + remarks,

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

function BindDrivers(vendorno, numbertype) {

    if (vendorno != "" && numbertype != "") {
        $.ajax(
            {
                url: '/SPWarehouse/GetVendorDrivers?vendorNo=' + vendorno + '&numberType=' + numbertype,
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {
                    //debugger;
                    $('#tbDriver').empty();
                    var rowData = "";

                    if (data != null && data != "") {
                        $.each(data, function (index, item) {
                            var rowData = "<tr><td><a href='' onclick='return SetDriverDetail(\"" + item.Name + "\",\"" + item.Registration_Number + "\",\"" + item.Mobile_Phone_No + "\")'>Select</a></td><td>" + item.No + "</td><td>" + item.Name + "</td><td>" + item.Job_Title + "</td><td>" + item.Registration_Number + "</td><td>" + item.Mobile_Phone_No + "</td></tr>";
                            $('#tbDriver').append(rowData);
                        });
                    }
                    else {
                        rowData = "<tr><td colspan=7>No Records Found</td></tr>";
                        $('#tbDriver').append(rowData);
                    }

                },
                error: function () {
                    alert("error");
                }
            }
        );
    }
    else {
        $('#tbDriver').empty();
        $('#tbDriver').append("<tr><td colspan=7>No Records Found</td></tr>");
    }
}

function SetDriverDetail(name, licenseno, phoneno) {
    //debugger;
    //alert(link);
    //alert($(link).closest('tr').find('td').eq(2).html());
    //$('#txtDirverName').val($(link).closest('tr').find('td').eq(2).html());
    $('#txtDirverName').val(name);
    $('#txtDirverLicenseNo').val(licenseno);
    $('#txtDirverMobileNo').val(phoneno);
    $('#hfDriverflag').val(true);
    return false;

}

function SaveNewDriver(name, licenseno, phoneno, companyno) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';

    if ($('#hfDriverflag').val() == true) {

        $.post(
            apiUrl + 'SaveNewDriver?Name=' + name + '&Registration_Number=' + licenseno + '&Mobile_Phone_No=' + phoneno + '&Company_No=' + companyno + '&Type=' + "Person",
            function (data) {
                if (data) {

                    var actionMsg = "Contact Added Successfully";
                    ShowActionMsg(actionMsg);

                    location.reload(true);
                }
            }
        );
    }
}

function SetLoaingUnloading() {
    //debugger;
    var apiUrl = $('#getServiceApiUrl').val() + 'SPWarehouse/';

    var fromcode = "";
    var tocode = "";
    var doctype = "";
    var filter = "";
    var totalqty = 0;

    fromcode = $('#hfFromLocationCode').val();
    tocode = $('#hfToLocationCode').val();
    doctype = $('#lblDocumentType')[0].innerText;
    totalqty = $('#hfTotalQty').val();

    if (doctype == "Sales Order") {
        filter = "Code eq '" + fromcode + "'";
    }
    else if (doctype == "Purchase Order") {
        filter = "Code eq '" + tocode + "'";
    }
    else if (doctype == "Transfer Order" || doctype == "Sales Return") {
        filter = "(Code eq '" + fromcode + "' or Code eq '" + tocode + "')";
    }

    if (filter != "") {

        $.get(
            apiUrl + 'GetLoadingUnloadingCharge?filter=' + filter,
            function (data) {
                if (data) {

                    if (doctype == "Sales Order") {
                        var loading = parseFloat(data.PCPL_Loading_Charges_Per_Unit) * totalqty;
                        $('#txtLoadingCharges').val(loading);//data.PCPL_Loading_Charges_Per_Unit * totalqty
                        $('#txtUnloadingCharges').val("0");
                    }
                    else if (doctype == "Purchase Order") {
                        $('#txtLoadingCharges').val("0");
                        $('#txtUnloadingCharges').val(data.PCPL_Unloading_Chgs_Per_Unit * totalqty);
                    }
                    else if (doctype == "Transfer Order") {
                        $('#txtLoadingCharges').val(data.PCPL_Loading_Charges_Per_Unit * totalqty);
                        $('#txtUnloadingCharges').val(data.PCPL_Unloading_Chgs_Per_Unit * totalqty);
                    }
                }
            }
        );

    }
    else {
        alert('Kindly check From & To Location of Document');
    }

}

function SaveTransporter() {

    $('#lblMsg').html('');

    if ($('#lblAcceptedBy')[0].innerHTML == "") {
        alert('Please Accept The Task First Then You Can Map the Details...!!!');
        return false;
    }

    $('#btnSaveSpinner').show();
    var doctype, transporterCode, lrno, lrdate, drivername, driverlicenseno, drivermobileno, vehicleno, loadingcharges, unloadingcharges, transporteramount, remarks, isclosed, createDriver;
    var systemId = "";
    debugger;
    doctype = $('#lblDocumentType')[0].innerText;
    systemId = $('#hfSystemId')[0].value;
    transporterCode = $('#hfTransporterNo')[0].value;
    lrno = $('#txtLRRRNo')[0].value;
    lrdate = $('#txtLRRRDate')[0].value;
    drivername = $('#txtDirverName')[0].value;
    driverlicenseno = $('#txtDirverLicenseNo')[0].value;
    drivermobileno = $('#txtDirverMobileNo')[0].value;
    vehicleno = $('#txtVehicleNo')[0].value;
    loadingcharges = $('#txtLoadingCharges')[0].value;
    unloadingcharges = $('#txtUnloadingCharges')[0].value;
    transporteramount = $('#txtFraightCharges')[0].value;
    remarks = $('#txtRemarks')[0].value;
    isclosed = false;
    createDriver = $('#hfDriverflag')[0].value;
    vendorcompanyNo = $('#hfVendorCompanyNo')[0].value;

    if (systemId.length > 0) {
        //return;
        $.post(
            apiUrl + 'ClosedTaskOfWarehouse?doctype=' + doctype + '&transporterCode=' + transporterCode + '&systemids=' + systemId + '&lrno=' + lrno + '&lrdate=' + lrdate + '&drivername=' + drivername + '&driverlicenseno=' + driverlicenseno + '&drivermobileno=' + drivermobileno + '&vehicleno=' + vehicleno + '&loadingcharges=' + loadingcharges + '&unloadingcharges=' + unloadingcharges + '&transporteramount=' + transporteramount + '&remarks=' + remarks + '&isclosed=' + isclosed + '&selectedExisting=' + createDriver + '&vendorcompanyNo=' + vendorcompanyNo,

            function (data) {
                //alert(data);
                if (data) {

                    $('#btnSaveSpinner').hide();
                    var actionMsg = "Task Save Successfully.";
                    ShowActionMsg(actionMsg);
                    window.setTimeout(function () {

                        window.location.href = '/SPWarehouse/WarehouseList';

                    }, 2000);
                }
            }
        );
    }
    else {
        var msg = "Please Select Atleast One task to accept.";
        ShowErrMsg(msg);

    }

}

function SaveAndCloseTransporter() {

    var flag = validateForm();

    if (flag == true) {
        var doctype, transporterCode, lrno, lrdate, drivername, driverlicenseno, drivermobileno, vehicleno, loadingcharges, unloadingcharges, transporteramount, remarks, isclosed;
        var systemId = "";

        doctype = $('#lblDocumentType')[0].innerText;
        systemId = $('#hfSystemId')[0].value;
        transporterCode = $('#hfTransporterNo')[0].value;
        lrno = $('#txtLRRRNo')[0].value;
        lrdate = $('#txtLRRRDate')[0].value;
        drivername = $('#txtDirverName')[0].value;
        driverlicenseno = $('#txtDirverLicenseNo')[0].value;
        drivermobileno = $('#txtDirverMobileNo')[0].value;
        vehicleno = $('#txtVehicleNo')[0].value;
        loadingcharges = $('#txtLoadingCharges')[0].value;
        unloadingcharges = $('#txtUnloadingCharges')[0].value;
        transporteramount = $('#txtFraightCharges')[0].value;
        remarks = $('#txtRemarks')[0].value;
        isclosed = true;
        createDriver = $('#hfDriverflag')[0].value;
        vendorcompanyNo = $('#hfVendorCompanyNo')[0].value;

        $.post(
            apiUrl + 'ClosedTaskOfWarehouse?doctype=' + doctype + '&transporterCode=' + transporterCode + '&systemids=' + systemId + '&lrno=' + lrno + '&lrdate=' + lrdate + '&drivername=' + drivername + '&driverlicenseno=' + driverlicenseno + '&drivermobileno=' + drivermobileno + '&vehicleno=' + vehicleno + '&loadingcharges=' + loadingcharges + '&unloadingcharges=' + unloadingcharges + '&transporteramount=' + transporteramount + '&remarks=' + remarks + '&isclosed=' + isclosed + '&selectedExisting=' + createDriver + '&vendorcompanyNo=' + vendorcompanyNo,

            function (data) {
                //alert(data);
                if (data) {

                    var actionMsg = "Task Closed Successfully.";
                    ShowActionMsg(actionMsg);
                    window.setTimeout(function () {

                        window.location.href = '/SPWarehouse/WarehouseList';

                    }, 2000);

                }
            }
        );

    } else {
        return false;
    }
}

function validateForm() {

    $('#lblMsg').html('');


    if ($('#lblAcceptedBy')[0].innerHTML == "") {
        alert('Please Accept the Task first then you Can Map the Details....');
        return false;
    }

    if ($('#hfTransporterNo').val() == "") {
        $('#lblMsg').html('Select Transporter');
        $('#txtTransporterNo').focus();
        return false;
    }
    else if ($('#txtFraightCharges').val() == "") {
        $('#lblMsg').html('Enter Fraight Charges');
        $('#txtFraightCharges').focus();
        return false;
    }
    else if ($('#txtLoadingCharges').val() == "") {
        $('#lblMsg').html('Enter Loading Charges');
        $('#txtLoadingCharges').focus();
        return false;
    }
    else if ($('#txtUnloadingCharges').val() == "") {
        $('#lblMsg').html('Enter Unloading Charges');
        $('#txtUnloadingCharges').focus();
        return false;
    }
    else if ($('#txtLRRRNo').val() == "") {
        $('#lblMsg').html('Enter LR RR No');
        $('#txtLRRRNo').focus();
        return false;
    }
    else if ($('#txtLRRRDate').val() == "") {
        $('#lblMsg').html('Enter LR RR Date');
        $('#txtLRRRDate').focus();
        return false;
    }
    else if ($('#txtVehicleNo').val() == "") {
        $('#lblMsg').html('Enter VehicleNo');
        $('#txtVehicleNo').focus();
        return false;
    }
    else if ($('#txtDirverName').val() == "") {
        $('#lblMsg').html('Enter Dirver Name');
        $('#txtDirverName').focus();
        return false;
    }
    else if ($('#txtDirverMobileNo').val() == "") {
        $('#lblMsg').html('Enter Dirver MobileNo');
        $('#txtDirverMobileNo').focus();
        return false;
    }
    else if ($('#txtDirverLicenseNo').val() == "") {
        $('#lblMsg').html('Enter Dirver LicenseNo');
        $('#txtDirverLicenseNo').focus();
        return false;
    }
    return true;
}