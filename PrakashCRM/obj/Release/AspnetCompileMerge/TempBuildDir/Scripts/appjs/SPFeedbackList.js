var apiUrl = $('#getServiceApiUrl').val() + 'SPFeedback/';
$(document).ready(function () {
    var fDate = "";
    var tDate = "";
    var txtSP = "";
    FeedBackList(fDate, tDate, txtSP);
    BindCustomerDropdown();

});

$('#SearchBtn').on('click', function () {

    var fDate = $("#txtFromDate").val();

    var tDate = $("#txtToDate").val();
    var txtSP = $("#txtCustomer_Name").val();




    if (fDate == "" || tDate == "" || txtSP == "") {

        if (fDate == "" && tDate != "") {

            $("#Fdatevalidate").text("From Date is Required");
            $("#Tdatevalidate").text("");

        }

        else if (fDate != "" && tDate == "") {

            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("To Date is Required");

        }
        else if (fDate == "" && tDate == "" && txtSP == "") {

            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("");
            FeedBackList(fDate, tDate, txtSP);

        }
        else if (fDate == "" && tDate == "" && txtSP != "") {
            $("#Fdatevalidate").text("");
            $("#Tdatevalidate").text("");
            FeedBackList(fDate, tDate, txtSP);

        }


    }

    else if (fDate != "" && tDate != "") {

        if (fDate != null) {

            $("#Fdatevalidate").text("");

        } if (tDate != null) {

            $("#Tdatevalidate").text("");

        }

    }



    if (fDate != "" && tDate != "") {

        $("#Fdatevalidate").text("");

        $("#Tdatevalidate").text("");

        FeedBackList(fDate, tDate, txtSP);
    }


});

function FeedBackList(fDate, tDate, txtSP) {

    $.ajax({
        url: '/SPFeedback/GetFeedBackList',
        type: 'GET',
        data: { FDate: fDate, TDate: tDate, TXTSp: txtSP },
        contentType: 'application/json',
        success: function (data) {
            $("#tblFeedBacklist").empty();

            var rowData = "";
            if (data.length > 0) {
                $.each(data, function (index, item) {

                    const itemJson = JSON.stringify(item).replace(/"/g, '&quot;');
                    rowData += `<tr><td class="open-modal" data-item ="${itemJson}"><a class='order_no cursor-pointer'>` + item.No + "</a></td><td>" + item.Submitted_On + "</td><td>" + item.Contact_No + "</td><td>" + item.Contact_Person + "</td><td>" + item.Company_Name + "</td><td>" + item.Products + "</td><td>"
                        + item.Overall_Rating + "</td><td>" + item.Overall_Rating_Comments + "</td><td>" + item.Suggestion + "</td><td>" +
                        item.Employee_Name + "</td></tr>";

                });
                $(document).on('click', '.open-modal', function (e) {

                    const itemJson = $(this).attr("data-item");
                    const item = JSON.parse(itemJson);
                    OpenModal(item);
                });
                function OpenModal(item) {
                    var feedbackId = item.No;

                    $.ajax({
                        url: '/SPFeedback/GetFeedBackLineList',
                        type: 'GET',
                        contentType: 'application/json',
                        data: { FeedbackId: feedbackId },
                        success: function (data) {
                            $("#tblFeedbackLine").empty();
                            $("#FeedbackLinesModal").modal('show');
                            /*   $("#tblFeedbackLine").empty();*/
                            if (data.length > 0) {
                                var rowData = "";

                                $.each(data, function (index, item) {

                                    rowData += "<tr><td>" + item.Feedback_Question_No + "</td><td>" + item.Feedback_Question + "</td><td>" + item.Rating + "</td><td>" + item.Comments + "</td></tr>";
                                });
                            }
                            else {
                                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
                            }
                            $("#tblFeedbackLine").append(rowData);
                        }
                    });
                }
            }
            else {
                rowData = "<tr><td colspan='9' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblFeedBacklist").append(rowData);
        }
    });
}


function BindCustomerDropdown() {
    if (typeof ($.fn.autocomplete) === 'undefined') return;
    const $loader = $("#loader");
    const $spinner = $("#spinnerId");
    $('#txtCustomer_Name').autocomplete({
        serviceUrl: '/SPFeedback/GetCustomerDropdown',
        paramName: "prefix",
        minChars: 2,
        noCache: true,
        ajaxSettings: {
            type: "POST"
        },
        onSelect: function (suggestion) {
            $("#hfCustomerNo").val(suggestion.data);
            $("#txtCustomerName").val(suggestion.value);
        },
        onShow: function () {
            setTimeout(() => {
                $input.focus();
            }, 10);
        },
        onSearchStart: function () {
            $spinner.addClass("input-group");
            $loader.show();
        },
        transformResult: function (response) {
            $spinner.removeClass("input-group");
            $loader.hide();
            var json;
            try {
                json = $.parseJSON(response);
            } catch (e) {
                console.error("Invalid JSON response", response);
                return { suggestions: [] };
            }

            return {
                suggestions: $.map(json, function (item) {
                    return {
                        value: item.Company_Name,
                        data: item.Company_No
                    };
                })
            };
        }
    });
}

$("#btnClearFilter").on('click', function () {
    FeedBackListClear();
});
function FeedBackListClear() {

    $("#txtCustomer_Name").val('');
    $("#txtFromDate").val('');
    $("#txtToDate").val('');
    $("#Fdatevalidate").text("");
    $("#Tdatevalidate").text("");
    FeedBackList("", "", "");
}