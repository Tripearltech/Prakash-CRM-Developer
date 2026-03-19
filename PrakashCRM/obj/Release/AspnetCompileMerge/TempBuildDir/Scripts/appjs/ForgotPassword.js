$(document).ready(function () {

    var portalUrl = $('#getSPPortalUrl').val();
    var apiUrl = $('#getServiceApiUrl').val() + 'Salesperson/';

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

        if ($('#forgotEmail').val() != "") {

            $.get(apiUrl + 'GetByEmail?email=' + $('#forgotEmail').val(), function (data) {

                if (data.Company_E_Mail != null && data.No != null) {

                    $.get(
                        apiUrl + 'ForgotPassword?email=' + data.Company_E_Mail + '&userNo=' + data.No + '&portalUrl=' + portalUrl,
                        function (data) {
                            $('#divImage').hide();
                            if (sessionStorage.getItem('#modal') !== 'true') {
                                $('#modal').css('display', 'block');

                                //then the modal will be set true in the current session due to which the modal won't

                                //reappear on the refresh, we need to reload the page in a new tab to make the modal

                                //reappear.

                                sessionStorage.setItem('#ad_modal', 'true');
                            }
                            $('#btnForgotPass').prop('disabled', true);
                        }
                    );
                }
                else {

                    $('#divImage').hide();
                    var msg = "Please Enter Email ID Of Registered User.";
                    ShowErrMsg(msg);

                    $('#forgotEmail').val('');
                    $('#forgotEmail').focus();
                }

            });

        }
        else {

            $('#divImage').hide();
            var msg = "Please Enter Email ID.";
            ShowErrMsg(msg);

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