[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_Equalizer)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=bugs)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)

[![Core validation](https://github.com/TryAtSoftware/CleanTests/actions/workflows/Core%20validation.yml/badge.svg)](https://github.com/TryAtSoftware/CleanTests/actions/workflows/Core%20validation.yml)

# About the project

`TryAtSoftware.CleanTests` is a library that should simplify the process of automated testing for complex setups.

The repeating pattern that we could discover in some advanced projects is that whenever more features are added or old ones are being refactored, adding new tests or modifying existing ones could be a tough challenge.

One of the private projects that uses our library has a lot of polymorphic components and every concrete implementation has a totally different logic.
There were two main test assemblies - an old one (let's call it `Standard` for brevity) where standard patterns for testing were applied and a new one (let's call it `Clean` for brevity) integrating `TryAtSoftware.CleanTests`.
We could easily compare the two approaches as we were working on them independently.

In the past, there were multiple test assemblies with more than **1500** tests that were executing for over 10 minutes.
However, `TryAtSoftware.CleanTests` was integrated in a different assembly and we could easily compare the two approaches.
Moreover, if the more components and logical branches there are in the code, the less scenarios are covered.

After finalizing the integration, we could notice the following:

| Criteria                        | Standard test assembly | Clean test assembly |
|---------------------------------|------------------------|---------------------|
| Number of written test          | \> 1500                | \< 100              |
| Number of test cases            | \< 1700                | \> 15000            |
| Execution time _(approximated)_ | 15 minutes             | 20-25 minutes       |
| Code coverage _(approximated)_  | \< 20%                 | \> 80%              |
| Number of found bugs            | 0                      | \> 20               |

As you can see, this is quite big of a difference!
With much less effort we managed to achieve unthinkable results.
If we had to stick to the `Standard` testing approach in order to increase the code coverage and amount of test cases and optimize the performance, we had to write a lot of code.
And if we had to do the same for every new functionality that is coming, that would slow down the software development process significantly.
`TryAtSoftware.CleanTests` gave us an alternative approach of automatic testing that not only improved the quality of the product but also saved us a lot of time that we could invest in adding more and more features.

The main goals that have been accomplished are:

- Automatic generation of test cases using all proper variations of registered `clean utilities`
- Every `clean utility` can define external demands that represent conditions about what other `clean utilities` should be present within a variation in order to generate a test case with it
- Every `clean utility` can depend internally on other `clean utilities`
- Every `clean utility` can define internal demands that represent conditions about what `clean utilities` should be injected upon initialization
- Global and local `clean utilities` - local `clean utilities` are instantiated for every test case; global `clean utilities` are instantiated only once and can be used to share common context between similar test cases
- Parallel execution of tests cases

# About us

`Try At Software` is a software development company based in Bulgaria. We are mainly using `dotnet` technologies (`C#`, `ASP.NET Core`, `Entity Framework Core`, etc.) and our main idea is to provide a set of tools that can simplify the majority of work a developer does on a daily basis.

# Getting started

## Installing the package

Before creating any equalization profiles, you need to install the package.
The simplest way to do this is to either use the `NuGet package manager`, or the `dotnet CLI`.

Using the `NuGet package manager` console within Visual Studio, you can install the package using the following command:

> Install-Package TryAtSoftware.CleanTests

Or using the `dotnet CLI` from a terminal window:

> dotnet add package TryAtSoftware.CleanTests

## What are the `clean utilities`?

The `clean utility` is a key component for our library. Every `clean utility` has a `category` and a `name` that are required.
One test may require many `clean utilities` and whenever there are two or more utilities from the same category that can be used for its execution, then a test case will be generated for each possible variation of utilities.

Every `clean utility` can me marked as `local` or `global`.
`Local clean utilities` will be instantiated at least once for every test case requiring their participation.
`Global clean utilities` will be instantiated only once for all test cases sharing a common context. 

Moreover, every `clean utility` can optionally define its own characteristics.
These characteristics can be used to filter out on some basis the utilities that we want to use when generating the cases for a given test.
They do often correspond to essential segments of the requested component's behavior.
We use `demands` to make sure that the capabilities our test needs are present for the resolved utilities used to execute the test.

In order to use a type as a `clean utility`, it should be marked with the `CleanUtility` attribute that accepts `category`, `name` and `characteristics`. You can also explicitly set a value to the `IsGlobal` flag.

Example:
```C#
[CleanUtility(Categories.Writers, "Console writer", Characteristics.UsesConsole, Characteristics.ActiveWriter)]
public class ConsoleWriter : IWriter
{
    public void Write(string text) => Console.WriteLine(text);
}

[CleanUtility(Categories.Writers, "File writer", Characteristics.UsesFile, Characteristics.ActiveWriter)]
public class FileWriter : IWriter
{
    public void Write(string text) => File.WriteAllText("C:/path_to_document", text);
}

[CleanUtility(Categories.Writers, "Fake writer")]
public class FakeWriter : IWriter
{
    public void Write(string text) { /* Do nothing */ }
}
```

## How to use clean tests?

This library is built atop [XUnit](https://xunit.net/) so if you are familiar with the way this framework operates, you are most likely ready to use `clean tests`.
There are only two requirements for this:
- The test should be marked with either `CleanFact` (instead of `Fact`) or `CleanTheory` (instead of `Theory`).
> You can still use tests that are marked with other attributes, however, they will be executed as standard tests and will have none of the behavior clean tests can benefit from.

- The type containing the requested test should implement the `ICleanTest` interface. We suggest reusing the abstract `CleanTest` that we have exposed as it will make accessing instances of the registered `clean utilites` easier and you will not have to think about various internal processes that should be handled.

Clean tests can define `requirements` representing the set of `categories` for which `clean utilities` should be provided.
The `WithRequirements` attribute can be used in order to achieve that.

Clean tests can also define `demands` to filter out only a specific subset of the `clean utilities` that can be used for the generation of test cases.
The `TestDemands` attribute can be used in order to achieve that - for each `category` a set of demanded `characteristics` can be used.

Example:
```C#
[CleanFact]
[WithRequirements(Categories.Writers)]
[TestDemands(Categories.Writers, Characteristics.ActiveWriter)]
public void WriteShouldSucceed()
{
    IWriter writer = this.GetService<IWriter>();
    writer.Write("Some text");
}
```