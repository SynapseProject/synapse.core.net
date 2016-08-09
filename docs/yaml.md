#Example YAML

```css
Name: plan0
Description: planDesc
IsActive: true
Actions:
- Name: ac0
  Handler:
    Type: Synapse.Core:Synapse.Core.Runtime.FooHandler
    Config:
      Type: Yaml
      Uri: http://foo
      Values:
        Magical: Mystery1
        Lucy: In the sky1
        Kitten:
          Cat: Liger
          Color: Green
  Parameters:
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
  Actions:
  - Name: ac0.1
    ExecuteCase: Failed
    Handler:
      Type: Synapse.Core:Synapse.Core.Runtime.EmptyHandler
    Actions:
    - Name: ac0.1.1
      Handler:
        Type: Synapse.Core:Synapse.Core.Runtime.FooHandler
        Config:
          Type: Xml
          Uri: http://foo
      Parameters:
        Type: Xml
        Uri: file://C:/Devo/git/Synapse/Synapse.Tester/yaml/parms.xml
        Values: <xml attr="value1"><data>foo1</data></xml>
        Dynamic:
        - Name: app
          Path: /xml[1]/data[1]
        - Name: type
          Path: /xml[1]/@attr
    - Name: ac0.1.2
      Handler:
        Type: Synapse.Core:Synapse.Core.Runtime.BarHandler
        Config:
          Type: Xml
          Uri: http://foo
      Parameters:
        Type: Xml
        Uri: http://foo
        Values: <xml attr="value1"><data>foo1</data></xml>
        Dynamic:
        - Name: app
          Path: /xml[1]/data[1]
        - Name: type
          Path: /xml[1]/@attr
    - Name: ac0.1.3
      Handler:
        Type: Synapse.Core:Synapse.Core.Runtime.BarHandler
        Config:
          Type: Xml
          Uri: http://foo
      Parameters:
        Type: Yaml
        InheritFrom: myYamlParms
- Name: ac1
  ExecuteCase: Complete
  Handler:
    Type: Synapse.Core:Synapse.Core.Runtime.FooHandler
    Config:
      Type: Xml
      Uri: http://foo
  Parameters:
    Values: foo
- Name: ac2
  Handler:
    Type: Synapse.Core:Synapse.Core.Runtime.BarHandler
    Config:
      Type: Xml
      Uri: http://foo
  Parameters:
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