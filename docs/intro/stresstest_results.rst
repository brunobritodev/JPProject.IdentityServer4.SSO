Stress Tests Results
====================

When running in production scenarios. Some endpoints has more requests than other. There are two that has a heavy workload in stressed scenarios.

* `connect/token`
* `connect/introspect`

Why?

* Usually developers don't carry about generate a new token at each request to some API. 
* There a huge amount of stuff that donn't use Token introspect endpoint, and when it is used, don't care about send it to server to validate at each request.

In fact the most used component for token introspection for ASP.NET users is `AccessTokenValidation`. It has a default cache of 5 minutes. But when are using another component for node.js, Springboot (java). it doesn't has cache capabilities.
So the stress test was base in this case.

Generate token
^^^^^^^^^^^^^^

.. image:: ../images/stress/connecttoken.png


Token introspect
^^^^^^^^^^^^^^^^

.. image:: ../images/stress/tokenintrospect.png