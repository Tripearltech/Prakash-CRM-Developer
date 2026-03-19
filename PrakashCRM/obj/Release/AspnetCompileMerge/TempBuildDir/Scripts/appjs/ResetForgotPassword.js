$(document).ready(function () {

    var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';
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
        if ($('#newpass').val() != "") {
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
        if ($('#confirmnewpass').val() != "") {
            if ($('#newpass').val() != $('#confirmnewpass').val()) {

                var msg = "New Password and Confirm New Password didn\'t match.";
                ShowErrMsg(msg);

                $('#confirmnewpass').val('');
                $('#confirmnewpass').focus();
            }
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

        $('#divImage').show();

        if ($('#newpass').val() != "" && $('#confirmnewpass').val() != "" && $('#newpass').val() == $('#confirmnewpass').val()) {

            $.get(apiUrl + 'GetByEmail?email=' + getUrlVars()["token"] + '&isEncrypted=true', function (data) {

                if (data != null) {

                    $.post(
                        apiUrl + 'ResetForgotPassword?email=' + data.Company_E_Mail + '&userNo=' + data.No + '&newPassword=' + $('#confirmnewpass').val(),
                        function (data) {

                            if (data) {

                                $('#divImage').hide();
                                //$('#successMsg').show();
                                if (sessionStorage.getItem('#modal') !== 'true') {
                                    $('#modal').css('display', 'block');

                                    //then the modal will be set true in the current session due to which the modal won't

                                    //reappear on the refresh, we need to reload the page in a new tab to make the modal

                                    //reappear.

                                    sessionStorage.setItem('#ad_modal', 'true');
                                }
                                $('#btnReset').prop('disabled', true);
                            }
                        }
                    );
                }
            });
        }
        else {

            $('#divImage').hide();
            var msg = "Please Enter New Password and Confirm New Password and both must be a same.";
            ShowErrMsg(msg);

        }

    });
});

function isValid(str) {
    return /^(?=.*[!@$_])[a-zA-Z0-9!@$_]+$/.test(str);
}
function getUrlVars() {

    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;

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