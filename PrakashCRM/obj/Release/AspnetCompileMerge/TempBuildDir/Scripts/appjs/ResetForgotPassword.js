$(document).ready(function () {

    var validateResetTokenUrl = $('#validateResetTokenUrl').val();
    var completeResetPasswordUrl = $('#completeResetPasswordUrl').val();
    var resetToken = $('#resetToken').val();
    var isTokenValid = false;

    function showTokenError(message) {
        $('#tokenStatusMessage').text(message).show();
        $('#btnReset').prop('disabled', true);
        $('#newpass').prop('disabled', true);
        $('#confirmnewpass').prop('disabled', true);
    }

    function clearTokenError() {
        $('#tokenStatusMessage').hide().text('');
        $('#newpass').prop('disabled', false);
        $('#confirmnewpass').prop('disabled', false);
    }

    $.get(validateResetTokenUrl, { token: resetToken }, function (data) {
        if (data && data.IsValid) {
            isTokenValid = true;
            clearTokenError();
        }
        else {
            showTokenError(data && data.Message ? data.Message : 'Invalid link');
        }
    }).fail(function () {
        showTokenError('Invalid link');
    });

    $("#show_hide_newpassword a").on('click', function (event) {
        debugger;
        event.preventDefault();
        if ($('#show_hide_newpassword input').attr("type") == "text") {
            $('#show_hide_newpassword input').attr('type', 'password');
            $('#show_hide_newpassword i').addClass("bx-hide");
            $('#show_hide_newpassword i').removeClass("bx-show");
        } else if ($('#show_hide_newpassword input').attr("type") == "password") {
            $('#show_hide_newpassword input').attr('type', 'text');
            $('#show_hide_newpassword i').removeClass("bx-hide");
            $('#show_hide_newpassword i').addClass("bx-show");
        }
    });

    $("#show_hide_confpassword a").on('click', function (event) {
        event.preventDefault();
        if ($('#show_hide_confpassword input').attr("type") == "text") {
            $('#show_hide_confpassword input').attr('type', 'password');
            $('#show_hide_confpassword i').addClass("bx-hide");
            $('#show_hide_confpassword i').removeClass("bx-show");
        } else if ($('#show_hide_confpassword input').attr("type") == "password") {
            $('#show_hide_confpassword input').attr('type', 'text');
            $('#show_hide_confpassword i').removeClass("bx-hide");
            $('#show_hide_confpassword i').addClass("bx-show");
        }
    });

    $('#newpass').blur(function () {
        if (isTokenValid && $('#newpass').val() !== "") {
            var str = $('#newpass').val();
            if (str.match(/[a-z]/g) && str.match(/[A-Z]/g) && str.match(/[0-9]/g) && str.match(/[^a-zA-Z\d]/g) && isValid(str) && str.length >= 8) {
                $('#btnReset').prop('disabled', false);
            }
            else {

                Lobibox.notify('error', {
                    pauseDelayOnHover: true,
                    continueDelayOnInactiveTab: false,
                    position: 'top right',
                    icon: 'bx bx-x-circle',
                    msg: 'Password does not match the policy guidelines.'
                });

                $('#newpass').val('');
                $('#newpass').focus();
                $('#btnReset').prop('disabled', true);
            }
        }
    });

    $('#confirmnewpass').blur(function () {
        if ($('#confirmnewpass').val() !== "" && $('#newpass').val() !== $('#confirmnewpass').val()) {
            ShowErrMsg("New Password and Confirm New Password didn't match.");
            $('#confirmnewpass').val('');
            $('#confirmnewpass').focus();
        }
    });

    $('#confirmnewpass').keypress(function (e) {
        if (e.which == 13) {
            $('#btnReset').click();
            return false;
        }
    });

    $('#msgCloseBtn').click(function () {
        alert('close click');
    });

    $('#btnReset').click(function () {

        if (!isTokenValid) {
            showTokenError('Invalid link');
            return;
        }

        $('#divImage').show();

        if ($('#newpass').val() !== "" && $('#confirmnewpass').val() !== "" && $('#newpass').val() === $('#confirmnewpass').val()) {
            $.ajax({
                url: completeResetPasswordUrl,
                type: 'POST',
                data: {
                    token: resetToken,
                    newPassword: $('#confirmnewpass').val()
                },
                success: function (data) {
                    $('#divImage').hide();

                    if (data && data.Success) {
                        if (sessionStorage.getItem('#modal') !== 'true') {
                            $('#modal').css('display', 'block');
                            sessionStorage.setItem('#ad_modal', 'true');
                        }

                        $('#btnReset').prop('disabled', true);
                    }
                    else {
                        var errorMessage = data && data.Message ? data.Message : 'Invalid link';
                        if (errorMessage === 'Invalid link' || errorMessage === 'Link already used' || errorMessage === 'Link expired') {
                            isTokenValid = false;
                            showTokenError(errorMessage);
                        }
                        else {
                            ShowErrMsg(errorMessage);
                        }
                    }
                },
                error: function () {
                    $('#divImage').hide();
                    ShowErrMsg('Unable to reset password right now.');
                }
            });
        }
        else {

            $('#divImage').hide();
            ShowErrMsg('Please Enter New Password and Confirm New Password and both must be a same.');

        }

    });
});

function isValid(str) {
    return /^(?=.*[^a-zA-Z0-9]).+$/.test(str);
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
