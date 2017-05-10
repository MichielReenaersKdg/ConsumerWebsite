

(function ($) {
    "use strict";
    jQuery(window).ready(function () {

    
    $('#topNav').affix({
        offset: {
            top: 200
        }
    });





    
    
    new WOW().init();


    $('a.page-scroll').bind('click', function(event) {
        var $ele = $(this);
        if ($($ele.attr('href')).offset() !== undefined ) {
        $('html, body').stop().animate({
            scrollTop: ($($ele.attr('href')).offset().top - 60)
        }, 2950, 'easeInOutExpo');
        event.preventDefault();
        }
    });


        $('#hplogo').click(function(event) {
            $('#bs-navbar li').removeClass('active');

        });
    $('#bs-navbar li').bind('click', function (event) {

        $('#bs-navbar li').removeClass('active');
        var $ele = $(this);
        $ele.addClass('active');
    });
    $('.navbar-collapse ul li a').click(function() {
        /* always close responsive nav after click */
        $('.navbar-toggle:visible').click();
    });

    $('#galleryModal').on('show.bs.modal', function (e) {
       $('#galleryImage').attr("src",$(e.relatedTarget).data("src"));
    });


    });
   
    $(document).ready(function () {
        $(window).scroll(function () { // check if scroll event happened
            if ($(document).scrollTop() > 50) { // check if user scrolled more than 50 from top of the browser window
                $(".navbar-fixed-top").css("background-color", "#222"); // if yes, then change the color of class "navbar-fixed-top" to white (#f8f8f8)
            } else {
                $(".navbar-fixed-top").css("background-color", "transparent"); // if not, change it back to transparent
            }
        });
    });
   

})(jQuery);
