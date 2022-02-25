using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JLAutoSkill {
    class InputUtils {
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        [Flags]
        public enum InputType {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEvent {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        [Flags]
        public enum MouseEvent {
            Absolute = 0x8000,
            HWheel = 0x01000,
            Move = 0x0001,
            MoveNoCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            VirtualDesk = 0x4000,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }

        public struct Input {
            public int type;
            public InputUnion u;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint VkKeyScanEx(char ch, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetKeyboardLayout(int idThread);

        private static IntPtr hkl = GetKeyboardLayout(0);

        private static uint getScanCode(char ch) {
            return MapVirtualKey(VkKeyScanEx(ch, hkl), 0);
        }

        public static void SendKeyboardInput(char ch, KeyEvent flags) {
            var scanCode = getScanCode(ch);
            var inputs = new Input[] {
                new Input {
                    type = (int)InputType.Keyboard,
                    u = new InputUnion {
                        ki = new KeyboardInput {
                            wVk = 0,
                            wScan = (ushort)(scanCode & 0xff),
                            dwFlags = (uint)(flags | KeyEvent.Scancode),
                            dwExtraInfo = IntPtr.Zero,
                            time = 0
                        }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        public static void SendMouseInput(MouseEvent flags) {
            var inputs = new Input[] {
                new Input {
                    type = (int)InputType.Mouse,
                    u = new InputUnion {
                        mi = new MouseInput {
                            dx = 0,
                            dy = 0,
                            mouseData = 0,
                            dwFlags = (uint)flags,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        public static void PressKey(string keys, int count=1, int delay=0) {
            Trace.WriteLine(keys);
            foreach (var ch in keys.ToLower().ToCharArray()) {
                for (var i = 0; i < count; i++) {
                    SendKeyboardInput(ch, KeyEvent.KeyDown);
                    if (delay > 0) Thread.Sleep(delay);
                    SendKeyboardInput(ch, KeyEvent.KeyUp);
                }
            }
        }

        public static void MouseRightDown() {
            SendMouseInput(MouseEvent.RightDown);
        }

        public static void MouseRightUp() {
            SendMouseInput(MouseEvent.RightUp);
        }
    }
}
