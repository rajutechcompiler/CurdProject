
/*
jquery-resizable
Version 0.32 - 5/5/2018
© 2015-2018 Rick Strahl, West Wind Technologies
www.west-wind.com
Licensed under MIT License
*/
(function(factory, undefined) {
	if (typeof define === 'function' && define.amd) {
		// AMD
		define(['jquery'], factory);
	} else if (typeof module === 'object' && typeof module.exports === 'object') {
		// CommonJS
		module.exports = factory(require('jquery'));
	} else {
		// Global jQuery
		factory(jQuery);
	}
}(function($, undefined) {
    
    if ($.fn.resizable)
        return;

    $.fn.resizable = function fnResizable(options) {
        var defaultOptions = {
            // selector for handle that starts dragging
            handleSelector: null,
            // resize the width
            resizeWidth: true,
            // resize the height
            resizeHeight: true,
            // the side that the width resizing is relative to
            resizeWidthFrom: 'right',
            // the side that the height resizing is relative to
            resizeHeightFrom: 'bottom',
            // hook into start drag operation (event passed)
            onDragStart: null,
            // hook into stop drag operation (event passed)
            onDragEnd: null,
            // hook into each drag operation (event passed)
            onDrag: null,
            // disable touch-action on $handle
            // prevents browser level actions like forward back gestures
            touchActionNone: true,
            // instance id
            instanceId: null
    };
        if (typeof options === "object")
            defaultOptions = $.extend(defaultOptions, options);

        return this.each(function () {
            var opt = $.extend({}, defaultOptions);
            if (!opt.instanceId)
                opt.instanceId = "rsz_" + new Date().getTime();            

            var startPos, startTransition;

            // get the element to resize 
            var $el = $(this);
            var $handle;

            if (options === 'destroy') {            
                opt = $el.data('resizable');
                if (!opt)
                    return;

                $handle = getHandle(opt.handleSelector, $el);
                $handle.off("mousedown." + opt.instanceId + " touchstart." + opt.instanceId);
                if (opt.touchActionNone)
                    $handle.css("touch-action", "");
                $el.removeClass("resizable");
                return;
            }
          
            $el.data('resizable', opt);

            // get the drag handle

            $handle = getHandle(opt.handleSelector, $el);

            if (opt.touchActionNone)
                $handle.css("touch-action", "none");

            $el.addClass("resizable");
            $handle.on("mousedown." + opt.instanceId + " touchstart." + opt.instanceId, startDragging);

            function noop(e) {
                e.stopPropagation();
                e.preventDefault();
            }

            function startDragging(e) {
                // Prevent dragging a ghost image in HTML5 / Firefox and maybe others    
                if ( e.preventDefault ) {
                  e.preventDefault();
                }
                
                startPos = getMousePos(e);
                startPos.width = parseInt($el.width(), 10);
                startPos.height = parseInt($el.height(), 10);

                startTransition = $el.css("transition");
                $el.css("transition", "none");

                if (opt.onDragStart) {
                    if (opt.onDragStart(e, $el, opt) === false)
                        return;
                }
                
                $(document).on('mousemove.' + opt.instanceId, doDrag);
                $(document).on('mouseup.' + opt.instanceId, stopDragging);
                if (window.Touch || navigator.maxTouchPoints) {
                    $(document).on('touchmove.' + opt.instanceId, doDrag);
                    $(document).on('touchend.' + opt.instanceId, stopDragging);
                }
                $(document).on('selectstart.' + opt.instanceId, noop); // disable selection
                $("iframe").css("pointer-events","none");
            }

            function doDrag(e) {
                
                var pos = getMousePos(e), newWidth, newHeight;

                if (opt.resizeWidthFrom === 'left')
                    newWidth = startPos.width - pos.x + startPos.x;
                else
                    newWidth = startPos.width + pos.x - startPos.x;

                if (opt.resizeHeightFrom === 'top')
                    newHeight = startPos.height - pos.y + startPos.y;
                else
                    newHeight = startPos.height + pos.y - startPos.y;

                if (!opt.onDrag || opt.onDrag(e, $el, newWidth, newHeight, opt) !== false) {
                    if (opt.resizeHeight)
                        $el.height(newHeight);                    

                    if (opt.resizeWidth)
                        $el.width(newWidth);                    
                }
            }

            function stopDragging(e) {
                e.stopPropagation();
                e.preventDefault();

                $(document).off('mousemove.' + opt.instanceId);
                $(document).off('mouseup.' + opt.instanceId);

                if (window.Touch || navigator.maxTouchPoints) {
                    $(document).off('touchmove.' + opt.instanceId);
                    $(document).off('touchend.' + opt.instanceId);
                }
                $(document).off('selectstart.' + opt.instanceId, noop);                

                // reset changed values
                $el.css("transition", startTransition);
                $("iframe").css("pointer-events","auto");

                if (opt.onDragEnd)
                    opt.onDragEnd(e, $el, opt);

                return false;
            }

            function getMousePos(e) {
                var pos = { x: 0, y: 0, width: 0, height: 0 };
                if (typeof e.clientX === "number") {
                    pos.x = e.clientX;
                    pos.y = e.clientY;
                } else if (e.originalEvent.touches) {
                    pos.x = e.originalEvent.touches[0].clientX;
                    pos.y = e.originalEvent.touches[0].clientY;
                } else
                    return null;

                return pos;
            }

            function getHandle(selector, $el) {
                if (selector && selector.trim()[0] === ">") {
                    selector = selector.trim().replace(/^>\s*/, "");
                    return $el.find(selector);
                }

                // Search for the selector, but only in the parent element to limit the scope
                // This works for multiple objects on a page (using .class syntax most likely)
                // as long as each has a separate parent container. 
                return selector ? $el.parent().find(selector) : $el;
            } 
        });
    };
}));

