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