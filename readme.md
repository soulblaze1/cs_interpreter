
C# interpreter with support for some basic things.

I tried to write something intuitive. The classes are easy to read and understand, and C# provides some incredibly useful syntax sugar.

The main idea was to be able to generate and run code dynamically. There are a few issues like how the function calling isn't properly done as in functional programming languages - so "a.b.c()" syntax doesn't work. But plenty is working.


Dependencies: You need to get csparser. https://archive.codeplex.com/?p=csparser

