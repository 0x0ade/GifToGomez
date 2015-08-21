# GifToGomez
Converts most GIFs into a FEZ character animation spritesheet to be used by FEZMod.

# Usage instructions
1. Put the GIF and GifToGomez.exe into one folder.
2. Open CMD (no GUI yet).
3. 'cd' into the folder.
4. Run the following command:

```
GifToGomez.exe gifhere.gif animname
```

Example:

```
    GifToGomez.exe sanic.gif run
```

To "install" the skin, copy the resulting animation XML and PNG into FEZ/Resources/character animations/&lt;character here&gt;/ (requires FEZMod to be run with -da once)
