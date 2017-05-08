# JSON Document Transforms

The JDT language aims to provide simple, intuitive transformations for JSON files while keeping the transformation file as close to the original file as possible. JDT also provides more complex behavior through specific transformations with special syntax outlining the desired result.

### Summary

JSON document transformations seek to change a single JSON file (source) based on transformations specified in another JSON file (transform), generating a new JSON (result). The default behavior of JDT is to merge the transformation file into the source file. More advanced behavior can be specified by the user through the defined JDT syntax.

## Default Transformation

If no behavior is specified in any JSON object, the default transformation is to merge the two files. The goal of JDT is to have a default behavior that satisfies most user scenarios. For more specifics on merging, see the Merge transformation.

#### Example

Source:
``` javascript
{
    "Version": 1,
    "Settings": {
        "Setting01" : "Default01",
        "Setting02" : "Default02"
    },
    "SupportedVersions" : [1, 2, 3]
}
```

Transform:
``` javascript
{
    "Version": 2,
    "Settings": {
        "Setting01" : "NewValue01",
        "Setting03" : "NewValue03"
    },
    "SupportedVersions" : [4, 5],
    "UseThis" : true
}
```

Result:
``` javascript
{
    // Overriden by the transformation file
    "Version": 2,
    "Settings": {
        // Overriden by the transformation file
        "Setting01" : "NewValue01",
        // Not present in the transformation file, unchanged
        "Setting02" : "Default02",
        // Added by the transformation file
        "Setting03" : "NewValue03"
    },
    // The array in the transformation file was appended
    "SupportedVersions" : [1, 2, 3, 4, 5],
    // Added by the transformation file
    "UseThis" : true
}
```

## Attributes

Attributes are advanced methods to specify behaviors that cannot be achieved through the default transformation. Any transformation can be an object containing valid attributes for that transformation. Attributes outside of transformations are not allowed and generate errors.

### Path

Use JSONPath syntax to navigate to the node where the transform should be applied.


| Use: | `"@jdt.path" : <Value>` (Case sensitive) |
| ---- |:----------------------------------------:|

Value must be a string with a valid JSONPath. Any other type generates an error. If the path does not match any nodes, the transformation is not performed.

All JDT Paths are relative to the node that they're in.

