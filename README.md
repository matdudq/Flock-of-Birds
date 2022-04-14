# Flock-of-Birds
[![Unity](https://img.shields.io/badge/unity-2020.3.32%2B-blue.svg)](https://unity3d.com/get-unity/download) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Table Of Contents

- [Introduction](#introduction)
- [Examples](#examples)
- [Features](#features)
- [System Requirements](#system-requirements)
- [License](#license)

## Introduction <a name="introduction"></a>

**Flock-of-Birds** Simulation of flock of birds with use of DOT system inside Unity Engine.

Based on:
 - Craig W. Reynolds - A Distributed Behavioral Model: http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
 - Daniel Shiffman - Nature of Code: https://natureofcode.com/

## Examples <a name="examples"></a>
<img src="https://i.imgur.com/h5XXxMw.gif">

## Features <a name="features"></a>
The system can handle even 100k entities efficiently, thanks to using DOTS platform and Bin-Lattice spatial subdivision approach.

Flocking system contains behaviours implemented as separated jobs.
Result of behaviour is partial force applied to the unit. 
Each behaviour can be combined freely with the others.
List of implemented flocking behaviours:
 - Alignment
 - Separation
 - (TODO) Cohesion
 - (TODO) Target following
 
Each unit has tendency to behaviours described by behaviour statistic. 
(TODO) Every statistic is changing constantly with Perlin Noise curve, which allows us to achieve more interesting results.

## System Requirements <a name="system-requirements"></a>

Unity 2020.3.32+
  
## License <a name="license"></a>
 
[MIT](https://opensource.org/licenses/MIT)
