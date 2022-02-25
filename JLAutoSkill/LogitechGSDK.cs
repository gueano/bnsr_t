using System;
using System.Runtime.InteropServices;

namespace JLAutoSkill {
    class LogitechGSDK {
        public const int LOGITECH_MAX_MOUSE_BUTTONS = 20;
        public const int LOGITECH_MAX_GKEYS = 29;
        public const int LOGITECH_MAX_M_STATES = 3;
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct GkeyCode {
            public ushort complete;
            //	index	of	the	G	key	or	mouse	button,	for	example,	6	for	G6	or	Button	6
            public int keyIdx {
                get {
                    return complete & 255;
                }
            }
            //	key	up	or	down,	1	is	down,	0	is	up
            public int keyDown {
                get {
                    return (complete >> 8) & 1;
                }
            }
            //	mState	(1,	2	or	3	for	M1,	M2	and	M3)
            public int mState {
                get {
                    return (complete >> 9) & 3;
                }
            }
            //	indicate	if	the	Event	comes	from	a	mouse,	1	is	yes,	0	is	no.
            public int mouse {
                get {
                    return (complete >> 11) & 15;
                }
            }
            //	reserved1
            public int reserved1 {
                get {
                    return (complete >> 15) & 1;
                }
            }
            //	reserved2
            public int reserved2 {
                get {
                    return (complete >> 16) & 131071;
                }
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void logiGkeyCB(GkeyCode gkeyCode, [MarshalAs(UnmanagedType.LPWStr)] String gkeyOrButtonString, IntPtr context);   //	??

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LogiGkeyInitWithoutCallback();

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LogiGkeyInitWithoutContext(logiGkeyCB gkeyCB);

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LogiGkeyIsMouseButtonPressed(int buttonNumber);

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LogiGkeyGetMouseButtonString(int buttonNumber);

        public static String LogiGkeyGetMouseButtonStr(int buttonNumber) {
            String str =
            Marshal.PtrToStringUni(LogiGkeyGetMouseButtonString(buttonNumber));
            return str;
        }

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int LogiGkeyIsKeyboardGkeyPressed(int gkeyNumber, int modeNumber);

        [DllImport("LogitechGkeyEnginesWrapper")]
        private static extern IntPtr LogiGkeyGetKeyboardGkeyString(int gkeyNumber, int modeNumber);

        public static String LogiGkeyGetKeyboardGkeyStr(int gkeyNumber, int modeNumber) {
            String str =
            Marshal.PtrToStringUni(LogiGkeyGetKeyboardGkeyString(gkeyNumber, modeNumber));
            return str;
        }

        [DllImport("LogitechGkeyEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiGkeyShutdown();
    }
}
    
