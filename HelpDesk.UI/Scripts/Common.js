function moveMessagesToTopUnderNavBar()
{
    if ($(window).scrollTop() > 52)
        $('#messages').css(
        {
            'position': 'fixed',
            'top': '0'
        });
    else
        $('#messages').css(
        {
            'position': 'absolute',
            'top': '52px'
        });
}

function displayNewSuccessMessage(messageText)
{
    displayNewMessage(messageText, 'success');
}

function displayNewFailMessage(messageText)
{
    displayNewMessage(messageText, 'fail');
}

function displayNewMessage(messageText, messageType)
{
    var closeButton = $('<a></a>');
    closeButton.addClass('close');
    closeButton.attr(
    {
        'href': '#',
        'aria-label': 'close'
    });
    closeButton.click(function()
    {
        alert.hide('slow');
    });
    closeButton.html("&times;");

    var icon = $('<span></span>');
    icon.addClass('glyphicon');
    if (messageType === 'success')
        icon.addClass('glyphicon-info-sign');
    if (messageType === 'fail')
        icon.addClass('glyphicon-exclamation-sign');

    var alert = $('<div></div>');
    alert.addClass('alert');
    if (messageType === 'success')
        alert.addClass('alert-success');
    if (messageType === 'fail')
        alert.addClass('alert-danger');
    alert.append(closeButton, icon, ' ' + messageText);

    $('#messages').append(alert);
    alert.show('slow');
    setTimeout(function() { $(alert).hide('slow') }, 10000);
}

var numberOfSentAjaxRequests = 0;

function showProgressIndicator()
{
    if (numberOfSentAjaxRequests++ === 0)
        $('#progressIndicator').show();
}

function hideProgressIndicator()
{
    if (--numberOfSentAjaxRequests === 0)
        $('#progressIndicator').hide();
}

function sendAjaxRequest(url, method, data, onSuccess)
{
    $.ajax(url,
    {
        type: method,
        data: data,
        success: function(response)
        {
            if (response.Success)
                displayNewSuccessMessage(response.Success);
            if (response.Fail)
                displayNewFailMessage(response.Fail);
            onSuccess(response);
        },
        error: function()
        {
            displayNewFailMessage("Problem with connection. Try again, and if the problem persists contact your system administrator.");
        },
        beforeSend: showProgressIndicator,
        complete: hideProgressIndicator
    });
}

function formatDate(ticks)
{
    function padZero(value)
    {
        return value < 10 ? "0" + value : value;
    }

    var date = new Date(Number(ticks));
    var year = date.getFullYear();
    var month = padZero(date.getMonth() + 1);
    var day = padZero(date.getDate());
    var hours = padZero(date.getHours());
    var minutes = padZero(date.getMinutes());
    var seconds = padZero(date.getSeconds());
    return year + "-" + month + "-" + day + " " + hours + ":" + minutes + ":" + seconds;
}

function getCookie(name)
{
    name = name + "=";
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++)
    {
        var cookie = cookies[i];
        while (cookie.charAt(0) == ' ')
            cookie = cookie.substring(1);
        if (cookie.indexOf(name) == 0)
            return cookie.substring(name.length, cookie.length);
    }
    return null;
}

function setCookie(name, value)
{
    var date = new Date();
    date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000));
    if (value)
        document.cookie = name + '=' + value + ';expires=' + date.toUTCString() + ';path=/';
    else
        deleteCookie(name);
}

function deleteCookie(name)
{
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
}