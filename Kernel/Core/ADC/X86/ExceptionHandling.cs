//
// (C) 2006-2007 The SharpOS Project Team (http://www.sharpos.org)
//
// Authors:
//	Mircea-Cristian Racasan <darx_kies@gmx.net>
//
// Licensed under the terms of the GNU GPL v3,
//  with Classpath Linking Exception for Libraries
//

using System;
using SharpOS.AOT.X86;
using SharpOS.AOT.IR;
using SharpOS.Korlib.Runtime;

namespace SharpOS.Kernel.ADC.X86 {
	public static class ExceptionHandling	{
		private const string GET_IP = "GET_IP";

		internal unsafe static StackFrame [] GetCallingStack ()
		{
			uint bp, ip, count = 0;
			StackFrame [] stackFrame = null;

			// 1st step: count the stack frames
			// 2nd step: create the array and find the method boundary for every stack frame
			for (int step = 0; step < 2; step++) {
				// Get the current IP
				Asm.CALL (GET_IP);
				Asm.LABEL (GET_IP);
				Asm.POP (R32.EAX);
				Asm.MOV (&ip, R32.EAX);

				// Get the current BP
				Asm.MOV (&bp, R32.EBP);

				if (step == 1)
					stackFrame = new StackFrame [count];

				count = 0;

				do {
					if (step == 1) {
						MethodBoundary entry = null;

						for (int i = 0; i < Runtime.MethodBoundaries.Length; i++) {
							if (ip >= (uint) Runtime.MethodBoundaries [i].Begin
									&& ip < (uint) Runtime.MethodBoundaries [i].End) {
								entry = Runtime.MethodBoundaries [i];
								break;
							}
						}

						stackFrame [count] = new StackFrame ();
						stackFrame [count].IP = (void*) ip;
						stackFrame [count].BP = (void*) bp;
						stackFrame [count].MethodBoundary = entry;
					}

					count++;

					// Get the next IP
					Asm.MOV (R32.EBX, &bp);
					Asm.MOV (R32.EAX, new DWordMemory (null, R32.EBX, null, 0, 4));
					Asm.MOV (&ip, R32.EAX);

					// Get the next BP
					Asm.MOV (R32.EAX, new DWordMemory (null, R32.EBX, null, 0));
					Asm.MOV (&bp, R32.EAX);

				} while (bp != 0);
			}

			return stackFrame;
		}
	}
}