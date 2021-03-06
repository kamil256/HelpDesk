﻿function moveMessagesToTopUnderNavBar()
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
    setTimeout(function() { $(alert).hide('slow') }, 20000);
}

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

var numberOfSentAjaxRequests = 0;

function sendAjaxRequest(url, method, data, onSuccess, ngHttpService, ngTimerService)
{
    ngTimerService.reset();
    
    showProgressIndicator();
    ngHttpService(
    {
        url: url,
        method: method,
        params: data
    }).then(function(response)
    {
        if (response.data.Success)
            displayNewSuccessMessage(response.data.Success);
        if (response.data.Fail)
            displayNewFailMessage(response.data.Fail);

        onSuccess(response.data);

        hideProgressIndicator();
    }, function()
    {
        displayNewFailMessage("Problem with connection. Try again, and if the problem persists contact your system administrator.");
        hideProgressIndicator();
    });
}

function padZero(value)
{
    return value < 10 ? "0" + value : value;
}

function formatDate(ticks)
{
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

function split(text, separator)
{
    var fragments = [];
    var separators = [];
    var nextFragmentIndex = 0;
    var nextSeparatorIndex = text.toLowerCase().indexOf(separator.toLowerCase());
    while (nextSeparatorIndex != -1)
    {
        fragments.push(text.substr(nextFragmentIndex, nextSeparatorIndex - nextFragmentIndex));
        separators.push(text.substr(nextSeparatorIndex, separator.length));
        nextFragmentIndex = nextSeparatorIndex + separator.length;
        nextSeparatorIndex = text.toLowerCase().indexOf(separator.toLowerCase(), nextSeparatorIndex + separator.length);
    }
    fragments.push(text.substr(nextFragmentIndex, text.length - nextFragmentIndex));
    return { fragments: fragments, separators: separators };
}

function join(fragments, separators)
{
    var text = '';
    for (var i = 0; i < separators.length; i++)
    {
        text += fragments[i];
        text += separators[i];
    }
    text += fragments[i];
    return text;
}

function mark(text, words)
{
    if (words.length > 0)
    {
        var obj = split(text, words[0]);
        for (var i = 0; i < obj.fragments.length; i++)
            obj.fragments[i] = mark(obj.fragments[i], words.slice(1, words.length));
        for (var i = 0; i < obj.separators.length; i++)
            obj.separators[i] = '<mark>' + obj.separators[i] + '</mark>';
        return join(obj.fragments, obj.separators);
    }
    else
        return text;
}

function removeExcessSpaces(text)
{
    if (text)
        return text.trim().replace(/\s\s+/g, ' ');
    else
        return text;
}