(function ($) {
    $.fn.confirmModal = function (opts) {
        var body = $('body');
        var defaultOptions = {
        //Modified by Hemin
        //confirmTitle: 'Please confirm',
        // confirmMessage: 'Are you sure you want to perform this action ?',
        //confirmOk: 'Yes',
        //confirmCancel: 'Cancel',
            confirmTitle: vrCommonRes["tiPleaseConfirm"],
            confirmMessage: vrCommonRes["msgAreUSureUWant2PerformThisAction"],
            confirmOk: vrCommonRes["Yes"],
            confirmCancel: vrCommonRes["Cancel"],
            confirmDirection: 'rtl',
            confirmStyle: 'warning',
            confirmCallback: defaultCallback,
            confirmCallbackCancel: defaultCancel,
            confirmOnlyOk:false,
            confirmObject:null
        };
        var options = $.extend(defaultOptions, opts);
        var time = Date.now();

        var buttonTemplate =
            '<button class="btn btn-primary" id="okButton" data-dismiss="ok" >' + options.confirmOk + '</button>' +
            '<button class="btn btn-' + options.confirmStyle + '"  aria-hidden="true" id="cancelButton">' + options.confirmCancel + '</button>';
        if (options.confirmDirection == 'ltr') {
            buttonTemplate =
                '<button class="btn btn-' + options.confirmStyle + '" id="okButton" data-dismiss="ok" >' + options.confirmOk +'</button>' +
                '<button class="btn btn-primary" aria-hidden="true" id="cancelButton">' + options.confirmCancel + '</button>';
        }
        if (options.confirmOnlyOk == true) {
            buttonTemplate =
                '<button class="btn btn-primary" id="okOnly" data-dismiss="ok" >' + options.confirmOk + '</button>';
        }

        var confirmModal =
                       $('<div class="modal fade" id="confirm-delete" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
                           '<div class="modal-dialog">' +
                               '<div class="modal-content">' +
                                   '<div class="modal-header">' +
                                       '<h3>' + options.confirmTitle + '</h3>' +
                                   '</div>' +
                                   '<div class="modal-body">' +
                                       '<p style="word-wrap:break-word">' + options.confirmMessage + '</p>' +
                                   '</div>' +
                                   '<div class="modal-footer">' +
                                       buttonTemplate +
                                   '</div>' +
                               '</div>' +
                           '</div>' +
                       '</div>');



        confirmModal.find('#okButton').click(function (event) {
            options.confirmCallback(options.confirmObject);
            confirmModal.modal('hide');
        });

        confirmModal.find('#cancelButton').click(function (event) {
            options.confirmCallbackCancel(options.confirmObject);
            confirmModal.modal('hide');
        });
        confirmModal.find('#okOnly').click(function (event) {
            options.confirmCallback(options.confirmObject);
            confirmModal.modal('hide');
            //showAjaxReturnMessage(vrApplicationRes['msgJsRequestorRecSaveSuccessfully'], 's');
        });

        confirmModal.ShowModel();

        function defaultCallback() {

        }

        function defaultCancel() {

        }
    };
})(jQuery);


