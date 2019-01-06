'use strict';
app.controller('homeController', ['$scope', 'authService', '$http', function ($scope, authService, $http) {
    $scope.authentication = authService.authentication;
    return $http.get('http://localhost:47039/api/protected').then(function (response) {
        $scope.externalSource = response.data;
    });
}]);