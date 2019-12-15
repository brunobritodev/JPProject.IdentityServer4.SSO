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
  - [Demo](#demo)
  - [We are online](#we-are-online)
  - [Wanna go production?](#wanna-go-production)
- [Technologies](#technologies)
  - [Architecture](#architecture)
  - [Give a Star! ⭐](#give-a-star-%e2%ad%90)
  - [Development Scenario](#development-scenario)
- [Docs](#docs)
  - [Contributing](#contributing)
  - [Free](#free)
  - [3.0.1](#301)
  - [v1.4.5](#v145)
- [What comes next?](#what-comes-next)
- [License](#license)

# Presentation

The main goal of JP Project is to be a Management Ecosystem for IdentityServer4 and ASP.NET Identity. 

Helping Startup's and Organization to Speed Up Microservices Environment. Providing tools for an OAuth2 Server and User Management. 

Built with IdentityServer4. An OpenID Connect and OAuth 2.0 framework for ASP.NET Core.

SSO has some flows:
* Single Sign On
* Register users
* Recover password flow
* MFA
* Federation Gateway (Login by Google, Facebook.. etc)
* Argon2 password hashing
* CSP Headers
* Event monitoring (For compliance scenarios)

Admin UI is an administrative panel where it's possible to manage both OAuth2 Server and Identities. 

From OAuth2 panel it's possible to manage:
* `Clients`
* `Identity Resources`
* `Api Resources`
* `Persisted Grants`

From Identity panel it's possible to manage `Users` and `Roles`

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

* You need everything (Best choice)? JP Project provide a complete SSO with an Administration panel. Check it at [SSO - Full Version](https://github.com/brunohbrito/JPProject.IdentityServer4.SSO)

* You already have an IdentityServer4 Up and running? Go to [Admin Panel - Light version](https://github.com/brunohbrito/JPProject.IdentityServer4.AdminUI)

## Demo 

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

Written in ASP.NET Core and Angular 8.

- Angular 8
- Rich UI interface
- ASP.NET Core 3.0
- ASP.NET MVC Core 
- ASP.NET WebApi Core
- ASP.NET Identity Core
- Argon2 Password Hashing
- MySql Ready
- Sql Ready
- Postgree Ready
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

- Architecture with responsibility separation concerns, SOLID and Clean Code
- Domain Driven Design (Layers and Domain Model Pattern)
- Domain Events
- Domain Notification
- CQRS (Imediate Consistency)
- Event Sourcing
- Unit of Work
- Repository and Generic Repository

## Give a Star! ⭐

Do you love it? give us a Star!

## Development Scenario

Jp Project is built against ASP.NET Core 3.0.

* [Install](https://www.microsoft.com/net/download/core#/current) the latest .NET Core 3.0 SDK

`src/JPProject.SSO.sln` Contains SSO and API

For UI's use VSCode.
- User Management -> Inside VSCode open folder `rootFolder/src/Frontend/Jp.UserManagement`, then terminal and `npm install && npm start`

Wait for ng to complete his proccess then go to http://localhost:5000!

Any doubts? Go to docs

# Docs #

Wanna start? please [Read the docs](https://jp-project.readthedocs.io/en/latest/index.html)

## Contributing

We'll love it! Please [Read the docs](https://jp-project.readthedocs.io/en/latest/index.html)

## Free ##

If you need help building or running your Jp Project platform
There are several ways we can help you out.

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

* Code coverage
* UI for Device codes 
* CI with SonarCloud
* E-mail template management
* Blob service management


# License

Jp Project is Open Source software and is released under the MIT license. This license allow the use of Jp Project in free and commercial applications and libraries without restrictions.