var vrApplicationRes = [];
var vrClientsRes = [];
var vrCommonRes = [];
var vrDataRes = [];
var vrDatabaseRes = [];
var vrDirectoriesRes = [];
var vrImportRes = [];
var vrLabelManagerRes = [];
var vrReportsRes = [];
var vrRetentionRes = [];
var vrScannerRes = [];
var vrSecurityRes = [];
var vrTablesRes = [];
var vrViewsRes = [];
var vrHTMLViewerRes = [];


function getResourcesByModule(prModuleName) {
    $.ajax({
        url: '/Resource/GetResources',
        type: 'GET',
        async: false, 
        data: { moduleName: prModuleName },
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            switch (prModuleName.toLowerCase()) {
                case 'application':
                    vrApplicationRes = response;
                    break;
                case 'clients':                    
                    vrClientsRes = response;
                    break;
                case 'common':
                    vrCommonRes = response;
                    break;
                case 'data':
                    vrDataRes = response;
                    break;
                case 'database':
                    vrDatabaseRes = response;
                    break;
                case 'directories':
                    vrDirectoriesRes = response;
                    break;
                case 'import':
                    vrImportRes = response;
                    break;
                case 'labelmanager':
                    vrLabelManagerRes = response;
                    break;
                case 'reports':
                    vrReportsRes = response;
                    break;
                case 'retention':
                    vrRetentionRes = response;
                    break;
                case 'scanner':
                    vrScannerRes = response;
                    break;
                case 'security':
                    vrSecurityRes = response;
                    break;
                case 'tables':                    
                    vrTablesRes = response;
                    break;
                case 'views':                    
                    vrViewsRes = response;
                    break;
                case 'htmlviewer':                    
                    vrHTMLViewerRes = response;
                    break;
                case 'all':
                    vrApplicationRes = response.resApp;
                    vrClientsRes = response.resClient;
                    vrCommonRes = response.resCommon;
                    vrDataRes = response.resData;
                    vrDatabaseRes = response.resDatabase;
                    vrDirectoriesRes = response.resDirectories;
                    vrImportRes = response.resImport;
                    vrLabelManagerRes = response.resLabelmanager;
                    vrReportsRes = response.resReports;
                    vrRetentionRes = response.resRetention;
                    vrScannerRes = response.resScanner;
                    vrSecurityRes = response.resSecurity;
                    vrTablesRes = response.resTables;
                    vrViewsRes = response.resViews;
                    vrHTMLViewerRes = response.resHTMLViewer;
                    break;
            }
        },
        error: function (xhr, status, error) {
        }
    });
}

