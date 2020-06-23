# Unity Coding Style

## Nomenclature

### General
Choose easily readable identifier names.  
**Example:** `verticalAlignment` is much better than `alignmentVertical`.  

Favor readability over brevity.  
**Example:** `userGroup` is much better than `usrGrp`.  
*Exceptions*: `id`, `url`, etc... (well known abbreviations)

In code, acronyms should be treated as words. **DO NOT** use all CAPS acronyms in variable/function names.  
**Example:**
```csharp
XmlHttpRequest
String url
findPostById
```
**DO NOT:** `XMLHTTPRequest`, `URL`, or `FindPostByID`

### Namespaces
Namespaces are all **PascalCase**, multiple words concatenated together, without underscores ( \_ ). The exception to this rule are acronyms like GUI or HUD, which can be uppercase.  
**Example**:
```csharp
namespace FPSGame.HUD.Healthbar 
{ 
    // ... 
}
```

### Classes & Interfaces
Classes and interfaces are written in **PascalCase**, multiple words concatenated together, without underscores ( \_ ). The exception to this rule are acronyms like GUI or HUD, which can be uppercase.  
**Example**:
```csharp
class MyClass 
{ 
    // ... 
}

interface IObserver 
{ 
    // ... 
}
```

### Methods
Methods are written in **PascalCase**, multiple words concatenated together, without underscores ( \_ ). The exception to this rule are acronyms like GUI or HUD, which can be uppercase.  
**Example**:
```csharp
class MyClass
{
    void MyMethod(...) 
    { 
        // ... 
    }
}
```

### Fields
All fields are written in **camelCase**, multiple words concatenated together, without underscores ( \_ ). **DO NOT** prefix with `_`, `m_`, or `m` or any other letter to denote class member access. 

`const` and `readonly` fields are instead written in SNAKE_CAPS.  
**Example**:
```csharp
class MyClass
{
    bool myVariable;
    public float currency;
    private int healthPoints;
    protected string playerName;
    const int I_AM_A_CONST = 42;
}
```
**DO NOT:** `_myVariable` OR `m_myVariable` OR `mMyVariable`

### Properties
All properties are written in **PascalCase**, multiple words concatenated together, without underscores ( \_ ). **DO NOT** prefix with `_`, `m_`, or `m` or any other letter to denote class member access.  
**Example**:
```csharp
class MyClass
{
    public HealthPoints { get; set; }
}
```

### Parameters
Parameters are written in **camelCase**, multiple words concatenated together, without underscores ( \_ ). **DO NOT** prefix with `_`, `m_`, or `m` or any other letter to denote class member access. Single character values are to be **avoided** except for temporary looping variables.  
**Example:**
```csharp
void DoSomething(Vector3 location) 
{ 
    for (int i = 0; i < locations.Count; ++i)
        Debug.Log(locations[i]);
}
```

### delegate functions, Action, and Func
All delegate functions, Action, and Func are written in **PascalCase**, multiple words concatenated together, without underscores ( \_ ). **DO NOT** prefix with `_`, `m_`, or `m` or any other letter to denote class member access.  
**Example:**
```csharp
public delegate int MyDelegateFunction(int x, int y);
public Action<int, int> MyAction = MyDelegateFunction;
public Func<int, int, int> MyFunc = MyDelegateFunction;
public event Action<int> UnityActionEvent;
```

## Declarations

### Use of `var`
**DO NOT** use `var` key word. The only exceptable use is with temporary variables like the index in a for loop for example.
**Example:**
```csharp
    // Good Use
    for(var i = 0; x < 10; i++)
    {
        //...
    }

    // Bad Use 
    var person = GetCulprit();
```

### `using` statements
`using` statements should be grouped together by the first word before the dot(`.`) in the namespace names.  
**Example:**
```csharp
using UnityEngine;
using UnityEngine.UI;

using System.Collections;
```

### Access Level Modifiers
Access level modifiers should be explicitly defined for classes, methods and member variables. The only exception is for classes with internal access in which case you can just exclude the `internal` keyword as it will default to it anyways.

