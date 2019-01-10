'use strict';
app.controller('homeController', ['$scope', 'authService', '$http', 'externalResource', function ($scope, authService, $http, externalResource) {
    $scope.authentication = authService.authentication;
    return $http.get(externalResource).then(function (response) {
        $scope.externalSource = response.data;
    });
}]);