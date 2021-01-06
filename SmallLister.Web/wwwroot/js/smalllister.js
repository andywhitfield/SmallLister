function smlInitialise() {
    smlInitialiseNav();

    let smlList = $('ul.sml-list');
    let baseUri = smlList.attr('data-baseuri');
    if (typeof baseUri !== 'undefined' && baseUri !== '') {
        smlList.sortable({
            handle: '.sml-list-item-drag-handle',
            isValidTarget: function(item, container) {
                return container.el[0].className.includes('sml-list-item');
            },
            onDrop: function(item, container, _super, event) {
                _super(item, container, event);

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

                    return;
                }
            }
        });
    }

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
    $('[data-href]').click(function() {
        window.location.href = $(this).attr('data-href');
    });
}