var apiUrl = $('#getServiceApiUrl').val() + 'SPFeedback/';

function setFeedbackSaveButton(isLoading, label) {
    var $btn = $('#btnSaveFeedbackQuestion');
    var btnLabel = label || $btn.data('label') || ($.trim($btn.text()) || 'Save');
    $btn.data('label', btnLabel);

    if (isLoading) {
        $btn.prop('disabled', true);
        $btn.html("<i class='bx bx-loader-alt bx-spin' style='margin-right:6px;'></i>" + btnLabel);
    } else {
        $btn.prop('disabled', false);
        $btn.text(btnLabel);
    }
}

function showFbqError(msg) {
    var forceWhiteText = function () {
        $('.lobibox-notify.lobibox-notify-error .lobibox-notify-msg,.lobibox-notify.lobibox-notify-error .lobibox-notify-message').css('color', '#fff');
    };

    if (typeof ShowErrMsg === 'function') {
        ShowErrMsg(msg);
        setTimeout(forceWhiteText, 0);
        return;
    }
    if (typeof Lobibox !== 'undefined' && Lobibox.notify) {
        Lobibox.notify('error', {
            pauseDelayOnHover: true,
            size: 'mini',
            rounded: true,
            delayIndicator: false,
            continueDelayOnInactiveTab: false,
            position: 'top right',
            msg: msg
        });
        setTimeout(forceWhiteText, 0);
        return;
    }
    alert(msg);
}

function showFbqSuccess(msg) {
    if (typeof ShowActionMsg === 'function') {
        ShowActionMsg(msg);
        return;
    }
    if (typeof Lobibox !== 'undefined' && Lobibox.notify) {
        Lobibox.notify('success', {
            pauseDelayOnHover: true,
            size: 'mini',
            rounded: true,
            icon: 'bx bx-check-circle',
            delayIndicator: false,
            continueDelayOnInactiveTab: false,
            position: 'top right',
            msg: msg
        });
        return;
    }
    alert(msg);
}

$(document).ready(function () {
    FeedBackQuestionList();

    $('#btnNewFeedbackQuestion').on('click', function () {
        $('#hdnFeedbackQuestionNo').val('');
        $('#txtFeedbackQuestion').val('');
        $('#txtOrderNo').val('');
        $('#chkIsActive').prop('checked', true);
        setFeedbackSaveButton(false, 'Save');
        $('#feedbackQuestionModal').modal('show');
    });

    $(document).on('click', '.btnEditFeedbackQuestion', function () {
        $('#hdnFeedbackQuestionNo').val($(this).data('no') || '');
        $('#txtFeedbackQuestion').val($(this).data('question') || '');
        $('#txtOrderNo').val($(this).data('order') || '');
        $('#chkIsActive').prop('checked', ($(this).data('active') === true || $(this).data('active') === 'true'));
        setFeedbackSaveButton(false, 'Update');
        $('#feedbackQuestionModal').modal('show');
    });

    $('#btnSaveFeedbackQuestion').on('click', function () {
        var question = ($('#txtFeedbackQuestion').val() || '').trim();
        var orderNo = ($('#txtOrderNo').val() || '').trim();

        if (!question) {
            showFbqError('Feedback Question is required');
            return;
        }
        if (!orderNo) {
            showFbqError('Order No is required');
            return;
        }

        var no = ($('#hdnFeedbackQuestionNo').val() || '').trim();
        var actionLabel = no ? 'Update' : 'Save';
        var payload = {
            No: no,
            Feedback_Question: question,
            Order_No: orderNo,
            IsActive: $('#chkIsActive').is(':checked')
        };

        setFeedbackSaveButton(true, actionLabel);

        $.ajax({
            url: no ? '/SPFeedback/UpdateFeedBackQuestion' : '/SPFeedback/AddFeedBackQuestion',
            type: no ? 'PATCH' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res && res.success) {
                    $('#feedbackQuestionModal').modal('hide');
                    FeedBackQuestionList();
                    showFbqSuccess(no ? 'Feedback Question Updated Successfully' : 'Feedback Question Added Successfully');
                } else {
                    showFbqError((res && res.message) ? res.message : 'Save failed');
                }
            },
            complete: function () {
                setFeedbackSaveButton(false, actionLabel);
            },
            error: function (xhr) {
                var msg = 'Save failed';
                if (xhr && xhr.responseJSON) {
                    msg = xhr.responseJSON.message || xhr.responseJSON.Message || msg;
                } else if (xhr && xhr.responseText) {
                    try {
                        var parsed = JSON.parse(xhr.responseText);
                        msg = parsed.message || parsed.Message || msg;
                    } catch (e) {
                        msg = xhr.responseText;
                    }
                }
                showFbqError(msg);
            }
        });
    });
});

function FeedBackQuestionList() {

    $.ajax({
        url: '/SPFeedback/GetFeedBackQuestionList',
        type: 'GET',
        contentType: 'application/json',
        success: function (data) {
            $("#tblFeedBackQuestionList").empty();
            //$("#FeedbackLinesModal").modal('show');
            /*   $("#tblFeedbackLine").empty();*/
            if (data.length > 0) {
                var rowData = "";

                $.each(data, function (index, item) {
                    var q = (item.Feedback_Question || '').replace(/"/g, '&quot;');
                    rowData += "<tr><td><button type='button' class='btn btn-sm p-0 border-0 bg-transparent btnEditFeedbackQuestion' data-no='" + item.No + "' data-order='" + item.Order_No + "' data-active='" + item.IsActive + "' data-question=\"" + q + "\"><i class='bx bxs-edit'></i></button></td><td>" + item.No + "</td><td>" + item.Order_No + "</td><td>" + item.Feedback_Question + "</td><td>" + (item.IsActive ? "Yes" : "No") + "</td></tr>";
                });
            }
            else {
                rowData = "<tr><td colspan='5' style='text-align:left;'>No Records Found</td></tr>";
            }
            $("#tblFeedBackQuestionList").append(rowData);
        }
    });
}
           
