var app = angular.module("stellarIO", ["stellarService"]);



app.controller("dashboardController", ["$scope", "planetService", function ($scope, planetService) {
    planetService.getMyPlanets().then(function (myPlanets) {
        $scope.myPlanets = myPlanets;
    }, function (error) {

    });
}]);