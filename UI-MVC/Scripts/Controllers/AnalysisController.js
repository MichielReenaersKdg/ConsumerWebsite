angular.module('sussol.controllers')
    .controller('AnalysisController',
    function ($scope, $window, $http, $location, $rootScope, srvLibrary) {
        var algorithms = [];
        var models = [];
        var process = 0;
        
        
        $scope.disabled = true;
        $scope.btnclass = "button-right disabled";
        $scope.modelDisabled = true;
        $scope.showAnalysis = { color: 'grey' };
        $scope.changeColor = function changeColor($event) {
            if (!contains(algorithms, $event.currentTarget.id)) {
                $event.currentTarget.style.background = "purple";
                $event.currentTarget.style.color = "white";
                
                algorithms.push($event.currentTarget.id);
                if (algorithms.length !== 0) {
                    $scope.btnclass = "button-right";
                    $scope.disabled = false;
                    $scope.next = { color: 'white' }
                    if (process === 0) {
                        process = process + 25;
                        angular.element(document.querySelector('#progressBar .progress-bar')).css("width", process + "%").attr("aria-valuenow", 0);
                    }
                }
            } else {
                $event.currentTarget.style.background = "#f0f1ec";
                $event.currentTarget.style.color = "#034b81";
                var index = algorithms.indexOf($event.currentTarget.id);
                algorithms.splice(index, 1);
                if (algorithms.length === 0) {
                    $scope.disabled = true;
                    $scope.btnclass = "button-right disabled";
                    process = process - 25;
                    angular.element(document.querySelector('#progressBar .progress-bar')).css("width", 0 + "%").attr("aria-valuenow", 0);
                }
            }
            
        };

        function contains(a, obj) {
            var i = a.length;
            while (i--) {
                if (a[i] === obj) {
                    return true;
                }
            }
            return false;
        }

        //function fillarray() {
        //    $https({
        //        method: 'GET',
        //        url: 'api/Analysis/FillAlgorithms',
        //        params: {algorithms: algorithms}
        //    })
        //}

        //$scope.fillAlgorithm = function fillAlgorithm() {
        //    $http({
        //        method: 'GET',
        //        url: 'api/Analysis/FillAlgorithmTwo',
        //        params: { algorithms: algorithms, Id: selectedModel.Id}
        //    })
        //}
        
        $scope.startAnalysis = function startAnalysis() {
            $rootScope.loadingView = true;
            algorithms.sort();
            $http({
                method: 'POST',
                url: 'api/Analysis/StartAnalysis',
                params: {algorithms : algorithms}
            }).success(function (data) {
                process = process + 25;
                angular.element(document.querySelector('#progressBar .progress-bar')).css("width", process + "%").attr("aria-valuenow", process);
                models = data;
                //$scope.algorithms = data;
                getTrainingsets(data);
                $scope.btnclass = "button-right disabled";
                $rootScope.loadingView = false;
            });
        }
        var selectedModel;
        var analyseName;
        function getTrainingsets(data) {
            var test = [];
        let trainingfiles = [...new Set(data.map(t => t.trainingSet.ID))];
        for (var i = 0; i < trainingfiles.length; i++) {
            var result = data.filter(function (obj) {
                return obj.trainingSet.ID == trainingfiles[i];
            });
            test.push(result[0]);
            }
        $scope.algorithms = test;
        }
        
        $scope.selectModel = function selectModel($event) {
            if (selectedModel !== undefined) {
                selectedModel.style.borderColor = "lightgray";
                selectedModel.style.backgroundColor = "rgba(255, 255, 255, 0.1)";

            }
            selectedModel = $event.currentTarget;
            $event.currentTarget.style.borderWidth = "3px";
            $event.currentTarget.style.borderColor = "rgba(156,39,193, 0.8)";
            $event.currentTarget.style.backgroundColor = "rgba(255, 255, 255, 0.4)";

            if (analyseName !== undefined && analyseName !== "") {
                $scope.btnclass = "button-right";
            }
            if (process === 50 || process === 60) {
                process = process + 40;
                angular.element(document.querySelector('#progressBar .progress-bar')).css("width", process + "%").attr("aria-valuenow", process);
            }
            
        }



        $scope.setName = function setName(name) {
            if (name === "") {
                $scope.btnclass = "button-right disabled";
                process = process - 10;
                angular.element(document.querySelector('#progressBar .progress-bar')).css("width", process + "%").attr("aria-valuenow", process);
            } else {
                analyseName = name;
                if (process === 90 || process === 50) {
                    process = process + 10;
                    angular.element(document.querySelector('#progressBar .progress-bar')).css("width", process + "%").attr("aria-valuenow", process);
                }
                if (selectedModel !== undefined) {
                    $scope.btnclass = "button-right";
                }
            }
        }

        $scope.showAlgorithms = function showAlgorithms() {
            //$scope.fillAlgorithm();
            $http({
                method: 'POST',
                url: 'api/Analysis/CreateAnalysis',
                params: { algorithms: algorithms, dataSet: selectedModel.id, name: analyseName }
            }).success(function (data) {
                $location.path("/analysis/overview/" + data.Id);
            }).error(function (data) {
                $scope.errorMessage = data.Message;
            });
        }



        $scope.previous = function previous() {
            algorithms = [];
            delete $scope.algorithms;
            $scope.btnclass = "button-right disabled";
            process = 0;
            angular.element(document.querySelector('#progressBar .progress-bar')).css("width", 0 + "%").attr("aria-valuenow", 0);
        }
    });