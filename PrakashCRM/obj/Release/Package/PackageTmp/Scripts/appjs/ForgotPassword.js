$(document).ready(function () {

    var requestPasswordResetUrl = $('#requestPasswordResetUrl').val();

    $('#invalidMsg').hide();
    $('#invalidForgotPassMsg').hide();

    $('.close').click(function () {
        $('#invalidMsg').hide();
        $('#invalidForgotPassMsg').hide();
        $('#forgotEmail').val('');
        $('#email').val('');
        $('#pass').val('');
    });

    $('#forgotEmail').keypress(function (e) {
        if (e.which == 13) {
            $('#btnForgotPass').click();
            return false;
        }
    });

    $('#btnForgotPass').click(function () {

        $('#divImage').show();

        if ($('#forgotEmail').val() !== "") {
            $.ajax({
                url: requestPasswordResetUrl,
                type: 'POST',
                data: { email: $('#forgotEmail').val() },
                success: function (data) {
                    $('#divImage').hide();

                    if (data && data.Success) {
                        if (sessionStorage.getItem('#modal') !== 'true') {
                            $('#modal').css('display', 'block');
                            sessionStorage.setItem('#ad_modal', 'true');
                        }

                        $('#btnForgotPass').prop('disabled', true);
                    }
                    else {
                        ShowErrMsg(data && data.Message ? data.Message : 'Please Enter Email ID Of Registered User.');
                        $('#forgotEmail').val('');
                        $('#forgotEmail').focus();
                    }
                },
                error: function () {
                    $('#divImage').hide();
                    ShowErrMsg('Unable to process password reset right now.');
                }
            });
        }
        else {

            $('#divImage').hide();
            ShowErrMsg('Please Enter Email ID.');

        }

    });
});

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