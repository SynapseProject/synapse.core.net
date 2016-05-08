#Architecture and Usage Patterns

## Synapse Engine

<p align="center">
<img alt="Synapse Engine" src="../images/syn_engine.png" />
</p>

The Synapse Engine processes workflow actions by tasking to a Handler.  Each handler is resposnsible only for its specific technology capability, whether that be local, or remote.  Handlers must also report status back to the calling engine.

### Engines as a Proxy

In order to simplify workflow processing, when considered as an end-to-end logical unit, a Synapse Engine can proxy actions to other Synapse engine instances.  This is useful also when crossing network/firewall bourndaries to minimize exposing wide port ranges.

## Synapse Enterprise

<p align="center">
<img alt="Synapse Enterprise" src="../images/syn_enterprise.png" />
</p>

Synapse Enterpise is implemented to provide scalability in high-volume environments.  The Enterprise service publishes inbound processing requests, which are then dequeued by available Synapse engines instances.  Engines, in turn, publish status back to a queue, which the Enterprise service then dequeues and logs.