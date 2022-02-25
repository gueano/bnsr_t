using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JLAutoSkill {

    public class GameBot : IDisposable {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ReleaseDC(IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPixel(IntPtr hdc, int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static GameBot _instance;
        private IntPtr hdc;
        private bool started = false;
        private List<GameSkillConfigV2> gsc = new();
        private bool lazyMode = false;
        private bool isRightDown = false;
        private DateTime nextPress = DateTime.Now;
        private BlockingCollection<GameSkillConfigV2> skillQueue = new();
        private bool isToggled = false;
        private LogitechGSDK.logiGkeyCB cb;

        public List<GameSkillConfigV2> GSC {
            get { return this.gsc; }
            set {
                this.gsc = value;
            }
        }

        public bool IsStarted {
            get { return this.started; }
        }

        public static GameBot GetInstance() {
            if (_instance == null) {
                _instance = new GameBot();
                //LogitechGSDK.LogiGkeyInitWithoutCallback();
                _instance.cb = new LogitechGSDK.logiGkeyCB(_instance.GkeySDKCallback);
                LogitechGSDK.LogiGkeyInitWithoutContext(_instance.cb);
            }

            return _instance;
        }

        private GameBot() {
            this.hdc = GetDC(IntPtr.Zero);
        }

        private void doSkillThing() {
            foreach (var skill in this.gsc) {
                var count = 0;

                foreach (var pointData in skill.points) {
                    var color = this.GetColor(pointData.point);
                    var shouldBreak = color != pointData.color;

                    if (pointData.not) shouldBreak = !shouldBreak;

                    if (shouldBreak) break;

                    count++;
                }
                if (count == skill.points.Count) {
                    this.skillQueue.Add(skill);
                }
            }

        }

        void GkeySDKCallback(LogitechGSDK.GkeyCode gKeyCode, String gKeyOrButtonString, IntPtr context) {
            if (gKeyCode.mouse != 1) return;

            if (this.lazyMode) {
                this.handleLazyMode(gKeyCode);
            } else {
                this.handleNormalMode(gKeyCode);
            }
        }

        private void handleNormalMode(LogitechGSDK.GkeyCode gKeyCode) {
            if (gKeyCode.keyDown == 0) {
                if (this.isRightDown) {
                    InputUtils.MouseRightUp();
                    this.isRightDown = false;
                }
            } else {
                if (!this.isRightDown) {
                    InputUtils.MouseRightDown();
                    this.isRightDown = true;
                }
            }
        }

        private void handleLazyMode(LogitechGSDK.GkeyCode gKeyCode) {
            if (gKeyCode.keyDown == 0) { // released
                this.isToggled = !this.isToggled;

                if (this.isToggled) {
                    if (!this.isRightDown) {
                        InputUtils.MouseRightDown();
                        this.isRightDown = true;
                    }
                } else {
                    if (this.isRightDown) {
                        InputUtils.MouseRightUp();
                        this.isRightDown = false;
                    }
                }
            }
        }

        private void start() {
            this.started = true;

            Task.Run(() => {
                while (this.started) {
                    if (this.isRightDown)
                        this.doSkillThing();
                    Thread.Sleep(10);
                }
            });

            Task.Run(() => {
                GameSkillConfigV2 skill;
                while (this.started) {
                    skill = this.skillQueue.Take();
                    var count = (skill.count <= 1 ? 1 : skill.count) * 2;
                    InputUtils.PressKey(skill.key, count, 0);
                }

            });
        }

        private void stop() {
            this.started = false;
        }

        public int GetColor(Point p) {
            return GetPixel(this.hdc, p.X, p.Y);
        }

        public void ToggleLazyMode(bool toggle) {
            this.lazyMode = toggle;
        }

        public bool Toggle(List<GameSkillConfigV2> gsc) {
            if (this.started) {
                this.stop();
            } else {
                this.start();
                this.gsc = gsc;
            }

            return this.started;
        }

        public void Dispose() {
            _instance = null;
            ReleaseDC(this.hdc);
            LogitechGSDK.LogiGkeyShutdown();
        }

        [Serializable]
        public class GameSkillConfig {
            public Point point { get; set; }
            public int color { get; set; }
            public string key { get; set; }
            public int delay { get; set; }
        }

        [Serializable]
        public class PointData {
            public Point point { get; set; }
            public int color { get; set; }
            public bool not { get; set; }

            public override string ToString() {
                var op = not ? "<>" : "=";

                return $"{point}{op}{color}";
            }
        }

        [Serializable]
        public class GameSkillConfigV2 {
            public List<PointData> points { get; set; }
            public string key { get; set; }
            public int count { get; set; }
            //public int delay { get; set; }

            public override string ToString() {
                var strPoints = String.Join("; ", points);
                return $"{strPoints},{(key == "\t" ? "!" : key)},{count}";
            }
        }
    }
}
