﻿namespace FIRConvolution.Tests;

public delegate T UnitTestFilterFactory<out T>(float[] h, MemoryAllocator allocator);