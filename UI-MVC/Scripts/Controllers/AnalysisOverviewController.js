angular.module('sussol.controllers')
    .controller('AnalysisOverviewController',
    function ($scope, $sce, $window, $http, $routeParams, constants, result, features, solvents, $timeout, organisation, $rootScope) {
        var solvents = [];
        var selectedAlgorithm;
        var organisationUser = organisation.data;
        var casnrRegex = new RegExp("^\\d{2,6}-\\d{2}-\\d{1}$");
        var egRegex = new RegExp("^\\d{3}-\\d{3}-\\d{1}$");
        var prevClassifiedInstances;
        var prevClusters;
        var clusters;
        var overlayOpened = false;
        var solventOverlayOpened = false;
        var solvInfo = true;
        //0.5.0.5 mogelijke errors door verwijdering minmax
        //var featureModel = minMax.data;
        var models;
        var selectedModel = null;
        var currentChart = null;
        var showInstance = false;
        $scope.allValuesValid = false;
        $scope.classify = false;
        $scope.upload = true;
        $scope.download = true;
        $scope.check = false;
        $scope.errorMessageMeta = "wrong value";
        $scope.errorMessageFeature = "is not a number";
        $scope.validation = [];
        var algorithms = [];
        var totalSolvents = 0;
        var colors = [
            "#44B3C2",
            "#F1A94E",
            "#F2635F",
            "#4CD4B0",
            "#8e44ad",
            "#FC575E",
            "#9F0088",
            "#32B92D",
            "#F20075",
            "#E0A025",
            "#0093D1"

        ];

        $scope.sharedWith = result.data.SharedWith;
        $scope.models = result.data.AnalysisModels;
        var data = result.data;
        //0.5.0.5
        //setEnumMinMax();
        showClusterAnalysis(result.data.AnalysisModels);

        if (organisationUser === "") {
            organisationUser = null;
        }
        $scope.organisationUser = organisationUser;

        if (data.CreatedBy.Id.toString() === $window.sessionStorage.userId) {
            $scope.isOwner = true;
        }

        function showClusterAnalysis(modelsTemp) {
            getClassifiedInstances();
            
            models = modelsTemp;
            setEnumNames();
            
            
            for (var i = 0; i < models.length; i++) {
                clusters = getClusters(models[i].Model);
                var clusterPositions = [];
                for (var j = 0; j < clusters.length; j++) {
                    clusterPositions.push(getClusterPosition(clusters[j]));
                }
                var normalizedValues = getNormalizedValues(clusterPositions);
                models[i].Model.NormalizedValues = normalizedValues;
            }

            
            if (showInstance) {
                for (var i = 0; i < models.length; i++) {
                    if (models[i].Model.AlgorithmName === selectedAlgorithm) {
                        createChart(models[i].Model);
                        showClassifyResult(models[i].ClassifiedInstance.ClusterNumber);
                        $scope.ClassifiedInstance = models[i].ClassifiedInstance;
                    }
                }
            }
            

        }

        selectedAlgorithm = models[0].Model.AlgorithmName;

        $scope.canEdit = false;

        if (data.CreatedBy.Id.toString() === $window.sessionStorage.userId) {
            $scope.canEdit = true;
        }



        $scope.$watchGroup(['form.features.$valid', 'allValuesValid'], function (newVal) {
            if (newVal[1] && newVal[0]) {
                $scope.valid = true;
            } else {
                $scope.valid = false;
            }
        });

        $scope.analysisName = data.Name;
        
        $timeout(function () {
            for (var i = 0; i < algorithms.length; i++) {
                if (i === 0) {
                    $('#' + algorithms[i]).addClass("active");
                    $('#' + algorithms[i] + '_CONTENT').addClass("active");
                }
                $('#' + algorithms[i]).removeClass("disabled");
                $('#' + algorithms[i]).removeClass("blurless");
            }

            
            selectedModel = models[0].Model;
            createChart(models[0].Model);
            createProgress(findAnalysisModelOnName(selectedAlgorithm));
        });

        
        $scope.focusFeatureInput = function (event) {
            event.currentTarget.style.borderWidth = "2px";
            event.currentTarget.style.borderColor = "purple";
        }


        function getClassifiedInstances() {
            $http({
                method: 'GET',
                url: 'api/Analysis/ReadClassifiedInstances',
                params: { userId: $window.sessionStorage.userId, analysisId: data.Id }
            }).success(function succesCallback(data) {
                prevClassifiedInstances = data;
                $scope.prevClassifiedInstances = prevClassifiedInstances;
            });

        }


        function getClusters(model) {
            clusters = [];
            for (var i = 0; i < model.Clusters.length; i++) {
                clusters.push(model.Clusters[i]);
                totalSolvents += clusters[i].Solvents.length;
            }
            return clusters;
        }

        function createProgress(analysisModel) {
            var totalSolvents = data.NumberOfSolvents;
            $scope.totalSolvents = totalSolvents;
            prevClusters = analysisModel.Model.Clusters;

            for (var i = 0; i < prevClusters.length; i++) {
                jQuery("#circle-" + selectedAlgorithm + "-" + i).radialProgress("init", {
                    'size': 110,
                    'fill': 14,
                    'font-size': 24,
                    'font-family': "Questrial",
                    "color": colors[i]
                }).radialProgress("to", { 'perc': ((prevClusters[i].Solvents.length / totalSolvents) * 100) - 0.2, 'time': 800 });
            }
        };


        function setEnumNames() {
            
            for (var i = 0; i < models.length; i++) {
                models[i].Model.AlgorithmName = constants.AlgorithmName[models[i].Model.AlgorithmName];
                algorithms.push(models[i].Model.AlgorithmName);
                if (showInstance) {
                    for (var j = 0; j < models[i].ClassifiedInstance.Features.length; j++) {
                        models[i].ClassifiedInstance.Features[j].FeatureName = constants.FeatureName[models[i].ClassifiedInstance.Features[j].FeatureName]; //0.4.9
                        models[i].ClassifiedInstance.Features[j].value = Number(models[i].ClassifiedInstance.Features[j].value.toFixed(2));
                    }
                }
                for (var j = 0; j < models[i].Model.Clusters.length; j++) {
                    for (var k = 0; k < models[i].Model.Clusters[j].VectorData.length; k++) {
                        
                        models[i].Model.Clusters[j].VectorData[k].feature.featureName = constants.FeatureName[models[i].Model.Clusters[j].VectorData[k].feature.featureName]; //0.4.9
                    }
                    for (var k = 0; k < models[i].Model.Clusters[j].Solvents.length; k++) {
                        
                        for (var l = 0; l < models[i].Model.Clusters[j].Solvents[k].Features.length; l++) {
                            models[i].Model.Clusters[j].Solvents[k].Features[l].FeatureName = constants.FeatureName[models[i].Model.Clusters[j].Solvents[k].Features[l].FeatureName]; //0.4.9
                            models[i].Model.Clusters[j].Solvents[k].Features[l].value[0] = Number(models[i].Model.Clusters[j].Solvents[k].Features[l].value.toFixed(2)); // 0.4.9 
                        }
                    }
                }
            }


            solvents = [];
            for (var i = 0; i < models[0].Model.Clusters.length; i++) {
                for (var j = 0; j < models[0].Model.Clusters[i].Solvents.length; j++) {
                    solvents.push(models[0].Model.Clusters[i].Solvents[j]);
                }
            }
            $scope.solvents = solvents;

        }
        $scope.getWidth = function getWidth(clusters) {
            var counter = clusters.filter(p => p.Solvents.length > 0).length;
            //debugger;
            return {
                "width": (100 / counter) + "%"
            }
        }
        //0.5.0.2
       

        $scope.form = {};
        

        $scope.newSolvent = function newSolvent() {
            console.log($scope.headerz)
            addSolvent($scope.solvent,$scope.headerz, $http);

        };
        //0.5.0.11
        function addSolvent(solvent, values, $http) {
            $('#load').button('loading');
            $scope.classify = false;
            $scope.check = false;
            var metadata = values[0];
            var features = values[1]
            document.getElementById('closecross').disabled = true;
            var solventName = metadata[1].value;
            var casNumber = metadata[2].value;
            var featureNames = [];
            var featureValues = [];
            var modelPaths = [];
            var modelIds = [];
            for (var i = 0; i < features.length; i++) {
                featureNames.push(features[i].FeatureName); 
                featureValues.push(features[i].value);
            }
            for (var i = 0; i < models.length; i++) {
                modelPaths.push(models[i].Model.ModelPath);
                modelIds.push(models[i].Id);
            }
            var model = {
                'Name': solventName,
                'CasNumber': casNumber,
                'Values': featureValues,
                'FeatureNames': featureNames,
                'AnalysisModels': models,
                'UserId': $window.sessionStorage.userId
            };
            $http({
                method: 'POST',
                url: 'api/Analysis/ClassifyNewSolvent',
                params: { analysisId: data.Id },
                data: model
            }).success(function succesCallback(data) {
                $('#addSolvent-modal').modal('hide');
                showInstance = true;
                showClusterAnalysis(data);
                $scope.errorMessage = undefined;
                $('#load').button('reset');
                delete $scope.headerz;

                document.getElementById('closecross').disabled = false;
                for (var i = 0; i < document.getElementsByClassName("feature-input").length; i++) {
                    document.getElementsByClassName("feature-input")[i].style.borderColor = "black";
                }
                //document.getElementById("newSolventCasNr").style.borderColor = "black";
                //document.getElementById("newSolventName").style.borderColor = "black";
            }).error(function errorCallback(data) {
                $scope.errorMessage = data.Message;
                $('#load').button('reset');
                document.getElementById('closecross').disabled = false;
                for (var i = 0; i < document.getElementsByClassName("feature-input").length; i++) {
                    document.getElementsByClassName("feature-input")[i].style.borderColor = "black";
                }
                //document.getElementById("newSolventCasNr").style.borderColor = "black";
                //document.getElementById("newSolventName").style.borderColor = "black";

            });
            
        }

        function getClusterPosition(cluster) {
            var som = 0;
            for (var i = 0; i < cluster.VectorData.length; i++) {
                var vector = cluster.VectorData[i].Value;
                som += Math.pow(vector, 2);
            }
            return Math.sqrt(som);
        }



        
        function getNormalizedValuesWithFixedMin(lengths, minimum) {
            if (lengths.length === 1) {
                return [0];
            }
            var max = Math.max.apply(Math, lengths);
            
            var normalizedValues = [];
            for (var i = 0; i < lengths.length; i++) {
                normalizedValues.push((lengths[i] - minimum) / (max - minimum));
            }
            return normalizedValues;
        }

        //[0.5.0.6]This function gets a model from one of the algorithms (Canopy, SOM, Xmeans)
        //returns a json which you use in createChart() (mode 1)
        function createJsonModel(model) {
            var json = [];
            var percentages = [];
            var totalSolvents = model.Model.NumberOfSolvents;
            // returns a distance-matrix
            var distancematrix = buildMatrix2(model.Model.Clusters);
            //use distance-matrix to calculate PcoA
            var graphdata = calculatePcoA(distancematrix);

            for (var i = 0; i < model.Model.Clusters.length; i++) {
                var valuesSolvents = [];
                for (var j = 0; j < model.Model.Clusters[i].Solvents.length; j++) {

                    valuesSolvents.push(model.Model.Clusters[i].Solvents[j].DistanceToClusterCenter);
                }
                var max = Math.max.apply(Math, valuesSolvents);


                var percentage = ((valuesSolvents.length) / totalSolvents) * 100;
                var data = graphdata[i]
                percentages.push(percentage);
                //x = object[0], y = object[1] from the object returned from calculatePcoA, z changes the size of the bubbles.
                json[i] = ({
                    'x': data[0], 'y': data[1], 'z': Number(max.toFixed(3)), 'name': model.Model.Clusters[i].Number, 'cursor': 'pointer', 'solvents': model.Model.Clusters[i].Solvents.length, 'color': colors[i], 'markerBorderColor': "#F4FE00", //change color here
                    'markerBorderThickness': 0
                });

            }

            model.Model.maxPercent = Math.max.apply(Math, percentages);


            return json;
        }


        $("div.bhoechie-tab-menu>div.list-group>a").click(function (e) {
            e.preventDefault();
            $(this).siblings('a.active').removeClass("active");
            $(this).addClass("active");

            $("div.bhoechie-tab>div.bhoechie-tab-content").removeClass("active");
            $("#" + e.currentTarget.id + "_CONTENT").addClass("active");
            $scope.closeOverlay(selectedAlgorithm);

            resetProgress();
            selectedAlgorithm = e.currentTarget.id;
            createProgress(findAnalysisModelOnName(selectedAlgorithm));
            $scope.clusters = getClusters(findModelOnName(selectedAlgorithm));
            createChart(findModelOnName(e.currentTarget.id));
            if (showInstance) {
                var datapoint = getDatapoint(findAnalysisModelOnName(e.currentTarget.id).ClassifiedInstance.ClusterNumber);
                changeInstance(findAnalysisModelOnName(e.currentTarget.id).ClassifiedInstance);
                
                setBorderDatapoint(datapoint);
                currentChart.render();
            }
        });

        function changeInstance(instance) {
            $scope.ClassifiedInstance = instance;
        }

        
        $scope.downloadPdf = function () {
            loadGraphs();
        }
        var counter = 0;
        var chartArray = [];
        var progressArray = [];
        var lastSelectedAlgorithm;
        function loadGraphs() {
           
           
            if (counter === 0) {
                
                lastSelectedAlgorithm = selectedAlgorithm;
                $rootScope.loadingPdf = true;
            }
            if (counter < result.data.AnalysisModels.length) {
                progress(((counter / result.data.AnalysisModels.length) * 100), $('#progresspdf'));
            }
            if (counter === result.data.AnalysisModels.length) {
                counter = 0;
                $('#' + lastSelectedAlgorithm).click();
                generatePdf();
            } else {
                $('#' + result.data.AnalysisModels[counter].Model.AlgorithmName).click();
                setTimeout(
                    function() {
                        html2canvas(document.getElementById('chart-container-' + result.data.AnalysisModels[counter].Model.AlgorithmName),
                        {
                            onrendered: function (canvas) {
                                chartArray.push(canvas.toDataURL());
                                html2canvas(document.getElementById('progress_' + result.data.AnalysisModels[counter].Model.AlgorithmName),
                                {
                                    onrendered: function (canvas) {
                                    progressArray.push(canvas.toDataURL());
                                    counter++;
                                    loadGraphs();
                                }
                                });
                            }
                        });
                    }, 2500);
            }
        }
        function progress(percent, $element) {
            
            var progressBarWidth = percent * $element.width() / 100;
            $element.find('div').animate({ width: progressBarWidth }, 1000).html(percent + "% ");
        }
        function generatePdf() {
            var featureArray = [];
            var algorithmArray = [];
            
            //0.5.0.12 Need to fix
            for (var i = 0; i < features.length; i++) {
                featureArray.push({ text: features[i].features.FeatureName, margin: [60, 10, 0, 0] }); //0.4.9
            }
            for (var i = 0; i < result.data.AnalysisModels.length; i++) {
                $scope.currAlgo = data.AnalysisModels[i];
                algorithmArray.push({ text: result.data.AnalysisModels[i].Model.AlgorithmName, style: 'header4', margin: [10, 10, 0, 0], pageBreak: 'before', pageOrientation: 'landscape' });
                algorithmArray.push({
                    image: progressArray[i], width: 800,
                    height: 90,
                    alignment: 'center',
                    margin: [0,20,0,10]
                });
                algorithmArray.push({
                    image: chartArray[i], width: 800,
                    height: 340,
                    alignment: 'center',
                    pageBreak: 'after'
                });
                
                for (var j = 0; j < $scope.currAlgo.Model.Clusters.length; j++) {
                    algorithmArray.push({ text: 'Cluster ' + j + ": " + result.data.AnalysisModels[i].Model.Clusters[j].Solvents.length + ' Solvents', style: 'headercluster',color: colors[j], margin: [25, 10, 0, 0] });
                    

                    algorithmArray.push({ text: 'Centroids', style: 'header5', margin: [27, 10, 0, 0] });
                    var centroids = "[";
                    for (var z = 0; z < result.data.AnalysisModels[i].Model.Clusters[j].VectorData.length; z++) {
                        centroids += result.data.AnalysisModels[i].Model.Clusters[j].VectorData[z].Value;
                        if (z !== result.data.AnalysisModels[i].Model.Clusters[j].VectorData.length - 1) {
                            centroids += ", ";
                        }
                    }
                    algorithmArray.push({ text: centroids + "]", margin: [32, 5, 0, 0] });
                    algorithmArray.push({ text: 'Min and max distance to cluster center', style: 'header5', margin: [27, 10, 0, 0] });
                    var maxSolvent = undefined, minSolvent = undefined;

                    for (var k = 0;k < result.data.AnalysisModels[i].Model.Clusters[j].Solvents.length; k++) {
                        if (maxSolvent === undefined || result.data.AnalysisModels[i].Model.Clusters[j].Solvents[k].DistanceToClusterCenter > maxSolvent.DistanceToClusterCenter) {
                            maxSolvent = result.data.AnalysisModels[i].Model.Clusters[j].Solvents[k];
                        }
                        if (minSolvent === undefined || result.data.AnalysisModels[i].Model.Clusters[j].Solvents[k].DistanceToClusterCenter < minSolvent.DistanceToClusterCenter) {
                            minSolvent = result.data.AnalysisModels[i].Model.Clusters[j].Solvents[k];
                        }
                    }
                    var minMaxForPdf = "Minimum distance: " + minSolvent.DistanceToClusterCenter.toFixed(2) + " - " + minSolvent.Name + " \n"
                        + "Maximum distance: " + maxSolvent.DistanceToClusterCenter.toFixed(2) + " - " + maxSolvent.Name;
                    
                    algorithmArray.push({ text: minMaxForPdf, margin: [32, 5, 0, 0] });
                    if (showInstance) {
                        if (result.data.AnalysisModels[i].ClassifiedInstance.ClusterNumber === result.data.AnalysisModels[i].Model.Clusters[j].Number) {
                            algorithmArray.push({ text: 'Classified solvent', style: 'header5', margin: [27, 10, 0, 0] });
                            algorithmArray.push({ text: 'Name: ' + result.data.AnalysisModels[i].ClassifiedInstance.Name, margin: [32, 5, 0, 0] });
                            algorithmArray.push({ text: 'Cas number: ' + result.data.AnalysisModels[i].ClassifiedInstance.CasNumber, margin: [32, 5, 0, 0] });
                            algorithmArray.push({ text: 'Distance to cluster center: ' + result.data.AnalysisModels[i].ClassifiedInstance.DistanceToClusterCenter.toFixed(2), margin: [32, 5, 0, 0] });
                        }
                    }
                }
                
            }
            chartArray = [];
            
            var docDef = {
                content: [
                { text: 'Sussol Solvent Cluster Analysis', style: 'header', margin: [0, 200, 0, 0] },
                { text: result.data.Name + ' by', style: 'header2', margin: [0, 50, 0, 0] },
                { text: result.data.CreatedBy.Firstname + ' ' + result.data.CreatedBy.Lastname, style: 'header2', margin: [0, 15, 0, 0] },
                { text: new Date(result.data.DateCreated).toDateString(), style: 'header2', margin: [0, 15, 0, 0] },

                { text: result.data.AnalysisModels.length + ' algorithms - ' + result.data.NumberOfSolvents + ' solvents - ' + result.data.AnalysisModels[0].Model.NumberOfFeatures + ' features', style: 'header3', margin: [0, 15, 0, 0] },

                {
                    image: 'data:image/png;base64,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',
                    width: 285,
                    height: 40,
                    alignment: 'center',
                    margin: [0, 30, 0, 0],
                    pageBreak: 'after'
                },
                { text: 'General Information', style: 'header4', margin: [10, 10, 0, 0] },
                { text: 'Training set: ', style: 'header5', margin: [30, 30, 0, 0] },
                { text: 'Name: ' + result.data.AnalysisModels[0].Model.DataSet, margin: [60, 10, 0, 0] },
                { text: 'Number of solvents: ' + result.data.AnalysisModels[0].Model.NumberOfSolvents, margin: [60, 10, 0, 0] },
                { text: 'Number of features: ' + result.data.AnalysisModels[0].Model.NumberOfFeatures, margin: [60, 10, 0, 0] },
                { text: 'Features: ', style: 'header5', margin: [30, 30, 0, 0] },
                    {
                        ul: featureArray

                    },
                    algorithmArray
                ],
                footer: function (currentPage, pageCount) {
                    return {
                        columns: [
                            {
                                image: 'data:image/png;base64,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',
                                width: 135,
                                height: 20,
                                alignment: 'left',
                                margin: [10, 5, 0, 0]
                            },
                            {
                                text: currentPage.toString(),
                                alignment: 'right',
                                margin: [0, 5, 15, 0]
                            }
                        ]
                    };
                },
                styles: {
                    header: {
                        fontSize: 23,
                        bold: true,
                        color: '#4183D7',
                        alignment: 'center'
                    },
                    header2: {
                        fontSize: 18,
                        bold: true,
                        color: 'black',
                        alignment: 'center'
                    },
                    header3: {
                        fontSize: 16,
                        bold: true,
                        color: '#4183D7',
                        alignment: 'center'
                    },
                    header4: {
                        fontSize: 18,
                        bold: true,
                        color: '#4183D7',
                        alignment: 'left'
                    },
                    header5: {
                        fontSize: 14,
                        bold: true,
                        color: 'black',
                        alignment: 'left'
                    },
                    headercluster: {
                        fontSize: 14,
                        bold: true,
                        
                        alignment: 'left'
                    },
                }
            };
            pdfMake.createPdf(docDef).download('Sussol_Analysis' + '.pdf', function () {
                progress((100), $('#progresspdf'));
                setTimeout(function() {
                    $rootScope.loadingPdf = false;
                    $scope.$apply();

                    setTimeout(function() { progress((0), $('#progresspdf'));},750);
                }, 750);
            });
            

            
           
        }
        function resetProgress() {
            for (var i = 0; i < prevClusters.length; i++) {
                jQuery("#circle-" + selectedAlgorithm + "-" + i).empty();
            }
        }
        //[0.5.0.6] Calculation X en Y for cluster-graph (mode 1) with PcoA algorithm
        //This functions gets distances from a distance-matrix and returns an object with two arrays for X en Y
        //This function is called in the createJsonModel function
        function calculatePcoA(distances) {
            dimensions = 2;

            // square distances
            var M = numeric.mul(-0.5, numeric.pow(distances, 2));

            // double centre the rows/columns
            function mean(A) { return numeric.div(numeric.add.apply(null, A), A.length); }
            var rowMeans = mean(M),
                colMeans = mean(numeric.transpose(M)),
                totalMean = mean(rowMeans);

            for (var i = 0; i < M.length; ++i) {
                for (var j = 0; j < M[0].length; ++j) {
                    M[i][j] += totalMean - rowMeans[i] - colMeans[j];
                }
            }

            // take the SVD of the double centred matrix, and return the
            // points from it
            var ret = numeric.svd(M),
                eigenValues = numeric.sqrt(ret.S);
            return ret.U.map(function (row) {
                return numeric.mul(row, eigenValues).splice(0, dimensions);
            });
        }
        //[0.5.0.6] Draws the cluster-chart (mode 1)
        //Gets model from selected algorithm and draws the chart
        function createChart(model) {
            CanvasJS.addColorSet("greenShades", colors
                );
            var jsonModel = createJsonModel(findAnalysisModelOnName(selectedAlgorithm));
            var maxX = 0;
            var maxY = 0;
            var minX = 0;
            var minY = 0;
            for (var i = 0; i < jsonModel.length; i++) {
                if (maxX < jsonModel[i].x) {
                    maxX = jsonModel[i].x;
                }
                if (maxY < jsonModel[i].y) {
                    maxY = jsonModel[i].y;
                }
                if (minX > jsonModel[i].x) {
                    minX = jsonModel[i].x;
                }
                if (minY > jsonModel[i].y) {
                    minY = jsonModel[i].y;
                }
            }

            var chart = new CanvasJS.Chart("chartContainer_" + model.AlgorithmName,
            {
                animationEnabled: true,
                animationDuration: 500,
                theme: "theme3",

                backgroundColor: "rgba(30,30,30,1)",

                axisX: {
                    //title: "Test",
                    valueFormatString: " ",
                    tickLength: 0,
                    //viewportMaximum: 1.1,
                    //viewportMinimum: -0.1,
                    viewportMaximum: maxX+200,
                    viewportMinimum: minX-200,
                    interval: 100,
                    gridThickness: 1,
                    tickThickness: 1,
                    gridColor: "grey",
                    tickColor: "grey",
                    titleFontColor: "gray",
                    lineThickness: 0
                },
                axisY: {
                    //title: "Test",
                    valueFormatString: " ",
                    interval: 100,
                    gridThickness: 1,
                    tickThickness: 1,
                    //viewportMaximum: model.maxPercent + 15,
                    viewportMaximum: maxY+200,
                    viewportMinimum: minY-200,
                    gridColor: "grey",
                    tickColor: "grey",
                    lineThickness: 0,
                    //valueFormatString: "#0'%'",
                    titleFontColor: "gray"

                },

                data: [
                {
                   
                    type: "bubble",
                        toolTipContent: "<span style='\"'color: {color};'\"'><strong>Cluster {name}</strong></span><br/><strong>#solvents</strong> {solvents} <br/> <strong>Percentage</strong> {y}%<br/> <strong>Max distance</strong> {z}",
                        dataPoints: jsonModel,
                        click: function (e) {
                            drawSolventChart(e.dataPoint.name);
                            $scope.$apply();
                        },
                        mouseout: function (e) {       
                        var tooltip = document.getElementsByClassName("canvasjs-chart-tooltip");
                        for (var i = 0; i < tooltip.length; i++) {
                            tooltip[i].style.display = "none";    
                        }
                    }
                    }
                ]
            });

            chart.render();

            currentChart = chart;
            
        }

        $scope.clusterChange = function (clusternumber) {
            $scope.closeOverlay(selectedAlgorithm);
            drawSolventChart(clusternumber);

        }

        function drawSolventChart(clusternumber) {

            var model = findModelOnName(selectedAlgorithm);
            var solventen = getSolventsFromCluster(model, clusternumber);
            $('#overlay_' + selectedAlgorithm).removeClass("not-visible");

            $('#overlay_' + selectedAlgorithm).addClass("div-overlay");
            overlayOpened = true;
            
            var distances = [];
            for (var i = 0; i < solventen.length; i++) {
                distances.push(solventen[i].DistanceToClusterCenter);
            }
            var max = Math.max.apply(Math, distances);
            for (var i = 0; i < solventen.length; i++) {
                solventen[i].DistanceToClusterPercentage = (solventen[i].DistanceToClusterCenter / max) * 95;
            }

            setTimeout(function () {
                var tooltip = document.getElementsByClassName("canvasjs-chart-tooltip");
                for (var i = 0; i < tooltip.length; i++) {
                    tooltip[i].style.display = "none";
                }
                createClusterChart(model.Clusters[clusternumber]);
                $scope.overlayvisible = true;
            }, 900);

            $scope.solventsInCluster = solventen;

            $scope.cluster = clusternumber;

            $(".cluster-div").removeClass("selected");
            $("#cluster-div-" + clusternumber).addClass("selected");
        }

        var canvaz;
        function drawClassifySolvent(datapoint) {
            $scope.closeOverlay(selectedAlgorithm);
            

            if (canvaz !== undefined) {
                canvaz.timeline.stop();
                canvaz.clear();
            }
            var canvas = document.getElementById("canvas-overlay-" + selectedAlgorithm);
            var chartCanvas = document.getElementById("chart-container-" + selectedAlgorithm);
            var xAxisLength = chartCanvas.offsetWidth - 82 - 24; //beginnen tekenen op 87
            var yAxisLength = chartCanvas.offsetHeight - 46 - 73; //beginnen tekenen van chartCanvas.offsetHeight - 46
            canvas.width = xAxisLength;
            canvas.height = yAxisLength;
            canvaz = oCanvas.create({
                canvas: "#canvas-overlay-" + selectedAlgorithm
            });

            var arc = canvaz.display.polygon({
                x: xAxisLength / 2,
                y: yAxisLength / 2,
                radius: 24,
                sides: 6,
                fill: "#F4FE00"
            });
            canvaz.addChild(arc);

            canvaz.setLoop(function () {
                arc.rotation++;
            });
            canvaz.timeline.start();
            window.addEventListener("resize", function () {
                xAxisLength = chartCanvas.offsetWidth - 87 - 24; //beginnen tekenen op 87
                yAxisLength = chartCanvas.offsetHeight - 46 - 117; //beginnen tekenen van chartCanvas.offsetHeight - 46
                canvas.width = xAxisLength;
                canvas.height = yAxisLength;
                canvaz = oCanvas.create({
                    canvas: "#canvas-overlay-" + selectedAlgorithm
                });
            });

            var datapointX = (xAxisLength / 2) + datapoint.x;
            var maxy = yAxisLength;
            var datapointY = ((maxy + 44) / 2) - datapoint.y;//canvas.height - (((datapoint.y / maxX) * 100) * (yAxisLength / 100));

            arc.animate({
                x: datapointX,
                y: datapointY,
                radius: 0
            }, {
                duration: 10000,
                easing: "ease-in-expo",
                callback: function () {
                    currentChart.render();
                    this.fill = "transparent";
                    canvaz.redraw();
                    
                }
            });

        }

        function getDatapoint(clusterNumber) {
            var datapoint = null;
            for (var i = 0; i < currentChart.options.data[0].dataPoints.length; i++) {
                if (currentChart.options.data[0].dataPoints[i].name === clusterNumber) {
                    datapoint = currentChart.options.data[0].dataPoints[i];
                }
            }
            return datapoint;
        }


        function showClassifyResult(clusterNumber) {
            var datapoint = getDatapoint(clusterNumber);
            if (datapoint != null) {
                drawClassifySolvent(datapoint);
                setBorderDatapoint(datapoint);
            }
        }

        function setBorderDatapoint(datapoint) {
            datapoint.markerBorderThickness = 5;
        }

        $scope.shareWithOrganisation = function () {
            $http({
                method: 'POST',
                url: 'api/Analysis/ShareWithOrganisation',
                params: { organisationId: organisationUser.Id, analysisId: data.Id }
            }).success(function succesCallback(data) {
                notie.alert(1, "Cluster Analysis shared with the organisation", 2);
                $('#organisation-model').modal('hide');
                $scope.sharedWith = data.SharedWith;
            });
        }

        function getSolventsFromCluster(model, number) {
            return model.Clusters[number].Solvents;
        }

        $scope.selectedSolventFunc = function ($item) {
            $scope.selectedSolvent = $item.originalObject;
            delete $scope.selectedCluster;
            getClusterFromSolvent($scope.selectedSolvent);
            var element = document.getElementsByClassName('angucomplete-holder');
            element[0].style.width = '50px';
            $scope.overlayvisible = true;
            overlayOpened = true;

        }
        $scope.selectedSolventCasFunc = function ($item) {
            $scope.selectedSolvent = $item.originalObject;
            delete $scope.selectedCluster;
            getClusterFromSolvent($scope.selectedSolvent);
            var element = document.getElementsByClassName('angucomplete-holder');
            element[0].style.width = '50px';
            $scope.overlayvisible = true;
            overlayOpened = true;

        }
        $scope.focusSearch = function (index) {
           
            var element = document.getElementsByClassName('angucomplete-holder');
            element[index].style.width = '250px';
        }
        $scope.focusOutSearch = function (index) {
            var element = document.getElementsByClassName('angucomplete-holder');
            element[index].style.width = '50px';
        }
        $scope.selectClassifiedSolvent = function (item, index) {
            var btns = document.getElementsByClassName('classifiedBtn');
            for (var i = 0; i < btns.length; i++) {
                btns[i].className += ' disabled-element';
            }
            $('#closecross-solvents').addClass('disabled-element');
           
            $('#classBtn_' + index).button("loading");
            $http({
                method: 'POST',
                url: 'api/Analysis/SetClassifiedSolvent',
                params: { name: item.Name, analysisId: $routeParams.id, userId: $window.sessionStorage.userId }
            }).success(function succesCallback(data) {
                $scope.closeOverlay(selectedAlgorithm);
                $('#prevClassified-modal').modal('hide');
                showInstance = true;
                showClusterAnalysis(data);
                $('#closecross-solvents').removeClass('disabled-element');
                $('#classBtn_' + index).button("reset");
            });
        }

        $scope.changeName = function changeName() {
            $http({
                method: 'POST',
                url: 'api/Analysis/ChangeName',
                params: { name: $scope.newName, analysisId: $routeParams.id }
            }).success(function succesCallback(data) {
                notie.alert(1, "Name has been changed", 2);
                $scope.analysisName = $scope.newName;
                $scope.newName = "";
                $scope.errorChangeName = undefined;
                $('#changeName-model').modal('hide');
            }).error(function errorCallback(data) {
                $scope.errorChangeName = data.Message;
            });
        }

        $scope.undoShare = function() {
            $http({
                method: 'POST',
                url: 'api/Analysis/UndoShare',
                params: { id: data.Id }
            }).success(function succesCallback(data) {
                $scope.sharedWith = null;
                notie.alert(1, "Cluster Analysis is no longer shared with the organisation", 2);
            });
        }
        
        $scope.closeOverlay = function closeOverlay(name) {

            $(".cluster-div").removeClass("selected");
            closeSolventOverlay(name);
            $scope.overlayvisible = false;
            delete $scope.selectedNodeObject;
            delete $scope.selectedCluster;
            delete $scope.selectedSolvent;
            $('#overlay_' + name).addClass("not-visible");
            $('#overlay_' + name).removeClass("div-overlay");
            d3.selectAll("svg > *").remove();
            overlayOpened = false;
            $scope.showSolventInfo();

        }

        function closeSolventOverlay(name) {
            delete $scope.centeredSolvent;
            delete $scope.numberOfOtherSolvents;
            delete $scope.selectedNodeObject;
            delete $scope.selectedCluster;
            delete $scope.selectedSolvent;
            solventOverlayOpened = false;
            $scope.overlaySolventVisible = false;
            $('#overlay_solvent_' + name).addClass("not-visible");
            $('#overlay_solvent_' + name).removeClass("div-overlay");


        };

        $scope.closeSolventOverlayScope = function (name) {
            closeSolventOverlay(name);
        }

        function findModelOnName(name) {
            var model = null;
            for (var i = 0; i < models.length; i++) {
                if (models[i].Model.AlgorithmName === name) {
                    model = models[i].Model;
                }
            }
            return model;
        }



        function findAnalysisModelOnName(name) {
            for (var i = 0; i < models.length; i++) {
                if (models[i].Model.AlgorithmName === name) {
                    return models[i];
                }
            }
            return null;
        }

        function getClusterFromSolvent(solvent) {
            var model = findModelOnName(selectedAlgorithm);
            var cluster = null;
            for (var i = 0; i < model.Clusters.length; i++) {
                for (var j = 0; j < model.Clusters[i].Solvents.length; j++) {
                    if (model.Clusters[i].Solvents[j].CasNumber === solvent.CasNumber) {
                        cluster = model.Clusters[i];
                        $scope.selectedSolvent = cluster.Solvents[j];
                    }
                }
            }
            if (cluster !== null) {
                $('#overlay_' + model.AlgorithmName).removeClass("not-visible");
                $('#overlay_' + model.AlgorithmName).addClass("div-overlay");
                overlayOpened = true;

                var distances = [];
                for (var i = 0; i < cluster.Solvents.length; i++) {
                    distances.push(cluster.Solvents[i].DistanceToClusterCenter);
                }
                var max = Math.max.apply(Math, distances);
                for (var i = 0; i < cluster.Solvents.length; i++) {
                    cluster.Solvents[i].DistanceToClusterPercentage = (cluster.Solvents[i].DistanceToClusterCenter / max) * 95;
                    
                }
                
                createClusterChart(model.Clusters[cluster.Number]);
                $scope.solventsInCluster = cluster.Solvents;
                $scope.cluster = cluster.Number;
            }
        }

        $scope.clearNewSolvent = function() {
            //setMinMaxValues();
            delete $scope.errorMessage;
        }
        $scope.validateMetaData = function (metadata) {
            
            if (metadata.MetaDataName == "ID CAS Nr") {
                if (!casnrRegex.test(metadata.value)) {
                    $scope.validation[2] = true;
                } else {
                    $scope.validation[2] = false;
                }
            } else if (metadata.MetaDataName == "ID EG Nr") {
                if (!egRegex.test(metadata.value)) {
                    $scope.validation[3] = true;
                } else {
                    $scope.validation[3] = false;
                }
            }
            var classify = true;
            for (var i = 0; i < $scope.validation.length; i++) {
                if ($scope.validation[i] == true) {
                    classify = false;
                }
            }
            $scope.classify = classify;
        }

        $scope.validateFeatureData = function (featuredata, index) {
            
            if (isNaN(featuredata.value)) {
                $scope.validation[index + 9] = true;
            } else {
                $scope.validation[index + 9] = false;

            }
            var classify = true;
            for (var i = 0; i < $scope.validation.length; i++) {
                if ($scope.validation[i] == true) {
                    classify = false;
                }
            }
            $scope.classify = classify;
            $scope.errorMessageFeature = "Is not a number";
        }
        $scope.triggerDownload = function () {
            //var url = "http://localhost:14719/Resources/template.csv";
            //window.open(url, "template");

            var data = "Source;ID Name;ID CAS Nr;ID EG Nr;ID EG Annex Nr;EHS S Score;EHS H Score;EHS E Score;EHS Color Code;";
            var solvent = solvents[0];
            for (var i = 0; i < solvent.Features.length; i++) {
                if (i == solvent.Features.length - 1) {
                    data += solvent.Features[i].featureName;
                } else {
                    data += solvent.Features[i].featureName + ";";
                }
                
            }
            var blob = new Blob([data], { type: 'text/csv' });
            var url = $window.URL || $window.webkitURL;
            $scope.url = url.createObjectURL(blob);
        }


        $scope.triggerUpload = function () {
            $("#csvFileUpload").click();
        };
        $scope.csvFile = [];
        $scope.getFile = function (e, files) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var string = reader.result;
                var csv = string.split("\n");
                if (csv.length < 2) {
                    $scope.errorMessage = "Your csv doesn't contain enough lines";
                    $scope.$apply();
                    return false;
                }
                var headers = csv[0].split(";").filter(p => p != '' || p.length > 0);
                
                if (headers.length !== solvents[0].Features.length + 9) { //0.5.0.13 && headers.length !== features.length + 10
                    $scope.errorMessage = "Your csv doens't contain the right amount of headers or it isn't split on ;";
                    $scope.$apply();
                    return false;
                }
                var values = csv[1].split(";");
                if (values.length !== solvents[0].Features.length + 9) { //0.5.0.13
                    $scope.errorMessage = "Your csv doens't contain the right amount of values or it isn't split on ;";
                    $scope.$apply();
                    return false;
                }
               /* headers[0] = headers[0].substr(1);
                headers[headers.length - 1] = headers[headers.length - 1].substr(0, headers[headers.length-1].length - 2);
                values[0] = values[0].substr(1);
                values[values.length - 1] = values[values.length - 1].substr(0, values[headers.length - 1].length - 2);*/
                var tempMetadata = headers.slice(0, 9);
                var tempFeaters = headers.slice(9, headers.length); // 0.5.0.13
                var MetaDataObjects = [];
                var FeatureObjects = [];
                var error = [];
                for (var i = 0; i < tempMetadata.length; i++) {
                    MetaDataObjects.push({ MetaDataName: tempMetadata[i], value: values[i] });
                    error.push(false);
                }
                for (var i = 0; i < tempFeaters.length; i++) {
                    tempFeaters[i].value = values[i];
                    FeatureObjects.push({ FeatureName: tempFeaters[i], value: "" });
                    error.push(false);
                }

                for (var i = 0; i < FeatureObjects.length; i++) {
                    FeatureObjects[i].value = Number(values[i + 9].replace(',', '.')); // 0.5.0.13
                }
                $scope.headerz = [];
                $scope.validation = error;
                var h = []

                h.push(MetaDataObjects);
                h.push(FeatureObjects);
                $scope.headerz = h;
                if (checkHeaders(headers)) {
                    checkValues(values, headers);
                }
                $scope.check = true;
                $scope.allValuesValid = true;
                $scope.$apply();
                $("#csvFile").val('');
                return true;
            }
            reader.readAsText(files[0]);
            
        };
        $scope.validateAll = function (values) {
            var meta = values[0];
            var features = values[1];

            for (var i = 0; i < meta.length; i++) {
                $scope.validateMetaData(meta[i]);
            }
            for (var i = 0; i < features.length; i++) {
                $scope.validateFeatureData(features[i],i);
            }
        }
        function checkValues(arrValues, arrHeaders) {
           
            $scope.headerz[0][1].value = arrValues[1];
            $scope.headerz[0][2].value = arrValues[2];
            for (var i = 0; i < $scope.headerz[0].length; i++) {
                $scope.headerz[0][i].value = arrValues[i];
            }
            for (var i = 0; i < $scope.headerz[1].length; i++) {
                $scope.headerz[1][i].value = Number(arrValues[i + 9].replace(',', '.'));
            }
            delete $scope.errorMessage;
            return true;
        }
        //0.5.0.6
        function checkHeaders(arrHeaders) {
            var metaData = [];

            metaData.push("Source");
            metaData.push("ID Name");
            metaData.push("ID CAS Nr");
            metaData.push("ID EG Nr");
            metaData.push("ID EG Annex Nr");
            metaData.push("EHS S Score");
            metaData.push("EHS H Score");
            metaData.push("EHS E Score");
            metaData.push("EHS Color Code");
            for (var i = 0; i < 9; i++) {
                if (arrHeaders[i] !== metaData[i]) {
                    $scope.errorMessage = "Wrong input in header metaData: " + arrHeaders[i];
                    $scope.$apply();
                    return false;
                }
            }
            //Fix
            //for (var i = 6; i < arrHeaders.length; i++) {
            //    if (arrHeaders[i].replace("\r", "") !== minMaxValues[i-6].FeatureName) {
            //        $scope.errorMessage = "Wrong input in header feature names: " + arrHeaders[i] + " must be " + minMaxValues[i - 6].FeatureName;
            //        $scope.$apply();
            //        return false;
            //    }
            //}
            
            
            return true;
        }

        function buildMatrix(clusterTemp) {
            
            buildDistanceMatrix.init();

            for (var j = 0; j < clusterTemp.Solvents.length; j++) {
                var distances = [];
                for (var k = 0; k < clusterTemp.Solvents.length; k++) {
                    var vector1 = [];
                    var vector2 = [];
                    for (var l = 0; l < clusterTemp.Solvents[j].Features.length ; l++) {
                        vector1[l] = clusterTemp.Solvents[j].Features[l].value;
                        vector2[l] = clusterTemp.Solvents[k].Features[l].value;
                    }
                    var distance = getEuclidianDistance(vector1, vector2);
                    distances.push(distance);
                }
                buildDistanceMatrix.addRowWithDistances(distances);
            }
            
            return buildDistanceMatrix.getMatrix();
        }
        //[0.5.0.6] Gets all the clusters from a model given by a algorithm (Canopy, SOM, Xmeans) and returns the distance matrix
        //It uses the DistanceToCluster values from each cluster
        function buildMatrix2(clusters) {
            var distancematrix = [];
            for (var i = 0; i < clusters.length; i++) {
                var lines = [];
                var distances = clusters[i].DistanceToClusters;
                for (var j = 0; j < distances.length; j++) {
                    var distanceobj = distances[j]
                    var distance = distanceobj.Distance;
                    lines.push(distance);
                }
                distancematrix.push(lines);
            }
            return distancematrix;
        }
        

        function createClusterChart(clusterTemp) {
            
            var cluster = JSON.parse(JSON.stringify(clusterTemp));
            if (showInstance) {
                var currentModel = findAnalysisModelOnName(selectedAlgorithm);
                if (currentModel.ClassifiedInstance.ClusterNumber === cluster.Number) {
                    cluster.Solvents.push(currentModel.ClassifiedInstance);
                }
            }
            for (var i = 0; i < cluster.DistanceToClusters.length; i++) {
                cluster.DistanceToClusters[i].Distance = Number(cluster.DistanceToClusters[i].Distance.toFixed(2));
            }

            var width = 700, height = 440;

            var color = d3.scale.category20();

            var force = d3.layout.force()
                .charge(-120)
                .linkDistance(function (d) { return d.distance; })
                .size([width, height]);

            var svg = d3.select("#clusterChart_" + selectedAlgorithm)
                .html("")
            .attr("width", width)
            .attr("height", height);

            var distances = [];

            for (var i = 0; i < cluster.Solvents.length; i++) {
                distances.push(cluster.Solvents[i].DistanceToClusterCenter);
            }

            var distancesNormalized = getNormalizedValues(distances);
            var jsonNodes = [];
            var jsonLinks = [];
            jsonNodes.push({
                "name": "Cluster center",
                "group": 2,
                "value": 20,
                "distance": 0,
                "casNumber": "None",
                "cluster": cluster
            });
            var maxSolvent, minSolvent;

            for (var i = 0; i < cluster.Solvents.length; i++) {
                if (maxSolvent === undefined || cluster.Solvents[i].DistanceToClusterCenter > maxSolvent.DistanceToClusterCenter) {
                    maxSolvent = cluster.Solvents[i];
                }
                if (minSolvent === undefined || cluster.Solvents[i].DistanceToClusterCenter < minSolvent.DistanceToClusterCenter) {
                    minSolvent = cluster.Solvents[i];
                }
                var node = { "name": cluster.Solvents[i].Name, "group": 1, "casNumber": cluster.Solvents[i].CasNumber, "distance": cluster.Solvents[i].DistanceToClusterCenter.toFixed(2), "value": 10, "solvent": cluster.Solvents[i] };
                jsonNodes.push(node);
                var link = { "source": i + 1, "target": 0, "distance": (distancesNormalized[i] * 160) + 33 };
                jsonLinks.push(link);

            }

            $scope.maxDistance = maxSolvent.DistanceToClusterCenter.toFixed(2);
            $scope.minDistance = minSolvent.DistanceToClusterCenter.toFixed(2);
            $scope.maxSolvent = maxSolvent;
            $scope.minSolvent = minSolvent;

            var jsonGraph = {
                "nodes": jsonNodes,
                "links": jsonLinks
            };

            d3.json(jsonGraph, function (error, graph) {

                force
                    .nodes(jsonGraph.nodes)
                    .links(jsonGraph.links)

                    .start();

                var link = svg.selectAll(".link")
                    .data(jsonGraph.links)
                  .enter().append("line")
                    .attr("class", "link")
                    .style("stroke-width", 1)
                ;
                delete $scope.selectedNodeObject;
                delete $scope.selectedCluster;
                delete $scope.selectedD3Node;
                $scope.$apply();
                var selectedNode = null;
                var selectedCluster = null;
                var node = svg.selectAll(".node")
                    .data(jsonGraph.nodes)
                    .enter().append("circle")
                    .attr("class", "node")
                    .attr("r", function (d) { return d.value; })
                    .style("stroke", function (d) {
                        if ($scope.selectedSolvent !== undefined && $scope.selectedSolvent.CasNumber === d.casNumber) {
                            var selectedNodeObject = {
                                Name: d.name,
                                CasNumber: d.casNumber,
                                DistanceToClusterCenter: d.distance
                            };
                            selectedNode = this;
                            $scope.selectedNodeObject = selectedNodeObject;
                            $scope.$apply();
                            return "red";
                        } else {
                            return "white";
                        }
                    })
                    .style("stroke-width", function (d) {
                        if ($scope.selectedSolvent !== undefined && $scope.selectedSolvent.CasNumber === d.casNumber) {
                            return "3";
                        } else {
                            return "1.5";
                        }
                    })
                    .style("fill", function (d) {
                        if (findAnalysisModelOnName(selectedAlgorithm).ClassifiedInstance !== null && findAnalysisModelOnName(selectedAlgorithm).ClassifiedInstance !== undefined) {
                            switch (d.casNumber) {
                                case
                                    findAnalysisModelOnName(selectedAlgorithm).ClassifiedInstance.CasNumber:
                                    return "#F4FE00";
                                    break;
                                case maxSolvent.CasNumber:
                                    return "#E68364";
                                    break;
                                case minSolvent.CasNumber:
                                    return "#26A65B";
                                    break;

                                default:
                                    //Hier kunnen wij de kleuren aan toevoegen
                                    return color(d.group);
                            }

                        } else {
                            switch (d.casNumber) {
                                case maxSolvent.CasNumber:
                                    return "#E68364";
                                    break;
                                case minSolvent.CasNumber:
                                    return "#26A65B";
                                    break;
                                default:
                                    return color(d.group);
                            }
                        }
                        
                    })
                    .on("click", function (d) {
                        if (window.event.ctrlKey) {
                            if (d.casNumber !== "None") {
                                handleCtrlClick(d, clusterTemp);
                            } else {
                                handleCtrlClickCenter(d, clusterTemp);
                            }
                        } else {
                            if (d.casNumber === "None") {
                                $scope.showSolventInfo();
                                $scope.selectedCluster = d.cluster;
                                if (selectedNode !== undefined) {
                                    d3.select(selectedNode).style("stroke", "white");
                                    d3.select(selectedNode).style("stroke-width", "1.5");
                                }

                                d3.select(this).style("stroke", "red");



                                selectedCluster = this;

                                

                            } else {
                                if (solvInfo) {
                                    $scope.showSolventInfo();
                                } else {
                                    $scope.showChemPic();
                                }
                                delete $scope.selectedCluster;
                                if (d.solvent !== undefined) {
                                    var selectedNodeObject = {
                                        Name: d.solvent.Name,
                                        CasNumber: d.solvent.CasNumber,
                                        DistanceToClusterCenter: Number(d.solvent.DistanceToClusterCenter.toFixed(3))
                                    };

                                    $scope.selectedNodeObject = selectedNodeObject;
                                    if (selectedNode !== undefined) {
                                        d3.select(selectedNode).style("stroke", "white");
                                        d3.select(selectedNode).style("stroke-width", "1.5");
                                    }
                                    if (selectedCluster !== null) {
                                        d3.select(selectedCluster).style("stroke", "white");

                                    }
                                    selectedCluster = null;
                                    d3.select(this).style("stroke", "red");
                                    d3.select(this).style("stroke-width", "3");
                                    selectedNode = this;
                                    $scope.selectedSolvent = d.solvent;
                                }

                            }
                            $scope.$apply();
                        }
                        
                        
                    });

                node.append("title")
                    .text(function (d) { return d.name; });

                force.on("tick", function () {
                    link.attr("x1", function (d) { return d.source.x; })
                        .attr("y1", function (d) { return d.source.y; })
                        .attr("x2", function (d) { return d.target.x; })
                        .attr("y2", function (d) { return d.target.y; });

                    node.attr("cx", function (d) { return d.x; })
                        .attr("cy", function (d) { return d.y; });
                });
            });
        };
   
        function createBulletChart(otherSolvents, normalizedDistances) {
            var width = 730, height = otherSolvents.length > 10 ? 40 * otherSolvents.length : 400;
            if (height > $("#scrollable-div").height() || otherSolvents.length >= 10) {
                $("#scrollable-div").css("overflow-y", "scroll");
            }
            var selectedBar = null;
            otherSolvents.sort(function(a, b) {
                return a.distanceToCentralSolvent - b.distanceToCentralSolvent;
            });
            normalizedDistances.sort();
            var data = [];
            for (var i = 0; i < otherSolvents.length; i++) {
                var dataPart = {
                    label: otherSolvents[i].Name + " ",
                    value: normalizedDistances[i],
                    solvent: otherSolvents[i]
                }
                data.push(dataPart);
            }
            
            var div = d3.select("body").append("div").attr("class", "toolTip");

            var axisMargin = 40,
                    margin = 30,
                    valueMargin = 15,
                    barHeight = (height - axisMargin - margin * 2) * 0.4 / data.length,
                    barPadding = (height - axisMargin - margin * 2) * 0.6 / data.length,
                    data, bar, svg, scale, xAxis, labelWidth = 0;

            max = d3.max(data, function (d) { return d.value; });

            svg = d3.select("#solventChart_" + selectedAlgorithm)
                .html("")
                .attr("width", width)
                .attr("height", height);

          
            bar = svg.selectAll("g")
                    .data(data)
                    .enter()
                    .append("g");

            bar.attr("class", "bar")
                    .attr("cx", 0)
                    .attr("transform", function (d, i) {
                        return "translate(" + margin + "," + (i * (barHeight + barPadding) + barPadding) + ")";
                    });

            bar.append("text")
                    .attr("class", "label")
                    .attr("style", "margin-right: 10px; font-size:100%")
                    .attr("y", barHeight / 2)
                    .attr("dy", ".35em") //vertical align middle
                    .text(function (d) {
                        return d.label;
                    }).each(function () {
                        labelWidth = Math.ceil(Math.max(labelWidth, this.getBBox().width));
                    });

            scale = d3.scale.linear()
                    .domain([0, max])
                    .range([0, width - margin * 2 - labelWidth]);

            xAxis = d3.svg.axis()
                    .scale(scale)
                    .tickSize(-height + 2 * margin + axisMargin)
                    .orient("bottom");

            bar.append("rect")
                    .attr("transform", "translate(" + (labelWidth+5) + ", 0)")
                    .attr("height", barHeight)
                    .attr("width", 0)
                    .transition()
                    .duration(700)
                    .ease("linear")
                    .attr("width", function(d) {
                    return scale(d.value);
                })
                    .style("fill", "rgb(70, 130, 180)");

            bar.append("text")
                    .attr("class", "value")
                    .attr("y", barHeight / 2)
                    .attr("dx", -valueMargin + labelWidth) //margin right
                    .attr("dy", ".35em") //vertical align middle
                    .attr("text-anchor", "end")
                    
                    .attr("x", function (d) {
                        var width = this.getBBox().width;
                        return Math.max(width + valueMargin, scale(d.value));
                    });

            bar
                    .on("mousemove", function (d) {
                        div.style("left", d3.event.pageX + 10 + "px");
                        div.style("top", d3.event.pageY - 25 + "px");
                        div.style("display", "inline-block");
                        div.html((d.label) + "<br>Distance: " + d.value);
                        d3.select(this).style("cursor", "pointer");

                    });
            bar
                    .on("mouseout", function (d) {
                        div.style("display", "none");
                        d3.select(this).style("cursor", "default");
                    });
            bar.on("click",
                function (d) {
                    if (d.solvent !== undefined) {
                        
                        
                        if (selectedBar != undefined) {
                            d3.select(selectedBar).style("fill", "steelblue");
                        }
                        selectedBar = this;
                        d3.select(this).style("fill", "rgb(241, 169, 78)");


                        var selectedNodeObject = {
                            Name: d.solvent.Name,
                            CasNumber: d.solvent.CasNumber
                            
                        };
                      
                        $scope.selectedNodeObject = selectedNodeObject;
                        
                        $scope.selectedSolvent = d.solvent;
                        $scope.$apply();
                    }
                });

            svg.insert("g", ":first-child")
                    .attr("class", "axisHorizontal")
                    .attr("transform", "translate(" + (margin + labelWidth + 5) + "," + (height - axisMargin - margin - 28) + ")")
                    .call(xAxis);

           
        }

        $(document).keyup(function (e) {
            if (e.keyCode == 27) { // escape key maps to keycode `27`
                if (solventOverlayOpened) {
                    closeSolventOverlay(selectedAlgorithm);
                } else if (overlayOpened) {
                    $scope.closeOverlay(selectedAlgorithm);
                }

                $scope.$apply();

            }
        });
        function handleCtrlClick(clickEvent, clusterTemp) {
           
            var newInstance = $scope.ClassifiedInstance;
            var copiedCluster = jQuery.extend(true,{}, clusterTemp);
            if (newInstance) {
                copiedCluster.Solvents.push(newInstance);
            }
            
            var matrix = buildMatrix(copiedCluster);

            delete $scope.selectedNodeObject;
            delete $scope.selectedCluster;
            delete $scope.selectedSolvent;
            var solvents = Object.create(copiedCluster.Solvents);
            $('#overlay_solvent_' + selectedAlgorithm).removeClass("not-visible");
            $('#overlay_solvent_' + selectedAlgorithm).addClass("div-overlay");
            solventOverlayOpened = true;
            $scope.overlaySolventVisible = true;

            var distances = [];
            for (var j = 0; j < solvents.length; j++) {
                if (j !== (clickEvent.index - 1)) {
                    solvents[j].distanceToCentralSolvent = matrix[clickEvent.index - 1][j];
                    distances.push(solvents[j].distanceToCentralSolvent);
                }
            }
            distances = getNormalizedValuesWithFixedMin(distances, 0);
            solvents.splice((clickEvent.index - 1), 1);
            
            createBulletChart(solvents, distances);
            $scope.centeredSolvent = clickEvent.solvent.Name;
            $scope.numberOfOtherSolvents = solvents.length;
            $scope.$apply();
        }
        function handleCtrlClickCenter(clickEvent, clusterTemp) {
            var copiedCluster = jQuery.extend(true, {}, clusterTemp);
            var matrix = buildMatrix(copiedCluster);
            
            drawDistanceMatrix(matrix, clusterTemp);
            //alert($scope.matrix[0][0]);
        }

        function drawDistanceMatrix(matrix, clustertemp) {
            $scope.matrix = matrix;
            $scope.$apply();
            var normalizeddistancevalues = [];
            

            var largestdis = 0;
            var smallestdis = 0;
            
            for (var i = 0; i < matrix.length; i++) {
                for (var j = 0; j < matrix[i].length; j++) {

                    if (largestdis < matrix[i][j]) {
                        largestdis = matrix[i][j];
                    }
                    
                    if (smallestdis > matrix[i][j]) {
                        smallestdis = matrix[i][j];
                    }
                    

                }
                
            }

            for (o = 0; o < matrix.length; o++) {

                normalizeddistancevalues[o] = getNormalizedValuesDis(matrix[o], largestdis,smallestdis);
                var l = 'l';
            }

            $('#distanceMatrixDiv').removeClass("not-visible");
            $('#distanceMatrixDiv').addClass("div-overlay");
            //$scope.selectedCentroid = true;
            var parentDiv = document.getElementById('distanceMatrixDiv');
            if (document.getElementById('distanceMatrixTable')) {
                document.getElementById('distanceMatrixTable').remove();
            }
            
            var tbl = document.createElement('table');
            tbl.setAttribute("id", "distanceMatrixTable");
            //tbl.style.tableLayout = 'fixed';
            tbl.style.width = '100%';
            tbl.style.height = '100%';
            tbl.style.overflow = 'scroll';
            tbl.style.textShadow = '1px 0 0 #000, 0 -1px 0 #000, 0 1px 0 #000, -1px 0 0 #000';
            tbl.setAttribute('border', '1');
            var tbdy = document.createElement('tbody');
            tbdy.style.overflow = 'scroll';
            
            var maxRow = normalizeddistancevalues.map(function (row) { return Math.max.apply(Math, row); });
            var max = Math.max.apply(null, maxRow);
            var tr = document.createElement('tr');
            var tdhf = document.createElement('td');
            tr.appendChild(tdhf);
            for (var i = 0; i < clustertemp.Solvents.length; i++){

                var tdh = document.createElement('td');

                tdh.style.minWidth = '100px';
                tdh.style.minHeight = '50px';
                tdh.style.textAlign = 'center';
                tdh.appendChild(document.createTextNode(clustertemp.Solvents[i].Name));
                tr.appendChild(tdh);
            }
            tbdy.appendChild(tr);
            for (var i = 0; i < matrix.length; i++) {
                var tr = document.createElement('tr');
                var tdhs = document.createElement('td');
                tdhs.appendChild(document.createTextNode(clustertemp.Solvents[i].Name))
                tr.appendChild(tdhs);
                
                for (var j = 0; j < matrix.length ; j++) {
                    var td = document.createElement('td');
                    td.style.minWidth = '100px';
                    td.style.minHeight = '50px';
                    td.style.textAlign = 'center';
                    td.style.backgroundColor = getColorForPercentage(parseFloat(normalizeddistancevalues[i][j]).toFixed(2) / max)
                    td.appendChild(document.createTextNode(parseFloat(normalizeddistancevalues[i][j]).toFixed(2)));
                    tr.appendChild(td)
                    
                }
                tbdy.appendChild(tr);
            }
            tbl.appendChild(tbdy);
            parentDiv.appendChild(tbl);
            $scope.$apply();
            
        }
        function getNormalizedValues(lengths) {
            if (lengths.length === 1) {
                return [0];
            }
            var max = Math.max.apply(Math, lengths);
            var min = Math.min.apply(Math, lengths);
            var normalizedValues = [];
            for (var i = 0; i < lengths.length; i++) {
                normalizedValues.push((lengths[i] - min) / (max - min));
            }
            return normalizedValues;
        }

        function getNormalizedValuesDis(lengths, maxdis,mindis) {
            if (lengths.length === 1) {
                return [0];
            }
           
            var normalizedValues = [];
            for (var i = 0; i < lengths.length; i++) {
                normalizedValues.push((lengths[i] - mindis) / (maxdis - mindis));
            }
            return normalizedValues;
        }
        var percentColors = [
    { pct: 0.0, color: { r: 178, g: 178, b: 242 } }, 	
    { pct: 0.5, color: { r: 127, g: 127, b: 234 } }, //	127	127	234
    { pct: 1.0, color: { r: 76, g: 76, b: 226 } }]; //76 76	226

        var getColorForPercentage = function (pct) {
            for (var i = 1; i < percentColors.length - 1; i++) {
                if (pct < percentColors[i].pct) {
                    break;
                }
            }
            var lower = percentColors[i - 1];
            var upper = percentColors[i];
            var range = upper.pct - lower.pct;
            var rangePct = (pct - lower.pct) / range;
            var pctLower = 1 - rangePct;
            var pctUpper = rangePct;
            var color = {
                r: Math.floor(lower.color.r * pctLower + upper.color.r * pctUpper),
                g: Math.floor(lower.color.g * pctLower + upper.color.g * pctUpper),
                b: Math.floor(lower.color.b * pctLower + upper.color.b * pctUpper)
            };
            return 'rgb(' + [color.r, color.g, color.b].join(',') + ')';
            // or output as hex if preferred
        }

        $scope.distanceMatrixClose = function() {

            $('#distanceMatrixDiv').removeClass("div-overlay");
            $('#distanceMatrixDiv').addClass("not-visible");
        }


        $scope.showSolventInfo = function () {
            solvInfo = true;
            var parentDiv = document.getElementById('solventDetailsDiv');
            var otherDiv = document.getElementById('ChemSolPicDiv');
            var ButChem = document.getElementById('ButChemStruct');
            var ButSolv = document.getElementById('ButSolvDet');
            ButChem.style.backgroundColor = 'transparent';
            ButSolv.style.backgroundColor = '#b92ed1';
            parentDiv.style.display = 'unset';
            otherDiv.style.display = 'none';

            
        }

        $scope.showChemPic = function () {
            solvInfo = false;
            var parentDiv = document.getElementById('ChemSolPicDiv');
            var otherDiv = document.getElementById('solventDetailsDiv');

            var ButChem = document.getElementById('ButChemStruct');
            var ButSolv = document.getElementById('ButSolvDet');

            ButChem.style.backgroundColor = '#b92ed1';

            ButSolv.style.backgroundColor = 'transparent';

            parentDiv.style.display = 'unset';
            otherDiv.style.display = 'none';
            if (typeof $scope.selectedCluster !== 'undefined') {
                $scope.showSolventInfo();
            }
            if (typeof $scope.selectedSolvent === 'undefined' || $scope.selectedSolvent == 'null') {
                
                //alert('no solvent selected');
            } else {
                var urlPic = $scope.trustSrc("https://www.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pccompound&retmax=100&term=" + $scope.selectedSolvent.CasNumber); 
                var result;
                    $.ajax({
                        type: "GET",
                        url: urlPic,
                        dataType: "xml",
                        success: function (xml) {
                            result = $(xml).find("Id").text();
                            $scope.cidurl = "https://embed.molview.org/v1/?mode=balls&cid=" + result.toString();
                            $sce.trustAsResourceUrl($scope.cidurl);
                            $scope.$apply();
                        },
                        error: function (xml) {
                            alert('Er ging iets mis, gelieve de volgende code naar de administrator te sturen: ' + xml.status + ' ' + xml.statusText);
                        },
                    });
            }
        }

        $scope.trustSrc = function (src) {
            return $sce.trustAsResourceUrl(src);
        }


        
    });





angular.module('sussol.services')
.directive('fileChange', ['$parse', function ($parse) {
    return {
        require: 'ngModel',
        restrict: 'A',
        link: function ($scope, element, attrs, ngModel) {
            var attrHandler = $parse(attrs['fileChange']);
            var handler = function (e) {
                $scope.$apply(function () {
                    attrHandler($scope, { $event: e, files: e.target.files });
                });
            };
            element[0].addEventListener('change', handler, false);
        }
    }
}]);