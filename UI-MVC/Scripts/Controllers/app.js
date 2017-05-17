
var app = angular.module('sussol', ['ngRoute', 'ngMessages',"angucomplete-alt", 'sussol.constants', 'sussol.services', 'sussol.controllers']);

app.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when("/", {
        controller: 'homeController',
        templateUrl: "Content/Views/Home.html",
        authenticate: false
    });
    $routeProvider.when("/register", {
        templateUrl: "Content/Views/Account/Register.html",
        authenticate: false
    });
    $routeProvider.when("/login", {
        templateUrl: "Content/Views/Account/Login.html",
        authenticate: false
    });
    $routeProvider.when("/account/admin", {
        controller: 'AdminController',
        templateUrl: "Content/Views/Account/Admin.html",
        authenticate: true,
        resolve: {
            result: function($route, srvLibrary) {
                return srvLibrary.getAdminInfo();
            }
    }
        
    });
    $routeProvider.when("/account/:id", { 
        templateUrl: "Content/Views/Account/Home.html",
        controller: 'AccountHomeController as account',
        authenticate: true,
        resolve: {
            result: function ($route, srvLibrary) {
                return srvLibrary.getUserInfo($route.current.params.id);
            },
            organisationsResult: function($route, srvLibrary) {
                return srvLibrary.readOrganisationForUser($route.current.params.id);
            },
            analysesResult: function($route, srvLibrary) {
                return srvLibrary.readAnalyses($route.current.params.id);
            }
        }
    });
    $routeProvider.when("/organisation/create", {
        templateUrl: "Content/Views/Organisation/Create.html", 
        authenticate: true
    });
    $routeProvider.when("/organisation/:id", {
        templateUrl: "Content/Views/Organisation/Home.html",
        controller: 'OrganisationController',
        authenticate: true,
        resolve: {
            result: function ($route, srvLibrary) {
                return srvLibrary.readOrganisation($route.current.params.id);
            },
            membersOrganisation: function($route, srvLibrary) {
                return srvLibrary.readUsersForOrganisation($route.current.params.id);
            },
            analysesOrganisation: function($route, srvLibrary) {
                return srvLibrary.readAnalysesForOrganisation($route.current.params.id);
            },
            organiserOrganisation: function($route, srvLibrary) {
                return srvLibrary.readOrganiser($route.current.params.id);
            }
        }
    });
    $routeProvider.when("/analysis/start", {
        templateUrl: "Content/Views/Analysis/Start.html",
        authenticate: true
    });
    $routeProvider.when("/analysis/overview/:id", {
        templateUrl: "Content/Views/Analysis/Overview.html",
        controller: 'AnalysisOverviewController as overview',
        authenticate: true,
        resolve: {
            result: function ($route, srvLibrary) {
                return srvLibrary.getSolventClusterResult($route.current.params.id);
            },
            organisation: function(srvLibrary) {
                return srvLibrary.readOrganisationForUser(window.sessionStorage.userId);
            },

        }
    });
    $routeProvider.when("/404", {
        templateUrl: "Content/Views/Error.html",
        authenticate: false
    });
    $routeProvider.otherwise({ redirectTo: "/" });

    $locationProvider.html5Mode(true);
});
app.config(['$compileProvider',
    function ($compileProvider) {
        $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|tel|file|blob):/);
    }]);
