(function () {
    'use strict';

    angular.module('ShellRepoApp')
        .service('ShellsService', ['$q', '$http', ShellsService]);

    function ShellsService($q, $http) {

        // Promise-based API
        return {

            getAllShells: function () {
                return $http.get('api/shells');
            }

        };
    }

})();