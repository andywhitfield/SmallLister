function smlInitialise() {
    smlInitialiseNav();

    let smlList = $('ul.sml-list');
    let baseUri = smlList.attr('data-baseuri');
    if (typeof baseUri !== 'undefined' && baseUri !== '') {
        Sortable.create(smlList.get(0), {
            handle: '.sml-list-item-drag-handle',
            animation: 150,
            ghostClass: 'sml-list-drag-ghost',
            onEnd: function (evt) {
                let item = $(evt.item);
                let listItemMoved = item.attr('data-listitem');
                if (typeof listItemMoved !== 'undefined') {
                    let prevListItem = parseInt(item.prev('li').attr('data-listitem'));

                    console.log('moving item ' + listItemMoved + ' to be after ' + prevListItem);
                    $.ajax({
                        url: baseUri + '/' + listItemMoved + '/move',
                        type: 'PUT',
                        data: JSON.stringify({ sortOrderPreviousListItemId: prevListItem == NaN ? null : prevListItem }),
                        contentType: 'application/json; charset=utf-8',
                        dataType: 'json'
                    });
                }
            }
        });
    }

    $('.sml-log-details-oc').click(function() {
        $(this).parent('div').next().find('.sml-log-details').toggle();
    });

    $('[data-depends]').each(function() {
        let btnWithDependency = $(this);
        let dependentFormObject = $(btnWithDependency.attr('data-depends'));
        if (dependentFormObject.is('input')) {
            dependentFormObject.on('keypress', function(e) {
                if (btnWithDependency.attr('disabled') && (e.keyCode || e.which) === 13) {
                    e.preventDefault();
                    return false;
                }
            });
        }
        dependentFormObject.on('change input paste keyup pickmeup-change', function() {
            let dependentValue = $(this).val();
            btnWithDependency.prop('disabled', dependentValue === null || dependentValue.match(/^\s*$/) !== null);
        });
        dependentFormObject.trigger('change');
    });

    $('[data-confirm]').click(function(event) {
        if (!confirm($(this).attr('data-confirm'))) {
            event.preventDefault();
            return false;
        }
    });
}

function smlInitialiseNav() {
    $(window).resize(function() {
        $('aside').css('display', '');
        if ($('.nav-close:visible').length > 0) {
            $('.nav-close').css('display', '');
            $('.nav-show').css('display', '');
        }
    });
    $('.nav-show').click(function() {
        $('aside').fadeToggle('fast');
        $(this).hide();
        $('.nav-close').show();
    });
    $('.nav-close').click(function() {
        $('aside').hide();
        $(this).hide();
        $('.nav-show').show();
    });
    $('[data-href]').click(function(e) {
        window.location.href = $(this).attr('data-href');
        e.preventDefault();
        return false;
    });
}

function smlOnAddToListChange() {
    let addToListSelect = $('form[name="sml-list-add"] select.sml-list-add-list');
    let addToListSelectNew = $('form[name="sml-list-add"] input.sml-list-add-list-new');
    console.log('selected list: ' + addToListSelect.val());
    if (addToListSelect.val() === 'new') {
        addToListSelectNew.parent().show();
        addToListSelectNew.select();
        addToListSelectNew.focus();
    } else {
        addToListSelectNew.parent().hide();
    }
}

coerceToArrayBuffer = function (thing, name) {
    if (typeof thing === "string") {
        // base64url to base64
        thing = thing.replace(/-/g, "+").replace(/_/g, "/");

        // base64 to Uint8Array
        var str = window.atob(thing);
        var bytes = new Uint8Array(str.length);
        for (var i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }
        thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
        thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
        throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
    }

    return thing;
};

coerceToBase64Url = function (thing) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
        thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
        thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
        var str = "";
        var len = thing.byteLength;

        for (var i = 0; i < len; i++) {
            str += String.fromCharCode(thing[i]);
        }
        thing = window.btoa(str);
    }

    if (typeof thing !== "string") {
        throw new Error("could not coerce to string");
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

    return thing;
};
