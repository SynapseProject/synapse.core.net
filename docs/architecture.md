#Architecture and Usage Patterns

## Synapse Engine

<p align="center">
<img alt="Synapse Engine" src="../images/syn_engine.png" />
</p>

The Synapse Engine processes workflow actions by tasking to a Handler.  Each Handler is responsible only for its specific technology capability, whether that be local, or remote.  Handlers must also report status back to the calling engine.

### Engines as a Proxy

In order to simplify workflow processing, when considered as an end-to-end logical unit, a Synapse Engine can proxy Actions to other Synapse Engine instances.  This is useful also when crossing network/firewall bourndaries to minimize exposing wide port ranges.

### Local Engine Processing Model

When invoked as a local process, such as through Synapse.CommandLine, a Synapse Engine is processing work as an island, but still maintains the capability to proxy Actions to remote Synapse Engines.  The primary difference is the request/response model does not participate in the Enterprise queueing mechanism.

## Synapse Enterprise

<p align="center">
<img alt="Synapse Enterprise" src="../images/syn_enterprise.png" />
</p>

Synapse Enterpise is implemented to provide scalability in high-volume environments.  The Enterprise service publishes inbound processing requests, which are then dequeued by available Synapse Engines instances.  Engines, in turn, publish status back to a queue, which the Enterprise service then dequeues and logs.