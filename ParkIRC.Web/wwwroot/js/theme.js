// Theme settings
$(document).ready(function() {
    // Initialize theme
    initializeTheme();

    // Handle theme change
    $('.theme-toggle').click(function() {
        toggleTheme();
    });
});

function initializeTheme() {
    // Check if theme preference exists in localStorage
    const theme = localStorage.getItem('theme') || 'light';
    setTheme(theme);
}

function setTheme(theme) {
    // Add or remove theme classes
    $('body').removeClass('theme-light theme-dark')
              .addClass(`theme-${theme}`);
    
    // Update theme toggle icon
    $('.theme-toggle i').removeClass('fa-sun fa-moon')
                        .addClass(theme === 'dark' ? 'fa-sun' : 'fa-moon');
    
    // Save theme preference
    localStorage.setItem('theme', theme);
}

function toggleTheme() {
    const currentTheme = $('body').hasClass('theme-dark') ? 'light' : 'dark';
    setTheme(currentTheme);
}
