# Higurashi Assembly

This repository contains the vanilla and modded versions of the decompiled `Assembly-CSharp.dll` for all Higurashi Chapters.

## Branches and Arcs

There are three branches per arc, the most pertinent being `<arc>-mod`, the mod version of a given arc. The vanilla MG and reconstructed vanilla Steam (may not be exactly the same) versions of the game can be found for each arc at `<arc>-mg` and `<arc>-steam` respectively, and should be useful for comparison.

The branch lineage looks like this, where the parent of a branch at depth n is the nearest branch above at depth n-1; for example, the parent of `hima-mg` is `tata-mg`.

```
oni-mg
* oni-steam
** oni-mod
*** mod <-- development
**** f-lipsync <-- example feature in progress
* wata-mg
** wata-steam
*** wata-mod
** tata-mg
*** tata-steam
**** tata-mod
*** hima-mg
**** hima-steam
***** hima-mod
****** console-arcs
**** mea-mg
```

...etc. The rationale behind this is that each arc seems to build on the code from the previous arc, and the Steam versions seem to be extensions of the MG versions, while our mods are based on the Steam version.

Ongoing development will be based on the `mod` branch and the relevant branch for each arc is `<arc>-mod`, with the exception of `console-arcs`, which is branched off `hima-mod`.

## Decompilation

The decompiled code was created via ILSpy commit 3fb25e543bda4ac6664b44b7183b5e29161b51b9.

This requires both the MG and the steam versions of the game.

Steps to get the code for a new game (using Minagoroshi as an example)):
* Prepare a new branch
  * Checkout the previous branch's vanilla arc, let's assume tsumi-mg
  * `git checkout -b mina-mg`
  * Take note of the DLLs in the `./DLLs` folder
  * Delete all code from this branch's folders without commmitting yet
* Open ILSpy
* Load Assembly-CSharp.dll from that MG-version `<data>/Managed` directory
* Save the code as a project to the repo's workspace, where the tsumi code was and is now deleted
  * Now, the Minagoroshi code will be there
* Carry over any DLLs from the new game's <Managed> directory to the DLLs folder that were there for the previous game's DLLs folder (also, the Assembly-CSharp.csproj file will reference these DLLs)
* Run the program located at https://github.com/07th-mod/reorder-attributes on all the C# files (e.g. using a combination of `find` and `xargs` in a unix command line).  After doing this, `git diff` should not show changes resulting from inconsistent attribute ordering.
* Try to build.  If it fails, the decompiler may have failed to decompile some things correctly
  * Compare to the previous arc's code using `git diff`.
  * If the diff looks substantial, then it may require manually analyzing the code to figure out its intended behavior and correction from there.
  * If the diff looks trivial, then it is probably the same old code as the last arc; you can checkout the corresponding code from the previous arc instead.
* Test the build by copying over the resulting Assembly-CSharp.dll to the games's installation directory and running the game.  Ensure everything looks to be in a working state before proceeding.
* Check in the new branch.
* For the <arc>-steam branch, this process can either be repeated with the steam version's DLL with the parent branch being the mg version of this game instead of the previous arc, or one can merge the `steam` branch (recommended).
  * If doing it from the Steam DLL, check the `steam` branch history to see what changes we have made to the base Steam code and apply those accordingly.
* For the <arc>-mod branch, create the <arc>-mod branch based on the steam branch and merge the mod branch.  It may also be possible to base this branch off of the previous branch's mod branch for less conflicts, but the previous arcs all has a branch parentage of arc-mg <- arc-steam <- arc-mod as in the diagram above.

## Supported video formats

### For Chapters 1-8 and console

**TODO** - If anyone knows what encoding settings we used for the previous chapters (for linux and windows), let me know.

### For Chapter 9: (Rei) and above

These chapters use the native Unity video playback for both Windows and Linux.

Additionally, I modified it so that it will play [any compatible container format it can find](https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html) (note that just because the container is correct doesn't mean the video will play!).
Previous chapters would play `.mp4` files for Windows, and `.ogv` files for Linux.

#### On Windows
 - Many video containers and formats are supported
 - Suggest using H.264 + AAC audio

#### On Linux
 - Only very specific formats are supported.
 - Unity suggests using `vp8` video encoding files and `vorbis` audio encoding.
 - The following ffmpeg encode command is tested working on Ubuntu 20.04: `ffmpeg -i mv11.mp4 -c:a libvorbis -c:v libvpx -crf 20 -b:v 10M mv11.webm`.
    - `-b:v 10M` more or less sets maximum bitrate that the encoder can use (I think)
    - `-crf 20` more or less sets the quality (I think)
    - Increasing one without the other will cap the quality/filesize, so you will need to play around with the values.

 - The `.ogv` container didn't work for me, even though the Unity page states it is supported.
 - For more details on VP8 encoding see [https://trac.ffmpeg.org/wiki/Encode/VP8](https://trac.ffmpeg.org/wiki/Encode/VP8)

For more details on the Unity Video Player, see [https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html](https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html)

### Encoding Tips

Use the `-to` command to process just part of the video for testing. For example, `ffmpeg -to 00:00:10 -i in.webm out.mp4` will process just the first 10 seconds of the video.

## Build details

Build info:
* These projects (`Assembly-CSharp.csproj`) have been built successfully with Visual Studio Community 2017 v15.7.5 and msbuild v15.7.180.61344, though in theory any compiler supporting C# 7.1 should suffice.
* To build with Visual Studio, load the sln file and use build command.
* To build with msbuild, execute `msbuild Assembly-CSharp.csproj` via the command line.  On Windows, this may require a developer command prompt to have `msbuild` in your PATH; this comes with Visual Studio.
* The resulting `Assembly-CSharp.dll` will be located at `bin/Debug/Assembly-CSharp.dll`.

The built DLLs are available here:

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=oni-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Onikakushi](https://07th-mod.com/higurashi_dlls/onikakushi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=wata-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Watanagashi](https://07th-mod.com/higurashi_dlls/watanagashi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=tata-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Tatarigoroshi](https://07th-mod.com/higurashi_dlls/tatarigoroshi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=hima-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Himatsubushi](https://07th-mod.com/higurashi_dlls/himatsubushi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=mea-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Meakashi](https://07th-mod.com/higurashi_dlls/meakashi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=tsumi-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Tsumihoroboshi](https://07th-mod.com/higurashi_dlls/tsumihoroboshi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=mina-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Minagoroshi](https://07th-mod.com/higurashi_dlls/minagoroshi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=matsuri-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Matsuribayashi](https://07th-mod.com/higurashi_dlls/matsuribayashi/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=rei-mod)](https://travis-ci.com/07th-mod/higurashi-assembly) [Rei](https://07th-mod.com/higurashi_dlls/rei/Assembly-CSharp.dll)

[![Build Status](https://travis-ci.com/07th-mod/higurashi-assembly.svg?branch=console-arcs)](https://travis-ci.com/07th-mod/higurashi-assembly) [Console Arcs](https://07th-mod.com/higurashi_dlls/consolearcs/Assembly-CSharp.dll)

Other branches may not have up-to-date READMEs.
