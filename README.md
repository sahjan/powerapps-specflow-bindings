# Power Apps SpecFlow Bindings

[![Build Status](https://capgeminiuk.visualstudio.com/GitHub%20Support/_apis/build/status/CI-Builds/NuGet%20Packages/Capgemini.PowerApps.SpecFlowBindings?branchName=master)](https://capgeminiuk.visualstudio.com/GitHub%20Support/_build/latest?definitionId=195&branchName=master) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_powerapps-specflow-bindings&metric=alert_status)](https://sonarcloud.io/dashboard?id=Capgemini_powerapps-specflow-bindings) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Capgemini_powerapps-specflow-bindings&metric=coverage)](https://sonarcloud.io/dashboard?id=Capgemini_powerapps-specflow-bindings)

A SpecFlow bindings library for Power Apps.

The aim of this project is to make Power Apps test automation easier, faster and more accessible. It does this by providing a library of generic SpecFlow step bindings that adhere to test automation best practices. This allows effective automated tests to be written without the dependency on developers who are both proficient with Power Apps and test automation.

## Table of Contents

- [Installation](#Installation)
- [Usage](#Usage)
  - [Configuration](#Configuration)
  - [Writing feature files](#Writing-feature-files)
  - [Writing step bindings](#Writing-step-bindings)
  - [Test setup](#Test-setup)
- [Contributing](#Contributing)
- [Licence](#Licence)

## Installation

Follow the guidance in the **Installation and Setup** section in https://specflow.org/getting-started/. After installing the IDE integration and setting up your project, install the NuGet package.

```shell
PM> Install-Package Capgemini.PowerApps.SpecFlowBindings
```

Once the NuGet package is installed, follow the SpecFlow [documentation](https://specflow.org/documentation/Configuration/) on referencing an external binding library. At the time of writing - for SpecFlow 3.x - you should update a _specflow.json_ file in your project root as follows -

```json
{
    "stepAssemblies": [
        { "assembly": "Capgemini.PowerApps.SpecFlowBindings" }
    ]
}
```

### WebDrivers

We do not have a dependency on any specific WebDrivers. You will need to ensure that the correct WebDrivers are available in your project based on the browser that you are targetting. For example - if your configuration file is targetting Chrome - you can install the Chrome WebDriver via NuGet - 

```shell
PM> Install-Package Selenium.Chrome.WebDriver
```

## Usage

### Configuration

Installing the NuGet package creates a _power-apps-bindings.yml_ file in your project's root. This is used to configure the URL, browser, and users that will be used for your tests.

```yaml
url: SPECFLOW_POWERAPPS_URL
browserOptions:
  browserType: Chrome
  headless: true
  width: 1920
  height: 1080
  startMaximized: false
users:
  - username: SPECFLOW_POWERAPPS_USERNAME_SALESPERSON
    password: SPECFLOW_POWERAPPS_PASSWORD_SALESPERSON
    alias: a salesperson
```

The URL, usernames, and passwords will be set from environment variable (if found). Otherwise, the value from the config file will be used. The browserOptions node supports anything in the EasyRepro `BrowserOptions` class.

### Writing feature files

You can use the predefined step bindings to write your tests.

```gherkin
Scenario: User can create a new account
	Given I am logged in to the 'Sales Team Member' app as 'an admin'
	When I open the 'Accounts' sub area of the 'Customers' group
	Then I can see the 'New' command
```

Alternatively, write your own step bindings (see below).

### Writing step bindings

You can write your own step bindings that have thread-safe access to EasyRepro and the Selenium WebDriver. This ensures that your tests can be ran safely in parallel. You can do this by creating a class decorated with the SpecFlow `BindingAttribute` and inheriting from the `PowerAppsStepDefiner` base class. You can then create your SpecFlow step bindings by interacting with the `XrmApp` and `Driver` properties.

```csharp
[Binding]
public class MyCustomSteps : PowerAppsStepDefiner
{
    [Given(@"I have a custom step")]
    public void GivenIHaveACustomStep()
    {
      // Interact with the inherited EasyRepro 'Browser' object.
      // Interact with the inherited Selenium 'Driver' object.
    }
}
```

### Test setup

We avoid performing test setup via the UI. This speeds up test execution and makes the tests more robust (as UI automation is more fragile than using supported APIs). _Given_ steps should therefore be carried out using the [Client API](client-api), [WebAPI](web-api) or [Organization Service](org-service).

You can create test data by using the following _Given_ step -

```gherkin
Given I have created "a record"
```

This will look for a JSON file named _a record.json_ in the _data_ folder (you must ensure that these files are copying to the build output directory). The JSON is the same as expected by WebAPI when creating records via a [deep insert](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/create-entity-web-api#create-related-entities-in-one-operation). The example below will create the following -

- An account
- An primary contact related to the account
- An opportunity related to the account
- A task related to the opportunity

```json
{
  "@logicalName": "account",
  "@alias": "sample account",
  "name": "Sample Account",
  "primarycontactid": {
    "firstname": "John",
    "lastname": "Smith"
  },
  "opportunity_customer_accounts": [
    {
      "name": "Opportunity associated to Sample Account",
      "Opportunity_Tasks": [{ "subject": "Task associated to opportunity" }]
    }
  ]
}
```

The `@logicalName` property is required for the root record.

The `@alias` property can optionally be added to any record and allows the record to be referenced in certain bindings. The _Given I have created_ binding itself supports relating records using `@alias.bind` syntax.

```json
{
  "@logicalName": "account",
  "@alias": "sample account",
  "name": "Sample Account",
  "primarycontactid@alias.bind": "sample contact" 
}
```

We also support the use of 
[faker.js](https://github.com/marak/Faker.js) moustache template syntax for generating dynamic test data at run-time. Please refer to the faker documentation for all of the functionality that is available. 

The below JSON will generate a contact with a random name, credit limit, email address, and date of birth in the past 90 years:

```json
{
  "@logicalName": "contact",
  "@alias": "a dynamically generated contact",
  "lastname": "{{name.firstName}}",
  "firstname": "{{name.lastName}}",
  "creditlimit@faker.number": "{{finance.amount}}",
  "emailaddress1": "{{internet.email}}",
  "birthdate@faker.date":  "{{date.past(90)}}"
}
```

When using faker syntax, you must also annotate number or date fields using `@faker.number`, `@faker.date` or `@faker.dateonly` to ensure that the JSON is formatted correctly.

## Contributing

Please refer to the [Contributing](./CONTRIBUTING.md) guide.

## Licence

Power Apps SpecFlow Bindings is released under the [MIT licence](./LICENCE).
