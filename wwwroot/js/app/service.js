var appService = angular.module("stellarService", []);

appService.service("stellarApi", ["$http", "$q", function ($http, $q) {
    var getUrl = function (path, version) {
        var url = "/api/";
        if (version) {
            url += version + "/";
        }
        else {
            url += "v1/";
        }
        url += path;
        return url;
    }
    return {
        url: function (path, version) {
            return getUrl(path, version);
        },
        get: function (path, version) {
            var deferred = $q.defer();

            var url = getUrl(path, version);
            $http.get(url).then(
                function (response) {
                    deferred.resolve(response.data);
                },
                function (error) {
                    deferred.reject(error);
                }
            )

            return deferred.promise;
        },
        post: function (path, data, version) {
            var deferred = $q.defer();

            var url = getUrl(path, version);


            $http.post(url, data).then(
                function (response) {
                    deferred.resolve(response.data);
                },
                function (error) {
                    deferred.reject(error);
                }
            )


            return deferred.promise;
        },
        put: function (path, data, version) {
            var deferred = $q.defer();

            var url = getUrl(path, version);


            $http.put(url, data).then(
                function (response) {
                    deferred.resolve(response.data);
                },
                function (error) {
                    deferred.reject(error);
                }
            )


            return deferred.promise;
        },
        delete: function (path, version) {
            var deferred = $q.defer();

            var url = getUrl(path, version);
            $http.delete(url).then(
                function (response) {
                    deferred.resolve(response.data);
                },
                function (error) {
                    deferred.reject(error);
                }
            )

            return deferred.promise;
        }
    }
}]);

appService.service("planetService", ["$http", "$q", "stellarApi", function ($http, $q, stellarApi) {
    const planetImagesCount = 7;
    return {
        getPlanets: function () {
            return stellarApi.get("planet");
        },
        getMyPlanets: function () {
            return stellarApi.get("planet/mine");
        },
        getPlanet: function (planetId) {
            return stellarApi.get("planet/" + planetId);
        },
        getBuildingOptions: function (planetId) {
            return stellarApi.get("planet/" + planetId + "/building/options")
        },
        startConstruction: function (planetId, buildingName) {
            return stellarApi.post("planet/" + planetId + "/building", '"' + buildingName + '"' );
        },
        getPlanetImgUrl: function (planet) {
            if (!planet) {
                return "";
            }
            var seed = planet.id + 1;
            var x = Math.sin(seed++) * (planetImagesCount -1);
            if (x < 0) {
                x = x * -1;
            }
            x = Math.floor(x+1);
            return "/img/planet/p" + x + ".jpeg";
        },
        getPlanetImgStyle: function (planet) {
            if (!planet) {
                return "";
            }
            var seed = planet.id + 1;
            var x = Math.sin(seed++) * 360;
            if (x < 0) {
                x = x * -1;
            }
            x = Math.floor(x + 1);
            return "filter: saturate(200%) brightness(100%) hue-rotate(" + x + "deg);"
        }
    }
}])

appService.service("buildingService", ["$http", "$q", "stellarApi", function ($http, $q, stellarApi) {
    return {
        cancelConstruction: function (buildingId) {
            return stellarApi.get("building/" + buildingId + "/cancel");
        }
    }
}])