angular.module('sussol.services')
    .factory('srvLibrary', ['$http', '$location', function($http, $location) {
        var services = {
            getSolvents: function(){
                var promise = $http({
                    method: 'GET',
                    url: 'api/Analysis/GetSolvents',

                });
                promise.success(function (data, status, headers, conf) {
                    return data;
                });
                return promise;
            },
            getFeaturesList: function() {
                var promise = $http({
                    method: 'GET',
                    url: 'api/Analysis/GetFeatures',
                    
                });
                promise.success(function (data, status, headers, conf) {
                    return data;
                });
                return promise;
            },
                getSolventClusterResult: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Analysis/GetAnalysis',
                        params: { id: id }
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                getUserInfo: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Account/GetUserInfo',
                        params: { id: id }
                    }).error(function errorCallback(data) {
                        $location.path("/404");
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                getAdminInfo: function () {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Account/GetAllAdminInfo'

                    });
                    promise.success(function (data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readOrganisationForUser: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Organisation/ReadOrganisationForUser',
                        params: { id: id }
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readOrganisation: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Organisation/ReadOrganisation',
                        params: { id: id }
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readAnalyses: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Analysis/GetAnalysesForUser',
                        params: { id: id }
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readAnalysesForOrganisation: function(id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Organisation/GetAnalysesForOrganisation',
                        params: { id: id }
                    });
                    promise.success(function(data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readUsersForOrganisation: function (id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Organisation/GetUsersForOrganisation',
                        params: { id: id }
                    });
                    promise.success(function (data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                },
                readOrganiser: function (id) {
                    var promise = $http({
                        method: 'GET',
                        url: 'api/Organisation/ReadOrganiser',
                        params: { id: id }
                    });
                    promise.success(function (data, status, headers, conf) {
                        return data;
                    });
                    return promise;
                }


            }
            return services;
        }
    ]);


angular.module('sussol.controllers')
    .controller('homeController', 
    function ($timeout, $rootScope, $scope, $window) {
        
        $rootScope.footer = false;
        
        if ($window.sessionStorage.scrollDiv !== undefined) {
            $(document).ready(function () {
            $('html, body').stop().animate({
                    scrollTop: ($($window.sessionStorage.scrollDiv).offset().top - 30)
            }, 2950, 'easeInOutExpo');
                event.preventDefault();
            });
            delete $window.sessionStorage.scrollDiv;
        }
        


        $scope.$on("$destroy", function() {
            $rootScope.footer = true;
        });
        
        $timeout(function () {
            $('a.page-scroll').bind('click', function (event) {
                var $ele = $(this);
                if ($($ele.attr('href')).offset() !== undefined) {
                    $('html, body').stop().animate({
                        scrollTop: ($($ele.attr('href')).offset().top - 20)
                    }, 2950, 'easeInOutExpo');
                    event.preventDefault();
                }
            });

        });
        
        $("#js-rotating").Morphext({
            animation: "fadeInLeft",
            separator: ",",
            speed: 9000
        });
        var timelineBlocks = $('.cd-timeline-block'),
		offset = 0.8;

        //hide timeline blocks which are outside the viewport
        hideBlocks(timelineBlocks, offset);

        //on scolling, show/animate timeline blocks when enter the viewport
        $(window).on('scroll', function () {
            (!window.requestAnimationFrame)
                ? setTimeout(function () { showBlocks(timelineBlocks, offset); }, 100)
                : window.requestAnimationFrame(function () { showBlocks(timelineBlocks, offset); });
        });

        function hideBlocks(blocks, offset) {
            blocks.each(function () {
                ($(this).offset().top > $(window).scrollTop() + $(window).height() * offset) && $(this).find('.cd-timeline-img, .cd-timeline-content').addClass('is-hidden');
            });
        }

        function showBlocks(blocks, offset) {
            blocks.each(function () {
                ($(this).offset().top <= $(window).scrollTop() + $(window).height() * offset && $(this).find('.cd-timeline-img').hasClass('is-hidden')) && $(this).find('.cd-timeline-img, .cd-timeline-content').removeClass('is-hidden').addClass('bounce-in');
            });
        }
        
        
    });

angular.bootstrap(document.body, ['sussol']);

app.run([
    '$rootScope', '$location', 'AuthenticationService', '$window', '$http', '$timeout', function ($root, $location, AuthenticationService, $window, $http) {
        
        $root.toHome = function () {
            window.location.href = '/';
        }
        $root.scrollPage = function (event) {
            if ($location.url() !== '/') {
                $window.sessionStorage.scrollDiv = event;
                window.location.href = '/';
            } else {
                $('html, body').stop().animate({
                    scrollTop: ($(event).offset().top - 20)
                }, 2950, 'easeInOutExpo');
                event.preventDefault();

            }
        }
        $root.$on('$routeChangeStart', function (event, curr) {
            
            $root.username = $window.sessionStorage.username;
            $root.userId = $window.sessionStorage.userId;
            $root.admin = $window.sessionStorage.admin;
            if ($window.sessionStorage.username !== undefined) {
                AuthenticationService.isLogged = true;
            }
            if (curr.authenticate && !AuthenticationService.isLogged) {
                event.preventDefault();
                $location.path('/');
                $('#login-modal').modal('show');
            } else {
                if (curr.templateUrl === "Content/Views/Account/Admin.html") {
                    if ($window.sessionStorage.role !== "SuperAdministrator") {
                        $location.path('/');
                    }
                }
                if (curr.templateUrl === "Content/Views/Analysis/Overview.html") {
                    $http({
                        method: 'POST',
                        url: 'api/Analysis/CheckPermission',
                        params: { userId: $window.sessionStorage.userId, analysisId: curr.params.id }
                    }).error(function succesCallback(data) {
                        $location.path('/404');
                    });
                }
                if (curr.templateUrl === "Content/Views/Organisation/Home.html") {
                    $http({
                        method: 'POST',
                        url: 'api/Organisation/CheckPermission',
                        params: { userId: $window.sessionStorage.userId, organisationId: curr.params.id }
                    }).error(function succesCallback(data) {
                        $location.path('/404');
                    });
                }
            }

            if (curr.$$route && curr.$$route.resolve) {
                // Show a loading message until promises aren't resolved   
                $root.loadingView = true;
            }
        });
        $root.$on('$routeChangeSuccess', function (e, curr, prev) {
            // Hide loading message
            $root.loadingView = false;
        });
        $root.footer = true;
    }
]);
app.directive("regExpRequire", function () {

    var regexp = [];
    return {
        restrict: 'A',
        link: function (scope, elem, attrs) {
            regexp.push(eval(attrs.regExpRequire));
            var char;
            elem.on("keypress", function (event) {
                char = String.fromCharCode(event.which);
                if (!regexp[event.currentTarget.title].test(elem.val() + char))
                    event.preventDefault();
            });
        }
    }

})