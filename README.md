![image](https://github.com/brunohbrito/JPProject.Core/blob/master/build/logo.png?raw=true)

[![Build Status](https://dev.azure.com/brunohbrito/Jp%20Project/_apis/build/status/JPProject%20-%20SSO%20-%20CD?branchName=master)](https://dev.azure.com/brunohbrito/Jp%20Project/_build/latest?definitionId=10&branchName=master)
[![License](https://img.shields.io/github/license/brunohbrito/JPProject.IdentityServer4.SSO)](LICENSE)
[![Gitter](https://badges.gitter.im/JPProject-IdentityServer4/community.svg)](https://gitter.im/JPProject-IdentityServer4/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

This is the full version of JP Project. It provide SSO and an Api where it's possible to manage both IdentityServer4 and ASP.NET Identity.

# Installation

Windows users:
* download [jpproject-docker.zip](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/releases/download/3.0.0/jpproject-docker.zip)
* Unzip and execute `docker-run.bat` (As administrator)

Linux users:
* Download [jpproject-docker.zip](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/releases/download/3.0.0/jpproject-docker.zip)
* Add `127.0.0.1 jpproject-sso` entry to hosts file (`/etc/hosts`)
* unzip and execute `docker-compose up`

## Table of Contents ##

- [Installation](#installation)
  - [Table of Contents](#table-of-contents)
- [Presentation](#presentation)
  - [Admin UI](#admin-ui)
  - [Login page](#login-page)
  - [Consent page](#consent-page)
  - [Profile](#profile)
- [How to start?](#how-to-start)
  - [Already have an ASP.NET Identity?](#already-have-an-aspnet-identity)
- [Demo](#demo)
  - [We are online](#we-are-online)
  - [Wanna go production?](#wanna-go-production)
- [Technologies](#technologies)
  - [Architecture](#architecture)
  - [Key Material](#key-material)
  - [Data protection Keys (ASP.NET Core)](#data-protection-keys-aspnet-core)
- [Give a Star! ⭐](#give-a-star-%e2%ad%90)
  - [Development Scenario](#development-scenario)
- [Docs](#docs)
  - [Contributors](#contributors)
  - [Contributing](#contributing)
  - [Free](#free)
- [3.2.0](#320)
  - [3.0.1](#301)
  - [v1.4.5](#v145)
- [What comes next?](#what-comes-next)
- [License](#license)

# Presentation

The main goal of JP Project is to be a Management Ecosystem for IdentityServer4 and ASP.NET Identity. Helping Startup's and companies to Speed Up Microservices Environment. Providing tools for an OAuth 2.0 Server and User Management. It's highly modular and easy to change for .NET teams.

Built with IdentityServer4. An OpenID Connect and OAuth 2.0 framework for ASP.NET Core.

SSO Features:
* Single Sign On
* Register users
* Recover password flow
* MFA
* Federation Gateway (Login by Google, Facebook.. etc)
* Argon2 password hashing
* CSP Headers
* Event monitoring (For compliance scenarios)
* Key Material Management
* ASP.NET Core Dataprotection keys management

Admin UI is an administrative panel where it's possible to manage both OAuth2 Server and Identities. 

From OAuth 2.0 panel it's possible to manage:
* `Clients`
* `Identity Resources`
* `Api Resources`
* `Persisted Grants`

For Identity panel it's possible to manage 
* `Users` 
* `Roles` 
* Events
* Server Settings:
  * Create custom e-mail for Confirm Account and Forgot Password. It's also possible to configure E-mail settings and a blob Storage to store Users pictures (Azure Blob, AWS S3 and Filesystem).


It's open source and free. From community to community.

Screenshots

## Admin UI ##
<img src="https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/docs/images/jp-adminui.gif"  width="480" />

## Login page ##
<img src="https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/docs/images/login.JPG?raw=true" width="480" />

## Consent page ##
<img src="https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/docs/images/consent-page.JPG?raw=true" width="480" />

## Profile ##
<img src="https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/docs/images/jp-usermanagement.gif" width="480" />


# How to start?

First you need to choose.

* You need everything (Best choice)? JP Project provide a complete SSO with an Administration panel. Check it at [SSO - Full Version](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO). This version has some additional Tables:
  * Template - store e-mail template
  * Email - An instance of template with e-mail settings
  * GlobalSettings - It store settings like E-mail credentials, S3 / Azure Blob settings. Logo / Version settings
  * StoredEvent and EventDetails - Store everything that is happening in your SSO.
  * DataProtectionKeys and SecurityKeys - Special tables to store Key Material (JWK) and ASP.NET Dataprotection Keys

* You already have an IdentityServer4 Up and running and don't wanna any changes to your current model. Only the admin panel? Go to [Admin Panel - Light version](https://github.com/brunohbrito/JPProject.IdentityServer4.AdminUI)


## Already have an ASP.NET Identity? 

These options above requires almost no code. If you already have an ASP.NET IdentitySystem it's possible to connect SSO to use your users, but requires some modifications:

* Check Argon2 implementation at `Startup.cs`
* You UserIdentity must implement `IDomainUser` (No additional fields will be added to your ASP.NET Identity)
  * All SSO fields are users claims
* If your Users have custom fields, you can implement `IIdentityFactory<TUser>` and `IRoleFactory<TUser>`. These classes will help you in Register / Update user flow. It give you hability to intercept the request before add / update user or role to database.

# Demo 

Check our demo online.

## We are online

<img align="right" width="100px" src="https://www.developpez.net/forums/attachments/p289604d1/a/a/a" />

Check it now at [Admin Panel](https://admin.jpproject.net).

You can check also [SSO](https://sso.jpproject.net) and [Profile Manager](https://user.jpproject.net)

_New users are readonly_

## Wanna go production?

Check [docs](https://jp-project.readthedocs.io/en/latest/) to see how to and some examples: 
* Azure App Service 
* Docker Swarm + Nginx in Linux.
* Docker compose + nginx in linux
* Make a PR and show how you have done your environment!

# Technologies #

Check below how it was developed.

Written in ASP.NET Core 3.1 and Angular 8.

- Angular 8
- Rich UI interface
- ASP.NET Core 3.0
- ASP.NET MVC Core 
- ASP.NET WebApi Core
- ASP.NET Identity Core
- Argon2 Password Hashing
- MySql Ready
- Sql Ready
- Postgres Ready
- SQLite Ready
- Entity Framework Core
- .NET Core Native DI
- AutoMapper
- FluentValidator
- MediatR
- Swagger UI
- High customizable
- Translation for 7 different languages


## Architecture

It respect the IdentityServer4 base classes and was built in the same way, for better compatibility and minimize impacts for future versions.

![Dependencies](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/docs/images/DependenciesGraph.png?raw=true)

- Architecture with responsibility separation concerns, SOLID and Clean Code
- Hexagonal architecture (Layers and Domain Model Pattern)
- Domain Events
- Domain Notification
- CQRS (Imediate Consistency)
- Event Sourcing
- Unit of Work
- Repository and Generic Repository

## Key Material

The Cryptography Keys (JWKS) are stored within Database and auto refresh it every 90 days. It uses ECDSA using P-256 and SHA-256 (ES256) by default.

## Data protection Keys (ASP.NET Core)

The dataprotection keys are stored with database, like Key Material. 

# Give a Star! ⭐

Do you love it? give us a Star!

## Development Scenario

Jp Project is built against ASP.NET Core 3.1.

* [Install](https://www.microsoft.com/net/download/core#/current) the latest .NET Core 3.10 SDK

`src/JPProject.SSO.sln` Contains SSO and API

For UI's use VSCode.
- User Management -> Inside VSCode open folder `rootFolder/src/Frontend/Jp.UserManagement`, then terminal and `npm install && npm start`

Wait for ng to complete his proccess then go to http://localhost:5000!

Any doubts? Go to docs

# Docs #

Wanna start? please [Read the docs](https://jp-project.readthedocs.io/en/latest/index.html)

## Contributors

Thank you all!

[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/0)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/0)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/1)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/1)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/2)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/2)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/3)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/3)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/4)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/4)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/5)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/5)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/6)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/6)[![](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/images/7)](https://sourcerer.io/fame/brunohbrito/brunohbrito/JPProject.IdentityServer4.SSO/links/7)

## Contributing

We'll love it! Please [Read the docs](https://jp-project.readthedocs.io/en/latest/index.html)

## Free ##

If you need help building or running your Jp Project platform
There are several ways we can help you out.

# 3.2.0

1. ASP.NET Identity - Now you can plug your running Identity to use SSO. It need to made some changes at you IdentityUser with more data, like Name, Url, Bio.
2. Changes in Events - Now all events are attached at his Aggregate Roots. Now events are very strong source of analisys.
3. Event search at Admin Panel
4. OAuth 2.0 Best practices
   1. Jwa with Elliptic Curves
   2. Jwk using ECDSA using P-256 and SHA-256 (ES256) by default
   3. Changed how clients are created by default. Using Authorization Code with PKCE or Client Credentials only.
5. Key Material management - Key material now available at Database. Now it's possible to Scale Horizontal without any "Unprocted ticket failed" error

## 3.0.1

1. ASP.NET Core 3.0 support
2. Separated repositories, for better management. Improving tests, integration tests. And to support more scenarios.

## v1.4.5

Breaking change: **Argon2 password hashing**. Be careful before update. If you are using the old version all users must need to update their passwords.

1. Bug fixes:
   1. Tooltip for admin-ui
2. Argon2 Password Hasher
3. Show version at footer

Check [Changelog.md](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO/blob/master/CHANGELOG.md) for a complete list of changes.

# What comes next?

* An easy way to insert Client, IdentityResources and Api Resources by JSON - Aiming teams who needs to take data from Staging to past it to production
* Key Material Management from Admin UI
* Code coverage
* UI for Device codes 
* CI with SonarCloud


# License

Jp Project is Open Source software and is released under the MIT license. This license allow the use of Jp Project in free and commercial applications and libraries without restrictions.
