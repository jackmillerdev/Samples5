﻿using System;
using System.Windows.Forms;

namespace GrapeCity.ActiveReports.Sample.DigitalSignaturePro
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PDFDigitalSignature());
		}
	}
}
