var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';

/* start pagination filter code */
var filter = "";
var orderBy = 4;
var orderDir = "asc";

$(document).ready(function () {
    bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    $('#ddlField').val("Name");
    $('#ddlRecPerPage').change(function () {
        bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
    });

    $('#btnSearch').click(function () {

        if ($('#ddlField').val() == "-1" || $('#ddlOperator').val() == "-1" || $('#txtSearch').val().trim() == "") {

            ShowErrMsg("Please Fill All Filter Details");

        } else {

            var searchText = $('#txtSearch').val().trim();
            var selectedField = $('#ddlField').val();

            switch ($('#ddlOperator').val()) {

                case 'Equal':
                    filter = selectedField + " eq '" + searchText + "'";
                    break;

                case 'Not Equal':
                    filter = selectedField + " ne '" + searchText + "'";
                    break;

                case 'Starts With':
                    filter = "startswith(" + selectedField + ", '" + searchText + "')";
                    break;

                case 'Ends With':
                    filter = "endswith(" + selectedField + ", '" + searchText + "')";
                    break;

                case 'Contains':
                    filter = "contains(" + selectedField + ", '" + searchText + "')";
                    break;

                default:
                    filter = "";
                    break;
            }

            $('ul.pager li').remove();
            bindGridData(0, $('#ddlRecPerPage').val(), 1, orderBy, orderDir, filter);
        }
    });

    $('#btnClearFilter').click(function () {

        $('#ddlField').val('-1');
        $('#ddlOperator').val('Contains');
        $('#txtSearch').val('');

        filter = "";
        $('ul.pager li').remove();
        orderBy = 4;
        orderDir = "asc";
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
            default:
                filter = "";
                break;
        }

        exportGridData(0, 0, 0, orderBy, orderDir, filter);

    });

    $('#dataList th').click(function () {
        var table = $(this).parents('table').eq(0)

        this.asc = !this.asc;
        if (this.cellIndex >= 4 && this.cellIndex <= 13) {
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

    $('#btnItemSpec').click(function () {
        showPopup('ItemSpecification');
    });

    $('#btnMSDS').click(function () {
        showPopup('MSDS');
    });

    $('#btnCOA').click(function () {
        showPopup('COA');
    });

    $('#btnSendItemSpecMSDSCOA').click(function () {

        if (!$('#chkEmail,#chkWhatsApp').is(':checked')) {
            $('#lblOptErr').css('display', 'block');
        }
        else if ($('#ddlItems').val() == "-1") {
            $('#lblProdErr').css('display', 'block');
        }
        else {
            //var flag = confirm('Are you sure you want to send?');
            //if (flag) {
            $('#lblProdErr').css('display', 'none');
            var isEmail = false, isWhatsApp = false, ToEmail = "", ToWhatsApp = "", subject = "", isCOA = false, selectedLocation = "";
            var availStock = 0;
            //var selectedOpt = "";
            //var emailOrWhatsApp = "";
            if ($('#chkEmail').is(':checked')) {
                isEmail = true;
                ToEmail = $('#txtItemSpecMSDSCOAEmail').val();
                //selectedOpt = "Email";
                //emailOrWhatsApp = $('#txtItemSpecMSDSCOAEmail').val();
            }

            if ($('#chkWhatsApp').is(':checked')) {
                isWhatsApp = true;
                ToWhatsApp = $('#txtItemSpecMSDSCOAMobile').val();
            }

            //else {
            //    selectedOpt = "WhatsApp";
            //    emailOrWhatsApp = $('#txtItemSpecMSDSCOAMobile').val();
            //}


            if ($('#hdnItemSpecMSDSCOA').val() == "COA") {
                isCOA = true;
                selectedLocation = $('#ddlLocations option:selected').text();
                availStock = $('#lblCOAProdTotalStock').text();
            }

            var selectedProd = $('#ddlItems option:selected').text();

            var BodyText = $('.ck-content > p').text();
            var path = $('#lblItemSpecMSDSCOAPath').text();

            subject = $('#hdnModalTitle').val();

            $.post(
                apiUrl + 'SendItemSpecMSDSCOA?isEmail=' + isEmail + '&ToEmail=' + ToEmail + '&isWhatsApp=' + isWhatsApp + '&ToWhatsApp=' + ToWhatsApp + '&Subject=' + subject + '&BodyText=' + BodyText + '&SelectedProd=' + selectedProd + '&isCOA=' + isCOA + '&SelectedLocation=' + selectedLocation + '&availStock=' + availStock + '&Path=' + path,
                function (data) {
                    if (data) {

                        //var actionMsg = subject + ' Successfully';
                        //ShowActionMsg(actionMsg);
                        $('#modalItemSpecMSDSCOA').css('display', 'none');
                        $('#modalMSDSItemSpecMsg').css('display', 'block');

                    }
                }
            );
            /*}*/
        }
    });

    $('#btnCloseMSDSItemSpecMsg').click(function () {

        $('#modalMSDSItemSpecMsg').css('display', 'none');

    });


    $('#btnSendMsg').click(function () {

        var checkboxes = $('input[type=checkbox]');
        var checked = checkboxes.filter(':checked');

        if (checked.length <= 0) {

            var msg = "Please Select Contact";
            ShowErrMsg(msg);

        }
        else {

            $('#modalSendMsg').css('display', 'block');

            $('#chkSMS1').attr('checked', true).change();

            var ItemSpecMobile = "";

            $('#tableBody input[type=checkbox]:checked').each(function () {

                var row = $(this).closest("tr")[0];
                ItemSpecMobile += row.cells[7].innerHTML + ",";

            });

            $('#modalSendMsg .modal-title').text('SMS/WhatsApp');
            ItemSpecMobile = ItemSpecMobile.slice(0, -1);
            $('#txtMsgMobile').val(ItemSpecMobile);
        }
    });

    $(document).on('click', '.js-view-contact', function (e) {
        e.preventDefault();
        var companyNo = $(this).attr('data-company-no') || '';
        var companyName = $(this).attr('data-company-name') || '';
        ViewContactPerson(companyNo, companyName);
    });

    $('#chkEmail').change(function () {
        if ($(this).is(':checked')) {
            $('#lblOptErr').css('display', 'none');
            $('#dvItemSpecMSDSCOAEmail').css('display', 'block');
        }
        else {
            $('#dvItemSpecMSDSCOAEmail').css('display', 'none');
        }
    });

    $('#chkWhatsApp').change(function () {
        if ($(this).is(':checked')) {
            $('#lblOptErr').css('display', 'none');
            $('#dvItemSpecMSDSCOAMobile').css('display', 'block');
        }
        else {
            $('#dvItemSpecMSDSCOAMobile').css('display', 'none');
        }
    });

    $('.btn-close').click(function () {
        //$('.popupChk').each(function (i) {
        //    $(this).attr('checked', false).change();
        //});

        //$('#chkEmail').attr('checked', false).change();
        //$('#chkWhatsApp').attr('checked', false).change();
        //$('#chkSMS1').attr('checked', false);
        //$('#chkWhatsApp1').attr('checked', false);
        $('#ddlItems option').remove();
        $('#modalItemSpecMSDSCOA').css('display', 'none');
        $('#modalSendMsg').css('display', 'none');
        $('#modalContacts').css('display', 'none');
    });

    $('#ddlItems').on('change', function () {
        //$('#lblItemSpecMSDSCOAPath').text('D:/ProductFiles/' + $('#ddlItems').val() + '.pdf');
        if ($('#hdnModalTitle').val() == "COA Document") {
            $('#dvCOADetails').css('display', 'block');

            //$.ajax(
            //    {
            //        url: '/SPContacts/GetQtyForCountStockFromILE?LocationCode=' + $('#ddlLocations').val() + '&ItemNo=' + $('#ddlItems').val(),
            //        type: 'GET',
            //        contentType: 'application/json',
            //        success: function (data) {
            //            $('#lblCOAProdTotalStock').text(data);
            //        },
            //        error: function () {
            //            //alert("error");
            //        }
            //    }
            //);

            $.get(apiUrl + 'GetBatchWiseQty?ProdNo=' + $('#ddlItems').val() + '&LocCode=' + $('#ddlLocations').val(), function (data) {

                if (data.length == 1 && data[0].LotNo == "") {

                    $('#lblCOAProdAvailableStock').text(data[0].AvailableQty);
                    $('#lblCOAProdAvailableStock').css('display', 'block');
                    $('#dvCOAProdAvailableStock').css('display', 'none');
                }
                else if (data.length > 0) {

                    $('#lblCOAProdAvailableStock').css('display', 'none');
                    $('#dvCOAProdAvailableStock').css('display', 'block');
                    var TRProdQty = "";
                    for (var a = 0; a < data.length; a++) {

                        TRProdQty += "<tr><td><input type='checkbox' class='form-check-input' id=\"chk_" + data[a].LotNo + "\"></td><td>" + data[a].LotNo + "</td><td>" + data[a].AvailableQty + "</td></tr>";
                    }

                    $('#tblBatchWiseQtyDetails').append(TRProdQty);

                }

            });

            //$.ajax(
            //    {
            //        url: apiUrl + 'GetBatchWiseQty?ProdNo=' + $('#ddlItems').val() + '&LocCode=' + $('#ddlLocations').val(),
            //        type: 'GET',
            //        contentType: 'application/json',
            //        success: function (data) {
            //            //$('#lblCOAProdAvailableStock').text(data);

            //            if (data.length > 0) {

            //                $('#dvCOAProdAvailableStock').css('display', 'block');
            //                var TRProdQty = "";
            //                for (var a = 0; a < data.length; a++) {

            //                    TRProdQty += "<tr><td>" + data[a].LotNo + "</td><td>" + data[a].AvailableQty + "</td></tr>";
            //                }

            //                $('#tblBatchWiseQtyDetails').append(TRProdQty);

            //            }

            //        },
            //        error: function () {
            //            //alert("error");
            //        }
            //    }
            //);

            $('#lblItemSpecMSDSCOAPath').text('D:/TestFile-COADocument.pdf');
        }
        else {
            $('#lblItemSpecMSDSCOAPath').text('D:/TestFile.pdf');
        }

    });

    $('#chkSMS1').change(function () {
        if ($(this).is(':checked')) {
            $('#lblOptErr1').css('display', 'none');
        }
    });

    $('#chkWhatsApp1').change(function () {
        if ($(this).is(':checked')) {
            $('#lblOptErr1').css('display', 'none');
        }
    });

    $('#btnModalSendMsg').click(function () {
        if (!$('#chkSMS1,#chkWhatsApp1').is(':checked')) {
            $('#lblOptErr1').css('display', 'block');
        }
        else {
            var flag = confirm('Are you sure you want to save?');
            alert(flag);
        }
    });

    $('#chkAll').click(function () {

        $('#tableBody input[type=checkbox]').prop('checked', this.checked);

    });

    //$('#tableBody input[type=checkbox]').click(function () {

    //    if ($(this).is(":checked")) {
    //        var isAllChecked = 0;

    //        $('#tableBody input[type=checkbox]').each(function () {
    //            if (!$('#tableBody input[type=checkbox]').is(':checked'))
    //                isAllChecked = 1;
    //        });

    //        if (isAllChecked == 0) {
    //            $('#chkAll').prop('checked', true);
    //        }
    //    }
    //    else {
    //        $('#chkAll').prop('checked', false);
    //    }

    //});

    //$('#tableBody input[type=checkbox]').each(function (checkbox) {

    //    checkbox.checked = this.checked;

    //    checkbox.addEventListener('change', function () {

    //        if (this.checked == false) {
    //            $('#chkAll').prop('checked', false);
    //        }

    //        if (document.querySelectorAll('#tableBody input[type=checkbox]:checked').length == $('#tableBody input[type=checkbox]').length) {
    //            $('#chkAll').prop('checked', true);
    //        }
    //    });

    //});

    $('#btnFeedBack').click(function () {
        var checkboxes = $('input[type=checkbox]');
        var checked = checkboxes.filter(':checked');

        if (checked.length <= 0) {

            var msg = "Please Select Contact";
            ShowErrMsg(msg);

        }
        else {
            var FeedbackEmail = "";
            //var ContactPersonEmail = "";
            var FeedbackMobile = "";
            var ContactName = "";
            var ContactAddress = "";
            var ContactNo = "";
            let FeedbackEmailToCC = new Array();
            var EmailFlag = true;
            var i = 0;

            $('#tableBody input[type=checkbox]:checked').each(function () {

                var row = $(this).closest("tr")[0];
                /*FeedbackMobile += row.cells[7].innerHTML + ",";*/
                FeedbackMobile += row.cells[12].innerHTML + ",";

                if (row.cells[9].innerHTML != "") {

                    if (row.cells[5].innerHTML != "") {

                        if (row.cells[13].innerHTML != "") {
                            //FeedbackEmail += row.cells[8].innerHTML + "," + row.cells[12].innerHTML + ",";
                            /*FeedbackEmailToCC[i] = row.cells[8].innerHTML + "," + row.cells[12].innerHTML + "," + $('#getLoggedInSPEmail').val();*/
                            FeedbackEmailToCC[i] = row.cells[13].innerHTML + "," + $('#getLoggedInSPEmail').val();
                        }
                        else {
                            //FeedbackEmail += row.cells[8].innerHTML + ",";
                            /*FeedbackEmailToCC[i] = row.cells[8].innerHTML + ",''," + $('#getLoggedInSPEmail').val();*/
                            FeedbackEmailToCC[i] = row.cells[13].innerHTML + ",''," + $('#getLoggedInSPEmail').val();
                        }
                        i += 1;

                        //if (row.cells[11].innerHTML != "") {
                        //    ContactName += row.cells[4].innerHTML + "," + row.cells[11].innerHTML + ",";
                        //}
                        //else {
                        //    ContactName += row.cells[4].innerHTML + ",";
                        //}

                        ContactName += row.cells[5].innerHTML + ",";
                        /*if (row.cells[10].innerHTML == "" || row.cells[10].innerHTML == null) {
                            ContactAddress += "" + ";";
                        }
                        else {
                            ContactAddress += row.cells[10].innerHTML + ";";
                        }*/
                        if (row.cells[9].innerHTML == "" || row.cells[9].innerHTML == null) {
                            ContactAddress += "" + ";";
                        }
                        else {
                            ContactAddress += row.cells[9].innerHTML + ";";
                        }

                        ContactNo += row.cells[4].innerHTML + ";";
                    }
                }
                else {
                    EmailFlag = false;
                }

            });

            FeedbackMobile = FeedbackMobile.slice(0, -1);
            if (!EmailFlag) {

                var msg = "Email ID not available in selected contact";
                ShowErrMsg(msg);

            }
            else {
                //FeedbackEmail = FeedbackEmail.slice(0, -1);
                //const FeedbackEmails = FeedbackEmail.split(',');
                const ContactNames = ContactName.split(',');
                const ContactAddresses = ContactAddress.split(';');
                const ContactNos = ContactNo.split(';');
                const FeedbackMobiles = FeedbackMobile.split(',');

                if (!EmailFlag) {

                    var msg = "Please select contact only which have Email ID";
                    ShowErrMsg(msg);

                }
                else {

                    var flag;

                    for (var a = 0; a < FeedbackEmailToCC.length; a++) {
                        $.post(
                            apiUrl + 'SendFeedbackFormLink?contactCompanyNo=' + ContactNos[a] + '&ToCCEmail=' + FeedbackEmailToCC[a] + '&contactName=' + ContactNames[a] + '&contactMobileNo=' + FeedbackMobiles[a] +
                            '&contactAddress=' + ContactAddresses[a] + '&custVendorPortalUrl=' + custVendorPortalUrl + '&SPNo=' + $('#hfSPNo').val(),
                            function (data) {
                                if (data) {
                                    flag = true;
                                }
                                else {
                                    flag = false;
                                }
                            }
                        );
                    }

                    //if (flag) {

                    //var actionMsg = "Feedback Form Link Sent Successfully";
                    //ShowActionMsg(actionMsg);

                    $('#modalFeedbackFormMsg').css('display', 'block');


                    //}
                }
            }
        }
    });

    $('#btnCloseFeedbackFormMsg').click(function () {

        $('#modalFeedbackFormMsg').css('display', 'none');
        location.reload(true);

    });

});

function showPopup(popuptype) {
    var checkboxes = $('input[type=checkbox]');
    var checked = checkboxes.filter(':checked');

    if (checked.length <= 0) {

        var msg = "Please Select Contact";
        ShowErrMsg(msg);

    }
    else {

        $('#modalItemSpecMSDSCOA').css('display', 'block');
        if (popuptype == "ItemSpecification") {
            $('#modalItemSpecMSDSCOA .modal-title').text('Item Specification Sheet');
            $('#hdnModalTitle').val('Item Specification Sheet');
            $('#dvCOALocations').css('display', 'none');
            $('#hdnItemSpecMSDSCOA').val('ItemSpecification');
        }
        if (popuptype == "MSDS") {
            $('#modalItemSpecMSDSCOA .modal-title').text('MSDS Sheet');
            $('#hdnModalTitle').val('Item Specification - MSDS Sheet');
            $('#dvCOALocations').css('display', 'none');
            $('#hdnItemSpecMSDSCOA').val('MSDS');
        }
        if (popuptype == "COA") {
            $('#modalItemSpecMSDSCOA .modal-title').text('COA Document');
            $('#hdnModalTitle').val('COA Document');
            $('#dvCOALocations').css('display', 'block');
            $('#hdnItemSpecMSDSCOA').val('COA');
        }

        $('#chkEmail').attr('checked', true).change();

        var ItemSpecEmail = "";
        var ItemSpecMobile = "";

        $('#tableBody input[type=checkbox]:checked').each(function () {

            var row = $(this).closest("tr")[0];
            //ItemSpecMobile += row.cells[7].innerHTML + ",";
            //ItemSpecEmail += row.cells[8].innerHTML + ",";
            ItemSpecMobile += row.cells[15].innerHTML + ",";
            ItemSpecEmail += row.cells[13].innerHTML + ",";

        });

        ItemSpecMobile = ItemSpecMobile.slice(0, -1);
        ItemSpecEmail = ItemSpecEmail.slice(0, -1);

        $('#txtItemSpecMSDSCOAMobile').val(ItemSpecMobile);
        $('#txtItemSpecMSDSCOAEmail').val(ItemSpecEmail);

        $.ajax(
            {
                url: '/SPLocations/GetLocationsListData',
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#ddlLocations option').remove();
                    var locationsOpts = "<option>---Select---</option>";
                    $.each(data, function (index, item) {
                        locationsOpts += "<option value='" + item.Code + "'>" + item.Name + "</option>";
                    });

                    $('#ddlLocations').append(locationsOpts);
                },
                error: function () {
                    //alert("error");
                }
            }
        );

        $.ajax(
            {
                url: '/SPItems/GetItemsListData',
                type: 'GET',
                contentType: 'application/json',
                success: function (data) {

                    $('#ddlItems option').remove();
                    var itemsOptions = "<option value='-1'>---Select---</option>";
                    $.each(data, function (index, item) {
                        itemsOptions += "<option value='" + item.No + "'>" + item.Description + "</option>";
                    });

                    $('#ddlItems').append(itemsOptions);
                },
                error: function () {
                    alert("error");
                }
            }
        );
    }
}

var dtable;
function bindGridData(skip, top, firsload, orderBy, orderDir, filter) {

    showPageDataLoader();

    $.get(apiUrl + 'GetApiRecordsCount?SPCode=' + $('#hdnLoggedInUserSPCode').val() +
        '&apiEndPointName=ContactDotNetAPI&type=Company&filter=' + filter, function (data) {
            $('#hdnCCompanyCount').val(data);
        });

    $.ajax({
        url: '/SPContacts/GetContactCompanyListData?orderBy=' + orderBy +
            '&orderDir=' + orderDir +
            '&filter=' + filter +
            '&skip=' + skip +
            '&top=' + top,
        type: 'GET',
        contentType: 'application/json',

        success: function (data) {

            if ($.fn.dataTable.isDataTable('#dataList')) {
                $('#dataList').DataTable().destroy();
            }

            $('#tableBody').empty();

            var selectedField = $('#ddlField').val();
            var searchText = $('#txtSearch').val().toLowerCase();

            $.each(data, function (index, item) {

                // 🔥 CLIENT SIDE DYNAMIC FILTER
                if (filter && searchText) {

                    var fieldValue = item[selectedField] || "";
                    fieldValue = fieldValue.toString().toLowerCase();

                    if (!fieldValue.includes(searchText)) {
                        return;
                    }
                }

                var rowData = `<tr>
<td></td>
<td><input type='checkbox' class='form-check-input' /></td>
<td><a href='/SPContacts/CompanyContactCard?No=${item.No}'><i class='bx bxs-edit'></i></a></td>
<td></td>
<td align='center'>
    <a class='js-view-contact' data-company-no='${item.No}' data-company-name='${item.Name}'>
        <i class='bx bx-show'></i>
    </a>
</td>

<td>${item.No}</td>
<td>${item.Name}</td>
<td>${item.Industry || ''}</td>
<td>${item.Source_of_Contact || ''}</td>
<td>${item.Business_Type || ''}</td>
<td>${item.City || ''}</td>
<td>${item.Area || ''}</td>
<td>${item.Post_Code || ''}</td>
<td>${item.Phone_No || ''}</td>
<td>${item.E_Mail || ''}</td>
<td>${item.PCPL_Primary_Contact_Name || ''}</td>
<td>${item.Mobile_Phone_No || 'N/A'}</td>
<td>${item.Salesperson_Code || ''}</td>
<td>${item.Credit_Limit || ''}</td>
<td>${item.GST_Registration_No || ''}</td>
<td>${item.P_A_N_No || ''}</td>
<td>${item.PCPL_Feedback_Status || ''}</td>

</tr>`;
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
        complete: function () {
            hidePageDataLoader();
        },
        error: function () {
            alert("error");
        }
    });
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
        $('#dataList th:lt(4)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th:gt(13)').removeClass("sorting_asc").removeClass("sorting_disabled");
        $('#dataList th').slice(4, 14).removeClass("sorting_asc").removeClass("sorting_desc").removeClass("sorting_disabled").addClass("sorting");
        $("#dataList th:nth-child(" + (orderBy + 1) + ")").removeClass("sorting").removeClass("sorting_desc").addClass("sorting_asc");
    }
    if (orderDir == "desc") {
        $('#dataList th:lt(4)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th:gt(13)').removeClass("sorting_desc").removeClass("sorting_disabled");
        $('#dataList th').slice(4, 14).removeClass("sorting_desc").removeClass("sorting_asc").removeClass("sorting_disabled").addClass("sorting");
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

    var numItems = $('#hdnCCompanyCount').val(); //32;
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
            url: '/SPContacts/ExportCompanyListData?orderBy=' + orderBy + '&orderDir=' + orderDir + '&filter=' + filter + '&skip=' + skip + '&top=' + top,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                if (data.fileName != "") {

                    window.location.href = "/SPContacts/Download?file=" + data.fileName;
                }
            },
            error: function (data1) {
                alert(data1);
            }
        }
    );

}

function ViewContactPerson(itemNo, itemName) {

    $.ajax(
        {
            url: '/SPContacts/GetAllContactsOfCompanyForPopup?No=' + itemNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {
                $('#tbContacts').empty();
                var rowData = "";
                if (data != null && data != "") {
                    $.each(data, function (index, item) {
                        rowData = rowData + "<tr><td>" + item.Name + "</td><td>" + item.Mobile_Phone_No + "</td><td>" + item.E_Mail + "</td><td>" + item.PCPL_Department_Name + "</td><td>" + item.PCPL_Job_Responsibility + "</td></tr>";
                    });
                }
                else {
                    rowData = "<tr><td colspan=3>No Records Found</td></tr>";
                }

                $('#tbContacts').append(rowData);

                $('#modalContacts .modal-title').text(getSafeText(itemName) + '\'s Contact Person');
                $('#modalContacts').css('display', 'block');
                $('#dvContacts').css('display', 'block');
            },
            error: function () {
                alert("error");
            }
        }
    );
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

//sales quotes

function SalesQuotes(item) {
    currentCompanyName = getSafeText(item && item.Name);
    $('#quotesModal .modal-title').text(getCurrentCompanyLabel() + ' - Sales Quotes');
    var no = item.No;
    BindContactSQ("", "", no);
    $("#CompanyNo").val(no);
}

function BindContactSQ(FromDate, ToDate, No) {

    var apiUrl = $('#getServiceApiUrl').val() + 'SPContacts/';
    if (No == "") {
        No = $("#CompanyNo").val();
    }
    $('#btnSQListSpinner').show();
    $.get(apiUrl + 'GetContactSalesQuotes?CCompanyNo=' + No + '&FromDate=' + FromDate + '&ToDate=' + ToDate, function (data) {

        if (data != null) {

            var i;
            var TROpts = "";

            $('#tblContactSQ').empty();
            $('#quotesModal .modal-title').text(getCurrentCompanyLabel() + ' - Sales Quotes');
            $('#quotesModal').modal('show');
            if (data.length > 0) {

                for (i = 0; i < data.length; i++) {

                    TROpts += "<tr><td>" + data[i].No + "</td ><td>" + data[i].Order_Date + "</td><td>" + data[i].Sell_to_Customer_Name + "</td>" +
                        "<td>" + data[i].Payment_Terms_Code + "</td>";

                    if (data[i].TPTPL_Schedule_status == "Pending") {
                        TROpts += "<td><span class='badge bg-danger'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }
                    else if (data[i].TPTPL_Schedule_status == "Partial") {
                        TROpts += "<td><span class='badge bg-info text-dark'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }
                    else if (data[i].TPTPL_Schedule_status == "Completed") {
                        TROpts += "<td><span class='badge bg-success' title='Schedule completed'>" + data[i].TPTPL_Schedule_status + "</span></td>";
                    }

                    TROpts += "<td><a onclick='ShowSQProds(\"" + data[i].No + "\")'><i class='bx bx-show'></i></a></td></tr>";
                }
                $('#tblContactSQ').append(TROpts);
            }
            else {
                $('#tblContactSQ').append("<tr><td colspan=6 align='center'>No Records</td></tr>");

            }
            $('#btnSQListSpinner').hide();

        }
    });

}

$('#txtSQListToDate').change(function () {

    if ($('#txtSQListFromDate').val() == null || $('#txtSQListFromDate').val() == "") {
        $('#lblSQListFDateMsg').css('display', 'block');
        $('#txtSQListToDate').val("");
    }
    else {
        $('#lblSQListFDateMsg').css('display', 'none');
        var fromDate = $('#txtSQListFromDate').val();
        var toDate = $('#txtSQListToDate').val();
        BindContactSQ(fromDate, toDate, "");
    }

});

$('#btnSQListShowAll').click(function () {

    $('#txtSQListFromDate, #txtSQListToDate').val("");
    BindContactSQ("", "", "");
});

function ShowSQProds(SQNo) {

    $.ajax(
        {
            url: '/SPSalesQuotes/GetSalesLineItems?DocumentNo=' + SQNo,
            type: 'GET',
            contentType: 'application/json',
            success: function (data) {

                $('#lblSQNo').text(SQNo);
                $('#tblContactSQProds').empty();
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

                        $('#tblContactSQProds').append(rowData);
                    });
                }
                else {
                    rowData = "<tr><td colspan=9>No Records Found</td></tr>";
                    $('#tbSQProduct').append(rowData);
                }

                $('#modalSQProds').css('display', 'block');
                $('#dvSQProds').css('display', 'block');
            },
            error: function () {
                alert("error");
            }
        }
    );

}

$('.btn-close').click(function () {

    $('#modalSQProds').css('display', 'none');
    $('#dvSQProds').css('display', 'none');

});

//business plan
function BindFinancialYear() {
    $('#ddlFinancialYear').empty();
    let currentDate = new Date();
    let currentYear = currentDate.getFullYear();
    let currentMonth = currentDate.getMonth();

    var yearOpts = "";
    yearOpts += "<option value='-1'>---Select---</option>";
    var prevFinancialYear = (currentYear - 1) + '-' + currentYear;
    var currFinancialYear = currentYear + '-' + (currentYear + 1);

    if (currentMonth <= 2) {
        /*yearOpts += "<option value='" + (currentYear - 1) + '-' + currentYear + "'>" + (currentYear - 1) + '-' + currentYear + "</option>";*/
        yearOpts += "<option value='" + prevFinancialYear + "'>" + prevFinancialYear + "</option>";
    }

    yearOpts += "<option value='" + currFinancialYear + "'>" + currFinancialYear + "</option>";

    $('#ddlFinancialYear').append(yearOpts);

    if (currentMonth <= 2) {
        $('#ddlFinancialYear').val(prevFinancialYear);
    }
    else {
        $('#ddlFinancialYear').val(currFinancialYear);
    }

    $('#lblPrevFinancialYear').text(prevFinancialYear);
    $('#lblFinancialYear').text(currFinancialYear);

    //filter = "Plan_Year eq '" + $('#ddlFinancialYear').val() + "'";

    BindContactBusinessPlan($('#ddlFinancialYear').val(), "");
}


function BusinessPlan(item) {
    //$("#CompanyNo").val('');
    currentCompanyName = getSafeText(item && item.Name);
    $('#businessplanModal .modal-title').text(getCurrentCompanyLabel() + ' - Business Plan');
    var no = item.No;

    $("#CompanyNo").val(no);
    BindFinancialYear();
    /*  BindContactBusinessPlan($('#ddlFinancialYear').val(), no);*/
    /*  var num = $("#CompanyNo").val(no);*/

}

function BindContactBusinessPlan(PlanYear, No) {

    if (No == "") {
        No = $("#CompanyNo").val();
    }
    $.get('/SPContacts/GetContactBusinessPlan?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&CCompanyNo=' + No + '&PlanYear=' + PlanYear, function (data) {

        $('#lblDetailsYearGrid').text(PlanYear);

        const PlanYear_ = PlanYear.split('-');
        $('#lblDetailsPrevYear').text((parseInt(PlanYear_[0]) - 1) + "-" + PlanYear_[0]);

        var TROpts = "";
        var i;
        $('#tblDetailsContactBusinessPlan').empty();
        $('#businessplanModal .modal-title').text(getCurrentCompanyLabel() + ' - Business Plan');
        $('#businessplanModal').modal('show');

        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td hidden>" + data[i].Product_No + "</td><td>" + data[i].Product_Name + "</td><td>" + data[i].Pre_Year_Demand.toFixed(3) + "</td><td>" + data[i].Pre_Year_Target.toFixed(3) + "</td><td>" +
                    data[i].Last_year_Sale_Qty.toFixed(3) + "</td><td>" + data[i].Last_year_Sale_Amount.toFixed(2) + "</td>" +
                    "<td>" + data[i].Demand.toFixed(3) + "</td><td>" + data[i].Target.toFixed(3) + "</td><td>" + data[i].Average_Price.toFixed(2) + "</td><td>" +
                    data[i].PCPL_Target_Revenue.toFixed(2) + "</td></tr>";

            }

        }
        else {
            TROpts += "<tr><td colspan='10'>No Data Found</td></tr>";
        }

        $('#tblDetailsContactBusinessPlan').append(TROpts);

    });

}



