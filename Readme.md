
![Mindustry Compiler Icon](https://i.imgur.com/xW0zxYJ.png)

# Mindustry Compiler

Mindustry Compiler is an open-source C-style compiler for the game Mindustry. You can write basic C-style programs in your favorite code editor, and the program will compile it into assembly-like language that is compatible for the logic processors in game.
![Mindustry Compiler screenshot](https://i.imgur.com/5J60pUm.png)

To use it, create a `.cpp`, `.c`, or `.txt` file in your text editor of choice. Then, click the `Open` button at the top of the Mindustry compiler.

As you change the source file, and when you switch focus into Mindustry, the Mindustry compiler will automatically compile the contents of your source file into your clipboard.

When you switch into the game, a compilation overlay will breifly appear. Select a logic processor and select 'Import From Clipboard'.

![GjAscIk.png (347×180)](https://i.imgur.com/GjAscIk.png)

... and then you're all set!

__Features__
* C-style programming
* Automatic compilation to clipboard
* Program structure flexibility
* Arithmetic parsing
* Memory cell access (`cell2[i]`)
* Branching (`if`, `else if`, `else`)
* Loops (`for`, `while`, etc.)
* Custom function definitions
* Built-in `sleep()` and `wait()` functions
* Direct assembly injection (`asm()`)
* Preprocessor `#defines`

# Program Structure

__basic program__
```c
// This is a simple program
conveyor1.enabled = false;
sleep(2);
conveyor1.enabled = true;
sleep(1);
```

__advanced program__
```c
// Initialization code
// Code that is outside 'main()' runs only once on start.
i = 0;

// Main function
// Loops forever. Variable values do not reset on loop.
void main()
{
	i++;
	println(i);
	sleep(1);
}
```


### Typenames
Typenames are ignored.
```c
// Type names do not matter (left in for C-style compatibility)
static const volatile int number = "hello world";
println(number);
// prints 'hello world'

// Python style-compatible (assign, no declare/typename)
myStr = "hello";
```
### Return
Return either exits a custom function, or ends program execution (if inside `main()`)
```c
void myFunction()
{
	return i + 55;
}

// Initialize code
int i = 5;

// Main loop
void main()
{
	print(myFunction());
	
	// Restart program (equivalent to 'end')
	return;
}
```


# Basic Functions

### print(_value_, _value2_, _value3_, ...)
### println(print(_value_, _value2_, _value3_, ...)
```c
// Appends a value to the print output (optionally with a line end)

print("Right now, ");
println("'x' is currently equal to ", x);

// Output: "Right now, 'x' is currently equal to 5"
```

### Wait
> wait(_condition_)
```c
// Stop program execution until a condition is met.
  
println("Container is full!");
  
// Pause until container is empty ...
wait(container1.totalItems < 25);
println("... but now its empty.");

// Pause until container is full ...  
wait(container1.totalItems > 275);
println("... aaaand its full again.");
```
### Sleep
> sleep(_seconds_)
```c
// Stop program execution for a number of seconds.

// Sleep for 3 seconds
sleep(3);

// Sleep for 1.25 seconds
x = 1.25;
sleep(x);
```
### asm
```c
// Add assembly instructions directly using 'asm()'

// Print 'hello world' to 'message3'
asm(print "hello world");
asm(printflush message3);

// Turns conveyor1 off
asm(control enabled conveyor1 0);
```
# Logic (Branch, Loop)
___Note:__ Branches need to start and end with `{` and `}` respecitvely (implied, one-line loops/branches not yet supported)._
### If /Else-if/Else
```c
// Standard if/else-if/else logic.
if (a == 25)
{
	// action
}
else if(b < c - d && e == 2.71)
{
	// other action
}
else
{
	// different action
}
```

### For Loop
```c
// Counts from 0 -> max - 1
// Type names are auto--removed on compile (left in for 'C-style')
for (int i = 0; i < max; i++)
{
	println("Current value: ", i);
}
```


# Math Functions

```c
// Add, subtract, parenthesis, exponent
result = a + (b - d) * 2^3;
result += 1;
result *= 2;
result /= 7.5;

// Modulo-division
result = (x + 4) % 25;

// Trig functions
result = (sin(x) * cos(y)) / tan(z);

// Floor/Ceil (outputs '2' and '4')
result = ceil(2.1)
result = floor(4.9);

// Min/max functions
x = min(1, 2);
z = max(3, 100000);

```



# Memory
```c
// Read/write to memory using standard array notation.
// If a cell # is not specified, default is 'cell 1'

// Write '10' to cell at index '0'...
cell[3] = 10;

// Add into cell 4 (index 'x + 2') whatever cell 6 has at index '31'
cell4[x + 2] += cell6[31];

// Fill cell-5 up with numbers 0 -> 9 ...
for (int i = 0; i < 10; i++)
{
	cell[5] = i;
}
```



## Game Object Control (sensor/control)
```c
// Get or set properties objects using dot-properties.
// (equivalent to 'control/sensor' commands)

// Turn conveyors/buildings on or off
conveyor1.enabled = false;
driver3.enabled = x > 3 && y - 2 == 1;

// Set the 'type' of an unloader ('type' auto-remaps to 'config')
unloader1.type = @copper;
unloader2.config = @titanium

// Get the amount of silicon in 'vault1' ...
println(vault1.copper);
if (vault1.totalItems < 5)
{
	println("your vault is empty bruh");
}
```
## Preprocessor #define
```c
// C-style preprocessor defines (primative implementation)
// Preprocessor functions are not supported (yet).

// Tell compiler to replace 'Q' with 'cell[35 + x]'
#define Q cell[35 + x]

x = 2;
Q = 3 * x;
println(cell[35 + x]);
// prints '6';
```

### Main function
The main loop of the funciton. Loops forever. Calling `return` in main will 'end' the program, starting it over (re-initialize).

## Custom function definitions
```c
// Custom function declaration
void doCustomThing(number, string)
{
	// Print the string and double the number
	number *= multiplier;
	println(string, ": ", number);

	// Return the multiplied number
	return number;
}

// Main loop
void main()
{
	// Set multiplier (vars inside custom function*)
	multiplier = 50;
	result = doCustomThing(4, "A string");
	println(result);
	// prints '200'
}
```

### Note on functions:
* Functions can call `return` to exit the function, or `return x` to return a value out of the funciton.
  ___Note__: the returned value is undefined in functions that do not return._
```c
void returnNoting()
{
	return;
}

int returnANumber()
{
	return 100;
}
```
* Pre-defined variables with the same name as parameters are saved. Defining a variable `num` in the main loop, then calling a function with a parameter named `num` will _not_ overwrite the main function's version. However, any variables that do not share names with a parameter are modifiable.
```c
// Funciton with parameter named 'num'
void doThing(num)
{
	num = 42;
	
	// Print 'doThing's version of 'num'
	println(num);  // prints '42'
}

// Main loop
void main()
{
	// Set 'main's variable 'num' to 5 ...
	num = 5;
	doThing(num);
	
	// Print 'main's version of num
	println(num); // prints '5'
}
```

* Functions calls are ___not recursive___ compatible. That is to say, calling a custom function from inside another function is not supported (yet). This is planned to be added.
```c
void first()
{
	println("first");
}

void second()
{
	// Calling first will get stuck
	first();
	println("second");
}

second();
println("done");
```
