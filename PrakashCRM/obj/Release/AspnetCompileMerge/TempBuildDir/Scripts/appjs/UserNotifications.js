(function ($) {
    var currentFilter = 'All';
    var previewSize = 10;
    var pollHandle = null;
    var notificationPage = 1;
    var notificationPageSize = 10;

    function isNotificationPage() {
        return $('#allNotificationsList').length > 0;
    }

    function escapeHtml(value) {
        return $('<div/>').text(value || '').html();
    }

    function formatRelativeTime(value) {
        if (!value) {
            return '';
        }

        var date = new Date(value);
        if (isNaN(date.getTime())) {
            return value;
        }

        var seconds = Math.max(1, Math.floor((new Date().getTime() - date.getTime()) / 1000));
        if (seconds < 60) {
            return seconds + ' sec ago';
        }

        var minutes = Math.floor(seconds / 60);
        if (minutes < 60) {
            return minutes + ' min ago';
        }

        var hours = Math.floor(minutes / 60);
        if (hours < 24) {
            return hours + ' hr ago';
        }

        var days = Math.floor(hours / 24);
        if (days < 30) {
            return days + ' day ago';
        }

        return date.toLocaleDateString();
    }

    function buildPreviewItem(item) {
        var isUnreadClass = item.IsRead ? '' : ' notification-is-unread';
        var href = item.LinkUrl || 'javascript:;';

        return '' +
            '<a class="dropdown-item notification-item-card js-notification-item' + isUnreadClass + '" href="' + escapeHtml(href) + '" data-notification-id="' + escapeHtml(item.Id) + '" data-is-read="' + item.IsRead + '">' +
            '    <div class="d-flex align-items-center">' +
            '        <div class="notify ' + escapeHtml(item.IconColorClass || 'bg-light-secondary text-secondary') + '">' +
            '            <i class="' + escapeHtml(item.IconClass || 'bx bx-bell') + '"></i>' +
            '        </div>' +
            '        <div class="flex-grow-1">' +
            '            <h6 class="msg-name">' + escapeHtml(item.Title || 'Notification') + '<span class="msg-time float-end">' + escapeHtml(formatRelativeTime(item.CreatedOnUtc)) + '</span></h6>' +
            '            <p class="msg-info">' + escapeHtml(item.Description || '') + '</p>' +
            '        </div>' +
            '    </div>' +
            '</a>';
    }

    function buildPageItem(item) {
        var href = item.LinkUrl || 'javascript:;';
        var unreadBadge = item.IsRead ? '' : '<span class="badge bg-warning text-dark ms-2">Unread</span>';

        return '' +
            '<a class="text-decoration-none text-reset js-notification-item" href="' + escapeHtml(href) + '" data-notification-id="' + escapeHtml(item.Id) + '" data-is-read="' + item.IsRead + '">' +
            '    <div class="border rounded-3 p-3 mb-3 notification-item-card' + (item.IsRead ? '' : ' notification-is-unread') + '" data-category="' + escapeHtml(item.Category || 'Notification') + '">' +
            '        <div class="d-flex align-items-start gap-3">' +
            '            <div class="notify ' + escapeHtml(item.IconColorClass || 'bg-light-secondary text-secondary') + '"><i class="' + escapeHtml(item.IconClass || 'bx bx-bell') + '"></i></div>' +
            '            <div class="flex-grow-1">' +
            '                <div class="d-flex flex-wrap justify-content-between gap-2 align-items-center">' +
            '                    <div>' +
            '                        <h5 class="mb-1">' + escapeHtml(item.Title || 'Notification') + unreadBadge + '</h5>' +
            '                        <div class="text-muted small">' + escapeHtml(item.Category || 'Notification') + ' • ' + escapeHtml(formatRelativeTime(item.CreatedOnUtc)) + '</div>' +
            '                    </div>' +
            '                    <div class="text-muted small">' + escapeHtml(item.ReferenceNo || '') + '</div>' +
            '                </div>' +
            '                <p class="mb-0 mt-2">' + escapeHtml(item.Description || '') + '</p>' +
            '            </div>' +
            '        </div>' +
            '    </div>' +
            '</a>';
    }

    function updateUnreadBadge(unreadCount) {
        var $badge = $('#notificationUnreadCount');
        if (!$badge.length) {
            return;
        }

        if (!unreadCount) {
            $badge.hide();
            return;
        }

        $badge.text(unreadCount > 99 ? '99+' : unreadCount).show();
    }

    function updateItemUnreadBadge(unreadCount) {
        var $badge = $('#itemUpdateUnreadCount');
        if (!$badge.length) {
            return;
        }

        if (!unreadCount) {
            $badge.hide();
            return;
        }

        $badge.text(unreadCount > 99 ? '99+' : unreadCount).show();
    }

    function loadPreview() {
        if (!$('#headerNotificationsList').length) {
            return;
        }

        $('#headerNotificationsList').html('<div class="notification-preview-empty">Loading notifications...</div>');

        $.get('/SPNotification/GetAllUserNotifications', { top: previewSize, includeRead: true, skip: 0 })
            .done(function (feed) {
                feed = feed || {};
                var notifications = feed.Notifications || [];
                updateUnreadBadge(feed.UnreadCount || 0);

                if (!notifications.length) {
                    $('#headerNotificationsList').html('<div class="notification-preview-empty">No notifications yet.</div>');
                    return;
                }

                var html = '';
                $.each(notifications, function (_, item) {
                    html += buildPreviewItem(item);
                });

                $('#headerNotificationsList').html(html);
            });
    }

    function loadItemUpdatesPreview() {
        if (!$('#headerItemUpdatesList').length) {
            return;
        }

        $('#headerItemUpdatesList').html('<div class="notification-preview-empty">Loading updates...</div>');

        $.get('/SPNotification/GetAllUserNotifications', { top: previewSize, includeRead: true, skip: 0, category: 'Item' })
            .done(function (feed) {
                feed = feed || {};
                var updates = feed.Notifications || [];
                updateItemUnreadBadge(feed.UnreadCount || 0);

                if (!updates.length) {
                    $('#headerItemUpdatesList').html('<div class="notification-preview-empty">No price updates yet.</div>');
                    return;
                }

                var html = '';
                $.each(updates, function (_, item) {
                    html += buildPreviewItem(item);
                });

                $('#headerItemUpdatesList').html(html);
            });
    }

    function refreshUnreadCount() {
        if (!$('#notificationUnreadCount').length) {
            return;
        }

        $.get('/SPNotification/GetUserNotifications', { top: 1, includeRead: false, skip: 0 })
            .done(function (feed) {
                feed = feed || {};
                updateUnreadBadge(feed.UnreadCount || 0);
                if ($('#notificationPageUnreadBadge').length) {
                    $('#notificationPageUnreadBadge').text((feed.UnreadCount || 0) + ' unread');
                }
            });
    }

    function refreshItemUnreadCount() {
        if (!$('#itemUpdateUnreadCount').length) {
            return;
        }

        $.get('/SPNotification/GetUserNotifications', { top: 1, includeRead: false, skip: 0, category: 'Item' })
            .done(function (feed) {
                feed = feed || {};
                updateItemUnreadBadge(feed.UnreadCount || 0);
            });
    }

    function buildPagerButton(label, pageNumber, isActive, isDisabled) {
        var activeClass = isActive ? ' btn-dark active' : ' btn-outline-dark';
        var disabledClass = isDisabled ? ' disabled' : '';
        var attributes = isDisabled ? ' tabindex="-1" aria-disabled="true"' : '';

        return '<button type="button" class="btn btn-sm js-notification-page' + activeClass + disabledClass + '" data-page="' + pageNumber + '"' + attributes + '>' + label + '</button>';
    }

    function buildPagerMarkup(totalPages) {
        if (totalPages <= 1) {
            return '';
        }

        var pagesToRender = [];
        var startPage = Math.max(1, notificationPage - 1);
        var endPage = Math.min(totalPages, notificationPage + 1);

        pagesToRender.push(1);

        for (var page = startPage; page <= endPage; page += 1) {
            pagesToRender.push(page);
        }

        pagesToRender.push(totalPages);

        pagesToRender = $.grep(pagesToRender, function (pageNumber, index) {
            return $.inArray(pageNumber, pagesToRender) === index;
        }).sort(function (left, right) {
            return left - right;
        });

        var markup = '';
        var previousPage = 0;

        $.each(pagesToRender, function (_, pageNumber) {
            if (previousPage && pageNumber - previousPage > 1) {
                markup += buildPagerButton('...', previousPage, false, true);
            }

            markup += buildPagerButton(pageNumber, pageNumber, pageNumber === notificationPage, false);
            previousPage = pageNumber;
        });

        return markup;
    }

    function renderPager(totalCount) {
        if (!$('#notificationPagePager').length) {
            return;
        }

        var totalPages = Math.max(1, Math.ceil(totalCount / notificationPageSize));
        var startRecord = totalCount === 0 ? 0 : ((notificationPage - 1) * notificationPageSize) + 1;
        var endRecord = Math.min(totalCount, notificationPage * notificationPageSize);

        $('#notificationPageSummary').text('Showing ' + startRecord + ' - ' + endRecord + ' of ' + totalCount + ' notifications');
        $('#notificationPageInfo').text('Page ' + notificationPage + ' of ' + totalPages);
        $('#notificationPageNumbers').html(buildPagerMarkup(totalPages));
        $('#notificationPagePrev').prop('disabled', notificationPage <= 1);
        $('#notificationPageNext').prop('disabled', notificationPage >= totalPages);
        $('#notificationPagePager').toggle(totalCount > 0);
    }

    function loadAllNotifications() {
        if (!$('#allNotificationsList').length) {
            return;
        }

        $.get('/SPNotification/GetAllUserNotifications', {
            top: notificationPageSize,
            includeRead: true,
            skip: (notificationPage - 1) * notificationPageSize,
            category: currentFilter === 'All' ? '' : currentFilter
        })
            .done(function (feed) {
                feed = feed || {};
                var notifications = feed.Notifications || [];
                var totalCount = feed.TotalCount || 0;
                updateUnreadBadge(feed.UnreadCount || 0);
                $('#notificationPageUnreadBadge').text((feed.UnreadCount || 0) + ' unread');

                if (!notifications.length) {
                    $('#allNotificationsList').html('<div class="notification-page-empty">No notifications available.</div>');
                    renderPager(totalCount);
                    return;
                }

                var html = '';
                $.each(notifications, function (_, item) {
                    html += buildPageItem(item);
                });

                $('#allNotificationsList').html(html);
                renderPager(totalCount);
            });
    }

    function markNotificationRead(id) {
        return $.ajax({
            url: '/SPNotification/MarkNotificationRead',
            type: 'POST',
            data: { id: id }
        });
    }

    function markAllNotificationsRead() {
        return $.ajax({
            url: '/SPNotification/MarkAllNotificationsRead',
            type: 'POST'
        });
    }

    function startPolling() {
        if (pollHandle !== null) {
            window.clearInterval(pollHandle);
        }

        pollHandle = window.setInterval(function () {
            refreshUnreadCount();
            refreshItemUnreadCount();
            if (isNotificationPage()) {
                loadAllNotifications();
            }
        }, 60000);
    }

    function applyInitialCategoryFilter() {
        if (!isNotificationPage()) {
            return;
        }

        var selectedCategory = new URLSearchParams(window.location.search).get('category');
        if (!selectedCategory) {
            return;
        }

        var $selectedFilter = $('#notificationPageFilters .notification-filter-chip[data-filter="' + selectedCategory + '"]');
        if (!$selectedFilter.length) {
            return;
        }

        $('#notificationPageFilters .notification-filter-chip').removeClass('active');
        $selectedFilter.addClass('active');
        currentFilter = selectedCategory;
    }

    $(document).ready(function () {
        refreshUnreadCount();
        refreshItemUnreadCount();
        applyInitialCategoryFilter();

        if (isNotificationPage()) {
            loadAllNotifications();
        }

        startPolling();

        $('#userNotificationBell').on('click', function () {
            loadPreview();
        });

        $('#itemUpdateBell').on('click', function () {
            loadItemUpdatesPreview();
        });

        $('#markAllNotificationsRead, #notificationPageMarkAllRead, #markAllItemUpdatesRead').on('click', function () {
            markAllNotificationsRead().always(function () {
                loadPreview();
                loadItemUpdatesPreview();
                loadAllNotifications();
                refreshItemUnreadCount();
            });
        });

        $('#notificationPageFilters').on('click', '.notification-filter-chip', function () {
            $('#notificationPageFilters .notification-filter-chip').removeClass('active');
            $(this).addClass('active');
            currentFilter = $(this).data('filter') || 'All';
            notificationPage = 1;
            loadAllNotifications();
        });

        $('#notificationPageSize').on('change', function () {
            notificationPageSize = parseInt($(this).val(), 10) || 20;
            notificationPage = 1;
            loadAllNotifications();
        });

        $('#notificationPagePrev').on('click', function () {
            if (notificationPage <= 1) {
                return;
            }

            notificationPage -= 1;
            loadAllNotifications();
        });

        $('#notificationPageNext').on('click', function () {
            notificationPage += 1;
            loadAllNotifications();
        });

        $(document).on('click', '.js-notification-page', function () {
            var pageNumber = parseInt($(this).data('page'), 10);

            if (!pageNumber || pageNumber === notificationPage || $(this).hasClass('disabled')) {
                return;
            }

            notificationPage = pageNumber;
            loadAllNotifications();
        });

        $(document).on('click', '.js-notification-item', function (event) {
            var $item = $(this);
            var id = $item.data('notification-id');
            var isRead = $item.data('is-read');
            var href = $item.attr('href');

            if (!id || isRead === true || isRead === 'True' || href === 'javascript:;') {
                return;
            }

            event.preventDefault();
            markNotificationRead(id).always(function () {
                window.location.href = href;
            });
        });
    });
}(jQuery));
