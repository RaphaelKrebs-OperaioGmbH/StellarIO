var app = angular.module("stellarIO", ["stellarService"]);


app.controller("galaxyController", ["$scope", "galaxyService", "planetService", function ($scope, galaxyController, planetService) {
    $scope.getPlanetImgStyle = function (planet) {
        return planetService.getPlanetImgStyle(planet);
    }
    $scope.getPlanetImgSrc = function (planet) {
        return planetService.getPlanetImgUrl(planet);
    }
}]);

app.controller("dashboardController", ["$scope", "$interval", "planetService", "buildingService", function ($scope, $interval, planetService, buildingService) {
    
    function loadPlanets(selectedPlanetId) {
        planetService.getMyPlanets().then(function (myPlanets) {
            $scope.myPlanets = myPlanets;
            if (selectedPlanetId) {
                for (var i = 0; i < $scope.myPlanets.length; i++) {
                    if ($scope.myPlanets[i].id === selectedPlanetId) {
                        $scope.selectPlanet($scope.myPlanets[i]);
                        break;
                    }
                }
            }
        }, function (error) {

        });
    }

    function updateResources() {
        planetService.getMyPlanets().then(function (myPlanets) {
            for (var i = 0; i < myPlanets.length; i++) {
                var resources = myPlanets[i];
                for (var j = 0; j < $scope.myPlanets; j++) {
                    var planet = $scope.myPlanets[j];
                    if (resources.id === planet.id) {
                        updatePlanetResources(planet, resources);
                        break;
                    }
                }
            }
        })
    }

    function updatePlanetResources(planet, resources) {
        planet.iron = resources.iron;
        planet.silver = resources.silver;
        planet.aluminium = resources.aluminium;
        planet.h2 = resources.h2;
        planet.energy = resources.energy;
    }

    var resourcesInterval = $interval(updateResources, 15000);
    $scope.$on("$destroy", function () {
        $interval.cancel(resourcesInterval);
    })

    loadPlanets();

    $scope.getPlanetImgStyle = function (planet) {
        return planetService.getPlanetImgStyle(planet);
    }
    $scope.getPlanetImgSrc = function (planet) {
        return planetService.getPlanetImgUrl(planet);
    }

    $scope.getBuildingImgSrc = function (building) {
        if (!building) {
            return "";
        }
        return "/img/building/" + building.name.replaceAll(" ", "_") + ".jpeg";
    }

    $scope.selectPlanet = function (planet) {
        $scope.selectedPlanet = planet;
        $scope.currentBuildingOptions = null;
        if (planet) {
            planetService.getBuildingOptions(planet.id).then(function (options) {
                $scope.currentBuildingOptions = options;
            })
        }
    }

    $scope.isInProgress = function (planet, buildingName) {
        var building = $scope.getBuildingByName(planet, buildingName);
        if (!building || !building.constructionEndTime) {
            return false;
        }
        var endDate = new Date(building.constructionEndTime + "Z");
        return endDate - new Date() >= 0;
    }

    $scope.isAnyInProgress = function (planet) {
        if (!planet || !planet.buildings) {
            return false;
        }
        for (var i = 0; i < planet.buildings.length; i++) {
            var building = planet.buildings[i];
            if (!building.constructionEndTime) {
                continue;
            }
            var endDate = new Date(building.constructionEndTime + "Z");
            var isInProgress = endDate - new Date() >= 0;
            if (isInProgress) {
                return true;
            }
        }
        return false;
    }

    $scope.getBuildingByName = function (planet, buildingName) {
        if (!planet || !planet.buildings || !buildingName) {
            return null;
        }
        for (var i = 0; i < planet.buildings.length; i++) {
            var building = planet.buildings[i];
            if (building.name === buildingName) {
                return building;
            }
        }
    }

    $scope.selectBuildingOption = function (option) {
        $scope.selectedBuildingOption = option;
    }

    $scope.startConstruction = function (planet, buildingOption) {
        planetService.startConstruction(planet.id, buildingOption.name).then(function (ok) {
            //tjoa.. wenn ok... dann was?
            loadPlanets($scope.selectedPlanet.id);
        }, function (error) {

        });
    }

    $scope.stopConstruction = function (planet, buildingOption) {
        var building = $scope.getBuildingByName(planet, buildingOption.name);
        if (!building) {
            return;
        }
        buildingService.cancelConstruction(building.id).then(function (ok) {
            loadPlanets($scope.selectedPlanet.id);
        }, function (error) {

        })
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
                if (!value) {
                    return "";
                }
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
    };
}]);

