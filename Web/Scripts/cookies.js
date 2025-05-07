const CookieManager = {
    setCookie: function (name, value, minutes) {
        let expires = "";
        if (minutes) {
            const date = new Date();
            date.setTime(date.getTime() + (minutes * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },


    getCookie: function (name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },


    deleteCookie: function (name) {
        document.cookie = name + '=; Max-Age=-99999999; path=/';
    },


    setLanguage: function (lang) {
        this.setCookie('preferred_language', lang, 60 * 24 * 365); 
    },


    getLanguage: function () {
        return this.getCookie('preferred_language') || 'ro';
    },


    saveLastViewedCars: function (carIds) {
        this.setCookie('last_viewed_cars', JSON.stringify(carIds), 60 * 24 * 30); 
    },


    getLastViewedCars: function () {
        const cars = this.getCookie('last_viewed_cars');
        return cars ? JSON.parse(cars) : [];
    },


    saveFilterPreferences: function (filters) {
        this.setCookie('filter_preferences', JSON.stringify(filters), 60 * 24 * 30); 
    },

    getFilterPreferences: function () {
        const filters = this.getCookie('filter_preferences');
        return filters ? JSON.parse(filters) : {};
    },

    savePassword: function (password) {
        this.setCookie('user_password', password, 60 * 24 * 30); 
    },

    getPassword: function () {
        return this.getCookie('user_password');
    },

    saveUserSession: function (userData) {
        this.setCookie('user_session', JSON.stringify(userData), 60); 
    },

    getUserSession: function () {
        const session = this.getCookie('user_session');
        return session ? JSON.parse(session) : null;
    },

    clearUserSession: function () {
        this.deleteCookie('user_session');
    },

    saveLastUser: function (userData) {
        this.setCookie('last_user', JSON.stringify(userData), 60 * 24 * 365); 
    },

    getLastUser: function () {
        const user = this.getCookie('last_user');
        return user ? JSON.parse(user) : null;
    }
}; 