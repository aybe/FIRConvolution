# FIRConvolution

Faster FIR filter convolution for Unity.

<img src="Wiki/header.png" width="400"/>

## Description

This project is a collection of 12 algorithms for FIR filter convolution, with a focus on half-band filtering.

Most of the algorithms are vectorized, leveraging SIMD extensions through the Unity Burst compiler.

Check the sample scene to see them perform filtering from within `MonoBehaviour.OnAudioFilterRead`.


## Installation

Add the package to your Unity project using the following Git URL: 

`https://github.com/aybe/FIRConvolution.git?path=Assets/FIRConvolution`

## Performance

### Motivation

Implementing a fast FIR filter convolution purely using managed code ended up being an impossible task, because as soon as one tries to uses a high-quality filter with many taps; the audio DSP CPU usage immediately ranges between 30% to 50%. This, no matter how hard you'd apply various optimizations in order to try speed up the processing time.

### Profiling environment

The candidate is a high-quality half-band FIR filter, with 461 taps and for 1 channel @ 44100 Hz.

Trying to mimick the typical use with 10 measurements and 1000 iterations for 1024 samples.

Both managed and native implementations are tested to give the audience an overall comparison.

### Profiling results

The results are surprising, some algorithms perform better than some others ought to be worse.

Note: an algorithm is fit to use without noticeable impact when it spends less than 10 seconds running.


**Legend:**

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

**Managed, alphabetically:**

<img src="Wiki\managed-abc.png"/>

**Managed, fastest to slowest:**

<img src="Wiki\managed-spd.png"/>

**Native, alphabetically:**

<img src="Wiki\native-abc.png"/>

**Native, fastest to slowest:**

<img src="Wiki\native-spd.png"/>

**Versus, alphabetically:**

<img src="Wiki\vs-abc.png"/>

**Versus, fastest to slowest:**

<img src="Wiki\vs-spd.png"/>

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
  - little gain, likely due to short input and overhead
- vectorized, outer/inner variant
  - full-band, performs worse than the other variants
  - half-band, performs better but the gain is marginal

Overall, considering native implementations:
- full-band: outer variant is the fastest
  - outer/inner variant ought to be the fastest but really isn't in the end
- half-band: half loop, outer/inner variant is the fastest
  - when a substantially longer/trickier algorithm ends up being faster

## Notes

In the repository, there are [extra projects](Projects) that implement testing using MSTest while using the Unity code base. Leveraging [a few tricks](Projects/FIRConvolution/Fakes), it is indeed possible to achieve such setup and in return benefit of a friendlier testing environment than Unity's one. As this is a Unity project, by convention the Visual Studio solution isn't committed so you'll have to add them manually to it.

Regarding getting the code to be runnable under vanilla .NET, it already does in the mentioned projects although there's an indirect use of `Unity.Mathematics`. However, one might not be able to use that assembly for licensing reasons. To address that issue, one shall create extra shims for the relevant types and methods then use the aligned memory allocator [for vanilla .NET](Assets/FIRConvolution/Runtime/MemoryAllocatorNet.cs) instead.

## Credits

https://fiiir.com

https://thewolfsound.com/fir-filter-with-simd

https://github.com/Rabadash8820/UnityAssemblies