For more information on JSONPath syntax, see [here](http://goessner.net/articles/JsonPath/index.html)

### Value

The transformation value that should be applied.

| Use: | `"@jdt.value" : <Value>` (Case sensitive) |
| ---- |:-----------------------------------------:|

Value depends on the syntax of the transformation verb. See Transformation Verbs.

## Transformation Verbs

Verbs are used within the transformation file to specify specific transformations that should be executed. This helps perform actions that are not covered by the default transformation, such as entirely replacing an existing node. 

A transformation verb always applies to the values of the current node and never the key.

### Rename

| Use: | `"@jdt.rename" : <Value>` (Case sensitive) |
| ---- |:------------------------------------------:|


| Value Type: | Behavior                        |
| ----------- | ------------------------------- |
| Primitive   | Not allowed. Generates an error |
| Object      | If the object contains JDT attributes, apply them. <br> See Attributes. If not, it must only contain key-value pairs where the key is the name of the node that should be renamed and the value is a string with the new name.
| Array       | Applies rename with each element of the array as the transformation value. <br> If the transformation value is an array, generate an error.

**Obs:** Renaming the root node is not allowed and will generate an error.

#### Example

Source:
``` javascript
{
    "A" : {
        "A1" : 11,
        "A2" : {
            "A21" : 121,
            "A22" : 122
        }
    },
    "B" : [
        21,
        22
    ],
    "C" : 3
}
```

Transform:
``` javascript
{
    "@jdt.rename" : {
        "A" : "Astar",
        "B" : "Bstar"
    }
}
```

Result:
``` javascript
{
    // Does not alter result
    "Astar" : {
        "A1" : 11,
        "A2" : {
            "A21" : 121,
            "A22" : 122
        }
    },
    // Does not depend  on object type
    "Bstar" : [
        21,
        22
    ],
    // Does not alter siblings
    "C" : 3
}
```

#### Path Attribute

The `@jdt.path` attribute can be used to specify a node to rename. Absolute or relative paths can be specified to the node. Renaming elements of arrays is not supported and should generate an error.

Source:
``` javascript
{
    "A" : {
        "RenameThis" : true
    },
    "B" : {
        "RenameThis" : false
    },
    "C" : [
        {
            "Name" : "C01",
            "Value" : 1
        },
        {
            "Name" : "C02",
            "Value" : 2
        }
    ]
}
```

Transform:
``` javascript
{
    "@jdt.rename" : {
        "@jdt.Path " : "$[?(@.Rename == true)]",
        "@jdt.Value" : "Astar"
    },
    "C" : {
        "@jdt.rename" : {
            "@jdt.path" : "@[*].Name",
            "@jdt.value" : "Nstar"
        }
    }
}
```

Result:
``` javascript
{
    // Only this node matches the path
    "Astar" : {
        "RenameThis" : true
    },
    "B" : {
        "RenameThis" : false
    },
    // Renaming nodes from an object 
    // in the array is allowed
    "C" : [
        {
            "Nstar" : "C01",
            "Value" : 1
        },
        {
            "Nstar" : "C02",
            "Value" : 2
        }
    ]
}
```
### Remove

| Use: | `"@jdt.remove" : <Value>` (Case sensitive) |
| ---- |:------------------------------------------:|


| Value Type:  | Behavior                                                    |
| ------------ | ----------------------------------------------------------- |
| String       | Removes the node with the given name from the current level |
| Boolean      | If true, remove all the nodes from the current level and sets value to null. If false, do nothing
| Number, null | Not allowed. Generates an error.
| Object       | If the object contains JDT attributes, apply them. See Attributes. <br> If not, generate error.
| Array        | Applies remove with each element of the array as the transformation value. <br> If the transformation value is an array, generate an error.


**Obs:** The `@jdt.value` attribute cannot be used with this transformation

#### Example

Source:
``` javascript
{
    "A" : 1,
    "Astar" : 10,
    "B" : 2,
    "C" : {
        "C1" : 31,
        "C2" : 32
    },
    "D" : {
        "D1" : 41,
        "D2" : 42,
        "D3" : 43
    }
}
```

Transform:
``` javascript
{
    "@jdt.remove" : "Astar",
    "C" : {
        "@jdt.remove" : true
    },
    "D" : {
        "@jdt.remove" : ["D2", "D3"]
    }
}
```

Result:
``` javascript
{
    // Astar is completely removed
    "A" : 1,
    "B" : 2,
    // All nodes are removed
    "C" : null,
    "D" : {
        "D1" : 41
        // Multiple nodes were removed
    }
}
```

#### Path Attribute

The `@jdt.path` attribute can be used to specify the absolute or relative path to the nodes that should be removed. It can also be used to remove elements from arrays. If the Path attribute is present, the Value attribute is not supported and is ignored if present in the transformation.

Source:
``` javascript
{
    "A" : {
        "RemoveThis" : true
    },
    "B" : {
        "RemoveThis" : false
    },
    "C" : {
        "C1" : 1,
        "C2" : {
            "C21" : 21
        }
    }
}
```

Transform:
``` javascript
{
    //Remove only matching nodes from this level
    "@jdt.remove" : {
        "@jdt.path" : "$[?(@.RemoveThis == true)]"
    },
    "C" : {
        //Specify a relative path to the node
        "@jdt.remove" : {
            "@jdt.path" : "@.C2.C21"
        }
    }
}
```

Result:
``` javascript
{
    "B" : {
        "RemoveThis" : false
    },
    "C" : {
        "C1" : 1,
        "C2" : {
        }
    }
}
```

### Merge

| Use: | `"@jdt.merge" : <Value>` (Case sensitive) |
| ---- |:-----------------------------------------:|


| Value Type: | Behavior                                                    |
| ----------- | ----------------------------------------------------------- |
| Primitive   | Replaces the value of the current node with the given value |
| Object      | Recursively merges the object into the current node. Keys that are not present in the source file will be added. <br> If the object contains JDT attributes, apply them. See Attributes.
| Array       | Applies merge with each element of the array as the transformation value. <br> In an explicit merge, if the transformation value should be the array, double brackets should be used (e.g. `[[<value>]]`). In a default transformation, this is not necessary.

**Obs:** If the transformation value does not match the source value for an already existing node, the transformation value will replace the existing one.

#### Path Attribute

The `@jdt.path` attribute can be used if a specific node or multiple nodes should be changed. It can also be used to change nodes within arrays. See Attributes for more information.

Source:
``` javascript
{
    "A": {
        "TransformThis": true
    },
    "B": {
        "TransformThis": false
    },
    "C": {
    },
    "D": {
        "TransformThis": "WrongValue"
    },
    "E": {
        "TransformThis": false,
        "Items": [
            {
                "Value": 10
            },
            {
                "Value": 20
            },
            {
                "Value": 30
            }
        ]
    }
}
```

Transform:
``` javascript
{
    //Executes for all nodes on this level
    "@jdt.merge" : [{
        "@jdt.path" : "$.*",
        "@jdt.value" : {
            "Default" : 0
        }
    },
    //This only executes for matching nodes
    {
        "@jdt.path" : "$[?(@.TransformThis == true)]",
        "@jdt.Value" : {
            "Transformed" : true
        }
    }],
    "E": {
        // Accessing objects in array
        "@jdt.merge" : {
            "@jdt.path" : "$.Items[?(@.Value < 15)]",
            "@jdt.value" : {
                "Value" : 15,
                "Changed" : true
            }
        }
    }
}
```

Result:
``` javascript
{
    "A": {
        "TransformThis" : true,
        "Default" : 0,
        "Transformed" : true
    },
    "B": {
        "TransformThis": false,
        "Default" : 0
    },
    "C": {
        "Default" : 0
    },
    "D": {
        "TransformThis": "WrongValue",
        "Default" : 0
    },
    "E": {
        "TransformThis": false,
        "Items": [
            {
                "Value" : 15,
                "Changed" : true
            },
            {
                "Value": 20
            },
            {
                "Value": 30
            }
        ],
        "Default" : 0
    }
}
```

#### Value Attribute

The `@jdt.value` attribute in a Merge is the only type that supports nested transformations. This means that transformations that should be executed in newly created or merged nodes can be added through this value.

### Replace

| Use: | `"@jdt.replace" : <Value>` (Case sensitive) |
| ---- |:-------------------------------------------:|


| Value Type: | Behavior                                                    |
| ----------- | ----------------------------------------------------------- |
| Primitive   | Replaces the current node with the given value |
| Object      | If the object contains JDT attributes, apply them. See Attributes. <br> If not, replaces the node with the given object.
| Array       | Applies merge with each element of the array as the transformation value. <br> If the transformation value is an array, replace the node with the given array.

#### Example

Source:
``` javascript
{
    "A" : {
        "A1" : "11"
    },
    "B" : {
        "1B" : 12,
        "2B" : 22
    },
    "C" : {
        "C1" : 31,
        "C2" : 32
    }
}
```

Transform:
``` javascript
{
    "A": {
        "@jdt.replace": 1
    },
    "B": {
        "@jdt.replace": {
            "B1": 11,
            "B2": 12
        }
    },
    "C": {
        // Double brackets are needed to specify
        // the array as the transformation value
        "@jdt.replace": [[
            {
                "Value": 31
            },
            {
                "Value": 32
            }
        ]]
    }
}
```

Result:
``` javascript
{
    "A" : 1,
    "B" : {
        "B1" : 11,
        "B2" : 12
    },
    "C" : [
        {
            "Value": 31
        },
        {
            "Value": 32
        }
    ]
}
```

#### Path Attribute

Source:
``` javascript
{
    "A" : {
        "A1" : 11,
        "A2" : "Replace"
    },
    "B" : [
        {
            "ReplaceThis" :true
        },
        {
            "ReplaceThis" : false
        }
    ]
}
```

Transform:
``` javascript
{
    "@jdt.replace" : {
        "@jdt.path" : "$.A.A2",
        "@jdt.value" : 12
    },
    "B" : {
        "@jdt.replace" : {
            "@jdt.path" : "@[?(@.ReplaceThis == true)]",
            "@jdt.value" : {
                "Replaced" : true
                }
        }
    }
}
```

Result:
``` javascript
{
    "A" : {
        "A1" : 11,
        "A2" : 12
    },
    "B" : [
        {
            // The entire object was replaced
            "Replaced" : true
        },
        {
            "ReplaceThis" : false
        }
    ]
}
```

## Order of Execution

Transformations should be done in depth-first order to guarantee that everything is executed. Breadth-first order would have a slightly faster execution time as Remove transformations would potentially exclude other transformations from child nodes. For the same reason, this ordering could exclude transformations on lower level nodes that a user would like to execute.

In the same level, the order of priority for transformation is as follows:

Remove > Replace > Merge > Default > Rename

This order guarantees that explicitly named transforms execute first. It also guarantees that removals prevent unnecessary transformations from occurring.

### Processing Transform Files

To guarantee the order of execution defined previously, the transformed file is processed in the following order.

Starting from the root node:

1)	Enqueue all JDT transformations, identified by @jdt verbs
2)	Iterate through the rest of the node
    1.	Object
        1.	If it exists in the original file: step into the object and start from step 1
        2.	If it does not exist, enqueue a merge transformation
    2.	Array: Enqueue a merge transformation
    3.	Primitive: Enqueue a merge (replace) transformation
3)	Process all the transformation in the queue, per the order of execution