function LoadErrorMessage(errorType, message, msgTitle) {

    var i = -1;
    var toastCount = 0;
    var $toastlast;
    var getMessage = '';
    var getMessageWithClearButton = function (msg) {
        msg = msg ? msg : 'Clear itself?';
        msg += '<br /><br /><button type="button" class="btn clear">Yes</button>';
        return msg;
    };

    var shortCutFunction = errorType;
    var msg = message;
    var title = msgTitle || '';
    var $showDuration = 300;
    var $hideDuration = 1000;
    var $timeOut = 5000;
    var $extendedTimeOut = 1000;
    var $showEasing = 'swing';
    var $hideEasing = 'linear';
    var $showMethod = 'fadeIn';
    var $hideMethod = 'fadeOut';
    var toastIndex = toastCount++;
    var addClear = false;

    toastr.options = {
        closeButton: false,
        debug: false,
        newestOnTop: false,
        progressBar: false,
        positionClass: 'toast-top-center',
        preventDuplicates: true,
        onclick: null
    };

    if ($showDuration.length) {
        toastr.options.showDuration = 10;
    }

    if ($hideDuration.length) {
        toastr.options.hideDuration = $hideDuration;
    }

    if ($timeOut.length) {
        toastr.options.timeOut = $timeOut;//addClear ? 0 : $timeOut;
    }

    if ($extendedTimeOut.length) {
        toastr.options.extendedTimeOut = $extendedTimeOut;// addClear ? 0 : $extendedTimeOut;
    }

    if ($showEasing.length) {
        toastr.options.showEasing = $showEasing;
    }

    if ($hideEasing.length) {
        toastr.options.hideEasing = $hideEasing;
    }

    if ($showMethod.length) {
        toastr.options.showMethod = $showMethod;
    }

    if ($hideMethod.length) {
        toastr.options.hideMethod = $hideMethod;
    }

    var $toast = toastr[shortCutFunction](msg, title); // Wire up an event handler to a button in the toast, if it exists
    $toastlast = $toast;

    if (typeof $toast === 'undefined' || $toast === null) {
        return;
    }

    if ($toast.find('#okBtn').length) {
        $toast.delegate('#okBtn', 'click', function () {
            $toast.remove();
        });
    }
    if ($toast.find('#surpriseBtn').length) {
        $toast.delegate('#surpriseBtn', 'click', function () {
            //alert('Surprise! you clicked me. i was toast #' + toastIndex + '. You could perform an action here.');
        });
    }
    if ($toast.find('.clear').length) {
        $toast.delegate('.clear', 'click', function () {
            toastr.clear($toast, { force: true });
        });
    }
    function getLastToast() {
        return $toastlast;
    }
}

function showAjaxReturnMessage(message, msgType) {
    var divId = '';
    var msgcls = '';
    var msgTitle = '';
    switch (msgType.toLowerCase()) {
        case 'warning': case 'w':
            msgcls = 'warning';
            msgTitle = vrCommonRes["Warning"];
            break;
        case 'error': case 'e':
            msgcls = 'error';
            msgTitle = vrCommonRes["msgError"];
            break;
        case 'success': case 's':
            msgcls = 'success';
            msgTitle = vrCommonRes["Success"];
            break;
        case 'info': case 'i':
            msgcls = 'info';
            msgTitle = vrCommonRes["Information"];
            break;
        case 'loading':
            divId = 'ajaxloading';
            break;
    }
    LoadErrorMessage(msgcls, message, msgTitle);
}

String.format = function () {
    // The string containing the format items (e.g. "{0}")
    // will and always has to be the first argument.
    var theString = arguments[0];

    // start with the second argument (i = 1)
    for (var i = 1; i < arguments.length; i++) {
        // "gm" = RegEx options for Global search (more than one instance)
        // and for Multiline search
        var regEx = new RegExp("\\{" + (i - 1) + "\\}", "gm");
        theString = theString.replace(regEx, arguments[i]);
    }
    return theString;
}

$.fn.ShowModel = function () {
    $(this).modal({ show: true, keyboard: false, backdrop: 'static' });
}
$.fn.HideModel = function () {
    $('body').removeClass("modal-open");
    $(this).modal('hide');
}