### Fields & Variables
Prefer single declaration per line. You should use the `[SerializeField]` attribute instead of marking a data member as `public`. If a field needs to be public but should not be exposed in the inspector, mark it with `HideInInspector` attribute.  
**Example:**
```csharp
class Example
{
    [SerializeField] private string exposedFieldInInspector = "";
    [SerializeField] public string exposedPublicField = "";
    [HideInInspector] public int hiddenPublicField;
}
```

### Classes
Exactly one class per source file, although inner classes are encouraged where scoping is appropriate.

### Interfaces
All interfaces should be prefaced with the letter **I**.  
**Example:**
```csharp
class IObserver 
{ 
    // ... 
}
```

## Spacing

### Indentation
Indentation should be done using **tabs** or **spaces** but **never both** in the same project.  
If using spaces for indentation, use **2** or **4** spaces but **never both**.

### Line Length
You should ***try*** to keep lines no longer than **150** characters long.

### Horizontal Spacing
Separate parameters with a space only after each comma when listing parameters in between normal and curly braces, with the exception of curly braces where both opening and closing braces are on the same line in which case put a space between the braces, the commas, and the words. 
Furthermore, arithmetic operators should be separated with a space between other words.  
**Example:**
```csharp
public int Health { get; set; } // <-- space before get; and after set;
public int[] numbers = { 1, 2, 3 } // <-- space before 1 and after 3

public int DoExample(int param1, int param2)
{
    if (condition1 || condition2 && condition3)
        return numbers[0] - numbers[1] * numbers[2];
}
```
**DO NOT:**
```csharp
public int Health {get; set;}
public int[] numbers = {1, 2, 3}
public int DoExample(int param1,int param2) 
{
    if (condition1||condition2&&condition3)
        return numbers[0]-numbers[1]*numbers[2];
}
```

### Vertical Spacing
There should be exactly one blank line between classes and methods to aid in visual clarity 
and organization. Blank lines within methods and classes should separate functionality. Also, you should leave 1 blank line between a one line conditional or looping statement body with no braces if there is another line of code following it.  
**Example:**
```csharp
public void DoExample()
{
    if (condition) return;
    
    while (health > 0)
        DoStuff();
    
    if (CheckSomething())
        DoSomething();    // <-- we do not add a blank line after this line
}
```

## Brace Style

All curly braces get their own line unless it is a one line automatic Property or array initialization. **DO NOT** use **egyptian-style braces**.  
**Example:**
```csharp
public int Health { get; set; }

public int[] numbers = { 1, 2, 3 }

public void DoExample()
{
    // ...
}
```
**DO NOT:**
```csharp
public void DoExample() { // <-- egyptian-style
    // ...
}
```

## Switch Statements

**Avoid using switch statements** and prefer using if statements.
If you do chose to use them and if the `default` case can never be reached, be sure to remove it.  
**Example:**
```csharp
switch (variable) 
{
    case 1:
        break;
    case 2:
        break;
}
```

## Language

Use US English spelling.  
**Example:**
```csharp
string color = "red";
```
**DO NOT:** `colour`  
**Unity exceptional cases:** `MonoBehaviour`

## Documentation

[Recommended tags for documentation comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/recommended-tags-for-documentation-comments)

## Copyright Statement

If the source code needs to be protected by copyright, then the appropriate copyright statement should be included at the top of every source file in multiline comments.  

## Unity Code Style Example

```csharp
using UnityEngine;

using System.Collections;

/// <summary>
/// documentaton comments...
/// </summary>
namespace ExampleNamespace
{
    class InternalClass : MonoBehaviour, IObserver
    {
        public const int EXAMPLE_CONSTANT = 7;

        public enum ExampleEnum { None, Example, MultiWordExample }

        [SerializeField] private string exposedFieldInInspector = "";
        [SerializeField] public string exposedPublicField = "";
        [HideInInspector] public int hiddenPublicField;

        private float exampleCount;

        public float ExampleSpeed { get; set; }

        public void MethodName(string exampleName)
        {
            int exampleCount = 0;
            
            example_count = this.GetComponent<Transform>().ChildCount();
        }

        private class SecretClass
        {
            public Vector3 color;
        }
    }
}

```
--------------------------------------------------

