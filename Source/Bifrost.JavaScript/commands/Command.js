Bifrost.namespace("Bifrost.commands", {
    Command: Bifrost.Type.extend(function (commandCoordinator, commandValidationService, commandSecurityService, mapper, options, region) {
        var self = this;
        var hasChangesObservables = ko.observableArray();

        this.region = region;
        this._name = "";
        this._generatedFrom = "";
        this.targetCommand = this;
        this.validators = ko.observableArray();
        this.validationMessages = ko.observableArray();
        this.securityContext = ko.observable(null);
        this.populatedFromExternalSource = ko.observable(false);


        this.isBusy = ko.observable(false);
        this.isValid = ko.computed(function () {
            var valid = true;
            self.validators().some(function (validator) {
                if (ko.isObservable(validator.isValid) && validator.isValid() === false) {
                    valid = false;
                    return true;
                }
            });

            if (self.validationMessages().length > 0) {
                return false;
            }

            return valid;
        });
        this.isAuthorized = ko.observable(false);
        this.canExecute = ko.computed(function () {
            return self.isValid() && self.isAuthorized();
        });
        this.isPopulatedExternally = ko.observable(false);
        this.isReady = ko.computed(function () {
            if (self.isPopulatedExternally() === false) {
                return true;
            }
            return self.populatedFromExternalSource();
        });
        this.isReadyToExecute = ko.computed(function () {
            if (self.isPopulatedExternally() === false) {
                return true;
            }

            return self.hasChanges();
        });


        this.hasChanges = ko.computed(function () {
            var hasChange = false;
            hasChangesObservables().some(function (item) {
                if (item() === true) {
                    hasChange = true;
                    return true;
                }
            });

            return hasChange;
        });

        this.failedCallbacks = [];
        this.succeededCallbacks = [];
        this.completedCallbacks = [];

        this.commandCoordinator = commandCoordinator;
        this.commandValidationService = commandValidationService;
        this.commandSecurityService = commandSecurityService;

        this.options = {
            beforeExecute: function () { },
            failed: function () { },
            succeeded: function () { },
            completed: function () { },
            properties: {}
        };

        this.failed = function (callback) {
            self.failedCallbacks.push(callback);
            return self;
        };
        this.succeeded = function (callback) {
            self.succeededCallbacks.push(callback);
            return self;
        };
        this.completed = function (callback) {
            self.completedCallbacks.push(callback);
            return self;
        };

        this.setOptions = function (options) {
            Bifrost.extend(self.options, options);
            if (typeof options.name !== "undefined" && typeof options.name === "string") {
                self._name = options.name;
            }
        };

        this.copyPropertiesFromOptions = function () {
            for (var property in self.targetCommand.options.properties) {
                var value = self.targetCommand.options.properties[property];
                if (!ko.isObservable(value)) {
                    value = ko.observable(value);
                }

                self.targetCommand[property] = value;
            }
        };

        this.getProperties = function () {
            var properties = [];
            for (var property in self.targetCommand) {
                if (self.targetCommand.hasOwnProperty(property) && 
                    !(self.hasOwnProperty(property))) {
                    properties.push(property);
                }
            }

            return properties;
        };

        this.makePropertiesObservable = function () {
            var properties = self.getProperties();
            properties.forEach(function (property) {
                var value = null;
                var propertyValue = self.targetCommand[property];

                if (!ko.isObservable(propertyValue) &&
                     (typeof propertyValue !== "object" || Bifrost.isArray(propertyValue))) {

                    if (typeof propertyValue !== "function") {
                        if (Bifrost.isArray(propertyValue)) {
                            value = ko.observableArray(propertyValue);
                        } else {
                            value = ko.observable(propertyValue);
                        }
                        self.targetCommand[property] = value;
                    }
                }
            });
        };

        this.extendPropertiesWithHasChanges = function () {
            var properties = self.getProperties();
            properties.forEach(function(property) {
                var propertyValue = self.targetCommand[property];
                if (ko.isObservable(propertyValue)) {
                    propertyValue.extend({ hasChanges: {} });
                    if (!Bifrost.isNullOrUndefined(propertyValue.hasChanges)) {
                        hasChangesObservables.push(propertyValue.hasChanges);
                    }
                }
            });
        };

        this.onBeforeExecute = function () {
            self.options.beforeExecute();
        };

        this.onFailed = function (commandResult) {
            self.options.failed(commandResult);

            self.failedCallbacks.forEach(function (callback) {
                callback(commandResult);
            });
        };

        this.setInitialValuesForProperties = function (properties) {
            properties.forEach(function (propertyName) {
                var property = self.targetCommand[propertyName];
                if (ko.isObservable(property) &&
                    ko.isWriteableObservable(property) &&
                    Bifrost.isFunction(property.setInitialValue)) {
                    var value = property();
                    property.setInitialValue(value);
                }
            });
        };

        this.setInitialValuesFromCurrentValues = function () {
            var properties = self.getProperties();
            self.setInitialValuesForProperties(properties);
        };

        this.onSucceeded = function (commandResult) {
            self.options.succeeded(commandResult);

            self.setInitialValuesFromCurrentValues();

            self.succeededCallbacks.forEach(function (callback) {
                callback(commandResult);
            });
        };

        this.onCompleted = function (commandResult) {
            self.options.completed(commandResult);

            self.completedCallbacks.forEach(function (callback) {
                callback(commandResult);
            });
        };

        this.handleCommandResult = function (commandResult) {
            self.isBusy(false);
            if (typeof commandResult.commandValidationMessages !== "undefined") {
                self.validationMessages(commandResult.commandValidationMessages);
            }

            if (commandResult.success === false || commandResult.invalid === true) {
                if (commandResult.invalid && typeof commandResult.validationResults !== "undefined") {
                    self.commandValidationService.applyValidationResultToProperties(self.targetCommand, commandResult.validationResults);
                }
                self.onFailed(commandResult);
            } else {
                self.onSucceeded(commandResult);
            }
            self.onCompleted(commandResult);
        };

        this.getCommandResultFromValidationResult = function (validationResult) {
            var result = Bifrost.commands.CommandResult.create();
            result.invalid = true;
            return result;
        };

        this.execute = function () {
            self.isBusy(true);
            try {
                self.onBeforeExecute();
                var validationResult = self.commandValidationService.validate(this);
                if (validationResult.valid === true) {
                        self.commandCoordinator.handle(self.targetCommand).continueWith(function (commandResult) {
                            self.handleCommandResult(commandResult);
                        });
                } else {
                    var commandResult = self.getCommandResultFromValidationResult(validationResult);
                    self.handleCommandResult(commandResult);
                }
            } catch (ex) {
                self.isBusy(false);
            }
        };

        this.populatedExternally = function () {
            self.isPopulatedExternally(true);
        };

        this.populateFromExternalSource = function (values) {
            self.isPopulatedExternally(true);
            self.setPropertyValuesFrom(values);
            self.populatedFromExternalSource(true);
            commandValidationService.clearValidationMessagesFor(self.targetCommand);
        };

        this.setPropertyValuesFrom = function (values) {
            var mappedProperties = mapper.mapToInstance(self.targetCommand._type, values, self.targetCommand);
            self.setInitialValuesForProperties(mappedProperties);
        };

        this.onCreated = function (lastDescendant) {
            self.targetCommand = lastDescendant;
            if (typeof options !== "undefined") {
                this.setOptions(options);
                this.copyPropertiesFromOptions();
            }
            this.makePropertiesObservable();
            this.extendPropertiesWithHasChanges();
            if (typeof lastDescendant._name !== "undefined" && lastDescendant._name !== "") {
                commandValidationService.extendPropertiesWithoutValidation(lastDescendant);
                var validators = commandValidationService.getValidatorsFor(lastDescendant);
                if (Bifrost.isArray(validators) && validators.length > 0) {
                    self.validators(validators);
                }
                commandValidationService.validateSilently(this);
            }
            
            commandSecurityService.getContextFor(lastDescendant).continueWith(function (securityContext) {
                lastDescendant.securityContext(securityContext);

                if (ko.isObservable(securityContext.isAuthorized)) {
                    lastDescendant.isAuthorized(securityContext.isAuthorized());
                    securityContext.isAuthorized.subscribe(function (newValue) {
                        lastDescendant.isAuthorized(newValue);
                    });
                }
            });
        };
    })
});