app.directive("timeDuration", [function () {
    const SECONDSPERDAY = 86400;
    const SECONDSPERHOUR = 3600;
    const SECONDSPERMINUTE = 60;
    function getTimeSpanString(secondsRemaining) {
        var daysRemaining = 0;
        var hoursRemaining = 0;
        var minutesRemaining = 0;
        while (secondsRemaining >= SECONDSPERDAY) {
            daysRemaining += 1;
            secondsRemaining -= SECONDSPERDAY;
        }
        while (secondsRemaining >= SECONDSPERHOUR) {
            hoursRemaining += 1;
            secondsRemaining -= SECONDSPERHOUR;
        }
        while (secondsRemaining >= SECONDSPERMINUTE) {
            minutesRemaining += 1;
            secondsRemaining -= SECONDSPERMINUTE;
        }
        if (daysRemaining > 0) {
            return "" + daysRemaining + "d " + hoursRemaining + "h " + minutesRemaining + "m " + secondsRemaining + "s";
        }
        if (hoursRemaining > 0) {
            return "" + hoursRemaining + "h " + minutesRemaining + "m " + secondsRemaining + "s";
        }
        if (minutesRemaining > 0) {
            return "" + minutesRemaining + "m " + secondsRemaining + "s";
        }
        return "" + secondsRemaining + "s";
    }
    return {
        restrict: "E",
        scope: {
            seconds: "="
        },
        template: "<span>{{timeDisplay()}}</span>",
        link: function (scope, elem, attrs) {
            scope.timeDisplay = function () {
                try {
                    var s = parseInt(scope.seconds)
                    if (s < 0) {
                        return "";
                    }
                    return getTimeSpanString(s);
                }
                catch {
                    return "";
                }
            }
        }
    }
}])

app.directive("timeCountdown", ["$interval", function ($interval) {
    function parseDate(dateString) {
        if (!dateString) {
            return null;
        }
        return new Date(dateString + "Z");
    }
    function getSecondsRemaining(date) {
        return parseInt((date - new Date()) / 1000);
    }

    return {
        restrict: "E",
        scope: {
            endTime: "="
        },
        template: '<time-duration seconds="secondsRemaining"></time-duration>',
        link: function (scope, elem, attrs) {
            scope.secondsRemaining = -1;
            scope.properlyStarted = false;

            scope.tick = function () {
                var endTimeDate = parseDate(scope.endTime)
                if (!endTimeDate) {
                    scope.secondsRemaining = -1;
                    scope.stop();
                    return;
                }
                scope.secondsRemaining = getSecondsRemaining(endTimeDate);
                if (scope.secondsRemaining < 0) {
                    if (scope.properlyStarted) {
                        // fire event that time ran out here
                    }
                    scope.stop();
                    return;
                }
                scope.properlyStarted = true;
            }

            scope.intervalPromise = null;

            scope.stop = function () {
                scope.properlyStarted = false;
                if (scope.intervalPromise) {
                    $interval.cancel(scope.intervalPromise);
                }
            };

            scope.start = function () {
                scope.stop();
                scope.intervalPromise = $interval(scope.tick, 1000);
                scope.tick();
            };
            scope.$on('$destroy', function () {
                scope.stop();
            });

            scope.$watch('endTime', function (newValue, oldValue) {
                scope.start();
            })

            scope.start();
        }
    };
}]);

app.directive("timeCountup", ["$interval", function ($interval) {
    return {
        restrict: "E",
        scope: {
            seconds: "="
        },
        template: '<span>{{dateString}}</span>',
        link: function (scope, elem, attrs) {
            scope.dateString = "";

            scope.tick = function () {
                try {
                    var s = parseInt(scope.seconds);
                    var now = new Date();
                    now.setSeconds(now.getSeconds() + s);
                    scope.dateString = now.toLocaleString();
                }
                catch {
                    scope.dateString = "";
                }
            }
            

            scope.stop = function () {
                if (scope.intervalPromise) {
                    $interval.cancel(scope.intervalPromise);
                }
            };

            scope.start = function () {
                scope.stop();
                scope.intervalPromise = $interval(scope.tick, 1000);
                scope.tick();
            };
            scope.$on('$destroy', function () {
                scope.stop();
            });

            scope.start();
        }
    };
}]);