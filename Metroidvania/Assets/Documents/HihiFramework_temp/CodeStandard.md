# Code Standard

## Capitalization Styles

| Type | Capitalization Style | Remarks |
| ---- | -------------------- | ------- |
| Namespace | PascalCase | |
| Interface | PascalCase | |
| Class | PascalCase | |
| Enum type | PascalCase | |
| Enum value | PascalCase | |
| Event | PascalCase | |
| Method | PascalCase | |
| Constructor parameter | camelCase | |
| Method parameter | camelCase | |
| Local variable | camelCase | |
| Property | PascalCase | |
| Constant | PascalCase | |
| Static field | PascalCase | |
| Private instance field | camelCase | |
| Protected instance field | camelCase | Rarely used. Use property instead if no concern. |
| Public instance field | PascalCase | Rarely used. Use property instead if no concern. |


### Other rules

* For names that contain abbreviations:
	* if it is 2-char abbreviations, use the same case for the both characters
		* class `UIManager`
		* var `uiManager`
		* Exception : `Id` (e.g., `userId`)
	* if it is 3-char abbreviations or above, use PascalCasing or camelCasing
		* Namespace `HihiFramework`
		* var `ftpTransfer`

## Naming Guideline

* Use prefix `I` for interfaces (e.g., `IDisposable`)
* Use suffix `Base` for abstract classes and classes that are created for inheritance (e.g., `AnimalBase`)
* Use underscores(`_`) <b>only</b> for the cases:
	* prefix of a private field which is with corresponding property, e.g.,
	``` CSharp
	private string _text;
	public string Text {
		get { return _text; }
		set { _text = value + " Hello"; }
	}
	```
	* connect to the suffix that distinguish similar objects with different properties (e.g., `command_Hit`, `command_Jump`). In this case, the suffix after underscore should be in <b>Pascal case</b>.
* Do <b>not</b> use Hungarian notation (type prefix) (e.g., `iCount`)
* Use singular word for Enum type name, except bit field enums, e.g.,
	``` CSharp
	public enum Command {
		Hit,
		Jump,
	}
	[Flags]
	public enum LogTypes {
		None = 0,
		UI = 1 << 0,
		Input = 1 << 1,
	}
	```

### Delegate/Events

---

<b>Note</b> : Below event standard does not align with Microsoft standard. I do not use the `EventHandler` and `EventArgs` structure in order to reduce the complexity of codes.

---

* Use a verb for an event name. Use a gerund (the "ing" form of a verb) instead of `BeforeXxx` for a pre-event and use a past-tense verb instead of `AfterXxx` for a post-event
	``` CSharp
	// Correct
	public event Action LangChanging;
	public event Action LangChanged;
	// Incorrect
	public event Action BeforeLangChange;
	public event Action AfterLangChange;
	```
* If you need a protected method for the derived class to override the event trigger behaviour, use the name `OnXxx` which is the event name with a prefix `On`
	``` CSharp
	public event Action LangChanged;
	protected virtual void OnLangChanged () {
		LangChanged?.Invoke ();
	}
	```
* Use suffix `Handler` for the name of the method that is added to be an event handler
	``` CSharp
	LangManager.LangChanged += LangChangedHandler;
	```
* Use prefix `on` for the callback parameter name of a method
	``` CSharp
	public void DoSomething (Action onFinished) {
	}
	```

## Miscellaneous

* Always state the access modifiers
* Use `var` instead of specify type whenever possible
* Declare stuffs at the top of a class in below order (except the case that there is a seperate region that contains all the stuffs of a isolated content, e.g. overriding an interface):
	1. enums
	2. fields and corresponding properties that use in binding in Unity editor (Normally they are with `[SerializeField]` attribute)
	3. other fields and properties
	4. Constructors
	5. Unity life cycle methods (e.g. `Awake`, `OnDestroy`) and Init/Reset methods
	6. Others
* Do <b>not</b> use non private field. Use property instead.
	* Exception :
		* Classes that are constructed to use with `JsonUtility`. In this case, the field name follow JSON naming convension
		* private classes inside a class
* Use shift operator for bit field enum values
	``` CSharp
	// Correct
	[Flags]
	public enum LogTypes {
		None = 0,
		UI = 1 << 0,
		Input = 1 << 1,
	}
	// Incorrect
	[Flags]
	public enum LogTypes {
		None = 0,
		UI = 1,
		Input = 2,
	}
	```
* Add comma to the last item of a list, Dictionary, etc.
	``` CSharp
	// Correct
	private List<int> SampleList = new List<int> {
		1,
		2,
	};
	// Incorrect
	private List<int> SampleList = new List<int> {
		1,
		2
	};
	```
* Set `List`, `Dictionary`, etc. to be `readonly` and initialize them while declaring whenever possible in order to prevent null reference exception
	``` CSharp
	private readonly List<int> sampleList = new List<int> ();
	```

## References

---

<b>Note</b> : I basically follow the standard of below references, but not strictly.

---

* https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-1.1/xzf533w0(v=vs.71)?redirectedfrom=MSDN
* https://www.dofactory.com/reference/csharp-coding-standards