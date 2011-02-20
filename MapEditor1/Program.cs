using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace MapEditor1 {
	[Serializable] public class State {
		public string LastUsedDirectory;
	}

	static class Program {
		static readonly string StatePath = Path.Combine( Application.LocalUserAppDataPath, "state.xml" );
		public static State State = new State();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			if ( File.Exists(StatePath) ) try {
				var xs = new XmlSerializer(typeof(State));
				var ms = new MemoryStream(File.ReadAllBytes(StatePath));
				State = (State)xs.Deserialize(ms);
			} catch ( Exception e ) {
			}
			//Application.LocalUserAppDataPath

			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			Application.Run( new MapEditorForm() );

			try {
				var xs = new XmlSerializer(typeof(State));
				var ms = new MemoryStream();
				xs.Serialize(ms,State);
				
				ms.Position = 0;
				var test = (State)xs.Deserialize(ms);

				File.WriteAllBytes(StatePath,ms.ToArray());
			} catch ( Exception ) {
			}
		}
	}
}
