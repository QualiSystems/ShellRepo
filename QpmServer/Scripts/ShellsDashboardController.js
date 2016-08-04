(function () {
    'use strict';

    angular.module('ShellRepoApp')
        .controller('ShellsDashboardController', ['$rootScope', '$scope', 'ShellsService', ShellsDashboardController]);

    function ShellsDashboardController($rootScope, $scope, shellsService) {

        var self = this;

        self.shells = undefined;

        shellsService.getAllShells().then(function (response) {
            self.shells = response.data;
        });
    }

})();