//daily visit 
function DailyVisit(item) {
    currentCompanyName = getSafeText(item && item.Name);
    $('#DetailsModal .modal-title').text(getCurrentCompanyLabel() + ' - Daily Visit Plan');
    var no = item.No;
    $("#CompanyNo").val(no);
    BindContactDailyVisit("", "", no);
}
function BindContactDailyVisit(FromDate, ToDate, No) {

    if (No == "") {
        No = $("#CompanyNo").val();
    }
    $('#btnDailyVisitListSpinner').show();
    $.get('/SPContacts/GetContactDailyVisits?SPCode=' + $('#hdnLoggedInUserSPCode').val() + '&FromDate=' + FromDate + '&ToDate=' + ToDate + '&CCompanyNo=' + No, function (data) {

        var TROpts = "";
        var i;
        $('#tblContactDailyVisit').empty();
        $('#DetailsModal .modal-title').text(getCurrentCompanyLabel() + ' - Daily Visit Plan');
        $('#DetailsModal').modal('show');
        if (data.length > 0) {

            for (i = 0; i < data.length; i++) {

                TROpts += "<tr><td>" + data[i].Date + "</td><td>" + data[i].Visit_Name + "</td><td>" + data[i].Visit_SubType_Name + "</td><td>" + data[i].Contact_Company_Name +
                    "</td><td>" + data[i].Contact_Person_Name + "</td><td>" + data[i].Event_Name + "</td><td>" + data[i].Topic_Name + "</td><td>" + data[i].Mode_of_Visit +
                    "</td><td>" + data[i].Feedback + "</td></tr>";

            }

        }
        else {
            TROpts += "<tr><td colspan='9'>No Data Found</td></tr>";
        }

        $('#tblContactDailyVisit').append(TROpts);
        $('#btnDailyVisitListSpinner').hide();

    });

}

$('#txtDailyVisitTDate').change(function () {

    if ($('#txtDailyVisitFDate').val() == null || $('#txtDailyVisitFDate').val() == "") {
        $('#lblDailyVisitFDateMsg').css('display', 'block');
        $('#txtDailyVisitTDate').val("");
    }
    else {
        $('#lblDailyVisitFDateMsg').css('display', 'none');
        var fromDate = $('#txtDailyVisitFDate').val();
        var toDate = $('#txtDailyVisitTDate').val();
        BindContactDailyVisit(fromDate, toDate, "");
    }

});

$('#btnDailyVisitShowAll').click(function () {

    $('#txtDailyVisitFDate, #txtDailyVisitTDate').val("");
    BindContactDailyVisit("", "", "");
});
var custVendorPortalUrl = $('#getCustVendorPortalUrl').val();
var currentCompanyName = "";

function getSafeText(value) {
    if (value === null || value === undefined) return "";
    return (value + "").trim();
}

function getCurrentCompanyLabel() {
    return getSafeText(currentCompanyName) || "Selected Company";
}

function escapeHtmlAttr(value) {
    return (value === null || value === undefined ? '' : (value + ''))
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}