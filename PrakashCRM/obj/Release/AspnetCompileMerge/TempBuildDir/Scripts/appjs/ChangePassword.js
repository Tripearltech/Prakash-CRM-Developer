$(document).ready(function () {

    var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';

    $("#show_hide_currpassword a").on('click', function (event) {
        debugger;
        event.preventDefault();
        if ($('#show_hide_currpassword input').attr("type") == "text") {
            $('#show_hide_currpassword input').attr('type', 'password');
            $('#show_hide_currpassword i').addClass("bx-hide");
            $('#show_hide_currpassword i').removeClass("bx-show");
        } else if ($('#show_hide_currpassword input').attr("type") == "password") {
            $('#show_hide_currpassword input').attr('type', 'text');
            $('#show_hide_currpassword i').removeClass("bx-hide");
            $('#show_hide_currpassword i').addClass("bx-show");
        }
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

    $('#successMsg').hide();
    $('#invalidMsg').hide();
    $('#PassNotMatchMsg').hide();
    $('#PassPatternMsg').hide();

    $('#txtNewPass').blur(function () {
        if ($('#txtNewPass').val() != "") {
            var str = $('#txtNewPass').val();
            //alert(isValid(str));
            if (str.match(/[a-z]/g) && str.match(/[A-Z]/g) && str.match(/[0-9]/g) && str.match(/[^a-zA-Z\d]/g) && isValid(str) && str.length >= 8) {
                $('#btnSavePass').prop('disabled', false);
            }
            else {

                Lobibox.notify('error', {
                    pauseDelayOnHover: true,
                    continueDelayOnInactiveTab: false,
                    position: 'top right',
                    icon: 'bx bx-x-circle',
                    msg: 'Password does not match the policy guidelines.'
                });

                $('#txtNewPass').val('');
                $('#txtNewPass').focus();
                $('#btnSavePass').prop('disabled', true);
            }
        }
    });

    $('#txtConfirmPass').blur(function () {
        if ($('#txtConfirmPass').val() != "") {
            if ($('#txtNewPass').val() != $('#txtConfirmPass').val()) {

                var msg = "New Password and Confirm New Password didn\'t match.";
                ShowErrMsg(msg);

                $('#txtConfirmPass').val('');
                $('#txtConfirmPass').focus();

            }
        }
    });

    $('#btnSavePass').click(function () {

        if ($('#txtCurPass').val() == "" || $('#txtNewPass').val() == "" || $('#txtConfirmPass').val() == "") {

            var msg = "Please Fill Details";
            ShowErrMsg(msg);

        }
        else {
            alert($('#txtNewPass').val());
            $('#divImage').show();
            //$.get(apiUrl + 'GetPassByEmail?email=nishant.m@tripearltech.com', function (userPass) {
            $.get(apiUrl + 'GetPassByEmail?email=' + $('#hfLoggedInUserEmail').val(), function (userPass) {
                if (userPass == $('#txtCurPass').val()) {
                    $.get(apiUrl + 'GetByEmail?email=' + $('#hfLoggedInUserEmail').val(), function (data) {
                        if (data.Company_E_Mail != null && data.No != null) {
                            //var newpass = encodeURIComponent($('#txtNewPass').val());
                            $.get(apiUrl + "ChangePassword?email=" + data.Company_E_Mail + "&userNo=" + data.No + "&newPassword=" + $('#txtNewPass').val(),
                                function (data) {
                                    if (data) {
                                        $('#divImage').hide();

                                        if (sessionStorage.getItem('#modal') !== 'true') {
                                            $('#modal').css('display', 'block');

                                            //then the modal will be set true in the current session due to which the modal won't

                                            //reappear on the refresh, we need to reload the page in a new tab to make the modal

                                            //reappear.

                                            sessionStorage.setItem('#ad_modal', 'true');
                                        }
                                        $('#btnSavePass').prop('disabled', true);
                                    }
                                }
                            );
                        }
                    });
                }
                else {

                    var msg = "Current Password Is Incorrect, Please Enter Correct Password.";
                    ShowErrMsg(msg);

                    $('#divImage').hide();
                }
            });
        }
    });

    $('#btnCancel').click(function () {
        $('#txtCurPass').val('');
        $('#txtNewPass').val('');
        $('#txtConfirmPass').val('');
    });
});

function isValid(str) {
    return /^(?=.*[!@$_])[a-zA-Z0-9!@$_]+$/.test(str);
}

function hideshowCurPass() {
    var password = document.getElementById("txtCurPass");
    var slash = document.getElementById("slash");
    var eye = document.getElementById("eye");

    if (password.type === 'password') {
        password.type = "text";
        slash.style.display = "block";
        eye.style.display = "none";
    }
    else {
        password.type = "password";
        slash.style.display = "none";
        eye.style.display = "block";
    }
}

function hideshowNewPass() {

    var password1 = document.getElementById("txtNewPass");
    var slash1 = document.getElementById("slash1");
    var eye1 = document.getElementById("eye1");

    if (password1.type === 'password') {
        password1.type = "text";
        slash1.style.display = "block";
        eye1.style.display = "none";
    }
    else {
        password1.type = "password";
        slash1.style.display = "none";
        eye1.style.display = "block";
    }

}

function hideshowConfPass() {
    var password2 = document.getElementById("txtConfirmPass");
    var slash2 = document.getElementById("slash2");
    var eye2 = document.getElementById("eye2");

    if (password2.type === 'password') {
        password2.type = "text";
        slash2.style.display = "block";
        eye2.style.display = "none";
    }
    else {
        password2.type = "password";
        slash2.style.display = "none";
        eye2.style.display = "block";
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