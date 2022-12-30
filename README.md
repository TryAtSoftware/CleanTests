[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_Equalizer)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=TryAtSoftware_CleanTests&metric=bugs)](https://sonarcloud.io/summary/new_code?id=TryAtSoftware_CleanTests)

[![Core validation](https://github.com/TryAtSoftware/CleanTests/actions/workflows/Core%20validation.yml/badge.svg)](https://github.com/TryAtSoftware/CleanTests/actions/workflows/Core%20validation.yml)

# About the project

`TryAtSoftware.CleanTests` is a library that should simplify the process of automated testing for complex setups. The main goals that have been accomplished are:
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