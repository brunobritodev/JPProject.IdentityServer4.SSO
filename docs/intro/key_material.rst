Key Material
=============

The Cryptography Keys (JWKS) are stored within Database and auto refresh it every 90 days. It uses ECDSA using P-256 and SHA-256 (ES256) by default.

Signing key rollover
^^^^^^^^^^^^^^^^^^^^
While you can only use one signing key at a time, IdentityServer4 provide mechanisms to publish more than one validation key to the discovery document. This is useful for key rollover.
So every 90 days a new Key is auto published following NIST `SP 800-107 Rev. 1 <https://csrc.nist.gov/publications/detail/sp/800-107/rev-1/final>`_ best practices.

Algorithm
^^^^^^^^^

It uses Elliptic Curves through ECDSA using P-256 and SHA-256 as default, following `RFC 7518 <https://tools.ietf.org/html/rfc7518#section-3.1>`_ best practices.

Database Store
^^^^^^^^^^^^^^

To manage keys, SSO use Jwks.Manager component. It provide many Algorithm, you can change it at `Startup.cs`