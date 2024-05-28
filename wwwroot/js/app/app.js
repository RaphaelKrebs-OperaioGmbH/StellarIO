var app = angular.module("stellarIO", ["stellarService"]);



app.controller("dashboardController", ["$scope", "planetService", function ($scope, planetService) {
    planetService.getMyPlanets().then(function (myPlanets) {
        $scope.myPlanets = myPlanets;
    }, function (error) {

    });

    $scope.getPlanetImgStyle = function (planet) {
        return planetService.getPlanetImgStyle(planet);
    }
    $scope.getPlanetImgSrc = function (planet) {
        return planetService.getPlanetImgUrl(planet);
    }

    $scope.getBuildingImgSrc = function (building) {
        return "/img/building/" + building.name.replaceAll(" ", "_") + ".jpeg";
    }

    $scope.selectPlanet = function (planet) {
        $scope.selectedPlanet = planet;
    }
}]);

app.directive("sciNumber", [function () {
    const steps = ["k", "M", "G", "T", "P"];
    return {
        restrict: "E",
        scope: {
            value: "="
        },
        template: '<span ng-bind="getNum()"></span>',
        link: function (scope, elem, attrs) {
            scope.getNum = function () {
                var value = scope.value;
                var sign = "";
                for (var i = 0; i < steps.length; i++) {
                    if (value > 1000) {
                        value = value / 1000;
                        sign = steps[i];
                    }
                }
                if (sign === "") {
                    return value.toString();
                }
                return value.toFixed(2) + sign;
            }
        }
    }
}])