var Page = (function () {

    function getAverageFor(array) {
        var average = null;

        array.forEach(function (value) {
            if (null == average) {
                average = value;
            } else {
                average += value;
            }
        });

        return (average / array.length);
    }

    function CityElementType(type, name, icon) {
        this.type = ko.observable(type);
        this.name = ko.observable(name);
        this.icon = ko.observable(icon);
    }

    function CityElement(id, cityElementType) {
        var self = this;

        this.id = ko.observable(id);
        this.type = ko.observable(cityElementType.type());
        this.name = ko.observable(cityElementType.name());
        this.icon = ko.observable(cityElementType.icon());
        this.icons = ko.observableArray([]);
        this.size = ko.observable(0);
        this.state = ko.observable(0);

        this.resourceTypeDeficits = [];

        this.cssClass = ko.computed(function () {
            return 'city-element ' + self.type().toLowerCase() + ' underserved-state-' + self.state();
        }, this);

        this.increaseSize = function () {
            var s = self.size();
            s++;

            if (s > 3) {
                s = 1;
            }

            self.size(s);

            cityHub.server.changeElementSize(self.id(), self.size());
        };

        this.setState = function (deficit, resourceType) {
            var deficitsForResourceType = self.resourceTypeDeficits[resourceType];
            if (!deficitsForResourceType) {
                deficitsForResourceType = [];
            }

            deficitsForResourceType.push(deficit);
            if (10 < deficitsForResourceType.length) {
                deficitsForResourceType.shift();
            }

            self.resourceTypeDeficits[resourceType] = deficitsForResourceType;

            var averages = [];

            self.resourceTypeDeficits.forEach(function (deficits) {
                var localAverage = getAverageFor(deficits);
                averages.push(localAverage);
            });

            var average = getAverageFor(averages);
            self.state(Math.round(average) || 1);
        };

        this.size.subscribe(function (value) {
            self.icons.removeAll();

            for (var i = 0; i < value; i++) {
                self.icons.push(self.icon());
            }
        });

        this.size(1);
    }

    function setupDropDown($element) {
        $element.on('click', function (event) {
            $(this).toggleClass('active');
            event.stopPropagation();
        });
    }

    function viewModel() {
        var self = this;

        this.elementTypes = ko.observableArray([new CityElementType('Electricity', 'Electric company', 'icon-lightbulb'), new CityElementType('Water', 'Water tower', 'icon-tint'), new CityElementType('House', 'House', 'icon-home'), new CityElementType('Business', 'Business', 'icon-building'), new CityElementType('Trash', 'Trash incinerator', 'icon-trash'), new CityElementType('Sewage', 'Sewage treatment', 'icon-facebook')]);
        this.elements = ko.observableArray([]);

        this.addElement = function (elementType) {
            cityHub.server.addElement(elementType.type());
        };

        this.removeElement = function (element) {
            cityHub.server.removeElement(element.id());
        };

        this.onElementAdded = function (id, type) {
            self.elementTypes()
                .filter(function (item) {
                    return (item.type() == type);
                })
                .forEach(function (item) {
                    self.elements.push(new CityElement(id, item));
                });
        };

        this.onElementRemoved = function (id) {
            self.elements()
                .filter(function (item) {
                    return (item.id() == id);
                })
                .forEach(function (item) {
                    self.elements.remove(item);
                });
        };

        this.onConsumptionDataReported = function (id, deficit, resourceType) {
            self.elements()
                .filter(function (item) {
                    return (item.id() == id);
                })
                .forEach(function (item) {
                    item.setState(deficit, resourceType);
                });
        };

        this.onWasteReported = function (data) {
        };
    }

    var model = null;
    var cityHub = null;

    return {
        init: function () {
            cityHub = $.connection.city;

            cityHub.client.onElementAdded = function (id, type) {
                model.onElementAdded(id, type);
            };

            cityHub.client.onElementRemoved = function (id) {
                model.onElementRemoved(id);
            };

            cityHub.client.onConsumptionDataReported = function (id, deficit, resourceType) {
                model.onConsumptionDataReported(id, deficit, resourceType);
            };

            cityHub.client.onWasteReported = function (data) {
                model.onWasteReported(data);
            };

            $.connection.hub.start()
                .done(function () {
                    model = new viewModel();
                    ko.applyBindings(model);

                    cityHub.server.startCity();

                    setupDropDown($('#toolbox'));
                });
        }
    };
})();