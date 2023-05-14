//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2022 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert C++ rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
	public static uint16_t[][] RectangularUint16_tArray(int size1, int size2)
	{
		uint16_t[][] newArray = new uint16_t[size1][];
		for (int array1 = 0; array1 < size1; array1++)
		{
			newArray[array1] = new uint16_t[size2];
			for (int array2 = 0; array2 < size2; array2++)
			{
				newArray[array1][array2] = new uint16_t();
			}
		}

		return newArray;
	}
}