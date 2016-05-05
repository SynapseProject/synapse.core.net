# Welcome to Synapse

Synapse is a lightweight execution engine designed to take data from disparate sources and connect it to a process.  Synapse is designed to run as a local or remote process.

## Elements

A Synapse workflow, called a Plan, is comprised of a hierarhy of Actions.  An Action is essentially the definition of a local or remote process and the parameters required to initiate it.  Plans are declared in YAML, as follows:

```
Action:
- Name: {friendly name}
  Handler:
    Type: {library}:{module}
    Config:
      Name: {string}
      Type: {Xml | Yaml | Json}
      Uri: {http:// | file://}
      Values:
        {Block of directly declared Xml, Yaml, Json as specified by Type}
  Parameters:
    Name: {string}
    Type: {Xml | Yaml | Json}
    Uri: {http:// | file://}
    Values:
      {Block of directly declared Xml, Yaml, Json as specified by Type}
    Dynamic:
    - Name: {friendly name}
      Path: {XPath or root:path0:path1}
      Options:
      - Key: {key}
        Value: {display value}
```

## Components

| Component | Description
|--------|--------
|Synapse.Core|Contains the workflow execution engine and is responsible for initiating calls to sub-processes.
|Synapse.CommandLine|A CLI wrapper on Synapse.Core for local process execution.
|Synapse.Server|A server daemon designed to act as remote Synapse.Core agent.
|Synapse.Enterprise|An API interface to creating, storing, and executing Synapse Plans under an RBAC.  Manages exection log-capture and keeps detailed audit logs.

## Commands

* `mkdocs new [dir-name]` - Create a new project.
* `mkdocs serve` - Start the live-reloading docs server.
* `mkdocs build` - Build the documentation site.
* `mkdocs help` - Print this help message.

## Project layout

    mkdocs.yml    # The configuration file.
    docs/
        index.md  # The documentation homepage.
        ...       # Other markdown pages, images and other files.
