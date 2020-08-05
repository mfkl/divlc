# divlc

**di**ff tool for lib**vlc**.

.NET Core CLI tool to generate public API diffs between 2 libvlc versions (minors or majors).

# LibVLC differences

LibVLC bindings maintainers need to keep up with LibVLC public API and documentation changes. This tool aim to help with this task.
Here is a list of elements that may change between 2 libvlc hash
- libvlc function names,
- total libvlc function count
- libvlc function parameter type,
- libvlc function parameter name,
- libvlc function return type
- libvlc struct changes (new/deleted struct, new/deleted/modified struct field types/names)
- APIs documentation

```
.\divlc.exe --help
divlc 1.0.0
Copyright (C) 2020 divlc

  -v, --verbose    Set output to verbose messages.

  --libvlc4        The hash of the libvlc 4 version to compare.

  --libvlc3        The hash of the libvlc 3 version to compare.

  --no-clone       Do not clone, use the provided local git repositories.

  --files          Comma separated list of libvlc header files to include: Defaults to all

  --no-comment     Do not include comments/documentation in the comparison. Defaults to false

  --help           Display this help screen.

  --version        Display version information.
  ```