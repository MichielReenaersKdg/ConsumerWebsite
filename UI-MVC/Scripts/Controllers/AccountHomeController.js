angular.module('sussol.controllers')
    .controller('AccountHomeController', function ($scope, $rootScope, $http, fileReader, $routeParams, $location, result, organisationsResult, analysesResult) {
        var data = result.data;
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
        $scope.firstname = data.Firstname;
        $scope.lastname = data.Lastname;
        $scope.id = data.Id;
        var picture = data.Picture;
        if (picture != null && picture !== "") {
            $scope.imageSrc = '/Content/Images/Users/' + picture;
        }
        $scope.noOrganisations = true;
        $scope.noAnalyses = true;
        $scope.OrderBy = "DateCreated";

        var organisation = organisationsResult.data;
        if (organisation !== null && organisation !== undefined && organisation !== "") {

            $scope.noOrganisations = false;
            if (organisation.LogoUrl !== null && organisation.LogoUrl !== "") {
                organisation.LogoUrl = 'Content/Images/Organisations/' + organisation.LogoUrl;
            } else {
                organisation.LogoUrl = 'Content/Images/organisationLogo.jpg';
            }
            $scope.organisation = organisation;
        }

        var analyses = analysesResult.data;

        var i;
        for (i = 0; i < analyses.length; i++) {
            analyses[i].image = getRandomImage();
            analyses[i].DateCreated = timeSince(new Date(Date.parse(analyses[i].DateCreated + "+0200")));
        }

        $scope.analyses = analyses;
        if (analyses.length !== 0) {
            $scope.noAnalyses = false;
        }

        function timeSince(date) {
            var seconds = Math.floor((new Date() - date) / 1000);
            var interval = Math.floor(seconds / 31536000);

            if (interval > 1) {
                return interval + " years";
            }
            interval = Math.floor(seconds / 2592000);
            if (interval > 1) {
                return interval + " months";
            }
            interval = Math.floor(seconds / 86400);
            if (interval > 1) {
                return interval + " days";
            }
            interval = Math.floor(seconds / 3600);
            if (interval > 1) {
                return interval + " hours";
            }
            interval = Math.floor(seconds / 60);
            if (interval > 1) {
                return interval + " minutes";
            }
            return Math.floor(seconds) + " seconds";
        }


        $('ul.tabs li').click(function () {
            var tab_id = $(this).attr('data-tab');

            $('ul.tabs li').removeClass('current');
            $('.tab-content').removeClass('current');

            $(this).addClass('current');
            $("#" + tab_id).addClass('current');
        });

        var analysisToDelete = null;
        $scope.deleteAnalysis = function (id) {
            analysisToDelete = id;
            $('#delete-analysis').modal("show");
        }

        $scope.confirmDeleteAnalysis = function () {
            $('#delete-analysis').modal("hide");
            deleteAnalysisFromDatabase(analysisToDelete);
            analysisToDelete = null;
        }
        $scope.declineDeleteAnalysis = function () {
            $('#delete-analysis').modal("hide");
            analysisToDelete = null;
        }
        $scope.declineDeleteUser = function () {
            $('#delete-analysis').modal("hide");
            analysisToDelete = null;
        }

        function deleteAnalysisFromDatabase(id) {
            $http({
                method: 'POST',
                url: 'api/Analysis/Delete/' + id
            }).success(function (data) {
                notie.alert(1, data, 2);
                var analysesLeft = $scope.analyses.filter(obj => obj.Id !== id);
                $scope.analyses = analysesLeft;
            });
        }


        $scope.selectOrganisation = function (id) {
            $location.path("/organisation/" + id);
        }

        $scope.selectAnalysis = function selectAnalysis(id) {
            $location.path("/analysis/overview/" + id);
        }

        $scope.triggerUpload = function () {
            $("#profileImage").click();
        };
        $scope.getFile = function () {
            fileReader.readAsDataUrl($scope.file, $scope)
                          .then(function (result) {
                              $scope.imageSrc = result;
                              var formData = new FormData();
                              formData.append('email', $rootScope.username);
                              formData.append('picture', $scope.file);
                              $http({
                                  method: 'POST',
                                  url: 'api/Account/ChangeAvatar',
                                  headers: {
                                      'Content-Type': undefined
                                  },
                                  transformRequest: angular.identity,
                                  data: formData
                              }).success(function succesCallback(data) {
                                  //$scope.message = data;
                                  //$location.path("/");
                              }).error(function errorCallback(data) {
                                  //$scope.message = data;
                              });
                          });
        };

        $scope.OrderByFunc = function ($event) {

            var element = $event.currentTarget.id;
            if (element === "name") {
                $scope.OrderBy = "Name";
            } else if (element === "namedesc") {
                $scope.OrderBy = "-Name";
            } else {
                if ($scope.OrderBy === "DateCreated")
                    $scope.OrderBy = "-DateCreated";
                else
                    $scope.OrderBy = "DateCreated";
            }
        }

        var model = this;
        model.password = {
            currentPassword: "",
            newPassword: "",
            confirmPassword: ""
        };

        $scope.clearPassword = function () {
            model.password = {
                currentPassword: "",
                newPassword: "",
                confirmPassword: ""
            };
            delete $scope.message;
            delete $scope.success;
        }

        var changePassword = function (model, $http) {
            var formData = new FormData();
            formData.append('newPassword', model.password.newPassword);
            formData.append('confirmNewPassword', model.password.confirmPassword);
            formData.append('password', model.password.currentPassword);

            $http({
                method: 'POST',
                url: 'api/Account/ChangePassword?currentPassword=' + model.password.currentPassword + '&newPassword=' + model.password.newPassword
            }).success(function succesCallback(data) {
                $scope.success = data;

                setTimeout(function () {
                    $('#password-modal').modal('hide');
                    $scope.account.password.currentPassword = null;
                    $scope.account.password.newPassword = null;
                    $scope.account.password.confirmPassword = null;
                }, 2000);

                //$location.path("/");
            }).error(function errorCallback(data) {
                $scope.message = data;
            });

        }


        model.submit = function (isValid) {
            if (isValid) {
                changePassword(model, $http);
            } else {
                $scope.message = "There are still invalid fields below";
            }
        };

        function getRandomImage() {
            var number = Math.floor((Math.random() * 4) + 1);
            return "/Content/Images/random" + number + ".jpg";
        }
    });
