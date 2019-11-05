.. include:: special.rst

Getting Started
===============

.. image:: images/logo.png
    :align: center

The main goal of JP Project is to be a Management Ecosystem for IdentityServer4 and ASP.NET Identity. 

Helping Startup's and Organization to Speed Up Microservices Environment. Providing tools for an OAuth2 Server and User Management. 

Built with IdentityServer4. An OpenID Connect and OAuth 2.0 framework for ASP.NET Core.

Features of SSO:

* Register users
* Recover password flow
* MFA
* Federation Gateway (Login by Google, Facebook.. etc)
* Argon2 password hashing
* CSP Headers
* Event monitoring (For compliance scenarios)

Admin UI is an administrative panel where it's possible to manage the OAuth2 Server, Users and Roles. 

From OAuth2 panel it's possible to manage:
* `Clients`
* `Identity Resources`
* `Api Resources`
* `Persisted Grants`

From Identity panel it's possible to manage `Users` and `Roles`

It's open source and free. From community to community.

Screenshots

Admin UI
^^^^^^^^
<img src="https://github.com/brunohbrito/JP-Project/blob/master/docs/images/jp-adminui.gif"  width="480" />

Login page
^^^^^^^^^^
<img src="https://github.com/brunohbrito/JP-Project/blob/master/docs/images/login.JPG?raw=true" width="480" />

Consent page
^^^^^^^^^^^^
<img src="https://github.com/brunohbrito/JP-Project/blob/master/docs/images/consent-page.JPG?raw=true" width="480" />

Profile
^^^^^^^
<img src="https://github.com/brunohbrito/JP-Project/blob/master/docs/images/jp-usermanagement.gif" width="480" />



Demo online
===========

Now we are online! `See it in action <https://jpproject.azurewebsites.net/admin-ui>`_

Below an intro video!

.. raw:: html

    <div style="position: relative; height: 0; overflow: hidden; max-width: 100%; height: auto;">
        <iframe src="https://player.vimeo.com/video/288481888?color=ff9933&title=0&byline=0" width="800" height="350" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>
    </div>

Give a Star ⭐!
----------------

Do you love it? give us a :yellow:`Star!` ⭐

.. raw:: html 
    
    <iframe src="https://ghbtns.com/github-btn.html?user=brunohbrito&repo=JP-Project&type=star&size=large" frameborder="0" scrolling="0" width="160px" height="30px"></iframe>


Contributing
------------
Wanna contribute? Feel free to do that!

But remember to check the `Contributing Section <intro/contributing.html>`_

Take a special place in our heart!

All contributors has a heart💓! See them in `Contributor list <https://github.com/brunohbrito/JP-Project/blob/master/CONTRIBUTORS.md>`_

Free
^^^^
If you need help building or running your Jp Project platform
There are several ways we can help you out.


.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Introduction

   intro/big_picture
   intro/architecture
   intro/contributing
   intro/code_of_conduct
   intro/contributors.md
   intro/stresstest_results

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Get Start

   quickstarts/build
   quickstarts/docker_support
   quickstarts/vs_vscode
   quickstarts/ambient_variables
   quickstarts/app_settings

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Configuration

   configuration/databaseType
   configuration/serilog
   configuration/application_insights
   configuration/certificate