# Special Values
## Resources
> @copper
> @lead
> @scrap
> @sand
> @coal
> @titanium
> @thorium
> @graphite
> @metaglass
> @spore pod
> @silicon
> @plastanium
> @phase fabric
> @surge alloy
> @blast compound
> @pyratite
> 
### Liquids
> @water
> @oil
> @slag
> @cryofluid


# Basic Functions
>### print(_value_)
Appends a value to the print output.

>### println(_value_)
Prints a value to the print output, following with a new line.

>### amount(_resource_, _object_)
Returns the amount of 'resource' type inside 'object'

>### return
Ends execution. Starts again on next tick.


# Math Functions

> ###  _a_ + _b_ 
> ### _a_ - _b_ 
> ### _a_ * _b_
> ### _a_ / _b_ 
> ### _a_ % _b_
> ### _a_ ^ _b_, sqrt(_number_)
>### sin(_number_), cos(_number_), tan(_number_)
>### log(_number_), log10(_number_)
>### _number_
>### floor(_number_), ceil(_number_)
>### min(_a_, _b_), max(_a_, _b_)
>### rand(_max_exclusive_)
>### angle(x, y)



# Memory
>### _cell_[_index_]
>### _cell_[_index_] = _value_
Gets or sets the value stored in a memory cell at index



# Object control
>### object.enabled
>### object.enabled = false
Gets or sets the enabled state of an object (1/0/true/false)

>### object.type = copper
>### _value_ = object._type_
Gets or sets the resource type of an object (equivalent to `.configure`)

>### object.configure
Gets or sets the configure of an object (equivalent to `.type`)

# If

## Operators
```
if (a - b > 50)
{
	// action
}
else if( b == 32)
{
	// other action
}
else
{
	// different action
}
```