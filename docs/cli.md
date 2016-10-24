# Synapse CommandLine

Synapse.cli provides local process execution and is the semantic and functional equivalent of Synapse.Server.  Output from Synapse.cli is redirected to the prompt by default, instead of to a log.

## CommandLine Help:

```css
synapse.cli.exe, Version: 1.0.0.0

Syntax:
  synapse.cli.exe /plan:{filePath}|{encodedPlanString} [/dryRun:true|false]
    [/taskModel:inProc|external] [/render:encode|decode] [dynamic parameters]

  /plan        - filePath: Valid path to plan file.
               - encodedPlanString: Inline base64 encoded plan string.
  /dryRun      Specifies whether to execute the plan as a DryRun only.
                 Default is false.
  /taskModel   Specifies whether to execute the plan on an internal
                 thread or shell process.  Default is InProc.
  /render      - encode: Returns the base64 encoded value of the
                 specifed plan file.
               - decode: Returns the base64 decoded value of the specified
                 encodedPlanString.
  dynamic      Any remaining /arg:value pairs will passed to the plan
                 as dynamic parms.
```