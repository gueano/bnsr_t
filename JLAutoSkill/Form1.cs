using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;

namespace JLAutoSkill {

    public partial class Form1 : Form {

        private Point currentMousePosition;
        private int currentColor;
        private GameBot bot;
        private List<GameBot.GameSkillConfigV2> gsc = new();

        public Form1() {
            InitializeComponent();
            HotKey.RegisterHotKey(this.Handle, 100, HotKey.KeyModifiers.Alt, Keys.D1);
            HotKey.RegisterHotKey(this.Handle, 200, HotKey.KeyModifiers.Alt, Keys.D2);
            HotKey.RegisterHotKey(this.Handle, 300, HotKey.KeyModifiers.Alt, Keys.D3);
            HotKey.RegisterHotKey(this.Handle, 400, HotKey.KeyModifiers.Ctrl, Keys.F1);
            this.bot = GameBot.GetInstance();
        }

        public void WriteCfg(string filePath, List<GameBot.GameSkillConfigV2> objectToWrite, bool append = false) {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create)) {
                stream.Write(JsonSerializer.SerializeToUtf8Bytes(objectToWrite));
            }
        }

        public void ReadCfg(string filePath) {
            if (!File.Exists(filePath)) return;

            string fileContent;

            using (var stream = File.OpenText(filePath)) {
                fileContent = stream.ReadToEnd();

                this.gsc = JsonSerializer.Deserialize<List<GameBot.GameSkillConfigV2>>(fileContent);

                if (this.gsc.Count > 0 && this.gsc[0].points == null) {
                    var oldGsc = JsonSerializer.Deserialize<List<GameBot.GameSkillConfig>>(fileContent);

                    this.gsc.Clear();
                    this.fromOldGscToV2(oldGsc, ref this.gsc);
                }

            };

        }

        private void fromOldGscToV2(List<GameBot.GameSkillConfig> oldGsc, ref List<GameBot.GameSkillConfigV2> gsc) {
            GameBot.GameSkillConfigV2 skillV2;

            foreach(var skill in oldGsc) {
                skillV2 = new GameBot.GameSkillConfigV2() {
                    points = new List<GameBot.PointData> {
                        new GameBot.PointData {
                            point = skill.point,
                            color = skill.color
                        }
                    },
                    key = skill.key,
                    count = skill.delay
                };

                gsc.Add(skillV2);
            };
        }

        private void timer1_Tick(object sender, EventArgs e) {
            this.currentMousePosition = MousePosition;
            this.currentColor = this.bot.GetColor(this.currentMousePosition);
        }

        private void button1_Click(object sender, EventArgs e) {
            this.timer1.Enabled = !this.timer1.Enabled;
            if (this.timer1.Enabled) {
                this.button1.Text = "停止取色";
            } else {
                this.button1.Text = "开始取色";
            }
        }

        private void toggleBot() {
            if (this.bot.Toggle(this.gsc)) {
                this.button2.Text = "停止";
            } else {
                this.button2.Text = "开始";
            }
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0312) {
                var id = m.WParam.ToInt32();
                if (id == 100) {
                    if (this.timer1.Enabled) {
                        var skill = new GameBot.GameSkillConfigV2 {
                            points = new List<GameBot.PointData> {
                                new GameBot.PointData {
                                    point = this.currentMousePosition,
                                    color = this.currentColor
                                }
                            },
                            count = 1,
                            key = "v"
                        };

                        this.listBox1.Items.Add(skill);
                        this.gsc.Add(skill);
                    }
                } else if (id == 200 || id == 300) {
                    if (this.timer1.Enabled) {
                        var lastSkillIdx = this.gsc.Count - 1;

                        if (lastSkillIdx >= 0) {
                            var skill = this.gsc[lastSkillIdx];
                            var point = new GameBot.PointData {
                                point = this.currentMousePosition,
                                color = this.currentColor,
                                not = false
                            };

                            if (id == 300) {
                                point.not = true;
                            }
                            skill.points.Add(point);

                            this.listBox1.Items.RemoveAt(lastSkillIdx);
                            this.listBox1.Items.Add(skill);
                        }
                    }
                } else if (id == 400) {
                    this.toggleBot();
                }
            }

            base.WndProc(ref m);
        }

        private void button2_Click(object sender, EventArgs e) {
            this.toggleBot();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) {
            var idx = this.listBox1.SelectedIndex;
            if (idx == -1) return;

            this.panel3.Visible = true;

            var skill = this.gsc[idx];
            this.textBox1.Text = skill.key == "\t" ? "!" : skill.key;
            this.textBox2.Text = skill.count.ToString();
        }

        private void button3_Click(object sender, EventArgs e) {
            this.panel3.Visible = false;

            var idx = this.listBox1.SelectedIndex;

            if (idx == -1) return;

            var skill = this.gsc[idx];
            skill.key = this.textBox1.Text == "!" ? "\t" : this.textBox1.Text;
            skill.count = Int32.Parse(this.textBox2.Text);

            this.listBox1.Items[idx] = skill.ToString();
            this.gsc[idx] = skill;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e) {
            if (this.listBox1.SelectedIndex == -1 || e.KeyCode != Keys.Delete) return;

            var idx = this.listBox1.SelectedIndex;

            this.gsc.RemoveAt(idx);
            this.listBox1.Items.RemoveAt(idx);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            this.bot.Dispose();
        }

        private void checkBox1_Click(object sender, EventArgs e) {
            this.bot.ToggleLazyMode(this.checkBox1.Checked);
        }

        private void button4_Click(object sender, EventArgs e) {
            var cfgFileName = $"{Application.StartupPath}/{this.textBox3.Text}";
            if (!cfgFileName.EndsWith(".json")) cfgFileName += ".json";

            if (!File.Exists(cfgFileName)) {
                MessageBox.Show("配置不存在");
                return;
            }

            this.ReadCfg(cfgFileName);
            this.listBox1.Items.Clear();

            foreach (var s in this.gsc) {
                this.listBox1.Items.Add(s);
            }

            if (this.bot.IsStarted)
                this.bot.GSC = this.gsc;
        }

        private void button5_Click(object sender, EventArgs e) {
            var cfgFileName = $"{Application.StartupPath}/{this.textBox3.Text}";
            if (!cfgFileName.EndsWith(".json")) cfgFileName += ".json";

            if (File.Exists(cfgFileName)) {
                if (MessageBox.Show("配置已存在，是否覆盖？", "提示", MessageBoxButtons.YesNo) == DialogResult.No) return;
            }

            this.WriteCfg(cfgFileName, this.gsc);
        }
    }
}
