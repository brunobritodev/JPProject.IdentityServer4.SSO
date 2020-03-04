Data Protection Keys
=====================

SSO has a default implementation for ASP.NET Core Data Protection Keys at database level. Using dataprotection at database level we leverage configuration to run through a Web Farm:

* **Reliability/availability** – When one or more nodes fail, the load balancer can route requests to other functioning nodes to continue processing requests.
* **Capacity/performance** – Multiple nodes can process more requests than a single server. The load balancer balances the workload by distributing requests to the nodes.
* **Scalability** – When more or less capacity is required, the number of active nodes can be increased or decreased to match the workload. Web farm platform technologies, such as Azure App Service, can automatically add or remove nodes at the request of the system administrator or automatically without human intervention.
* **Maintainability** – Nodes of a web farm can rely on a set of shared services, which results in easier system management. For example, the nodes of a web farm can rely upon a single database server and a common network location for static resources, such as images and downloadable files.

It's ready for load balance scenarios and to run under multiple containers as well.