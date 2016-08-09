#Synapse Plans

## Plans

A Synapse Plan is a declarative workflow that is based on execution-result branching.  Plans are comprised of a series of hierarchical Actions, where each Action optionally specifies an Execution Case.

### Fields

|Name|Description
|-|-
|Name|The friendly name of the Plan, used when reporting status.
|UniqueName|A globally unique name for the Plan to support database fetches.
|Description|A friendly Plan description.
|IsActive|The boolean enabled/disabled state of the Plan.
|Actions|The list of child Actions.
|RunAs|The overriding, Plan-level SecurityContext.

### Example YAML

```css
Name: myPlan
UniqueName: myPlan012
Description: Runs important actions on nodes.
IsActive: true
Actions:
  {Actions}
RunAs:
  {SecurityContext}
```

---

## Actions

An Action is a workflow process, which can essentially be anything.  Synapse is extended by creating/invoking new, custom processes as required via runtime modules.

### Fields

|Name|Description
|-|-
|Name|The friendly name of the Action, used when reporting status.
|Proxy|The URI of a remote Synapse daemon, used under distributed execution models.
|ExecuteCase|A list of StatusType values to match the ExecuteResult of a parent Action.
|Handler|Declares the library to support executing the Action.
|Parameters|Delares the ParameterInfo block used when invoking the Action.
|ExecuteResult|Holds the post-execution result of the Action.  Rolls-up child execution results to the highest StatusType.
|Actions|The list of child Actions.
|RunAs|The Action-level SecurityContext, overrides Plan-level declaration.

### Example YAML

```css
Name: Start Service
Proxy: http://foo
ExecuteCase: Success
Handler:
  Type: myLibrary.Utilities:myLibrary.Utilities.ServiceController
  Config: {ParameterInfo}
Parameters: {ParameterInfo}
RunAs: {SecurityContext}
```

---

## ParameterInfo

ParameterInfo blocks declare start-up configuration for Handlers modules, runtime invocation data for Handler methods, and start-up configuration for SecurityContext modules. ParameterInfo blocks can be inherited throughout Plans, and individual ParameterInfo Value settings can be overridden locally.

|Name|Description
|-|-
|Name|The friendly name of the ParameterInfo, used when inheriting the values.
|Type|SerializationType: Xml, Yaml, Json
|InheritFrom|The name of another ParameterInfo block.
|Uri|A file or http Uri from which to fetch values.
|Values|Locally declared values.
|Dynamic|List of Name/Path pairs used in value substitution.  Paths are declared in XPath for Xml serialization and colon-separated lists (root:node0:node1:...) for Yaml/Json.

### Example YAML - YAML Values

```css
Name: myYamlParms
Type: Yaml
Uri: http://foo
Values:
  Magical: Mystery1
  Lucy: In the sky1
  Kitten:
    Cat: Liger
    Color: Green
Dynamic:
- Name: app
  Path: Magical
- Name: type
  Path: Kitten:Color
```

### Example YAML - XML Values

```css
Type: Xml
Values: <xml attr="value1"><data>foo1</data></xml>
Dynamic:
- Name: app
  Path: /xml[1]/data[1]
- Name: type
  Path: /xml[1]/@attr
```

### Example YAML - JSON Values

```css
Type: Json
Uri: http://foo
Values:
  {
    "ApplicationName": "fooApp",
    "EnvironmentName": "dev1",
    "Tier": {
        "Name": "webserver1",
        "Type": "python1",
        "Version": "1.0"
    },
  }
Dynamic:
- Name: app
    Path: ApplicationName
    Options:
    - Key: 1
      Value: TheOne
    - Key: 2
      Value: Shoe
- Name: type
  Path: Tier:Type
```