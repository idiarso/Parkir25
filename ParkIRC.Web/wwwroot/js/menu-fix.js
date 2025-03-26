/**
 * Menu Navigation Fix
 * Enhances the navigation menu functionality in PARKIR_WEB application
 */
(function($) {
    "use strict";

    $(document).ready(function() {
        // Sidebar toggle functionality
        $("#sidebar-toggle").on("click", function(e) {
            e.preventDefault();
            $(".parkiRC-sidebar").toggleClass("collapsed");
            $(".parkiRC-content").toggleClass("expanded");
        });

        // Add active class to current menu item
        const currentPath = window.location.pathname;
        
        // Handle submenu items
        $(".parkiRC-sidebar__menu__item.has-children > .parkiRC-sidebar__menu__link").on("click", function(e) {
            const $parent = $(this).parent();
            
            if ($parent.hasClass("active")) {
                $parent.removeClass("active");
                $(this).siblings(".parkiRC-sidebar__submenu").slideUp();
            } else {
                // Close other open submenus
                $(".parkiRC-sidebar__menu__item.has-children").removeClass("active");
                $(".parkiRC-sidebar__submenu").slideUp();
                
                $parent.addClass("active");
                $(this).siblings(".parkiRC-sidebar__submenu").slideDown();
            }
            
            // Prevent navigation if it's a submenu toggle
            e.preventDefault();
        });

        // Find active menu item based on current path
        $('.parkiRC-sidebar__menu__link, .parkiRC-sidebar__submenu .parkiRC-sidebar__menu__link').each(function() {
            const menuLink = $(this).attr('href');
            
            if (menuLink && currentPath.indexOf(menuLink) > -1) {
                $(this).addClass('active');
                
                // If it's a submenu item, also highlight parent
                if ($(this).closest('.parkiRC-sidebar__submenu').length) {
                    $(this).closest('.parkiRC-sidebar__menu__item.has-children')
                           .addClass('active');
                }
            }
        });

        // Close sidebar when clicking outside
        $(document).on("click", function(e) {
            if (!$(e.target).closest(".parkiRC-sidebar").length && 
                !$(e.target).closest("#sidebar-toggle").length &&
                $(".parkiRC-sidebar").hasClass("collapsed")) {
                $(".parkiRC-sidebar").removeClass("collapsed");
                $(".parkiRC-content").removeClass("expanded");
            }
        });
    });

})(jQuery);