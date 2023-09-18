# FIRConvolution

Faster FIR filter convolution for Unity.

<img src="Wiki/header.png" width="500"/>

## Description

Collection of 12 algorithms for FIR filter convolution, with a focus on half-band filtering.

Most of the algorithms are vectorized, leveraging SIMD extensions through Unity Burst.

Check the sample scene to see them in action and their performance using the profiler.

## Installation

Add the package to your Unity project using the following Git URL: 

`https://github.com/aybe/FIRConvolution.git?path=Assets/FIRConvolution`

## Performance

### Motivation

Implementing a high quality FIR filter convolution in pure managed code is too slow.

The audio DSP CPU usage ranges between ~30% to ~50% no matter how optimized.

Result is that audio dropouts are frequent since `OnAudioFilterRead` is struggling.

### Profiling environment

Half-band filter, 44100 Hz, 461 taps, 1 channel, 10 measurements, 1000 iterations, 1024 samples.

Mimicking what'd happen inside `OnAudioFilterRead`, other scenarios would yield other results.

The method call overhead clearly has an impact, hindering some algorithms as you'll see below.

### Profiling results

#### Legend

- `[Scalar|Vector]`
  - `Scalar` : 1 sample at a time
  - `Vector` : 4 samples at a time
- `[Full|Half]`
  - `Full` : full-band filter
  - `Half` : half-band filter
- `[Full|Half]` (only for half-band filter)
  - `Full` : iterating the taps loop fully, i.e. using 50% of the taps
    - in a half-band filter, half of the taps are zeros and thus can be ignored
  - `Half` : iterating the taps loop first half, i.e. using 25% of the taps
    - in addition to above, leveraging taps symmetry to halve the iterations
- `[Inner|Outer|OuterInner]`
  - `Inner` : taps loop vectorized
  - `Outer` : samples loop vectorized
  - `OuterInner` : both loops vectorized

#### Managed, sorted alphabetically

<img src="Wiki\managed-abc.png" width="500"/>

#### Managed, sorted from fastest to slowest

<img src="Wiki\managed-spd.png" width="500"/>

#### Native, sorted alphabetically

<img src="Wiki\native-abc.png" width="500"/>

#### Native, sorted from fastest to slowest

<img src="Wiki\native-spd.png" width="500"/>

#### Managed VS Native, sorted alphabetically

<img src="Wiki\vs-abc.png" width="500"/>

#### Managed VS Native, sorted from fastest to slowest

<img src="Wiki\vs-spd.png" width="500"/>

### Conclusions

On the managed side:
- scalar half-band
  - almost twice as fast as full-band variant, this is totally expected
  - half loop variant gain is marginal although halved tap iterations 
- vectorized
  - outer loop variant is the slowest in most cases, it's the opposite for native
  - SIMD isn't used while in vanilla .NET it is when inspecting generated code

On the native side:
- scalar half-band
  - little gain, mostly due to short input and overhead
- vectorized, outer/inner variant
  - full-band, performs worse than the other variants
  - half-band, performs better but gain is marginal

Overall, considering native implementations:
- full-band: outer variant is the fastest
  - outer/inner variant ought to be the best but turns it isn't in the end
- half-band: half loop, outer/inner variant is the fastest
  - when a substantially longer/trickier algorithm ends up being better

## Notes

With a few tricks, it is possible to test code for Unity outside it, i.e. using MSTest and Live Unit Testing.

This project does that and it has been tremendously helpful for implementing the different algorithms.
