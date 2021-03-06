﻿angular.module('sussol.controllers')
    .controller("RegistrationController", function ($scope, $http, $location, fileReader) {
        var model = this;

        model.message = "";

        model.user = {
            firstname: "",
            lastname: "",
            picture: "",
            email: "",
            password: "",
            confirmPassword: ""
        };

        $scope.triggerUpload = function () {
            $("#profileImage").click();
        };
        $scope.getFile = function () {
            $scope.progress = 0;
            fileReader.readAsDataUrl($scope.file, $scope)
                .then(function (result) {
                    $scope.imageSrc = result;
                });
        };

        $scope.$on("fileProgress", function (e, progress) {
            $scope.progress = progress.loaded / progress.total;
        });

        var registration = function (model, $http) {
            console.log("test");
            var formData = new FormData();
            formData.append('firstname', model.user.firstname);
            formData.append('lastname', model.user.lastname);
            formData.append('email', model.user.email);
            formData.append('password', model.user.password);
            formData.append('picture', $scope.file);
            console.log("test1");
            $http({
                method: 'POST',
                url: 'api/Account/Register',
                headers: {
                    'Content-Type': undefined
                },
                transformRequest: angular.identity,
                data: formData
            }).success(function succesCallback(data) {
                model.message = data;
                var myEl = angular.element(document.querySelector('#register'));
                myEl.remove();
                $scope.registrationComplete = data;
                //$location.path("/");
            }).error(function errorCallback(data) {
                model.message = data.Message;
            });

        }
        model.submit = function (isValid) {
            if (isValid) {
                registration(model, $http);
                model.message = $scope.details;
            } else {
                model.message = "There are still invalid fields below";
            }
        };
    });



angular.module('sussol.services')
    .directive("compareTo", function () {
        return {
            require: "ngModel",
            scope: {
                otherModelValue: "=compareTo"
            },
            link: function (scope, element, attributes, ngModel) {

                ngModel.$validators.compareTo = function (modelValue) {
                    return modelValue === scope.otherModelValue;
                };

                scope.$watch("otherModelValue", function () {
                    ngModel.$validate();
                });
            }
        };
    })
.directive("ngFileSelect", function () {
    return {
        link: function ($scope, el) {
            el.bind("change", function (e) {
                $scope.file = (e.srcElement || e.target).files[0];
                $scope.getFile();
            });

        }

    }
});