(function () {
    function clearAuthState() {
        try { localStorage.removeItem('token'); } catch (e) { }
        try { localStorage.removeItem('jwt'); } catch (e) { }
        try { sessionStorage.removeItem('token'); } catch (e) { }
        try { sessionStorage.removeItem('jwt'); } catch (e) { }
        try { document.cookie = 'authToken=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/'; } catch (e) { }
    }

    $(document).ajaxError(function (event, jqxhr) {
        if (!jqxhr || jqxhr.status !== 401 || window.__authRedirecting) return;
        window.__authRedirecting = true;
        clearAuthState();
        window.location.replace('/Account/Login');
    });
})();
