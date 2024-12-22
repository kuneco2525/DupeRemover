using System.Text;

namespace DupeRemover;

internal class Main : Form {
	private static readonly List<Contents> FILES = [];
	private static readonly Queue<string> log = new();

	private readonly FolderBrowserDialog DialogDir = new();
	private readonly TextBox TextDir = new();
	private readonly Button ButtonDir = new(), ButtonExecute = new();
	private readonly Label LabelPath = new();

	private void InitializeComponent() {
		SuspendLayout();
		TextDir.ImeMode = ImeMode.Disable;
		TextDir.Location = new Point(12, 12);
		TextDir.Name = "TextOutput";
		TextDir.Size = new Size(200, 23);
		TextDir.TabIndex = 0;
		ButtonDir.FlatStyle = FlatStyle.Popup;
		ButtonDir.Location = new Point(218, 12);
		ButtonDir.Name = "ButtonDir";
		ButtonDir.Size = new Size(23, 23);
		ButtonDir.TabStop = false;
		ButtonDir.Text = "c";
		ButtonDir.Click += new EventHandler(ButtonDir_Click);
		ButtonExecute.FlatStyle = FlatStyle.Popup;
		ButtonExecute.Location = new Point(247, 12);
		ButtonExecute.Name = "ButtonExecute";
		ButtonExecute.Size = new Size(46, 23);
		ButtonExecute.TabIndex = 2;
		ButtonExecute.Text = "ŽÀs";
		ButtonExecute.Click += new EventHandler(ButtonExecute_Click);
		LabelPath.AutoSize = true;
		LabelPath.Location = new Point(12, 38);
		LabelPath.Name = "LabelPath";
		LabelPath.Size = new Size(0, 15);
		BackColor = Color.Honeydew;
		ClientSize = new Size(305, 62);
		Controls.Add(ButtonExecute);
		Controls.Add(LabelPath);
		Controls.Add(TextDir);
		Controls.Add(ButtonDir);
		ForeColor = Color.DarkGreen;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		Name = "Main";
		Text = "DupeRemover";
		FormClosing += new FormClosingEventHandler(Main_FormClosing);
		ResumeLayout(false);
		PerformLayout();
	}

	internal Main() => InitializeComponent();

	private void Main_FormClosing(object? sender, FormClosingEventArgs e) {
		using StreamWriter r = new(new FileStream("log.txt", FileMode.Append, FileAccess.Write), Encoding.UTF8) { AutoFlush = true, NewLine = "\n" };
		while(log.Count > 0) { r.WriteLine(log.Dequeue()); }
	}

	private static void AddFiles(string p) {
		if(File.Exists(p)) {
			FILES.Add(new(new(p)));
		} else if(Directory.Exists(p)) {
			foreach(string f in Directory.GetFiles(p)) { AddFiles(f); }
			foreach(string d in Directory.GetDirectories(p)) { AddFiles(d); }
		}
	}

	private void ButtonDir_Click(object? sender, EventArgs e) { if(DialogDir.ShowDialog(this) == DialogResult.OK) { TextDir.Text = DialogDir.SelectedPath; } }

	private void Seek() {
		AddFiles(TextDir.Text);
		SortedList<long, List<Contents>> o = [];
		foreach(Contents f in FILES) {
			if(!File.Exists(f.FullName) || f.Dupe) { continue; }
			_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
			if(o.ContainsKey(f.Size)) {
				for(int i = 0; i < o[f.Size].Count; ++i) {
					Contents s = o[f.Size][i];
					if(s.Dupe || s.FullName == f.FullName) { continue; }
					bool dupe = true;
					using(BinaryReader a = new(new FileStream(s.FullName, FileMode.Open, FileAccess.Read))) {
						using BinaryReader b = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read));
						for(long j = 0; dupe && j < a.BaseStream.Length; ++j) if(a.ReadByte() != b.ReadByte()) { dupe = false; }
					}
					if(dupe) {
						Contents ds = f.Create == s.Create ? (f.LastWrite == s.LastWrite ? (f.Name.Length < s.Name.Length ? s : f) : (f.LastWrite < s.LastWrite ? s : f)) : (f.Create < s.Create ? s : f);
						log.Enqueue($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}> {ds.FullName} << {s.FullName}, {f.FullName}");
						File.Delete(ds.FullName);
						ds.Dupe = true;
					} else { o[f.Size].Add(f); }
				}
			} else { o.Add(f.Size, [f]); }
			GC.Collect();
		}
	}
	private async void ButtonExecute_Click(object? sender, EventArgs e) {
		ButtonDir.Enabled = false;
		ButtonExecute.Enabled = false;
		TextDir.Enabled = false;
		await Task.Run(Seek);
		Close();
	}
}