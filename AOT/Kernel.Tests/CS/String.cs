//
// (C) 2006-2007 The SharpOS Project Team (http://www.sharpos.org)
//
// Authors:
//	Mircea-Cristian Racasan <darx_kies@gmx.net>
//
// Licensed under the terms of the GNU GPL License version 2.
//

namespace SharpOS.Kernel.Tests.CS {
	public class String {
		private const string TEST_STRING = "TST\u2665";

		public static int GetLength (string value)
		{
			return value.Length;
		}

		public static int CMPGetLength () 
		{
			string value = TEST_STRING;

			if (GetLength (value) != 4)
				return 0;

			return 1;
		}

		public static int CMPGetChars ()
		{
			if (TEST_STRING [0] != 'T')
				return 0;

			if (TEST_STRING [1] != 'S')
				return 0;

			if (TEST_STRING [2] != 'T')
				return 0;

			if (TEST_STRING [3] != '\u2665')
				return 0;

			return 1;
		}

		public static int CMPGetChars2 ()
		{
			int i = 0;

			foreach (char value in TEST_STRING) {
				if (value != TEST_STRING [i++])
					return 0;
			}

			return 1;
		}
	}
}
